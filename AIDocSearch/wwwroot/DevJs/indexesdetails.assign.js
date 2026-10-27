// devjs/indexesdetails.assign.js
// Handles assign button, modal, user search and assign operation
let UserList = [];
let selectedUsers = new Set();
let uncheckeduser = new Set();

let fileCurrentPage = 1;
const fileRecordsPerPage = 10;

$(function () {

    function openAssignModal(uuid, indexName) {
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
                const table = $('#assignUserTable').DataTable();

                table.columns.adjust();

                // Recalculate only if the Responsive extension is available.
                if (table.responsive &&
                    typeof table.responsive.recalc === 'function') {
                    table.responsive.recalc();
                }

                table.clear().draw();
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
                indexName: "fs-"+indexName,
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
                    if (resp && resp.Code == 200) {
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
    // ========================================
    // CARD VIEW
    // ========================================

    $(document).on('click', '#cardViewBtn', function () {

        $('#tableViewContainer').addClass("d-none")
        $('#cardViewContainer').removeClass("d-none")

        $('#cardViewBtn')
            .removeClass('btn-outline-primary')
            .addClass('btn-primary active');

        $('#tableViewBtn')
            .removeClass('btn-primary active')
            .addClass('btn-outline-primary');
    });


    // ========================================
    // TABLE VIEW
    // ========================================

    $(document).on('click', '#tableViewBtn', function () {

        $('#tableViewContainer').removeClass("d-none")
        $('#cardViewContainer').addClass("d-none")

        $('#tableViewBtn')
            .removeClass('btn-outline-primary')
            .addClass('btn-primary active');

        $('#cardViewBtn')
            .removeClass('btn-primary active')
            .addClass('btn-outline-primary');
    });
    $(document).on('keyup', '#fileSearch', function () {

        const searchValue = $(this)
            .val()
            .toLowerCase()
            .trim();


        // Card View
        $('.file-item').each(function () {

            const fileName =
                $(this).data('filename') || '';

            $(this).toggle(
                fileName.includes(searchValue)
            );
        });


        // Table View
        $('.file-table-item').each(function () {

            const fileName =
                $(this).data('filename') || '';

            $(this).toggle(
                fileName.includes(searchValue)
            );
        });


        // Count matching files
        const visibleCount =
            $('.file-item:visible').length;

        $('#fileCount').text(visibleCount);
    });
    // wire assign button click handler on table
    $(document).on('click', '.btn-assign-index', function () {
        let uuid = $(this).data('index');
        let indexName = $(this).data('index-name');

        $(".Indexnameinmodal").html(indexName.replace('fs-', ''));
        openAssignModal(uuid, indexName);
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
    // ======================================================
    // OPEN indexing statistics MODAL
    // ======================================================

    let crawlerStatusInterval = null;
    $(document).on('click', '#refreshStatisticsbyIndexName', function () {
        let indexName = $(this).attr('data-index');
        let uuid = $(this).attr('data-uuid');
        GetstatisticsByIndexName(indexName, uuid);
    });
    $(document).on('click', '.btn-indexingstatistics-index', function () {

        ResetCrawlerDashboard();
        let indexName = $(this).data('index');
        let uuid = $(this).data('uuid');
        $(".spanindexNameForDashboard").html(indexName)
        $(".spanuuidForDashboard").html(uuid)
        $(".lbl_for_CrawlerDashboard").html("Indexer Statistics For " + indexName.replace('fs-', ''))
        // clear existing interval
        if (crawlerStatusInterval) {
            clearInterval(crawlerStatusInterval);
        }


        // call immediately
        GetCrawlerStatus(indexName);


        // call every 10 seconds
        // crawlerStatusInterval = setInterval(function () {

        //     GetCrawlerStatus(indexName);

        // }, 10000);


        $(".Indexnameinmodal").html(
            $(this).data('index-name')
        );
        let modalElement =
            document.getElementById(
                'indexingstatisticsModal'
            );


        let modal =
            bootstrap.Modal.getOrCreateInstance(
                modalElement
            );

        $('#refreshStatisticsbyIndexName').attr('data-index', indexName);
        $('#refreshStatisticsbyIndexName').attr('data-uuid', uuid);
        GetstatisticsByIndexName(indexName, uuid);

       

        modal.show();
    });

    // ======================================================
    // OPEN FILE LIST MODAL
    // ======================================================

    $(document).on('click', '#AllDocDetailsByIndexName', function () {
        const indexedSearchable =
            Number($("#indexedSearchable").text().replace(/,/g, "").trim());
        if (indexedSearchable > 0) {
            GetDocDetailsByIndexName($(".spanindexNameForDashboard").html(), $(".spanuuidForDashboard").html(), 3)

            $(".filesViewType").html("Indexed Searchable");
        }
     
    })

    $(document).on('click', '#OCRDocDetailsByIndexName', function () {

        const indexedOcr_Files =
            Number($("#indexedOcr_Files").text().replace(/,/g, "").trim());
        if (indexedOcr_Files > 0) {
            GetDocDetailsByIndexName($(".spanindexNameForDashboard").html(), $(".spanuuidForDashboard").html(), 1)
            $(".filesViewType").html("OCR Enabled");
        }

    })

    $(document).on('click', '#NotOCRDocDetailsByIndexName', function () {
        const indexedNotOcr_Files =
            Number($("#indexedNotOcr_Files").text().replace(/,/g, "").trim());

        if (indexedNotOcr_Files > 0) {
            GetDocDetailsByIndexName($(".spanindexNameForDashboard").html(), $(".spanuuidForDashboard").html(), 2)
            $(".filesViewType").html("Text Extraction");
        }

    })
    
    $(document).on('click', '.btn-fileview-index-delete', function () {
        let indexName = $(this).data('index');
        let uuid = $(this).data('uuid');

        $(".Indexnameinmodal").html(
            $(this).data('index-name')
        );
        $(".filesViewType").html("Deleted Indexes");
        GetDocDetailsByIndexName(indexName, uuid, 5)
    });
    $(document).on('click', '.btn-fileview-index', function () {

        let indexName = $(this).data('index');
        let uuid = $(this).data('uuid');

        $(".Indexnameinmodal").html(
            $(this).data('index-name')
        );

        GetDocDetailsByIndexName(indexName, uuid, 4)
        $(".filesViewType").html("Active Indexes");
       

    });
    function GetstatisticsByIndexName(indexName, uuid) {
        $.ajax({

            url: '/IntelliSearch/Index/GetstatisticsByIndexName',

            type: 'POST',

            data: {

                indexName:
                    encryptPayloadData(indexName),

                uuid:
                    encryptPayloadData(uuid)
            },

            headers: {

                'RequestVerificationToken':
                    $('input[name="__RequestVerificationToken"]').val()
            },

            success: function (response) {

                if (response != null &&
                    response.data.DTOStatisticsHits &&
                    response.data.DTOStatisticsHits.length > 0 &&
                    response.data.DTOStatisticsHits[0]._source) {
                    DTODocCountByIndexName = response.data.DTODocCountByIndexName;
                    DTOStatisticsHits = response.data.DTOStatisticsHits[0]._source;
                    const size = DTODocCountByIndexName.Size
                    const ScanTime = DTOStatisticsHits.scantime
                    const Total = DTODocCountByIndexName.Total
                    const allowedExtensionSet = DTOStatisticsHits.allowedExtensionSet;
                    let TotalFiles = DTOStatisticsHits.totalfile
                    const File_Extensions = DTODocCountByIndexName.File_Extensions;
                    // Percentage = (Total / TotalFiles) * 100

                    const Ocr_Files = DTODocCountByIndexName.Ocr_Files;
                    const Non_Ocr_Files = DTODocCountByIndexName.Non_Ocr_Files;

                    totalfilestoindexed = Ocr_Files + Non_Ocr_Files;


                    const Totalunsupportedfile = DTOStatisticsHits.unsupportedfile + DTODocCountByIndexName.Total_failed_files;
                    let underprocess = TotalFiles - (totalfilestoindexed + Totalunsupportedfile)
                    // if (underprocess < 0) {
                    //     TotalFiles = TotalFiles - underprocess;
                    //     underprocess = 0;
                    // }
                    let Percentage = 0;

                    if (TotalFiles > 0) {

                        Percentage = Math.round(
                            (((totalfilestoindexed + Totalunsupportedfile) / TotalFiles) * 100) * 10
                        ) / 10;

                    }
                    animateCounter(
                        "filesDiscovered",
                        TotalFiles
                    );


                    animateCounter(
                        "indexedSearchable",
                        totalfilestoindexed
                    );


                    animateCounter(
                        "indexedOcr_Files",
                        Ocr_Files
                    );


                    animateCounter(
                        "indexedNotOcr_Files",
                        Non_Ocr_Files
                    );


                    animateCounter(
                        "failedProcess",
                        Totalunsupportedfile
                    );


                    // Progress animation

                    animateCounter(
                        "indexunderProgress",
                        underprocess,
                        1000

                    );


                    animateCounter(
                        "progressPercent",
                        Percentage,
                        1000,
                        "%"
                    );
                    // Progress bar

                    animateProgressBar(
                        "progressBar",
                        Percentage,
                        1000
                    );


                    // Progress text

                    $("#progressText").text(
                        `${TotalFiles.toLocaleString()} of ${(totalfilestoindexed + Totalunsupportedfile).toLocaleString()} files are searchable`
                    );

                    showCrawleExtensions(
                        File_Extensions
                    );
                    showAllowedExtensions(
                        allowedExtensionSet
                    );

                    $("#lastSync").text("Last synchronized: " + timeAgo(ScanTime) + " (" + formatDateTime(ScanTime) + ")");

                }
                // $("#indexNameModalTitle").text(indexName);
            },


            error: function () {

                Swal.fire({

                    icon: "error",

                    title: "Error",

                    text: "Error loading files."
                });

            }

        });
    }
    function GetDocDetailsByIndexName(indexName, uuid,OcrType) {
        $.ajax({

            url: '/IntelliSearch/Index/GetDocDetailsByIndexName',

            type: 'POST',

            data: {

                indexName:
                    encryptPayloadData(indexName),

                uuid:
                    encryptPayloadData(uuid),
                OcrType: OcrType
            },

            headers: {

                'RequestVerificationToken':
                    $('input[name="__RequestVerificationToken"]').val()
            },

            success: function (response) {

                // ==========================================
                // API FAILED
                // ==========================================

                if (!response.success) {

                    Swal.fire({

                        position: "top-end",

                        icon: "error",

                        title: "Indexing Not Found",

                        showConfirmButton: false,

                        timer: 1500
                    });

                    return;
                }


                let html = '';


                // ==========================================
                // DATA FOUND
                // ==========================================

                if (
                    response.data &&
                    response.data.length > 0
                ) {

                    const totalFiles =
                        response.data.length;


                    // ==========================================
                    // HEADER
                    // ==========================================

                    html += `
                    <div class="row mb-3 align-items-center">

                        <div class="col-md-3 mb-2 mb-md-0">

                            <h6 class="mb-0">

                                <i class="fa-solid fa-folder-open text-primary me-1"></i>

                                Total Files:

                                <span class="badge bg-primary"
                                      id="fileCount">

                                    ${totalFiles}

                                </span>

                            </h6>

                        </div>


                        <div class="col-md-6 mb-2 mb-md-0">

                            <div class="input-group">

                                <span class="input-group-text">

                                    <i class="fa-solid fa-magnifying-glass"></i>

                                </span>


                                <input type="text"
                                       id="fileSearch"
                                       class="form-control"
                                       autocomplete="off"
                                       placeholder="Search file name..." />

                            </div>

                        </div>


                        <div class="col-md-3 text-md-end">

                            <div class="btn-group"
                                 role="group"
                                 aria-label="File View">

                                <button type="button"
                                        id="cardViewBtn"
                                        class="btn btn-primary active"
                                        title="Card View">

                                    <i class="fa-solid fa-table-cells-large"></i>

                                </button>


                                <button type="button"
                                        id="tableViewBtn"
                                        class="btn btn-outline-primary"
                                        title="Table View">

                                    <i class="fa-solid fa-table-list"></i>

                                </button>

                            </div>

                        </div>
                        
                    </div>
                `;


                    // ==================================================
                    // CARD VIEW
                    // ==================================================

                    html += `
                    <div id="cardViewContainer">

                        <div class="row g-3"
                             id="fileCardsContainer">
                `;


                    response.data.forEach(function (item) {

                        const fileInfo =
                            prepareFileInfo(
                                item,
                                indexName, OcrType
                            );
                        let Ocrused = item.crawler == null
                            ? `<span></span>`
                            : item.crawler.ocr_used === true
                                ? `<span class="ocr-badge ocr-enabled">OCR Enabled</span>`
                                : `<span class="ocr-badge text-extraction">Text Extraction</span>`;
                        let fileSize = ``;
                        if (fileInfo.fileSize == "0")
                            fileSize = `<span></span>`
                        else
                            fileSize = `<span class="file-size-badge"><i class="fa-solid fa-hard-drive me-1"></i>${fileInfo.fileSize}</span>`
                        html += `
                        <div class="col-md-6 col-lg-3 file-item"
                             data-filename="${fileInfo.searchName}">

                            <div class="card file-card h-100 shadow-sm">

                                <div class="card-body">

                                    <div class="d-flex align-items-center">


                                        <div class="file-icon me-3">

                                            <i class="${fileInfo.icon}
                                                      ${fileInfo.iconClass}
                                                      fa-2x">
                                            </i>

                                        </div>


                                        <div class="flex-grow-1 overflow-hidden">

                                            <h6 class="mb-1 file-name text-truncate">

                                                ${fileInfo.actionHtml}
                                               
                                            </h6>


                                            <small class="text-muted text-uppercase">

                                               

                                            </small>
                                          
                                         
                                        </div>


                                          
                                    </div>

                                </div>
                                  <div class="text-center mb-2">
                                 ${fileSize}
                                 ${Ocrused}
                                   </div>
                            </div>

                        </div>
                    `;
                    });


                    html += `
                        </div>

                    </div>
                `;


                    // ==================================================
                    // TABLE VIEW
                    // ==================================================

                    html += `
                   <div id="tableViewContainer"
         class="d-none">

        <div class="table-responsive">

            <table class="table table-hover table-bordered align-middle mb-0 file-list-table">

                <thead class="table-light">

                    <tr>

                        <th class="file-col-serial">
                            #
                        </th>

                        <th class="file-col-type text-center">
                            Type
                        </th>

                        <th class="file-col-name">
                            File / WebSite 
                        </th>
                        <th class="file-col-name">
                           Orignal File Size
                        </th>
                        <th class="file-col-name">
                           Extraction Method
                        </th>

                        <th class="file-col-format text-center">
                            Format
                        </th>

                        <th class="file-col-action text-center">
                            Action
                        </th>

                    </tr>

                </thead>

                <tbody id="fileTableBody">
                `;


                    response.data.forEach(function (item, index) {

                        const fileInfo = prepareFileInfo(
                            item,
                            indexName, OcrType
                        );

                        let shortFileName = fileInfo.fileName;


                        if (fileInfo.fileName.length > 30) {

                            shortFileName =
                                fileInfo.fileName.substring(
                                    0,
                                    30
                                ) + '...';
                        }


                        let Ocrused = item.crawler == null
                            ? `<span>-</span>`
                            : item.crawler.ocr_used === true
                                ? `<span class="ocr-badge ocr-enabled">OCR Enabled</span>`
                                : `<span class="ocr-badge text-extraction">Text Extraction</span>`;
                        let fileSize = ``;
                        if (fileInfo.fileSize == "0")
                            fileSize = `<span>-</span>`
                        else
                            fileSize = `<span class="file-size-badge"><i class="fa-solid fa-hard-drive me-1"></i>${fileInfo.fileSize}</span>`
                        html += `
        <tr class="file-table-item"
            data-filename="${fileInfo.searchName}"
            data-original-index="${index + 1}">

            <td class="file-row-number">
                ${index + 1}
            </td>

            <td class="text-center">

                <i class="${fileInfo.icon}
                          ${fileInfo.iconClass}
                          fa-xl">
                </i>

            </td>

            <td>

                <span title="${fileInfo.fileName}"
                      class="file-table-name">

                    ${shortFileName}

                </span>

            </td>
            <td>

                  ${fileSize}

            </td>
            <td>

               ${Ocrused}

            </td>

            <td class="text-center">

                <span class="badge bg-light text-dark border">

                    ${fileInfo.extension.toUpperCase()}

                </span>

            </td>

            <td class="text-center">

                ${fileInfo.actionIconHtml}

            </td>

        </tr>
    `;
                    });

                    html += `
                </tbody>

            </table>

        </div>

    </div>
`;


                    html += `
                                </tbody>

                            </table>

                        </div>

                    </div>
                `;


                    // ==================================================
                    // PAGINATION
                    // ==================================================

                    html += `
                    <div class="row mt-3 align-items-center">

                        <div class="col-md-6 mb-2 mb-md-0">

                            <small class="text-muted"
                                   id="paginationInfo">
                            </small>

                        </div>


                        <div class="col-md-6">

                            <nav aria-label="File pagination">

                                <ul class="pagination pagination-sm
                                           justify-content-md-end
                                           mb-0"
                                    id="filePagination">

                                </ul>

                            </nav>

                        </div>

                    </div>
                `;

                }

                // ==========================================
                // NO DATA
                // ==========================================

                else {

                    html = `
                    <div class="alert alert-info mb-0">

                        <i class="fa-solid fa-circle-info me-2"></i>

                        No files found.

                    </div>
                `;
                }


                // ==========================================
                // INSERT HTML
                // ==========================================

                $('#fileList').html(html);


                // ==========================================
                // INITIALIZE PAGINATION
                // ==========================================

                if (
                    response.data &&
                    response.data.length > 0
                ) {

                    initializeFilePagination(1);
                }


                // ==========================================
                // OPEN MODAL
                // ==========================================

                let modalElement =
                    document.getElementById(
                        'fileModal'
                    );


                let modal =
                    bootstrap.Modal.getOrCreateInstance(
                        modalElement
                    );


                modal.show();
            },


            error: function () {

                Swal.fire({

                    icon: "error",

                    title: "Error",

                    text: "Error loading files."
                });

            }

        });
    }

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


// ======================================================
// PREPARE FILE INFORMATION
// ======================================================

function prepareFileInfo(item, indexName, OcrType) {

    let extension = '';
    let path = '';
    let fileName = '';

    let filesize = 0;

    if (item.File && item.File.filesize) {
        filesize = item.File.filesize;
    }

    let displayFileSize = formatFileSize(filesize);
    // ==================================================
    // FILE CRAWLER
    // ==================================================

    if (
        item.Path &&
        item.Path.Real != null
    ) {

        path =
            decryptPayloadData(
                item.Path.Real
            ) || '';


        fileName =
            path
                .split('\\')
                .pop()
                .split('/')
                .pop();


        extension =
            fileName.includes('.')
                ? fileName
                    .split('.')
                    .pop()
                    .toLowerCase()
                : '';
    }


    // ==================================================
    // WEB CRAWLER
    // ==================================================

    else if (item.url != null) {

        extension = 'web';

        path = item.url;

        fileName = item.title;
    }


    // ==================================================
    // ICON
    // ==================================================

    let icon =
        'fa-solid fa-file';

    let iconClass =
        'text-secondary';


    switch (extension) {

        case 'web':

            icon =
                'fa-solid fa-globe';

            iconClass =
                'text-danger';

            break;


        case 'pdf':

            icon =
                'fa-solid fa-file-pdf';

            iconClass =
                'text-danger';

            break;


        case 'doc':

        case 'docx':

            icon =
                'fa-solid fa-file-word';

            iconClass =
                'text-primary';

            break;


        case 'xls':

        case 'xlsx':

            icon =
                'fa-solid fa-file-excel';

            iconClass =
                'text-success';

            break;


        case 'ppt':

        case 'pptx':

            icon =
                'fa-solid fa-file-powerpoint';

            iconClass =
                'text-warning';

            break;


        case 'txt':

            icon =
                'fa-solid fa-file-lines';

            iconClass =
                'text-dark';

            break;


        case 'jpg':

        case 'jpeg':

        case 'png':

        case 'gif':

        case 'bmp':

        case 'webp':

        case 'tif':

        case 'tiff':

            icon =
                'fa-solid fa-file-image';

            iconClass =
                'text-info';

            break;


        case 'zip':

        case 'rar':

        case '7z':

            icon =
                'fa-solid fa-file-zipper';

            iconClass =
                'text-warning';

            break;
    }


    // ==================================================
    // ACTION ICON
    // ==================================================

    let actionIcon = '';


    if (extension === 'web') {

        actionIcon =
            'fa-solid fa-arrow-up-right-from-square';
    }

    else if (extension === 'pdf') {

        actionIcon =
            'fa-solid fa-eye';
    }

    else {

        actionIcon =
            'fa-solid fa-download';
    }


    // ==================================================
    // FILE NAME DISPLAY
    // ==================================================

    let shortFileName = fileName;


    if (fileName.length > 30) {

        shortFileName =
            fileName.substring(
                0,
                30
            ) + '...';
    }


    // ==================================================
    // CARD ACTION HTML
    // ==================================================

    let actionHtml = '';

    let actionIconHtml = '';


    if (extension === 'web') {

        actionHtml = `
            <a href="${path}"
               target="_blank"
               rel="noopener noreferrer"
               class="text-decoration-none"
               title="${item.url}">

                <i class="${actionIcon} me-1"></i>

                ${shortFileName}
             
            </a>
            <br> 
          <a href="${path}"
               target="_blank"
               rel="noopener noreferrer"
               class="text-decoration-none"
               title="${item.url}">

             
                ${item.url}
             
            </a>
        `;


        actionIconHtml = `
            <a href="${path}"
               target="_blank"
               rel="noopener noreferrer"
               class="btn btn-sm btn-outline-primary"
               title="Open Website">

                <i class="${actionIcon}"></i>
                 
            </a>

        `;
    }

    else {
        if (OcrType != 5) {
            actionHtml = `
            <span class="text-decoration-none GetCrawlerId"
                  role="button"
                  title="${fileName}"
                  data-CrawlerId="${indexName}"
                  data-FileName="${path}">

                <i class="${actionIcon} me-1"></i>

                ${shortFileName}

            </span>
        `;


            actionIconHtml = `
            <button type="button"
                    class="btn btn-sm btn-outline-primary GetCrawlerId"
                    title="${extension === 'pdf'
                    ? 'View File'
                    : 'Download File'}"
                    data-CrawlerId="${indexName}"
                    data-FileName="${path}">

                <i class="${actionIcon}"></i>

            </button>
        `;
        }

        else {
            actionHtml = `
            <span class="text-decoration-none"
                  >

               <i class="fa-solid fa-eye-slash text-danger" ></i>

                ${shortFileName}

            </span>
        `;
            actionIconHtml = `<i class="fa-solid fa-eye-slash  text-danger"></i>`;
        }
    }


    return {

        extension:
            extension || 'file',

        path:
            path,

        fileName:
            fileName,
        fileSize:
            displayFileSize,
        searchName:
            fileName.toLowerCase(),

        icon:
            icon,

        iconClass:
            iconClass,

        actionHtml:
            actionHtml,

        actionIconHtml:
            actionIconHtml
    };
}


// ======================================================
// PAGINATION
// ======================================================

function initializeFilePagination(page = 1) {

    fileCurrentPage = page;


    const searchText =
        ($('#fileSearch').val() || '')
            .toLowerCase()
            .trim();


    // ==================================================
    // CHECK ACTIVE VIEW
    // ==================================================

    const isCardView =
        !$('#cardViewContainer')
            .hasClass('d-none');


    let items;


    if (isCardView) {

        items =
            $('.file-item');
    }

    else {

        items =
            $('.file-table-item');
    }


    // ==================================================
    // FILTER SEARCH RESULTS
    // ==================================================

    const filteredItems =
        items.filter(function () {

            const fileName =
                ($(this)
                    .attr('data-filename') || '')
                    .toLowerCase();


            return fileName.includes(
                searchText
            );
        });


    const totalRecords =
        filteredItems.length;


    const totalPages =
        Math.ceil(
            totalRecords /
            fileRecordsPerPage
        );


    // ==================================================
    // VALID PAGE NUMBER
    // ==================================================

    if (
        totalPages > 0 &&
        fileCurrentPage > totalPages
    ) {

        fileCurrentPage =
            totalPages;
    }


    if (fileCurrentPage < 1) {

        fileCurrentPage = 1;
    }


    // ==================================================
    // HIDE ALL ITEMS
    // ==================================================

    items.addClass('d-none');


    // ==================================================
    // CURRENT PAGE
    // ==================================================

    const startIndex =
        (fileCurrentPage - 1) *
        fileRecordsPerPage;


    const endIndex =
        startIndex +
        fileRecordsPerPage;


    filteredItems
        .slice(
            startIndex,
            endIndex
        )
        .removeClass('d-none');


    // ==================================================
    // UPDATE TABLE SERIAL NUMBER
    // ==================================================

    if (!isCardView) {

        filteredItems
            .slice(
                startIndex,
                endIndex
            )
            .each(
                function (index) {

                    $(this)
                        .find(
                            '.file-row-number'
                        )
                        .text(
                            startIndex +
                            index +
                            1
                        );

                }
            );
    }


    // ==================================================
    // UPDATE COUNT
    // ==================================================

    $('#fileCount')
        .text(totalRecords);


    // ==================================================
    // PAGINATION UI
    // ==================================================

    renderFilePagination(
        totalPages
    );


    updatePaginationInfo(
        totalRecords,
        startIndex,
        endIndex
    );
}


// ======================================================
// RENDER PAGINATION
// ======================================================

function renderFilePagination(totalPages) {

    let html = '';


    if (totalPages <= 1) {

        $('#filePagination')
            .html('');

        return;
    }


    // ==================================================
    // PREVIOUS
    // ==================================================

    html += `
        <li class="page-item
                   ${fileCurrentPage === 1
            ? 'disabled'
            : ''}">

            <button type="button"
                    class="page-link file-page-btn h-30px"
                    data-page="${fileCurrentPage - 1}">

                <i class="fa-solid fa-chevron-left "></i>

            </button>

        </li>
    `;


    // ==================================================
    // COMPACT PAGE NUMBERS
    // ==================================================

    const pageNumbers =
        getPaginationNumbers(
            totalPages,
            fileCurrentPage
        );


    pageNumbers.forEach(
        function (page) {

            if (page === '...') {

                html += `
                    <li class="page-item disabled">

                        <span class="page-link">

                            ...

                        </span>

                    </li>
                `;
            }

            else {

                html += `
                    <li class="page-item
                               ${page === fileCurrentPage
                        ? 'active'
                        : ''}">

                        <button type="button"
                                class="page-link file-page-btn"
                                data-page="${page}">

                            ${page}

                        </button>

                    </li>
                `;
            }

        }
    );


    // ==================================================
    // NEXT
    // ==================================================

    html += `
        <li class="page-item
                   ${fileCurrentPage === totalPages
            ? 'disabled'
            : ''}">

            <button type="button"
                    class="page-link file-page-btn h-30px"
                    data-page="${fileCurrentPage + 1}">

                <i class="fa-solid fa-chevron-right"></i>

            </button>

        </li>
    `;


    $('#filePagination')
        .html(html);
}


// ======================================================
// COMPACT PAGE NUMBERS
//
// Example:
// 1 2 3 4 5
//
// or
//
// 1 ... 5 6 7 ... 20
// ======================================================

function getPaginationNumbers(
    totalPages,
    currentPage
) {

    const pages = [];


    // Show everything if 7 or fewer pages
    if (totalPages <= 7) {

        for (
            let i = 1;
            i <= totalPages;
            i++
        ) {

            pages.push(i);
        }


        return pages;
    }


    // ==================================================
    // FIRST PAGE
    // ==================================================

    pages.push(1);


    // ==================================================
    // LEFT DOTS
    // ==================================================

    if (currentPage > 4) {

        pages.push('...');
    }


    // ==================================================
    // MIDDLE
    // ==================================================

    let startPage =
        Math.max(
            2,
            currentPage - 1
        );


    let endPage =
        Math.min(
            totalPages - 1,
            currentPage + 1
        );


    if (currentPage <= 4) {

        startPage = 2;

        endPage = 5;
    }


    if (
        currentPage >=
        totalPages - 3
    ) {

        startPage =
            totalPages - 4;

        endPage =
            totalPages - 1;
    }


    for (
        let i = startPage;
        i <= endPage;
        i++
    ) {

        pages.push(i);
    }


    // ==================================================
    // RIGHT DOTS
    // ==================================================

    if (
        currentPage <
        totalPages - 3
    ) {

        pages.push('...');
    }


    // ==================================================
    // LAST PAGE
    // ==================================================

    pages.push(totalPages);


    return pages;
}


// ======================================================
// PAGINATION INFORMATION
// ======================================================

function updatePaginationInfo(
    totalRecords,
    startIndex,
    endIndex
) {

    if (totalRecords === 0) {

        $('#paginationInfo')
            .text(
                'No matching files found'
            );

        return;
    }


    const start =
        startIndex + 1;


    const end =
        Math.min(
            endIndex,
            totalRecords
        );


    $('#paginationInfo')
        .text(
            `Showing ${start} - ${end} of ${totalRecords} files`
        );
}


// ======================================================
// PAGINATION BUTTON CLICK
// ======================================================

$(document).on(
    'click',
    '.file-page-btn',
    function () {

        const parent =
            $(this)
                .closest(
                    '.page-item'
                );


        if (
            parent.hasClass(
                'disabled'
            )
        ) {

            return;
        }


        const page =
            parseInt(
                $(this).data('page')
            );


        if (
            isNaN(page) ||
            page < 1
        ) {

            return;
        }


        initializeFilePagination(
            page
        );
    }
);


// ======================================================
// SEARCH
// ======================================================

$(document).on(
    'input',
    '#fileSearch',
    function () {

        initializeFilePagination(
            1
        );
    }
);


// ======================================================
// CARD VIEW
// ======================================================

$(document).on(
    'click',
    '#cardViewBtn',
    function () {

        $('#tableViewContainer')
            .addClass('d-none');


        $('#cardViewContainer')
            .removeClass('d-none');


        $('#cardViewBtn')
            .removeClass(
                'btn-outline-primary'
            )
            .addClass(
                'btn-primary active'
            );


        $('#tableViewBtn')
            .removeClass(
                'btn-primary active'
            )
            .addClass(
                'btn-outline-primary'
            );


        initializeFilePagination(
            1
        );
    }
);


// ======================================================
// TABLE VIEW
// ======================================================

$(document).on(
    'click',
    '#tableViewBtn',
    function () {

        $('#cardViewContainer')
            .addClass('d-none');


        $('#tableViewContainer')
            .removeClass('d-none');


        $('#tableViewBtn')
            .removeClass(
                'btn-outline-primary'
            )
            .addClass(
                'btn-primary active'
            );


        $('#cardViewBtn')
            .removeClass(
                'btn-primary active'
            )
            .addClass(
                'btn-outline-primary'
            );


        initializeFilePagination(
            1
        );
    }
);
function showCrawleExtensions(File_Extensions) {

    let html = "";


    File_Extensions.forEach(function (ext, index) {

        let iconClass = getFileIcon(ext.Key);


        html += `
        <span class="extension-badge extension-animation"
              >

            <i class="${iconClass}"></i>

            ${ext.Key.toUpperCase()}

        </span>
        `;

    });


    $("#CrawleExtensionsContainer").html(html);

}
function showAllowedExtensions(AllowedExtensions) {

    let html = "";


    AllowedExtensions.forEach(function (ext, index) {

        let iconClass = getFileIcon(ext);


        html += `
        <span class="extension-badge extension-animation"
              >

            <i class="${iconClass}"></i>

            ${ext.toUpperCase()}

        </span>
        `;

    });


    $("#extensionContainer").html(html);

}

function ResetCrawlerDashboard() {

    // ===============================
    // Crawler Status Reset
    // ===============================

    $("#crawlerStatusText")
        .text("Checking Indexer...")
        .removeClass("text-success text-danger");


    $("#crawlerStatusDot")
        .css("background", "#94a3b8");


    $("#lastSync")
        .text("");



    // ===============================
    // Statistics Cards Reset
    // ===============================

    $("#filesDiscovered")
        .text("0");


    $("#indexedSearchable")
        .text("0");


    $("#indexedOcr_Files")
        .text("0");


    $("#indexedNotOcr_Files")
        .text("0");


    $("#failedProcess")
        .text("0");


    $("#indexunderProgress")
        .text("0%");



    // ===============================
    // Progress Reset
    // ===============================

    $("#progressText")
        .text("0 of 0 files are searchable");


    $("#progressPercent")
        .text("0%");


    $("#progressBar")
        .css("width", "0%");



    // ===============================
    // Extension Reset
    // ===============================

    $("#extensionContainer")
        .empty();

}