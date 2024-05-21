'use strict';

var filterData = null, partCategory = null;

const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';

const cusSubGroup = 'cusSubGroup';
const roCusDetail = 'roCusDetail', roWiseBranchCusDetail = 'roWiseBranchCusDetail';
const seCusDetail = 'seCusDetail', seWiseBranchCusDetail = 'seWiseBranchCusDetail';
let selectedBranchCode, branchTable;

var dbCus = {

    initCus: () =>
    {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbCus.showLoader(cusSubGroup);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbCus.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbCus.fetchCusBySubGroup();
            }
        });
       
    },

    showLoader: (tag) =>
    {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (tag)
        {
            case cusSubGroup:
                loadMsg = 'Loading customers by sub-group...';
                break;

            case roCusDetail:
                loadMsg = 'Loading RO customers detail...';
                break;

            case roWiseBranchCusDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO customers detail by branch...';
                break;

            case seCusDetail:
                loadMsg = 'Loading SE customers detail...';
                break;

            case seWiseBranchCusDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE customers detail by branch...';
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

                dbCus.fetchCusBySubGroup();
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
                dbCus.fetchCusBySubGroup();
                break;

            case 'RO':
                dbCus.fetchCusDetailForSubGroup('RO');
                break;

            case 'SE':
                dbCus.fetchCusDetailForSubGroup('SE');
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

    fetchCusByDateFilter: () =>
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
            dbCus.fetchCusBySubGroup();
        }
    },

    fetchCusBySubGroup: () =>
    {
        console.log('Fetch sub-group customers');

        // show main back
        dbCommon.BackMainDb(true);

        dbCus.setBackLink('', '', false);
        dbCus.showLoader(cusSubGroup);
        dbCommon.request(dashboardController + 'FetchCustomerBySubGroup', filterData, function (response)
        {
            $('#divContainer').html(response);
        });
    },

    fetchCusDetailForSubGroup: (subGroup) =>
    {
        console.log(`Table 1 - Fetch customer detail for '${subGroup}' sub group`);

        // hide main back
        dbCommon.BackMainDb(false);

        dbCus.setBackLink('Main', 'Back to all customers', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate
        };

        let tag = '';
        let actionMethod = '';

        switch (subGroup)
        {
            case 'RO':
                tag = roCusDetail;
                actionMethod = 'FetchRoWiseCustomerDetail';
                break;

            case 'SE':
                tag = seCusDetail;
                actionMethod = 'FetchSeWiseCustomerDetail';
                break;
        }

        dbCus.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response)
        {
            $('#divContainer').html(response);

            dbCus.fetchBranchCusDetail(subGroup);
        });
    },

    fetchBranchCusDetail: (subGroup) => 
    {
        console.log(`Table 2 - Fetch ${subGroup} wise branch customer detail`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate
        };

        let tag = '';
        let actionMethod = '';

        switch (subGroup)
        {
            case 'RO':
                tag = roWiseBranchCusDetail;
                actionMethod = 'FetchRoWiseBranchCustomerDetail';
                break;

            case 'SE':
                tag = seWiseBranchCusDetail;
                actionMethod = 'FetchSeWiseBranchCustomerDetail';
                break;
        }

        dbCus.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response)
        {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchBilledCustomers: (subGroup, branchCode, salesExecUserId, anchor) => 
    {
        console.log(subGroup === 'RO' ? `Table 3 - Fetch ${subGroup} wise billed customers for '${branchCode}' branch code` : `Table 3 - Fetch ${subGroup} wise billed customers for '${branchCode}' branch code and '${salesExecUserId}' sales executive user id`);

        dbCus.setBackLink(subGroup, `Back to ${subGroup} customers`, true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode,
            SalesExecUserId: salesExecUserId
        };

        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="bCustomerDetail">
                            <caption>
                              <div class="greenCaption">Billed Customers</div>
                            </caption>
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>                                    
                                    <th>Avg Sales (Lacs)</th>
                                    <th>Current Month Sale</th>
                                    <th>PY ${filterData.DateType}</th>
                                </tr>
                            </thead>                            
                        </table>
                    </div>`;

        // Prepare columns
        var columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'AvgSale' },
            { "data": 'CurrentMonthSale' },
            { "data": 'PrvYrSale' }
        ];

        $('#divContainer').html(table);
        var url = subGroup === 'RO' ? `${dashboardController}FetchRoWiseBilledCustomers` : `${dashboardController}FetchSeWiseBilledCustomers`;
        dbCommon.bindDataToTable(url, dbFilter, columns, '#bCustomerDetail', '#divContainer', null, true);

        dbCus.fetchNonBilledCustomers(subGroup, branchCode, salesExecUserId, anchor);
    },

    showSelectedCustomer: (subGroup, anchor, totalAvgSale) => 
    {
        var secondCol = subGroup === 'RO' ? 'Branch Name' : 'Sales Exec Name';

        // Selected customer table
        const currentRow = $(anchor).closest('tr');
        let selCustTable = `<div class="table-responsive pb-2" id="selectedCustomer">
                        <table class="table table-bordered w-auto table-center">
                           <caption>
                              <div class="redCaption">Non Billed Customers</div>
                            </caption>
                        <thead class="thead-light">
                            <tr>
                                <th>Branch Code</th>
                                <th>${secondCol}</th>
                                <th>Total Customer</th>
                                <th>Non Billed Customer</th>
                                <th>Non Billed %</th>
                                <th>Lost Sale (Lacs)</th>
                             </tr>
                        </thead>
                        <tbody>
                        <tr>                        
                        <td>${currentRow.find('td:eq(1)').text()}</td>
                        <td>${currentRow.find('td:eq(2)').text()}</td>
                        <td>${currentRow.find('td:eq(3)').text()}</td>
                        <td>${currentRow.find('td:eq(6)').text()}</td>
                        <td>${currentRow.find('td:eq(7)').text()}</td>
                        <td>${dbCommon.numberFormat(totalAvgSale)}</td> 
                        </tr>
                    </tbody>
                    </table>                    
                </div>`;
        $('#divSubContainer').html(selCustTable);
    },

    fetchNonBilledCustomers: (subGroup, branchCode, salesExecUserId, anchor) => 
    {
        console.log(subGroup === 'RO' ? `Table 4 & 5 - Fetch ${subGroup} wise billed customers for '${branchCode}' branch code` : `Table 4 & 5 - Fetch ${subGroup} wise billed customers for '${branchCode}' branch code and '${salesExecUserId}' sales executive user id`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode,
            SalesExecUserId: salesExecUserId
        };

        // Selected customer table
        //dbCus.showSelectedCustomer(anchor);

        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="nonBilledCustomerDetail">
                            
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>                                    
                                    <th>Avg Sales (Lacs)</th>
                                    <th>Non Billed Since (In Days)</th>
                                    <th>PY ${filterData.DateType}</th>
                                </tr>
                            </thead>                            
                        </table>
                    </div>`;

        // Prepare columns
        var columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'AvgSale' },
            { "data": 'NonBilledFromDays' },
            { "data": 'PrvYrSale' }
        ];

        const callbackArgs = { SubGroup: subGroup, Anchor: anchor };

        $('#divLastContainer').html(table);
        var url = subGroup === 'RO' ? `${dashboardController}FetchRoWiseNonBilledCustomers` : `${dashboardController}FetchSeWiseNonBilledCustomers`;
        dbCommon.bindDataToTable(url, dbFilter, columns, '#nonBilledCustomerDetail', '#divLastContainer', null, true, true, dbCus.showSelectedCustomer, callbackArgs);
    }
};