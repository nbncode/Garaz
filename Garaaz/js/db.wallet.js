'use strict';

var filterData = null, partCategory = null, selectedBranchCode = null, selectedCustomerType = null;

const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';

const WalletSubGroup = 'WalletSubGroup';
const roWalletDetail = 'roWalletDetail', roWiseBranchWallet = 'roWiseBranchWallet', roWiseBranchWalletByType = 'roWiseBranchWalletByType';
const seWalletDetail = 'seWalletDetail', seWiseBranchWallet = 'seWiseBranchWallet', seWiseBranchWalletByType = 'seWiseBranchWalletByType';
const csWalletDetail = 'csWalletDetail';

var dbWal = {

    initWallet: () =>
    {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbWal.showLoader(WalletSubGroup);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbWal.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbWal.fetchWalletBySubGroup();
            }
        });
    },

    showLoader: (tag) =>
    {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (tag)
        {
            case WalletSubGroup:
                loadMsg = 'Loading wallet by sub-group...';
                break;

            case roWalletDetail:
                loadMsg = 'Loading RO wallet detail...';
                break;

            case roWiseBranchWallet:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO wallet detail by branch...';
                break;

            case roWiseBranchWalletByType:
                loadMsg = `Loading RO wise branch's customer wallet detail...`;
                break;

            case seWalletDetail:
                loadMsg = 'Loading SE wallet detail...';
                break;

            case seWiseBranchWallet:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE wallet detail by branch...';
                break;

            case seWiseBranchWalletByType:
                loadMsg = `Loading SE wise branch's customer wallet detail...`;
                break;

            case csWalletDetail:
                loadMsg = 'Loading CS wallet detail...';
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

                dbWal.fetchWalletBySubGroup();
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

        const back = $('#back');
        const referer = back.data('referer');

        switch (referer)
        {
            case 'Main':
                dbWal.fetchWalletBySubGroup();
                break;

            case 'RO':
                dbWal.fetchWalletDetailForSubGroup('RO');
                break;

            case 'SE':
                dbWal.fetchWalletDetailForSubGroup('SE');
                break;

            case 'CS':
                dbWal.fetchWalletDetailForSubGroup('CS');
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

    setBackLink: (referer, title, show) =>
    {
        $('#divContainer').empty();
        $('#divSubContainer').empty();

        const back = $('#back');
        back.data('referer', referer);
        back.prop('title', title);
        back.toggle(show);
    },

    fetchWalletByDateFilter: () =>
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
            dbWal.fetchWalletBySubGroup();
        }
    },

    fetchWalletBySubGroup: () =>
    {
        console.log('Fetch sub-group wallets');

        // show main back
        dbCommon.BackMainDb(true);

        dbWal.setBackLink('', '', false);
        dbWal.showLoader(WalletSubGroup);
        dbCommon.request(dashboardController + 'FetchWalletBySubGroup', filterData, function (response)
        {
            $('#divContainer').html(response);
        });
    },

    fetchWalletDetailForSubGroup: (subGroup) =>
    {
        console.log(`Table 1 - Fetch wallet detail for '${subGroup}' sub group`);

        // hide main back
        dbCommon.BackMainDb(false);

        dbWal.setBackLink('Main', 'Back to all wallets', true);

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
                tag = roWalletDetail;
                actionMethod = 'FetchRoWiseWalletDetail';
                break;

            case 'SE':
                tag = seWalletDetail;
                actionMethod = 'FetchSeWiseWalletDetail';
                break;

            case 'CS':
                tag = csWalletDetail;
                actionMethod = 'FetchCsWiseWalletDetail';
                break;
        }

        dbWal.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response)
        {
            $('#divContainer').html(response);

            if (subGroup === 'RO' || subGroup === 'SE')
            {
                dbWal.fetchBranchWalletDetail(subGroup);
            }
        });
    },

    // #region Common for RO & SE

    fetchBranchWalletDetail: (subGroup) => 
    {
        console.log(`Table 2 - Fetch ${subGroup} wise branch wallet detail`);

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
                tag = roWiseBranchWallet;
                actionMethod = 'FetchRoWiseBranchWalletDetail';
                break;

            case 'SE':
                tag = seWiseBranchWallet;
                actionMethod = 'FetchSeWiseBranchWalletDetail';
                break;
        }

        dbWal.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response)
        {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchBranchWalletDetailByType: (subGroup, branchCode) =>
    {
        console.log(`Table 3 - Fetch ${subGroup} wise wallet detail for '${branchCode}' branch code group by type`);

        dbWal.setBackLink(subGroup, `Back to ${subGroup} wallets`, true);

        selectedBranchCode = branchCode;
        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode
        };

        let tag = '';
        let actionMethod = '';

        switch (subGroup)
        {
            case 'RO':
                tag = roWiseBranchWalletByType;
                actionMethod = 'FetchRoWiseWalletDetail';

                break;

            case 'SE':
                tag = seWiseBranchWalletByType;
                actionMethod = 'FetchSeWiseWalletDetail';
                break;
        }

        dbWal.showLoader(tag);
        dbCommon.request(dashboardController + actionMethod, dbFilter, function (response)
        {
            $('#divContainer').html(response);

            dbWal.fetchCustomerWalletDetail(subGroup, '', branchCode);
        });
    },
    
    // #endregion

    // #region COMMON FOR RO, SE, CS

    fetchCustomerWalletDetail: (subGroup, customerType, branchCode) => 
    {
        if (subGroup === 'CS')
        {
            console.log(`Table 2 - Fetch CS wise customer wallet detail for '${customerType}' customer type`);
            selectedCustomerType = customerType;
            dbWal.setBackLink('CS', 'Back to CS wallets', true);

        } else
        {
            console.log(`Table 4 - Fetch ${subGroup} wise customer wallet detail for '${customerType}' customer type and '${branchCode}' branch code`);
        }

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="walletCustomerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>
                                    <th>Avg Sale (3 Months)</th>
                                    <th>Wallet Balance</th>
                                    <th>Payout of Sales %</th>      
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
            { "data": 'AverageSale' },
            { "data": 'WalletBalance' },
            { "data": 'PayoutOfSalesPercentageTxt' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbWal.fetchCustomerDetail('${subGroup}','${branchCode}','${row.CustomerCode}',this)">View</a>`;
                }
            }
        ];

        // Create set footer function
        const setFooter = function (api, intVal)
        {
            // api is DataTable method
            // intVal is method defined in db.common.js

            const totalAvgSale = api.column(4).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalWalletBalance = api.column(5).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalPayoutOfSalesPercentage = api.column(6).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            $(api.column(1).footer()).html('<b>Total</b>');
            $(api.column(4).footer()).html(`<b>${totalAvgSale.toFixed(2)}</b>`);
            $(api.column(5).footer()).html(`<b>${totalWalletBalance.toFixed(2)}</b>`);
            $(api.column(6).footer()).html(`<b>${totalPayoutOfSalesPercentage.toFixed(2)} %</b>`);
        };

        const divId = subGroup === 'CS' ? '#divContainer' : '#divSubContainer';
        $(divId).html(table);

        let url = '';
        switch (subGroup)
        {
            case 'RO':
                url = `${dashboardController}FetchRoWiseBranchCustomersWallet`;
                break;

            case 'SE':
                url = `${dashboardController}FetchSeWiseBranchCustomersWallet`;
                break;

            case 'CS':
                url = `${dashboardController}FetchCsWiseCustomersWallet`;
                break;
        }

        dbCommon.bindDataToTable(url, dbFilter, columns, '#walletCustomerDetail', divId, setFooter, true);
    },

    fetchCustomerDetail: function (subGroup, branchCode, customerCode, anchor)
    {
        if (subGroup === 'CS')
        {
            console.log(`Table 3 - Fetch ${subGroup} particular customer detail for '${customerCode}' customer code`);
            dbWal.setBackLink(`${subGroup}-CU`, `Back to ${subGroup} customers`, true);

        } else
        {
            console.log(`Table 5 & 6 - Fetch ${subGroup} particular customer detail for '${customerCode}' customer code`);
            dbWal.setBackLink(`${subGroup}-BD`, `Back to ${subGroup} ${branchCode} wallets`, true);
        }


        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerCode: customerCode
        };

        // Selected customer table
        const currentRow = $(anchor).closest('tr');
        const selCustTable = `<div class="table-responsive pb-2" id="selectedCustomer">
                        <table class="table table-bordered w-auto table-center">
                        <thead class="thead-light">
                            <tr>
                                <th>Customer Code</th>
                                <th>Customer Name</th>
                                <th>Customer Type</th>
                                <th>Avg Sale (3 Months)</th>
                                <th>Wallet Balance</th>
                                <th>Payout of Sales %</th>
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
                        </tr>
                    </tbody>
                    </table>                    
                </div>`;
        $('#divContainer').html(selCustTable);

        // Create table
        const table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Date Of Transaction</th>
                                    <th>Transaction Details</th>
                                    <th>Amount</th>  
                                </tr>
                            </thead>                            
                        </table>
                    </div>`;

        // Prepare columns
        const columns = [
            { "data": 'SlNo' },
            { "data": 'DateOfTransaction' },
            { "data": 'TransactionDetails' },
            { "data": 'Amount' }
        ];

        $('#divSubContainer').html(table);
        dbCommon.bindDataToTable(dashboardController + 'FetchWalCustomerDetails', dbFilter, columns, '#customerDetail', '#divSubContainer', null, true, false);
    }
    
    // #endregion
};