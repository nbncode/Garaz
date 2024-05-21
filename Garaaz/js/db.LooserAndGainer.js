'use strict';

var filterData = null, partCategory = null, selectedBranchCode = null, selectedCustomerType = null;

const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';

const LGSubGroup = 'LGSubGroup';
const roLGDetail = 'roLGDetail', roWiseBranchLG = 'roWiseBranchLG', roWiseBranchLGByType = 'roWiseBranchLGByType';
const seLGDetail = 'seLGDetail', seWiseBranchLG = 'seWiseBranchLG', seWiseBranchLGByType = 'seWiseBranchLGByType';
const csLGDetail = 'csLGDetail';

var dbLG = {

    initLG: () => {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbLG.showLoader(LGSubGroup);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbLG.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbLG.fetchLGBySubGroup();
            }
        });
    },

    showLoader: (tag) => {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (tag) {
            case LGSubGroup:
                loadMsg = 'Loading Loosers & Gainers by sub-group...';
                break;

            case roLGDetail:
                loadMsg = 'Loading RO wise Loosers & Gainers detail...';
                break;

            case roWiseBranchLG:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO Loosers & Gainers detail by branch...';
                break;

            case roWiseBranchLGByType:
                loadMsg = `Loading RO wise branch's customer Loosers & Gainers detail...`;
                break;

            case seLGDetail:
                loadMsg = 'Loading SE Loosers & Gainers detail...';
                break;

            case seWiseBranchLG:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE Loosers & Gainers detail by branch...';
                break;

            case seWiseBranchLGByType:
                loadMsg = `Loading SE wise branch's customer Loosers & Gainers detail...`;
                break;

            case csLGDetail:
                loadMsg = 'Loading CS Loosers & Gainers detail...';
                break;
        }

        const loader = `<div class="loader"><div class="kt-spinner home-page kt-spinner--v2 kt-spinner--md kt-spinner--info" style="display: block;"></div><span>${loadMsg}</span></div>`;
        div.html(loader);
    },

    applyFilter: () => {
        let startDate = $('#startDate').val();
        let endDate = $('#endDate').val();

        dbCommon.setSessionValue('Dashboard_StartDate', startDate);
        dbCommon.setSessionValue('Dashboard_EndDate', endDate);

        if (startDate && endDate) {
            // Warning - Don't try to change format of the dates
            // moment create date object using the format of datepicker
            const mStartDate = moment(startDate, 'DD-MM-YYYY', true);
            const mEndDate = moment(endDate, 'DD-MM-YYYY', true);

            if (mEndDate.isSameOrAfter(mStartDate)) {
                // So that MVC bind date properly
                startDate = mStartDate.format('YYYY-MM-DD');
                endDate = mEndDate.format('YYYY-MM-DD');

                filterData = {
                    DateType: dateTypeCd,
                    StartDate: startDate,
                    EndDate: endDate
                };

                dbLG.fetchLGBySubGroup();
            } else {
                common.LoadErrorMessage('End Date must be greater than Start Date');
            }

        } else {
            common.LoadErrorMessage('Start Date and End Date cannot be blank.');
        }
    },

    handleBack: () => {
        // Reset containers
        $('#divContainer').empty();
        $('#divSubContainer').empty();

        const back = $('#back');
        const referer = back.data('referer');

        switch (referer) {
            case 'Main':
                dbLG.fetchLGBySubGroup();
                break;

            case 'RO':
                dbLG.fetchLGDetailForSubGroup('RO');
                break;

            case 'SE':
                dbLG.fetchLGDetailForSubGroup('SE');
                break;

            case 'CS':
                dbLG.fetchLGDetailForSubGroup('CS');
                break;

            case 'RO-BD':
                dbWal.fetchBranchWalletDetailByType('RO', selectedBranchCode);
                break;

            case 'SE-BD':
                dbWal.fetchBranchWalletDetailByType('SE', selectedBranchCode);
                break;

            case 'CS-CU':
                dbWal.fetchCustomerWalletDetail('CS', selectedCustomerType);
                break;
        }
    },

    setBackLink: (referer, title, show) => {
        $('#divContainer').empty();
        $('#divSubContainer').empty();

        const back = $('#back');
        back.data('referer', referer);
        back.prop('title', title);
        back.toggle(show);
    },

    fetchLGByDateFilter: () => {
        const option = $('#ddlDateFilter').val();

        dbCommon.setSessionValue('Dashboard_DateType', option);

        $('#divStartDate').toggle(option === dateTypeCd);
        $('#divEndDate').toggle(option === dateTypeCd);
        $('#divFilter').toggle(option === dateTypeCd);

        // Fetch all if selected option is not 'custom date'
        if (option !== dateTypeCd) {
            filterData = { DateType: option };
            dbLG.fetchLGBySubGroup();
        }
    },

    fetchLGBySubGroup: () => {
        console.log('Fetch sub-group loosers & gainers');

        // show main back
        dbCommon.BackMainDb(true);

        dbLG.setBackLink('', '', false);
        dbLG.showLoader(LGSubGroup);
        dbCommon.request(dashboardController + 'FetchLooserAndGainersBySubGroup', filterData, function (response) {
            $('#divContainer').html(response);
        });
    },

    fetchLGDetailForSubGroup: (subGroup) => {
        console.log(`Table 1 - Fetch Loosers & Gainers detail for '${subGroup}' sub group`);

        // hide main back
        dbCommon.BackMainDb(false);

        dbLG.setBackLink('Main', 'Back to all Loosers & Gainers', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate
        };

        let tag = '';
        let actionMethod = '';

        switch (subGroup) {
            case 'RO':
                tag = roLGDetail;
                actionMethod = 'FetchRoWiseLGDetail';
                break;

            case 'SE':
                tag = seLGDetail;
                actionMethod = 'FetchSeWiseLGDetail';
                break;

            case 'CS':
                tag = csLGDetail;
                actionMethod = 'FetchCsWiseLGDetail';
                $('#divMiniContainer').html(null);
                break;
        }

        dbLG.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response) {
            $('#divContainer').html(response);

            if (subGroup === 'RO' || subGroup === 'SE') {
                dbLG.fetchBranchLGDetail(subGroup);
            }
        });
    },

    // #region Common for RO & SE
    fetchBranchLGDetail: (subGroup) => {
        console.log(`Table 2 - Fetch ${subGroup} wise branch Loosers & Gainers detail`);
        $('#divMiniContainer').html(null);
        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate
        };

        let tag = '';
        let actionMethod = '';

        switch (subGroup) {
            case 'RO':
                tag = roWiseBranchLG;
                actionMethod = 'FetchRoWiseBranchLGDetail';
                break;

            case 'SE':
                tag = seWiseBranchLG;
                actionMethod = 'FetchSeWiseBranchLGDetail';
                break;
        }

        dbLG.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response) {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    // #region Common for RO , SE & CS
    fetchCustomerDetail: function (subGroup, branchCode, salesExecUserId, customerType, anchor) {
        let gainerAction = null;
        let looserAction = null;

        if (subGroup === 'RO') {
            console.log(`Table 4 & 5 - Fetch ${subGroup} wise Loosers & Gainers customer details for '${branchCode}' branchCode`);
            dbLG.setBackLink(`${subGroup}`, `Back to ${subGroup} Loosers & Gainers`, true);
            gainerAction = 'FetchRoGainersCustomerDetails';
            looserAction = 'FetchRoLoosersCustomerDetails';

        } else if (subGroup === 'SE') {
            console.log(`Table 4 & 5 - Fetch ${subGroup} wise Loosers & Gainers customer details for '${salesExecUserId}' salesExecutive`);
            dbLG.setBackLink(`${subGroup}`, `Back to ${subGroup} Loosers & Gainers`, true);
            gainerAction = 'FetchSEGainersCustomerDetails';
            looserAction = 'FetchSELoosersCustomerDetails';
        }

        else {
            console.log(`Table 2 & 3 - Fetch CS wise Loosers & Gainers customer details for '${customerType}' customer type`);
            dbLG.setBackLink(`${subGroup}`, `Back to ${subGroup} Loosers & Gainers`, true);
            gainerAction = 'FetchCSGainersCustomerDetails';
            looserAction = 'FetchCSLoosersCustomerDetails';
        }

        dbLG.SetSelectedTable(subGroup, anchor);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode,
            SalesExecUserId: salesExecUserId,
            CustomerType: customerType
        };

        // Create table
        var gainerTable = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="gCustomerDetail">
                            <caption>
                              <div class="greenCaption">Gainers Customers</div>
                            </caption>
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>                                                <th>Days Ordered_FY</th>
                                    <th>Days Ordered_PY</th>
                                    <th>Growth%</th>
                                    <th>Order Value_FY</th>
                                    <th>Order Value_PY</th>
                                    <th>Growth %</th>
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
            { "data": 'CurrentOrderDays' },
            { "data": 'PreviousOrderDays' },
            { "data": 'GrowthDays' },
            { "data": 'CurrentOrderValue' },
            { "data": 'PreviousOrderValue' },
            { "data": 'GrowthValue' },
        ];

        $('#divSubContainer').html(gainerTable);

        dbCommon.bindDataToTable(dashboardController + gainerAction, dbFilter, columns, '#gCustomerDetail', '#divSubContainer', null, true, true);


        // Create table
        var looserTable = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="lCustomerDetail">
                            <caption>
                              <div class="redCaption">Loosers Customers</div>
                            </caption>
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>                                                <th>Days Ordered_FY</th>
                                    <th>Days Ordered_PY</th>
                                    <th>Growth%</th>
                                    <th>Order Value_FY</th>
                                    <th>Order Value_PY</th>
                                    <th>Growth %</th>
                                </tr>
                            </thead>                            
                        </table>
                    </div>`;

        $('#divMiniContainer').html(looserTable);

        dbCommon.bindDataToTable(dashboardController + looserAction, dbFilter, columns, '#lCustomerDetail', '#divMiniContainer', null, true, true);

      
    },

    SetSelectedTable: function (subGroup, anchor) {
        
        if (subGroup === 'RO') {
            // Selected customer table
            const currentRow = $(anchor).closest('tr');
            const selCustTable = `<div class="table-responsive pb-2" id="selectedBranch">
                        <table class="table table-bordered w-auto table-center">
                        <thead class="thead-light">
                            <tr>
                        <th>Branch Code</th>
                        <th>BranchName</th>
                        <th>Total Customers</th>
                        <th>Loosers</th>
                        <th>Lost Retail Value</th>
                        <th>Gainers</th>
                        <th>Gained Retail Value</th>
                        <th>Days ordered_FY</th>
                        <th>Days ordered_PY</th>
                        <th>Growth%</th>
                        <th>Order Value_FY</th>
                        <th>Order Value_PY</th>
                        <th>Growth %</th>
                             </tr>
                        </thead>
                        <tbody>
                        <tr>                        
                        <td>${currentRow.find('td:eq(1)').text()}</td>
                        <td>${currentRow.find('td:eq(2)').text()}</td>
                        <td>${currentRow.find('td:eq(3)').text()}</td>
                        <td>${currentRow.find('td:eq(4)').text()}</td>
                        <td>${currentRow.find('td:eq(5)').text()}</td>
                        <td>${currentRow.find('td:eq(6)').text()}</td> 
                        <td>${currentRow.find('td:eq(7)').text()}</td>
                        <td>${currentRow.find('td:eq(8)').text()}</td>
                        <td>${currentRow.find('td:eq(9)').text()}</td>
                        <td>${currentRow.find('td:eq(10)').text()}</td>
                        <td>${currentRow.find('td:eq(11)').text()}</td>
                        <td>${currentRow.find('td:eq(12)').text()}</td> 
                        <td>${currentRow.find('td:eq(13)').text()}</td> 
                        </tr>
                    </tbody>
                    </table>                    
                </div>`;
            $('#divContainer').html(selCustTable);

        }
        else if (subGroup === 'SE') {

            // Selected customer table
            const currentRow = $(anchor).closest('tr');
            const selCustTable = `<div class="table-responsive pb-2" id="selectedSales">
                        <table class="table table-bordered w-auto table-center">
                        <thead class="thead-light">
                            <tr>
                        <th>Branch Code</th>
                        <th>Sales Exe. Name</th>
                        <th>Total Customers</th>
                        <th>Loosers</th>
                        <th>Lost Retail Value</th>
                        <th>Gainers</th>
                        <th>Gained Retail Value</th>
                        <th>Days ordered_FY</th>
                        <th>Days ordered_PY</th>
                        <th>Growth%</th>
                        <th>Order Value_FY</th>
                        <th>Order Value_PY</th>
                        <th>Growth %</th>
                             </tr>
                        </thead>
                        <tbody>
                        <tr>                        
                        <td>${currentRow.find('td:eq(1)').text()}</td>
                        <td>${currentRow.find('td:eq(2)').text()}</td>
                        <td>${currentRow.find('td:eq(3)').text()}</td>
                        <td>${currentRow.find('td:eq(4)').text()}</td>
                        <td>${currentRow.find('td:eq(5)').text()}</td>
                        <td>${currentRow.find('td:eq(6)').text()}</td> 
                        <td>${currentRow.find('td:eq(7)').text()}</td>
                        <td>${currentRow.find('td:eq(8)').text()}</td>
                        <td>${currentRow.find('td:eq(9)').text()}</td>
                        <td>${currentRow.find('td:eq(10)').text()}</td>
                        <td>${currentRow.find('td:eq(11)').text()}</td>
                        <td>${currentRow.find('td:eq(12)').text()}</td> 
                        <td>${currentRow.find('td:eq(13)').text()}</td> 
                        </tr>
                    </tbody>
                    </table>                    
                </div>`;
            $('#divContainer').html(selCustTable);
        }

        else {
            // Selected customer table
            const currentRow = $(anchor).closest('tr');
            const selCustTable = `<div class="table-responsive pb-2" id="selectedCustomer">
                        <table class="table table-bordered w-auto table-center">
                        <thead class="thead-light">
                            <tr>
                        <th>Customer Type</th>
                        <th>No of Customers</th>
                        <th>Loosers</th>
                        <th>Lost Retail Value</th>
                        <th>Gainers</th>
                        <th>Gained Retail Value</th>
                        <th>Days ordered_FY</th>
                        <th>Days ordered_PY</th>
                        <th>Growth%</th>
                        <th>Order Value_FY</th>
                        <th>Order Value_PY</th>
                        <th>Growth %</th>
                             </tr>
                        </thead>
                        <tbody>
                        <tr>                        
                        <td>${currentRow.find('td:eq(1)').text()}</td>
                        <td>${currentRow.find('td:eq(2)').text()}</td>
                        <td>${currentRow.find('td:eq(3)').text()}</td>
                        <td>${currentRow.find('td:eq(4)').text()}</td>
                        <td>${currentRow.find('td:eq(5)').text()}</td>
                        <td>${currentRow.find('td:eq(6)').text()}</td> 
                        <td>${currentRow.find('td:eq(7)').text()}</td>
                        <td>${currentRow.find('td:eq(8)').text()}</td>
                        <td>${currentRow.find('td:eq(9)').text()}</td>
                        <td>${currentRow.find('td:eq(10)').text()}</td>
                        <td>${currentRow.find('td:eq(11)').text()}</td>
                        <td>${currentRow.find('td:eq(12)').text()}</td> 
                        </tr>
                    </tbody>
                    </table>                    
                </div>`;
            $('#divContainer').html(selCustTable);

        }

    }

}