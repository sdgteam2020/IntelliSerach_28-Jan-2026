// devjs/indexesdetails.load.js
// This file fetches the data and renders table with assign buttons
let webServerList = [];
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
<div class="row mb-3">
    <div class="col-md-4 ms-auto">
        <div class="input-group">
            <span class="input-group-text">
               <i class="fa-solid fa-magnifying-glass"></i>
            </span>
            <input type="text"
                   id="indexSearch"
                   class="form-control"
                   placeholder="Search Index..." />
        </div>
    </div>
</div>


`);
        html.push(`
<div class="row mb-4">

    <!-- CRAWLER INSTALLED -->
    <div class="col-12">
        <div class="card border-0 mb-4">
            <div class="card-header">
                <strong>
                    <i class="fa-solid fa-server me-2"></i>
                    Crawler Installed
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

                    let spanHtml = `
                <span class="running text-secondary crawler-status-text"
                      data-crawler-id="${crawlerId}">
                </span>

                <span class="crawler-status-dot"
                      data-crawler-id="${crawlerId}">
                </span>
            `;

                    let htmlCrawlerStatistics = `
                <button
                    class="btn btn-outline-primary btn-sm btn-indexingstatistics-index text-nowrap"
                    data-index="${item.index || ''}"
                    data-uuid="${item.uuid || ''}"
                    data-indexingstatistics-id="${item.index || ''}"
                    data-index-name="${(item.index || '').replace('fs-', '')}">

                    <i class="fa-solid fa-chart-column"></i>
                    Crawler Stats
                </button>
            `;

                    html.push(`
                <div class="col-12 col-md-6 col-xl-3 index-item"
                     data-index-name="${((item.index || '').replace('fs-', '')).toLowerCase()}">

                    <div class="card index-card border-0">

                        <div class="card-header-custom">
                            <div>
                                <h5 class="mb-1">
                                    ${(item.index || '').replace('fs-', '')}
                                </h5>
                            </div>
                        </div>

                        <div class="crawler-status">
                            <div class="status-left">
                                ${spanHtml}
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

                                ${htmlCrawlerStatistics}

                                ${type === "Admin" ? `
                                    <button
                                        class="btn btn-outline-primary btn-sm btn-assign-index text-nowrap"
                                        data-index="${item.uuid || ''}"
                                        data-index-name="${(item.index || '').replace('fs-', '')}">

                                        <i class="fas fa-user-plus"></i>
                                        Assign
                                    </button>
                                ` : ''}

                            </div>

                        </div>

                    </div>
                </div>
            `);
                });
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
                    Website Scraper Only
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

                    html.push(`
                <div class="col-12 col-md-6 col-xl-3 index-item"
                     data-index-name="${((item.index || '').replace('fs-', '')).toLowerCase()}">

                    <div class="card index-card border-0">

                        <div class="card-header-custom">
                            <div>
                                <h5 class="mb-1">
                                    ${(item.index || '').replace('fs-', '')}
                                </h5>
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
                                        class="btn btn-outline-primary btn-sm btn-assign-index text-nowrap"
                                        data-index="${item.uuid || ''}"
                                        data-index-name="${(item.index || '').replace('fs-', '')}">

                                        <i class="fas fa-user-plus"></i>
                                        Assign
                                    </button>
                                ` : ''}

                            </div>

                        </div>

                    </div>
                </div>
            `);
                });
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
        let token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: '/IntelliSearch/Index/GetIndexesDetails',
            method: 'POST',
            headers: { 'RequestVerificationToken': token },
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
        const token = $('input[name="__RequestVerificationToken"]').val();
        try {
            const response = await fetch('/IntelliSearch/Master/GetAllWebServer', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    "RequestVerificationToken": token   // Pass the CSRF token in the header
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
    GetWebServerUrlMapping();
    GetAllCrawlerStatus();
});
function refreshAllCrawlerStatus() {

    $('.crawler-status-text').each(function () {

        let $statusText = $(this);

        let crawlerId =
            $statusText.attr('data-crawler-id');

        if (!crawlerId)
            return;

        $.ajax({
            url: '/IntelliSearch/Files/GetCrawlerStatus',
            type: 'GET',

            data: {
                crawlerId: crawlerId
            },

            success: function (response) {

                updateCrawlerStatus(
                    crawlerId,
                    response.connected
                );
            },

            error: function () {

                updateCrawlerStatus(
                    crawlerId,
                    false
                );
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


    if (connected) {

        $text
            .text('Crawler Connected')
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

    }
    else {
        //console.log($indexingstatistics)
       // $indexingstatistics.addClass("d-none");
        $text
            .text('Crawler Not Connected')
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
    }
    // Automatically sort:
    // Live first -> Offline second
    sortCrawlerCards();
}
let crawlerStatusTimer = null;

function startCrawlerStatusMonitoring() {

    if (crawlerStatusTimer !== null)
        return;

    crawlerStatusTimer = setInterval(function () {
        $("#loading").hide();
        refreshAllCrawlerStatus();
        $("#loading").hide();
    }, 5000);
}
function sortCrawlerCards() {

    let $container = $('#crawlerInstalledContainer');

    let $cards = $container.children('.index-item');

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