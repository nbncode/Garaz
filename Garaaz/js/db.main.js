'use strict';

// Global variables
var filterData = null;

const dashboardController = dbCommon.getDashboardUrl();


const dateTypeMtd = 'MTD', dateTypeCd = 'CustomDate';
const sale = 'sale', outstanding = 'outstanding', collection = 'collection', inventory = 'inventory', cbo = 'cbo', scheme = 'scheme', walletBal = 'walletBalance', customer = 'customer', loserAndGainer = 'loserAndGainer';

// Variables for local storage
const locDateType = 'dateType', locStartDate = 'StartDate', locEndDate = 'EndDate',
    locSales = 'SaleData', locOutstanding = 'OutstandingData', locCollection = 'CollectionData', locInventory = 'Inventory', locCbo = 'CboDetails', locScheme = 'SchemeInfo', locWallet = 'WalletInfo', locCustomer = 'CustomerInfo', locLooserAndGainer = 'LooserAndGainers';

var dashboard = {

    init: () => {
        dbCommon.bindDatepicker();

        // set select date from session
        //dbCommon.getSessionValue(function () {
        //    dashboard.showLoader(sale);
        //    var dateType = $('#ddlDateFilter').val();
        //    if (dateType == dateTypeCd) { dashboard.applyFilter(); }
        //    else {
        //        // Fetch all info
        //        filterData = { DateType: dateType };
        //        dashboard.fetchAllInfo();
        //    }
        //});
    },

    applyFilter: () => {
        let startDate = $('#startDate').val();
        let endDate = $('#endDate').val();

        localStorage.setItem(locStartDate, startDate);
        localStorage.setItem(locEndDate, endDate);

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

                dashboard.fetchAllInfo();
            } else {
                common.LoadErrorMessage('End Date must be greater than Start Date');
            }

        } else {
            common.LoadErrorMessage('Start Date and End Date cannot be blank.');
        }
    },

    showLoader: (box) => {
        let div = '';
        let loadMsg = '';
        let promise;

        switch (box) {
            case sale:
                div = $('#divSalesInfo');
                loadMsg = 'Loading sale info...';
                break;

            case outstanding:
                div = $('#divOutstandingInfo');
                loadMsg = 'Loading outstanding info...';
                break;

            case collection:
                div = $('#divCollectionInfo');
                loadMsg = 'Loading collection info...';
                break;

            case inventory:
                div = $('#divInventoryInfo');
                loadMsg = 'Loading inventory info...';
                break;

            case cbo:
                div = $('#divCboInfo');
                loadMsg = 'Loading cbo info...';
                break;

            case scheme:
                div = $('#divSchemeInfo');
                loadMsg = 'Loading scheme info...';
                break;

            case walletBal:
                div = $('#divWalletBalanceInfo');
                loadMsg = 'Loading wallet balance info...';
                break;

            case customer:
                div = $('#divCustomerInfo');
                loadMsg = 'Loading customers info...';
                break;

            case loserAndGainer:
                div = $('#divLoserAndGainerInfo');
                loadMsg = 'Loading loser and gainer info...';
                break;
        }

        promise = new Promise(function (resolve, reject) {
            const loader = `<div class="loader"><div class="kt-spinner home-page kt-spinner--v2 kt-spinner--md kt-spinner--info" style="display: block;"></div><span>${loadMsg}</span></div>`;
            div.html(loader);
            resolve(true);
        });
        return promise;
    },

    OptionChange: () => {
        let option = $('#ddlDateFilter').val();
        $('#divStartDate').toggle(option === dateTypeCd);
        $('#divEndDate').toggle(option === dateTypeCd);
    },

    fetchAllByDateFilter: () => {
        const option = $('#ddlDateFilter').val();

        localStorage.setItem(locDateType, option);

        dbCommon.setSessionValue('Dashboard_DateType', option);

        $('#divStartDate').toggle(option === dateTypeCd);
        $('#divEndDate').toggle(option === dateTypeCd);
        $('#divFilter').toggle(option === dateTypeCd);

        // Fetch all if selected option is not 'custom date'
        if (option !== dateTypeCd) {
            filterData = { DateType: option };
            dashboard.fetchAllInfo();
        }
        else {
            dashboard.applyFilter();
        }
    },

    fetchAllInfo: () => {
        console.group('Fetching all info');
        dashboard.fetchWalletBalanceInfo();
        dashboard.fetchCboInfo();

        dashboard.fetchCustomerInfo();
        dashboard.fetchLoserAndGainerInfo();

        dashboard.fetchSaleInfo();
        dashboard.fetchOutstandingInfo();
        dashboard.fetchCollectionInfo();
        dashboard.fetchInventoryInfo();

        dashboard.fetchSchemeInfo();


        console.groupEnd();
    },

    GetAllInfoFromLocalStorage: () => {
        common.showLoader();
        // Date filter data
        if (localStorage.getItem(locDateType) != null) {
            let dateType = localStorage.getItem(locDateType);
            $('#ddlDateFilter').val(dateType);
            $('#divStartDate').toggle(dateType === dateTypeCd);
            $('#divEndDate').toggle(dateType === dateTypeCd);
            $('#divFilter').toggle(dateType === dateTypeCd);
            if (dateType == dateTypeCd) {
                $('#startDate').val(localStorage.getItem(locStartDate));
                $('#endDate').val(localStorage.getItem(locEndDate));
            }
        }

        if (localStorage.getItem(locSales) != null) {
            $('#divSalesInfo').html(localStorage.getItem(locSales));
        }

        if (localStorage.getItem(locOutstanding) != null) {
            $('#divOutstandingInfo').html(localStorage.getItem(locOutstanding));
        }

        if (localStorage.getItem(locCollection) != null) {
            $('#divCollectionInfo').html(localStorage.getItem(locCollection));
        }

        if (localStorage.getItem(locInventory) != null) {
            $('#divInventoryInfo').html(localStorage.getItem(locInventory));
        }

        if (localStorage.getItem(locCbo) != null) {
            $('#divCboInfo').html(localStorage.getItem(locCbo));
        }

        if (localStorage.getItem(locScheme) != null) {
            $('#divSchemeInfo').html(localStorage.getItem(locScheme));
        }

        if (localStorage.getItem(locWallet) != null) {
            $('#divWalletBalanceInfo').html(localStorage.getItem(locWallet));
        }

        if (localStorage.getItem(locCustomer) != null) {
            $('#divCustomerInfo').html(localStorage.getItem(locCustomer));
        }

        if (localStorage.getItem(locLooserAndGainer) != null) {
            $('#divLoserAndGainerInfo').html(localStorage.getItem(locLooserAndGainer));
        }
        common.hideLoader();
    },

    fetchSaleInfo: () => {
        console.log('Fetching sale info');

        dashboard.showLoader(sale);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchSaleInfo', filterData, function (response) {
                $('#divSalesInfo').html(response);
                localStorage.setItem(locSales, response);
            });
        });
    },

    fetchOutstandingInfo: () => {
        console.log('Fetching outstanding info');

        dashboard.showLoader(outstanding);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchOutstandingInfo', filterData, function (response) {
                $('#divOutstandingInfo').html(response);
                localStorage.setItem(locOutstanding, response);
            });
        });
    },

    fetchCollectionInfo: () => {
        console.log('Fetching collection info');

        dashboard.showLoader(collection);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchCollectionInfo', filterData, function (response) {
                $('#divCollectionInfo').html(response);
                localStorage.setItem(locCollection, response);
            });
        });
    },

    fetchInventoryInfo: () => {
        console.log('Fetching inventory info');

        dashboard.showLoader(inventory);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchInventoryInfo', filterData, function (response) {
                $('#divInventoryInfo').html(response);
                localStorage.setItem(locInventory, response);
            });
        });
    },

    fetchCboInfo: () => {
        console.log('Fetching cbo info');

        dashboard.showLoader(cbo);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchCboInfo', filterData, function (response) {
                $('#divCboInfo').html(response);
                localStorage.setItem(locCbo, response);
            });
        });
    },

    fetchSchemeInfo: () => {
        console.log('Fetching scheme info');

        dashboard.showLoader(scheme);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchSchemeInfo', filterData, function (response) {
                $('#divSchemeInfo').html(response);
                localStorage.setItem(locScheme, response);
            });
        });
    },

    fetchWalletBalanceInfo: () => {
        console.log('Fetching wallet balance info');

        dashboard.showLoader(walletBal);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchWalletBalanceInfo', filterData, function (response) {
                $('#divWalletBalanceInfo').html(response);
                localStorage.setItem(locWallet, response);
            });
        });
    },

    fetchCustomerInfo: () => {
        console.log('Fetching billing and non-billing customers info');

        dashboard.showLoader(customer);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchCustomerInfo', filterData, function (response) {
                $('#divCustomerInfo').html(response);
                localStorage.setItem(locCustomer, response);
            });
        });
    },

    fetchLoserAndGainerInfo: () => {
        console.log('Fetching losers and gainers info');

        dashboard.showLoader(loserAndGainer);
        setTimeout(function () {
            dbCommon.requestSync(dashboardController + 'FetchLoserAndGainerInfo', filterData, function (response) {
                $('#divLoserAndGainerInfo').html(response);
                localStorage.setItem(locLooserAndGainer, response);
            });
        });
    }
};