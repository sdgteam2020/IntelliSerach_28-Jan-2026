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


    const maxSize = 5 * 1024 * 1024; // 5 MB
    const fileInput = document.getElementById("FileName");
    const errorElement = document.getElementById("fileError");

    if (!fileInput) return;

    fileInput.addEventListener("change", function () {
        errorElement.textContent = "";

        const file = fileInput.files[0];

        if (!file) return;

        const isPdf =
            file.type === "application/pdf" ||
            file.name.toLowerCase().endsWith(".pdf");

        if (!isPdf) {
            errorElement.textContent = "Please select a PDF file only.";
            fileInput.value = "";
            return;
        }

        if (file.size > maxSize) {
            errorElement.textContent =
                "File size must not exceed 5 MB.";

            fileInput.value = "";
        }
    });

    $(fileInput.form).on("submit", function (event) {
        errorElement.textContent = "";

        const file = fileInput.files[0];

        if (!file) {
            errorElement.textContent = "Please select a PDF file.";
            event.preventDefault();
            return;
        }

        if (file.size > maxSize) {
            errorElement.textContent =
                "File size must not exceed 5 MB.";

            fileInput.value = "";
            event.preventDefault();
        }
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