'use strict';

var filterData = null;
const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';

const osDetail = 'osDetail', osCustomerDetail = 'osCustomerDetail';
const rowOsDetail = 'rowOsDetail', roWiseOsBranchDetail = 'roWiseOsBranchDetail', roWiseOsDetailByBranch = 'roWiseOsDetailByBranch';
const sewOsDetail = 'sewOsDetail', seWiseOsBranchDetail = 'seWiseOsBranchDetail', seWiseOsDetailByBranch = 'seWiseOsDetailByBranch';
const cswOsDetail = 'cswOsDetail';

var dbOs = {

    initOs: () =>
    {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbOs.showLoader(osDetail);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbOs.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbOs.fetchOsBySubGroup();
            }
        });
        
    },

    showLoader: (tag) =>
    {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (tag)
        {
            case osDetail:
                loadMsg = 'Loading outstanding by sub group...';
                break;

            case rowOsDetail:
                loadMsg = 'Loading RO wise outstanding detail...';
                break;

            case roWiseOsBranchDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO wise branch outstanding detail...';
                break;

            case roWiseOsDetailByBranch:
                loadMsg = 'Loading RO wise outstanding detail for branch...';
                break;

            case sewOsDetail:
                loadMsg = 'Loading SE wise outstanding detail...';
                break;

            case seWiseOsBranchDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE wise branch outstanding detail...';
                break;

            case seWiseOsDetailByBranch:
                loadMsg = 'Loading SE wise outstanding detail for branch...';
                break;

            case osCustomerDetail:
                div = $('#divLastContainer');
                loadMsg = 'Loading specific customer detail...';
                break;

            case cswOsDetail:
                loadMsg = 'Loading CS wise outstanding detail...';
                break;
        }

        const loader = `<div class="loader"><div class="kt-spinner home-page kt-spinner--v2 kt-spinner--md kt-spinner--info" style="display: block;"></div><span>${loadMsg}</span></div>`;
        div.html(loader);
    },

    applyFilter: () =>
    {
        let startDate = $('#startDate').val();
        let endDate = $('#endDate').val();

        dbCommon.setSessionValue('Dashboard_StartDate', startDate);
        dbCommon.setSessionValue('Dashboard_EndDate', endDate);

        if (startDate && endDate)
        {
            // Warning - Don't try to change format of the dates
            // moment create date object using the format of datepicker
            const mStartDate = moment(startDate, 'DD-MM-YYYY', true);
            const mEndDate = moment(endDate, 'DD-MM-YYYY', true);

            if (mEndDate.isSameOrAfter(mStartDate))
            {
                // So that MVC bind date properly
                startDate = mStartDate.format('YYYY-MM-DD');
                endDate = mEndDate.format('YYYY-MM-DD');

                filterData = {
                    DateType: dateTypeCd,
                    StartDate: startDate,
                    EndDate: endDate
                };

                dbOs.fetchOsBySubGroup();
            } else
            {
                common.LoadErrorMessage('End Date must be greater than Start Date');
            }

        } else
        {
            common.LoadErrorMessage('Start Date and End Date cannot be blank.');
        }
    },

    handleBack: () => 
    {
        // Reset containers
        $('#divContainer').empty();
        $('#divSubContainer').empty();
        $('#divLastContainer').empty();

        const back = $('#back');
        const referer = back.data('referer');

        switch (referer)
        {
            case 'Main':
                dbOs.fetchOsBySubGroup();
                break;

            case 'RO':
                dbOs.fetchGroupWiseOsDetail('RO');
                break;

            case 'SE':
                dbOs.fetchGroupWiseOsDetail('SE');
                break;

            case 'CS':
                dbOs.fetchGroupWiseOsDetail('CS');
                break;
        }
    },

    setBackLink: (referer, title, show) =>
    {
        $('#divContainer').empty();
        $('#divSubContainer').empty();
        $('#divLastContainer').empty();

        const back = $('#back');
        back.data('referer', referer);
        back.prop('title', title);
        back.toggle(show);
    },

    fetchOsByDateFilter: () =>
    {
        const option = $('#ddlDateFilter').val();

        dbCommon.setSessionValue('Dashboard_DateType', option);

        $('#divStartDate').toggle(option === dateTypeCd);
        $('#divEndDate').toggle(option === dateTypeCd);
        $('#divFilter').toggle(option === dateTypeCd);

        // Fetch all if selected option is not 'custom date'
        if (option !== dateTypeCd)
        {
            filterData = { DateType: option };
            dbOs.fetchOsBySubGroup();
        }
    },

    fetchOsBySubGroup: () =>
    {
        console.log('Fetch outstanding by sub group');

        // show main back
        dbCommon.BackMainDb(true);

        dbOs.setBackLink('', '', false);
        dbOs.showLoader(osDetail);
        dbCommon.request(dashboardController + 'FetchOsBySubGroup', filterData, function (response)
        {
            $('#divContainer').html(response);
        });
    },

    fetchGroupWiseOsDetail: (category) =>
    {
        console.log(`Table 1 - Fetch outstanding detail by '${category}' category`);
        // hide main back
        dbCommon.BackMainDb(false);

        dbOs.setBackLink('Main', 'Back to all outstanding', true);
        filterData.Category = category;

        let groupOsDetail = '';
        let actionMethod = '';

        switch (category)
        {
            case 'RO':
                actionMethod = 'FetchRoWiseOsDetail';
                groupOsDetail = rowOsDetail;
                break;

            case 'SE':
                actionMethod = 'FetchSeWiseOsDetail';
                groupOsDetail = sewOsDetail;
                break;

            case 'CS':
                actionMethod = 'FetchCsWiseOsDetail';
                groupOsDetail = cswOsDetail;
                break;
        }

        if (actionMethod)
        {
            const dbFilter = {
                DateType: filterData.DateType,
                StartDate: filterData.StartDate,
                EndDate: filterData.EndDate,
                Category: category
            };

            dbOs.showLoader(groupOsDetail);
            dbCommon.request(dashboardController + actionMethod, dbFilter, function (response)
            {
                $('#divContainer').html(response);

                switch (category) 
                {
                    case 'RO':
                        dbOs.fetchRoWiseOsBranchDetail();
                        break;

                    case 'SE':
                        dbOs.fetchSeWiseOsBranchDetail();
                        break;
                }
            });
        } else
        {
            console.error('No action method was set.');
        }
    },

    // #region RO wise OS detail

    fetchRoWiseOsBranchDetail: () =>
    {
        console.log(`Table 2 - Fetch RO wise outstanding detail group by branch`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category
        };

        dbOs.showLoader(roWiseOsBranchDetail);
        dbCommon.request(dashboardController + 'FetchRoWiseOsBranchDetail', dbFilter, function (response)
        {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchRoWiseOsDetailByBranch: (branchCode) => 
    {
        console.log(`Table 3 - Fetch RO wise outstanding detail for '${branchCode}' branch code`);

        dbOs.setBackLink('RO', 'Back to RO wise outstanding', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category,
            BranchCode: branchCode
        };

        dbOs.showLoader(roWiseOsDetailByBranch);
        dbCommon.request(dashboardController + 'FetchRoWiseOsDetailByBranch', dbFilter, function (response)
        {
            $('#divContainer').html(response);

            dbOs.fetchRoWiseCustomerOsDetail(branchCode, '');
        });
    },

    fetchRoWiseCustomerOsDetail: (branchCode, customerType) =>
    {
        console.log(`Table 4 - Fetch RO wise customer os detail for '${branchCode}' branch code and '${customerType}' customer type`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        dbOs.fetchCustomerDetail('RO', dbFilter);
    },

    // #endregion

    // #region SE wise OS detail

    fetchSeWiseOsBranchDetail: () =>
    {
        console.log(`Table 2 - Fetch SE wise outstanding detail group by branch`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category
        };

        dbOs.showLoader(seWiseOsBranchDetail);
        dbCommon.request(dashboardController + 'FetchSeWiseOsBranchDetail', dbFilter, function (response)
        {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchSeWiseOsDetailByBranch: (branchCode) => 
    {
        console.log(`Table 3 - Fetch SE wise outstanding detail for '${branchCode}' branch code`);

        dbOs.setBackLink('SE', 'Back to SE wise outstanding', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category,
            BranchCode: branchCode
        };

        dbOs.showLoader(seWiseOsDetailByBranch);
        dbCommon.request(dashboardController + 'FetchSeWiseOsDetailByBranch', dbFilter, function (response)
        {
            $('#divContainer').html(response);

            dbOs.fetchSeWiseCustomerOsDetail(branchCode, '');
        });
    },

    fetchSeWiseCustomerOsDetail: (branchCode, customerType) =>
    {
        console.log(`Table 4 - Fetch SE wise customer os detail for '${branchCode}' branch code and '${customerType}' customer type`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        dbOs.fetchCustomerDetail('SE', dbFilter);
    },

    // #endregion

    fetchCustomerDetail: (subGroup, dbFilter) =>
    {
        // Create table
        const table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>
                                    <th>Outstanding Days</th>
                                    <th>Outstanding</th>
                                    <th>Credit Limit</th>
                                    <th>Critical payment</th>
                                    <th>0-14 Days</th>
                                    <th>14-28 Days</th>
                                    <th>28-50 Days</th>
                                    <th>50-70 days</th>
                                    <th>> 70 days</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                              <tfoot>
		                        <tr>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
	                        </tfoot>
                        </table>
                    </div>`;

        // Prepare columns
        var columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'OutstandingDays' },
            { "data": 'Outstanding' },
            { "data": 'CreditLimit' },
            { "data": 'CriticalPayment' },
            { "data": 'ZeroToFourteenDays' },
            { "data": 'FourteenToTwentyEightDays' },
            { "data": 'TwentyEightToFiftyDays' },
            { "data": 'FiftyToSeventyDays' },
            { "data": 'MoreThanSeventyDays' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbOs.fetchSpecificCustomer('${row.CustomerCode}')">View</a>`;
                }
            }
        ];

        // Create set footer function
        const setFooter = function (api, intVal)
        {
            // api is DataTable method
            // intVal is method defined in db.common.js

            const totalOutstandingDays = api.column(4).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalOutstanding = api.column(5).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCreditLimit = api.column(6).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            const totalCp = api.column(7).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total0To14Days = api.column(8).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total14To28Days = api.column(9).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total28To50Days = api.column(10).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total50To70Days = api.column(11).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalMore70Days = api.column(12).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            $(api.column(1).footer()).html('<b>Total</b>');
            $(api.column(4).footer()).html(`<b>${totalOutstandingDays}</b>`);
            $(api.column(5).footer()).html(`<b>${totalOutstanding}</b>`);
            $(api.column(6).footer()).html(`<b>${totalCreditLimit}</b>`);
            $(api.column(7).footer()).html(`<b>${totalCp}</b>`);
            $(api.column(8).footer()).html(`<b>${total0To14Days}</b>`);
            $(api.column(9).footer()).html(`<b>${total14To28Days}</b>`);
            $(api.column(10).footer()).html(`<b>${total28To50Days}</b>`);
            $(api.column(11).footer()).html(`<b>${total50To70Days}</b>`);
            $(api.column(12).footer()).html(`<b>${totalMore70Days}</b>`);
        };

        var url = subGroup === 'RO' ? `${dashboardController}FetchRoWiseCustomerOsDetail` : `${dashboardController}FetchSeWiseCustomerOsDetail`;
        $('#divSubContainer').html(table);
        dbCommon.bindDataToTable(url, dbFilter, columns, '#customerDetail', '#divSubContainer', setFooter, true);
    },

    // CS wise outstanding
    fetchCsWiseCustomerOsDetail: (customerType) =>
    {
        console.log(`Table 2 - Fetch CS wise outstanding detail for '${customerType}' customer type`);

        dbOs.setBackLink('CS', 'Back to CS wise outstanding', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: filterData.Category,
            CustomerType: customerType
        };

        // Create table
        const table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="csWiseCustomerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Branch Code</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>
                                    <th>Outstanding Days</th>
                                    <th>Outstanding</th>
                                    <th>Credit Limit</th>
                                    <th>Critical payment</th>
                                    <th>0-14 Days</th>
                                    <th>14-28 Days</th>
                                    <th>28-50 Days</th>
                                    <th>50-70 days</th>
                                    <th>> 70 days</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                              <tfoot>
		                        <tr>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                    <td></td>
                                </tr>
	                        </tfoot>
                        </table>
                    </div>`;

        // Prepare columns
        const columns = [
            { "data": 'SlNo' },
            { "data": 'BranchCode' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'OutstandingDays' },
            { "data": 'Outstanding' },
            { "data": 'CreditLimit' },
            { "data": 'CriticalPayment' },
            { "data": 'ZeroToFourteenDays' },
            { "data": 'FourteenToTwentyEightDays' },
            { "data": 'TwentyEightToFiftyDays' },
            { "data": 'FiftyToSeventyDays' },
            { "data": 'MoreThanSeventyDays' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbOs.fetchSpecificCustomer('${row.CustomerCode}')">View</a>`;
                }
            }
        ];

        // Create set footer function
        const setFooter = function (api, intVal)
        {
            // api is DataTable method
            // intVal is method defined in db.common.js

            const totalOutstandingDays = api.column(5).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalOutstanding = api.column(6).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCreditLimit = api.column(7).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            const totalCp = api.column(8).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total0To14Days = api.column(9).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total14To28Days = api.column(10).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total28To50Days = api.column(11).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const total50To70Days = api.column(12).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalMore70Days = api.column(13).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            $(api.column(1).footer()).html('<b>Total</b>');
            $(api.column(5).footer()).html(`<b>${totalOutstandingDays}</b>`);
            $(api.column(6).footer()).html(`<b>${totalOutstanding}</b>`);
            $(api.column(7).footer()).html(`<b>${totalCreditLimit}</b>`);
            $(api.column(8).footer()).html(`<b>${totalCp}</b>`);
            $(api.column(9).footer()).html(`<b>${total0To14Days}</b>`);
            $(api.column(10).footer()).html(`<b>${total14To28Days}</b>`);
            $(api.column(11).footer()).html(`<b>${total28To50Days}</b>`);
            $(api.column(12).footer()).html(`<b>${total50To70Days}</b>`);
            $(api.column(13).footer()).html(`<b>${totalMore70Days}</b>`);
        };

        $('#divContainer').html(table);
        dbCommon.bindDataToTable(`${dashboardController}FetchCsWiseCustomerOsDetail`, dbFilter, columns, '#csWiseCustomerDetail', '#divContainer', setFooter, true);
    },

    fetchSpecificCustomer: (customerCode) =>
    {
        console.log(`Fetch specific customer detail for '${customerCode}' customer code`);

        const dbFilter = { CustomerCode: customerCode };
        dbOs.showLoader(osCustomerDetail);
        dbCommon.request(dashboardController + 'FetchOsSpecificCustomer', dbFilter, function (response)
        {
            $('#divLastContainer').html(response);
        });
    }
};