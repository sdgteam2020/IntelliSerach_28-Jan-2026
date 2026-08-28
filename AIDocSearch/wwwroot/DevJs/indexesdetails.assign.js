// devjs/indexesdetails.assign.js
// Handles assign button, modal, user search and assign operation
let UserList = [];
let selectedUsers = new Set();
let uncheckeduser = new Set();

$(function () {
   

    function openAssignModal(uuid) {
        selectedUsers.clear(); // ✅
        uncheckeduser.clear(); // ✅
        let modalEl = document.getElementById('assignModal');
        let bsModal = new bootstrap.Modal(modalEl, {});
       
        let $modal = $(modalEl);
        resetUserList()
        // clear previous results and search box
        $modal.find('#userSearch').val('');
        $modal.find('#assignUserTable tbody').empty();

        // show modal (Bootstrap 5)
        bsModal.show();

        let searchTimeout;
        function doSearch(q) {

            let token = $('input[name="__RequestVerificationToken"]').val();
            $("#assignSelectAll").prop('checked', false);
            if ($.fn.DataTable.isDataTable('#assignUserTable')) {
                $('#assignUserTable').DataTable().columns.adjust().responsive.recalc();
                $('#assignUserTable tbody').empty();
            }

            $('#assignUserTable').DataTable({
                processing: true,
                serverSide: true,
                destroy: true,
                paging: true,
                ordering: true,
                searching: false,
                responsive: true,
                pageLength: 10,

                ajax: {
                    url: '/IntelliSearch/Index/SearchUsers',
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': token
                    },

                    data: function (d) {

                        let sortColumn = "DomainId";
                        let sortDirection = "asc";

                        if (d.order && d.order.length > 0) {
                            sortColumn = d.columns[d.order[0].column].data;
                            sortDirection = d.order[0].dir;
                        }

                        return {
                            uuid: uuid,

                            Draw: d.draw,
                            Start: d.start,
                            Length: d.length,

                            searchValue: q,
                            sortColumn: sortColumn,
                            sortDirection: sortDirection
                        };
                    }

                  
                },

                columns: [
                    {
                        data: 'Id',
                        orderable: false,
                        searchable: false,
                        render: function (data, type, row) {

                            if (row.IndexId) {
                                selectedUsers.add(row.Id);
                            }
                            if ($("#assignSelectAll").is(':checked')) {
                                return '<input type="checkbox" class="assignUserChk" data-id="' +
                                    row.Id + '"' +
                                    
                                    ' checked>';
                            }
                           

                            if (uncheckeduser.has(row.Id)) {
                                return '<input type="checkbox" class="assignUserChk" data-id="' +
                                    row.Id + '">';
                            }
                            else
                            return '<input type="checkbox" class="assignUserChk" data-id="' +
                                row.Id + '"' +
                                (row.IndexId ? ' checked' : '') +
                                '>';

                        }
                    },
                    {
                        data: 'DomainId',
                        title: 'User Name'
                    },
                    {
                        data: 'Name',
                        title: 'Name',
                        render: function (data, type, row) {
                            return (row.RankName || '') + ' ' + (row.Name || '');
                        }
                    }
                ],

                order: [[1, 'asc']]
            });
        }

        // immediate load of users (first page)

        doSearch('');

        // namespace events to avoid duplicate bindings
        $modal.off('.assign').on('input.assign', '#userSearch', function () {
            let q = $(this).val();
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () { doSearch(q); }, 300);
        });
        $(document).on('change', '.assignUserChk', function () {
            
            let userId = $(this).data('id');
            let userName = $(this).data('username');

            if ($(this).is(':checked')) {

                selectedUsers.add(userId);
                uncheckeduser.delete(userId);

            } else {

                selectedUsers.delete(userId);
                uncheckeduser.add(userId);
                $("#assignSelectAll").prop('checked', false);
            }

            
        });
        // select all checkbox handler (toggle)
        $modal.off('change.assign').on('change.assign', '#assignSelectAll', function () {
            let checked = $(this).is(':checked');
           
            $modal.find('.assignUserChk').prop('checked', checked);
        });

        $modal.off('click.assign').on('click.assign', '#assignSave', function () {
            const params = {
                IndexId: uuid,
                UserIds: Array.from(selectedUsers),
                AllSelected: $("#assignSelectAll").is(':checked')
            };

            $.ajax({
                url: '/IntelliSearch/Index/AssignIndex',
                method: 'POST',
                contentType: 'application/json',
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                data: JSON.stringify({
                    Data: encryptPayloadData(JSON.stringify(params))
                }),
                success: function (resp) {
                    if (resp && resp.Code==200) {
                        Swal.fire({
                            position: "top-end",
                            icon: "success",
                            title: resp.Message || "Assigned successfully.",
                            showConfirmButton: false,
                            timer: 1500
                        }); 
                        bsModal.hide();
                    } else {
                        Swal.fire({
                            position: "top-end",
                            icon: "error",
                            title: resp.Message || "Failed to assign.",
                            showConfirmButton: false,
                            timer: 1500
                        })
                    }
                },
                error: function () {
                    alert('Error while assigning');
                }
            });
        });
    }

    // wire assign button click handler on table
    $(document).on('click', '.btn-assign-index', function () {
        let uuid = $(this).data('index');
        let indexName = $(this).data('index-name');

        $(".Indexnameinmodal").html(indexName);
        openAssignModal(uuid);
        loadAssignedUsers(uuid);
        resetUserList()

    });

    $(document)
        .off('keyup', '#fileSearch')
        .on('keyup', '#fileSearch', function () {

            const search = $(this)
                .val()
                .toLowerCase()
                .trim();

            let visibleCount = 0;

            $('.file-item').each(function () {

                const fileName =
                    ($(this).data('filename') || '')
                        .toString();

                const match =
                    fileName.includes(search);

                $(this).toggle(match);

                if (match)
                    visibleCount++;
            });

            $('#fileCount').text(visibleCount);
        });

    $(document).on('click', '.btn-fileview-index', function () {

        let indexName = $(this).data('index');
        let uuid = $(this).data('uuid');
        
        $(".Indexnameinmodal").html($(this).data('index-name'));

        $.ajax({
            url: '/IntelliSearch/Index/GetDocDetailsByIndexName',
            type: 'POST',
            data: {
                indexName: encryptPayloadData(indexName) ,  //encryptPayloadData
                    uuid: encryptPayloadData(uuid)   //encryptPayloadData
            },
            headers: {
                'RequestVerificationToken':
                    $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {

                if (!response.success) {
                    Swal.fire({
                        position: "top-end",
                        icon: "error",
                        title: "Indexing Not Found",
                        showConfirmButton: false,
                        timer: 1500
                    })
                    return;
                }

                let html = '';

                if (response.data && response.data.length > 0) {

                    const totalFiles = response.data.length;

                    html += `
<div class="row mb-3 align-items-center">

    <div class="col-md-6">
        <h6 class="mb-0">
            <i class="fa-solid fa-folder-open text-primary"></i>
            Total Files:
            <span class="badge bg-primary" id="fileCount">
                ${totalFiles}
            </span>
        </h6>
    </div>

    <div class="col-md-6">
        <div class="input-group">
            <span class="input-group-text">
                <i class="fa-solid fa-magnifying-glass"></i>
            </span>
            <input type="text"
                   id="fileSearch"
                   class="form-control"
                   placeholder="Search file name..." />
        </div>
    </div>

</div>

<div class="row g-3" id="fileCardsContainer">
`;

                    response.data.forEach(function (item) {

                        let extension = "";
                        let path = "";
                        let fileName =""
                        if (item.Path.Real != null) {
                            path = decryptPayloadData(item.Path?.Real) || '';
                            fileName = path.split('\\').pop().split('/').pop();

                            extension = fileName.includes('.')
                                ? fileName.split('.').pop().toLowerCase()
                                : '';
                        } if (item.url != null) {
                            extension = "web";
                            path = item.url;
                            fileName = item.url;
                        }

                        let icon = 'fa-solid fa-file';
                        let iconClass = 'text-secondary';
                        
                        switch (extension) {
                            case 'web':
                                icon = 'fa fa-globe me-2';
                                iconClass = 'text-danger';
                                break;
                            case 'pdf':
                                icon = 'fa-solid fa-file-pdf';
                                iconClass = 'text-danger';
                                break;

                            case 'doc':
                            case 'docx':
                                icon = 'fa-solid fa-file-word';
                                iconClass = 'text-primary';
                                break;

                            case 'xls':
                            case 'xlsx':
                                icon = 'fa-solid fa-file-excel';
                                iconClass = 'text-success';
                                break;

                            case 'ppt':
                            case 'pptx':
                                icon = 'fa-solid fa-file-powerpoint';
                                iconClass = 'text-warning';
                                break;

                            case 'txt':
                                icon = 'fa-solid fa-file-lines';
                                iconClass = 'text-dark';
                                break;

                            case 'jpg':
                            case 'jpeg':
                            case 'png':
                            case 'gif':
                            case 'bmp':
                            case 'webp':
                                icon = 'fa-solid fa-file-image';
                                iconClass = 'text-info';
                                break;

                            case 'zip':
                            case 'rar':
                            case '7z':
                                icon = 'fa-solid fa-file-zipper';
                                iconClass = 'text-warning';
                                break;
                        }
                        let downloadUrl = "";/* `###S###192.168.10.207/pdf/${encodeURIComponent(fileName)}`; */
                        const normalizedPath = path.replace(/\\/g, "/");

                        const match = webServerList.find(item =>
                            normalizedPath.includes(item.Includes)
                            
                        );
                        if (match) {
                            const marker = match.Includes.replace("/", "").replace(/\\/g, "/");
                            const index = path.indexOf(marker);

                            const result = index >= 0
                                ? path.substring(index + marker.length + 1)
                                : path;

                            //fileName = path.split('\\').pop().split('/').pop();
                            downloadUrl = match.Url + "/" + result;

                        }
                        const isPdf = extension === 'pdf';

                        let fileUrl;
                        if (extension == "web") {
                            fileUrl = path;
                        }
                        else if (isPdf) {
                            const encrypted = encryptPayloadData(downloadUrl);
                            fileUrl = `/IntelliSearch/Master/WatermarkPdfFromUrl?pdfUrl=${encodeURIComponent(encrypted)}`;
                        }
                        else {
                            // fileUrl = downloadUrl;
                            const encrypted = encryptPayloadData(downloadUrl);
                            fileUrl = `/IntelliSearch/Master/WatermarkPdfFromUrl?pdfUrl=${encodeURIComponent(encrypted)}`;
                        }
                        let Icon_for_Download_Url = "";
                        if (extension=="web")
                            Icon_for_Download_Url = "fa fa-external-link";
                        else if (isPdf)
                            Icon_for_Download_Url = "fa-solid fa-eye";
                        else
                            Icon_for_Download_Url = "fa-solid fa-download";

                       
                        html += `
            <div class="col-md-6 col-lg-4 file-item"
     data-filename="${fileName.toLowerCase()}">
                <div class="card file-card h-100 border-0">

                    <div class="card-body d-flex align-items-start">

                        <div class="file-icon me-3">
                            <i class="${icon} ${iconClass} fa-2x"></i>
                        </div>

                        <div class="flex-grow-1">

                      <h6 class="mb-1 file-name">
    <a href="${fileUrl}"
       target="_blank"
       class="text-decoration-none"
       title="${fileName}">

        <i class="${Icon_for_Download_Url} me-1"></i>
       ${fileName.length > 20 ? fileName.substring(0, 20) + '...' : fileName}
    </a>
   
</h6>

                            <small class="text-muted">
                                ${extension.toUpperCase()}
                            </small>

                          

                        </div>

                    </div>

                </div>
            </div>
        `;
                    });

                    html += '</div>';
                }
                else {
                    html = '<div class="alert alert-info">No files found.</div>';
                }

                $('#fileList').html(html);

                let modal = new bootstrap.Modal(
                    document.getElementById('fileModal')
                );

                modal.show();
            },
            error: function () {
                alert('Error loading files.');
            }
        });
    });

    function resetUserList() {
        $('.DispalyUserList').html("")
        UserList=[]
    }
    function loadAssignedUsers(IndexId) {

        let token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/IntelliSearch/Index/GetIndexWiseAssginUsers',
            type: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            data: {
                IndexId: IndexId
            },
            success: function (resp) {

                resetUserList();

                if (resp.success && resp.data) {

                    let users = resp.data.split(',');

                    users.forEach(function (user) {

                        UserList.push(
                            '<span class="user-tag">' +
                            '<i class="fa-solid fa-user me-1"></i>' +
                            user.trim() +
                            '</span>'
                        );
                    });

                    $('.DispalyUserList')
                        .html(UserList.join(''))
                        .removeClass('d-none');
                }
                else {

                    $('.DispalyUserList')
                        .empty()
                        .addClass('d-none');
                }
            }
        });
    }
});
