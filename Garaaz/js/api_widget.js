function InitWidget() {
    var oriparts_iframe = new OripartsIFrame();
    //alert(JSON.stringify(oriparts_iframe));
    oriparts_iframe.init(
        /*
        the id of client container that will contain widget frame
         */
        'widget_container',
        /*
        client password
         */
        'lZuJ3jrdg0TyP1yd',
        /*
        an object with extra data. it will be returned to client in response
         */
        { cartId: 12345 },
        /*
        callback that will be executed after response is got.
        response contains "auto_brand", "code", "name" elements and client extra data.
         */
        function (response) {
            console.log("response", response);
            //$("[data-element=parts-table]").append(
            //    "<tr>" +
            //    "<td>" + response.auto_brand + "</td>" +
            //    "<td>" + response.code + "</td>" +
            //    "<td>" + response.name + "</td>" +
            //    "<td>" + response.price + "</td>" +
            //    "<td><input type='button' value='Replacements' data-oriparts-iframe-action='getReplacements' data-id='" + response.id + "' data-is_b='" + response.is_b + "'></td>" +
            //    "</tr>"
            //);  

            AddProdToCart(response);
        },

        /*
         a callback function to process widget response for replacements. This function receives one
         parameter "response" and used to provide your own logic for visualizing replacements
          */
        function (response) {

            var dialog = $("<div data-oriparts-iframe-element='replacements-dialog'></div>"),
                table = $("<table data-oriparts-iframe-element='replacements-table' class='parts'></table>");

            $(table).append(
                "<tr>" +
                "        <th>Auto brand</th>" +
                "        <th>Part number</th>" +
                "        <th>Name</th>" +
                "        <th>Price</th>" +
                "        <th></th>" +
                "    </tr>"
            );

            $(table).append(
                "<tr><th colspan='5'>OEM</th></tr>"
            );

            if (response.oem.length > 0) {
                for (var i = 0; i < response.oem.length; i++) {

                    $(table).append(
                        "<tr>" +
                        "        <td>" + response.oem[i].auto_brand + "</td>" +
                        "        <td>" + response.oem[i].code + "</td>" +
                        "        <td>" + response.oem[i].name + "</td>" +
                        "        <td>" + response.oem[i].price + "</td>" +
                        "        <td><input type='button' value='add' data-oriparts-iframe-action='getReplacement' data-id='" + response.oem[i].id + "'></td>" +
                        "    </tr>"
                    );
                }
            } else {
                $(table).append(
                    "<tr><td colspan='5'>empty</td></tr>"
                );
            }

            $(table).append(
                "<tr><th colspan='5'>Aftermarket</th></tr>"
            );

            if (response.am.length > 0) {
                for (var i = 0; i < response.am.length; i++) {

                    $(table).append(
                        "<tr>" +
                        "        <td>" + response.am[i].auto_brand + "</td>" +
                        "        <td>" + response.am[i].code + "</td>" +
                        "        <td>" + response.am[i].name + "</td>" +
                        "        <td>" + response.am[i].price + "</td>" +
                        "        <td><input type='button' value='add' data-oriparts-iframe-action='getReplacement' data-id='" + response.am[i].id + "'></td>" +
                        "    </tr>"
                    );
                }
            } else {
                $(table).append(
                    "<tr><td colspan='5'>empty</td></tr>"
                );
            }

            $(dialog).append(table);
            $(dialog).dialog({
                minWidth: 550,
                maxWidth: 950
            });
        }
    );    

    //initialization of search with hints
    oriparts_iframe.initSearchWithHints("[data-oriparts-iframe-searchWithHints]");
}

function AddProdToCart(response) {
    common.showLoader();
    var url = "/Home/AddProductToCart";
    var data = {
        PartNumber: response.code,
        Price: response.price
    };

    $.ajax({
        type: "POST",
        data: JSON.stringify(data),
        contentType: 'application/json; charset=utf-8',
        url: url,
        success: function (d) {
            console.log(d.Message);
            common.hideLoader();
            common.ShowMessage(d);
        },
        error: function (xhr) {
            // Log error message
            var errorMessage = xhr.status + ': ' + xhr.statusText;
            console.log('%c ' + errorMessage, 'color: #FF0000; font-weight: bold;');
            console.log(xhr.responseText);

            alert("Some unexpected error occured.");
        }
    });
}