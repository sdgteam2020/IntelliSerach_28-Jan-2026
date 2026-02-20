$(document).ready(function () {
    $("#pdfOption").change(function () {
        if ($(this).is(":checked")) {
            $(".pdfviewcontainer").removeClass("d-none");
            $(".webviewcontainer").addClass("d-none");
            $("#pdfTable tbody").empty();
            if ($.fn.DataTable.isDataTable('#pdfTable')) {
                $('#pdfTable').DataTable().destroy();
            }
        }
        
    });
    $("#webOption").change(function () {
       
        if ($(this).is(":checked")) {
            $(".pdfviewcontainer").addClass("d-none");
            $(".webviewcontainer").removeClass("d-none");
            $("#webTable tbody").empty();

            if ($.fn.DataTable.isDataTable('#webTable')) {
                $('#webTable').DataTable().destroy();
            }
        }
    });

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
                            showConfirmButton: false,
                            timer: 1500
                        });
                        $("#loading").hide();
                        return;
                    }

                    // Parse the JSON response
                    const data = await response.json(); // Parse JSON response

                    // Example: { code: 200, message: "...", data: { ... } }
                    if (data.Code === 200) {
                        if ($("#pdfOption").is(":checked")) {
                            BindpdfScraping(data)
                            $(".pdfviewcontainer").removeClass("d-none");
                        }
                        else {
                            BindWebScraping(data)
                            $(".webviewcontainer").removeClass("d-none");
                        }

                        $("#loading").hide();
                    } else if (data.Code === 4) {
                        $("#loading").hide();
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: data.Message + "\n " + data.Data,
                            showConfirmButton: false,
                            timer: 3500
                        });
                    } else {
                        $("#loading").hide();
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: data.Message,
                            showConfirmButton: false,
                            timer: 3500
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
function BindpdfScraping(Data) {
    // Safety check
    if (!Data || Data.Code !== 200) {
        alert(Data?.message || "No data available");
        return;
    }

    var scraper = Data.Data.data;

    // ===== Bind Summary =====
    $("#websiteUrl").text(scraper.website_url);
    $("#downloadCount").text(scraper.downloaded_count);
    /*$("#downloadDir").text(scraper.download_directory);*/
    if (scraper.downloaded_count > 0) {
        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Scraping success",
            showConfirmButton: false,
            timer: 3500
        });
    } else {
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "The PDF file was not found during scraping",
            showConfirmButton: true,
        });
    }
    // ===== Clear Old Rows =====
    $("#pdfTable tbody").empty();

    // ===== Bind Table Rows =====
    if (scraper.documents && scraper.documents.length > 0) {
        $.each(scraper.documents, function (index, doc) {
            var row = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${doc.filename}</td>
                    <td>
                        <a href="${doc.source_page}" target="_blank">
                            <img src="/Images/website.png" class="icon-website" />
                        </a>
                    </td>
                    <td>${doc.size_kb}KB</td>
                    <td>${DateFormat(doc.downloaded_display)}</td>
                    <td>
                        <a href="${doc.url}"
                           class="btn"
                           target="_blank">

                           <i class="fa-solid fa-file-pdf fa-2xl text-warning"></i>
                        </a>
                    </td>
                </tr>
            `;

            $("#pdfTable tbody").append(row);
        });
    } else {
        $("#pdfTable tbody").append(`
            <tr>
                <td colspan="6" class="text-center text-muted">
                    No PDFs found
                </td>
            </tr>
        `);
    }
    $('#pdfTable').DataTable({
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
    $("#loading").hide();
}

function BindWebScraping(Data) {
    // Safety check
    if (!Data || Data.Code !== 200) {
        alert(Data?.message || "No data available");
        return;
    }

    var scraper = Data.Data.Data;

    // ===== Bind Summary =====
    $("#websitewebUrl").text($("#Url").val());
    $("#pages_crawled").text(Data.Data.PagesCrawled);
    /*$("#downloadDir").text(scraper.download_directory);*/
    if (Data.Data.PagesCrawled > 0) {
        Swal.fire({
            position: "top-end",
            icon: "success",
            title: "Scraping success",
            showConfirmButton: false,
            timer: 3500
        });
    } else {
        Swal.fire({
            position: "top-end",
            icon: "error",
            title: "The PDF file was not found during scraping",
            showConfirmButton: true,
        });
    }
    // ===== Clear Old Rows =====
    $("#webTable tbody").empty();

    // ===== Bind Table Rows =====
    if (scraper && scraper.length > 0) {
        $.each(scraper, function (index, doc) {
            var row = `
                <tr>
                    <td>${index + 1}</td>
                    <td>
                    <a href="${doc.CanonicalUrl}" target="_blank">
                            <img src="/Images/website.png" class="icon-website" />
                        </a>
                    </td>
                    <td>${doc.Title}</td>

                    <td>${doc.Summary}</td>

                </tr>
            `;

            $("#webTable tbody").append(row);
        });
    } else {
        $("#webTable tbody").append(`
            <tr>
                <td colspan="4" class="text-center text-muted">
                    No PDFs found
                </td>
            </tr>
        `);
    }
    $('#webTable').DataTable({
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

    $("#loading").hide();
}

function DateFormat(inputDate) {
    // Remove UTC and convert to Date
    var date = new Date(inputDate.replace(" UTC", ""));

    function pad(n) {
        return n < 10 ? '0' + n : n;
    }

    var formatted =
        pad(date.getDate()) + "/" +
        pad(date.getMonth() + 1) + "/" +
        date.getFullYear() + " " +
        pad(date.getHours()) + ":" +
        pad(date.getMinutes()) + ":" +
        pad(date.getSeconds());

    return formatted;
}