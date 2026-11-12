$(document).ready(async function () {

    // sendFileToClient(
    //     "fs-kapoor_40",
    //     "/UploadForIndexing/kapoor/CommentsHistory_WM_20260615_165646.pdf"
    // );

    let dattt = await getUserActivity("SearchHistory");

    if (dattt) {

        dattt = JSON.parse(dattt);
        $(".spnlastsearchdate").html(timeAgo(dattt.Date));
        $(".spnRecentSearches").html(dattt.Data);

    }
    loadDocWebCount();
});
function sendFileToClient(crawlerId, fileName) {

    const csrfToken = token();

    if (!csrfToken) {
        Swal.fire({
            icon: "error",
            title: "Security Error",
            text: "Anti-forgery token is missing."
        });
        return;
    }

    $.ajax({
        url: "/IntelliSearch/Files/SendFileToClient",
        type: "POST",
        dataType: "json",

        headers: {
            "RequestVerificationToken": csrfToken
        },

        data: {
            crawlerId: crawlerId,
            serverFilePath: fileName
        },

        beforeSend: function () {
            console.log("Sending file to client:", fileName);

            Swal.fire({
                title: "Transferring File",
                text: "Please wait while the file is being transferred...",
                allowOutsideClick: false,
                allowEscapeKey: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
        },

        success: function (response) {

            Swal.close();

            console.log("Transfer response:", response);

            if (!response) {
                Swal.fire({
                    icon: "error",
                    title: "Invalid Response",
                    text: "Server returned an empty response."
                });
                return;
            }

            const success = response.Success === true;
            const errorCode = response.ErrorCode || "";
            const message = response.Message || "";
            const transferId = response.TransferId || "";

            // File successfully saved on client
            if (success) {

                console.log("Transfer ID:", transferId);

                Swal.fire({
                    icon: "success",
                    title: "File Transfer Successful",
                    text: message || "File saved successfully on client.",
                    confirmButtonText: "OK"
                });

                return;
            }

            // Handle specific error types
            let errorTitle = "File Transfer Failed";
            let errorMessage = message || "Unable to transfer file.";

            switch (errorCode) {

                case "DUPLICATE_FILE":
                    errorTitle = "Duplicate File";
                    errorMessage = message ||
                        "This file already exists on the client.";
                    break;

                case "INCOMPLETE_TRANSFER":
                    errorTitle = "Incomplete Transfer";
                    errorMessage = message ||
                        "The file was not transferred completely.";
                    break;

                case "TRANSFER_NOT_FOUND":
                    errorTitle = "Transfer Not Found";
                    errorMessage = message ||
                        "The file transfer session was not found.";
                    break;

                case "ACCESS_DENIED":
                    errorTitle = "Access Denied";
                    errorMessage = message ||
                        "Permission denied while saving the file.";
                    break;

                case "FILE_IO_ERROR":
                    errorTitle = "File System Error";
                    errorMessage = message ||
                        "Unable to write the file on the client.";
                    break;

                case "CONFIRMATION_TIMEOUT":
                    errorTitle = "Transfer Confirmation Timeout";
                    errorMessage = message ||
                        "Client did not confirm the transfer.";
                    break;

                case "CONNECTION_ERROR":
                    errorTitle = "Connection Error";
                    errorMessage = message ||
                        "Unable to confirm file transfer status.";
                    break;

                case "NULL_RESPONSE":
                    errorTitle = "No Client Response";
                    errorMessage = message ||
                        "Client returned an empty transfer response.";
                    break;

                case "TRANSFER_FAILED":
                    errorTitle = "Transfer Failed";
                    break;

                default:
                    errorTitle = "File Transfer Failed";
                    break;
            }

            console.error("Transfer failed:", {
                transferId: transferId,
                errorCode: errorCode,
                message: errorMessage
            });

            Swal.fire({
                icon: "error",
                title: errorTitle,
                text: errorMessage,
                confirmButtonText: "OK"
            });
        },

        error: function (xhr, textStatus, errorThrown) {

            Swal.close();

            console.error("HTTP Status:", xhr.status);
            console.error("Response:", xhr.responseText);
            console.error("Error:", errorThrown);

            const response = xhr.responseJSON;

            let errorTitle = "Server Error";
            let errorMessage = response?.message ||
                "Unable to transfer file.";

            if (textStatus === "timeout") {
                errorTitle = "Request Timeout";
                errorMessage = "The transfer request timed out.";
            }
            else if (xhr.status === 400) {
                errorTitle = "Invalid Request";
            }
            else if (xhr.status === 401) {
                errorTitle = "Unauthorized";
                errorMessage = "Please log in again.";
            }
            else if (xhr.status === 403) {
                errorTitle = "Access Denied";
            }
            else if (xhr.status === 404) {
                errorTitle = "Not Found";
            }
            else if (xhr.status === 500) {
                errorTitle = "Internal Server Error";
            }

            Swal.fire({
                icon: "error",
                title: errorTitle,
                text: errorMessage,
                confirmButtonText: "OK"
            });
        }
    });
}

function loadDocWebCount() {


    const csrfToken = token();

    if (!csrfToken) {
        throw new Error("Anti-forgery token is missing.");
    }

    $.ajax({
        url: '/IntelliSearch/Search/GetDoc_webCount',
        type: 'POST',
        dataType: 'json',
        headers: {
            'RequestVerificationToken': csrfToken
        },

        success: function (response) {

            if (response && response.Data) {

                let responses = response.Data.Responses;

                let documentCount = 0;
                let scrapWebCount = 0;

                if (responses && responses.length > 0) {
                    documentCount =
                        responses[0]?.Hits?.Total?.Value ?? 0;
                }

                if (responses && responses.length > 1) {
                    scrapWebCount =
                        responses[1]?.Hits?.Total?.Value ?? 0;
                }

                $('#totalDocuments').text(documentCount+"+");
                $('#totalScrapWeb').text(scrapWebCount);
            }
            else {
                $('#totalDocuments').text('0');
                $('#totalScrapWeb').text('0');
            }
        },

        error: function (xhr, status, error) {

            console.error(
                'Error loading document/web count:',
                error
            );

            $('#totalDocuments').text('-');
            $('#totalScrapWeb').text('-');
        }
    });
}