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

    BindData();
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
        const token = $('input[name="__RequestVerificationToken"]').val();
        const response = await fetch('/IntelliSearch/Master/DeleteWebServer', {
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
        if (data == true) {
            await Swal.fire({
                icon: 'success',
                title: 'Deleted!',
                text: 'Record deleted successfully.'
            });
            // Reload page or refresh table
            BindData();
            reset();
        } else {

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Record Not deleted'
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

async function BindData() {

    const token = $('input[name="__RequestVerificationToken"]').val();

    try {

        const response = await fetch('/IntelliSearch/Master/GetAllWebServer', {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token
            }
        });

        const data = await response.json();

        // Destroy existing DataTable
        if ($.fn.DataTable.isDataTable('#tblData')) {
            $('#tblData').DataTable().destroy();
        }

        let html = '';

        data.forEach((item, index) => {

            html += `
                <tr>
                    <td>${index + 1}</td>
                    <td>${item.Url ?? ''}</td>
                    <td>${item.Includes ?? ''}</td>
                    <td>${item.Index_Name ?? ''}</td>
                    <td>${formatDate(item.CreatedOn)}</td>
                    <td class="text-center">
                        <button
                            type="button"
                            class="btn btn-primary btnEdit"
                            data-id="${item.Id}"
                            data-url="${item.Url}"
                            data-index_name="${item.Index_Name}"
                            data-includes="${item.Includes}">
                            Edit
                        </button>

                        <button
                            type="button"
                            class="btn btn-danger btnDel"
                            data-id="${item.Id}">
                            <i class="fa-solid fa-trash"></i> Delete
                        </button>
                    </td>
                </tr>`;
        });

        $('#tblData tbody').html(html);

        // Reinitialize DataTable
        $('#tblData').DataTable({
            pageLength: 10,
            responsive: false,
            ordering: true,
            searching: true,
            lengthMenu: [10, 25, 50, 100]
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
    }
    catch (error) {
        console.error(error);
        Swal.fire('Error', 'Failed to load data.', 'error');
    }
}

function formatDate(dateString) {

    if (!dateString) return '';

    const date = new Date(dateString);

    return date.toLocaleString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}
function reset() {
    $("input[name='Id']").val(0);
    $("input[name='Url']").val("");
    $("input[name='Includes']").val("");
    $("input[name='Index_Name']").val("");
    $(".alert").html("");
}