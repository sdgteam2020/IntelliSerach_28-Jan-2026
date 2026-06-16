// devjs/indexesdetails.load.js
// This file fetches the data and renders table with assign buttons

$(function () {
    function renderTable(data) {
        // helper to read values that may be serialized with dots in keys (e.g. "docs.count")
        function getVal(obj, /* names in priority */) {
            for (var i = 1; i < arguments.length; i++) {
                var name = arguments[i];
                if (!name) continue;
                // direct property
                if (obj.hasOwnProperty(name) && obj[name] !== undefined && obj[name] !== null) return obj[name];
                // bracketed dotted key
                if (obj[name] !== undefined && obj[name] !== null) return obj[name];
                // nested path a.b.c
                if (name.indexOf('.') !== -1) {
                    var parts = name.split('.');
                    var cur = obj;
                    var ok = true;
                    for (var p = 0; p < parts.length; p++) {
                        if (cur == null) { ok = false; break; }
                        cur = cur[parts[p]];
                    }
                    if (ok && cur !== undefined && cur !== null) return cur;
                }
            }
            return '';
        }
        var html = [];
        html.push('<table class="table table-hover mb-0 align-middle" id="tblData">');
        html.push('<thead class="table-light"><tr>');
        html.push('<th>Health</th>');
        html.push('<th>Index</th>');
        html.push('<th>Docs Count</th>');
        html.push('<th>Docs Deleted</th>');
        html.push('<th>Store Size</th>');
     
      

      
        html.push('<th>Actions</th>');
        html.push('</tr></thead>');
        html.push('<tbody>');

        if (Array.isArray(data) && data.length) {
            data.forEach(function (item) {
                html.push('<tr>');
                html.push('<td><span class="health-' + (item.health || '').toLowerCase() + '">' + (item.health || '').toUpperCase() + '</span></td>');
                html.push('<td>' + ((item.index || '').replace('seo_', '')) + '</td>');
                html.push('<td>' + (getVal(item, 'DocsCount', 'docs.count') || '') + '</td>');
                html.push('<td>' + (getVal(item, 'DocsDeleted', 'docs.deleted') || '') + '</td>');
                html.push('<td>' + (getVal(item, 'StoreSize', 'store.size') || '') + '</td>');
             
              
               
               
                html.push('<td><button class="btn btn-sm btn-primary btn-assign-index" data-index="' + (item.uuid || '') + '">Assign</button></td>');
                html.push('</tr>');
            });
        } else {
            html.push('<tr><td colspan="6" class="text-center text-danger">No Index Data Found</td></tr>');
        }

        html.push('</tbody></table>');

        $('#tableContainer').html(html.join(''));

        // initialize datatables if required
        if ($.fn.DataTable) {
            if ($.fn.dataTable.isDataTable('#tblData')) {
                $('#tblData').DataTable().destroy();
            }
            $('#tblData').DataTable({
                "order": [],
                "paging": true,
                "searching": true,
                "info": true,
                "autoWidth": false,
                "responsive": true,
                "select": true,
                pageLength: 25
            });
        }
    }

    function load() {
        var token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: '/Index/GetIndexesDetails',
            method: 'POST',
            headers: { 'RequestVerificationToken': token },
            success: function (resp) {
                if (resp && resp.success) {
                    renderTable(resp.data);
                } else {
                    $('#tableContainer').html('<div class="text-danger">Failed to load data.</div>');
                }
            },
            error: function () {
                $('#tableContainer').html('<div class="text-danger">Error while loading data.</div>');
            }
        });
    }

    load();
});
