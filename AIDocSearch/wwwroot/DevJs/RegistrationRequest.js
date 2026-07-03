let table; // Declare table variable outside the function to preserve the instance
$(document).ready(function () {
    setTimeout(BindData, 100);
    $('#tbldata').DataTable({
        "order": [],
        "paging": true,
        "searching": true,
        "info": true,
        "autoWidth": false,
        "responsive": true,
        "buttons": [
            'copy', 'csv', 'excel', 'pdf', 'print'
        ],
        "select": true
    });
});
function BindData() {
    if ($.fn.DataTable.isDataTable("#tbldata")) {
        $("#tbldata").DataTable().destroy();
    }
    table = $("#tbldata").DataTable({
        processing: true,
        serverSide: true,
        filter: true,
        stateSave: false,
        "lengthMenu": [[5, 10, 20, 50, 100], [5, 10, 20, 50, 100]],
        order: [[1, 'desc']],
        ajax: async function (data, callback, settings) {
            let requestData = {
                draw: data.draw,
                start: data.start,
                length: data.length,
                searchValue: data.search.value,
                sortColumn: data.order.length > 0 ? data.columns[data.order[0].column].data : '',
                sortDirection: data.order.length > 0 ? data.order[0].dir : '',
            };
            try {
                let response = await fetch("/IntelliSearch/Account/GetAllUser", {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: new URLSearchParams(requestData).toString()
                });

                if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
                let result = await response.json();
                callback(result);
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        },
        columns: [
            {
                data: null,
                name: "Slno.",
                orderable: false,
                render: function (data, type, row, meta) {
                    return meta.row + (meta.settings?._iDisplayStart || 0) + 1;
                }
            },
            { data: "DomainId", name: "DomainId" },
            { data: "RoleNames", name: "RoleNames" },
            { data: "Name", name: "Name" },
            {
                data: "Id",
                name: "Id",
                orderable: false,
                render: function (data, type, row) {
                    if (!data) return "NA";

                    let isActive = row.Active === true;

                    return `
            <button type="button"
                    class="btn btn-sm ${isActive ? 'btn-success' : 'btn-danger'} updateuser"
                    data-id="${data}"
                    data-active="${isActive}">
                ${isActive ? 'Active' : 'Inactive'}
            </button>
        `;
                }
            }

        ],
        language: {
            search: "",
            searchPlaceholder: "Search Domain ID"
        },
        dom: 'lBfrtip',
        buttons: [
            {
                extend: 'copy',
                exportOptions: { columns: "thead th:not(.noExport)" }
            },
            {
                extend: 'excel',
                exportOptions: { columns: "thead th:not(.noExport)" }
            },
            {
                extend: 'pdfHtml5',
                orientation: 'landscape',
                pageSize: 'LEGAL',
                title: 'User Reg',
                exportOptions: { columns: "thead th:not(.noExport)" },
                customize: function (doc) {
                    // WaterMarkOnPdf(doc);
                }
            }
        ],
        drawCallback: function (settings) {
            $("#tbldata tbody").off("click", ".cls-btnedit").on("click", ".cls-btnedit", function () {
                let rowData = table.row($(this).closest("tr")).data();
                if (rowData != null) {
                    $("#UserName").val(rowData.DomainId);

                    $("#Role").val(rowData.RoleNames);
                }
            });
        }
    });
}
$(document).on('click', '.updateuser', async function () {

    const userId = $(this).data('id');
    const isActive = $(this).data('active');      // Current status
    const newStatus = !isActive;                  // Status after update

    const result = await Swal.fire({
        title: "Are you sure?",
        text: `Do you want to ${newStatus ? "activate" : "deactivate"} this user?`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes"
    });

    if (!result.isConfirmed) {
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();

    try {
        const resp = await fetch("/IntelliSearch/Account/UpdateApprovalStatus", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            },
            body: JSON.stringify({
                Id: userId,
                Active: newStatus
            })
        });

        if (!resp.ok) {
            const text = await resp.text();

            Swal.fire({
                icon: "error",
                title: "Update Failed",
                text: text
            });

            return;
        }

        const data = await resp.json();

        if (data.Code === 200) {

            BindData();

            Swal.fire({
                position: "top-end",
                icon: "success",
                title: `User ${newStatus ? "activated" : "deactivated"} successfully.`,
                showConfirmButton: false,
                timer: 1500
            });

        } else {
            Swal.fire({
                icon: "error",
                title: data.Message || "Update failed."
            });
        }

    } catch (err) {

        Swal.fire({
            icon: "error",
            title: "Network Error",
            text: "Unable to connect to the server."
        });

        console.error(err);
    }
});