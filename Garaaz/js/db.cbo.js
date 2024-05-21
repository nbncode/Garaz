'use strict';

var filterData = null, partCategory = null;

const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';

const cboSubGroup = 'cboSubGroup';
const roCboDetail = 'roCboDetail', roWiseBranchCboDetail = 'roWiseBranchCboDetail', roWiseBranchCustomerCboDetail = 'roWiseBranchCustomerCboDetail', roWiseCustomerPartsDetail = 'roWiseCustomerPartsDetail';
const seCboDetail = 'seCboDetail', seWiseBranchCboDetail = 'seWiseBranchCboDetail', seWiseBranchCustomerCboDetail = 'seWiseBranchCustomerCboDetail',
    seWiseCustomerPartsDetail = 'seWiseCustomerPartsDetail';

let selectedBranchCode, branchTable;

var dbCbo = {

    initCbo: () =>
    {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbCbo.showLoader(cboSubGroup);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbCbo.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbCbo.fetchCboBySubGroup();
            }
        });
       
    },

    showLoader: (tag) =>
    {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (tag)
        {
            case cboSubGroup:
                loadMsg = 'Loading cbo by sub-group...';
                break;

            case roCboDetail:
                loadMsg = 'Loading RO CBO detail...';
                break;

            case roWiseBranchCboDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO CBO detail by branch...';
                break;

            case roWiseBranchCustomerCboDetail:
                div = $('#divSubContainer');
                loadMsg = `Loading RO wise branch's customer CBO detail...`;
                break;

            case roWiseCustomerPartsDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO customer parts detail...';
                break;

            case seCboDetail:
                loadMsg = 'Loading SE CBO detail...';
                break;

            case seWiseBranchCboDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE CBO detail by branch...';
                break;

            case seWiseBranchCustomerCboDetail:
                div = $('#divSubContainer');
                loadMsg = `Loading SE wise branch's customer CBO detail...`;
                break;

            case seWiseCustomerPartsDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE customer parts detail...';
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

                dbCbo.fetchCboBySubGroup();
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
                dbCbo.fetchCboBySubGroup();
                break;

            case 'RO':
                dbCbo.fetchCboDetailForSubGroup('RO');
                break;

            case 'SE':
                dbCbo.fetchCboDetailForSubGroup('SE');
                break;

            case 'RO-BD':
                dbCbo.fetchBranchCustomerCboDetail('RO', selectedBranchCode);
                break;

            case 'SE-BD':
                dbCbo.fetchBranchCustomerCboDetail('SE', selectedBranchCode);
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

    fetchCboByDateFilter: () =>
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
            dbCbo.fetchCboBySubGroup();
        }
    },

    fetchCboBySubGroup: () =>
    {
        console.log('Fetch sub-group cbos');

        // show main back
        dbCommon.BackMainDb(true);

        dbCbo.setBackLink('', '', false);
        dbCbo.showLoader(cboSubGroup);
        dbCommon.request(dashboardController + 'FetchCboBySubGroup', filterData, function (response)
        {
            $('#divContainer').html(response);
        });
    },

    fetchCboDetailForSubGroup: (subGroup) =>
    {
        console.log(`Table 1 - Fetch cbo detail for '${subGroup}' sub group`);

        // hide main back
        dbCommon.BackMainDb(false);

        dbCbo.setBackLink('Main', 'Back to all customer back orders', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate
        };

        switch (subGroup)
        {
            case 'RO':
                dbCbo.showLoader(roCboDetail);
                dbCommon.request(dashboardController + 'FetchRoWiseCboDetail', dbFilter, function (response)
                {
                    $('#divContainer').html(response);

                    dbCbo.fetchBranchCboDetail('RO', '');
                });
                break;

            case 'SE':
                dbCbo.showLoader(seCboDetail);
                dbCommon.request(dashboardController + 'FetchSeWiseCboDetail', dbFilter, function (response)
                {
                    $('#divContainer').html(response);

                    dbCbo.fetchBranchCboDetail('SE', '');
                });
                break;
        }
    },

    fetchBranchCboDetail: (subGroup, customerType) => 
    {
        console.log(`Table 2 - Fetch ${subGroup} wise branch cbo detail for '${customerType}' customer type`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerType: customerType
        };

        switch (subGroup)
        {
            case 'RO':
                dbCbo.showLoader(roWiseBranchCboDetail);
                dbCommon.request(dashboardController + 'FetchRoWiseBranchCboDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                    dbCommon.applyDataTable();
                });
                break;

            case 'SE':
                dbCbo.showLoader(seWiseBranchCboDetail);
                dbCommon.request(dashboardController + 'FetchSeWiseBranchCboDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                    dbCommon.applyDataTable();
                });
                break;
        }
    },

    fetchBranchCustomerCboDetail: (subGroup, branchCode, anchor) =>
    {
        console.log(`Table 3 & 4 - Fetch ${subGroup} wise branch customer cbo detail for '${branchCode}' branch code`);

        dbCbo.setBackLink(subGroup, `Back to ${subGroup} customer back orders`, true);

        selectedBranchCode = branchCode;
        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode
        };

        // Prepare cbo branch table
        if (anchor)
        {
            const currentRow = $(anchor).closest('tr');
            branchTable = `<table class="table table-bordered w-auto table-center">
                                <thead class="thead-light">
                                    <tr>
                                        <th>Branch Code</th>
                                        <th>Branch Name</th>
                                        <th>No.CBO Customers</th>
                                        <th>No.CBO Orders</th>
                                        <th>CBO (In lacs)</th>
                                        <th>CBO (Lacs) 0-7 Days</th>
                                        <th>CBO (Lacs) 7- 15 Days</th>
                                        <th>CBO (Lacs) >15 Days</th>
                                        <th>Sales (In Lacs)</th>
                                        <th>% CBO</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>${branchCode}</td>
                                        <td>${currentRow.find('td:eq(2)').text()}</td>
                                        <td>${currentRow.find('td:eq(3)').text()}</td>
                                        <td>${currentRow.find('td:eq(4)').text()}</td>
                                        <td>${currentRow.find('td:eq(5)').text()}</td>
                                        <td>${currentRow.find('td:eq(6)').text()}</td>      
                                        <td>${currentRow.find('td:eq(7)').text()}</td>   
                                        <td>${currentRow.find('td:eq(8)').text()}</td>
                                        <td>${currentRow.find('td:eq(9)').text()}</td>
                                        <td>${currentRow.find('td:eq(10)').text()}</td>                       
                                     </tr>
                                </tbody>
                            </table>`;
        }
        $('#divContainer').html(branchTable);

        switch (subGroup)
        {
            case 'RO':
                dbCbo.showLoader(roWiseBranchCustomerCboDetail);
                dbCommon.request(dashboardController + 'FetchRoWiseBranchCustomerCboDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                    dbCbo.fetchCustomerCboDetail(subGroup, '', branchCode);
                });
                break;

            case 'SE':
                dbCbo.showLoader(seWiseBranchCustomerCboDetail);
                dbCommon.request(dashboardController + 'FetchSeWiseBranchCustomerCboDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                    dbCbo.fetchCustomerCboDetail(subGroup, '', branchCode);
                });
                break;
        }
    },

    fetchCustomerCboDetail: (subGroup, customerType, branchCode) => 
    {
        console.log(`Table 5 - Fetch ${subGroup} wise customer cbo detail for '${customerType}' customer type and '${branchCode}' branch code`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="cboCustomerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>
                                    <th>No.CBO Customers</th>
                                    <th>No.CBO Orders</th>
                                    <th>CBO (In lacs)</th>
                                    <th>CBO (Lacs) 0-7 Days</th>
                                    <th>CBO (Lacs) 7- 15 Days</th>
                                    <th>CBO (Lacs) >15 Days</th>
                                    <th>Avg Sale (In Lacs)</th>
                                    <th>% CBO</th>
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
            { "data": 'NumberOfCboCustomers' },
            { "data": 'NumberOfCboOrders' },
            { "data": 'CboPrice' },
            { "data": 'CboPrice0To7Days' },
            { "data": 'CboPrice7To15Days' },
            { "data": 'CboPriceMoreThan15Days' },
            { "data": 'AvgSale' },
            { "data": 'CboPercentageTxt' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbCbo.fetchCustomerDetail('${subGroup}','${row.CustomerCode
                        }','${dbFilter.CustomerType}','${dbFilter.BranchCode}')">View</a>`;
                }
            }
        ];

        // Create set footer function
        const setFooter = function (api, intVal)
        {
            // api is DataTable method
            // intVal is method defined in db.common.js

            const totalCboCustomer = api.column(4).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCboOrder = api.column(5).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCbo = api.column(6).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCbo0To7 = api.column(7).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCbo7To15 = api.column(8).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCboMoreThan15 = api.column(9).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalSale = api.column(10).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCboPercent = api.column(11).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            $(api.column(1).footer()).html('<b>Total</b>');
            $(api.column(4).footer()).html(`<b>${totalCboCustomer}</b>`);
            $(api.column(5).footer()).html(`<b>${totalCboOrder}</b>`);
            $(api.column(6).footer()).html(`<b>${totalCbo}</b>`);
            $(api.column(7).footer()).html(`<b>${totalCbo0To7}</b>`);
            $(api.column(8).footer()).html(`<b>${totalCbo7To15}</b>`);
            $(api.column(9).footer()).html(`<b>${totalCboMoreThan15}</b>`);
            $(api.column(10).footer()).html(`<b>${totalSale}</b>`);
            $(api.column(11).footer()).html(`<b>${totalCboPercent}%</b>`);
        };

        $('#divLastContainer').html(table);
        var url = subGroup === 'RO' ? `${dashboardController}FetchRoWiseCustomerCboDetail` : `${dashboardController}FetchSeWiseCustomerCboDetail`;
        dbCommon.bindDataToTable(url, dbFilter, columns, '#cboCustomerDetail', '#divLastContainer', setFooter,true);
    },

    fetchCustomerDetail: (subGroup, customerCode, customerType, branchCode) =>
    {
        console.log(`Table 6 - Fetch ${subGroup} wise particular customer detail for '${customerCode}' customer code, '${customerType}' customer type and '${branchCode}' branch code`);

        dbCbo.setBackLink(`${subGroup}-BD`, `Back to ${subGroup} ${branchCode} customer back orders`, true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerType: customerType,
            BranchCode: branchCode,
            CustomerCode: customerCode
        };

        // Create table
        const table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>CO Number</th>
                                    <th>No of Parts</th>
                                    <th>CBO Value</th>
                                    <th>No of Days Since order</th>
                                    <th>Action</th>
                                </tr>
                            </thead>
                        </table>
                    </div>`;

        // Prepare columns
        const columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CoNumber' },
            { "data": 'NumberOfParts' },
            { "data": 'CboPrice' },
            { "data": 'NumberOfDaysSinceOrder' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbCbo.fetchCustomerParts('${subGroup}','${row.CoNumber}')">View</a>`;
                }
            }
        ];

        $('#divContainer').html(table);
        var url = subGroup === 'RO' ? `${dashboardController}FetchRoWiseCustomerDetail` : `${dashboardController}FetchSeWiseCustomerDetail`;
        dbCommon.bindDataToTable(url, dbFilter, columns, '#customerDetail', '#divContainer', null, true);
    },

    fetchCustomerParts: (subGroup, coNumber) =>
    {
        console.log(`Table 7 - Fetch ${subGroup} wise customer parts detail for '${coNumber}' customer number`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CoNumber: coNumber
        };

        switch (subGroup)
        {
            case 'RO':
                dbCbo.showLoader(roWiseCustomerPartsDetail);
                dbCommon.request(dashboardController + 'FetchRoWiseCustomerPartsDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                });
                break;

            case 'SE':
                dbCbo.showLoader(seWiseCustomerPartsDetail);
                dbCommon.request(dashboardController + 'FetchSeWiseCustomerPartsDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                });
                break;
        }
    }
};