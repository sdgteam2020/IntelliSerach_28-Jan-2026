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



    // Clear error when user selects another file
    // $("#fileInput").on("change", function () {
        
    //     $("#uploadMessage").empty();
    // });
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
        const csrfToken = token();

        if (!csrfToken) {
            throw new Error("Anti-forgery token is missing.");
        }
        const response = await fetch('/IntelliSearch/Search/DeleteFiles', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                "RequestVerificationToken": csrfToken   // Pass the CSRF token in the header
            },
            body: JSON.stringify({ Id: id })
        });

        if (!response.ok) {
            
            toastr.error('Failed to delete record.');
        }

        const data = await response.json();
        if (data.Code == 200) {
           
            await Swal.fire({
                icon: 'success',
                title: 'Deleted!',
                text: data.Message
            });
            // Reload page or refresh table
            // Reload page or refresh table
            location.reload();
        } else {
            toastr.error(data.Message);
            
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