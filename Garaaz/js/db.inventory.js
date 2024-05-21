'use strict';

var filterData = null;
const dashboardController = dbCommon.getDashboardUrl();
const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';
const invDetail = 'invDetail', invForBranch = 'invForBranch';

var dbInv = {

    initInv: () =>
    {
        dbCommon.bindDatepicker();

        // set select date from session
        dbCommon.getSessionValue(function () {
            dbInv.showLoader(invDetail);
            var dateType = $('#ddlDateFilter').val();
            if (dateType == dateTypeCd) { dbInv.applyFilter(); }
            else {
                filterData = { DateType: dateType };
                dbInv.fetchInvByBranch();
            }
        });
        
    },

    showLoader: (tag) =>
    {
        let div = '';
        let loadMsg = '';

        switch (tag)
        {
            case invDetail:
                div = $('#divContainer');
                loadMsg = 'Loading inventory details...';
                break;

            case invForBranch:
                div = $('#divSubContainer');
                loadMsg = 'Loading inventory for branch...';
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

                dbInv.fetchInvByBranch();
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
                dbInv.fetchInvByBranch();
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

    fetchInvByDateFilter: () => 
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
            dbInv.fetchInvByBranch();
        }
    },

    fetchInvByBranch: () =>
    {
        console.log('Table 1 - Fetch inventories group by branch');

        // show main back
        dbCommon.BackMainDb(true);

        dbInv.setBackLink('', '', false);

        dbInv.showLoader(invDetail);
        dbCommon.request(dashboardController + 'FetchInvByBranch', filterData, function (response) 
        {
            $('#divContainer').html(response);
            dbCommon.applyDataTable();
        });
    },

    fetchInvForBranch: (branchCode, anchor) =>
    {
        console.log(`Table 2 - Fetch inventories for branch with '${branchCode}' branch code`);

        // hide main back
        dbCommon.BackMainDb(false);

        dbInv.setBackLink('Main', 'Back to all inventories', true);

        const dbFilter = {
            DateType: filterData.DateType,
            StartDate: filterData.StartDate,
            EndDate: filterData.EndDate,
            BranchCode: branchCode
        };

        // Selected inventory table
        const currentRow = $(anchor).closest('tr');
        var selInvTable = `<div class="table-responsive pb-2" id="selectedInventory">
                        <table class="table table-bordered w-auto table-center">
                        <thead class="thead-light">
                            <tr>
                                <th>Branch Code</th>
                                <th>Branch Name</th>
                                <th>Stock Days</th>
                                <th>Avg. Sales (Lacs)</th>
                                <th>Stock (In lacs)</th>
                                <th>No. of Partlines In Stock</th>
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
        $('#divContainer').html(selInvTable);

        // Create table
        var table = `<div class="table-responsive">
                        <table class="table table-bordered w-auto table-center" id="invForBranch">
                            <thead class="thead-light">
                                <tr>
                                   <th>Sr No</th>
                                   <th>Part Group</th>
                                   <th>Part Category</th>
                                   <th>Root Part Number</th>
                                   <th>Part Number</th>
                                   <th>Part Description</th>
                                   <th>MRP</th>
                                   <th>Avg. Consumption (3 Months)</th>
                                   <th>No of Stock</th>
                                </tr>
                            </thead>                             
                        </table>
                    </div>`;

        // Prepare columns
        var columns = [
            { "data": 'SlNo' },
            { "data": 'PartGroup' },
            { "data": 'PartCategory' },
            { "data": 'RootPartNumber' },
            { "data": 'PartNumber' },
            { "data": 'PartDescription' },
            { "data": 'Mrp' },
            { "data": 'AvgConsumption' },
            { "data": 'NumberOfStock' }
        ];

        $('#divSubContainer').html(table);
        dbCommon.bindDataToTable(`${dashboardController}FetchInvForBranch`, dbFilter, columns, '#invForBranch', '#divInvForBranch', null, true);
    }
};