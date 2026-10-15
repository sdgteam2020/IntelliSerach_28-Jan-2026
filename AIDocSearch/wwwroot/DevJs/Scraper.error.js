
let loadingErrors = false;
let crawlErrorRecords = [];
let errorCurrentPage = 1;

$(document).ready(function () {


    let statusPollingTimer = null;

    $("#popup_errors")
        .off("click.crawlErrors")
        .on("click.crawlErrors", function (e) {
            e.preventDefault();
            ShowError();
        });

    $("#closeCrawlErrors")
        .off("click.crawlErrors")
        .on("click.crawlErrors", function () {
            hideErrorsModal();
        });

    $("#errorSearch")
        .off("input.crawlErrors")
        .on("input.crawlErrors", function () {
            errorCurrentPage = 1;
            renderErrorRecords();
        });

    $("#errorPageSize")
        .off("change.crawlErrors")
        .on("change.crawlErrors", function () {
            errorCurrentPage = 1;
            renderErrorRecords();
        });

    $("#errorPagination")
        .off("click.crawlErrors", "button")
        .on("click.crawlErrors", "button", function () {
            errorCurrentPage = Number($(this).attr("data-page"));
            renderErrorRecords();
        });

    $("#popup_errors").on("click", function (e) {
        e.preventDefault();
       
        ShowError();
    });

    $("#closeCrawlErrors").on("click", function (e) {
        e.preventDefault();
        hideErrorsModal();
    });
    
});


// API call
async function ShowError() {
   
    if (loadingErrors) {
        return;
    }

    const errorsApi =
        "https://192.168.10.207/fastapi-scrapy-crawler/api/v1/scrapy-crawler/crawl/errors/";

    const jobId = $("#job_id").text().trim();
    if (jobId=="-") {
        return;
    }
    crawlErrorRecords = [];
    errorCurrentPage = 1;

    $("#errorSearch").val("");
    $("#errorJobId").text(jobId);
    $("#errorRecordCount").text("0");
    $("#crawlErrorsBody, #errorPagination").empty();
    $("#errorPageInfo").text("");

    $("#errorStatus")
        .removeClass("text-danger text-success")
        .text("");

    showErrorsModal();

    if (!jobId || jobId === "-") {
        $("#errorStatus")
            .addClass("text-danger")
            .text("No crawl job is available. Start scraping first.");
        return;
    }

    loadingErrors = true;
    $("#popup_errors").attr("aria-busy", "true");
    $("#errorSearch, #errorPageSize").prop("disabled", true);
    $("#errorStatus").text("Loading error records...");

    try {
        const data = await $.ajax({
            url: errorsApi + encodeURIComponent(jobId),
            type: "GET",
            dataType: "json",
            timeout: 30000,
            headers: {
                Accept: "application/json"
            }
        });

        if (!data || !Array.isArray(data.pages)) {
            throw new Error("The API returned an unexpected response.");
        }

        crawlErrorRecords = data.pages;
        $("#errorRecordCount").text(data.count ?? data.pages.length);

        renderErrorRecords();
    } catch (error) {
        let message = "Unable to load error records.";

        if (error.statusText === "timeout") {
            message = "The API request timed out. Please try again.";
        } else if (error.status === 0) {
            message =
                "Cannot connect to the API. Check network access, HTTPS certificate and CORS settings.";
        } else if (error.status) {
            message += " HTTP status: " + error.status + ".";
        } else if (error.message) {
            message = error.message;
        }

        $("#errorStatus")
            .addClass("text-danger")
            .text(message);
    } finally {
        loadingErrors = false;
        $("#popup_errors").removeAttr("aria-busy");
        $("#errorSearch, #errorPageSize").prop("disabled", false);
    }
}

// Formats details without inserting API content as HTML.
function formatErrorDetails(details) {
    if (details == null) {
        return "-";
    }

    return typeof details === "string"
        ? details
        : JSON.stringify(details, null, 2);
}

// Search, card display and pagination
function renderErrorRecords() {
    const search = String($("#errorSearch").val() || "")
        .trim()
        .toLowerCase();

    const pageSize = Number($("#errorPageSize").val()) || 10;

    // Keep the original issue number when filtering.
    const filtered = crawlErrorRecords
        .map(function (record, index) {
            return { record: record, issueNumber: index + 1 };
        })
        .filter(function (item) {
            const record = item.record;
            const searchable = [
                record.url ?? "",
                record.reason ?? "",
                formatErrorDetails(record.details)
            ].join(" ").toLowerCase();

            return searchable.includes(search);
        });

    const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));

    errorCurrentPage = Math.max(
        1,
        Math.min(errorCurrentPage, totalPages)
    );

    const start = (errorCurrentPage - 1) * pageSize;
    const visible = filtered.slice(start, start + pageSize);

    $("#crawlErrorsBody, #errorPagination").empty();
    $("#errorStatus")
        .removeClass("text-danger text-success")
        .text("");

    if (filtered.length === 0) {
        $("#errorStatus")
            .addClass("text-success")
            .text(
                crawlErrorRecords.length === 0
                    ? "No error records found for this job."
                    : "No errors match your search."
            );

        $("#errorPageInfo").text("Showing 0 records");
        return;
    }

    const fragment = document.createDocumentFragment();

    visible.forEach(function (item) {
        const record = item.record;

        const card = $("<article>", {
            class: "error-card"
        });

        const heading = $("<div>", {
            class: "error-card-heading"
        });

        heading.append(
            $("<h6>").text("Issue " + item.issueNumber),
            $("<span>", {
                class: "error-reason"
            }).text(record.reason ?? "Unknown reason")
        );

        const content = $("<div>", {
            class: "error-card-content"
        });

        function addField(label, value) {
            const field = $("<div>", { class: "error-field" });

            field.append(
                $("<strong>").text(label),
                $("<div>", { class: "error-url" }).text(value)
            );

            content.append(field);
        }

        addField("Problem URL", record.url ?? "-");

        const details = record.details;
        const foundOn =
            details && typeof details === "object"
                ? details.found_on
                : null;

        if (foundOn != null) {
            addField("Found On", String(foundOn));
        }

        content.append(
            $("<div>", { class: "error-field" }).append(
                $("<strong>").text("Details"),
                $("<pre>", {
                    class: "error-json"
                }).text(formatErrorDetails(details))
            )
        );

        card.append(heading, content);
        fragment.appendChild(card[0]);
    });

    document.getElementById("crawlErrorsBody")
        .appendChild(fragment);

    $("#errorPageInfo").text(
        "Showing " + (start + 1) + "–" +
        Math.min(start + pageSize, filtered.length) +
        " of " + filtered.length +
        (search ? " matching records" : " records")
    );

    function addPageButton(label, page, disabled, active) {
        const button = $("<button>", {
            type: "button",
            class: active
                ? "btn btn-primary btn-sm"
                : "btn btn-outline-secondary btn-sm"
        })
            .text(label)
            .attr("data-page", page)
            .prop("disabled", disabled);

        if (active) {
            button.attr("aria-current", "page");
        }

        $("#errorPagination").append(button);
    }

    addPageButton("First", 1, errorCurrentPage === 1, false);
    addPageButton(
        "Previous",
        errorCurrentPage - 1,
        errorCurrentPage === 1,
        false
    );

    // Show up to five page numbers.
    const firstPage = Math.max(
        1,
        Math.min(errorCurrentPage - 2, totalPages - 4)
    );

    const lastPage = Math.min(totalPages, firstPage + 4);

    for (let page = firstPage; page <= lastPage; page++) {
        addPageButton(
            String(page),
            page,
            false,
            page === errorCurrentPage
        );
    }

    addPageButton(
        "Next",
        errorCurrentPage + 1,
        errorCurrentPage === totalPages,
        false
    );

    addPageButton(
        "Last",
        totalPages,
        errorCurrentPage === totalPages,
        false
    );

    // Begin each page at the top of the modal body.
    $("#crawlErrorsModal .modal-body").scrollTop(0);
}

function showErrorsModal() {
    const element = document.getElementById("crawlErrorsModal");

    if (window.bootstrap && window.bootstrap.Modal) {
        let modal = window.bootstrap.Modal.getInstance(element);

        if (!modal) {
            modal = new window.bootstrap.Modal(element);
        }

        modal.show();
    } else {
        $(element).modal("show");
    }
}

function hideErrorsModal() {
    const element = document.getElementById("crawlErrorsModal");

    if (window.bootstrap && window.bootstrap.Modal) {
        const modal = window.bootstrap.Modal.getInstance(element);

        if (modal) {
            modal.hide();
        }
    } else {
        $(element).modal("hide");
    }
}