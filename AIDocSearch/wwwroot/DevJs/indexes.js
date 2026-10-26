$(function () {
    const $form = $("#indexForm");

    // Use the action URLs configured on the form's data-* attributes.
    const urls = {
        list:  "GetActiveIndexes",
        get:   "GetActiveIndexById",
        create: "Create",
        delete: "Delete",
        ActiveAndDeActive: "ActiveAndDeActive"
    };

    function getValue(object, propertyName) {
        if (!object) return undefined;

        const matchingKey = Object.keys(object).find(
            key => key.toLowerCase() === propertyName.toLowerCase()
        );

        return matchingKey ? object[matchingKey] : undefined;
    }

    function token() {
        return $('input[name="__RequestVerificationToken"]').val();
    }

    function postJson(url, data) {
        return $.ajax({
            url: url,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            data: data === undefined ? undefined : JSON.stringify(data),
            headers: {
                RequestVerificationToken: token()
            }
        });
    }

    function escapeHtml(value) {
        return $("<div>").text(value ?? "").html();
    }

    function resetForm() {
        if ($form.length && $form[0]) {
            $form[0].reset();
        }

        $("#Id").val("0");
        $("#btnSave").text("Add");
    }

    function loadIndexes() {
        postJson(urls.list)
            .done(function (records) {
                const $tbody = $("#indexesTableBody");
                $tbody.empty();

                if (!Array.isArray(records) || records.length === 0) {
                    $tbody.append(
                        '<tr><td colspan="7" class="text-center">No records found.</td></tr>'
                    );
                    return;
                }
                let count = 1;
                records.forEach(function (item) {
                    const id = getValue(item, "Id");
                    const indexName = getValue(item, "Index_Name");
                    const ipAddress = getValue(item, "IpAddress");
                    const key = getValue(item, "Key");
                    const createdOn = getValue(item, "CreatedOn");
                    const isActive = getValue(item, "IsActive");
                    // <button type="button"
                    //     class="btn btn-sm btn-warning btn-edit"
                    //     data-id="${escapeHtml(id)}">
                    //     Edit
                    // </button>
                    const row = `
                        <tr>
                            <td>${escapeHtml(count++)}</td>
                            <td>${escapeHtml(indexName)}</td>
                            <td>${escapeHtml(ipAddress)}</td>
                            <td>${escapeHtml(key)}</td>
                            <td>${escapeHtml(formatDateTime(createdOn))}</td>
                           <td>
                                <span class="badge ${isActive ? "bg-success" : "bg-warning text-black"}">
                                    ${isActive ? "Active" : "Deactivate"}
                                </span>
                            </td>
                            <td>
                               
                                <button type="button"
                                        class="btn btn-sm btn-danger btn-delete"
                                        data-id="${escapeHtml(id)}">
                                    Delete
                                </button>
                               <button type="button"
                                class="btn btn-sm ${isActive ? "btn-warning" : "btn-success"} btn-updatestaus"
                                data-id="${escapeHtml(id)}">
                            ${isActive ? "Deactivate" : "Activate"}
                        </button>
                            </td>
                        </tr>`;

                    $tbody.append(row);
                });
            })
            .fail(function (xhr) {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: xhr.responseJSON?.message || "Could not load records."
                });
            });
    }

    $form.on("submit", function (event) {
        event.preventDefault();

        const id = parseInt($("#Id").val(), 10) || 0;
        const isEdit = id > 0;

        const requestData = {
            Id: id,
            Index_Name: ($("#Index_Name").val() || "").trim(),
            IpAddress: ($("#IpAddress").val() || "").trim()
        };

        if (!requestData.Index_Name || !requestData.IpAddress) {
            Swal.fire({
                icon: "warning",
                title: "Required fields",
                text: "Enter both Index Name and IP Address."
            });
            return;
        }

        Swal.fire({
            title: isEdit ? "Update index?" : "Add index?",
            text: isEdit
                ? "Are you sure you want to update this index?"
                : "Are you sure you want to add this index?",
            icon: "question",
            showCancelButton: true,
            confirmButtonText: isEdit ? "Yes, update" : "Yes, add",
            cancelButtonText: "Cancel"
        }).then(function (result) {
            if (!result.isConfirmed) return;

            // Your Create action inserts for Id == 0 and updates for Id > 0.
            postJson(urls.create, requestData)
                .done(function (response) {
                    Swal.fire({
                        icon: "success",
                        title: "Success",
                        text: response.message ||
                            (isEdit
                                ? "Index updated successfully."
                                : "Index added successfully.")
                    });

                    resetForm();
                    loadIndexes();
                })
                .fail(function (xhr) {
                    const response = xhr.responseJSON;
                    let errorMessages = [];

                    if (response?.errors) {
                        // ModelState errors are returned as { fieldName: [message, ...] }
                        Object.values(response.errors).forEach(function (fieldErrors) {
                            if (Array.isArray(fieldErrors)) {
                                errorMessages.push(...fieldErrors);
                            } else if (fieldErrors) {
                                errorMessages.push(fieldErrors);
                            }
                        });
                    }

                    const message = errorMessages.length > 0
                        ? errorMessages.join("\n")
                        : response?.message || "The operation could not be completed.";

                    Swal.fire({
                        icon: "error",
                        title: "Validation error",
                        text: message
                    });
                });
        });
    });

    $("#indexesTableBody").on("click", ".btn-edit", function () {
        const id = $(this).data("id");

        // Your GetActiveIndexById action currently accepts int id.
        postJson(urls.get, { id: id })
            .done(function (item) {
                $("#Id").val(getValue(item, "Id") ?? 0);
                $("#Index_Name").val(getValue(item, "Index_Name") ?? "");
                $("#IpAddress").val(getValue(item, "IpAddress") ?? "");
                $("#btnSave").text("Update");

                $("html, body").animate({ scrollTop: 0 }, "fast");
            })
            .fail(function (xhr) {
                Swal.fire({
                    icon: "error",
                    title: "Error",
                    text: xhr.responseJSON?.message ||
                        "Could not load this index."
                });
            });
    });

    $("#indexesTableBody").on("click", ".btn-delete", function () {
        const id = $(this).data("id");

        Swal.fire({
            title: "Delete index?",
            text: "This record will be marked as deleted.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Yes, delete",
            cancelButtonText: "Cancel",
            confirmButtonColor: "#d33"
        }).then(function (result) {
            if (!result.isConfirmed) return;

            // Your Delete action accepts int id.
            postJson(`${urls.delete}?id=${encodeURIComponent(id)}`)
                .done(function (response) {
                    Swal.fire({
                        icon: "success",
                        title: "Deleted",
                        text: response.message || "Index deleted successfully."
                    });

                    loadIndexes();
                })
                .fail(function (xhr) {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: xhr.responseJSON?.message ||
                            "Could not delete this index."
                    });
                });
        });
    });
    $("#indexesTableBody").on("click", ".btn-updatestaus", function () {
        const id = $(this).data("id");
        const action = $(this).text().trim(); // "Activate" or "Deactivate"
        const isActivating = action === "Activate";

        Swal.fire({
            title: `${action} index?`,
            text: isActivating
                ? "This index will be activated."
                : "This index will be deactivated.",
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: `Yes, ${action}`,
            cancelButtonText: "Cancel",
            confirmButtonColor: isActivating ? "#198754" : "#d33"
        }).then(function (result) {
            if (!result.isConfirmed) return;

            postJson(`${urls.ActiveAndDeActive}?id=${encodeURIComponent(id)}`)
                .done(function (response) {
                    Swal.fire({
                        icon: "success",
                        title: "Status updated",
                        text: response.message
                    });

                    loadIndexes();
                })
                .fail(function (xhr) {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: xhr.responseJSON?.message || "Could not update this index."
                    });
                });
        });
    });

    $("#btnNew").on("click", resetForm);

    loadIndexes();
});