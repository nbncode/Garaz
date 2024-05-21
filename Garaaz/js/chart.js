let chart;

var gChart = {
    ShowSalesChart: function () {
        common.showLoader();
        var data = {
            GroupId: $('#ddlProductGroup').val(),
            StartDate: $('#startDate').val(),
            EndDate: $('#endDate').val(),
            Frequency: $('#ddlFrequency').val(),
            Growth: $('#ddlGrowth').val()
        };

        // Set hidden fields for using in comparison
        $('#hdnGroupId').val(data.GroupId);
        $('#hdnFrequency').val(data.Frequency);
        $('#hdnGrowth').val(data.Growth);

        if (data.GroupId && data.StartDate && data.EndDate && data.Frequency && data.Growth) {
            doAjaxPost(apiController + "GetSalesChartData", data, function (d) {
                common.hideLoader();

                // Reset previous sales
                sales = [];

                if (d.ResultFlag == 1) {
                    var labels = d.Data.Labels;
                    sales = d.Data.Sales;
                    var salesToCompare = [];
                    var title = d.Data.Title;
                    var xAxisLabel = d.Data.XAxisLabel;

                    gChart.DrawChart(labels, sales, salesToCompare, title, xAxisLabel);
                }
                else {
                    common.ShowMessage(d);
                }

            });
        }
        else {
            common.hideLoader();
            common.LoadErrorMessage("One of the filters is blank.");
        }

    },
    CompareSalesChart: function () {
        var data = {
            GroupId: $('#ddlProductGroup').val(),
            StartDate: $('#startDate').val(),
            EndDate: $('#endDate').val(),
            Frequency: $('#ddlFrequency').val(),
            Growth: $('#ddlGrowth').val()
        };

        // Get previous filters and check if matches with previous selection
        var prevGroupId = $('#hdnGroupId').val();
        var prevFrequency = $('#hdnFrequency').val();
        var prevGrowth = $('#hdnGrowth').val();

        //if (prevGroupId !== data.GroupId) {
        //    common.LoadErrorMessage("For comparison, the group should be same as previously selected group.");
        //    return false;
        //}
        if (prevFrequency !== data.Frequency) {
            common.LoadErrorMessage("For comparison, the frequency should be same as previously selected frequency.");
            return false;
        }
        if (prevGrowth !== data.Growth) {
            common.LoadErrorMessage("For comparison, the growth should be same as previously selected growth.");
            return false;
        }

        if (data.GroupId && data.StartDate && data.EndDate && data.Frequency && data.Growth) {
            common.showLoader();
            doAjaxPost(apiController + "GetSalesChartData", data, function (d) {
                common.hideLoader();

                if (d.ResultFlag == 1) {
                    var labels = d.Data.Labels;
                    var salesToCompare = d.Data.Sales;
                    var title = d.Data.Title;
                    var xAxisLabel = d.Data.XAxisLabel;

                    gChart.DrawChart(labels, sales, salesToCompare, title, xAxisLabel);
                }
                else {
                    common.ShowMessage(d);
                }
            });
        }
        else {
            common.hideLoader();
            common.LoadErrorMessage("One of the filters is blank.");
        }
    },
    DrawChart: function (labelsArr, sales, salesToCompare, titleText, xAxisLabel) {
        if (chart != undefined) {
            chart.destroy();
        }

        // Colours
        var red = 'rgb(255, 99, 132)';
        var blue = 'rgb(54, 162, 235)';

        var config = {

            type: 'line',
            data: {
                labels: labelsArr,
                datasets: [{
                    label: 'Sale 1',
                    fill: false,
                    backgroundColor: red,
                    borderColor: red,
                    data: sales
                }]
            },

            // Configuration options go here
            options: {
                responsive: true,
                maintainAspectRatio: true,
                scaleShowValues: true,
                title: {
                    display: true,
                    text: titleText
                },
                tooltips: {
                    mode: 'point',
                    intersect: true
                },
                hover: {
                    mode: 'point',
                    intersect: true
                },
                scales: {
                    xAxes: [{
                        display: true,
                        beginAtZero: true,
                        scaleLabel: {
                            display: true,
                            labelString: xAxisLabel,
                            fontColor: '#3498DB',
                            fontSize: 15
                        },
                        ticks: {
                            autoSkip: false, // To show all labels
                            stepSize: 1
                        }
                    }],
                    yAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Sales (in ₹)',
                            fontColor: '#3498DB',
                            fontSize: 15
                        }
                    }]
                }
                //events: [] // Disable all events
            }
        };

        var salesChart = $('#salesChart');
        chart = new Chart(salesChart, config);

        // If user is comparing, update the chart
        if (salesToCompare.length > 0) {
            var newDataset = {
                label: 'Sale 2',
                fill: false,
                backgroundColor: blue,
                borderColor: blue,
                data: salesToCompare
            };

            config.data.datasets.push(newDataset);
            chart.update();
        }
    }
};