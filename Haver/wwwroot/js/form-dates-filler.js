// Helper function to get the current date in the format YYYY-MM-DD
function getCurrentDate() {
    const now = new Date();
    const year = now.getFullYear();
    const month = (now.getMonth() + 1).toString().padStart(2, '0');
    const day = now.getDate().toString().padStart(2, '0');
    const hours = now.getHours().toString().padStart(2, '0');
    const minutes = now.getMinutes().toString().padStart(2, '0');

    return `${year}-${month}-${day}`;
}

$(document).ready(function () {
    $("#qualityRepDate").val(getCurrentDate());
    $("#engineeringDate").val(getCurrentDate());
    $("#operationsDate").val(getCurrentDate());
    $("#procurementDate").val(getCurrentDate());
    $("#reinspectionDate").val(getCurrentDate());
});