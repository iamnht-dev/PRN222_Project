(function () {
    "use strict";

    const form = document.getElementById("signalRCoachForm");
    if (!form) return;

    const modalElement = document.getElementById("signalRModal");
    const status = document.getElementById("hubStatus");
    const error = document.getElementById("signalRFormError");
    const saveButton = document.getElementById("btnSignalRSave");
    const createButton = document.getElementById("btnCoachCreate");
    const normalButton = document.getElementById("btnNormalSave");
    const fieldset = document.getElementById("coachFields");
    const deleteConfirmation = document.getElementById("deleteConfirmation");
    const specialization = document.getElementById("srSpecialization");
    const fields = ["CoachThaiNhid", "FullName", "Email", "Phone", "DateOfBirth", "Gender", "Address",
        "ImageUrl", "Experience", "Certification", "Salary", "HireDate", "Status", "PublishDate", "IsActive"];
    const connection = new signalR.HubConnectionBuilder().withUrl("/coachHub").withAutomaticReconnect().build();
    let currentId = 0;
    let currentAction = "Create";
    const coaches = new Map(JSON.parse(document.getElementById("initialCoaches").textContent).map(item => [item.coachThaiNhid, item]));
    const tbody = document.querySelector("#myTable tbody");
    let syncing = true;
    let pendingChanges = [];
    let ready = false;
    const filters = ["fullName", "email", "phone"].map(name =>
        (document.querySelector('input[name="' + name + '"]').value || "").trim().toLocaleLowerCase());
    const defaultAvatar = "data:image/svg+xml," + encodeURIComponent('<svg xmlns="http://www.w3.org/2000/svg" width="50" height="50"><rect width="50" height="50" fill="#e9ecef"/><circle cx="25" cy="18" r="9" fill="#6c757d"/><path d="M8 48a17 17 0 0 1 34 0" fill="#6c757d"/></svg>');

    function setReady(value) {
        ready = value;
        saveButton.disabled = !ready;
    }

    function matchesFilters(coach) {
        return [coach.fullName, coach.email, coach.phone].every((value, index) =>
            !filters[index] || String(value || "").toLocaleLowerCase().includes(filters[index]));
    }

    function cell(row, value, className) {
        const td = document.createElement("td");
        td.textContent = value == null ? "" : String(value);
        if (className) td.className = className;
        row.append(td);
        return td;
    }

    function renderRows() {
        const rows = [...coaches.values()].sort((a, b) => b.coachThaiNhid - a.coachThaiNhid).map(coach => {
            const signature = JSON.stringify(coach);
            const existing = tbody.querySelector('tr[data-coach-id="' + coach.coachThaiNhid + '"]');
            if (existing && existing.dataset.signature === signature) return existing;
            const row = document.createElement("tr");
            row.dataset.coachId = coach.coachThaiNhid;
            row.dataset.signature = signature;
            cell(row, coach.coachThaiNhid);
            const image = document.createElement("img");
            image.alt = "Avatar";
            image.className = "rounded-circle object-fit-cover shadow-sm";
            image.style.width = image.style.height = "50px";
            image.src = defaultAvatar;
            if (coach.imageUrl) {
                try {
                    const url = new URL(coach.imageUrl, window.location.origin);
                    if (["http:", "https:"].includes(url.protocol)) image.src = url.href;
                } catch (_) { /* Use the inline placeholder for invalid URLs. */ }
            }
            cell(row, "").append(image);
            cell(row, coach.fullName, "fw-bold");
            const badge = document.createElement("span");
            badge.className = "badge bg-primary";
            badge.textContent = coach.specializationName || "General";
            cell(row, "").append(badge);
            cell(row, coach.phone);
            cell(row, coach.email);
            const actions = cell(row, "");
            for (const [label, style] of [["Edit", "primary"], ["Details", "info"], ["Delete", "danger"]]) {
                const control = document.createElement(label === "Details" ? "a" : "button");
                control.className = "btn btn-sm btn-outline-" + style;
                if (label === "Details") control.href = "/Coaches/Details/" + coach.coachThaiNhid;
                else {
                    control.type = "button";
                    control.classList.add("btn-coach-" + label.toLowerCase());
                    control.dataset.id = coach.coachThaiNhid;
                }
                control.textContent = label;
                actions.append(control, " ");
            }
            return row;
        });
        if (!rows.length) {
            const row = document.createElement("tr");
            row.className = "js-empty-row";
            cell(row, "Không có dữ liệu phù hợp.", "text-center text-muted").colSpan = 7;
            rows.push(row);
        }
        tbody.replaceChildren(...rows);
        window.tablePagination.init({ table: "#myTable", pager: "#pager", info: "#pageInfo", pageSizeSelector: "#pageSize", pageSize: 5 });
    }

    function applyChange(payload) {
        const id = payload.coachThaiNhid;
        if (payload.action === "deleted" || !payload.coach || !matchesFilters(payload.coach)) coaches.delete(id);
        else coaches.set(id, payload.coach);
        renderRows();
    }

    async function syncRoster() {
        syncing = true;
        const items = await connection.invoke("GetAll", ...filters.map(value => value || null));
        coaches.clear();
        items.forEach(item => coaches.set(item.coachThaiNhid, item));
        pendingChanges.forEach(applyChange);
        pendingChanges = [];
        syncing = false;
        renderRows();
    }

    async function loadSpecializations() {
        const items = await connection.invoke("GetSpecializations");
        specialization.replaceChildren(new Option("-- Select --", ""));
        items.forEach(item => specialization.add(new Option(item.specializationName, String(item.specializationThaiNhid))));
    }

    function fillForm(item, action = "Create") {
        currentAction = action;
        currentId = item ? item.coachThaiNhid : 0;
        form.reset();
        error.textContent = "";
        fields.forEach(name => {
            const input = document.getElementById("sr" + name);
            const key = name[0].toLowerCase() + name.slice(1);
            if (input.type === "checkbox") input.checked = item ? !!item[key] : true;
            else input.value = item && item[key] != null ? item[key] : "";
        });
        document.getElementById("srCoachThaiNhid").value = currentId;
        specialization.value = item && item.specializationThaiNhid != null ? item.specializationThaiNhid : "";
        const deleting = action === "Delete";
        fieldset.disabled = deleting;
        fieldset.hidden = deleting;
        deleteConfirmation.hidden = !deleting;
        deleteConfirmation.textContent = deleting ? "Xác nhận xoá Coach #" + currentId + ": " + item.fullName + "?" : "";
        form.action = "/Coaches/" + action + (currentId ? "/" + currentId : "");
        document.getElementById("signalRModalLabel").textContent = action + " Coach" + (currentId ? " #" + currentId : "");
        normalButton.textContent = action;
        saveButton.textContent = action + " by SignalR";
        normalButton.className = "btn btn-" + (deleting ? "danger" : "primary");
        saveButton.className = "btn btn-outline-" + (deleting ? "danger" : "success");
        setReady(ready);
        bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }

    function readForm() {
        const dto = {};
        fields.forEach(name => {
            const input = document.getElementById("sr" + name);
            const key = name[0].toLowerCase() + name.slice(1);
            dto[key] = input.type === "checkbox" ? input.checked : input.value.trim() || null;
        });
        dto.coachThaiNhid = currentId;
        dto.salary = dto.salary == null ? null : Number(dto.salary);
        dto.status = dto.status == null ? null : Number(dto.status);
        dto.specializationThaiNhid = specialization.value === "" ? null : Number(specialization.value);
        return dto;
    }

    createButton.addEventListener("click", () => fillForm(null));
    tbody.addEventListener("click", async event => {
        const edit = event.target.closest(".btn-coach-edit");
        const remove = event.target.closest(".btn-coach-delete");
        if (!edit && !remove) return;
        try {
            const id = Number((edit || remove).dataset.id);
            const item = coaches.get(id) || (ready ? await connection.invoke("GetById", id) : null);
            if (!item) throw new Error("Coach no longer exists. Reload the page to refresh.");
            fillForm(item, edit ? "Edit" : "Delete");
        } catch (err) { status.textContent = err.message; }
    });

    form.addEventListener("submit", async event => {
        // A normal submit goes directly to the existing MVC action with antiforgery protection.
        if (event.submitter !== saveButton) return;
        event.preventDefault();
        if (!form.reportValidity()) return;
        error.textContent = "";
        if (!ready) { error.textContent = "SignalR chưa kết nối. Bạn có thể lưu bình thường hoặc chờ kết nối lại."; return; }
        saveButton.disabled = true;
        normalButton.disabled = true;
        try {
            if (currentAction === "Delete") await connection.invoke("Delete", currentId);
            else await connection.invoke(currentAction === "Edit" ? "Update" : "Create", readForm());
            bootstrap.Modal.getInstance(modalElement).hide();
        } catch (err) { error.textContent = err.message; }
        finally { saveButton.disabled = !ready; normalButton.disabled = false; }
    });

    connection.on("CoachChanged", payload => {
        if (syncing) pendingChanges.push(payload);
        else applyChange(payload);
        status.textContent = "Coach " + payload.fullName + " " + payload.action + ". Updated via SignalR (no reload).";
    });
    connection.onreconnecting(() => { syncing = true; pendingChanges = []; setReady(false); status.textContent = "Reconnecting..."; });
    connection.onreconnected(async () => {
        try { await loadSpecializations(); await syncRoster(); setReady(true); status.textContent = "Live updates connected."; }
        catch (err) { status.textContent = err.message; }
    });
    connection.onclose(() => { setReady(false); status.textContent = "Disconnected. Reload to retry."; });
    renderRows();
    connection.start().then(async () => {
        await loadSpecializations();
        await syncRoster();
        setReady(true);
        status.textContent = "Live updates connected.";
    }).catch(err => { setReady(false); status.textContent = "Cannot connect: " + err.message; });
})();
