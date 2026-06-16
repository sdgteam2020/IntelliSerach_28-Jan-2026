// devjs/indexesdetails.assign.js
// Handles assign button, modal, user search and assign operation
var UserList = [];
var selectedUsers = new Set();
var uncheckeduser = new Set();
$(function () {
    function openAssignModal(uuid) {
        selectedUsers.clear(); // ✅
        uncheckeduser.clear(); // ✅
        var modalEl = document.getElementById('assignModal');
        var bsModal = new bootstrap.Modal(modalEl, {});
       
        var $modal = $(modalEl);
        resetUserList()
        // clear previous results and search box
        $modal.find('#userSearch').val('');
        $modal.find('#assignUserTable tbody').empty();

        // show modal (Bootstrap 5)
        bsModal.show();

        var searchTimeout;
        function doSearch(q) {

            var token = $('input[name="__RequestVerificationToken"]').val();
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
                    url: '/Index/SearchUsers',
                    type: 'POST',
                    headers: {
                        'RequestVerificationToken': token
                    },

                    data: function (d) {

                        var sortColumn = "DomainId";
                        var sortDirection = "asc";

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
            var q = $(this).val();
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () { doSearch(q); }, 300);
        });
        $(document).on('change', '.assignUserChk', function () {
            
            var userId = $(this).data('id');
            var userName = $(this).data('username');

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
            var checked = $(this).is(':checked');
           
            $modal.find('.assignUserChk').prop('checked', checked);
        });

        $modal.off('click.assign').on('click.assign', '#assignSave', function () {
           
            $.ajax({
                url: '/Index/AssignIndex',
                method: 'POST',
                contentType: 'application/json',
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                data: JSON.stringify({ IndexId: uuid, UserIds: Array.from(selectedUsers), AllSelected: $("#assignSelectAll").is(':checked') }),
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
        var uuid = $(this).data('index');
        openAssignModal(uuid);
        loadAssignedUsers(uuid);
        resetUserList()

    });

    function resetUserList() {
        $('.DispalyUserList').html("")
        UserList=[]
    }
    function loadAssignedUsers(IndexId) {

        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Index/GetIndexWiseAssginUsers',
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

                    var users = resp.data.split(',');

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
