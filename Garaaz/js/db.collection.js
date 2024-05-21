'use strict';

var filterData = null, selectedBranchCode, selectedCustomerType;
const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate'; 
const colDetail = 'colDetail';
const rowColDetail = 'rowColDetail', rowBranchColDetail = 'rowBranchColDetail', rowBranchColByType = 'rowBranchColByType';
const sewColDetail = 'sewColDetail', sewBranchColDetail = 'sewBranchColDetail', sewBranchColByType = 'sewBranchColByType';
const cswColDetail = 'cswColDetail';

var dbCol = {

    initCol: () =>
    {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbCol.showLoader(colDetail);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbCol.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbCol.fetchColBySubGroup();
            }
        });
    },

    showLoader: (tag) =>
    {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (tag)
        {
            case colDetail:
                loadMsg = 'Loading collection by sub group...';
                break;

            case rowColDetail:
                loadMsg = 'Loading RO wise collection detail...';
                break;

            case rowBranchColDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO wise branch collection detail...';
                break;

            case rowBranchColByType:
                loadMsg = 'Loading RO wise collection detail for branch...';
                break;

            case sewColDetail:
                loadMsg = 'Loading SE wise collection detail...';
                break;

            case sewBranchColDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE wise branch collection detail...';
                break;

            case sewBranchColByType:
                loadMsg = 'Loading SE wise collection detail for branch...';
                break;

            case cswColDetail:
                loadMsg = 'Loading CS wise collection detail...';
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

                dbCol.fetchColBySubGroup();
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
                dbCol.fetchColBySubGroup();
                break;

            case 'RO':
                dbCol.fetchColDetailForSubGroup('RO');
                break;

            case 'SE':
                dbCol.fetchColDetailForSubGroup('SE');
                break;

            case 'CS':
                dbCol.fetchColDetailForSubGroup('CS');
                break;

            case 'RO-BD':
                dbCol.fetchBranchColByType('RO', selectedBranchCode);
                break;

            case 'SE-BD':
                dbCol.fetchBranchColByType('SE', selectedBranchCode);
                break;

            case 'CS-CU':
                dbCol.fetchCsTypeCustomers('CS', selectedCustomerType);
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

    fetchColByDateFilter: () =>
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
            dbCol.fetchColBySubGroup();
        }
    },

    fetchColBySubGroup: () =>
    {
        console.log('Fetch collection by sub group');

        // show main back
        dbCommon.BackMainDb(true);

        dbCol.setBackLink('', '', false);
        dbCol.showLoader(colDetail);
        dbCommon.request(dashboardController + 'FetchColBySubGroup', filterData, function (response)
        {
            $('#divContainer').html(response);
        });
    },

    fetchColDetailForSubGroup: (subGroup) =>
    {
        console.log(`Table 1 - Fetch collection detail for '${subGroup}' sub group`);

        // hide main back
        dbCommon.BackMainDb(false);

        dbCol.setBackLink('Main', 'Back to all collections', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: subGroup
        };

        switch (subGroup)
        {
            case 'RO':
                dbCol.showLoader(rowColDetail);
                dbCommon.request(dashboardController + `FetchRoWiseColDetail`, dbFilter, function (response)
                {
                    $('#divContainer').html(response);
                    dbCol.fetchBranchColDetail(subGroup);
                });
                break;

            case 'SE':
                dbCol.showLoader(sewColDetail);
                dbCommon.request(dashboardController + `FetchSeWiseColDetail`, dbFilter, function (response)
                {
                    $('#divContainer').html(response);
                    dbCol.fetchBranchColDetail(subGroup);
                });
                break;

            case 'CS':
                dbCol.showLoader(cswColDetail);
                dbCommon.request(dashboardController + `FetchCsWiseColDetail`, dbFilter, function (response)
                {
                    $('#divContainer').html(response);
                });
                break;

        }
    },

    // #region Common for RO and SE

    fetchBranchColDetail: (subGroup) =>
    {
        console.log(`Table 2 - Fetch ${subGroup} wise collection detail group by branch`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate
        };

        switch (subGroup)
        {
            case 'RO':
                dbCol.showLoader(rowBranchColDetail);
                dbCommon.request(dashboardController + 'FetchRoWiseBranchColDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                    dbCommon.applyDataTable();
                });
                break;

            case 'SE':
                dbCol.showLoader(sewBranchColDetail);
                dbCommon.request(dashboardController + 'FetchSeWiseBranchColDetail', dbFilter, function (response)
                {
                    $('#divSubContainer').html(response);
                    dbCommon.applyDataTable();
                });
                break;
        }

    },

    fetchBranchColByType: (subGroup, branchCode) => 
    {
        console.log(`Table 3 - Fetch ${subGroup} wise collection detail for '${branchCode}' branch code group by type`);

        dbCol.setBackLink(subGroup, `Back to ${subGroup} collections`, true);

        selectedBranchCode = branchCode;
        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode
        };

        switch (subGroup)
        {
            case 'RO':
                dbCol.showLoader(rowBranchColByType);
                dbCommon.request(dashboardController + `FetchRoWiseColDetail`, dbFilter, function (response)
                {
                    $('#divContainer').html(response);
                    dbCol.fetchBranchCustomers(subGroup, '', branchCode);
                });
                break;

            case 'SE':
                dbCol.showLoader(sewBranchColByType);
                dbCommon.request(dashboardController + `FetchSeWiseColDetail`, dbFilter, function (response)
                {
                    $('#divContainer').html(response);
                    dbCol.fetchBranchCustomers(subGroup, '', branchCode);
                });
                break;
        }
    },

    fetchBranchCustomers: (subGroup, customerType, branchCode) =>
    {
        console.log(`Table 4 - Fetch ${subGroup} wise customer collection detail for '${customerType}' customer type and '${branchCode}' branch code`);

        if (subGroup === 'CS')
        {
            selectedCustomerType = customerType;
            dbCol.setBackLink('CS', 'Back to CS collections', true);
        }

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        // Create table
        const table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>                                    
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
            { "data": 'CustomerType' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbCol.fetchCustomerDetail('${subGroup}','${branchCode}','${row.CustomerCode}')">View</a>`;
                }
            }
        ];
       
        $('#divSubContainer').html(table);
        const url = subGroup === 'RO' ? `${dashboardController}FetchRoWiseBranchCustomersCol` : `${dashboardController}FetchSeWiseBranchCustomersCol`;
        dbCommon.bindDataToTable(url, dbFilter, columns, '#customerDetail', '#divSubContainer', null, true);
    },

    fetchCustomerDetail: (subGroup, branchCode, customerCode) =>
    {
        console.log(`Table 5 - Fetch ${subGroup} particular customer detail for '${customerCode}' customer code`);

        dbCol.setBackLink(`${subGroup}-BD`, `Back to ${subGroup} ${branchCode} collections`, true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
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
                                    <th>Customer Type</th>                                    
                                    <th>Payment Date</th>
                                    <th>Particulars</th>
                                    <th>Payment Amount</th>
                                </tr>
                            </thead>                            
                        </table>
                    </div>`;

        // Prepare columns
        const columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'PaymentDate' },
            { "data": 'Particulars' },
            { "data": 'PaymentAmount' }
        ];

        $('#divContainer').html(table);
        dbCommon.bindDataToTable(`${dashboardController}FetchColCustomerDetails`, dbFilter, columns, '#customerDetail', '#divContainer', null, true);
    },

    // #endregion

    // #region CS wise collection

    fetchCsTypeCustomers: (subGroup, customerType) =>
    {
        console.log(`Table 2 - Fetch ${subGroup} wise customer collection detail for '${customerType}' customer type`);

        selectedCustomerType = customerType;
        dbCol.setBackLink('CS', 'Back to CS collections', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            CustomerType: customerType
        };

        // Create table
        const table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>                                    
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
            { "data": 'CustomerType' },
            {
                "render": function (data, type, row, meta)
                {
                    return `<a href="javascript:void(0);" onclick="dbCol.fetchCsCustomerDetail('${subGroup}','${row.CustomerCode}')">View</a>`;
                }
            }
        ];

        $('#divContainer').html(table);
        dbCommon.bindDataToTable(dashboardController + `FetchCsWiseCustomersCol`, dbFilter, columns, '#customerDetail', '#divContainer',null,true);
    },

    fetchCsCustomerDetail: (subGroup, customerCode) =>
    {
        console.log(`Table 3 - Fetch ${subGroup} particular customer detail for '${customerCode}' customer code`);

        dbCol.setBackLink(`${subGroup}-CU`, `Back to ${subGroup} customers`, true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
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
                                    <th>Customer Type</th>                                    
                                    <th>Payment Date</th>
                                    <th>Particulars</th>
                                    <th>Payment Amount</th>
                                </tr>
                            </thead>                            
                        </table>
                    </div>`;

        // Prepare columns
        const columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'PaymentDate' },
            { "data": 'Particulars' },
            { "data": 'PaymentAmount' }
        ];

        $('#divContainer').html(table);
        dbCommon.bindDataToTable(`${dashboardController}FetchColCustomerDetails`, dbFilter, columns, '#customerDetail', '#divContainer', null, true);
    }

    // #endregion
};