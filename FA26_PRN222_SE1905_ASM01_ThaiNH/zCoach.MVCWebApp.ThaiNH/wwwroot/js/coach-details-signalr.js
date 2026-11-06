(function () {
    "use strict";
    const root = document.getElementById("coachDetails");
    if (!root) return;
    const id = Number(root.dataset.coachId);
    const status = document.getElementById("detailsLiveStatus");
    const deleted = document.getElementById("detailsDeleted");
    const edit = document.getElementById("detailsEditLink");
    const connection = new signalR.HubConnectionBuilder().withUrl("/coachHub").withAutomaticReconnect().build();

    function format(field, value) {
        if (value == null || value === "") return field === "specializationName" ? "General" : "-";
        if (field === "isActive") return value ? "Yes" : "No";
        if (["dateOfBirth", "hireDate", "publishDate"].includes(field)) {
            const parts = String(value).slice(0, 10).split("-");
            return parts.length === 3 ? parts.reverse().join("/") : String(value);
        }
        if (field === "salary") return Number(value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        if (field === "updateAt") return new Date(value).toLocaleString();
        return String(value);
    }

    function render(coach) {
        deleted.hidden = !!coach;
        root.querySelector("dl").hidden = !coach;
        edit.hidden = !coach;
        if (!coach) return;
        root.querySelectorAll("[data-coach-field]").forEach(cell => {
            const field = cell.dataset.coachField;
            cell.textContent = format(field, coach[field]);
        });
    }

    connection.on("CoachChanged", payload => {
        if (payload.coachThaiNhid !== id) return;
        render(payload.action === "deleted" ? null : payload.coach);
        status.textContent = "Details updated via SignalR (no reload).";
    });
    connection.onreconnecting(() => { status.textContent = "Reconnecting..."; });
    async function sync() {
        render(await connection.invoke("GetById", id));
        status.textContent = "Live updates connected.";
    }
    connection.onreconnected(() => sync().catch(() => { status.textContent = "Cannot refresh Details. Reload to retry."; }));
    connection.onclose(() => { status.textContent = "Disconnected. Reload to retry."; });
    connection.start().then(sync).catch(() => { status.textContent = "Cannot connect to live updates. Reload to retry."; });
})();
