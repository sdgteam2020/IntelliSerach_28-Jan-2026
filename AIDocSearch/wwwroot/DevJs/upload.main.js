$(document).ready(function () {

    const maxSize = 5 * 1024 * 1024;

    const modalEl = document.getElementById("uploadPdfModal");
    const $fileInput = $("#fileInput");
    const $dropZone = $("#uploadDropZone");
    const $progressBox = $("#uploadProgressBox");

    let currentXHR = null;
    let isUploading = false;
    let uploadSucceeded = false;

    function uploadPDF(file) {

        if (isUploading || !file) return;

        $("#fileError").text("");

        if (!/\.pdf$/i.test(file.name)) {
            toastr.warning("Only PDF files are allowed.");
            $fileInput.val("");
            return;
        }

        if (file.size > maxSize) {
            toastr.warning("File size must not exceed 5 MB.");
            $fileInput.val("");
            return;
        }

        if (file.size === 0) {
            toastr.warning("Empty files are not allowed.");
            $fileInput.val("");
            return;
        }

        const form = $("#uploadForm")[0];
        const formData = new FormData(form);

        // Works for both browse and drag/drop
        formData.set("FileName", file, file.name);

        $("#uploadFileName").text(file.name);
        $("#uploadFileSize").text(
            (file.size / (1024 * 1024)).toFixed(2) + " MB"
        );

        $("#uploadPercent").text("0%");
        $("#uploadStatus").text("Uploading...");

        $("#uploadProgressBar").css({
            width: "0%",
            background: "#1a73e8"
        });

        $progressBox.css("display", "flex");
        $dropZone.addClass("uploading");
        $fileInput.prop("disabled", true);

        isUploading = true;

        currentXHR = $.ajax({
            url: $(form).attr("action"),
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            dataType: "json",

            xhr: function () {
                const xhr = $.ajaxSettings.xhr();

                if (xhr.upload) {
                    xhr.upload.addEventListener("progress", function (e) {

                        if (e.lengthComputable) {
                            const percent = Math.min(
                                100,
                                Math.round(e.loaded / e.total * 100)
                            );

                            $("#uploadPercent").text(percent + "%");
                            $("#uploadProgressBar").css(
                                "width", percent + "%"
                            );

                            if (percent === 100) {
                                $("#uploadStatus").text(
                                    "Processing PDF..."
                                );
                            }
                        }
                    });
                }

                return xhr;
            },

            success: function (response) {

                if (response.success) {

                    uploadSucceeded = true;

                    $("#uploadProgressBar").css({
                        width: "100%",
                        background: "#188038"
                    });

                    $("#uploadPercent").text("100%");
                    $("#uploadStatus").text("Upload completed");

                    toastr.success(
                        response.message || "File uploaded successfully."
                    );

                    form.reset();

                    // Close Bootstrap modal after success
                    bootstrap.Modal.getOrCreateInstance(modalEl).hide();

                } else {
                    showError(
                        response.errors?.length
                            ? response.errors.join(" ")
                            : response.message || "File upload failed."
                    );
                }
            },

            error: function (xhr, status) {

                if (status === "abort") {
                    toastr.info("Upload cancelled.");
                    $progressBox.hide();
                    return;
                }

                let message = "Something went wrong while uploading.";

                if (xhr.status === 413) {
                    message = "File size exceeds the allowed limit.";
                } else if (xhr.responseJSON) {
                    const response = xhr.responseJSON;

                    message = response.errors?.length
                        ? response.errors.join(" ")
                        : response.message || message;
                }

                showError(message);
            },

            complete: function () {
                isUploading = false;
                currentXHR = null;
                $fileInput.prop("disabled", false).val("");
                $dropZone.removeClass("uploading");
            }
        });
    }

    function showError(message) {
        $("#uploadStatus").text("Upload failed");
        $("#uploadProgressBar").css("background", "#d93025");
        toastr.error(message);
    }

    // File selection
    $fileInput.on("change", function () {
        if (this.files.length) {
            uploadPDF(this.files[0]);
        }
    });

    // Drag and drop
    $dropZone.on("dragover dragenter", function (e) {
        e.preventDefault();
        e.stopPropagation();

        if (!isUploading) {
            $dropZone.addClass("dragover");
        }
    });

    $dropZone.on("dragleave dragend", function (e) {
        e.preventDefault();
        e.stopPropagation();
        $dropZone.removeClass("dragover");
    });

    $dropZone.on("drop", function (e) {
        e.preventDefault();
        e.stopPropagation();
        $dropZone.removeClass("dragover");

        if (isUploading) return;

        const files = e.originalEvent.dataTransfer.files;

        if (files.length !== 1) {
            toastr.warning("Please select only one PDF file.");
            return;
        }

        uploadPDF(files[0]);
    });

    // Cancel upload
    $("#cancelUpload").on("click", function () {
        if (currentXHR && isUploading) {
            currentXHR.abort();
        } else {
            $progressBox.hide();
        }
    });

    // Prevent modal closing during upload
    modalEl.addEventListener("hide.bs.modal", function (e) {
        if (isUploading) {
            e.preventDefault();
          
        }
    });

    // Refresh table after modal closes successfully
    modalEl.addEventListener("hidden.bs.modal", function () {

        if (uploadSucceeded) {
            window.location.reload();
            return;
        }

        $("#uploadForm")[0].reset();
        $progressBox.hide();
        $("#fileError").text("");
        $dropZone.removeClass("dragover");
    });

    // Prevent normal submission
    $("#uploadForm").on("submit", function (e) {
        e.preventDefault();
    });

});