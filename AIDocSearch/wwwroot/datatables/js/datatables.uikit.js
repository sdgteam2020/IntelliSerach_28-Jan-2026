/*! DataTables UIkit 3 integration
 */

(function(factory){
	if (typeof define === 'function' && define.amd) {
		// AMD
		define(['datatables.net'], function (dt) {
			return factory(window, document, dt);
		});
	}
	else if (typeof exports === 'object') {
		// CommonJS
		var cjsRequires = function (root) {
			if (! root.DataTable) {
				require('datatables.net')(root);
			}
		};

		if (typeof window === 'undefined') {
			module.exports = function (root) {
				if (! root) {
					// CommonJS environments without a window global must pass a
					// root. This will give an error otherwise
					root = window;
				}

				cjsRequires(root);
				return factory(root, root.document, root.DataTable);
			};
		}
		else {
			cjsRequires(window);
			module.exports = factory(window, window.document, window.DataTable);
		}
	}
	else {
		// Browser
		factory(window, document, window.DataTable);
	}
}(function(window, document, DataTable) {
'use strict';


/**
 * This is a tech preview of UIKit integration with DataTables.
 */
/* Set the defaults for DataTables initialisation */
DataTable.util.object.assignDeep(DataTable.defaults, {
    renderer: 'uikit'
});
/* Default class modification */
DataTable.util.object.assignDeep(DataTable.ext.classes, {
    table: 'dataTable uk-form dt-uikit',
    search: {
        input: 'uk-form-small uk-input'
    },
    length: {
        select: 'uk-form-small uk-select'
    },
    processing: {
        container: 'dt-processing uk-panel'
    }
});
DataTable.ext.renderer.pagingButton.uikit = function (settings, buttonType, content, active, disabled) {
    var btnClasses = [];
    if (active) {
        btnClasses.push('uk-active');
    }
    if (disabled) {
        btnClasses.push('uk-disabled');
    }
    var li = DataTable.Dom.c('li').classAdd(btnClasses.join(' '));
    var a = DataTable.Dom
        .c(disabled ? 'span' : 'a')
        .attr('href', disabled ? null : '#')
        .html(content)
        .appendTo(li);
    return {
        display: li.get(0),
        clicker: a.get(0)
    };
};
DataTable.ext.renderer.pagingContainer.uikit = function (settings, buttonEls) {
    return DataTable.Dom
        .c('ul')
        .classAdd('uk-pagination uk-pagination-right uk-flex-right')
        .append(buttonEls)
        .get(0);
};
DataTable.ext.renderer.layout.uikit = function (settings, container, items) {
    var row = DataTable.Dom
        .c('div')
        .classAdd('uk-flex uk-flex-between')
        .appendTo(container);
    DataTable.ext.rendererDisplayRowCells(items, function (key, val) {
        var klass = '';
        if (key === 'start') {
            klass += 'uk-text-left';
        }
        else if (key === 'end') {
            klass += 'uk-text-right';
        }
        else if (key === 'full') {
            if (val.table) {
                klass += 'uk-width-1-1';
            }
            else {
                klass += 'uk-text-center';
            }
        }
        DataTable.Dom
            .c('div')
            .attr('id', val.id || null)
            .classAdd(klass + ' ' + (val.className || ''))
            .append(val.contents)
            .appendTo(row);
    });
};


return DataTable;
}));
