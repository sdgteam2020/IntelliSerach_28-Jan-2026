// devjs/indexesdetails.load.js
// This file fetches the data and renders table with assign buttons
var webServerList = [];
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

<div class="row" id="indexCardsContainer">
`);
        if (Array.isArray(data) && data.length) {

            data.forEach(function (item) {

                var health = (item.health || '').toLowerCase();
                var badgeClass = 'bg-success';

                if (health === 'yellow')
                    badgeClass = 'bg-warning text-dark';
                else if (health === 'red')
                    badgeClass = 'bg-danger';

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

            <span class="status-badge ${health}">
                ${(item.health || '').toUpperCase()}
            </span>
        </div>

        <div class="card-body">

            <div class="metrics-grid">

                <div class="metric-item_Storage">
                    <div class="metric-value text-primary">
                        ${getVal(item, 'DocsCount', 'docs.count') || 0}
                    </div>
                    <div class="metric-label">
                        Documents
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

                <button
                    class="btn btn-outline-info btn-sm btn-fileview-index"
                    data-index="${item.index || ''}"
                     data-index-name="${(item.index || '').replace('fs-', '')}"
                    >
                    👁 View Files
                </button>

                ${type === "Admin" ? `
               <button
                class="btn btn-outline-primary btn-sm btn-assign-index"
                data-index="${item.uuid || ''}"
                data-index-name="${(item.index || '').replace('fs-', '')}">
                <i class="fas fa-user-plus"></i>  Assign
            </button>
                ` : ''}

            </div>

        </div>

    </div>
</div>
`);
            });

        } else {

            html.push(`
        <div class="col-12">
            <div class="card shadow-sm border-0">
                <div class="card-body text-center text-muted py-5">
                    No indexes found.
                </div>
            </div>
        </div>
    `);
        }

        html.push('</div>');

        $('#tableContainer').html(html.join(''));
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
            url: '/Index/GetIndexesDetails',
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

});
