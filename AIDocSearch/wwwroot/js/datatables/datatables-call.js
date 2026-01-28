// Call the dataTables jQuery plugin
$(document).ready(function () {
	$('#dataTable').DataTable();
});

$(document).ready(function () {
	$('#dataTableActivity').DataTable({
		"order": [[0, 'desc']]
	});
});

$(function (e) {
	//file export datatable
	//var tableBtn = $('.dataTable').DataTable({
	//	lengthChange: false,
	//	buttons: ['copy', 'excel', 'pdf', 'colvis'],
	//	responsive: true,
	//	language: {
	//		searchPlaceholder: 'Search...',
	//		sSearch: '',
	//		lengthMenu: '_MENU_ ',
	//	}
	//});
	//tableBtn.buttons().container().appendTo('#DataTables_Table_0_wrapper .col-md-6:eq(0)');

	//$('.dataTable').DataTable({
	//	language: {
	//		searchPlaceholder: 'Search...',
	//		sSearch: '',
	//		lengthMenu: '_MENU_',
	//	}
	//});
	//var tableBtn = $('.dataTableBtn').DataTable({
	//	lengthChange: false,
	//	buttons: ['copy', 'excel', 'pdf']
	//});

	//tableBtn.buttons().container().appendTo('#DataTables_Table_0_wrapper .col-md-6:eq(0)');

	//$('.dataTableRes').DataTable({
	//	responsive: true,
	//	language: {
	//		searchPlaceholder: 'Search...',
	//		sSearch: '',
	//		lengthMenu: '_MENU_',
	//	}
	//});
	//var table = $('.dataTable-delete').DataTable({
	//	responsive: true,
	//	language: {
	//		searchPlaceholder: 'Search...',
	//		sSearch: '',
	//		lengthMenu: '_MENU_',
	//	}
	//});
	//$('.dataTable-delete tbody').on('click', 'tr', function () {
	//	if ($(this).hasClass('selected')) {
	//		$(this).removeClass('selected');
	//	}
	//	else {
	//		table.$('tr.selected').removeClass('selected');
	//		$(this).addClass('selected');
	//	}
	//});

	//$('#button').click(function () {
	//	table.row('.selected').remove().draw(false);
	//});

	//Details display datatable
	//$('.dataTableRes-1').DataTable({
	//	responsive: true,
	//	language: {
	//		searchPlaceholder: 'Search...',
	//		sSearch: '',
	//		lengthMenu: '_MENU_',
	//	},
	//	responsive: {
	//		details: {
	//			display: $.fn.dataTable.Responsive.display.modal({
	//				header: function (row) {
	//					var data = row.data();
	//					return 'Details for ' + data[0] + ' ' + data[1];
	//				}
	//			}),
	//			renderer: $.fn.dataTable.Responsive.renderer.tableAll({
	//				tableClass: 'table border mb-0'
	//			})
	//		}
	//	}
	//});
});
