var schemes = function () {
    const luckyDraw = 'Lucky Draw',
        fixedPrice = 'Fixed Price',
        category = 'Category',
        focusPartsGroup = 'Focus Parts Group',
        fmsPartsGroup = 'FMS Parts Group',
        dateFormat = 'MMM Do, YYYY',
        fmsValueErrorMsg = 'Sum of FMS values should be equal to 100.';

    // For steps
    var basic = 'Basic',
        partInfo = 'PartInfo',
        customerSegment = 'CustomerSegment',
        targetInfo = 'TargetInfo',
        cashback = 'Cashback',
        tabs = 'Tabs',
        done = 'Done',
        cashBackJson = [basic, partInfo, customerSegment, targetInfo, cashback, done],
        luckyDrawJson = [basic, partInfo, customerSegment, targetInfo, tabs, done],
        currentStep = 0;

    // For saving on tab click
    var stepToSave = basic;

    // Scheme table related variables for saving scheme data with one function
    var schemeId = 0,
        schemeName = null,
        schemeType = null,
        subSchemeType = null,
        isCashback = null,
        isAssuredGift = null,
        isLuckyDraw = null,
        distributorId = null,
        startDate = null,
        endDate = null,
        paybackPeriod = null,
        paybackType = null,
        thumbnailImage = null,
        bannerImage = null,

        // Part Info
        partCategory = null,
        partGroup = null,
        focusPartBenifitType = null,
        focusPartBenifitTypeVal = null,
        focusPartTarget = null,
        focusPartBenifitTypeNumber = null,
        startRange = null,
        endRange = null,
        fValue = null,
        mValue = null,
        sValue = null,
        partCreation = null,
        focusGroupIds = '0',

        // Customer segment
        branchCodes = null,
        salesExecutives = null,
        customerTypes = null,

        // Target Info
        prevYearFromDate = null,
        prevYearToDate = null,
        prevMonthFromDate = null,
        prevMonthToDate = null,
        growthCompPercentMinValue = null,
        growthCompPercentBaseValue = null,

        // Reward Info
        cashbackCriteria = null,
        areBothCbAgApplicable = null,
        areBothAgLdApplicable = null,
        canTakeMoreThanOneGift = null,
        maxGiftsAllowed = null;

    init = function () {

        bindFn();
        // show next step after change url to save first step
        let step = localStorage.getItem("SchemeStep");
        if (step != undefined || step != null) {
            localStorage.removeItem("SchemeStep");
            schemes.showStepNew(step);
        }

        readElementsValues();
    },
        bindFn = function () {
            common.bindDatepicker();
            bindMultiSelect();

            if ($('#hdnSchemeId').val() !== '') {
                schemeId = $('#hdnSchemeId').val();
            }

            schemeType = $('#ddlSchemeType').val();
            schemes.handleSchemeTypeChange($('#hdnPaybackType').val());

            $('#ddlSchemeType').change(function () {
                schemeType = $(this).val();
                $('#lblSchemeTypeHeader').text(schemeType);
                schemes.handleSchemeTypeChange($('#hdnPaybackType').val());
            });


            $('.submit-btn').click(function () {
                common.showLoader();
                window.location.href = $('#Role').val() === 'SuperAdmin' ? `/admin/Schemes` : `/distributor/Schemes`;
            });

            $('.active-scheme').click(function () {
                schemes.saveActiveScheme(true);
            });

            $('.inactive-scheme').click(function () {
                schemes.saveActiveScheme(false);
            });

            common.bindFileUpload(function (d) {
                schemes.bindTargetWorkShopFromExcel(d);
            });

            schemes.bindICheck();
            schemes.bindOtherFileUploads();

            $('#PartCategory').change(function () {
                distributorId = $('#distributorId').val();
                partCategory = $(this).val();

                // bindProdGroups();
            });

            $('#PartType').change(function () {
                schemes.loadFocusPartData();
            });

            $('#FValue').change(function () {
                fValue = $(this).val();
                mValue = $('#MValue').val();
                sValue = $('#SValue').val();

                const totalFms = parseFloat(fValue) + parseFloat(mValue) + parseFloat(sValue);
                if (totalFms !== 100) {
                    common.LoadErrorMessage(fmsValueErrorMsg);
                    return;
                }
            });

            $('#MValue').change(function () {
                fValue = $('#FValue').val();
                mValue = $(this).val();
                sValue = $('#SValue').val();

                const totalFms = parseFloat(fValue) + parseFloat(mValue) + parseFloat(sValue);
                if (totalFms !== 100) {
                    common.LoadErrorMessage(fmsValueErrorMsg);
                    return;
                }
            });
            $('#SValue').change(function () {
                fValue = $('#FValue').val();
                mValue = $('#MValue').val();
                sValue = $(this).val();

                const totalFms = parseFloat(fValue) + parseFloat(mValue) + parseFloat(sValue);
                if (totalFms !== 100) {
                    common.LoadErrorMessage(fmsValueErrorMsg);
                    return;
                }
            });

            $('#ddlRoIncharge').change(function () {
                const roId = $(this).val();
                bindSalesExecutive(roId);
            });

            $('#ddlBranchCode').change(function () {
                branchCodes = $(this).val();
                if (branchCodes) {
                    bindSalesExecutiveByBranchCode();
                }
            });

            $('#distributorId').change(function () {
                distributorId = $(this).val();
                bindBranchCodeByDistributorId();
                bindCustomerTypeByDistributorId();
            });

            $('#ddlCustomerType').change(function () {
                customerTypes = $(this).val();
            });

            $('#add-category').click(function () {
                schemes.showStepNew('', customerSegment);
            });

            schemes.bindCheckAll();

            $('#cashbackRangeModal').on('shown.bs.modal',
                function () {
                    getCashBackRangeData(false);
                });

            // Set radio buttons in Reward info
            setRewardInfoChecks();

            // This function will be used to open specific tab when user click on 'Edit' button in SchemePreview page
            showSchemeTab();
        },

        bindMultiSelect = function () {
            $('.multiselect').multiselect('destroy');
            $('.multiselect').multiselect({
                nonSelectedText: 'None',
                includeSelectAllOption: true,
                selectAllText: 'Select All',
                maxHeight: 300
            });
        },
        bindCheckAll = function () {
            $("#checkbox-selectall").on('ifChanged',
                function (event) {
                    console.log($(this).is(":checked"));
                    if ($(this).is(":checked")) {
                        $("#workshops .col-sm-4 .icheck").iCheck('check');
                    } else {
                        $("#workshops .col-sm-4 .icheck").iCheck('uncheck');
                    }
                });
        },
        showStepNew = function (type, stepDefined) {
            var nextStep = '';

            if (stepDefined !== undefined && stepDefined !== null) {
                nextStep = stepDefined;
                currentStep =
                    (schemeType === luckyDraw ? luckyDrawJson.indexOf(nextStep) : cashBackJson.indexOf(nextStep));
                if (currentStep > 0 && schemeId === 0) {
                    return;
                }

            } else {
                nextStep = getNextStep(type);
            }

            if (nextStep === basic) {
                $('.step-basic').attr('data-ktwizard-state', 'current');
                $('.step-part-info').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment').attr('data-ktwizard-state', 'pending');
                $('.step-target').attr('data-ktwizard-state', 'pending');
                $('.step-other').attr('data-ktwizard-state', 'pending');
                $('.step-done').attr('data-ktwizard-state', 'pending');

                $('.step-basic-content').attr('data-ktwizard-state', 'current');
                $('.step-part-content').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment-content').attr('data-ktwizard-state', 'pending');
                $('.step-target-content').attr('data-ktwizard-state', 'pending');
                $('.step-other-content').attr('data-ktwizard-state', 'pending');
                $('.step-done-content').attr('data-ktwizard-state', 'pending');

                stepToSave = basic;

            } else if (nextStep === partInfo) {
                $('.step-basic').attr('data-ktwizard-state', 'pending');
                $('.step-part-info').attr('data-ktwizard-state', 'current');
                $('.step-customer-segment').attr('data-ktwizard-state', 'pending');
                $('.step-target').attr('data-ktwizard-state', 'pending');
                $('.step-other').attr('data-ktwizard-state', 'pending');
                $('.step-done').attr('data-ktwizard-state', 'pending');

                $('.step-basic-content').attr('data-ktwizard-state', 'pending');
                $('.step-part-content').attr('data-ktwizard-state', 'current');
                $('.step-customer-segment-content').attr('data-ktwizard-state', 'pending');
                $('.step-target-content').attr('data-ktwizard-state', 'pending');
                $('.step-other-content').attr('data-ktwizard-state', 'pending');
                $('.step-done-content').attr('data-ktwizard-state', 'pending');

                schemes.loadFocusPartData();
                stepToSave = partInfo;

            } else if (nextStep === customerSegment) {
                $('.step-basic').attr('data-ktwizard-state', 'pending');
                $('.step-part-info').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment').attr('data-ktwizard-state', 'current');
                $('.step-target').attr('data-ktwizard-state', 'pending');
                $('.step-other').attr('data-ktwizard-state', 'pending');
                $('.step-done').attr('data-ktwizard-state', 'pending');

                $('.step-basic-content').attr('data-ktwizard-state', 'pending');
                $('.step-part-content').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment-content').attr('data-ktwizard-state', 'current');
                $('.step-target-content').attr('data-ktwizard-state', 'pending');
                $('.step-other-content').attr('data-ktwizard-state', 'pending');
                $('.step-done-content').attr('data-ktwizard-state', 'pending');

                schemes.getTargetGrowth(schemeId);
                stepToSave = customerSegment;

            } else if (nextStep === targetInfo) {
                // Set target info dates if adding scheme
                setTargetInfoDates();

                //schemes.generateTargets(false);

                $('.step-basic').attr('data-ktwizard-state', 'pending');
                $('.step-part-info').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment').attr('data-ktwizard-state', 'pending');
                $('.step-target').attr('data-ktwizard-state', 'current');
                $('.step-other').attr('data-ktwizard-state', 'pending');
                $('.step-done').attr('data-ktwizard-state', 'pending');

                $('.step-basic-content').attr('data-ktwizard-state', 'pending');
                $('.step-part-content').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment-content').attr('data-ktwizard-state', 'pending');
                $('.step-target-content').attr('data-ktwizard-state', 'current');
                $('.step-other-content').attr('data-ktwizard-state', 'pending');
                $('.step-done-content').attr('data-ktwizard-state', 'pending');

                stepToSave = targetInfo;
            } else if (nextStep === cashback) {
                getTargetOverview();
                schemes.getCashBackData(false);

                $('.step-basic').attr('data-ktwizard-state', 'pending');
                $('.step-part-info').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment').attr('data-ktwizard-state', 'pending');
                $('.step-target').attr('data-ktwizard-state', 'pending');
                $('.step-other').attr('data-ktwizard-state', 'current');
                $('.step-done').attr('data-ktwizard-state', 'pending');

                $('.step-basic-content').attr('data-ktwizard-state', 'pending');
                $('.step-part-content').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment-content').attr('data-ktwizard-state', 'pending');
                $('.step-target-content').attr('data-ktwizard-state', 'pending');
                $('.step-other-content').attr('data-ktwizard-state', 'current');
                $('.step-done-content').attr('data-ktwizard-state', 'pending');

                showLuckyDrawTabs(false);
                stepToSave = cashback;

            } else if (nextStep === tabs) {
                getTargetOverview();
                schemes.getCashBackData(false);
                schemes.getAssuredGiftData(false);
                schemes.getLuckyDrawData(false);
                schemes.getCouponAllocationData(false);

                $('.step-basic').attr('data-ktwizard-state', 'pending');
                $('.step-part-info').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment').attr('data-ktwizard-state', 'pending');
                $('.step-target').attr('data-ktwizard-state', 'pending');
                $('.step-other').attr('data-ktwizard-state', 'current');
                $('.step-done').attr('data-ktwizard-state', 'pending');

                $('.step-basic-content').attr('data-ktwizard-state', 'pending');
                $('.step-part-content').attr('data-ktwizard-state', 'pending');
                $('.step-customer-segment-content').attr('data-ktwizard-state', 'pending');
                $('.step-target-content').attr('data-ktwizard-state', 'pending');
                $('.step-other-content').attr('data-ktwizard-state', 'current');
                $('.step-done-content').attr('data-ktwizard-state', 'pending');

                $('.step-category-content').attr('data-ktwizard-state', 'pending');

                showLuckyDrawTabs(true);
                stepToSave = tabs;
            } else if (nextStep === category) // TODO: What's the use of this step?
            {
                schemes.getCategoryData(schemeId);

                $('.step-other-content').attr('data-ktwizard-state', 'pending');
                $('.step-category-content').attr('data-ktwizard-state', 'current');

                //setTimeout(function () {
                //    schemes.getCategoryData(0);
                //    schemes.getCategoryData(0);
                //    schemes.getCategoryData(0);
                //}, 100);

            } else if (nextStep === done) {
                window.location.href = `/home/SchemePreview?schemeId=${schemeId}`;

                //schemes.getActiveScheme();

                //$('.step-basic').attr('data-ktwizard-state', 'pending');
                //$('.step-part-info').attr('data-ktwizard-state', 'pending');
                //$('.step-customer-segment').attr('data-ktwizard-state', 'pending');
                //$('.step-target').attr('data-ktwizard-state', 'pending');
                //$('.step-other').attr('data-ktwizard-state', 'pending');
                //$('.step-done').attr('data-ktwizard-state', 'current');

                //$('.step-basic-content').attr('data-ktwizard-state', 'pending');
                //$('.step-part-content').attr('data-ktwizard-state', 'pending');
                //$('.step-customer-segment-content').attr('data-ktwizard-state', 'pending');
                //$('.step-target-content').attr('data-ktwizard-state', 'pending');
                //$('.step-other-content').attr('data-ktwizard-state', 'pending');
                //$('.step-done-content').attr('data-ktwizard-state', 'current');

                //$('.nav-tabs.review-tab .nav-item').hide();
                //$('.nav-tabs.review-tab .nav-item .nav-link').removeClass('active');
                //$('.nav-tabs.review-tab .nav-item .dropdown-item').hide();

                //$('.tab-content.review-tab-content .tab-pane').removeClass('active');

                //if (schemeType === luckyDraw)
                //{
                //    $('.nav-tabs.review-tab .nav-item-category .nav-link').addClass('active');
                //    $('.nav-tabs.review-tab .nav-item-category').show();

                //    $('.nav-tabs.review-tab .nav-item-gift').show();
                //    $('.nav-tabs.review-tab .nav-item-assured').show();
                //    $('.nav-tabs.review-tab .nav-item-cashback').show();
                //    $('.nav-tabs.review-tab .nav-item-other').show();
                //    $('.nav-tabs.review-tab .nav-item-qualifying').show();
                //    $('.nav-tabs.review-tab .nav-item-target').show();
                //    $('.nav-tabs.review-tab .nav-item-focus').show();

                //    $('#kt_tabs_Category').addClass('active');
                //    $('#kt_tabs_Category').show();
                //} else if (schemeType === cashback)
                //{
                //    $('.nav-tabs.review-tab .nav-item-cashback').show();
                //    $('.nav-tabs.review-tab .nav-item-target').show();
                //    $('.nav-tabs.review-tab .nav-item-focus').show();
                //    $('.nav-tabs.review-tab .nav-item-other').show();

                //    $('.nav-tabs.review-tab .nav-item-cashback .nav-link').addClass('active');

                //    $('#kt_tabs_CashBack').addClass('active');
                //    $('#kt_tabs_CashBack').show();
                //}

                //setPreviewFields();
            }
        },
        goToStep = function (type) {
            if (stepToSave) {
                if (stepToSave === basic) {
                    //bindBranchCodeByDistributorId();
                    //bindCustomerTypeByDistributorId();
                    saveBasicInfo(false);
                } else if (stepToSave === partInfo) {
                    savePartInfo(false);
                } else if (stepToSave === customerSegment) {
                    saveCustomerSegment(false);
                } else if (stepToSave === targetInfo) {
                    saveTargetInfo(false);
                } else if (stepToSave === cashback || stepToSave === tabs) {
                    saveRewardInfo(false);
                }
            }

            showStepNew('', type);
        },
        getNextStep = function (type) {
            if (type === 'Next') {
                currentStep = currentStep + 1;
            } else if (type === 'Prev') {
                currentStep = currentStep - 1;

            } else if (type === 'Skip') {
                currentStep = currentStep + 1;
            }

            return schemeType === luckyDraw ? luckyDrawJson[currentStep] : cashBackJson[currentStep];
        },
        saveBasicInfo = function (canGoToNextStep) {
            schemes.readElementsValues();
            if (schemeName === '') {
                alert('Please enter Scheme Name');
                return;
            } else if (schemeType === '') {
                alert('Please select scheme type');
                return;
            } else if (distributorId === '') {
                alert('Please select Distributor');
                return;
            } else if (startDate === '') {
                alert('Please select StartDate');
                return;
            } else if (endDate === '') {
                alert('Please select EndDate');
                return;
            } else if (schemeType === 'Cashback' && !subSchemeType) {
                alert('Please select sub scheme type');
                return;
            } else if (schemeType === 'Lucky Draw') {
                if (!isCashback && !isAssuredGift && !isLuckyDraw) {
                    alert('Please select one of the reward options');
                    return;
                }
            }
            // bindBranchCodeByDistributorId();

            schemes.saveScheme(canGoToNextStep);
        },
        readElementsValues = function () {
            // Read values of all elements

            // Basic info
            schemeType = $('#ddlSchemeType').val();
            schemeName = $('#SchemeName').val();
            distributorId = $('#distributorId').val();
            startDate = $('#StartDate').val();
            endDate = $('#EndDate').val();
            paybackPeriod = $('#FrequencyId').val();
            paybackType = $('#DispersalTypeId').val();
            thumbnailImage = $('#ThumbnailImage').val();
            bannerImage = $('#BannerImage').val();
            subSchemeType = $('#ddlSubSchemeType').val();
            isCashback = $('#chkCashback').is(':checked');
            isAssuredGift = $('#chkAssuredGift').is(':checked');
            isLuckyDraw = $('#chkLuckyDraw').is(':checked');

            // Part info
            partGroup = $('select#PartType').val();
            partCategory = $('#PartCategory').val().join(',');
            focusPartBenifitType = $("input[name='FocusPartBenifitTypetab']:checked").val();
            focusPartBenifitTypeVal = $('#focusPartBenifitTypeValuetab option:selected').val();
            focusPartTarget = $('#focusPartTargettab').val();
            focusPartBenifitTypeNumber = $('#benifitTypeNumbertab').val();
            fValue = $('#FValue').val();
            mValue = $('#MValue').val();
            sValue = $('#SValue').val();
            startRange = $('#startRange').val();
            endRange = $('#endRange').val();
            partCreation = $('select#ddlPartCreation').val();

            // Customer Segment
            const branchCode = $('#ddlBranchCode').val();
            const salesExecutive = $('#ddlSalesExecutive').val();
            const customerType = $('#ddlCustomerType').val();

            branchCodes = branchCode === '' || branchCode === null || branchCode === undefined
                ? branchCodes
                : branchCode.join(',');
            salesExecutives = salesExecutive === '' || salesExecutive === null || salesExecutive === undefined
                ? null
                : salesExecutive.join(',');
            customerTypes = customerType === '' || customerType === null || customerType === undefined
                ? customerTypes
                : customerType.join(',');

            // Target Info
            prevYearFromDate = $('#PrevYearFromDate').val();
            prevYearToDate = $('#PrevYearToDate').val();
            prevMonthFromDate = $('#PrevMonthFromDate').val();
            prevMonthToDate = $('#PrevMonthToDate').val();
            growthCompPercentMinValue = $('#growthCompPercentMinValue').val();
            growthCompPercentBaseValue = $('#growthCompPercentBaseValue').val();

            // Reward Info
            cashbackCriteria = $('#CriteriaCashback').val();
            areBothCbAgApplicable = $('.both-cbAg-applicable:checked').val() === 'Yes' ? true : false;
            areBothAgLdApplicable = $('.both-agLd-applicable:checked').val() === 'Yes' ? true : false;
            canTakeMoreThanOneGift = $('.can-take:checked').val() === 'Yes' ? true : false;
            maxGiftsAllowed = $('#maxGiftAllowed').val();
        },
        saveScheme = function (canGoToNextStep) {
            const data =
            {
                SchemeId: schemeId,
                UserId: $('#user_id').val(),

                // STEP - Basic information
                SchemeName: schemeName,
                Types: schemeType,
                SubSchemeType: subSchemeType,
                IsCashBack: isCashback,
                IsAssuredGift: isAssuredGift,
                IsLuckyDraw: isLuckyDraw,
                DistributorId: distributorId,
                StartDate: startDate,
                EndDate: endDate,
                ThumbnailImage: thumbnailImage,
                BannerImage: bannerImage,
                DispersalFrequency: paybackPeriod, // Payback Period
                SchemesType: paybackType, // Payback Type

                // STEP - Part Info
                PartCategory: partCategory,
                PartType: partGroup,
                FocusPartBenifitType: focusPartBenifitType,
                FocusPartBenifitTypeValue: focusPartBenifitTypeVal,
                FocusPartTarget: focusPartTarget,
                FocusPartBenifitTypeNumber: focusPartBenifitTypeNumber,
                StartRange: startRange,
                EndRange: endRange,
                FValue: fValue,
                MValue: mValue,
                SValue: sValue,
                PartCreations: partCreation,

                // STEP - Customer Segment
                BranchCode: branchCodes,
                SalesExecutiveId: salesExecutives,
                PartyType: customerTypes,

                // STEP - Target Info
                PrevYearFromDate: prevYearFromDate,
                PrevYearToDate: prevYearToDate,
                PrevMonthFromDate: prevMonthFromDate,
                PrevMonthToDate: prevMonthToDate,
                GrowthCompPercentMinValue: growthCompPercentMinValue,
                GrowthCompPercentBaseValue: growthCompPercentBaseValue,

                // STEP - Reward Info
                CashbackCriteria: cashbackCriteria,
                AreBothCbAgApplicable: areBothCbAgApplicable,
                AreBothAgLdApplicable: areBothAgLdApplicable,
                CanTakeMoreThanOneGift: canTakeMoreThanOneGift,
                MaxGiftsAllowed: maxGiftsAllowed
            };

            common.showLoader();
            doAjaxPost(apiController + 'AddScheme',
                data,
                function (d) {
                    if (d.ResultFlag === 1) {
                        if (schemeId == undefined || schemeId == 0) {
                            localStorage.setItem("SchemeStep", 'Next');
                            // TODO: Change url in browser only
                            const url = `AddSchemes/${d.Data}?mode=edit`;
                            window.location.replace(url)
                        }
                        else {
                            if (canGoToNextStep) {
                                schemes.showStepNew('Next');
                            }
                        }
                        //schemeId = d.Data;

                        // Change url of browser so that if user refresh browser, then he still gets access to editing page
                        //const isBasicStep = schemeType === luckyDraw ? luckyDrawJson[currentStep] === basic : cashBackJson[currentStep] === basic;
                        //if ($('#hdnAction').val() === 'Add' && isBasicStep) {
                        //    // TODO: Change url in browser only
                        //    //const url = `AddSchemes/${schemeId}?mode=edit`;
                        //    // window.location.replace(url);
                        //}

                        //if (canGoToNextStep) {
                        //    schemes.showStepNew('Next');
                        //}

                    } else {
                        common.hideLoader();
                        alert(d.Message);
                    }
                });
        },
        // ---- PART INFO ----
        //bindProdGroups = function () {
        //    const partType = $('select#PartType').val();
        //    if (partType !== focusPartsGroup) return;

        //    partCategory = $('#PartCategory').val();

        //    const allCategoriesSelected = $('#PartCategory option:not(:selected)').length === 0;
        //    const categories = allCategoriesSelected ? 'All' : partCategory.join(',');

        //    $('select#ddlProducts').empty();
        //    $('select#ddlProducts').append('<option value="">-- Select --</option>');

        //    const data = {
        //        DistributorId: distributorId.length > 0 ? distributorId : 0,
        //        PartCategory: categories
        //    };

        //    doAjaxPost(homeController + 'GetProdGroups', data, function (d) {
        //        $('select#ddlProdGroups').empty();
        //        $('select#ddlProdGroups').append('<option value="">-- Select --</option>');
        //        $.each(d.Data,
        //            function (key, group) {
        //                $('select#ddlProdGroups')
        //                    .append(`<option value='${group.GroupId}'>${group.GroupName}</option>`);
        //            });
        //    });
        //},
        //bindProducts = function (prodGroup) {
        //    const groupId = prodGroup.val();
        //    if (groupId.length === 0) {
        //        $(prodGroup).closest('.row').find('#ddlProducts').empty();
        //        $(prodGroup).closest('.row').find('#ddlProducts').append('<option value="">-- Select --</option>');
        //    }

        //    const row = prodGroup.parents('.row');
        //    const data = {
        //        GroupId: groupId.length > 0 ? groupId : 0
        //    };

        //    doAjaxPost(homeController + 'GetProducts', data, function (d) {
        //        var ddlProducts = row.find('.product');
        //        $(ddlProducts).empty();
        //        $(ddlProducts).append('<option value="">-- Select --</option>');
        //        $.each(d.Data,
        //            function (key, product) {
        //                $(ddlProducts).append(`<option value="${product.ProductId}">${product.ProductName}</option>`);
        //            });
        //    });
        //},
        loadFocusPartData = function () {
            const partType = $('select#PartType').val();

            if (partType === focusPartsGroup) {
                getFocusPartData(false);
                $('#partInfoFocusPart').removeClass('hide');
                $('#fmsPartsGroup').addClass('hide');
            } else if (partType === fmsPartsGroup) {
                getFmsPartData();
                $('#fmsPartsGroup').removeClass('hide');
                $('#partInfoFocusPart').addClass('hide');
            } else {
                $('#fmsPartsGroup').addClass('hide');
                $('#partInfoFocusPart').addClass('hide');
            }
        },
        removeCurrentFpgRow = function (node) {
            if (confirm('Are you sure you want to remove current row?')) {
                if ($('.focus-part-data-mix .row').length <= 1) {
                    common.LoadErrorMessage('Min one row required, You can not delete this.'); return;
                }
                const row = $(node).parent().parent();
                $(row).remove();
                focusGroupIds = null;
                $('.focus-part-data-mix').find('.row').each(function () {
                    focusGroupIds += $(this).attr('id');
                });
            }
        },
        bindFocusPartFromExcel = function (filePath, focusPartEle) {
            partCategory = $('#PartCategory').val();
            const allCategoriesSelected = $('#PartCategory option:not(:selected)').length === 0;
            const categories = allCategoriesSelected ? 'All' : partCategory.join(',');

            common.showLoader();
            const data = {
                SchemeId: schemeId,
                FilePath: filePath,
                DistributorId: $('#distributorId').val(),
                PartCategory: categories
            };

            doAjax(homeController + 'UploadFocusPartsGroup', data, function (d) {
                common.hideLoader();
                focusPartEle.html(d);
                focusPartEle.addClass('mt-5');

                // Hide columns that we don't want to display
                $('.fp-column').css('display', 'none');
                $('.resizebox').addClass('col-sm-5');

                const errorMsgForNonMatchedGroups = $('#hdnErrorMsgForNonMatchedGroups').val();
                if (errorMsgForNonMatchedGroups) {
                    alert(errorMsgForNonMatchedGroups);
                }

                //if ($('.focusPart-benifitType:checked').val() === 'BenifitByGroup')
                //{
                //    $('.fp-column').css('display', 'none');
                //    $('.fpTargetDiv').css('display', 'flex');
                //    $('.resizebox').addClass('col-sm-6');
                //} else
                //{
                //    $('.fp-column').css('display', 'block');
                //    $('.fpTargetDiv').css('display', 'none');
                //    $('.resizebox').removeClass('col-sm-6');
                //}
            });
        },
        getFocusPartData = function (emptyRow) {
            // schemes.bindICheck();
            common.showLoader();

            const data = {
                SchemeId: schemeId,
                EmptyRow: emptyRow
            };

            doAjax(homeController + 'GetFocusGroupParts',
                data,
                function (d) {
                    common.hideLoader();
                    if (emptyRow) {
                        $('.focus-part-data').append(d);
                        $('.focus-part-data-mix').append(d);
                    } else {
                        $('.focus-part-data').html(d);
                        $('.focus-part-data-mix').html(d);
                    }
                    focusGroupIds = '0';

                    $('.focus-part-data-mix').find('.row').each(function () {
                        focusGroupIds += ',' + $(this).attr('id');
                    });
                    setAutoCompleteForGroup();
                });

            //partCategory = $('#PartCategory').val();
            //const allCategoriesSelected = $('#PartCategory option:not(:selected)').length === 0;
            //const categories = allCategoriesSelected ? 'All' : partCategory.join(',');

            //const data = {
            //    SchemeId: schemeId,
            //    EmptyRow: emptyRow,
            //    DistributorId: distributorId,
            //    PartCategory: categories
            //};

            //doAjax(homeController + 'GetFocusPartData',
            //    data,
            //    function (d) {
            //        common.hideLoader();
            //        if (emptyRow) {
            //            $('.focus-part-data').append(d);
            //            $('.focus-part-data-mix').append(d);
            //        } else {
            //            $('.focus-part-data').html(d);
            //            $('.focus-part-data-mix').html(d);
            //        }

            //        if ($('.focusPart-benifitType:checked').val() === 'BenifitByGroup') {
            //            $('input[value="BenifitByGroup"]').iCheck('check');
            //            $('input[value="BenifitIndividual"]').iCheck('uncheck');
            //            $('.fp-column').css('display', 'none');
            //            $('.fpTargetDiv').css('display', 'flex');
            //            $('.resizebox').addClass('col-sm-5');
            //        } else if ($('.focusPart-benifitType:checked').val() === 'BenifitIndividual') {
            //            $('input[value="BenifitByGroup"]').iCheck('uncheck');
            //            $('input[value="BenifitIndividual"]').iCheck('check');
            //            $('.fp-column').css('display', 'block');
            //            $('.fpTargetDiv').css('display', 'none');
            //            $('.resizebox').removeClass('col-sm-5');
            //        } else {
            //            $('input[value="BenifitByGroup"]').iCheck('check');
            //            $('input[value="BenifitIndividual"]').iCheck('uncheck');
            //            $('.fp-column').css('display', 'none');
            //            $('.fpTargetDiv').css('display', 'flex');
            //            $('.resizebox').addClass('col-sm-5');
            //        }
            //    });
            //schemes.bindICheck();
            //setTimeout(function () {
            //    $('.focusPart-benifitType').on('ifChecked',
            //        function () {
            //            if ($(this).val() === 'BenifitByGroup') {
            //                $('input[value="BenifitByGroup"]').iCheck('check');
            //                $('input[value="BenifitIndividual"]').iCheck('uncheck');
            //                $('.fp-column').css('display', 'none');
            //                $('.fpTargetDiv').css('display', 'flex');
            //                $('.resizebox').addClass('col-sm-5');
            //            } else {
            //                $('input[value="BenifitByGroup"]').iCheck('uncheck');
            //                $('input[value="BenifitIndividual"]').iCheck('check');
            //                $('.fp-column').css('display', 'block');
            //                $('.fpTargetDiv').css('display', 'none');
            //                $('.resizebox').removeClass('col-sm-5');
            //            }
            //        });
            //},
            //1000);


        },

        setAutoCompleteForGroup = function () {
            // Ref - https://www.devbridge.com/sourcery/components/jquery-autocomplete/#jquery-autocomplete-response-format

            const url = `/Home/SearchGroup?partCategory=${partCategory.toString()}&&distributorId=${distributorId}&&groupIds=${focusGroupIds}`;

            const options = {
                serviceUrl: url,
                triggerSelectOnValidInput: false,
                showNoSuggestionNotice: 'true',
                noSuggestionNotice: 'No matching group found',
                preventBadQueries: true,
                deferRequestBy: 1000,
                minChars: 3,
                onSelect: function (suggestion) {
                    //console.log(`You selected: ${suggestion.value}, ${suggestion.data}`); 

                    const groupName = suggestion.value;
                    const groupId = suggestion.data;
                    if (groupName) {
                        const row = $('.auto-complete').parents('.row');

                        focusGroupIds += "," + groupId;
                        $(row).attr('id', groupId);
                        $('.auto-complete').val(groupName);
                        $('.product').html('All parts selected');
                        $('.auto-complete').removeAttr('autocomplete');
                        $('.auto-complete').removeClass('auto-complete');
                    }
                }
            };

            $('.auto-complete').devbridgeAutocomplete(options);
        },

        openPartsPopup = function () {
            $('#hdnGroupPopup').val(200);
            document.getElementById("product-parts").style.display = "block";
        },

        getGroupParts = function (node) {
            common.showLoader();

            const row = $(node).parent().parent().parent();
            var groupId = $(row).attr('id');

            const data = {
                ProductIds: $(row).children('.product-ids').val(),
                GroupId: groupId,
                DistributorId: distributorId
            };

            doAjax(homeController + 'GetGroupParts', data, function (d) {

                $('#focus-parts-model').html(d);
                openPartsPopup();
                common.hideLoader();
            });
        },

        updateFocusParts = function (groupId) {
            common.showLoader();
            var productIds = "";
            var totalProduct = 0;
            $(".icheck_n:checked").each(function (index) {
                var val = $(this).val();
                productIds += productIds.length > 0 ? "," + val : val;
                totalProduct += 1;
            });

            const rowId = `#${groupId}`;
            const row = $(rowId);

            if (totalProduct > 0) {

                $(row).find('.product').html(`${totalProduct} parts selected`);
                $(row).children('.product-ids').val(productIds.toString());
            }
            else {
                $(row).find('.product').html('All parts selected');
                $(row).children('.product-ids').val("");
            }
            // hide popup
            closePartsPopup();
            common.hideLoader();
        },

        getFmsPartData = function () {
            const data = { schemeId: schemeId };
            doAjax(homeController + 'GetFmsPartData',
                data,
                function (d) {
                    common.hideLoader();
                    if (d.ResultFlag === 1) {
                        $('#startRange').val(d.Data.StartRange);
                        $('#endRange').val(d.Data.EndRange);
                        $('#FValue').val(d.Data.FValue);
                        $('#MValue').val(d.Data.MValue);
                        $('#SValue').val(d.Data.SValue);
                        $('#ddlPartCreation').val(d.Data.PartCreations);

                    } else {
                        console.error(d.Message);
                    }
                });
        },
        savePartInfo = function (canGoToNextStep) {
            schemes.readElementsValues();

            partGroup = $('select#PartType').val();
            if (partGroup == undefined || partGroup == '') {
                common.LoadErrorMessage('Please select Part Group.');
                return;
            }
            if (partGroup === focusPartsGroup) {
                schemes.saveFocusPartData($('.focus-part-data-mix')).then(function (isSaved) {
                    if (isSaved) {
                        schemes.saveScheme(canGoToNextStep);
                    }
                }).catch(function (error) {
                    alert(error);
                });

            } else if (partGroup === fmsPartsGroup) {
                partCategory = $('#PartCategory').val().join(',');

                startRange = $('#startRange').val();
                endRange = $('#endRange').val();
                fValue = $('#FValue').val();
                mValue = $('#MValue').val();
                sValue = $('#SValue').val();
                partCreation = $('select#ddlPartCreation').val();

                const totalFms = parseFloat(fValue) + parseFloat(mValue) + parseFloat(sValue);
                if (totalFms !== 100) {
                    alert(fmsValueErrorMsg);
                    return;
                }

                if (!startRange || !endRange) {
                    alert('Please select both FMS Start Date and End Date.');
                    return;
                }

                if (!partCreation) {
                    alert('Please select part creation.');
                    return;
                }

                saveGroupsByFms().then(function (isSaved) {
                    if (isSaved) {
                        schemes.saveScheme(canGoToNextStep);
                    }
                }).catch(function (error) {
                    alert(error);
                });
            }
            else { schemes.saveScheme(canGoToNextStep); }
        },
        saveFocusPartData = function (main) {
            var catData = [];
            var index = 0;
            var length = main.find('.row').length;
            let promise;

            main.find('.row').each(function () {
                const tr = $(this);
                const cat = {
                    SchemeId: schemeId,
                    GroupId: tr.attr('id'),
                    ProductIds: tr.find('.product-ids').val()
                };

                catData.push(cat);
                index++;
                if (index === length) {
                    promise = new Promise(function (resolve, reject) {
                        doAjaxPostJson(homeController + 'AddFocusPart',
                            catData,
                            function (d) {
                                if (d.ResultFlag == 0) {
                                    reject(d.Message);
                                }
                                else {
                                    resolve(true);
                                }
                            });
                    });
                };
            });

            return promise;
        },
        saveGroupsByFms = () => {
            // Ensure these values are set before calling this function

            partCategory = $('#PartCategory').val();
            const allCategoriesSelected = $('#PartCategory option:not(:selected)').length === 0;
            const categories = allCategoriesSelected ? 'All' : partCategory.join(',');

            const fmsPartsGroupModel = {
                SchemeId: schemeId,
                DistributorId: distributorId,
                PartCategory: categories,
                StartDate: startRange,
                EndDate: endRange,
                FValue: fValue,
                MValue: mValue,
                SValue: sValue,
                PartCreation: partCreation
            };

            let promise = new Promise(function (resolve, reject) {
                doAjaxPostJson(apiController + 'SaveGroupsByFms', fmsPartsGroupModel, function (d) {
                    if (d.ResultFlag == 0) {
                        reject(d.Message);
                    }
                    else {
                        resolve(true);
                    }
                });
            });
            return promise;
        },
        // ---- CUSTOMER SEGMENT FUNCTIONS ----
        getTargetGrowth = function (id) {
            common.showLoader();
            doAjax(homeController + `GetTargetGrowth?SchemeId=${id}`,
                null,
                function (response) {
                    common.hideLoader();
                    if (id === 0) {
                        $('.targetGrowth-rows').append(response);
                    } else {
                        $('.targetGrowth-rows').html(response);
                    }

                    $('#targetGrowth').show();
                });
        },
        removeCurrentTarget = function (node) {
            if (confirm('Are you sure you want to remove current target growth?')) {
                if ($('.targetGrowth-rows .row').length <= 1) {
                    common.LoadErrorMessage('Min one row required, You can not delete this.'); return;
                }
                const row = $(node).parent().parent();
                $(row).remove();
            }
        },
        toggleCategoryDetails = function () {
            if ($('#btnShowCategoryDetails').text() === 'Show Category Details') {
                // Add one blank category by default if no record in database
                schemes.getCategoryData(schemeId);

                $('#btnShowCategoryDetails').text('Hide Category Details');
                $('#hdnIsCategoryDetailsVisible').val(true);
                $('#categoryDetails').show();
            } else {
                // Delete all categories because user don't want to save them
                schemes.deleteAllCategories(schemeId);

                $('#btnShowCategoryDetails').text('Show Category Details');
                $('#hdnIsCategoryDetailsVisible').val(false);
                $('#categoryDetails').hide();
            }
        },
        getCategoryData = function (id) {
            common.showLoader();
            doAjax(homeController + `GetCategories?SchemeId=${id}`,
                null,
                function (response) {
                    common.hideLoader();
                    if (id === 0) {
                        $('.category-rows').append(response);
                    } else {
                        $('.category-rows').html(response);
                    }
                });
        },
        deleteAllCategories = function () {
            doAjax(homeController + `DeleteAllCategories?SchemeId=${schemeId}`,
                null,
                function (response) {
                    common.hideLoader();
                    if (response.ResultFlag === 0) {
                        console.log(response.Message);
                    }
                });
        },
        removeCurrentCategory = function (node) {
            if (confirm('Are you sure you want to remove current category?')) {
                const row = $(node).parent().parent();
                const container = $(row).parent();
                const childrenCount = $(container).children().length;

                if (childrenCount > 1) {
                    $(row).remove();
                } else {
                    $(row).remove();
                    schemes.toggleCategoryDetails();
                }
            }
        },
        saveCustomerSegment = function (canGoToNextStep) {
            schemes.readElementsValues();

            // First target growth will be saved
            // After successful saving, save criteria if user made rows visible
            // In last call save scheme method
            schemes.saveTargetGrowth(canGoToNextStep);
        },
        saveTargetGrowth = function (canGoToNextStep) {
            const data = [];
            let isError = false, tgTbody = '';

            const tgRows = $('.targetGrowth-rows .row');
            $('#prvTargetGrowth').toggle(tgRows.length > 0);

            tgRows.each(function () {
                const tr = $(this);
                const min = tr.find('.Min').val();
                const max = tr.find('.Max').val();
                const growth = tr.find('.Growth').val();
                const minTarget = tr.find('.MinTarget').val();

                if (min && max && growth) {
                    data.push({ SchemeId: schemeId, Min: min, Max: max, Growth: growth, MinTarget: minTarget });
                    tgTbody += `<tr><td>${min}</td><td>${max}</td><td>${growth}</td><td>${minTarget}</td></tr>`;
                } else {
                    isError = true;
                }
            });

            if (isError) {
                //common.LoadErrorMessage('Please enter all fields for target growth.');
                //return false;
                const isCategoryDetailsVisible = $('#hdnIsCategoryDetailsVisible').val();
                if (isCategoryDetailsVisible === 'true') {
                    schemes.saveCategories(canGoToNextStep);
                } else {
                    schemes.saveScheme(canGoToNextStep);
                }
                return;
            }

            if (data.length > 0) {
                $('#prvTargetGrowth table tbody').html(tgTbody);
                common.showLoader();
                doAjaxPostJson(apiController + 'SaveTargetGrowth',
                    data,
                    function (d) {
                        common.hideLoader();
                        if (d.ResultFlag === 1) {
                            const isCategoryDetailsVisible = $('#hdnIsCategoryDetailsVisible').val();
                            if (isCategoryDetailsVisible === 'true') {
                                schemes.saveCategories(canGoToNextStep);
                            } else {
                                schemes.saveScheme(canGoToNextStep);
                            }
                        } else {
                            alert(d.Message);
                        }
                    });
            }
        },
        saveCategories = function (canGoToNextStep) {
            var catData = [];
            const cRows = $('.category-rows .row');
            $('#prvCategories').toggle(cRows.length > 0);
            let cTbody = '';

            cRows.each(function () {
                const tr = $(this);
                var cat = {
                    CategoryId: tr.attr('id'),
                    SchemeId: schemeId,
                    Category: tr.find('.category-name').val(),
                    MinAmount: tr.find('.min-price').val(),
                    MaxAmount: tr.find('.max-price').val()
                };

                if (cat.Category) {
                    if (catData.length > 0) {
                        const existData = catData.filter(x => x.Category === cat.Category);
                        if (existData.length > 0) {
                            alert(`${existData[0].Category}  category already exists.`);
                            return;
                        }
                    }
                    cTbody += `<tr><td>${cat.Category}</td><td>${cat.MinAmount}</td><td>${cat.MaxAmount}</td></tr>`;
                }

                catData.push(cat);
            });

            if (catData.length > 0) {
                $('#prvCategories table tbody').html(cTbody);

                common.showLoader();
                doAjaxPostJson(apiController + 'AddCategoryScheme',
                    catData,
                    function (d) {
                        common.hideLoader();
                        if (d.ResultFlag === 1) {
                            schemes.saveScheme(canGoToNextStep);
                        } else {
                            alert(d.Message);
                        }
                    });
            }
        },
        // ---- TARGET INFO FUNCTIONS ----
        setTargetInfoDates = function () {
            if ($('#hdnAction').val() === 'Add' || $('#PrevYearFromDate').val() === '') {
                // If we are adding scheme
                const prvYrFromDate = moment(startDate, 'MM-DD-YYYY').subtract(1, 'years').format('MM-DD-YYYY');
                const prvYrToDate = moment(endDate, 'MM-DD-YYYY').subtract(1, 'years').format('MM-DD-YYYY');

                //Change logic to calculate prvMoFromDate and prvMoToDate by client on 30-07-2020

                let daysDifference = moment(endDate).diff(moment(startDate), 'days');
                const prvMoFromDate = moment(startDate, 'MM-DD-YYYY').subtract(daysDifference, 'days').format('MM-DD-YYYY');
                const prvMoToDate = moment(startDate, 'MM-DD-YYYY').format('MM-DD-YYYY');

                //const prvMoFromDate = moment(startDate, 'MM-DD-YYYY').subtract(1, 'months').format('MM-DD-YYYY');
                //const prvMoToDate = moment(endDate, 'MM-DD-YYYY').subtract(1, 'months').format('MM-DD-YYYY');

                // Ref - https://bootstrap-datepicker.readthedocs.io/en/stable/methods.html
                $('#PrevYearFromDate').datepicker('update', prvYrFromDate);
                $('#PrevYearToDate').datepicker('update', prvYrToDate);
                $('#PrevMonthFromDate').datepicker('update', prvMoFromDate);
                $('#PrevMonthToDate').datepicker('update', prvMoToDate);
            }
        },
        generateTargets = function (isFromBtn) {
            prevYearFromDate = $('#PrevYearFromDate').val();
            prevYearToDate = $('#PrevYearToDate').val();
            prevMonthFromDate = $('#PrevMonthFromDate').val();
            prevMonthToDate = $('#PrevMonthToDate').val();

            if (prevYearFromDate && prevYearToDate && prevMonthFromDate && prevMonthToDate) {
                schemes.getTargetWorkShopData();
            } else {
                if (isFromBtn) {
                    common.LoadErrorMessage('One of the dates is found empty. Please select all dates.');
                }
                else {
                    common.LoadErrorMessage('Cannot retrieve target workshops as one of the dates is empty.');
                }
            }
        },
        getTargetWorkShopData = function () {
            const data = {
                SchemeId: schemeId,
                DistributorId: distributorId,
                PrevYearFromDate: prevYearFromDate,
                PrevYearToDate: prevYearToDate,
                PrevMonthFromDate: prevMonthFromDate,
                PrevMonthToDate: prevMonthToDate
            };

            common.showLoader();
            doAjax(homeController + 'GetTargetWorkShopData',
                data,
                function (response) {
                    common.hideLoader();
                    $('.target-workshop-data').html(response);
                    $('#targetWorkshops').show();
                    schemes.bindICheck();
                });
        },
        bindTargetWorkShopFromExcel = function (path) {
            common.showLoader();
            const data = {
                SchemeId: schemeId,
                DistributorId: distributorId,
                FilePath: path
            };

            doAjax(homeController + 'GetTargetWorkShopData',
                data,
                function (response) {
                    common.hideLoader();
                    $('.target-workshop-data').html(response);
                    schemes.bindICheck();

                    const errorMsgForNonMatchedFileWorkshops = $('#hdnErrorMsgForNonMatchedFileWorkshops').val();
                    if (errorMsgForNonMatchedFileWorkshops) {
                        alert(errorMsgForNonMatchedFileWorkshops);
                    }
                });
        },
        removeCurrentWorkshop = function (node) {
        if (confirm('Are you sure you want to remove current target workshop?')) {
            const row = $(node).closest("tr");
                $(row).remove();
            }
        },
        sortByGrowth = function () {
            let sortBy = '';
            let $current = $('#sortIcon');

            if ($current.hasClass('fa-sort')) {
                $current.removeClass('fa-sort').addClass('fa-sort-asc');
                sortBy = 'ASC';
            } else if ($current.hasClass('fa-sort-asc')) {
                $current.removeClass('fa-sort-asc').addClass('fa-sort-desc');
                sortBy = 'DESC';
            } else if ($current.hasClass('fa-sort-desc')) {
                $current.removeClass('fa-sort-desc').addClass('fa-sort');
                sortBy = '';
            }

            common.showLoader();
            const workshops = getWorkshopsFromTable();
            console.table(workshops);
            const data = {
                Workshops: JSON.stringify(workshops),
                DistributorId: distributorId,
                SchemeId: schemeId,
                SortBy: sortBy
            };

            doAjaxPostJson(homeController + 'SortByGrowth',
                data,
                function (response) {
                    $('.target-workshop-data').html(response);
                    schemes.bindICheck();
                    common.hideLoader();

                    // To reflect earlier selected sorting to end user
                    $current = $('#sortIcon');
                    if (sortBy === 'ASC') {
                        $current.removeClass('fa-sort').addClass('fa-sort-asc');
                    } else if (sortBy === 'DESC') {
                        $current.removeClass('fa-sort').addClass('fa-sort-desc');
                    }
                });
        },
        askGrowthPercentModal = function () {
            $('#growthCompPercentModal').modal({
                keyboard: true
            });
        },
        calculateGrowth = function () {
            growthCompPercentMinValue = $('#growthCompPercentMinValue').val();
            growthCompPercentBaseValue = $('#growthCompPercentBaseValue').val();

            if (!growthCompPercentMinValue || !growthCompPercentBaseValue) {
                alert('Please fill all fields.');
                return;
            }

            if (growthCompPercentMinValue <= 0 || growthCompPercentBaseValue <= 0) {
                alert('Percentage values cannot be zero.');
                return;
            }

            // Hide modal as it will not hide automatically
            $('#growthCompPercentModal').modal('hide');

            const workshops = getWorkshopsFromTable();

            const data = {
                Workshops: JSON.stringify(workshops),
                SchemeId: schemeId,
                GrowthCompPercentMinValue: growthCompPercentMinValue,
                GrowthCompPercentBaseValue: growthCompPercentBaseValue
            };

            doAjaxPostJson(homeController + 'CalculateGrowth',
                data,
                function (response) {
                    $('.target-workshop-data').html(response);
                    schemes.bindICheck();
                    common.hideLoader();
                });
        },
        getWorkshopsFromTable = function () {
            const workshops = [];
            const rows = $('#tblTargetWs > tbody > tr');

            rows.each(function () {
                const tr = $(this);
                const data = {
                    SchemeId: schemeId,
                    WorkShopId: tr.attr('id'),
                    WorkShopName: tr.find('.workshop-name').text(),
                    WorkShopCode: tr.find('.workshop-code').html(),
                    CustomerType: tr.find('.customer-type').text(),
                    PrevYearAvgSale: tr.find('.prev-year-sale').text(),
                    GrowthPercentage: tr.find('.growth-percentage').text(),
                    NewTarget: tr.find('.new-target').val(),
                    PrevMonthAvgSale: tr.find('.prev-month-sale').text(),
                    GrowthComparisonPercentage: tr.find('.growth-comparison-percentage').text(),
                    IsQualifiedAsDefault: tr.find('.icheck:checked').length > 0 ? true : false
                };
                workshops.push(data);
            });

            return workshops;
        },
        saveTargetInfo = function (canGoToNextStep) {
            schemes.readElementsValues();
            schemes.saveTargetWorkShopData(canGoToNextStep);
        },
        saveTargetWorkShopData = function (canGoToNextStep) {
            var tbody = '';
            const workshops = getWorkshopsFromTable();
            if (workshops.length < 1) {
                schemes.saveScheme(canGoToNextStep);
                return;
            }

            $('#prvTargetWorkshops').toggle(workshops.length > 0);
            workshops.forEach((workshop) => {
                tbody += `<tr>
                            <td>${workshop.WorkShopName}</td>
                            <td>${workshop.CustomerType}</td>
                            <td>${workshop.PrevYearAvgSale}</td>
                            <td>${workshop.GrowthPercentage}</td>
                            <td>${workshop.NewTarget}</td>
                            <td>${workshop.PrevMonthAvgSale}</td>
                            <td>${workshop.GrowthComparisonPercentage}</td>
                            <td>${workshop.IsQualifiedAsDefault ? 'Yes' : 'No'}</td>
                         </tr>`;
            });
            $('#prvTargetWorkshops table tbody').html(tbody);

            doAjaxPostJson(apiController + `AddTargetWorkshop?schemeId=${schemeId}`, workshops, function (d) {
                common.hideLoader();
                if (d.ResultFlag === 1) {
                    schemes.saveScheme(canGoToNextStep);
                } else {
                    alert(d.Message);
                }
            });
        },
        // --- REWARD INFO ---
        this.getTargetOverview = function () {
            const data = { SchemeId: schemeId };

            common.showLoader();
            doAjax(`${homeController}GetTargetOverview`, data, function (d) {
                common.hideLoader();
                $('#targetOverview').html(d);
            });
        },
        showLuckyDrawTabs = function (display) {
            if (display) {
                $('#add-category').show();

                // Mark true or false based on reward options selected
                $('.nav-item-cashback-mix').toggle(isCashback);
                $('.nav-item-assured-mix').toggle(isAssuredGift);
                $('.nav-item-gift-mix').toggle(isLuckyDraw);
                $('.nav-item-coupon-mix').toggle(isLuckyDraw);

                // Set preview divs
                $('#prv-rewardInfo-Cashback').toggle(isCashback);
                $('#prv-rewardInfo-AssureGift').toggle(isAssuredGift);
                $('#prv-rewardInfo-LuckyDraw').toggle(isLuckyDraw);
                $('#prv-rewardInfo-CouponAllocation').toggle(isLuckyDraw);

                // If cashback tab hidden, mark next tab as active
                if (!isCashback) {
                    // Manually trigger click
                    // Else previously selected tab pane content not removes
                    if (isAssuredGift) {
                        $('.nav-item-assured-mix .nav-link')[0].click();
                    } else if (isLuckyDraw) {
                        $('.nav-item-gift-mix .nav-link')[0].click();
                    }
                }
            } else {
                $('#add-category').hide();

                $('.nav-item-cashback-mix').show();
                $('.nav-item-assured-mix').hide();
                $('.nav-item-gift-mix').hide();
                $('.nav-item-coupon-mix').hide();

                // Set preview divs
                $('#prv-rewardInfo-Cashback').show();
                $('#prv-rewardInfo-AssureGift').hide();
                $('#prv-rewardInfo-LuckyDraw').hide();
                $('#prv-rewardInfo-CouponAllocation').hide();

                // Manually trigger click to show cashback tab
                // Else previously selected tab pane content not removes
                $('.nav-item-cashback-mix .nav-link')[0].click();
            }
        },
        // --- REWARD INFO : CASHBACK---
        showCashbackRangeModal = function () {
            $('#cashbackRangeModal').modal({
                keyboard: true
            });
        },
        getCashBackRangeData = function (emptyRow) {
            const data = { SchemeId: schemeId, EmptyRow: emptyRow };
            common.showLoader();
            doAjax(`${homeController}GetCashBackRangeData`,
                data,
                function (d) {
                    common.hideLoader();
                    if (emptyRow) {
                        $('#cashbackRangeModal .cashback-range-main-data').append(d);
                    } else {
                        $('#cashbackRangeModal .cashback-range-main-data').html(d);
                    }
                });
        },
        removeCurrentCashBackRange = function (node) {
            if (confirm('Are you sure you want to remove current cashback range?')) {
                const row = $(node).parent().parent();
                $(row).remove();
            }
        },
        removeCurrentCashBack = function (node) {
            if (confirm('Are you sure you want to remove current cashback?')) {
                if ($('.cashBack-content-data-mix .row').length <= 1) {
                    common.LoadErrorMessage('Min one row required, You can not delete this.'); return;
                }
                const row = $(node).parent().parent();
                $(row).remove();
            }
        },
        saveCashBackRangeData = function () {
            var catData = [];
            var index = 0;
            var html = '';
            const main = $('#cashbackRangeModal .cashback-range-main-data');

            var length = main.find('.row').length;
            main.find('.row').each(function () {
                const tr = $(this);
                const cat = {
                    CashbackRangeId: tr.attr('id'),
                    SchemeId: schemeId,
                    StartRange: tr.find('.start-range').val(),
                    EndRange: tr.find('.end-range').val()
                };

                if (cat.StartRange !== '') {
                    html += `<tr><td>${cat.StartRange}</td><td>${cat.EndRange}</td></tr>`;
                    catData.push(cat);
                }

                index++;
                if (index === length) {
                    $('#kt_tabs_CashBack table tbody').html(html);

                    common.showLoader();

                    // Use both POST and QueryString Parameters in Conjunction to make it work
                    doAjaxPostJson(`${apiController}AddCashBackRange?schemeId=${schemeId}`,
                        catData,
                        function (d) {
                            if (d.ResultFlag === 1) {
                                common.hideLoader();
                                schemes.getCashBackData(false);

                                // Hide modal as it will not hide automatically
                                $('#cashbackRangeModal').modal('hide');

                                //$('.cashback-range-main').hide();
                                //$('.cashback-data-main').show();
                            } else {
                                common.hideLoader();
                                alert(d.Message);
                            }
                        });
                }
            });
        },
        getCashBackData = function (emptyRow) {
            schemes.bindICheck();

            common.showLoader();
            const data = { SchemeId: schemeId, EmptyRow: emptyRow };
            doAjax(`${homeController}GetCashBackData`,
                data,
                function (d) {
                    common.hideLoader();
                    if (emptyRow) {
                        $('.cashBack-content-data').append(d);
                        $('.cashBack-content-data-mix').append(d);
                    } else {
                        $('.cashBack-content-data').html(d);
                        $('.cashBack-content-data-mix').html(d);
                    }
                });

            // Show options as per scheme type
            if (schemeType === cashback) {
                $('.both-cbAg-applicable-data').hide();
            } else {
                $('.both-cbAg-applicable-data').toggle(isCashback && isAssuredGift);
                $('.both-agLd-applicable-data').toggle(isAssuredGift && isLuckyDraw);
            }
        },
        updateCashbackCriteria = function (cashbackCriteria) {
            const data = { SchemeId: schemeId, CashbackCriteria: cashbackCriteria };
            common.showLoader();
            doAjaxPost(apiController + 'UpdateCashbackCriteria',
                data,
                function (d) {
                    if (d.ResultFlag === 0) {
                        common.hideLoader();
                        common.LoadErrorMessage(d.Message);
                    }
                });
        },
        saveCashBackData = function (canGoToNextStep) {
            const cashbacks = [];
            let index = 0;
            let tbody = '';

            const rows = $('.cashBack-content-data-mix .row');
            const length = rows.length;
            $('#prvCashback').toggle(length > 0);

            rows.each(function () {
                const tr = $(this);
                var benefits = [];
                var cashbackRanges = [];

                tr.find('.cashback-range').each(function () {
                    const div = $(this);

                    benefits.push({
                        FromAmount: div.find('.cashback-range-input').attr('data-fromAmount'),
                        ToAmount: div.find('.cashback-range-input').attr('data-toAmount'),
                        Value: div.find('.cashback-range-input').val()
                    });

                    cashbackRanges.push({
                        CashbackRangeId: div.attr('id'),
                        SchemeId: schemeId,
                        Percentage: div.find('.cashback-range-input').val()
                    });
                });

                const cashbackRec = {
                    CashbackId: tr.attr('id'),
                    SchemeId: schemeId,
                    FromAmount: tr.find('.from-amount').val(),
                    ToAmount: tr.find('.to-amount').val(),
                    Benifit: JSON.stringify(benefits)
                };

                if (cashbackRec.FromAmount !== '') {
                    // Prepare preview data
                    tbody += `<tr><td>${cashbackRec.FromAmount}</td><td>${cashbackRec.ToAmount}</td>`;
                    if (index === 0) {
                        $('#prvCashback .cashback-th').remove();
                        $('#prvCashback .cashback-td').remove();
                        tr.find('.cashback-range').each(function () {
                            const div = $(this);
                            $('#prvCashback table thead tr')
                                .append(`<th class="cashback-th">${div.find('label').html()}</th>`);
                        });
                    }

                    tr.find('.cashback-range').each(function () {
                        const div = $(this);
                        tbody += `<td class="cashback-td">${div.find('.cashback-range-input').val()}</td>`;
                    });

                    tbody += '</tr>';
                }

                cashbackRec.lstCashbackRange = cashbackRanges;
                cashbacks.push(cashbackRec);
                index++;

                if (index === length) {
                    $('#prvCashback table tbody').html(tbody);

                    common.showLoader();
                    doAjaxPostJson(apiController + `AddCashBack?schemeId=${schemeId}`,
                        cashbacks,
                        function (d) {
                            if (d.ResultFlag === 1) {
                                if (schemeType === cashback) {
                                    schemes.saveScheme(canGoToNextStep);
                                }

                            } else {
                                common.hideLoader();
                                alert(d.Message);
                            }
                        });
                }
            });
        },
        // --- REWARD INFO : LUCKY DRAW ---
        setRewardInfoChecks = () => {
            areBothCbAgApplicable = $('#hdnBothCbAgApplicable').val();
            areBothAgLdApplicable = $('#hdnBothAgLdApplicable').val();

            if (areBothCbAgApplicable) {
                $("input[name=BothCbAgApplicable][value='Yes']").prop('checked', true);
            } else {
                $("input[name=BothCbAgApplicable][value='No']").prop('checked', true);
            }

            if (areBothAgLdApplicable) {
                $("input[name=BothAgLdApplicable][value='Yes']").prop('checked', true);
            } else {
                $("input[name=BothAgLdApplicable][value='No']").prop('checked', true);
            }

            schemes.bindICheck();
        },
        getAssuredGiftData = function (emptyRow) {
            common.showLoader();
            doAjax(homeController + `GetAssuredGiftData?SchemeId=${schemeId}&EmptyRow=${emptyRow}`,
                null,
                function (d) {
                    common.hideLoader();
                    if (emptyRow) {
                        $('.assured-gift-data').append(d);
                        $('.assured-gift-mix-data').append(d);
                    } else {
                        $('.assured-gift-data').html(d);
                        $('.assured-gift-mix-data').html(d);
                    }
                    schemes.bindMultiSelect();
                });
        },
        removeCurrentAssuredGift = function (node) {
            if (confirm('Are you sure you want to remove current row?')) {
                const row = $(node).parent().parent();
                $(row).remove();
            }
        },
        getLuckyDrawData = function (emptyRow) {
            common.showLoader();
            doAjax(homeController + `GetGiftManagementData?SchemeId=${schemeId}&EmptyRow=${emptyRow}`,
                null,
                function (d) {
                    common.hideLoader();
                    if (emptyRow) {
                        $('.gift-management-data').append(d);
                    } else {
                        $('.gift-management-data').html(d);
                    }
                    schemes.bindMultiSelect();
                    schemes.bindGiftFileUpload();
                });

            const data = { SchemeId: schemeId };
            doAjaxPost(apiController + 'GetCanTakeMoreThanOneGift',
                data,
                function (d) {
                    if (d.ResultFlag === 1) {
                        $('input[name=CanTake][value="Yes"]').prop('checked', true);
                        $('#maxGiftAllowed').val(d.Data);
                    } else {
                        $('input[name=CanTake][value="No"]').prop('checked', true);
                    }
                    schemes.bindICheck();
                });

            if (schemeType === luckyDraw) {
                $('.can-take-more-then-one-data').show();
            } else if (schemeType === cashback || schemeType === fixedPrice) {
                $('.can-take-more-then-one-data').hide();
            }
        },
        removeCurrentLuckyDraw = function (node) {
            if (confirm('Are you sure you want to remove current row?')) {
                const row = $(node).parent().parent();
                $(row).remove();
            }
        },
        getCouponAllocationData = function (emptyRow) {
            common.showLoader();
            doAjax(homeController + `GetQualifyingCriteriaData?SchemeId=${schemeId}&EmptyRow=${emptyRow}`,
                null,
                function (d) {
                    common.hideLoader();
                    if (emptyRow) {
                        $('.qualifying-criteria-data').append(d);
                    } else {
                        $('.qualifying-criteria-data').html(d);
                    }
                });
        },
        removeCurrentCouponAllocation = function (node) {
            if (confirm('Are you sure you want to remove current row?')) {
                const row = $(node).parent().parent();
                $(row).remove();
            }
        },
        saveAssuredGiftData = function () {
            const catData = [];
            let index = 0;
            let tbody = '';

            const rows = $('.assured-gift-mix-data .row');
            const length = rows.length;
            $('#prvAssuredGift').toggle(length > 0);

            rows.each(function () {
                const tr = $(this);

                var categories = '';
                $.each(tr.find('.category-gift option:selected'),
                    function () {
                        if ($(this).val() === 'All') {
                            categories = 'All';
                            return false;
                        } else {
                            if (categories === '') {
                                categories = $(this).val();
                            } else {
                                categories = categories + ',' + $(this).val();
                            }
                        }
                    });

                const cat = {
                    AssuredGiftId: tr.attr('id'),
                    SchemeId: schemeId,
                    Target: tr.find('.target-input').val(),
                    Point: tr.find('.point-input').val(),
                    Reward: tr.find('.reward-input').val(),
                    Categories: categories.toString()
                };
                if (cat.Target !== '' && cat.Categories !== '') {
                    tbody += `<tr>
                                <td>${cat.Categories}</td>
                                <td>${cat.Target}</td>
                                <td>${cat.Point}</td>
                                <td>${cat.Reward}</td>
                             </tr>`;
                }
                catData.push(cat);
                index++;

                if (index === length) {
                    $('#prvAssuredGift table tbody').html(tbody);

                    common.showLoader();
                    doAjaxPostJson(apiController + 'AddAssuredGift',
                        catData,
                        function (d) {
                            common.hideLoader();
                            if (d.ResultFlag === 0) {
                                alert(d.Message);
                            }
                        });
                }
            });
        },
        saveLuckyDrawData = function () {
            const catData = [];
            let index = 0;
            let tbody = '';

            const rows = $('.gift-management-data .row');
            const length = rows.length;
            $('#prvLuckyDraw').toggle(length > 0);

            rows.each(function () {
                const tr = $(this);

                var categories = '';
                $.each(tr.find('.category-gift option:selected'),
                    function () {
                        if ($(this).val() === 'All') {
                            categories = 'All';
                            return false;
                        } else {
                            if (categories === '') {
                                categories = $(this).val();
                            } else {
                                categories = categories + ',' + $(this).val();
                            }
                        }
                    });

                var cat = {
                    GiftId: tr.attr('id'),
                    SchemeId: schemeId,
                    Gift: tr.find('.gift-input').val(),
                    Qty: tr.find('.qty-input').val(),
                    DrawOrder: tr.find('.draw-order-input').val(),
                    Categories: categories.toString(),
                    ImagePath: tr.find('.hdnImgPath').val()
                };
                if (cat.Gift !== '' && cat.Categories !== '') {
                    if (catData.length > 0 && cat.DrawOrder.length > 0) {
                        const existData = catData.filter(x => x.DrawOrder === cat.DrawOrder);
                        if (existData.length > 0) {
                            alert(`DrawOrder-${existData[0].DrawOrder} already exists.`);
                            return;
                        }

                    }
                    tbody += `<tr>
                                <td>${cat.Categories}</td>
                                <td>${cat.Gift}</td>
                                <td>${cat.Qty}</td>
                                <td>${cat.DrawOrder}</td>
                                <td><img src='${cat.ImagePath}' alt='Gift' height=50 width=50/></td>
                             </tr>`;
                }
                catData.push(cat);
                index++;
                if (index === length) {
                    $('#prvLuckyDraw table tbody').html(tbody);

                    common.showLoader();
                    doAjaxPostJson(apiController + 'AddGifManagement',
                        catData,
                        function (d) {
                            common.hideLoader();
                            if (d.ResultFlag === 0) {
                                alert(d.Message);
                            }
                        });
                }
            });
        },
        saveCouponAllocationData = function () {
            const catData = [];
            let index = 0;
            let tbody = '';

            const rows = $('.qualifying-criteria-data .row');
            const length = rows.length;
            $('#prvCouponAllocation').toggle(length > 0);

            rows.each(function () {
                const tr = $(this);
                const cat = {
                    QualifyCriteriaId: tr.attr('id'),
                    SchemeId: schemeId,
                    AmountUpto: tr.find('.from-amount').val(),
                    Type: tr.find('.type').val(),
                    NumberOfCoupons: tr.find('.number-Of-coupons').val(),
                    TypeValue: tr.find('.product').val(),
                    AdditionalCouponAmount: tr.find('.additional-coupon-amount').val(),
                    AdditionalNumberOfCoupons: tr.find('.additional-number-of-coupon').val()
                };
                if (cat.AmountUpto !== '') {
                    tbody += `<tr>
                                 <td>${cat.AmountUpto}</td>
                                 <td>${cat.Type}</td>
                                 <td>${cat.TypeValue !== '' ? tr.find('.product option:selected').text() : ''}</td>
                                 <td>${cat.NumberOfCoupons}</td>
                                 <td>${cat.AdditionalCouponAmount}</td>
                                 <td>${cat.AdditionalNumberOfCoupons}</td>
                             </tr>`;
                }
                catData.push(cat);
                index++;

                if (index === length) {
                    $('#prvCouponAllocation table tbody').html(tbody);

                    common.showLoader();
                    doAjaxPostJson(apiController + 'AddQualifyCriteria',
                        catData,
                        function (d) {
                            common.hideLoader();
                            if (d.ResultFlag === 0) {
                                alert(d.Message);
                            }
                        });
                }
            });
        },
        saveRewardInfo = function (canGoToNextStep) {
            schemes.readElementsValues();

            if (schemeType === cashback) {
                schemes.saveCashBackData(canGoToNextStep);
            } else if (schemeType === luckyDraw) {
                schemes.saveCashBackData(canGoToNextStep);
                schemes.saveAssuredGiftData();
                schemes.saveLuckyDrawData();
                schemes.saveCouponAllocationData();

                schemes.saveScheme(canGoToNextStep);
            }
        },
        setPreviewFields = function () {
            schemes.readElementsValues();

            setBasicInfoPreview();
            setPartInfoPreview();
            setCustomerSegmentPreview();
            setTargetInfoPreview();
            setRewardInfoPreview();
        },
        setBasicInfoPreview = () => {
            // Set thumbnail image if available else default one
            const noPhotoPath = '/assets/images/NoPhotoAvailable.png';
            $('#prvThumbnailImg').load(thumbnailImage,
                function (response, status, xhr) {
                    $(this).attr('src', status === 'error' ? noPhotoPath : thumbnailImage);
                });

            // Set background image of prvBannerDiv and if not available then show background color
            //$('#prvBannerImg').attr('src', bannerImage ? bannerImage : noPhotoPath);

            $('#prvSchemeName').text($('#SchemeName').val());
            $('#prvSchemeDate').text(`${moment(startDate).format(dateFormat)} - ${moment(endDate).format(dateFormat)}`);
            $('#prvSchemeType').text(`Type: ${schemeType}`);

            $('#prvSubSchemeType').text(subSchemeType);
            $('.prv-sub-scheme-type').toggle(schemeType === cashback);
            $('.prv-reward-options').toggle(schemeType === luckyDraw);
            if (schemeType === luckyDraw) {
                const rewardOptions = [];
                if (isCashback) {
                    rewardOptions.push($("label[for='chkCashback']").text());
                }

                if (isAssuredGift) {
                    rewardOptions.push($("label[for='chkAssuredGift']").text());
                }

                if (isLuckyDraw) {
                    rewardOptions.push($("label[for='chkLuckyDraw']").text());
                }

                $('#prvRewardOptions').text(rewardOptions.join(', '));
            }

            $('#prvDistributor').text($('#distributorId option:selected').text());
            $('#prvPaybackPeriod').text($('#FrequencyId option:selected').text());
            $('#prvPaybackType').text($('#DispersalTypeId option:selected').text());
        },
        setPartInfoPreview = () => {
            $('#prvFpg').toggle(partGroup === focusPartsGroup);
            $('#prvFmsPartGroup').toggle(partGroup === fmsPartsGroup);

            const partCategories = [];
            $('#PartCategory option:selected').each(function () {
                partCategories.push($(this).text());
            });
            $('#prvPartCategory').text(partCategories.length > 0 ? partCategories.join(', ') : 'None selected');

            $('#prvPartGroup').text(partGroup);
            if (partGroup === fmsPartsGroup) {
                $('#prvFmsDateRange')
                    .text(`${moment(startRange).format(dateFormat)} - ${moment(endRange).format(dateFormat)}`);
                $('#prvFPercentage').text(fValue);
                $('#prvMPercentage').text(mValue);
                $('#prvSPercentage').text(sValue);
                $('#prvPartCreation').text(partCreation);
            }

            // Tabular data is set when particular tab's record is saved
            // In function - saveFocusPartData()
        },
        setCustomerSegmentPreview = () => {
            const branchNames = [], salesExecNames = [], customerTypeNames = [];

            $('#ddlBranchCode option:selected').each(function () {
                branchNames.push($(this).text());
            });

            $('#ddlSalesExecutive option:selected').each(function () {
                salesExecNames.push($(this).text());
            });

            $('#ddlCustomerType option:selected').each(function () {
                customerTypeNames.push($(this).text());
            });

            $('#prvBranchCode').text(branchNames.length > 0 ? branchNames.join(', ') : 'None selected');
            $('#prvSalesPerson').text(salesExecNames.length > 0 ? salesExecNames.join(', ') : 'None selected');
            $('#prvCustomerType').text(customerTypeNames.length > 0 ? customerTypeNames.join(', ') : 'None selected');

            // Tabular data is set when particular tab's record is saved
            // In function - saveTargetGrowth() and saveCategories()
        },
        setTargetInfoPreview = () => {
            $('#prvYearDateRange')
                .text(`${moment(prevYearFromDate).format(dateFormat)} - ${moment(prevYearToDate).format(dateFormat)}`);
            $('#prvMonthDateRange')
                .text(`${moment(prevMonthFromDate).format(dateFormat)} - ${moment(prevMonthToDate)
                    .format(dateFormat)}`);

            // Tabular data is set when particular tab's record is saved
            // In function - saveTargetWorkShopData()
        },
        setRewardInfoPreview = () => {
            showLuckyDrawTabs(schemeType === luckyDraw);

            $('#prvCashbackCriteria').text(cashbackCriteria);
            $('#prvAreBothCbAgApplicable').text(areBothCbAgApplicable === true ? 'Yes' : 'No');
            $('#prvCanTakeMoreThanOneGift').text(canTakeMoreThanOneGift === true ? 'Yes' : 'No');
            $('#prvMaxGiftsAllowed').text(maxGiftsAllowed.length > 0 ? maxGiftsAllowed : 'Not specified');

            // Tabular data is set when particular tab's record is saved
            // In function - saveCashBackData(), saveAssuredGiftData(), saveLuckyDrawData() and saveCouponAllocationData()
        },

        // --- OTHER FUNCTIONS ---
        deleteScheme = function (id) {
            if (confirm('Do you want to Delete ?')) {
                common.showLoader();
                var data = {
                    SchemeId: id
                };
                doAjaxPost(apiController + "DeleteScheme?SchemeId=" + id,
                    data,
                    function (d) {
                        admin.LoadPartialView('/home/SchemesPartialView');
                    });
            }
        },
        saveActiveScheme = function (schemeId, val) {
            common.showLoader();
            var data = {
                SchemeId: schemeId,
                IsActive: val
            };
            doAjaxPost(apiController + "SaveSchemeActive",
                data,
                function (d) {
                    if (d.ResultFlag == 1) {
                        window.location.reload();
                    }
                    else {
                        common.ShowMessage(d);
                    }
                    common.hideLoader();
                }
            );
        },
        getActiveScheme = function () {
            common.showLoader();
            var data = {
                SchemeId: schemeId
            };
            doAjaxPost(apiController + "GetSchemeActive",
                data,
                function (d) {
                    if (d.Data) {
                        $(".active-scheme").hide();
                        $(".inactive-scheme").show();
                    } else {
                        $(".active-scheme").show();
                        $(".inactive-scheme").hide();
                    }
                    common.hideLoader();
                });
        },
        bindICheck = function () {
            $('.icheck').iCheck('destroy');
            $('.icheck').iCheck({
                checkboxClass: 'icheckbox_square-blue',
                radioClass: 'iradio_square-blue',
                increaseArea: '20%'
            });
        },
        showWorkShopLabels = function () {

            var dId = $("#distributorId").val();
            if (dId === "") {
                alert("Please select distributor.");
                return;
            }

            $(".workshop-labels").slideToggle({
                direction: "up"
            },
                400);

            if (schemeId) {
                getWorkShopLabelData(false);
            } else {
                getWorkShopLabelData(true);
            }

        },
        getWorkShopLabelData = function (EmptyRow) {
            common.showLoader();

            var dId = $("#distributorId").val();
            var partCreation = $("#PartCreations").val();
            var partCategory = $("#PartCategory").val();
            var fValue = $("#FValue").val();
            var mValue = $("#MValue").val();
            var sValue = $("#SValue").val();

            var data = {
                SchemeId: schemeId,
                EmptyRow: EmptyRow,
                DistributorId: dId,
                PartCreation: partCreation,
                PartCategory: partCategory,
                FValue: fValue,
                MValue: mValue,
                SValue: sValue
            };

            doAjax(homeController + "getLabelData",
                data,
                function (d) {
                    common.hideLoader();
                    if (EmptyRow) {
                        $(".workshop-labels-data").append(d);
                    } else {
                        $(".workshop-labels-data").html(d);
                    }
                });
        },
        getWorkShopCriteriaData = function (EmptyRow, th) {
            common.showLoader();

            var labelId = $(th).parents(".kt-form__section").attr("id");

            doAjax(homeController + "getLabelCriteriaData?LabelId=" + labelId + "&EmptyRow=" + EmptyRow,
                null,
                function (d) {
                    common.hideLoader();
                    if (EmptyRow) {
                        $(th).parents(".kt-form__section").find(".workshop-criteria-data").append(d);
                    } else {
                        $(th).parents(".kt-form__section").find(".workshop-criteria-data").html(d);
                    }
                });
        },
        showWorkShopLabelResult = function () {
            var dId = $("#distributorId").val();
            var targetGroupArr = [];
            console.log("length", $(".workshop-labels-data .row").length);
            common.showLoader();
            if ($(".workshop-labels-data .row").length > 0) {
                $(".workshop-labels-data .row").each(function () {
                    var tr = $(this);
                    var data = {
                        DistributorId: dId,
                        CriteriaId: tr.attr('id'),
                        SchemeId: schemeId,
                        GroupId: tr.find('.group').val(),
                        ProductId: tr.find('.product').val(),
                        Condition: tr.find('.condition').val(),
                        Value: tr.find('.benifit-value').val(),
                        Qty: tr.find('.target-group-qty').val(),
                        Operator: tr.find('.operator').val(),
                        SalesExecutiveId: $("#ddlSalesExecutive").val()
                    };

                    targetGroupArr.push(data);
                });
            } else {
                var data = {
                    DistributorId: dId,
                    CriteriaId: "",
                    SchemeId: schemeId,
                    GroupId: "",
                    ProductId: "",
                    Condition: "",
                    Value: "",
                    Qty: "",
                    Operator: "",
                    SalesExecutiveId: $("#ddlSalesExecutive").val()
                };
                targetGroupArr.push(data);
            }
            console.log("dId", dId, schemeId, targetGroupArr);
            doAjaxPostJson(homeController + "getWorkShopDataByCriteria",
                targetGroupArr,
                function (d) {
                    $("#targetWorkshopResults").html(d);
                    schemes.bindICheck();
                    schemes.bindCheckAll();
                    common.hideLoader();
                });
        },
        bindOtherFileUploads = function () {
            var thumbImgUploader = new qq.FineUploader({
                element: $('#manual-fine-uploader-1')[0],
                action: '/Home/upload',
                type: "post",
                minSizeLimit: 0,
                autoUpload: true,
                text: {
                    uploadButton: '<i class="icon-plus icon-white"></i> Select Files'
                },
                validation: {
                    acceptFiles: "image/*",
                    sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
                },
                callbacks: {
                    onComplete: function (id, filename, responseJSON) {
                        if (responseJSON.success == true) {
                            $("#ThumbnailImage").val(responseJSON.filename);

                        } else {
                            alert("Error: " + responseJSON.message);
                        }
                    }
                }
            });

            var bannerImgUploader = new qq.FineUploader({
                element: $('#manual-fine-uploader-2')[0],
                action: '/Home/upload',
                type: "post",
                minSizeLimit: 0,
                autoUpload: true,
                text: {
                    uploadButton: '<i class="icon-plus icon-white"></i> Select Files'
                },
                validation: {
                    acceptFiles: "image/*",
                    sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
                },
                callbacks: {
                    onComplete: function (id, filename, responseJSON) {
                        if (responseJSON.success == true) {
                            $("#BannerImage").val(responseJSON.filename);

                        } else {
                            alert("Error: " + responseJSON.message);
                        }
                    }
                }
            });

            const focusPartsGroupUploader = new qq.FineUploader({
                element: $('#manual-fine-uploader-focusPartMix')[0],
                action: '/Home/upload',
                type: 'post',
                minSizeLimit: 0,
                autoUpload: true,
                text: {
                    uploadButton: '<i class="fa fa-file-excel-o"></i> Select Files'
                },
                validation: {
                    acceptFiles:
                        '.xls,.xlsx,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    allowedExtensions: ['xls', 'xlsx'],
                    sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
                },
                callbacks: {
                    onComplete: function (id, filename, responseJson) {
                        if (responseJson.success === true) {
                            bindFocusPartFromExcel(responseJson.filename, $('.focus-part-data-mix'));
                            //focusPartsGroupUploader.reset();
                        } else {
                            alert(`Error: ${responseJson.message}`);
                        }
                    }
                }
            });

            //var workshopUploader = new qq.FineUploader({
            //    element: $('#manual-fine-uploader-workshop')[0],
            //    action: '/Home/upload',
            //    type: "post",
            //    minSizeLimit: 0,
            //    autoUpload: true,
            //    text: {
            //        uploadButton: '<i class="fa fa-file-excel-o"></i> Select Files'
            //    },
            //    validation: {
            //        acceptFiles:
            //            ".xls,.xlsx,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            //        allowedExtensions: ['xls', 'xlsx'],
            //        sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
            //    },
            //    callbacks: {
            //        onComplete: function (id, filename, responseJSON)
            //        {
            //            if (responseJSON.success === true)
            //            {
            //                bindWorkshopFromExcel(responseJSON.filename);
            //            } else
            //            {
            //                alert("Error: " + responseJSON.message);
            //            }
            //        }
            //    }
            //});
        },
        AddRowTargetGrowth = function () {
            var html = `<div class="row more">
                        <div class="col-sm-4">
                            <div class="form-group">
                                <label>Monthly Purchase From</label>
                                <input type="text" class="form-control Min" placeholder="Monthly Purchase From" />
                            </div>
                        </div>

                        <div class="col-sm-4">
                            <div class="form-group">
                                <label>Monthly Purchase To</label>
                                <input type="text" class="form-control Max" placeholder="Monthly Purchase To" />
                            </div>
                        </div>

                        <div class="col-sm-4">
                            <div class="form-group">
                                <label>Growth (%)</label>
                                <input type="text" class="form-control Growth" placeholder="Growth" />
                            </div>
                        </div>
                    </div>`
            $(".TargetGrowth").append(html);
        },
        AddTargetGrowth = function () {
            $(".TargetGrowth input").val('');
            $(".TargetGrowth .more").remove();
            var data = { SchemeId: schemeId };
            doAjaxPost(apiController + "GetTargetGrowth",
                data,
                function (d) {
                    if (d.ResultFlag == 1) {
                        var response = d.Data;
                        var html = "";
                        for (var item in response) {
                            item = response[item];
                            html += `<div class="row more">

                        <div class="col-sm-3">
                            <div class="form-group">
                                <label>Monthly Purchase From</label>
                                <input type="text" class="form-control Min" placeholder="Monthly Purchase From" value=` +
                                item.Min +
                                ` />
                            </div>
                        </div>

                        <div class="col-sm-3">
                            <div class="form-group">
                                <label>Monthly Purchase To</label>
                                <input type="text" class="form-control Max" placeholder="Monthly Purchase To" value=` +
                                item.Max +
                                ` />
                            </div>
                        </div>

                        <div class="col-sm-2">
                            <div class="form-group">
                                <label>Growth (%)</label>
                                <input type="text" class="form-control Growth" placeholder="Growth" value=` +
                                item.Growth +
                                ` />
                            </div>
                        </div>

                        <div class="col-sm-2">
                            <div class="form-group">
                                <label>Min. Target</label>
                                <input type="text" class="form-control MinTarget" placeholder="Growth" value=` +
                                item.MinTarget +
                                ` />
                            </div>
                        </div>
                    </div>`
                        }
                        $(".TargetGrowth").prepend(html)
                    }
                });
        },
        bindRoIncharge = function (dId) {
            var data = {
                DistributorId: dId.length > 0 ? dId : 0
            };
            doAjaxPost(homeController + "GetRoIncharge",
                data,
                function (d) {
                    var selectEle = $("#ddlRoIncharge");
                    selectEle.empty();
                    selectEle.append('<option value="">-- Select --</option>');
                    selectEle.append('<option value="All">All Ro Incharge</option>');
                    $.each(d.Data,
                        function (key, roi) {
                            selectEle.append(
                                '<option value="' + roi.UserId + '">' + roi.UserName + '</option>');
                        });
                });
        },
        bindSalesExecutive = function (roId) {
            var data = {
                RoUserId: roId,
                isAllSelected: roId == "All" ? true : false,
                distributorId: $("#distributorId").val()
            };
            doAjaxPost(homeController + "GetSalesExecutive",
                data,
                function (d) {
                    var selectEle = $("#ddlSalesExecutive");
                    selectEle.empty();
                    selectEle.append('<option value="">-- Select --</option>');
                    selectEle.append('<option value="All">All Sales Executive</option>');
                    $.each(d.Data,
                        function (key, se) {
                            selectEle.append(
                                '<option value="' + se.UserId + '">' + se.UserName + '</option>');
                        });
                });

        },
        bindSalesExecutiveByBranchCode = function () {
            const data = { BranchCodes: branchCodes };

            doAjaxPost(homeController + 'GetSalesExecutiveByBranchCodes',
                data,
                function (d) {
                    if (d.ResultFlag === 1) {
                        const selectEle = $('#ddlSalesExecutive');
                        selectEle.empty();

                        var options = [];
                        $.each(d.Data,
                            function (key, se) {
                                options.push({ label: se.UserName, title: se.UserName, value: se.UserId });
                            });

                        // Ref - http://davidstutz.de/bootstrap-multiselect/#methods
                        selectEle.multiselect('dataprovider', options);
                        selectEle.val(salesExecutives.split(","));
                    } else {
                        common.ShowMessage(d);
                    }

                });
        },
        bindBranchCodeByDistributorId = function () {
            const data = { DistributorId: distributorId };

            doAjaxPost(homeController + 'GetBranchCodesByDistributorId',
                data,
                function (d) {
                    if (d.ResultFlag === 1) {
                        const selectEle = $('#ddlSalesExecutive');
                        selectEle.empty();
                        selectEle.multiselect('dataprovider', null);

                        const selectOut = $('#ddlBranchCode');
                        selectOut.empty();

                        var options = [];
                        $.each(d.Data,
                            function (key, se) {
                                options.push({ label: se.OutletName, title: se.OutletName, value: se.OutletId });
                            });

                        // Ref - http://davidstutz.de/bootstrap-multiselect/#methods
                        selectOut.multiselect('dataprovider', options);
                        selectOut.val(branchCodes.split(","));
                    } else {
                        common.ShowMessage(d);
                    }

                });
        },

        bindCustomerTypeByDistributorId = function () {
            const data = { DistributorId: distributorId };

            doAjaxPost(homeController + 'GetCustomerTypesByDistributorId',
                data,
                function (d) {
                    if (d.ResultFlag === 1) {
                        const selectCust = $('#ddlCustomerType');
                        selectCust.empty();

                        var options = [];
                        $.each(d.Data,
                            function (key, se) {
                                options.push({ label: se, title: se, value: se });
                            });
                        // Ref - http://davidstutz.de/bootstrap-multiselect/#methods
                        selectCust.multiselect('dataprovider', options);
                        $('#ddlCustomerType').val(customerTypes.split(","));

                    } else {
                        common.ShowMessage(d);
                    }

                });
        },
        bindWorkshopFromExcel = function (path) {
            common.showLoader();
            var data = {
                SchemeId: schemeId,
                FilePath: path,
                DistributorId: $("#distributorId").val()
            };
            doAjax(homeController + "UploadWorkshop",
                data,
                function (d) {
                    common.hideLoader();
                    $('#targetWorkshopResults').html(d);
                    schemes.bindICheck();
                    schemes.bindCheckAll();
                });
        },
        saveCouponForGift = function (couponNumber, controller, schemeName, giftName, callback) {
            var giftId = $('#hdnGiftId').val();

            var data = {
                GiftId: giftId,
                CouponNumber: couponNumber,
                SchemeName: schemeName,
                GiftName: giftName
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

            doAjaxPost(url + "SaveCouponForGift",
                data,
                function (d) {
                    callback(d);
                });
        },
        showWinners = function (giftId, controller) {
            common.showLoader();
            var data = {
                GiftId: giftId
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

            doAjaxPost(url + "GetWinners",
                data,
                function (response) {
                    common.hideLoader();
                    $(".modal-body").html(response);

                    // Set modal title with gift name
                    var hdnGiftName = $(response).filter('#hdnGiftName');
                    $("#winnersModal .modal-title").html(`${hdnGiftName.val()}'s Winners`);

                    $('#winnersModal').modal({
                        keyboard: true
                    });
                });
        },
        ShowWorkshopLevels = function (controller, schemeId, workshopId) {
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

                case "distributor":
                    url = distributorController;
                    break;
            }

            doAjaxPost(url + "ShowWorkshopLevels",
                data,
                function (d) {
                    common.hideLoader();

                    $('#levelModal .modal-body').html(d);

                    $('#levelModal').modal({
                        keyboard: true
                    });
                });
        },

        TransferToWallet = function (workshopId, schemeId, isAll = false) {
            if (confirm('Are you sure to transfer amount to wallet?')) {

                if ($('table tr td').hasClass('cashback')) {
                    common.showLoader();
                    var data = {
                        SchemeId: schemeId,
                        WorkshopId: workshopId,
                        isAll: isAll
                    };

                    doAjaxPost(adminController + "SchemeCashbackTransferToWallet",
                        data,
                        function (d) {
                            common.hideLoader();
                            common.ShowMessage(d);
                            if (d.ResultFlag == "1" || d.ResultFlag == 1) {
                                admin.LoadPartialView($("#PageView").val());
                            }
                        });
                }
                else { common.LoadErrorMessage('Any amount is not transferable.'); }
            }

        },
        showDecideWinners = function (schemeId, giftId) {
            common.showLoader();
            var data = {
                SchemeId: schemeId,
                GiftId: giftId
            };

            doAjaxPost(adminController + "DecideWinners",
                data,
                function (response) {
                    common.hideLoader();
                    $(".modal-body").html(response);

                    // Set modal title with gift name
                    var hdnGiftName = $(response).filter('#hdnGiftName');
                    $("#decideWinnersModal .modal-title").html(`Decide ${hdnGiftName.val()}'s winners`);

                    schemes.bindICheck();

                    $('#decideWinnersModal').modal({
                        keyboard: true
                    });
                });
        },
        saveDecideWinners = function () {
            common.showLoader();

            var json = [];
            var schemeId = $("#hdnSchemeId").val();
            var giftId = $("#hdnGiftId").val();
            var giftRemQty = $("#hdnGiftRemQty").val();

            $('#decideWinnersModal .modal-body .icheck:checked').each(function () {
                var couponNumber = $(this).val();
                var workshopId = $(this).attr("data-workshop-id");
                var data = {
                    SchemeId: schemeId,
                    WorkshopId: workshopId,
                    GiftId: giftId,
                    CouponNumber: couponNumber
                };
                json.push(data);
            });

            if (json.length > giftRemQty) {
                common.hideLoader();
                alert("Please select coupon less than or equal to gift remaining quantity.");
                return false;
            }

            doAjaxPostJson(adminController + "SaveDecideWinners",
                json,
                function (d) {
                    common.hideLoader();

                    if (d.ResultFlag === 1) {
                        $('#decideWinnersModal .modal-body .statusMsg')
                            .html(`<div class='alert alert-success' role='alert'>${d.Message}</div>`);
                    } else {
                        $('#decideWinnersModal .modal-body .statusMsg')
                            .html(`<div class='alert alert-danger' role='alert'>${d.Message}</div>`);
                    }
                });
        },
        bindGiftFileUpload = function () {

            $('.giftImg-fineUploader').each(function () {
                var th = $(this);
                var thumbImgUploader = new qq.FineUploader({
                    element: $(this)[0],
                    action: '/Home/upload',
                    type: "post",
                    minSizeLimit: 0,
                    autoUpload: true,
                    text: {
                        uploadButton: '<i class="icon-plus icon-white"></i> Select Files'
                    },
                    validation: {
                        acceptFiles: "image/*",
                        sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
                    },
                    callbacks: {
                        onComplete: function (id, filename, responseJSON) {
                            if (responseJSON.success == true) {
                                th.parents(".col-sm-2").find(".hdnImgPath").val(responseJSON.filename);
                                th.parents(".col-sm-2").find("img").attr("src", responseJSON.filename);
                            } else {
                                alert("Error: " + responseJSON.message);
                            }
                        }
                    }
                });
            });
        },
        handleSchemeTypeChange = function (dbPaybackType) {
            if (schemeType === 'Cashback') {
                $('#cashbackSchemeType').show();
                $('#luckyDrawSchemeType').hide();

                // Set payback types
                const paybackTypes = [
                    { key: '--Select--', value: '' },
                    { key: 'Cashback', value: 'Percentage' }
                ];
                $('#DispersalTypeId').empty();
                $.each(paybackTypes,
                    function (index, paybackType) {
                        $('#DispersalTypeId')
                            .append(`<option value="${paybackType.value}">${paybackType.key}</option>`);
                    });

                // Set other types values null
                isCashback = null;
                isAssuredGift = null;
                isLuckyDraw = null;

                // Change onclick of 'Reward Info' step's link
                $('.step-other').attr('onclick', `schemes.goToStep('${cashback}')`);
            } else if (schemeType === 'Lucky Draw') {
                $('#luckyDrawSchemeType').show();
                $('#cashbackSchemeType').hide();
                schemes.bindICheck();

                const paybackTypes = [
                    { key: '--Select--', value: '' },
                    { key: 'Coupon', value: 'Coupon' },
                    { key: 'Amount', value: 'Amount' },
                    { key: 'Point', value: 'Point' },
                    { key: 'Cashback', value: 'Percentage' },
                    { key: 'Product', value: 'Product' },
                    { key: 'Gift', value: 'Gift' }
                ];
                $('#DispersalTypeId').empty();
                $.each(paybackTypes,
                    function (index, paybackType) {
                        $('#DispersalTypeId')
                            .append(`<option value="${paybackType.value}">${paybackType.key}</option>`);
                    });

                // Set other types values null
                subSchemeType = null;

                // Change onclick of 'Reward Info' step's link
                $('.step-other').attr('onclick', `schemes.goToStep('${tabs}')`);
            }

            // Set payback type
            if (dbPaybackType) {
                $('#DispersalTypeId').val(dbPaybackType);
            }
        },

        // Scheme preview functions
        goToSchemeEditTab = (stepToShow) => {
            schemeId = $('#hdnSchemeId').val();

            try {
                // Ref - https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage
                localStorage.setItem('stepToShow', stepToShow);
            }
            catch (err) {
                console.log(err);
            }

            window.location.href = `/home/AddSchemes/${schemeId}?mode=edit`;
        },
        showSchemeTab = () => {
            try {
                const stepToShow = localStorage.getItem('stepToShow');
                if (stepToShow) {
                    schemes.goToStep(stepToShow);
                    localStorage.removeItem('stepToShow');
                }
            }
            catch (err) {
                console.log(err);
            }
        },
        SchemeStartDateEndDateValidate = (textboxId) => {

            let startDate = $('#StartDate').val();
            let endDate = $('#EndDate').val();
            if (startDate && endDate) {
                if (startDate >= endDate) {
                    common.LoadErrorMessage('EndDate is not less than or equal to startDate. ');
                    $('#' + textboxId).val('');
                }
            }
        },
        UpdateAchievedTargetForWorkshop = function (schemeId) {
            common.showLoader();
            var data = {
                SchemeId: schemeId
            };
            doAjaxPost(homeController + "UpdateAchievedTargetForWorkshop", data, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
            });
        };

    return {

        init: init,
        saveBasicInfo: saveBasicInfo,
        saveScheme: saveScheme,
        getCategoryData: getCategoryData,
        saveCategories: saveCategories,
        getLuckyDrawData: getLuckyDrawData,
        saveLuckyDrawData: saveLuckyDrawData,
        getAssuredGiftData: getAssuredGiftData,
        saveAssuredGiftData: saveAssuredGiftData,
        getCashBackData: getCashBackData,
        saveCashBackData: saveCashBackData,
        getCouponAllocationData: getCouponAllocationData,
        saveCouponAllocationData: saveCouponAllocationData,
        getTargetWorkShopData: getTargetWorkShopData,
        saveTargetWorkShopData: saveTargetWorkShopData,
        getFocusPartData: getFocusPartData,
        saveFocusPartData: saveFocusPartData,
        bindMultiSelect: bindMultiSelect,
        bindICheck: bindICheck,
        deleteScheme: deleteScheme,
        saveActiveScheme: saveActiveScheme,
        getActiveScheme: getActiveScheme,
        showWorkShopLabels: showWorkShopLabels,
        getWorkShopLabelData: getWorkShopLabelData,
        getWorkShopCriteriaData: getWorkShopCriteriaData,
        bindTargetWorkShopFromExcel: bindTargetWorkShopFromExcel,
        showWorkShopLabelResult: showWorkShopLabelResult,
        bindOtherFileUploads: bindOtherFileUploads,
        // bindProdGroups: bindProdGroups,
        // bindProducts: bindProducts,
        removeCurrentWorkshop: removeCurrentWorkshop,
        AddTargetGrowth: AddTargetGrowth,
        AddRowTargetGrowth: AddRowTargetGrowth,
        saveTargetGrowth: saveTargetGrowth,
        bindRoIncharge: bindRoIncharge,
        bindSalesExecutive: bindSalesExecutive,
        bindFocusPartFromExcel: bindFocusPartFromExcel,
        saveCouponForGift: saveCouponForGift,
        showWinners: showWinners,
        ShowWorkshopLevels: ShowWorkshopLevels,
        TransferToWallet: TransferToWallet,
        showDecideWinners: showDecideWinners,
        saveDecideWinners: saveDecideWinners,
        updateCashbackCriteria: updateCashbackCriteria,
        saveCustomerSegment: saveCustomerSegment,
        showStepNew: showStepNew,
        goToStep: goToStep,
        savePartInfo: savePartInfo,
        loadFocusPartData: loadFocusPartData,
        bindCheckAll: bindCheckAll,
        calculateGrowth: calculateGrowth,
        sortByGrowth: sortByGrowth,
        getCashBackRangeData: getCashBackRangeData,
        saveCashBackRangeData: saveCashBackRangeData,
        saveRewardInfo: saveRewardInfo,
        bindGiftFileUpload: bindGiftFileUpload,
        bindSalesExecutiveByBranchCode: bindSalesExecutiveByBranchCode,
        handleSchemeTypeChange: handleSchemeTypeChange,
        readElementsValues: readElementsValues,
        removeCurrentCategory: removeCurrentCategory,
        getTargetGrowth: getTargetGrowth,
        removeCurrentTarget: removeCurrentTarget,
        toggleCategoryDetails: toggleCategoryDetails,
        generateTargets: generateTargets,
        deleteAllCategories: deleteAllCategories,
        saveTargetInfo: saveTargetInfo,
        askGrowthPercentModal: askGrowthPercentModal,
        removeCurrentCashBackRange: removeCurrentCashBackRange,
        removeCurrentCashBack: removeCurrentCashBack,
        removeCurrentAssuredGift: removeCurrentAssuredGift,
        removeCurrentLuckyDraw: removeCurrentLuckyDraw,
        removeCurrentCouponAllocation: removeCurrentCouponAllocation,
        showCashbackRangeModal: showCashbackRangeModal,
        removeCurrentFpgRow: removeCurrentFpgRow,
        goToSchemeEditTab: goToSchemeEditTab,
        setAutoCompleteForGroup: setAutoCompleteForGroup,
        updateFocusParts: updateFocusParts,
        getGroupParts: getGroupParts,
        SchemeStartDateEndDateValidate: SchemeStartDateEndDateValidate,
        UpdateAchievedTargetForWorkshop: UpdateAchievedTargetForWorkshop
    };
}();