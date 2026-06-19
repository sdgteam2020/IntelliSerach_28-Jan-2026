$(document).ready(function () {
    $('#tblData').DataTable({
        "order": [],
        "paging": true,
        "searching": true,
        "info": true,
        "autoWidth": false,
        "responsive": true,
        "select": true
    });
    $("#SaveForm").on("submit", async function (e) {
        e.preventDefault();

        const isValid = false; await CheckValidation();
        if (isValid) {
            this.submit();
        }
    });

    $(".btnEdit").click(function () {

        let id = $(this).data("id");
        let url = $(this).data("url");
        let includes = $(this).data("includes");
        let Index_Name = $(this).data("index_name");
       
        // Set values in form
        $("input[name='Id']").val(id);
        $("input[name='Url']").val(url);
        $("input[name='Includes']").val(includes);
        $("input[name='Index_Name']").val(Index_Name);

        // Optional: Scroll to form
        $('html, body').animate({
            scrollTop: $("form").offset().top
        }, 500);
    });
    $(".btnDel").click(function () {

        let id = $(this).data("id");
        
        DeleteWebServer(id);
    });

});
async function DeleteWebServer(id) {

    const result = await Swal.fire({
        title: 'Are you sure?',
        text: 'You want to delete this record?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, Delete',
        cancelButtonText: 'Cancel',
        confirmButtonColor: '#d33'
    });

    if (!result.isConfirmed)
        return;

    try {

        const response = await fetch('/Master/DeleteWebServer', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Id: id })
        });

        if (!response.ok) {
            throw new Error('Failed to delete record.');
        }

        const data = await response.json();

        await Swal.fire({
            icon: 'success',
            title: 'Deleted!',
            text: 'Record deleted successfully.'
        });

        // Reload page or refresh table
        location.reload();

    }
    catch (error) {

        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message
        });

        console.error(error);
    }
}
async function CheckValidation() {
    const form = $("#SaveForm");
    if (!form.valid()) {
        return false;
    }

    let formData = {};

    form.serializeArray().forEach(function (item) {
        formData[item.name] = item.value;
    });

    let jsonData = JSON.stringify(formData);

    let encrypted = encryptPayloadData(jsonData);
    $("#EncryptedData").val(encrypted);

    $("#SaveForm")[0].submit(); // native submit
    return true;
}