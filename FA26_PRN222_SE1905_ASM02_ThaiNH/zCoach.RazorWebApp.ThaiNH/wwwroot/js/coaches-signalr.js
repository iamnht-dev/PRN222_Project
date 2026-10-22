(function () {
    "use strict";

    const form = document.getElementById("signalRCoachForm");
    if (!form) return;

    const modalElement = document.getElementById("signalRModal");
    const status = document.getElementById("hubStatus");
    const error = document.getElementById("signalRFormError");
    const saveButton = document.getElementById("btnSignalRSave");
    const createButton = document.getElementById("btnSignalRCreate");
    const specialization = document.getElementById("srSpecialization");
    const fields = ["CoachThaiNhid", "FullName", "Email", "Phone", "DateOfBirth", "Gender", "Address",
        "ImageUrl", "Experience", "Certification", "Salary", "HireDate", "Status", "PublishDate", "IsActive"];
    const connection = new signalR.HubConnectionBuilder().withUrl("/coachHub").withAutomaticReconnect().build();
    let currentId = 0;
    let reloadScheduled = false;

    function setReady(ready) {
        document.querySelectorAll("#btnSignalRCreate,.btn-sr-edit,.btn-sr-delete").forEach(button => button.disabled = !ready);
    }

    // Like Quang's new ASM02: keep GET filters and refresh every connected tab after a mutation.
    function scheduleReload() {
        if (reloadScheduled) return;
        reloadScheduled = true;
        window.setTimeout(() => window.location.reload(), 700);
    }

    async function loadSpecializations() {
        const items = await connection.invoke("GetSpecializations");
        specialization.replaceChildren(new Option("-- Select --", ""));
        items.forEach(item => specialization.add(new Option(item.specializationName, String(item.specializationThaiNhid))));
    }

    function fillForm(item) {
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
        document.getElementById("signalRModalLabel").textContent = currentId ? "SignalR Edit #" + currentId : "SignalR Create";
        saveButton.textContent = currentId ? "Update via SignalR" : "Create via SignalR";
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
    document.querySelector("#coachTable tbody").addEventListener("click", async event => {
        const edit = event.target.closest(".btn-sr-edit");
        const remove = event.target.closest(".btn-sr-delete");
        if (!edit && !remove) return;
        try {
            const id = Number((edit || remove).dataset.id);
            if (edit) {
                const item = await connection.invoke("GetById", id);
                if (!item) throw new Error("Coach no longer exists.");
                fillForm(item);
            } else if (window.confirm("Delete Coach #" + id + " via SignalR?")) {
                await connection.invoke("Delete", id);
                scheduleReload();
            }
        } catch (err) { status.textContent = err.message; }
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (!form.reportValidity()) return;
        error.textContent = "";
        saveButton.disabled = true;
        try {
            await connection.invoke(currentId ? "Update" : "Create", readForm());
            bootstrap.Modal.getInstance(modalElement).hide();
            scheduleReload();
        } catch (err) { error.textContent = err.message; }
        finally { saveButton.disabled = false; }
    });

    connection.on("CoachChanged", payload => {
        status.textContent = "Coach " + payload.fullName + " " + payload.action + ". Refreshing...";
        scheduleReload();
    });
    connection.onreconnecting(() => { setReady(false); status.textContent = "Reconnecting..."; });
    connection.onreconnected(async () => {
        try { await loadSpecializations(); setReady(true); status.textContent = "Live updates connected."; scheduleReload(); }
        catch (err) { status.textContent = err.message; }
    });
    connection.onclose(() => { setReady(false); status.textContent = "Disconnected. Reload to retry."; });
    connection.start().then(async () => {
        await loadSpecializations();
        setReady(true);
        status.textContent = "Live updates connected.";
    }).catch(err => { setReady(false); status.textContent = "Cannot connect: " + err.message; });
})();
