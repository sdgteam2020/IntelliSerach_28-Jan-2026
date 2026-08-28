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
    $(".btnDel").click(function () {

        let id = $(this).data("id");
        
        DeleteUpload(id);
    });
});
async function DeleteUpload(id) {

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
        const token = $('input[name="__RequestVerificationToken"]').val();
        const response = await fetch('/IntelliSearch/Search/DeleteFiles', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                "RequestVerificationToken": token   // Pass the CSRF token in the header
            },
            body: JSON.stringify({ Id: id })
        });

        if (!response.ok) {
            throw new Error('Failed to delete record.');
        }

        const data = await response.json();
        if (data.Code == 200) {
            await Swal.fire({
                icon: 'success',
                title: 'Deleted!',
                text: data.Message
            });
            // Reload page or refresh table
            location.reload();
        } else {

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: data.Message
            });
        }



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