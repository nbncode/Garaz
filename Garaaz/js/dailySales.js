var sales = {
    init: function () {
        $(".datepicker-txt").datepicker();
        var url = $("#PageView").val();
        //sales.LoadPartialView(url);
        //$("#btnShow").click(function () {
        //    sales.BindDataTable();
        //});
    },
    LoadPartialView: function (partail) {
        doAjax(partail, null, function (data) {
            $('#main-content').html(data);
        });
    },

    ShowDatabyFiler: function (partail) {
        var workshopId = $("#workshopId").val();
        var startDate = $("#FromDate").val();
        var endDate = $("#ToDate").val();
        var fromAmt = $("#FromAmt").val();
        var toAmt = $("#ToAmt").val();

        partail = partail + "&startDate=" + startDate + "&endDate=" + endDate + "&fromAmt=" + fromAmt + "&toAmt=" + toAmt + "&workshopId=" + workshopId + "";
        doAjax(partail, null, function (data) {
            $('#main-content').html(data);
        });
    },

    BindDataTable: function () {

        var workshopId = $("#workshopId").val();
        var startDate = $("#FromDate").val();
        var endDate = $("#ToDate").val();
        var fromAmt = $("#FromAmt").val();
        var toAmt = $("#ToAmt").val();
        var distributorId = $("#distributorId").val();

        data = {
            "distributorId": distributorId,
            "startDate": startDate,
            "endDate": endDate,
            "fromAmt": fromAmt,
            "toAmt": toAmt,
            "workshopId": workshopId
        };

        var table = $('table.datatable').DataTable({
            "processing": true, // for show progress bar
            "serverSide": true, // for process server side
            "filter": false, // this is for disable filter (search box)
            "orderMulti": true, // for disable multiple column at once
            "destroy": true,
            responsive: true,
            "ajax": {
                "url": "/home/LoadDailySales", data,
                "type": "POST",
                "datatype": "json",
            },
            "columns": [
                { "data": "ConsPartyName" },
                { "data": "PartNum" },
                { "data": "PartGroup" },
                { "data": "RetailSelling" },
                { "data": "NetRetailSelling" },
                { "data": "DiscountAmount" },
                { "data": "CreatedDate" },
                { "data": "LocCode" },
                { "data": "LocDesc" },
                { "data": "ReturnQty" },
                { "data": "NetRetailQty" },
                { "data": "ReturnSelling" },
                { "data": "PartDesc" },
                { "data": "RootPartNum" },
                { "data": "PartCategory" },
                { "data": "ConsPartyCode" },
                { "data": "Region" },
                { "data": "DealerCode" },
                { "data": "ConsPartyTypeDesc" },
                { "data": "DocumentNum" },
                { "data": "Remarks" }
            ]

        });

        new $.fn.dataTable.FixedHeader(table);
    },

};