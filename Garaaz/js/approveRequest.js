'use strict';

var appReq =
{
    init: () =>
    {
        admin.LoadPartialView('ApprovePartRequestPartialView');
    },

    setAutoComplete: (controller) =>
    {
        // Ref - https://www.devbridge.com/sourcery/components/jquery-autocomplete/#jquery-autocomplete-response-format
        var requestId = $("#RequestedId").val();
        
        const url = `/${controller}/SearchPart?requestId=${requestId}`;
       // const url = `/${controller}/SearchPart`;
        console.log('Id: ' + requestId+'Url: '+url);
        const options = {
            serviceUrl: url,
            triggerSelectOnValidInput: false,
            showNoSuggestionNotice: 'true',
            noSuggestionNotice: 'No matching part found',
            preventBadQueries: true,
            deferRequestBy: 1000,
            minChars: 3,
            onSelect: function (suggestion)
            {
                //console.log(`You selected: ${suggestion.value}, ${suggestion.data}`);

                const data = suggestion.data.split('||');
                if (data.length > 0)
                {
                    const [partNumber, price] = data;

                    appReq.addSelectedPartToTable(partNumber, suggestion.value, price);
                    $('#txtSearchPart').val('');
                }
            }
        };

        $('#txtSearchPart').devbridgeAutocomplete(options);
    },

    addSelectedPartToTable: (partNumber, description, price) =>
    {
        var rows = $('#tblPartRequests > tbody > tr').length;
        const row = `<tr price="${price}"><td>${partNumber}</td><td>${description}</td><td><a class="table-trash-icon" title="Remove current part" onclick="appReq.removeCurrentPart(this)" href="javascript:void(0);"><i class="la la-trash"></i></a></td></tr>`;

        if (rows > 0)
        {
            $('#tblPartRequests tr:last').after(row);
        } else
        {
            $('#tblPartRequests').append(row);
        }

        $('#tblPartRequests').show();
    },

    removeCurrentPart: (node) =>
    {
        $(node).closest('tr').remove();
    },

    savePartsForPartRequest: (controller, requestId) =>
    {
        var rows = $('#tblPartRequests > tbody > tr').length;
        if (rows === 0)
        {
            common.LoadErrorMessage('Please add parts first');
            return;
        }

        // Prepare data to send with request
        const data = {
            RequestId: requestId,
            Parts: []
        };

        $('#tblPartRequests').find('tbody tr').each(function ()
        {
            const part = {
                PartNumber: $(this).find('td:eq(0)').text(),
                Description: $(this).find('td:eq(1)').text(),
                Price: $(this).attr('price')
            };

            data.Parts.push(part);
        });

        common.showLoader();
        const url = `/${controller}/SavePartsForPartRequest`;
        doAjaxPost(url, data, function (d)
        {
            common.hideLoader();
            if (d.ResultFlag === 1)
            {
                common.LoadSuccessMessage(d.Message);

            } else
            {
                common.LoadErrorMessage(d.Message);
            }
        });
    }
};