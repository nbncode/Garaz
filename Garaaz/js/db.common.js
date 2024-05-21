'use strict';

var dbController = '';

var dbCommon = {

    getDashboardUrl: () => {
        const url = window.location.href;
        return url.indexOf('MobileDashboard') > -1 ? '/MobileDashboard/' : '/Dashboard/';
    },
    setSessionValue: (key, value) => {
        console.log('Set value in session.');
        var data = {
            parameter: key,
            value: value
        };
        doAjaxPost(homeController + 'SetSessionValue', data, function (response) {

        });
    },
    getSessionValue: (successCallback) => {
        console.log('Get value from session.');
        var data = {
            parameter: 'Dashboard_DateType'
        };

        doAjaxPost(homeController + 'GetSessionValue', data, function (response) {

            if (response != "" && response != null) { $('#ddlDateFilter').val(response); }

            if (response == dateTypeCd) {
                data.parameter = 'Dashboard_StartDate';
                doAjaxPost(homeController + 'GetSessionValue', data, function (response) {

                    $('#startDate').val(response);
                    data.parameter = 'Dashboard_EndDate';
                    doAjaxPost(homeController + 'GetSessionValue', data, function (response) {

                        $('#endDate').val(response);

                        $('#divStartDate').css("display", "block")
                        $('#divEndDate').css("display", "block")
                        $('#divFilter').css("display", "block")
                        successCallback();
                    });
                });

            }
            else {

                successCallback();
            }
        });
    },
    bindDatepicker: () => {
        const arrows = {
            leftArrow: '<i class="la la-angle-left"></i>',
            rightArrow: '<i class="la la-angle-right"></i>'
        };

        const config = {
            autoclose: true,
            orientation: 'bottom right',
            todayHighlight: true,
            templates: arrows
        };

        $('#startDate').datepicker(config);
        $('#endDate').datepicker(config);
    },

    request: (url, data, successCallback) => {
        $.ajax({
            type: 'GET',
            data: data,
            url: url,
            success: function (response) {
                successCallback(response);
            },
            error: function (xhr) {
                const errorMessage = `${xhr.status}:${xhr.statusText}`;
                console.error(errorMessage);
                console.log(xhr.responseText);
            }
        });
    },
    requestSync: (url, data, successCallback) => {
        $.ajax({
            type: 'GET',
            async: false,
            data: data,
            url: url,
            success: function (response) {
                successCallback(response);
            },
            error: function (xhr) {
                const errorMessage = `${xhr.status}:${xhr.statusText}`;
                console.error(errorMessage);
                console.log(xhr.responseText);
            }
        });
    },

    applyDataTable: () => {
        $('table.dataTable').DataTable({
            // Ref - https://stackoverflow.com/a/4964423/1041457
            // Disable default ordering so as to show schemes sorted by created date
            "order": []
        });
    },

    intVal: (i) => {
        return typeof i === 'string' ? i.replace(/[\%]/g, '') * 1 : typeof i === 'number' ? i : 0;
    },

    bindDataToTable: (url, filter, columns, tableId, divId, setFooter, enableOrdering = false, enableSearching = true, callback, callbackArgs) => {
        var table = $(tableId).DataTable({
            "processing": true,
            "serverSide": true,
            "responsive": true,
            "filter": enableSearching,
            "ordering": enableOrdering,
            "orderMulti": false,
            "autoWidth": false,
            "bLengthChange": true, // Show 'Show 10 entries'
            "paging": true,
            "pageLength": 50, // Number of default records to show
            "ajax": {
                "url": url,
                "data": { dbFilter: filter }, // https://datatables.net/reference/option/ajax.data
                "type": 'POST',
                "datatype": 'json'
            },
            "columns": columns,  // https://datatables.net/examples/ajax/objects.html
            "footerCallback": function () // https://datatables.net/examples/advanced_init/footer_callback.html
            {
                if (setFooter) // Check if not null or undefined
                {
                    const api = this.api();
                    setFooter(api, dbCommon.intVal);
                }
            }
        });

        $.fn.dataTable.SearchAfterDelay(table);

        // Don't show alert. Show error instead
        $.fn.dataTable.ext.errMode = 'none';
        $(tableId).on('error.dt', function (e, settings, techNote, message) {
            $(divId).html(`<p class="kt-font-bold"><i class="kt-font-warning flaticon-warning-sign"></i>&nbsp;${message}</p>`);
        });

        $(tableId).on('init.dt', function () {
            if (typeof callback === 'function') {
                const totalAvgSale = table.column(4).data().sum();
                callback(callbackArgs.SubGroup, callbackArgs.Anchor, totalAvgSale);
            }
        });
    },

    numberFormat: (number, decimals, decPoint, thousandsSep) => {
        // http://kevin.vanzonneveld.net
        // *     example 1: number_format(1234.56);
        // *     returns 1: '1,235'
        // *     example 2: number_format(1234.56, 2, ',', ' ');
        // *     returns 2: '1 234,56'
        // *     example 3: number_format(1234.5678, 2, '.', '');
        // *     returns 3: '1234.57'
        // *     example 4: number_format(67, 2, ',', '.');
        // *     returns 4: '67,00'
        // *     example 5: number_format(1000);
        // *     returns 5: '1,000'
        // *     example 6: number_format(67.311, 2);
        // *     returns 6: '67.31'
        // *     example 7: number_format(1000.55, 1);
        // *     returns 7: '1,000.6'
        // *     example 8: number_format(67000, 5, ',', '.');
        // *     returns 8: '67.000,00000'
        // *     example 9: number_format(0.9, 0);
        // *     returns 9: '1'
        // *    example 10: number_format('1.20', 2);
        // *    returns 10: '1.20'
        // *    example 11: number_format('1.20', 4);
        // *    returns 11: '1.2000'
        // *    example 12: number_format('1.2000', 3);
        // *    returns 12: '1.200'
        var n = !isFinite(+number) ? 0 : +number,
            prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
            sep = (typeof thousandsSep === 'undefined') ? ',' : thousandsSep,
            dec = (typeof decPoint === 'undefined') ? '.' : decPoint,
            toFixedFix = function (n, prec) {
                // Fix for IE parseFloat(0.55).toFixed(0) = 0;
                var k = Math.pow(10, prec);
                return Math.round(n * k) / k;
            },
            s = (prec ? toFixedFix(n, prec) : Math.round(n)).toString().split('.');
        if (s[0].length > 3) {
            s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
        }
        if ((s[1] || '').length < prec) {
            s[1] = s[1] || '';
            s[1] += new Array(prec - s[1].length + 1).join('0');
        }
        return s.join(dec);
    },

    BackMainDb: (isShow) => {
        const back = $('#backtoMain');
        back.toggle(isShow);
    }
};

// Ref - https://datatables.net/forums/discussion/33028/searchdelay-for-server-side-issue
$.fn.dataTable.SearchAfterDelay = function (table, delayTime = 500) {
    const tableId = table.settings()[0].sTableId;
    $(`.dataTables_filter input[aria-controls="${tableId}"]`) // select the correct input field
        .unbind() // Unbind previous default bindings
        .bind('input',
            delay(function (e) { // Bind our desired behavior
                table.search($(this).val()).draw();
                return;
            },
                delayTime)); // Set delay in milliseconds
};

function delay(callback, ms) {
    var timer = 0;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            callback.apply(context, args);
        }, ms || 0);
    };
}

// Ref - https://datatables.net/plug-ins/api/sum()
// This plug-in will take the data from an API result set and sum it, returning the summed value.
$.fn.dataTable.Api.register('sum()', function () {
    return this.flatten().reduce(function (a, b) {
        if (typeof a === 'string') {
            a = a.replace(/[^\d.-]/g, '') * 1;
        }
        if (typeof b === 'string') {
            b = b.replace(/[^\d.-]/g, '') * 1;
        }

        return a + b;
    }, 0);
});