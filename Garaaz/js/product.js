var product = function () {
    init = function () {
        bindFn();
    },
        bindFn = function () {
            $(".add-to-cart").click(function () {
                var pId = $(this).attr("product-id");
                var qty = "#txtQty_" + pId;
                var prodName = "#iProdName_" + pId;
                var price = "#iPrice_" + pId;
                var imgPath = "#iImagePath_" + pId;

                if ($(qty).val()) {
                    admin.AddToCart(pId, $(prodName).val(), $(price).val(), $(qty).val(), $(imgPath).val());
                }
                else {
                    alert('Please enter quantity.');
                }
            });
            product.bindICheck();
            product.unCheckRadio();
            product.BindOutletByDistributor();
        },
        bindICheck = function () {
            $('input.icheck_n').iCheck({
                checkboxClass: 'icheckbox_square-blue',
                radioClass: 'iradio_square-blue',
                increaseArea: '20%'
            });
        },
        unCheckRadio = function () {
            $('input.icheck_n').on('ifClicked', function () {
                $(this).iCheck('uncheck');
            });
            //$('input.icheck_n').on('ifChecked', function () {
            //    //alert("checked")
            //    $(this).iCheck('uncheck');
            //});
            //$('input.icheck_n').on('ifUnchecked', function () {
            //    //alert("unchecked")
            //    $(this).iCheck('uncheck');
            //});
        },
        UpdateCart = function () {
            var json = [];

            var num = 0;
            var len = $(".datatable.table tbody tr").length;

            $(".datatable.table tbody tr").each(function () {
                num = num + 1;
                var tr = $(this);
                var id = tr.attr('id');
                var qty = tr.find('.qty').val();
                var unitPrice = tr.find('.unit-price').val();
                var outletId = tr.find("#" + id + "OutletId").val();
                var productType = tr.find("input[name='" + id + "productType']:checked").val();

                var d = {
                    ProductId: id,
                    UnitPrice: unitPrice,
                    Quantity: qty,
                    OutletId: outletId == undefined ? null : outletId,
                    ProductAvailablityType: (productType == undefined ? "" : productType)
                };
                json.push(d);
                console.log(json);
                if (num == len) {
                    doAjaxPostJson(homeController + "UpdateShoppingCart", json, function (d) {
                        window.location.reload();
                    });
                }
            });

        },
        DeleteCart = function (productId) {
            var confirmation = confirm('Do you want to delete this item from your cart?');
            if (confirmation == true) {
                var dataToSend = { ProductID: productId };

                doAjaxPost(homeController + "DeleteFromCart", dataToSend, function (d) {
                    window.location.reload();
                });
            }
        },
        ActivateAutoComplete = function () {
            var input = document.getElementById('Address');

            var options = {
                types: ['(cities)'],
                componentRestrictions: { country: 'in' }
            };
            var autocomplete = new google.maps.places.Autocomplete(input, options);
            google.maps.event.addListener(autocomplete, 'place_changed', function () {
                var place = autocomplete.getPlace();
                if (!place.geometry) {

                    return;
                }

                lat = place.geometry.location.lat();
                lng = place.geometry.location.lng();
                $("#Latitude").val(lat);
                $("#Longitude").val(lng);
                console.log(lat, lng);
            });
        },
        UpdateStatus = function (controller) {
            var confirmation = confirm('Do you want to Update Status for this order?');
            if (confirmation == true) {
                var json = [];

                var num = 0;
                var len = $(".datatable.table tbody tr").length;

                $(".datatable.table tbody tr").each(function () {
                    num = num + 1;
                    var tr = $(this);
                    var orderStatus = tr.find('.status').val();

                    var d = {
                        OrderID: tr.attr("id"),
                        OrderStatus: orderStatus
                    };
                    json.push(d);

                    if (num === len) {
                        var url;
                        switch (controller.toLowerCase()) {
                            case "outlet":
                                url = outletController;
                                break;

                            case "distributor":
                                url = distributorController;
                                break;
                        }

                        doAjaxPostJson("/" + controller + "/" + "UpdateOrderStatus", json, function (d) {
                            if (d.ResultFlag === 1) {
                                window.location.reload();
                            }
                            else {
                                alert(d.Message);
                            }

                        });
                    }
                });
            }
        },
        DeleteFromOrder = function (OrderDetailID) {
            var confirmation = confirm('Do you want to delete this item from this order?');
            if (confirmation == true) {
                var dataToSend = { OrderDetailID: OrderDetailID };

                doAjaxPost(homeController + "DeleteFromOrder", dataToSend, function (d) {
                    if (d.ResultFlag === 1) {
                        window.location.reload();
                    }
                    else {
                        alert(d.Message);
                    }
                });
            }
        },
        UpdateQuantity = function (controller) {
            var json = [];

            var num = 0;
            var len = $(".table tbody tr").length;

            $(".table tbody tr").each(function () {
                num = num + 1;
                var tr = $(this);
                var productqty = tr.find('.qty').val();

                var d = {
                    OrderDetailId: tr.attr("id"),
                    Qty: productqty
                };
                json.push(d);

                if (num === len) {
                    var url;
                    switch (controller.toLowerCase()) {
                        case "outlet":
                            url = outletController;
                            break;

                        case "distributor":
                            url = distributorController;
                            break;
                    }

                    doAjaxPostJson("/" + controller + "/" + "UpdateQuantity", json, function (d) {
                        if (d.ResultFlag === 1) {
                            window.location.reload();
                        }
                        else {
                            alert(d.Message);
                        }

                    });
                }
            });
        },
        bindProductAvailablityType = function (th) {
            var outletId = $(th).find(":selected").attr("value");
            var productId = $(th).closest('tr').attr("id");
            if (outletId.length == 0) {
                $("#" + productId + "radio-blockCart").html('');
                return true;
            }
            var dataType = $(th).find(":selected").attr("data-type");
            if (dataType.length > 0) {
                dataType = dataType.split(',');
                console.log("dataType", dataType);
                var html = `<div class="radio-box">`
                for (var item in dataType) {
                    console.log("item", dataType[item]);
                    var checked = (dataType[item].toLowerCase() == "self pickup" || dataType[item].toLowerCase() == "Order To MSIL") ? "checked" : "";
                    html += `<input type="radio" ` + checked + ` name="` + productId + `productType" class="icheck_n name form-control ` + checked + `"  value="` + dataType[item] + `" />
                     <label style="margin-right: 10px;margin-left: 10px;">`+ dataType[item] + `</label>`
                }
                html += `</div >`
            }
            else {
                $("#" + productId + "radio-blockCart").html('');
                return;
            }
            $("#" + productId + "radio-blockCart").html(html);
            product.bindICheck();
            product.unCheckRadio();
        },
        generateOrder = function (orderId, ) {
            var orderDetails = [];
            var order = {
                OrderId: orderId,
                OrderDetails: orderDetails
            };

            if (confirm('The items with no Outlet selected will be moved to back orders. Are you sure you want to generate orders?')) {
                var num = 0;
                var len = $(".kt-invoice__body .kt-invoice__container .table tbody tr").length;
                var errorFound = false;

                $(".kt-invoice__body .kt-invoice__container .table tbody tr").each(function () {

                    if (!errorFound) {

                        num = num + 1;
                        var tr = $(this);

                        var odId = tr.attr("id");

                        var prodId = tr.find('.od-product').attr('prodId');
                        var prodNameandPart = tr.find('.od-product').html().trim();
                        var prodName = prodNameandPart.split('<br>,(')[0];
                       // var availabilityType = $("input[name='availabilityType" + odId + "']:checked").val();
                        var availabilityType = tr.find("#ddlavailabilityType" + odId).val();
                        var outletId = tr.find("#ddlOutlet" + odId).val();
                        var unitPrice = tr.find('.od-unit-price').html().trim();
                        var qty = tr.find('.qty').val();
                        var totalPrice = tr.find('.od-total-price').html().trim();

                        if (availabilityType == null || availabilityType=="") {
                            alert("Availability Type is not selected for one of the items. Please select Availability Type for all items.");
                            errorFound = true;
                        }

                        var orderDetail = {
                            OrderDetailID: odId,
                            ProductId: prodId,
                            ProductName: prodName,
                            UnitPrice: unitPrice,
                            Qty: qty,
                            TotalPrice: totalPrice,
                            OutletId: outletId,
                            AvailabilityType: availabilityType
                        };
                        order.OrderDetails.push(orderDetail);

                        if (num === len && !errorFound) {
                            console.log(JSON.stringify(order));
                            doAjaxPostJson(homeController + "GenerateOrder", order, function (d) {
                                if (d.ResultFlag === 0) {
                                    common.ShowMessage(d);
                                }
                                else {
                                    common.ShowMessage(d);
                                    window.setTimeout(function () { location.reload() }, 3000)
                                }
                            });
                          
                        }
                    }
                });
            } else {

            }
        },
        ApplyPromocode = function () {
            var code = null;
            code = $("#PromoCode").val();
            if (code == null || code == '') { common.LoadErrorMessage("Please Enter Coupon Code!"); }
            else {
                var d = {
                    TempOrderId: 0,
                    UserId: '',
                    CouponNo: code
                };
                doAjaxPostJson(homeController + "ApplyPromoCode", d, function (d) {
                    if (d.ResultFlag === 1) {
                        window.location.reload();
                        common.ShowMessage(d);
                    }
                    else {
                        common.LoadErrorMessage(d.Message);
                    }
                });
            }
        },
        RemovePromocode = function () {
            var confirmation = confirm('Do you want to remove this coupon ?');
            if (confirmation == true) {
                var code = null;
                code = $("#PromoCode").val();
                if (code == null) { common.LoadErrorMessage("Please Enter Coupon!"); }
                else {
                    var d = {
                        TempOrderId: 0,
                        UserId: '',
                        CouponNo: code
                    };
                    doAjaxPostJson(homeController + "RemovePromocode", d, function (d) {
                        if (d.ResultFlag === 1) {
                            window.location.reload();
                            common.ShowMessage(d);
                        }
                        else {
                            common.LoadErrorMessage(d.Message);
                        }
                    });
                }
            }
        },
        BindOutletByDistributor = function () {
            $('#DistributorId').on('change', function () {
                var distributorId = $(this).val();
                if (distributorId.length > 0) {
                    var data = { distributorId: distributorId };
                    doAjaxPost(homeController + "DistributorOutlet", data, function (d) {
                        console.log("distOutlet", d);
                        var html = "<div class='col-sm-6'><h2>Add Stock</h2>";
                        if (d.length) {
                            for (var item in d) {
                                console.log(d[item].OutletName);
                                html += `<div class='row'><div class="col-md-3">
                                                <div class="form-group">
                                                    <input type="checkbox" id="Outlet_`+ d[item].OutletId + `" class="icheck_n                                                    group" name="Outlet_` + d[item].OutletId + `" />
                                                    <span>`+ d[item].OutletName + `</span>
                                                </div>
                                            </div>
                                            <div class="col-md-2">
                                                <div class="form-group">
                                                    <input class="form-control" type="number" name="txt_`+ d[item].OutletId + `" />
                                                </div>
                                            </div></div>`
                            }
                            html += "</div></div>";
                            if (html.length) {
                                $(".outletlist").html(html);
                            }
                            else {
                                html = `<div class="col-md-4">
                                                <div class="form-group">
                                                    <span>No Outlet Found..</span>
                                                </div>
                                            </div>`
                                $(".outletlist").html(html);
                            }
                            product.bindICheck();
                        } else {
                            html = `<div class="col-md-4">
                                                <div class="form-group">
                                                    <span>No Outlet Found..</span>
                                                </div>
                                            </div>`
                            $(".outletlist").html(html);
                        }
                    });
                }

                product.BindDistributorBrand(distributorId);
            });
        },
        BindOutletonLoad = function (distId) {
            var distributorId = distId
            if (distributorId.length > 0) {
                var data = { distributorId: distributorId };
                doAjaxPost(homeController + "DistributorOutlet", data, function (d) {
                    console.log("distOutlet", d);
                    var html = "<div class='col-sm-6'><h2>Add Stock</h2>";
                    if (d.length) {
                        for (var item in d) {
                            console.log(d[item].OutletName);
                            html += `<div class='row'><div class="col-md-3">
                                                <div class="form-group">
                                                    <input type="checkbox" id="Outlet_`+ d[item].OutletId + `" class="icheck_n                                                    group" name="Outlet_` + d[item].OutletId + `" />
                                                    <span>`+ d[item].OutletName + `</span>
                                                </div>
                                            </div>
                                            <div class="col-md-2">
                                                <div class="form-group">
                                                    <input class="form-control" type="number" name="txt_`+ d[item].OutletId + `" />
                                                </div>
                                            </div></div>`
                        }
                        html += "</div></div>";
                        if (html.length) {
                            $(".outletlist").html(html);
                        }
                        else {
                            html = `<div class="col-md-4">
                                                <div class="form-group">
                                                    <span>No Outlet Found..</span>
                                                </div>
                                            </div>`
                            $(".outletlist").html(html);
                        }
                        product.bindICheck();
                    } else {
                        html = `<div class="col-md-4">
                                                <div class="form-group">
                                                    <span>No Outlet Found..</span>
                                                </div>
                                            </div>`
                        $(".outletlist").html(html);
                    }
                });
            }
        },
        BindDistributorBrand = function (distId) {
        brandId = $("#selectedBrand").val();
        var data = { distributorId: distId };
        doAjaxPost(homeController + "DistributorBrands", data, function (d) {
            console.log("distBrand", d);
            var html = `<option value="">-- Please Select --</option>`;
            if (d.length) {
                for (var item in d) {
                    console.log(item);
                    var selected = "";
                    if (brandId == d[item].BrandId)
                        selected = "selected";
                    html += `<option value="` + d[item].BrandId + `" ` + selected+`>` + d[item].Name + `</option>`
                }
            } 
            if (html.length) {
                $("#BrandId").html(html);
            }
        });
        }

    return {
        init: init,
        UpdateCart: UpdateCart,
        DeleteCart: DeleteCart,
        ActivateAutoComplete: ActivateAutoComplete,
        UpdateStatus: UpdateStatus,
        DeleteFromOrder: DeleteFromOrder,
        UpdateQuantity: UpdateQuantity,
        bindProductAvailablityType: bindProductAvailablityType,
        bindICheck: bindICheck,
        unCheckRadio: unCheckRadio,
        generateOrder: generateOrder,
        ApplyPromocode: ApplyPromocode,
        RemovePromocode: RemovePromocode,
        BindOutletByDistributor: BindOutletByDistributor,
        BindOutletonLoad: BindOutletonLoad,
        BindDistributorBrand: BindDistributorBrand
    };
}();