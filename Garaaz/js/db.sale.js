'use strict';

// Global variables
var filterData = null, partCategory = null, partGroupName = null, pgTable = null;

const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';

const salesDetail = 'salesDetail', salesSubGroup = 'salesSubGroup';
const roSalesDetail = 'roSalesDetail', seSalesDetail = 'seSalesDetail', csSalesDetail = 'csSalesDetail', pgSalesDetail = 'pgSalesDetail';

const roWiseBranchSaleDetail = 'roWiseBranchSaleDetail', roWiseCustomerSaleDetail = 'roWiseCustomerSaleDetail', roWiseBranchCustomerSaleDetail = 'roWiseBranchCustomerSaleDetail';
const seWiseBranchSaleDetail = 'seWiseBranchSaleDetail', seWiseCustomerSaleDetail = 'seWiseCustomerSaleDetail', seWiseBranchCustomerSaleDetail = 'seWiseBranchCustomerSaleDetail';
const csWiseCustomerSaleDetail = 'csWiseCustomerSaleDetail';
const pgWiseBranchSaleDetail = 'pgWiseBranchSaleDetail',
    pgWisePartGroupSaleDetail = 'pgWisePartGroupSaleDetail',
    pgWiseBranchCustomerSaleDetail = 'pgWiseBranchCustomerSaleDetail';

var dbSale = {

    initSale: () => {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbSale.showLoader(salesDetail);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbSale.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbSale.fetchSalesByCategory();
            }
        });
    },

    showLoader: (box) => {
        let div = $('#divContainer');
        let loadMsg = '';

        switch (box) {
            case salesDetail:
                loadMsg = 'Loading sales detail...';
                break;

            case salesSubGroup:
                loadMsg = 'Loading sales by sub-group...';
                break;

            case roSalesDetail:
                loadMsg = 'Loading RO sales detail...';
                break;

            case roWiseBranchSaleDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO sales detail by branch...';
                break;

            case roWiseBranchCustomerSaleDetail:
                loadMsg = 'Loading RO sales customer detail by branch...';
                break;

            case roWiseCustomerSaleDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading RO sales detail by customer...';
                break;

            case seSalesDetail:
                loadMsg = 'Loading SE sales detail...';
                break;

            case seWiseBranchSaleDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE sales detail by branch...';
                break;

            case seWiseBranchCustomerSaleDetail:
                loadMsg = 'Loading SE sales customer detail by branch...';
                break;

            case seWiseCustomerSaleDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading SE sales detail by customer...';
                break;

            case csSalesDetail:
                loadMsg = 'Loading CS sales detail...';
                break;

            case csWiseCustomerSaleDetail:
                loadMsg = 'Loading CS sales detail by customer type...';
                break;

            case pgSalesDetail:
                loadMsg = 'Loading PG sales detail...';
                break;

            case pgWisePartGroupSaleDetail:
                div = $('#divSubContainer');
                loadMsg = 'Loading PG sales detail by customer type...';
                break;

            case pgWiseBranchSaleDetail:
                div = $('#divLastContainer');
                loadMsg = 'Loading PG sales detail by branch...';
                break;

            case pgWiseBranchCustomerSaleDetail:
                loadMsg = 'Loading PG sales detail for branch by customer type...';
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

                dbSale.fetchSalesByCategory();
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
        $('#divLastContainer').empty();

        const back = $('#back');
        const referer = back.data('referer');

        switch (referer) {
            case 'Main':
                dbSale.fetchSalesByCategory();
                break;

            case 'SubGroup':
                dbSale.fetchSalesBySubGroup(partCategory);
                break;

            case 'RO':
                dbSale.fetchSaleDetailForSubGroup('RO');
                break;

            case 'SE':
                dbSale.fetchSaleDetailForSubGroup('SE');
                break;

            case 'CS':
                dbSale.fetchSaleDetailForSubGroup('CS');
                break;

            case 'PG':
                dbSale.fetchSaleDetailForSubGroup('PG');
                break;

            case 'PGP':
                dbSale.fetchPgWisePartGroupSaleDetail(partGroupName);
                break;
        }
    },

    setBackLink: (referer, title, show) => {
        $('#divContainer').empty();
        $('#divSubContainer').empty();
        $('#divLastContainer').empty();

        const back = $('#back');
        back.data('referer', referer);
        back.prop('title', title);
        back.toggle(show);
    },

    fetchSalesByDateFilter: () => {
        const option = $('#ddlDateFilter').val();

        dbCommon.setSessionValue('Dashboard_DateType', option);

        $('#divStartDate').toggle(option === dateTypeCd);
        $('#divEndDate').toggle(option === dateTypeCd);
        $('#divFilter').toggle(option === dateTypeCd);

        // Fetch all if selected option is not 'custom date'
        if (option !== dateTypeCd) {
            filterData = { DateType: option };
            dbSale.fetchSalesByCategory();
        }
    },

    fetchSalesByCategory: () => {
        console.log('Fetch sales by category');
        // show main back
        dbCommon.BackMainDb(true);

        dbSale.setBackLink('', '', false);
        dbSale.showLoader(salesDetail);
        dbCommon.request(dashboardController + 'FetchSalesByCategory', filterData, function (response) {
            $('#divContainer').html(response);
        });
    },

    fetchSalesBySubGroup: (category) => {
        console.log(`Fetch sub-group sales for '${category}' category`);

        // hide main back
        dbCommon.BackMainDb(false);

        partCategory = category;

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory
        };

        dbSale.setBackLink('Main', 'Back to all sales', true);
        dbSale.showLoader(salesSubGroup);
        dbCommon.request(dashboardController + 'FetchSalesBySubGroup', dbFilter, function (response) {
            $('#divContainer').html(response);
        });
    },

    fetchSaleDetailForSubGroup: (subGroup) => {
        console.log(`Table 1 - Fetch sale detail for '${subGroup}' sub group`);

        dbSale.setBackLink('SubGroup', 'Back to sub group sales', true);

        let subGroupSaleDetail = '';
        let actionMethod = '';

        switch (subGroup) {
            case 'RO':
                actionMethod = 'FetchRoWiseSaleDetail';
                subGroupSaleDetail = roSalesDetail;
                break;

            case 'SE':
                actionMethod = 'FetchSeWiseSaleDetail';
                subGroupSaleDetail = seSalesDetail;
                break;

            case 'CS':
                actionMethod = 'FetchCsWiseSaleDetail';
                subGroupSaleDetail = csSalesDetail;
                break;

            case 'PG':
                actionMethod = 'FetchPgWiseSaleDetail';
                subGroupSaleDetail = pgSalesDetail;
                break;
        }

        if (actionMethod) {
            const dbFilter = {
                DateType: filterData.DateType,
                StartDate: filterData.StartDate,
                EndDate: filterData.EndDate,
                Category: partCategory
            };

            dbSale.showLoader(subGroupSaleDetail);
            dbCommon.request(dashboardController + actionMethod, dbFilter, function (response) {
                $('#divContainer').html(response);

                switch (subGroup) {
                    case 'RO':
                        dbSale.fetchRoWiseBranchSaleDetail();
                        break;

                    case 'SE':
                        dbSale.fetchSeWiseBranchSaleDetail();
                        break;

                    case 'PG':
                        pgTable = null;
                        dbCommon.applyDataTable();
                        break;
                }
            });
        } else {
            console.error('No action method was set.');
        }
    },

    // #region RO wise sale detail

    fetchRoWiseBranchSaleDetail: () => {
        console.log(`Table 2 - Fetch RO wise branch sale detail`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory
        };

        dbSale.showLoader(roWiseBranchSaleDetail);
        dbCommon.request(dashboardController + 'FetchRoWiseBranchSaleDetail', dbFilter, function (response) {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchRoWiseBranchCustomerSaleDetail: (branchCode) => {
        console.log(`Table 3 - Fetch RO wise branch customer sale detail for '${branchCode}' branch code`);

        dbSale.setBackLink('RO', 'Back to RO wise sales', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            BranchCode: branchCode
        };

        dbSale.showLoader(roWiseBranchCustomerSaleDetail);
        dbCommon.request(dashboardController + 'FetchRoWiseBranchCustomerSaleDetail', dbFilter, function (response) {
            $('#divContainer').html(response);

            dbSale.fetchRoWiseCustomerSaleDetail(branchCode, '');
        });
    },

    fetchRoWiseCustomerSaleDetail: (branchCode, customerType) => {
        console.log(`Table 4 - Fetch RO wise customer sale detail for '${branchCode}' branch code and '${customerType}' customer type`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            BranchCode: branchCode,
            CustomerType: customerType
        };

        dbSale.showLoader(roWiseCustomerSaleDetail);
        dbSale.fetchCustomerDetail('RO', dbFilter);
    },

    // #endregion

    // #region SE wise sale detail

    fetchSeWiseBranchSaleDetail: () => {
        console.log(`Table 2 - Fetch SE wise branch sale detail`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory
        };

        dbSale.showLoader(seWiseBranchSaleDetail);
        dbCommon.request(dashboardController + 'FetchSeWiseBranchSaleDetail', dbFilter, function (response) {
            $('#divSubContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchSeWiseBranchCustomerSaleDetail: (branchCode) => {
        console.log(`Table 3 - Fetch SE wise branch customer sale detail for '${branchCode}' branch code`);

        dbSale.setBackLink('SE', 'Back to SE wise sales', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            BranchCode: branchCode
        };

        dbSale.showLoader(seWiseBranchCustomerSaleDetail);
        dbCommon.request(dashboardController + 'FetchSeWiseBranchCustomerSaleDetail', dbFilter, function (response) {
            $('#divContainer').html(response);

            dbSale.fetchSeWiseCustomerSaleDetail(branchCode, '');
        });
    },

    fetchSeWiseCustomerSaleDetail: (branchCode, customerType) => {
        console.log(`Table 4 - Fetch SE wise customer sale detail for '${branchCode}' branch code and '${customerType}' customer type`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            BranchCode: branchCode,
            CustomerType: customerType
        };

        dbSale.showLoader(seWiseCustomerSaleDetail);
        dbSale.fetchCustomerDetail('SE', dbFilter);
    },

    // #endregion

    // #region CS wise sale detail

    fetchCsWiseCustomerSaleDetail: (customerType) => {
        console.log(`Table 2 - Fetch CS wise customer sale detail for '${customerType}' customer type`);

        dbSale.setBackLink('CS', 'Back to CS wise sales', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            CustomerType: customerType
        };

        dbSale.showLoader(csWiseCustomerSaleDetail);
        dbSale.fetchCustomerDetail('CS', dbFilter);
    },

    // #endregion

    // #region PG wise sale detail

    fetchPgWisePartGroupSaleDetail: (partGroup, anchor) => {
        console.log(`Table 2 - Fetch PG wise sale detail for '${partGroup}' part group`);

        dbSale.setBackLink('PG', 'Back to PG wise sales', true);

        partGroupName = partGroup;
        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            PartGroupName: partGroupName
        };

        if (!pgTable) {
            const currentRow = $(anchor).closest('tr');
            pgTable = `<div class="table-responsive pb-2" id="pgSelectedGroupType">
                        <table class="table table-bordered w-auto table-center">
                        <thead class="thead-light">
                            <tr>
                                <th>Sr No</th>
                                <th>Customer Type</th>
                                <th>No of Customers</th>
                                <th>Avg Sale (3 Months)</th>
                                <th>Current Sale</th>
                                <th>% Contribution</th>
                                <th>PY ${dbFilter.DateType}</th>
                            </tr>
                        </thead>
                        <tbody>
                        <tr>
                        <td>1</td>
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
        }
        $('#divContainer').html(pgTable);

        dbSale.showLoader(pgWisePartGroupSaleDetail);
        dbCommon.request(dashboardController + 'FetchPgWisePartGroupSaleDetail', dbFilter, function (response) {
            $('#divSubContainer').html(response);

            dbSale.fetchPgWiseBranchSaleDetail(partGroup, '');
        });
    },

    fetchPgWiseBranchSaleDetail: (partGroup, customerType) => {
        console.log(`Table 3 - Fetch PG wise branch sale detail for '${partGroup}' part group and '${customerType}' customer type`);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            PartGroupName: partGroup,
            CustomerType: customerType
        };

        dbSale.showLoader(pgWiseBranchSaleDetail);
        dbCommon.request(dashboardController + 'FetchPgWiseBranchSaleDetail', dbFilter, function (response) {
            $('#divLastContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchPgWiseBranchCustomerSaleDetail: (customerType, branchCode) => {
        console.log(`Table 4 - Fetch PG wise branch customer sale detail for '${customerType}' customer Type and '${branchCode}' branch code`);

        dbSale.setBackLink('PGP', 'Back to PG wise part group sales', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            PartGroupName: partGroupName,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        dbSale.showLoader(pgWiseBranchCustomerSaleDetail);
        dbCommon.request(dashboardController + 'FetchPgWiseBranchCustomerSaleDetail', dbFilter, function (response) {
            $('#divContainer').html(response);

            dbSale.fetchPgWiseCustomerSaleDetail(customerType, branchCode);
        });
    },

    fetchPgWiseCustomerSaleDetail: (customerType, branchCode) => {
        console.log(`Table 5 - Fetch PG wise customer sale detail for '${customerType}' customer type and '${branchCode}' branch code `);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            Category: partCategory,
            PartGroupName: partGroupName,
            CustomerType: customerType,
            BranchCode: branchCode
        };

        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>
                                    <th>Avg Sale (3 Months)</th>
                                    <th>Current Sale</th>
                                    <th>% Contribution</th>                        
                                    <th>PY ${dbFilter.DateType}</th>                                                                      
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
        const columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'AverageSale' },
            { "data": 'NetRetailSelling' },
            { "data": 'ContributionTxt' },
            { "data": 'PrevAchieved' }
        ];

        // Create set footer function
        const setFooter = function (api, intVal) {
            // api is DataTable method
            // intVal is method defined in db.common.js

            const totalAvgSale = api.column(4).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalCurrentSale = api.column(5).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalContribution = api.column(6).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);
            const totalPrvSale = api.column(7).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            $(api.column(3).footer()).html('<b>Total</b>');
            $(api.column(4).footer()).html(`<b>${totalAvgSale.toFixed(2)}</b>`);
            $(api.column(5).footer()).html(`<b>${totalCurrentSale.toFixed(2)}</b>`);
            $(api.column(6).footer()).html(`<b>${totalContribution.toFixed(2)} %</b>`);
            $(api.column(7).footer()).html(`<b>${totalPrvSale.toFixed(2)}</b>`);
        };

        $('#divSubContainer').html(table);
        dbCommon.bindDataToTable(`${dashboardController}FetchPgWiseCustomerSaleDetail`, dbFilter, columns, '#customerDetail', '#divSubContainer', setFooter);
    },

    // #endregion

    fetchCustomerDetail: (subGroup, dbFilter) => {
        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="customerDetail">
                            <thead class="thead-light">
                                <tr>
                                    <th>Sr No</th>
                                    <th>Customer Code</th>
                                    <th>Customer Name</th>
                                    <th>Customer Type</th>
                                    <th>Achieved</th>
                                    <th>Achieved %</th>
                                    <th>PY ${dbFilter.DateType}</th>  
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
                                </tr>
	                        </tfoot>
                        </table>
                    </div>`;

        // Prepare columns
        const columns = [
            { "data": 'SlNo' },
            { "data": 'CustomerCode' },
            { "data": 'CustomerName' },
            { "data": 'CustomerType' },
            { "data": 'Achieved' },
            { "data": 'AchievedPercentage' },
            { "data": 'PrevAchieved' }
        ];

        // Create set footer function
        const setFooter = function (api, intVal) {
            // api is DataTable method
            // intVal is method defined in db.common.js

            let totalAchieved = api.column(4).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            const totalPrvSale = api.column(6).data().reduce(function (a, b) { return intVal(a) + intVal(b); }, 0);

            let totalAchievedPer = parseFloat(totalAchieved * 100 / (totalPrvSale == 0 ? 100 : totalPrvSale));

            $(api.column(1).footer()).html('<b>Total</b>');
            $(api.column(4).footer()).html(`<b>${totalAchieved.toFixed(2)}</b>`);
            $(api.column(5).footer()).html(`<b>${totalAchievedPer.toFixed(2)}%</b>`);
            $(api.column(6).footer()).html(`<b>${totalPrvSale.toFixed(2)}</b>`);
        };

        switch (subGroup) {
            case 'RO':
                $('#divSubContainer').html(table);
                dbCommon.bindDataToTable(`${dashboardController}FetchRoWiseCustomerSaleDetail`, dbFilter, columns, '#customerDetail', '#divSubContainer', setFooter);
                break;

            case 'SE':
                $('#divSubContainer').html(table);
                dbCommon.bindDataToTable(`${dashboardController}FetchSeWiseCustomerSaleDetail`, dbFilter, columns, '#customerDetail', '#divSubContainer', setFooter);
                break;

            case 'CS':
                $('#divContainer').html(table);
                dbCommon.bindDataToTable(`${dashboardController}FetchCsWiseCustomerSaleDetail`, dbFilter, columns, '#customerDetail', '#divContainer', setFooter);
                break;
        }
    },
    calculateTotal: () => {
       
        var totalCustomers = 0;
        var totalAvgSale = 0.0;
        var totalAchieved = 0.0;
        var totalPreAchieved = 0.0;
               
        const tr = $('#Customertable tbody tr');
        tr.find('td input[type="checkbox"]:checked').each(function () {
            let td = $(this).parent().parent();
            let cust = parseInt(td.find('.NumberOfCustomers').text());
            if (!isNaN(cust)) { totalCustomers += cust; }

            let avgSale = parseFloat(td.find('.AverageSale').text());
            if (!isNaN(avgSale)) { totalAvgSale += avgSale; }

            let achieved = parseFloat(td.find('.Achieved').text());
            if (!isNaN(achieved)) { totalAchieved += achieved; }

            let preAchieved = parseFloat(td.find('.PrevAchieved').text());
            if (!isNaN(preAchieved)) { totalPreAchieved += preAchieved; }
        });
        var totalAchievedPer = totalAchieved * 100 / (totalPreAchieved == 0 ? 100 : totalPreAchieved);
        var footer = `<tr>
                          <td></td>
                          <td><b>Total</b></td>
                          <td><b>${totalCustomers}</b></td>
                          <td><b>${totalAvgSale.toFixed(2)}</b></td>
                          <td><b>${totalAchieved.toFixed(2)}</b></td>
                          <td><b>${totalAchievedPer.toFixed(2)}%</b></td>
                          <td><b>${totalPreAchieved.toFixed(2)}</b></td>
                          <td></td>
                      </tr>`;
        $('#Customertable tfoot').html(footer);
    }
};