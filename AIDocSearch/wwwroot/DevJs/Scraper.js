let currentCrawlJobId = null;
$(document).ready(function () {

 
// ==========================================
// Status Button Click
// ==========================================


let statusPollingTimer = null;



    $("#ScraperForm").on("submit", async function (e) {
        // $(".pdfviewcontainer").addClass("d-none");
        // $(".webviewcontainer").addClass("d-none");
        $("#websiteUrl").text("");
        $("#downloadCount").text("");
        /*$("#downloadDir").text(scraper.download_directory);*/

        // ===== Clear Old Rows =====
        if ($.fn.DataTable.isDataTable('#pdfTable')) {
            $('#pdfTable').DataTable().destroy();
        }

        $("#websitewebUrl").text("");
        $("#pages_crawled").text("");
        if ($.fn.DataTable.isDataTable('#webTable')) {
            $('#webTable').DataTable().destroy();
        }

        e.preventDefault(); // stop normal submit

        // client-side validation (optional but nice)
        if (typeof $(this).valid === "function" && !$(this).valid()) {
            return;
        }
        const token = $('input[name="__RequestVerificationToken"]').val();
        const form = this;

        // build FormData (includes files)
        const fd = new FormData(form);

        const dto = {
            Url: $("#Url").val(),
            Abbreviation: $("#Abbreviation").val(),
            IsPdf: $('input[name="IsPdf"]:checked').val() === "true"
        };
       
      



        
        Swal.fire({
            title: "Do you want to Scrap?",
            text: "You won't be able to revert this!",
            icon: "warning",
            showCancelButton: true,
            confirmButtonColor: "#3085d6",
            cancelButtonColor: "#d33",
            confirmButtonText: "Yes, Scrap it!"
        }).then(async (result) => {
            if (result.isConfirmed) {
                $("#loading").show();
                reset();
                try {
                    const encInfo = await getSalt();
                    const cipherText = encryptPayload(dto, encInfo.key, encInfo.iv);
                    const fd1 = new FormData();
                    fd1.append("payload", cipherText);

                    const response = await fetch('/IntelliSearch/Scraper/Scraperingpayload', {
                        method: 'POST',
                        headers: {
                            "RequestVerificationToken": token // matches [ValidateAntiForgeryToken]
                        },
                        body: fd1 // don't set content-type; fetch will handle for FormData
                    });

                    // Check if the response is ok (status code 200-299)
                    if (!response.ok) {
                        const text = await response.text();
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: "Save failed.\n" + text,
                            
                            confirmButtonText: "OK",
                            showConfirmButton: true
                        });
                        $("#loading").hide();
                        return;
                    }

                    // Parse the JSON response
                    const data = await response.json(); // Parse JSON response

                    // Example: { code: 200, message: "...", data: { ... } }
                    if (data.Code === 200) {

                        $("#popup_loading").removeClass("d-none");
                        if ($("#pdfOption").is(":checked")) {
                            BindpdfScraping(data)
                            $(".pdfviewcontainer").removeClass("d-none");
                        }
                        else {
                            BindWebScraping(data)
                            $(".webviewcontainer").removeClass("d-none");
                        }

                        $("#loading").hide();
                    } else if (data.Code === 400) {
                        $("#loading").hide();
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: data.Message + "\n " + data.Data,
                           
                            confirmButtonText: "OK",
                            showConfirmButton: true
                        });
                    } else {
                        $("#loading").hide();
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: data.Message,
                           
                            confirmButtonText: "OK",
                            showConfirmButton: true
                        });
                    }
                } catch (error) {
                    $("#loading").hide();
                    // Handle any errors from the fetch request
                    Swal.fire({
                        text: "An error occurred Api Not Working: " + error.message
                    });
                }
            }
        });
    });
});


function BindWebScraping(Data) {

    // If API response is wrapped inside Data
    Data = Data?.Data;

    // ===== Safety Check =====
    if (!Data) {

        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "No data available",
            showConfirmButton: true
        });

        $("#loading").hide();
        return;
    }
    currentCrawlJobId = Data.JobId;
    // ===== Basic Information =====

    $("#websitewebUrl").text(
        Data.StartUrl || $("#Url").val() || "-"
    );

    $("#job_id").text(
        Data.JobId || "-"
    );

    $("#alias").text(
        Data.Alias || "-"
    );

    $("#index_name").text(
        Data.IndexName || "-"
    );

    $("#deleted_existing_docs").text(
        Data.DeletedExistingDocs ?? 0
    );

    // ===== Status =====

    $("#crawl_status").html(
        getStatusBadge(Data.Status)
    );

    // ===== Message =====

    $("#scraping_message").text(
        Data.Message || "-"
    );

    // ===== Boolean Information =====

    $("#registry_reused").html(
        getBooleanBadge(Data.RegistryReused)
    );

    $("#continue_from_last").html(
        getBooleanBadge(Data.ContinueFromLast)
    );

    $("#recovery_resumed").html(
        getBooleanBadge(Data.RecoveryResumed)
    );

    $("#purge_applied").html(
        getBooleanBadge(Data.PurgeApplied)
    );
    // Set status URL on button
    if (Data.StatusUrl) {
        setStatusUrl(
            GetFullStatusUrl(Data.StatusUrl)
        );
    }
    // ===== Action URLs =====

    // setActionUrl("#status_url", Data.StatusUrl);
    // setActionUrl("#logs_url", Data.LogsUrl);
    // setActionUrl("#config_url", Data.ConfigUrl);

    // ===== Show Status Message =====

    if (Data.Status) {

        const status = Data.Status.toLowerCase();

        if (status === "running") {

            Swal.fire({
                position: "top-end",
                icon: "success",
                title: "Scraping started",
                text: Data.Message || "New Scrapy crawl started.",
                showConfirmButton: false,
                timer: 3500
            });

        }
        else if (
            status === "completed" ||
            status === "success"
        ) {

            Swal.fire({
                position: "top-end",
                icon: "success",
                title: "Scraping completed",
                text: Data.Message || "Scraping completed successfully.",
                showConfirmButton: false,
                timer: 3500
            });

        }
        else if (
            status === "failed" ||
            status === "error"
        ) {

            Swal.fire({
                position: "top-end",
                icon: "error",
                title: "Scraping failed",
                text: Data.Message || "Scraping failed.",
                showConfirmButton: true
            });

        }
    }

    // ===== Hide Loading =====

    $("#loading").hide();
}

const SCRAPY_API_BASE =
    "https://192.168.10.208/fastapi-scrapy-crawler";

function GetFullStatusUrl(statusUrl) {

    if (!statusUrl) {
        return null;
    }

    // Already absolute
    if (statusUrl.startsWith("http://") ||
        statusUrl.startsWith("https://")) {

        return statusUrl;
    }

    return SCRAPY_API_BASE + statusUrl;
}


// ==========================================
// Set Status URL
// ==========================================

function setStatusUrl(url) {

    if (!url) {

        $("#status_url")
            .attr("data-status-url", "")
            .addClass("disabled");

        return;
    }

    $("#status_url")
        .attr("data-status-url", url)
        .removeClass("disabled");

    statusPollingTimer = setInterval(function () {

        const status = $("#crawl_status").text().trim().toLowerCase();

        if (status !== "completed") {
            CheckCrawlStatus(url);
        } else {

            // Show PDF button
            $("#btn_view_pdfs").removeClass("d-none");
            $("#btn_view_pdfs").show().off("click").on("click", function () { LoadPdfLinks(currentCrawlJobId); });
            clearInterval(statusPollingTimer);
            statusPollingTimer = null;
        }

    }, 2000);
}
// ==========================================
// Status Badge
// ==========================================

function getStatusBadge(status) {

    if (!status) {

        return `
    <span class="badge bg-secondary" >
        Unknown
            </span >
    `;
    }

    switch (status.toLowerCase()) {

        case "running":

            return `
    <span class="badge bg-primary" >
        <i class="fa fa-spinner fa-spin me-1"></i>
Scraping
                </span >
    `;

        case "completed":
        case "success":

            return `
    <span class="badge bg-success" >
        <i class="fa fa-check me-1"></i>
                    ${ status }
                </span >
    `;

        case "failed":
        case "error":

            return `
    <span class="badge bg-danger" >
        <i class="fa fa-times me-1"></i>
                    ${ status }
                </span >
    `;

        case "cancelled":

            return `
    <span class="badge bg-warning text-dark" >
        <i class="fa fa-ban me-1"></i>
Cancelled
                </span >
    `;

        default:

            return `
    < span class="badge bg-secondary" >
        ${ status }
                </span >
    `;
    }
}


// ==========================================
// Boolean Badge
// ==========================================

function getBooleanBadge(value) {

    if (value === true) {

        return `
    <span class="badge bg-success" >
        <i class="fa fa-check me-1"></i>
Yes
            </span >
    `;
    }

    if (value === false) {

        return `
    <span class="badge bg-secondary" >
        <i class="fa fa-times me-1"></i>
No
            </span >
    `;
    }

    return `
    <span class="badge bg-secondary" >
        -
        </span >
    `;
}


let crawlStatusTimer = null;

function StartCrawlStatusPolling(statusUrl) {

    StopCrawlStatusPolling();

    // Open popup
    ShowCrawlStatusPopup();

    // Check immediately
    CheckCrawlStatus(statusUrl);

    // Check every 3 seconds
    crawlStatusTimer = setInterval(function () {

        CheckCrawlStatus(statusUrl);

    }, 3000);
}


function StopCrawlStatusPolling() {

    if (crawlStatusTimer) {

        clearInterval(crawlStatusTimer);

        crawlStatusTimer = null;
    }
}


function CheckCrawlStatus(statusUrl) {

    console.log("========== CHECK CRAWL STATUS ==========");
    console.log("Status URL:", statusUrl);

    if (!statusUrl) {

        console.error("Status URL is empty.");

        $("#popup_message").text(
            "Status URL is not available."
        );

        return;
    }

    $.ajax({

        url: statusUrl,

        type: "GET",

        dataType: "json",

        headers: {
            "Accept": "application/json"
        },

        success: function (response) {

            console.log("Status API Success");
            console.log("Response:", response);

            UpdateCrawlStatusPopup(response);

            const status =
                (response.status || "").toLowerCase();

            console.log("Current status:", status);

            if (
                status === "completed" ||
                status === "failed" ||
                status === "error" ||
                status === "cancelled" ||
                status === "stopped"
            ) {

                StopCrawlStatusPolling();
               
                $("#loading").hide();

                console.log(
                    "Crawl finished. Polling stopped."
                );
            }
        },

        error: function (xhr, textStatus, errorThrown) {

            console.error("========== STATUS API ERROR ==========");

            console.error("HTTP Status:", xhr.status);
            console.error("Status Text:", xhr.statusText);
            console.error("Text Status:", textStatus);
            console.error("Error:", errorThrown);
            console.error("Response:", xhr.responseText);

            $("#popup_message").text(
                "Unable to retrieve current status."
            );

            $("#popup_loading").html(`
    <i class="fa fa-exclamation-triangle text-danger me-2" ></i >
        Status API Error: ${ xhr.status }
`);
        }
    });
}




function UpdateCrawlStatusPopup(Data) {

    if (!Data) {
        return;
    }

    // Job ID
    $("#popup_job_id").text(
        Data.job_id || "-"
    );
    
    // Status
    $("#popup_crawl_status").html(
        getStatusBadge(Data.status)
    );
    $("#crawl_status").html(
        getStatusBadge(Data.status)
    );
    $("#crawl_status_div").html(
        getStatusBadge(Data.status)
    );
    // Statistics
    $("#popup_pages_crawled").text(
        Data.pages_crawled ?? 0
    );

    $("#popup_docs_indexed").text(
        Data.docs_indexed ?? 0
    );

    $("#popup_links_found").text(
        Data.links_found ?? 0
    );

    // Errors
    $("#popup_errors").text(
        Data.errors ?? 0
    );

    // Message
    $("#popup_message").text(
        Data.message || "-"
    );

    // Final state
    const status =
        (Data.status || "").toLowerCase();

    if (
        status === "completed" ||
        status === "failed" ||
        status === "error" ||
        status === "cancelled" ||
        status === "stopped"
    ) {

        $("#popup_loading").html(`
    <i class="fa fa-check-circle text-success me-2" ></i >
       ${status}
            `);
    }
}


// function ShowCrawlStatusPopup() {

//     Swal.fire({
//         title: "Web Scraping",
//         html: `
//     <div class="text-start" >

//                 <div class="mb-3">
//                     <strong>Status:</strong>
//                     <span id="popup_crawl_status"
//                           class="badge bg-primary">
//                         Running
//                     </span>
//                 </div>

//                 <div class="mb-2">
//                     <strong>Job ID:</strong>
//                     <div id="popup_job_id"
//                          class="text-muted text-break">
//                         -
//                     </div>
//                 </div>

//                 <hr>

//                 <div class="row g-2 text-center">

//                     <div class="col-4">
//                         <div class="border rounded p-2">
//                             <small class="text-muted d-block">
//                                 Pages
//                             </small>
//                             <strong id="popup_pages_crawled">
//                                 0
//                             </strong>
//                         </div>
//                     </div>

//                     <div class="col-4">
//                         <div class="border rounded p-2">
//                             <small class="text-muted d-block">
//                                 Indexed
//                             </small>
//                             <strong id="popup_docs_indexed">
//                                 0
//                             </strong>
//                         </div>
//                     </div>

//                     <div class="col-4">
//                         <div class="border rounded p-2">
//                             <small class="text-muted d-block">
//                                 Links
//                             </small>
//                             <strong id="popup_links_found">
//                                 0
//                             </strong>
//                         </div>
//                     </div>

//                 </div>

//                 <div class="mt-3">
//                     <strong>Errors:</strong>
//                     <span id="popup_errors"
//                           class="badge bg-secondary">
//                         0
//                     </span>
//                 </div>

//                 <div class="mt-3">
//                     <strong>Message:</strong>
//                     <div id="popup_message"
//                          class="text-muted text-break">
//                         Starting crawl...
//                     </div>
//                 </div>

//                 <div class="mt-3 text-center"
//                      id="popup_loading">

//                     <i class="fa fa-spinner fa-spin me-2"></i>
//                     Checking current status...

//                 </div>

//             </div>
// `,

//         width: 550,

//         showConfirmButton: true,
//         confirmButtonText: "Close",

//         allowOutsideClick: false,
//         allowEscapeKey: false
//     });
// }


// ==========================================
// Action URL
// ==========================================

function setActionUrl(selector, url) {

    const element = $(selector);

    if (url) {

        element
            .attr("href", url)
            .removeClass("disabled")
            .removeAttr("aria-disabled");

    }
    else {

        element
            .attr("href", "#")
            .addClass("disabled")
            .attr("aria-disabled", "true");
    }
}




function DateFormat(inputDate) {
    // Remove UTC and convert to Date
    let date = new Date(inputDate.replace(" UTC", ""));

    function pad(n) {
        return n < 10 ? '0' + n : n;
    }

    let formatted =
        pad(date.getDate()) + "/" +
        pad(date.getMonth() + 1) + "/" +
        date.getFullYear() + " " +
        pad(date.getHours()) + ":" +
        pad(date.getMinutes()) + ":" +
        pad(date.getSeconds());

    return formatted;
}

function LoadPdfLinks(jobId) {

    if (!jobId) {

        Swal.fire({
            icon: "warning",
            title: "Job ID not available"
        });

        return;
    }

  

    $.ajax({

        url: "https://192.168.10.208/fastapi-scrapy-crawler/api/v1/pdf/list-by-job",

        type: "POST",

        contentType: "application/json",

        dataType: "json",

        headers: {
            "Accept": "application/json"
        },

        data: JSON.stringify({
            job_id: jobId,
            max_links: 1000
        }),

        success: function (response) {

            console.log("PDF List:", response);

            ShowPdfListPopup(response);
        },

        error: function (xhr) {

            console.error(
                "PDF List API Error:",
                xhr.status,
                xhr.responseText
            );

            Swal.fire({
                icon: "error",
                title: "Unable to load PDF links",
                text: xhr.responseText || "PDF API failed."
            });
        }
    });
}

function ShowPdfListPopup(response) {

    if (!response) {

        Swal.fire({
            icon: "error",
            title: "No PDF data available"
        });

        return;
    }

    const pdfLinks = response.pdf_links || [];

    let html = "";

    // ==========================================
    // No PDF Found
    // ==========================================

    if (pdfLinks.length === 0) {

        html = `
            <div class="text-center text-muted py-4">

                <i class="fa fa-file-pdf-o fa-2x mb-2"></i>

                <div>
                    No PDF links found.
                </div>

            </div>
        `;
    }
    else {

        // ==========================================
        // PDF Table
        // ==========================================

        html = `
            <div class="table-responsive">

                <table id="tblPdfList"
                       class="table table-bordered table-hover align-middle mb-0"
                       style="width:100%;">

                    <thead class="table-light">

                        <tr>

                            <th class="text-center">
                                #
                            </th>

                            <th>
                                Page URL
                            </th>

                            <th class="text-center">
                                PDF
                            </th>

                        </tr>

                    </thead>

                    <tbody>
        `;

        // ==========================================
        // Bind PDF Rows
        // ==========================================

        $.each(pdfLinks, function (index, item) {

            const pdfUrl = item.pdf_url || "";
            const pageUrl = item.page_url || "";

            html += `
                <tr>

                    <!-- Serial Number -->
                    <td class="text-center">

                        <span class="badge bg-secondary">
                            ${index + 1}
                        </span>

                    </td>

                    <!-- Page URL -->
                    <td>

                        ${pageUrl
                    ?
                    `
                                <a href="${escapeAttribute(pageUrl)}"
                                   target="_blank"
                                   rel="noopener noreferrer"
                                   class="text-decoration-none text-break">

                                    <i class="fa fa-external-link me-1"></i>

                                    ${escapeHtml(pageUrl)}

                                </a>
                                `
                    :
                    `
                                <span class="text-muted">
                                    -
                                </span>
                                `
                }

                    </td>

                    <!-- PDF -->
                    <td class="text-center">

                        ${pdfUrl
                    ?
                    `
                                <a href="${escapeAttribute(pdfUrl)}"
                                   target="_blank"
                                   rel="noopener noreferrer"
                                   class="text-danger"
                                   title="Open PDF">

                                    <i class="fa fa-file-pdf fa-lg"></i>

                                </a>
                                `
                    :
                    `
                                <span class="text-muted"
                                      title="PDF URL not available">

                                    <i class="fa fa-file-pdf fa-lg"></i>

                                </span>
                                `
                }

                    </td>

                </tr>
            `;
        });

        // ==========================================
        // Close Table
        // ==========================================

        html += `
                    </tbody>

                </table>

            </div>

            <div class="mt-3 pt-3 border-top">

                <button type="button"
                        id="btn_download_all_pdfs"
                        class="btn btn-danger w-100">

                    <i class="fa fa-download me-2"></i>

                    Download & Index All PDFs

                    <span class="badge bg-light text-danger ms-2">
                        ${pdfLinks.length}
                    </span>

                </button>

            </div>
        `;
    }


    // ==========================================
    // Show SweetAlert Modal
    // ==========================================

    Swal.fire({

        title: `
            <i class="fa fa-file-pdf text-danger me-2"></i>
            Scraped PDFs
        `,

        html: html,

        width: 950,

        showConfirmButton: true,

        confirmButtonText: "Close",

        confirmButtonColor: "#6c757d",

        allowOutsideClick: true,

        didOpen: function () {

            // ==========================================
            // Initialize DataTable
            // ==========================================

            if ($("#tblPdfList").length > 0) {

                $("#tblPdfList").DataTable({

                    pageLength: 10,

                    lengthMenu: [
                        [5, 10, 25, 50, -1],
                        [5, 10, 25, 50, "All"]
                    ],

                    paging: true,

                    searching: true,

                    ordering: true,

                    info: true,

                    responsive: true,

                    autoWidth: false,

                    order: [],

                    columnDefs: [

                        {
                            targets: 0,
                            orderable: false,
                            searchable: false,
                            width: "60px"
                        },

                        {
                            targets: 2,
                            orderable: false,
                            searchable: false,
                            width: "100px"
                        }

                    ],

                    language: {

                        search: "",

                        searchPlaceholder: "Search PDF / URL...",

                        lengthMenu: "Show _MENU_ records",

                        info: "Showing _START_ to _END_ of _TOTAL_ PDFs",

                        infoEmpty: "No PDFs available",

                        zeroRecords: "No matching PDF found",

                        paginate: {
                            previous: "Previous",
                            next: "Next"
                        }

                    }

                });
            }


            // ==========================================
            // Download All PDFs
            // ==========================================

            $("#btn_download_all_pdfs")
                .off("click")
                .on("click", function () {

                    DownloadAllPdfs(response.job_id);

                });

        },

        willClose: function () {

            // ==========================================
            // Destroy DataTable
            // ==========================================

            if (
                $("#tblPdfList").length > 0 &&
                $.fn.DataTable.isDataTable("#tblPdfList")
            ) {

                $("#tblPdfList").DataTable().destroy();

            }

        }

    });
}

// ==========================================
// HTML Encode
// ==========================================

function escapeHtml(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}


// ==========================================
// HTML Attribute Encode
// ==========================================

function escapeAttribute(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
}

 
function DownloadAllPdfs(jobId) {

    if (!jobId) {

        Swal.fire({
            icon: "warning",
            title: "Job ID not available"
        });

        return;
    }



    $.ajax({

        url: "https://192.168.10.208/fastapi-scrapy-crawler/api/v1/pdf/download-by-job",

        type: "POST",

        contentType: "application/json",

        dataType: "json",

        headers: {
            "Accept": "application/json"
        },

        data: JSON.stringify({

            job_id: jobId,

            verify_ssl: false,

            max_files: 1000,

            overwrite_existing: false,

            max_pdf_size_mb: 1,

            replace_if_size_changed: true

        }),

        success: function (response) {

            console.log(
                "PDF Download Response:",
                response
            );

            Swal.fire({

                icon:
                    response.failed > 0
                        ? "warning"
                        : "success",

                title: "Download completed successfully. Indexing has started and the changes will be reflected after some time.",

                html: `
    <div class="text-start" >

                        <div>
                            <strong>Total PDFs:</strong>
                            ${response.total_pdf_links_found ?? 0}
                        </div>

                        <div>
                            <strong>Downloaded:</strong>
                            ${response.downloaded ?? 0}
                        </div>

                        <div>
                            <strong>Replaced:</strong>
                            ${response.replaced_existing ?? 0}
                        </div>

                        <div>
                            <strong>Skipped:</strong>
                            ${response.skipped_existing ?? 0}
                        </div>

                        <div>
                            <strong>Invalid PDF:</strong>
                            ${response.invalid_pdf ?? 0}
                        </div>

                        <div>
                            <strong>Failed:</strong>
                            ${response.failed ?? 0}
                        </div>

                        <div>
                            <strong>Oversized:</strong>
                            ${response.oversized ?? 0}
                        </div>

                    </div >
    `,

                confirmButtonText: "OK"
            });
        },

        error: function (xhr) {

            console.error(
                "PDF Download API Error:",
                xhr.status,
                xhr.responseText
            );

            Swal.fire({

                icon: "error",

                title: "PDF Download Failed",

                text:
                    xhr.responseText ||
                    "Unable to download PDF files."

            });
        }
    });
}

function reset()
{
   
 
    // Reset variables
    currentCrawlJobId = null;
    $("#popup_loading").addClass("d-none");
    // Reset URL
    // $("#Url").val("");
    $("#btn_view_pdfs").addClass("d-none");
    // Reset summary
    $("#websitewebUrl").text("-");
    $("#job_id").text("-");
    $("#crawl_status")
        .removeClass()
        .addClass("badge bg-secondary fs-5 px-4 py-2")
        .text("Waiting");

    $("#alias").text("-");
    $("#index_name").text("-");
    $("#deleted_existing_docs").text("0");
    $("#scraping_message").text("-");

    // Hide PDF button
    $("#btn_view_pdfs").hide();
    $("#popup_loading").html(` <i class="fa fa-spinner fa-spin me-2"></i>
                                           Checking final status......`)
    // Clear table if still present
    $("#webTable tbody").empty();

    // Hide loading
    $("#loading").hide();




}

// ==========================================
// HTML Encode
// ==========================================

function escapeHtml(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}


// ==========================================
// HTML Attribute Encode
// ==========================================

function escapeAttribute(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;");
}

