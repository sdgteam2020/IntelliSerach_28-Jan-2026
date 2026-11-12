// devjs/indexesdetails.load.js
// This file fetches the data and renders table with assign buttons
let webServerList = [];

$(document).on('click', '.btnIndexprivate', function () {

    let CrawlerId = $(this).data('crawler-id');
    let uuid = $(this).data('uuid');

    Swal.fire({
        title: 'Make Index Public?',
        text: 'Are you sure you want to make this index Public?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, make it Public!',
        cancelButtonText: 'Cancel'
    }).then((result) => {

        if (result.isConfirmed) {

            UpdateIndexPrivatePublic(CrawlerId, uuid, false);

        }

    });

});
$(document).on('click', '.btnIndexpublic', function () {

    let CrawlerId = $(this).data('crawler-id');
    let uuid = $(this).data('uuid');

    Swal.fire({
        title: 'Make Index Private?',
        text: 'Are you sure you want to make this index private?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, make it private!',
        cancelButtonText: 'Cancel'
    }).then((result) => {

        if (result.isConfirmed) {

            UpdateIndexPrivatePublic(CrawlerId, uuid, true);

        }

    });

});
$(function () {

    function renderTable(data, type) {

       
        // helper to read values that may be serialized with dots in keys (e.g. "docs.count")
        function getVal(obj, /* names in priority */) {
            for (let i = 1; i < arguments.length; i++) {
                let name = arguments[i];
                if (!name) continue;
                // direct property
                if (obj.hasOwnProperty(name) && obj[name] !== undefined && obj[name] !== null) return obj[name];
                // bracketed dotted key
                if (obj[name] !== undefined && obj[name] !== null) return obj[name];
                // nested path a.b.c
                if (name.indexOf('.') !== -1) {
                    let parts = name.split('.');
                    let cur = obj;
                    let ok = true;
                    for (let p = 0; p < parts.length; p++) {
                        if (cur == null) { ok = false; break; }
                        cur = cur[parts[p]];
                    }
                    if (ok && cur !== undefined && cur !== null) return cur;
                }
            }
            return '';
        }
        let html = [];

        html.push('<div class="row ">');
        html.push(`



`);
        html.push(`
<div class="row mb-4">

    <!-- CRAWLER INSTALLED -->
    <div class="col-12">
        <div class="card border-0 mb-4">
            <div class="card-header">
                <strong>
                    <i class="fa-solid fa-server me-2"></i>
                    Indexer Application
                </strong>
            </div>

            <div class="card-body">
                <div class="row" id="crawlerInstalledContainer">
`);
        if (Array.isArray(data) && data.length) {

            // ============================================================
            // FIRST BOX : CRAWLER INSTALLED
            // ============================================================

            data.filter(item => item.IsCrawlerInstall === true)
                .forEach(function (item) {

                    let crawlerId = item.index || '';
                    let uuid = item.uuid || '';

                    let spanHtml = `
                <span class="running text-secondary crawler-status-text"
                      data-crawler-id="${crawlerId}">
                </span>

                <span class="crawler-status-dot"
                      data-crawler-id="${crawlerId}">
                </span>

                 <button type="button"
                            class="btn btn-outline-secondary btn-sm confirmRestart d-none" data-crawler-id="${crawlerId}">
                            <i class="fas fa-sync-alt me-1"></i>

                        </button>

                 
            `;

                    let htmlCrawlerStatistics = `
                <button
                    class="btn btn-outline-primary btn-sm btn-indexingstatistics-index text-nowrap" title="Statistics of ${(item.index || '').replace('fs-', '')}"
                    data-index="${item.index || ''}"
                    data-uuid="${item.uuid || ''}"
                    data-indexingstatistics-id="${item.index || ''}"
                    data-index-name="${(item.index || '').replace('fs-', '')}">

                    <i class="fa-solid fa-chart-column"></i>
                    Stats
                </button>


               <button
                type="button"
                class="btn btn-outline-danger btn-sm btn-UploadFileTo-index d-none text-nowrap fut-upload-btn" title="Upload To ${(item.index || '').replace('fs-', '')}"

                data-crawler-id="${item.index || ''}"
                data-uuid="${item.uuid || ''}"
                data-indexingstatistics-id="${item.index || ''}"
                data-index-name="${(item.index || '').replace('fs-', '')}">

                <i class="fa-solid fa-plus"></i>
                
                </button>


            `;

                    html.push(`
                <div class="col-12 col-md-6 col-xl-2 index-item"
                     data-index-name="${((item.index || '').replace('fs-', '')).toLowerCase()}">

                    <div class="card index-card border-0">

                      <div class="card-header-custom">

    <div class="index-header-title">
        <span class="index-title-icon">
            <i class="fa-solid fa-server me-2"></i>
        </span>

        <div class="index-header-info">
            <h5 class="mb-0">
                ${(item.index || '').replace('fs-', '')}
            </h5>

            <div class="crawler-status">
                <div class="status-left">
                    ${spanHtml}
                </div>
            </div>
        </div>
    </div>

    <div class="index-header-actions">

        <button type="button"
                class="btn btn-outline-success btn-sm btnIndexpublic d-none"
                title="Public"
                data-crawler-id="${crawlerId}"
                data-uuid="${uuid}">
            <i class="fas fa-lock-open"></i>
        </button>

        <button type="button"
                class="btn btn-outline-danger btn-sm btnIndexprivate d-none"
                title="Private"
                data-crawler-id="${crawlerId}"
                data-uuid="${uuid}">
            <i class="fas fa-lock"></i>
        </button>

    </div>
</div>
                        
                       

                        <div class="card-body">

                            <div class="metrics-grid">

                                <div class="metric-item_Storage btn-fileview-index"
                                     role="button"
                                     data-index="${item.index || ''}"
                                     data-uuid="${item.uuid || ''}"
                                     data-index-name="${(item.index || '').replace('fs-', '')}">

                                    <div class="metric-value text-primary">
                                        ${getVal(item, 'DocsCount', 'docs.count') || 0}
                                    </div>

                                    <div class="metric-label">
                                        Active
                                    </div>
                                </div>

                                <div class="metric-item_Storage btn-fileview-index-delete"
                                     role="button"
                                     data-index="${item.index || ''}"
                                     data-uuid="${item.uuid || ''}"
                                     data-index-name="${(item.index || '').replace('fs-', '')}">
                                    <div class="metric-value text-danger">
                                        ${getVal(item, 'DocsDeleted', 'docs.deleted') || 0}
                                    </div>

                                    <div class="metric-label">
                                        Deleted
                                    </div>
                                </div>

                                <div class="metric-item_Storage">
                                    <div class="metric-value text-success">
                                        ${getVal(item, 'StoreSize', 'store.size') || '0'}
                                    </div>

                                    <div class="metric-label">
                                        Storage
                                    </div>
                                </div>

                            </div>

                            <div class="action-area">

                                ${htmlCrawlerStatistics}

                                ${type === "Admin" ? `
                                    <button
                                        class="btn btn-outline-primary btn-sm btn-assign-index text-nowrap" title="Users Assgin to ${(item.index || '').replace('fs-', '')}"
                                        data-index="${item.uuid || ''}"
                                        data-index-name="${(item.index || '').replace('fs-', '')}">

                                        <i class="fas fa-user-plus"></i>
                                        
                                    </button>
                                ` : ''}

                            </div>

                        </div>

                    </div>
                </div>
            `);
                });
        } else {
            html.push(`<div class="metric-value text-danger text-center">No Indexer  Found</div>`);
        }

        html.push(`
                </div>
            </div>
        </div>
    </div>


    <!-- WEBSITE SCRAPER ONLY -->
    <div class="col-12">
        <div class="card border-0">
            <div class="card-header">
                <strong>
                    <i class="fa-solid fa-globe me-2"></i>
                    WebSite Crawler
                </strong>
            </div>

            <div class="card-body">
                <div class="row" id="websiteScraperContainer">
`);

        if (Array.isArray(data) && data.length) {

            // ============================================================
            // SECOND BOX : WEBSITE SCRAPER ONLY
            // ============================================================

            data.filter(item => item.IsCrawlerInstall === false)
                .forEach(function (item) {
                    let crawlerId = item.index || '';
                    let uuid = item.uuid || '';
                    html.push(`

                    


                <div class="col-12 col-md-6 col-xl-2 index-item"
                     data-index-name="${((item.index || '').replace('fs-', '')).toLowerCase()}">

                    <div class="card index-card border-0">

                        <div class="card-header-custom">
                            <div class="index-header-title">
        <span class="index-title-icon">
            <i class="fa-solid fa-globe"></i>
        </span>

        <div class="index-header-info">
            <h5 class="mb-0">
                ${(item.index || '').replace('fs-', '')}
            </h5>

            
        </div>
    </div>
                            <button type="button"
        class="btn btn-outline-success btn-sm btnIndexpublic d-none"
        title="Public"
        data-crawler-id="${crawlerId}"
        data-uuid="${uuid}">
    <i class="fas fa-lock-open me-1"></i>
</button>

<button type="button"
        class="btn btn-outline-danger btn-sm btnIndexprivate d-none"
        title="Private"
        data-crawler-id="${crawlerId}"
        data-uuid="${uuid}">
    <i class="fas fa-lock me-1"></i>
</button>
                        </div>
                        
                        <div class="card-body">

                            <div class="metrics-grid">

                                <div class="metric-item_Storage btn-fileview-index"
                                     role="button"
                                     data-index="${item.index || ''}"
                                     data-uuid="${item.uuid || ''}"
                                     data-index-name="${(item.index || '').replace('fs-', '')}">

                                    <div class="metric-value text-primary">
                                        ${getVal(item, 'DocsCount', 'docs.count') || 0}
                                    </div>

                                    <div class="metric-label">
                                        Active
                                    </div>
                                </div>

                                <div class="metric-item_Storage">
                                    <div class="metric-value text-danger">
                                        ${getVal(item, 'DocsDeleted', 'docs.deleted') || 0}
                                    </div>

                                    <div class="metric-label">
                                        Deleted
                                    </div>
                                </div>

                                <div class="metric-item_Storage">
                                    <div class="metric-value text-success">
                                        ${getVal(item, 'StoreSize', 'store.size') || '0'}
                                    </div>

                                    <div class="metric-label">
                                        Storage
                                    </div>
                                </div>

                            </div>

                            <div class="action-area">

                                ${type === "Admin" ? `
                                    <button
                                        class="btn btn-outline-primary btn-sm btn-assign-index text-nowrap" title="Users Assgin to ${(item.index || '').replace('fs-', '')}"
                                        data-index="${item.uuid || ''}"
                                        data-index-name="${(item.index || '').replace('fs-', '')}">

                                        <i class="fas fa-user-plus"></i>
                                       
                                    </button>
                                ` : ''}

                            </div>

                        </div>

                    </div>
                </div>
            `);
                });
        } else {
            html.push(`<div class="metric-value text-danger text-center">No Website Found</div>`);
        }

        html.push(`
                </div>
            </div>
        </div>
    </div>

</div>
`);

        $('#tableContainer').html(html.join(''));


        ///use in realtime
        refreshAllCrawlerStatus();
        startCrawlerStatusMonitoring();
        GetAllPrivateIndexes();
        ////////

        $(document)
            .off('keyup', '#indexSearch')
            .on('keyup', '#indexSearch', function () {

                let searchText = $(this).val().toLowerCase().trim();
                
                $('.index-item').each(function () {

                    let indexName = ($(this).attr('data-index-name') || '').toLowerCase();
                    
                    $(this).toggle(indexName.includes(searchText));
                });
            });
    }

    function load() {
        let csrfToken = token();
        if (!csrfToken) {
            throw new Error("Anti-forgery token is missing.");
        }
        $.ajax({
            url: '/IntelliSearch/Index/GetIndexesDetails',
            method: 'POST',
            headers: { 'RequestVerificationToken': csrfToken },
            success: function (resp) {
                if (resp && resp.success) {

                   
                    renderTable(resp.data, resp.type);
                } else {
                    $('#tableContainer').html('<div class="text-danger">Failed to load data.</div>');
                }
            },
            error: function () {
                $('#tableContainer').html('<div class="text-danger">Error while loading data.</div>');
            }
        });
    }
    async function GetWebServerUrlMapping() {
        const userdata = new URLSearchParams({
            id: 0
        });
        const csrfToken = token();
        if (!csrfToken) {
            throw new Error("Anti-forgery token is missing.");
        }   
        try {
            const response = await fetch('/IntelliSearch/Master/GetAllWebServer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    "RequestVerificationToken": csrfToken   // Pass the CSRF token in the header
                },
                body: userdata
            });

            if (!response.ok) {
                const text = await response.text();
                Swal.fire({
                    position: "top-end",
                    icon: "error",
                    title: "Save failed2.\n" + text,
                    showConfirmButton: false,
                    timer: 1500
                });

                return;
            }
            const data = await response.json();
            if (data != null) {
                if (data && Array.isArray(data)) {
                    webServerList = data;   // ✅ Store globally
                }
            }
        } catch (error) {
            Swal.fire({
                text: errormsg002
            });
        }
    }
    load();
    //GetWebServerUrlMapping();
    GetAllCrawlerStatus();
});
function refreshAllCrawlerStatus() {

    $('.crawler-status-text').each(function () {

        let $statusText = $(this);

        let crawlerId =
            $statusText.attr('data-crawler-id');

        if (!crawlerId)
            return;
        const csrfToken = token();
        if (!csrfToken) {
            throw new Error("Anti-forgery token is missing.");
        }   
        $.ajax({
            url: '/IntelliSearch/Files/GetCrawlerStatusLive',
            type: 'POST',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            headers: {
                'RequestVerificationToken':csrfToken
            },
            data: JSON.stringify(crawlerId),
            success: function (response) {
                updateCrawlerStatus(crawlerId, response.connected);
            },
            error: function () {
                updateCrawlerStatus(crawlerId, false);
            }
        });
    });
}
function updateCrawlerStatus(
    crawlerId,
    connected
) {

    let $indexingstatistics = $(
        '.btn-indexingstatistics-index[data-indexingstatistics-id="' +
        crawlerId +
        '"]'
    );


    let $text = $(
        '.crawler-status-text[data-crawler-id="' +
        crawlerId +
        '"]'
    );

    let $dot = $(
        '.crawler-status-dot[data-crawler-id="' +
        crawlerId +
        '"]'
    );
      
    let $restart_indexer = $(
        '.confirmRestart[data-crawler-id="' +
        crawlerId +
        '"]'
    );
    let $btnUploadFileToindex = $(
        '.btn-UploadFileTo-index[data-crawler-id="' +
        crawlerId +
        '"]'
    );

    if (connected) {

        $text
            .text('Connected')
            .removeClass(
                'text-danger text-secondary'
            )
            .addClass(
                'text-success'
            );

        $dot
            .removeClass(
                'offline-dot'
            )
            .addClass(
                'live-dot'
        );
        $restart_indexer.removeClass('d-none')
        $btnUploadFileToindex.removeClass('d-none')
    }
    else {
        //console.log($indexingstatistics)
       // $indexingstatistics.addClass("d-none");
        $text
            .text('Not Connected')
            .removeClass(
                'text-success text-secondary'
            )
            .addClass(
                'text-danger'
            );

        $dot
            .removeClass(
                'live-dot'
            )
            .addClass(
                'offline-dot'
        );

        $restart_indexer.addClass('d-none')
        $btnUploadFileToindex.addClass('d-none')
    }
    // Automatically sort:
    // Live first -> Offline second
    sortCrawlerCards();
}
let crawlerStatusTimer = null;

function startCrawlerStatusMonitoring() {

    if (crawlerStatusTimer !== null)
        return;

    // crawlerStatusTimer = setInterval(function () {
    //     $("#loading").hide();
    //     refreshAllCrawlerStatus();
    //     $("#loading").hide();
    // }, 5000);
}
function sortCrawlerCards() {

    let $container = $('#crawlerInstalledContainer');

    let $cards = $container.children('.index-item').toArray();

    $cards.sort(function (a, b) {

        let aLive = $(a)
            .find('.crawler-status-dot')
            .hasClass('live-dot');

        let bLive = $(b)
            .find('.crawler-status-dot')
            .hasClass('live-dot');

        if (aLive && !bLive)
            return -1;

        if (!aLive && bLive)
            return 1;

        return 0;
    });

    $container.append($cards);
}
function UpdateIndexPrivatePublic(CrawlerId, uuid,IsPrivate) {

    const data = {
        CrawlerId: CrawlerId,
        uuid: uuid,
        IsPrivate: IsPrivate
    };

    const csrfToken = token();

    if (!csrfToken) {
        Swal.fire({
            icon: "error",
            title: "Error",
            text: "Anti-forgery token is missing."
        });
        return;
    }

    $("#loading").show();

    fetch('/IntelliSearch/Index/UpdateIndexPrivatePublic', {

        method: 'POST',

        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': csrfToken
        },

        body: JSON.stringify(data)

    })
        .then(async response => {

            const result = await response.json();

            if (!response.ok) {

                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: result.Message || result.message,
                    customClass: {
                        popup: "swal-high-z"
                    }
                });

                return;
            }

            Swal.fire({
                position: "top-end",
                icon: "success",
                title: result.Message || result.message,
                showConfirmButton: false,
                timer: 1500
            });
            GetAllPrivateIndexes();
        })
        .catch(error => {

            console.error(error);

            Swal.fire({
                icon: "error",
                title: "Error",
                text: "Something went wrong. Please try again."
            });

        })
        .finally(() => {

            $("#loading").hide();

        });

}
function GetAllPrivateIndexes() {

    $.ajax({
        url: '/IntelliSearch/Index/GetAllPrivateIndexes',
        type: 'POST',

        headers: {
            'RequestVerificationToken': token()
        },

        success: function (response) {

            let privateIndexes = response.Data ?? response.data ?? [];

            // Handle JSON string response
            if (typeof privateIndexes === "string") {
                privateIndexes = JSON.parse(privateIndexes);
            }

            // Handle nested response
            if (!Array.isArray(privateIndexes)) {
                privateIndexes = privateIndexes.Data
                    ?? privateIndexes.data
                    ?? [];
            }

            if (!Array.isArray(privateIndexes)) {
                console.error("Invalid private index response");
                return;
            }

            // Get private IndexId values
            let privateUUIDs = new Set(
                privateIndexes
                    .filter(x => x.IsPrivate === true || x.isPrivate === true)
                    .map(x => String(x.IndexId ?? x.indexId).trim())
            );

            console.log("Private UUIDs:", privateUUIDs);

            // Update Public / Private buttons
            $('.btnIndexpublic, .btnIndexprivate').each(function () {

                let uuid = String(
                    $(this).attr('data-uuid') ?? ''
                ).trim();

                let isPrivate = privateUUIDs.has(uuid);

                if ($(this).hasClass('btnIndexprivate')) {

                    // Show private button if index is private
                    $(this).toggleClass('d-none', !isPrivate);

                } else {

                    // Show public button if index is public
                    $(this).toggleClass('d-none', isPrivate);

                }

            });

        },

        error: function (xhr) {

            console.error(xhr);

            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Unable to retrieve index privacy status.'
            });

        }

    });
}