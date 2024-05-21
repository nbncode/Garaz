var sales, schemeId, controller;
var workshopid, admin = {
    LoadAdminPartialView: function (partail) {
        console.log("partail", partail);
        doAjax(adminController + partail, null, function (data) {
            $('#main-content').html(data);
        });
    },
    LoadDistributorPartialView: function (partail) {
        doAjax(distributorController + partail, null, function (data) {
            $('#main-content').html(data);
        });
    },
    LoadPartialView: function (partail) {
        doAjax(partail, null, function (data) {
            $('#main-content').html(data);
        });
    },
    ApproveUser: function (id, Approved) {
        common.showLoader();
        var data = {
            UserId: $(id).parents("tr").attr("id"),
            Approved: Approved
        };
        doAjaxPost(apiController + "ApproveUser", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
            if (Approved) {
                $(id).parents("td").find("a.confirm").hide();
                $(id).parents("td").find("a.not-confirm").show();
            } else {
                $(id).parents("td").find("a.confirm").show();
                $(id).parents("td").find("a.not-confirm").hide();
            }
            if (d.ResultFlag) {
                admin.LoadPartialView($("#PageView").val());
            }
        });
    },
    DeleteUser: function (id, from) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            var data = {
                UserId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteUser", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                //if ($("#FromDis").length > 0) {
                //    admin.LoadDistributorPartialView($("#PageView").val());
                //} else {
                //    admin.LoadAdminPartialView($("#PageView").val());
                //}

            });
        }
    },

    DeleteFeature: function (id, from) {

        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            data = {
                FeatureId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteFeatures", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },

    DeleteProductGroup: function (id, from) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            data = {
                GroupId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteProductGroup", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },

    DeleteProduct: function (id, from) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            data = {
                ProductId: id//$(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteProduct", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },
    DeleteVehicle: function (id) {
        if (confirm('Do you want to delete ?')) {
            common.showLoader();
            data = {
                VehicleId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteVehicle", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },

    DeleteBrand: function (id, from) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            data = {
                BrandId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteBrand", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },
    DeleteVariant: function (id) {
        if (confirm('Do you want to delete ?')) {
            common.showLoader();
            data = {
                VariantId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteVariant", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },
    DeleteOutlet: function (id, from) {
        if (confirm('Do you want to delete this Outlet ?')) {
            common.showLoader();
            var data = {
                OutletId: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteOutlet", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());

            });
        }
    },

    LockUser: function (id, Locked) {
        common.showLoader();
        var data = {
            UserId: $(id).parents("tr").attr("id"),
            Locked: Locked
        };
        doAjaxPost(apiController + "LockUser", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
            if (Locked) {
                $(id).parents("td").find("a.lock").hide();
                $(id).parents("td").find("a.un-lock").show();
            } else {
                $(id).parents("td").find("a.lock").show();
                $(id).parents("td").find("a.un-lock").hide();
            }
        });
    },
    openDistributors: function (WorkShopId) {
        common.showLoader();
        var data = {
            WorkShopId: WorkShopId
        };
        doAjaxPost(adminController + "getDistributorsList", data, function (d) {
            common.hideLoader();
            $("#myDistributors").html(d);
            $("#myDistributors").modal();
        });
    },
    saveDistributors: function () {
        var id = $(".modal-dialog").attr("id");
        var checked = $(".modal-body .input-chk:checked");

        var Classes = "";
        $.each($(".modal-body .input-chk:checked"), function () {
            if (Classes === "") {
                Classes = $(this).val();
            } else {
                Classes = Classes + "," + $(this).val();
            }
        });

        if (Classes == "") {
            alert("Please select atleast one distributor");
        } else {
            common.showLoader();
            var data = {
                WorkShopId: id,
                Distributors: Classes
            };
            doAjaxPost(adminController + "saveDistributorsList", data, function (d) {
                common.hideLoader();
                $("#myDistributors").modal('hide');
                common.ShowMessage(d);
            });
        }
    },
    SetValueToInput: function () {
        var auth = $("#access_token").val();
        $("#tags").autocomplete({
            minLength: 0,
            source: function (request, response) {
                $.ajax({
                    type: "POST",
                    data: {
                        searchText: request.term
                    },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'bearer ' + auth);
                    },
                    url: apiController + "WorkShopList",
                    success: function (data) {
                        response(data.Data);
                    }
                });
            },
            focus: function (event, ui) {

                $("#tags").val(ui.item.Name);
                return false;
            },
            select: function (event, ui) {

                workshopid = ui.item.value;
                $("#tags").val(ui.item.Name);
                $("#tags-id").val(ui.item.value);

                return false;
            }
        }).data("ui-autocomplete")._renderItem = function (ul, item) {
            return $("<li>").data("ui-autocomplete-item", item).append("<span>" + item.Name + "</span>").appendTo(ul);
        };

    },
    SubmitWorkShops: function (DistributorId, tags) {
        if (tags) {
            common.showLoader();
            var data = {
                DistributorId: DistributorId,
                WorkshopId: workshopid
            };
            doAjaxPost(apiController + "AddWorkShopDistributorUser", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
            });
        }
        else {
            var json = {
                ResultFlag: 0,
                Message: "No workshop was selected."
            };
            common.ShowMessage(json);
        }
    },
    SubmitUserFeatures: function (userId) {

        var featureIds = "";
        $(".icheck_n:checked").each(function (index) {
            var val = $(this).val();
            featureIds += featureIds.length > 0 ? "," + val : val;
        });

        var data = {
            UserId: userId,
            FeatureIds: featureIds
        };
        doAjaxPost(apiController + "SaveUserFeatures", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    ApproveNewUser: function (id, userId, Approved) {
        common.showLoader();
        var data = {
            UserId: userId,
            Approved: Approved
        };
        doAjaxPost(apiController + "ApproveUser", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
            if (Approved) {
                $(id).parents("div").find("a.confirm").hide();
                $(id).parents("div").find("a.not-confirm").show();
            } else {
                $(id).parents("div").find("a.confirm").show();
                $(id).parents("div").find("a.not-confirm").hide();
            }
        });
    },
    SaveDistributorForWorkshop: function (refUserId, workshopId, distributorId) {
        common.showLoader();
        var data = {
            RefUserId: refUserId,
            WorkshopId: workshopId,
            DistributorId: distributorId
        };
        doAjaxPost(apiController + "SaveDistributorForWorkshop", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    GetVehicleByBrandId: function (brandId, edit) {
        common.showLoader();
        var data = { BrandId: brandId };
        doAjaxPost(apiController + "GetVehiclesByBrandID", data, function (d) {
            common.hideLoader();

            $("#ddlVehicles").empty(); // Else values will be appended to existing one
            $('select#ddlVehicles').append('<option value="0">-- Please Select --</option>');
            $.each(d.Data, function (key, vehicle) {
                $('select#ddlVehicles').append(
                    '<option value="' + vehicle.VehicleId + '">'
                    + vehicle.VehicleName +
                    '</option>');
            });

            // Select the vehicle id as per database record
            if (edit) {
                var selectedVehicle = $('#iVehicleId').val();
                console.log("Selected vehicle: " + selectedVehicle);
                $('select[name^="VehicleId"] option[value="' + selectedVehicle + '"]').attr("selected", "selected");
            }
        });
    },
    GetPGVehicleByBrandId: function (brandId, edit) {
        common.showLoader();
        var data = { BrandId: brandId };
        doAjaxPost(apiController + "GetVehiclesByBrandID", data, function (d) {
            common.hideLoader();

            $("#ddlVehicles").empty(); // Else values will be appended to existing one
            $('select#ddlVehicles').append('<option value="0">-- Please Select --</option>');
            $.each(d.Data, function (key, vehicle) {
                $('select#ddlVehicles').append(
                    '<option value="' + vehicle.VehicleId + '">'
                    + vehicle.VehicleName +
                    '</option>');
            });

            // Select the vehicle id as per database record
            if (edit) {
                var selectedVehicle = $('#iVehicleId').val();
                console.log("Selected vehicle: " + selectedVehicle);
                $('select[name^="VehicleId"] option[value="' + selectedVehicle + '"]').attr("selected", "selected");

                var vehicleId = $('select#ddlVehicles').val();
                admin.GetVariantsByVehicelId(vehicleId, true);
            }
        });
    },
    GetVariantsByVehicelId: function (vehicleId, edit) {
        common.showLoader();
        var data = { VehicleId: vehicleId };
        doAjaxPost(apiController + "GetVariantByVehicleID", data, function (d) {
            common.hideLoader();

            $("#ddlVariants").empty(); // Else values will be appended to existing one
            $('select#ddlVariants').append('<option value="0">-- Please Select --</option>');
            $.each(d.Data, function (key, variant) {
                $('select#ddlVariants').append(
                    '<option value="' + variant.VariantId + '">'
                    + variant.VariantName +
                    '</option>');
            });

            // Select the variant id as per database record
            if (edit) {
                var selectedVariant = $('#iVariantId').val();
                console.log("Selected variant: " + selectedVariant);
                $('select[name^="VariantId"] option[value="' + selectedVariant + '"]').attr("selected", "selected");

                var variantId = $('select#ddlVariants').val();
                admin.GetGroupsByVariantId(variantId, true);
            }
        });
    },
    GetGroupsByVariantId: function (variantId, edit) {
        var level = 1;
        common.showLoader();
        var data = { VariantId: variantId };
        doAjaxPost(apiController + "GetProductGroupsByVariantID", data, function (d) {
            common.hideLoader();

            $("#ddlGroups").empty(); // Else values will be appended to existing one
            $('select#ddlGroups').append('<option value="0">-- Please Select --</option>');
            $.each(d.Data, function (key, group) {
                level = 1;
                $('select#ddlGroups').append(
                    '<option value="' + group.GroupId + '">'
                    + group.GroupName +
                    '</option>');

                if (group.childs.length > 0) {
                    admin.getChildOfGroups(group.childs, level);
                }

            });

            // Select the group id as per database record
            if (edit) {
                var selectedGroup = $('#iParentId').val();
                $('select[name^="ParentId"] option[value="' + selectedGroup + '"]').attr("selected", "selected");
                $('select[name^="GroupId"] option[value="' + selectedGroup + '"]').attr("selected", "selected");
            }
        });
    },
    getChildOfGroups: function (data, level) {
        level = level + 1;
        var txt = "";
        for (var i = 0; i < level; i++) {
            txt += " &nbsp;&nbsp; ";
        }
        txt = txt + "--";
        $.each(data, function (key, group) {
            $('select#ddlGroups').append(
                '<option value="' + group.GroupId + '">' +
                txt + group.GroupName +
                '</option>');

            if (group.childs.length > 0) {
                admin.getChildOfGroups(group.childs, level);
            }
        });
    },
    AddToCart: function (pId, pName, pPrice, qty, imgPath) {
        common.showLoader();
        var data = {
            ProductID: pId,
            ProductName: pName,
            UnitPrice: pPrice,
            Qty: qty,
            ImagePath: imgPath
        };
        doAjaxPost(homeController + "AddToCart", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
            // Handle event to display adding to cart
        });
    },
    SubmitUserWorkshops: function (distributorId, userId) {

        var workshopIds = "";
        $(".icheck_n:checked").each(function (index) {
            var val = $(this).val();
            workshopIds += workshopIds.length > 0 ? "," + val : val;
        });

        var data = {
            DistributorId: distributorId,
            UserId: userId,
            WorkshopIds: workshopIds
        };
        doAjaxPost(adminController + "SaveUserWorkshops", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    DeleteLocation: function (id, controller) {
        if (confirm('Do you want to delete location ?')) {
            common.showLoader();
            var data = {
                LocationId: $(id).parents("tr").attr("id")
            };

            var url;
            switch (controller.toLowerCase()) {
                case "admin":
                    url = adminController;
                    break;

                case "distributor":
                    url = distributorController;
                    break;
            }

            doAjaxPost(url + "DeleteLocation", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
            });

        }
    },
    SubmitSalesExecutives: function (userId, ctrl) {

        var seUserIds = "";
        $(".icheck_n:checked").each(function (index) {
            var val = $(this).val();
            seUserIds += seUserIds.length > 0 ? "," + val : val;
        });

        var data = {
            RoUserId: userId,
            SeUserIds: seUserIds
        };
        doAjaxPost("/" + ctrl + "/" + "SaveSalesExecutives", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    SubmitSeWorkshops: function (userId, ctrl) {

        var workshopIds = "";
        $(".icheck_n:checked").each(function (index) {
            var val = $(this).val();
            workshopIds += workshopIds.length > 0 ? "," + val : val;
        });

        var data = {
            UserId: userId,
            WorkshopIds: workshopIds
        };
        doAjaxPost("/" + ctrl + "/" + "SaveSeWorkshops", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    SubmitStockColor: function () {

        var lowStockQty = $('#lowStockQty').val();
        var medStockQty = $('#mediumStockQty').val();
        var lowStockColor = $('#lowStockColor').val();
        var medStockColor = $('#mediumStockColor').val();
        var highStockColor = $('#highStockColor').val();
        var lowStockTag = $('#lowStockTag').val();
        var medStockTag = $('#mediumStockTag').val();
        var highStockTag = $('#highStockTag').val();

        if (lowStockQty && medStockQty && lowStockColor && medStockColor && highStockColor && lowStockTag && medStockTag && highStockTag) {
            common.showLoader();
            var data = {
                LowStockQty: lowStockQty,
                MediumStockQty: medStockQty,
                LowStockColor: lowStockColor,
                MediumStockColor: medStockColor,
                HighStockColor: highStockColor,
                LowStockTag: lowStockTag,
                MediumStockTag: medStockTag,
                HighStockTag: highStockTag
            };
            doAjaxPost(adminController + "SaveStockColorSettings", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
            });
        }
        else {
            alert("Please enter all values,tag and select color.");
        }
    },
    SubmitMgaProducts: function (BannerId, Controller) {

        var ProductIds = "";
        $(".icheck_n.product:checked").each(function (index) {
            var val = $(this).val();
            ProductIds += ProductIds.length > 0 ? "," + val : val;
        });
        var data = {
            BannerId: BannerId,
            ProductId: ProductIds
        };
        var controllername;
        switch (Controller.toLowerCase()) {
            case "admin":
                controllername = adminController;
                break;

            case "distributor":
                controllername = distributorController;
                break;
        }
        doAjaxPost(controllername + "SaveMgaProduct", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    SaveCoupons: function (controller) {
        var json = [];

        var schemeId = $('#schemeId').val();

        var num = 0;
        var len = $('.datatable.table tbody tr').length;

        $('.datatable.table tbody tr').each(function () {
            num = num + 1;
            const tr = $(this);

            const numberOfCoupon = tr.find('.numberOfCoupon').html();

            const d = {
                SchemeId: schemeId,
                WorkShopId: tr.attr('id'),
                WorkshopName: tr.find('.wsName').html().trim(),
                Qualified: tr.find('.qualified').html().trim(),
                NumberOfCoupon: numberOfCoupon === null || typeof numberOfCoupon === 'undefined' ? 0 : numberOfCoupon.trim()
            };
            json.push(d);

            if (num === len) {

                var url;
                switch (controller.toLowerCase()) {
                    case 'admin':
                        url = adminController;
                        break;

                    case 'distributor':
                        url = distributorController;
                        break;
                }

                doAjaxPostJson(url + 'SaveCoupons', json, function (d) {
                    if (d.ResultFlag === 1) {
                        var targetUrl = url + `AllocateCoupon?schemeId=${schemeId}`;
                        window.location.replace(targetUrl);
                    }
                    else {
                        common.ShowMessage(d);
                    }

                });
            }
        });
    },
    ShowCouponsDialog: function (schemeId, workshopId, controller) {
        common.showLoader();
        var data = {
            SchemeId: schemeId,
            WorkshopId: workshopId
        };

        var url;
        switch (controller.toLowerCase()) {
            case "admin":
                url = adminController;
                break;

            case "workshop":
                url = workshopController;
                break;

            case "distributor":
                url = distributorController;
                break;
        }

        doAjaxPost(url + "GetCoupons", data, function (d) {
            common.hideLoader();

            $('#couponModal .modal-body').html(d);

            $('#couponModal').modal({
                keyboard: true
            });
        });
    },
    GetWorkshopByCoupon: function (controller) {
        var couponNumber = $("#couponNumber").val();
        if (couponNumber) {
            if (couponNumber.length === 6) {
                common.showLoader();
                var data = {
                    SchemeId: $("#schemeId").val(),
                    CouponNumber: couponNumber
                };

                var url;
                switch (controller.toLowerCase()) {
                    case "admin":
                        url = adminController;
                        break;

                    case "distributor":
                        url = distributorController;
                        break;
                }

                doAjaxPost(url + "GetWorkshopByCoupon", data, function (d) {
                    common.hideLoader();
                    $("#workshops").html(d);
                });
            }
            else {
                alert("Coupon number must be 6 character long.");
            }
        }
        else {
            alert("Please enter coupon number before searching.");
        }
    },
    ShowGiftDialog: function (schemeId, workshopId) {
        common.showLoader();
        var data = {
            SchemeId: schemeId,
            WorkshopId: workshopId
        };

        doAjaxPost(workshopController + "ShowWorkshopGifts", data, function (d) {
            common.hideLoader();

            $('#giftModal .modal-body').html(d);

            $('#giftModal').modal({
                keyboard: false
            });
        });
    },
    ProductLoadMore: function (node, ctrl) {
        var bannerId = $("#bannerId").val();
        var distributorId = $("#distributorId").val();
        var groupId = $(node).attr('groupid');
        var pageNumber = $(node).attr('pagenumber');
        var data = {
            BannerId: bannerId,
            DistributorId: distributorId,
            GroupId: groupId,
            PageNumber: parseInt(pageNumber) + 1
        };
        console.log(data);
        console.log("node", $(node).closest('.mgaProductBox'));
        doAjaxPost(apiController + "GetBannerProductOnloadMore", data, function (d) {
            if (d.ResultFlag == 1) {
                var html = "";
                $(d.Data).each(function (index, item) {
                    var check = item.IsChecked ? "checked" : "";
                    html += `<div class="col-md-3">
                                <div class="form-group">
                                    <input type="checkbox"  `+ check + `  class="icheck_n product" name="User" value="` + item.ProductId + `">
                                    <label>`+ item.Product + `</label>
                                </div>
                            </div>`;
                });


                $(node).parents(".maindiv").find('.mgaProductBox').append(html);
                $(node).attr('pagenumber', parseInt(pageNumber) + 1)
                $('.icheck_n').iCheck('destroy');
                $('.icheck_n').iCheck({
                    checkboxClass: 'icheckbox_square-blue',
                    radioClass: 'iradio_square-blue',
                    increaseArea: '20%'
                });
            }
            $('input.icheck_n.group').on('ifChecked', function () {
                var productDiv = $(this).closest('.maindiv').find('.mgaProductBox input').iCheck('check');
            }).on('ifUnchecked', function () {
                var productDiv = $(this).closest('.maindiv').find('.mgaProductBox input').iCheck('uncheck');
            });
        });
    },
    SubmitFeatureRoles: function (FeatureId) {

        var RoleIds = "";
        $(".icheck_n.Role:checked").each(function (index) {
            var val = $(this).val();
            RoleIds += RoleIds.length > 0 ? "," + val : val;
        });
        var data = {
            FeatureId: FeatureId,
            RoleId: RoleIds
        };
        doAjaxPost(adminController + "SaveRole", data, function (d) {
            common.hideLoader();
            common.ShowMessage(d);
        });
    },
    AddCouponsDialog: function (workshopId, workshopName) {
        $("#hdnWorkshopId").val(workshopId);

        $("#addCouponModal .modal-title").html(`Generate Coupons for ${workshopName}`);

        $('#addCouponModal').modal({
            keyboard: true
        });
    },
    SaveAdditionalCoupons: function (schemeId) {
        common.showLoader();

        var numberOfCoupon = $("#numOfCouponToGen").val();
        if (numberOfCoupon > 0) {
            var data = {
                SchemeId: schemeId,
                WorkshopId: $("#hdnWorkshopId").val(),
                NumberOfCoupons: numberOfCoupon
            };

            doAjaxPost(adminController + "SaveAdditionalCoupons", data, function (d) {
                common.hideLoader();
                if (d.ResultFlag === 1) {
                    $("#statusMsg").html(`<div class='alert alert-success' role='alert'>${d.Message}</div>`);
                }
                else {
                    $("#statusMsg").html(`<div class='alert alert-danger' role='alert'>${d.Message}</div>`);
                }
            });
        }
        else {
            alert("Please enter number.");
        }
    },
    ResetWorkshopWithCoupons: function (controller, schemeId) {
        common.showLoader();

        var data = {
            SchemeId: schemeId
        };

        var url;
        switch (controller.toLowerCase()) {
            case "admin":
                url = adminController;
                break;

            case "distributor":
                url = distributorController;
                break;
        }

        doAjaxPost(url + "ResetWorkshopWithCoupons", data, function (d) {
            common.hideLoader();
            $("#workshops").html(d);
        });
    },
    GetWsWallet: function (isReset, controller) {
        var couponNumber = $("#couponNumber").val();
        if (isReset) {
            couponNumber = "";
            $("#couponNumber").val(couponNumber);
            common.showLoader();
            doAjaxPost("/" + controller + "/" + "WalletPartialView?couponNumber=" + couponNumber + "", null, function (d) {
                common.hideLoader();
                $("#main-content").html(d);
            });
        }
        else {
            if (couponNumber) {
                if (couponNumber.length === 6) {
                    common.showLoader();
                    doAjaxPost("/" + controller + "/" + "WalletPartialView?couponNumber=" + couponNumber + "", null, function (d) {
                        common.hideLoader();
                        $("#main-content").html(d);
                    });
                }
                else {
                    common.LoadErrorMessage("Coupon number must be 6 character long");
                }
            }
            else {
                common.LoadErrorMessage("Please enter coupon number before searching.");
            }
        }
    },
    ShowAddMoneyPopUp: function (controller, workshopId, userId, type) {
        $("#hdnWorkshopId").val(workshopId);
        $("#hdnUserId").val(userId);
        $("#hdnType").val(type);
        if (type === "Add") {
            $("#AddMoney h4").text("Add Money");
            $("#AddMoney .modal-footer a").text("Add");
        }
        else {
            $("#AddMoney h4").text("Remove Money");
            $("#AddMoney .modal-footer a").text("Remove");
        }
        console.log(workshopId, userId);
    },
    AddMoneyToWallet: function () {
        var workshopId = $("#hdnWorkshopId").val();
        var userId = $("#hdnUserId").val();
        var amount = $("#amount").val();
        var type = $("#hdnType").val();
        var description = $("#description").val();
        console.log(workshopId, userId, amount, description);
        if (amount.length == 0 || amount == 0) {
            common.LoadErrorMessage("Amount should be greater than zero");
            return;
        }
        else if (description.length == 0) {
            common.LoadErrorMessage("Description cannot be Empty");
            return;
        }
        var data = {
            WorkShopId: workshopId,
            UserId: userId,
            Type: type == "Add" ? "Cr" : "Dr",
            Description: description,
            WalletAmount: amount,
        };
        common.showLoader();
        doAjaxPost(homeController + "AddMoneyInWallet", data, function (d) {
            common.hideLoader();
            if (d.ResultFlag == 1) {
                $('#AddMoney').modal('hide');
                $(".addMoney input").val('');
                $(".addMoney textarea").val('');
            } else {
            }
            common.ShowMessage(d);
        });
    },
    ShowWorkshopCoupon: function (controller, workshopId, userId) {

        $("#hdnWorkshopId").val(workshopId);
        $("#hdnUserId").val(userId);
        console.log(userId);
        var data = {
            // WorkShopId: workshopId,
            UserId: userId,
            PageNumber: -1
        };

        common.showLoader();
        doAjaxPost(homeController + "WorkshopCoupons", data, function (d) {
            console.log("dd", d);
            common.hideLoader();
            var html = "";
            if (d.ResultFlag == 1) {
                $(d.Data.WsCoupons).each(function (index, obj) {
                    html += `<tr>
                                <td>`+ obj.Date + ` </td>
                                <td>`+ obj.CouponNo + ` </td>
                                <td>`+ obj.Sign + ` ` + obj.Amount + `</td>
                            </tr>`
                });
            }
            if (html.length == 0) {
                html = `<tr>
                          <td colspan="3">
                             <h4 class="text-center"> Coupons not found.</h4>
                         </td>
                        </tr>`
            }
            $(".modal-body .workshopCoupon").html(html);
        });
    },
    GenerateCoupon: function () {
        var workshopId = $("#hdnWorkshopId").val();
        var userId = $("#hdnUserId").val();
        var couponAmount = $("#couponAmount").val();
        var data = {
            // WorkShopId: workshopId,
            UserId: userId,
            CouponAmount: couponAmount
        };
        if (couponAmount.length == 0 || couponAmount == 0) {
            common.LoadErrorMessage("Amount should be greater then zero");
            return;
        }
        common.showLoader();
        doAjaxPost(homeController + "GenerateCoupon", data, function (d) {
            common.hideLoader();
            if (d.ResultFlag == 1) {
                admin.ShowWorkshopCoupon('', workshopId, userId);
            } else {
            }
            common.ShowMessage(d);
        });
    },
    WsTransaction: function (workshopId, userId, workshop) {
        $("#hdnWorkshopId").val(workshopId);
        $("#hdnUserId").val(userId);
        $("#workshopName").text(workshop);
        var data = {
            UserId: userId,
            PageNumber: 0
        };

        common.showLoader();
        doAjaxPost(homeController + "WorkshopTransaction", data, function (d) {
            console.log("dd", d);
            common.hideLoader();
            var html = "";
            if (d.ResultFlag == 1) {
                $(d.Data.WsTransaction).each(function (index, obj) {
                    html += `<tr>
                                <td>`+ obj.Date + ` </td>
                                <td>`+ obj.Description + ` </td>
                                <td>`+ obj.Type + `</td>
                                <td>`+ obj.Sign + ` ` + obj.Amount + `</td>
                            </tr>`
                });
            }
            if (html.length == 0) {
                html = `<tr>
                          <td colspan="4">
                             <h4 class="text-center"> Transactions not found.</h4>
                         </td>
                        </tr>`
            }
            $(".modal-body .wsTransaction").html(html);
        });
    },
    SearchCouponNo: function () {
        var couponNumber = $("#redeemCouponNo").val();
        if (couponNumber) {
            if (couponNumber.length === 6) {
                common.showLoader();
                doAjaxPost(homeController + "SearchCouponNumber?couponNumber=" + couponNumber + "", null, function (d) {
                    common.hideLoader();
                    console.log(d);
                    if (d.length > 0) {
                        $("#couponExistMsg").text(d);
                        $('#confirmCouponNo').removeClass('disabled');
                    }
                    else {
                        $('#confirmCouponNo').addClass('disabled');
                        $("#couponExistMsg").text("Coupon number not exists in any workshop.");
                    }
                    //$("#main-content").html(d);
                });
            }
            else {
                common.LoadErrorMessage("Coupon number must be 6 character long");
            }
        }
        else {
            common.LoadErrorMessage("Please enter coupon number before searching.");
        }
    },
    RedeemCouponNo: function () {
        var couponNumber = $("#redeemCouponNo").val();
        if (couponNumber) {
            if (couponNumber.length === 6) {
                common.showLoader();
                doAjaxPost(homeController + "RedeemCouponNo?couponNumber=" + couponNumber + "", null, function (d) {
                    common.hideLoader();
                    console.log(d);
                    if (d) {
                        common.LoadSuccessMessage("Coupon number has redeem.");
                        $("#redeemModal").modal('hide');
                    }
                    else {
                        common.LoadErrorMessage("Coupon number not redeem.");
                    }
                });
            }
            else {
                common.LoadErrorMessage("Coupon number must be 6 character long");
            }
        }
        else {
            common.LoadErrorMessage("Please enter coupon number before searching.");
        }
    },

    AddUpdateMailTemplate: function () {
        var id = $("#Id").val();
        var mailHeading = $("#MailHeading").val();
        var fullMailHtml = CKEDITOR.instances['fullMailHtml'].getData();
        var description = $("#Description").val();
        var smsText = $("#SmsText").val();
        var type = $("#Type").val();

        var data = {
            Id: id, MailHeading: mailHeading, MailHtml: fullMailHtml, Description: description, SmsText: smsText, Type: type
        }
        if (type.length == 0) {
            common.LoadErrorMessage("Please select Type");
            return;
        }
        common.showLoader();
        doAjaxPostJson(adminController + "AddMailTemplate", data, function (d) {
            common.hideLoader();
            console.log(d);
            if (d.success) {
                common.LoadSuccessMessage(d.message);
                window.location.href = "/admin/MailTemplate";
            }
            else {
                common.LoadErrorMessage(d.message);
            }
        });
    },
    DeleteMailTemplate: function (Id) {
        var id = Id;
        if (id == 0) {
            return;
        }
        common.showLoader();
        doAjaxPost(adminController + "DeleteMailTemplate?id=" + id + "", null, function (d) {
            common.hideLoader();
            console.log(d);
            if (d.success) {
                common.LoadSuccessMessage(d.message);
                window.location.href = "/admin/MailTemplate";
            }
            else {
                common.LoadErrorMessage(d.message);
            }
        });
    },
    SaveBestSeller: function (controller) {
        const distributorId = $('select#ddlDistributor').val();
        if (distributorId === '') {
            alert("Please select distributor");
            return false;
        }

        var url;
        switch (controller.toLowerCase()) {
            case 'admin':
                url = adminController;
                break;

            case 'distributor':
                url = distributorController;
                break;
        }

        var groupIds = [];
        $(".icheck_n.product:checked").each(function (index) {
            const val = $(this).val();
            groupIds.push(val);
        });

        if (groupIds.length > 0) {
            const data = {
                groupIds: groupIds,
                distributorId: distributorId
            };

            common.showLoader();
            doAjaxPost(url + 'SaveBestSeller',
                data,
                function (d) {
                    common.hideLoader();
                    common.ShowMessage(d);
                });
        } else {
            alert('Please select atleast one seller for saving.');
        }
    },
    DeleteSupport: function (id) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            data = {
                Id: id
            };
            doAjaxPost(apiController + "DeleteSupportHtml", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },
    DeleteProductType: function (id) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            data = {
                Id: id
            };
            doAjaxPost(apiController + "DeleteProductType", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },
    DeleteSupportQuery: function (id, from) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            var Id = $(id).parents("tr").attr("id");
            data = {
                Id: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteSupportQuery", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                if (d.ResultFlag) {
                    admin.LoadPartialView($("#PageView").val());
                }
            });
        }
    },
    GetBestSellerByDistributor: function (distributorId, controller) {
        common.showLoader();
        const data = { distributorId: distributorId };

        var url;
        switch (controller.toLowerCase()) {
            case 'admin':
                url = adminController;
                break;

            case 'distributor':
                url = distributorController;
                break;
        }

        doAjaxPost(url + 'BestSellerByDistributor', data, function (d) {
            common.hideLoader();
            $('.mgaProductBox').html(d);
            admin.bindICheck();
        });
    },
    bindICheck: function () {
        $('.icheck_n').iCheck('destroy');
        $('.icheck_n').iCheck({
            checkboxClass: 'icheckbox_square-blue',
            radioClass: 'iradio_square-blue',
            increaseArea: '20%'
        });
    },

    BindGrowthPercentage: function (distId) {
        common.showLoader();
        $("#MinValue").val(null);
        $("#GrowthPercentage").val(null);
        var data = { distributorId: distId };
        doAjaxPost(homeController + "GetGrowthPercentage", data, function (d) {
            console.log("GetGrowthPercentage", d);
            if (d.MinValue > 0) {
                $("#MinValue").val(d.MinValue);
            }
            if (d.GrowthPercentage > 0) {
                $("#GrowthPercentage").val(d.GrowthPercentage);
            }

            common.hideLoader;
        });
    },
    bindCouponUploader: function (currSchemeId, controller) {
        const couponUploader = new qq.FineUploader({
            element: $('.coupon-fineUploader')[0],
            action: '/Home/upload',
            type: "post",
            minSizeLimit: 0,
            autoUpload: true,
            text: {
                uploadButton: '<i class="fa fa-file-excel-o"></i> Select Files'
            },
            validation: {
                acceptFiles:
                    ".xls,.xlsx,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                allowedExtensions: ['xls', 'xlsx'],
                sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
            },
            callbacks: {
                onComplete: function (id, filename, responseJSON) {
                    if (responseJSON.success === true) {
                        admin.generateCoupons(responseJSON.filename, controller, currSchemeId);
                    } else {
                        alert("Error: " + responseJSON.message);
                    }
                }
            }
        });
    },
    generateCoupons: function (filePath, controller, schemeId) {
        const data = {
            FilePath: filePath,
            SchemeId: schemeId
        };

        var url = '';
        switch (controller.toLowerCase()) {
            case 'admin':
                url = adminController;
                break;

            case 'distributor':
                url = distributorController;
                break;
        }

        common.showLoader();
        doAjaxPost(`${url}GenerateCouponsFromFile`, data,
            function (d) {
                if (d.ResultFlag === 1) {
                    const targetUrl = `${url}AllocateCoupon?schemeId=${schemeId}`;
                    window.location.replace(targetUrl);
                } else {
                    common.ShowMessage(d);
                }
            });
    },
    SetCreditLimit: function () {
        var confirmation = confirm('Do you want to set credit limit?');
        if (confirmation == true) {
            common.showLoader();
            doAjaxPostJson(homeController + "SetCreditLimit", null, function (d) {
                common.ShowMessage(d);

            });
        }
    },
    BindSchemeonDistributor: function () {
        common.showLoader();
        var distId = $("#ddldistributor").val();
        var bannerType = $("#ddlbannertype").val();
        distId = distId.length == 0 ? 0 : distId;
        bannerType = bannerType.length == 0 ? 0 : bannerType;

        if (distId != undefined && distId != 0 && bannerType == 'Scheme') {
            var data = { distributorId: distId };
            doAjaxPostJson(homeController + "DistributorSchemes", data, function (d) {
                // Set scheme Id as per model
                const schemeId = $('#hdnSchemeId').val();
                var isScheme = false;
                if (d.ResultFlag === 1) {
                    $("#ddlSchemes").empty();
                    $("#ddlSchemes").append("<option value=''>--Select--</option>");
                    $.each(d.Data, function (Data, value) {
                        $("#ddlSchemes").append($("<option></option>").val(value.SchemeId).html(value.SchemeName));
                        if (schemeId == value.SchemeId) {
                            isScheme = true;
                        }
                    })
                    if (isScheme) {
                        $("#ddlSchemes").val(schemeId);
                    }
                }
            });
            $("#scheme").show();
        }
        else if (distId != undefined && distId != 0 && bannerType == 'OtherPage') {
            $("#ddlSchemes").empty();
            $("#ddlSchemes").append("<option value=''>--Select--</option>");
            $("#scheme").hide();
        }
        else {
            $("#ddlSchemes").empty();
            $("#ddlSchemes").append("<option value=''>--Select--</option>");
            $("#scheme").show();

        }
        common.hideLoader();
    },
    DeleteRequestPartFilter: function (id, from) {
        if (confirm('Do you want to Delete ?')) {
            common.showLoader();
            var data = {
                Id: $(id).parents("tr").attr("id")
            };
            doAjaxPost(apiController + "DeleteRequestPartFilter", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
                admin.LoadPartialView($("#PageView").val());
                //if ($("#FromDis").length > 0) {
                //    admin.LoadDistributorPartialView($("#PageView").val());
                //} else {
                //    admin.LoadAdminPartialView($("#PageView").val());
                //}

            });
        }
    },
    deleteNewPartRequest: function (controller, id) {
        let url = '';
        switch (controller.toLowerCase()) {
            case 'admin':
                url = adminController;
                break;

            case 'distributor':
                url = distributorController;
                break;
        }

        if (url === '') {
            common.LoadErrorMessage("Controller name not passed.");
            return;
        }

        if (confirm('Do you want to delete current part request?')) {
            common.showLoader();
            const data = { Id: id };

            doAjaxPost(url + 'DeleteNewPartRequest', data, function (response) {
                common.hideLoader();
                common.ShowMessage(response);
                admin.LoadPartialView($('#PageView').val());
            });
        }
    },
    SetWorkshopUserId: function (userId) {
        common.showLoader();
        var data = {
            CurrentUserId: userId
        };
        doAjaxPost(apiController + "SetWorkshopUserId", data, function (d) {
            window.location.reload();
            common.hideLoader();
        });
    },
    GetCBOByDistributor: function (distributorId, controller) {
        common.showLoader();
        const data = { distributorId: distributorId };

        var url;
        switch (controller.toLowerCase()) {
            case 'admin':
                url = adminController;
                break;

            case 'distributor':
                url = distributorController;
                break;
        }

        doAjaxPost(url + 'CustomerBackOrdersByDistributor', data, function (d) {
            common.hideLoader();
            $('.backOrders').html(d);
        });
    },
    CboExportToExel: function () {
        common.showLoader();
        let DistributorId = $('#ddlDistributor').val();
        if (DistributorId == "" || DistributorId == 0) {
            common.hideLoader();
            common.LoadErrorMessage('Please select distributor');
            return;
        }

        const data = { distributorId: DistributorId };

        doAjaxPost(homeController + 'BackOrdersExportToExcel', data, function (d) {
            common.hideLoader();
            if (d.error != undefined && d.error != "") { common.LoadErrorMessage(d.error); }
            else {
                var blob = new Blob([d], { type: 'application/ms-exel' });
                var downloadurl = URL.createObjectURL(blob);
                var a = document.createElement("a");
                a.href = downloadurl;
                a.download = "CustomerBackOrders.xls";
                document.body.appendChild(a);
                a.click();
            }
        });
    },
    OutletCboExportToExel: function () {
        common.showLoader();

        doAjaxPost(outletController + 'BackOrdersExportToExcel', null, function (d) {
            common.hideLoader();
            if (d.error != undefined && d.error != "") { common.LoadErrorMessage(d.error); }
            else {
                var blob = new Blob([d], { type: 'application/ms-exel' });
                var downloadurl = URL.createObjectURL(blob);
                var a = document.createElement("a");
                a.href = downloadurl;
                a.download = "CustomerBackOrders.xls";
                document.body.appendChild(a);
                a.click();
            }
        });
    }

};

