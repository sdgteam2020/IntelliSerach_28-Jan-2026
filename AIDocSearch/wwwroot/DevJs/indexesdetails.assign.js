// devjs/indexesdetails.assign.js
// Handles assign button, modal, user search and assign operation

$(function () {
    function openAssignModal(uuid) {
       
        var modalEl = document.getElementById('assignModal');
        var bsModal = new bootstrap.Modal(modalEl, {});

        var $modal = $(modalEl);

        // clear previous results and search box
        $modal.find('#userSearch').val('');
        $modal.find('#assignUserTable tbody').empty();

        // show modal (Bootstrap 5)
        bsModal.show();

        var searchTimeout;
        function doSearch(q) {
            var token = $('input[name="__RequestVerificationToken"]').val();
            $.ajax({
                url: '/Index/SearchUsers',
                method: 'POST',
                headers: { 'RequestVerificationToken': token },
                data: { q: q, uuid:uuid },
                success: function (resp) {
                    if (resp && resp.success) {
                        var rows = [];
                        resp.data.forEach(function (u) {
                           
                            rows.push('<tr>');
                            if (u.IndexId && u.IndexId.length > 0) {
                                // Unchecked
                                rows.push('<td><input type="checkbox" class="assignUserChk" data-id="' + u.Id + '" checked></td>');
                            } else {
                                // Checked
                                rows.push('<td><input type="checkbox" class="assignUserChk" data-id="' + u.Id + '" ></td>');
                            }
                            rows.push('<td>' + u.UserName + '</td>');
                            rows.push('<td>' + (u.Name || '') + '</td>');
                            rows.push('<td>' + (u.RankName || '') + '</td>');
                            rows.push('</tr>');
                        });
                        $modal.find('#assignUserTable tbody').html(rows.join(''));
                        // initialize datatables if required
                        if (!$.fn.dataTable.isDataTable('#assignUserTable')) {
                            table = $('#assignUserTable').DataTable({
                                order: [],
                                paging: true,
                                searching: false,
                                info: true,
                                autoWidth: false,
                                responsive: true,
                                pageLength: 25
                            });
                        } else {
                            table = $('#assignUserTable').DataTable();
                        }
                    } else {
                        $modal.find('#assignUserTable tbody').html('<tr><td colspan="4" class="text-center text-muted">No users found</td></tr>');
                    }
                },
                error: function () {
                    $modal.find('#assignUserTable tbody').html('<tr><td colspan="4" class="text-center text-danger">Error loading users</td></tr>');
                }
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

        // select all checkbox handler (toggle)
        $modal.off('change.assign').on('change.assign', '#assignSelectAll', function () {
            var checked = $(this).is(':checked');
            $modal.find('.assignUserChk').prop('checked', checked);
        });

            $modal.off('click.assign').on('click.assign', '#assignSave', function () {
            var selected = [];
            $modal.find('.assignUserChk:checked').each(function () {
                selected.push(parseInt($(this).attr('data-id')));
            });
            if (!selected.length) {
                alert('Select at least one user');
                return;
            }

            $.ajax({
                url: '/Index/AssignIndex',
                method: 'POST',
                contentType: 'application/json',
                headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
                data: JSON.stringify({ IndexId: uuid, UserIds: selected }),
                success: function (resp) {
                    if (resp && resp.Code==200) {
                        alert('Assigned successfully');
                        bsModal.hide();
                    } else {
                        alert('Failed to assign');
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
    });
});
