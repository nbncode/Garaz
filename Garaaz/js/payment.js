var payment = function () {
    PlaceOrder = function () {
        var payMode = $("input[name='paymentMethod']:checked").val();
        if (payMode.length == 0) {
            common.LoadErrorMessage("Please select payment mode");
            return;
        }
        if (payMode === "CashOnDelivery") {
            SavePaymentMethod(payMode, null, null, null);
        }
        else {
            doAjaxPost(homeController + "CreateOrderId_With_RazorPay", null, function (d) {
                console.log("responseOrder", d);
                if (d.OrderId != null && d.OrderId != undefined && d.OrderId.length > 0) {
                    var options = {
                        "key": d.Key, // Enter the Key ID generated from the Dashboard
                        "amount": d.Amount, // Amount is in currency subunits. Default currency is INR. Hence, 50000 refers to 50000 paise or INR 500.
                        "currency": d.Currency,
                        "name": d.Name,
                        "description": d.Description,
                        "image": d.Image,
                        "order_id": d.OrderId,//This is a sample Order ID. Create an Order using Orders API. (https://razorpay.com/docs/payment-gateway/orders/integration/#step-1-create-an-order). Refer the Checkout form table given below
                        "handler": function (response) {
                            console.log("response", response);
                            if (response.razorpay_payment_id != null && response.razorpay_payment_id != undefined && response.razorpay_payment_id.length > 0) {

                                common.LoadSuccessMessage("Payment sucessfully.")
                                SavePaymentMethod(payMode, response.razorpay_order_id, response.razorpay_payment_id, response.razorpay_signature);
                            }
                            else {
                                common.LoadErrorMessage("Payment Failed.")
                                window.location.href ="/Home/PaymentMethod"
                            }
                        },
                        "prefill": {
                            "name": d.Prefill_Name,
                            "email": d.Prefill_Email,
                            "contact": d.Prefill_Contact
                        },
                        "notes": {
                            "address": d.Address
                        },
                        "theme": {
                            "color": "#F37254"
                        }
                    };
                    var rzp1 = new Razorpay(options);
                    rzp1.open();

                }
                else {
                    common.LoadErrorMessage("Payment Failed.")
                }

            });



        }
    },
        SavePaymentMethod = function (paymode, razorpay_orderId, razorpay_paymentId, razorpay_signature) {
            common.showLoader();
        doAjaxPost(homeController + "PaymentMethod?paymentMethod=" + paymode + "&razorOrderId=" + razorpay_orderId + "&razorPaymentId=" + razorpay_paymentId + "&razorSignature=" + razorpay_signature+"", null, function (d) {
                common.hideLoader();
                common.ShowMessage(d);
            if (d.ResultFlag === 1) {
                //var data = paymode + "&" + razorpay_orderId + "&" + razorpay_paymentId + "&" + razorpay_signature;
                var data = "" + paymode + "~" + razorpay_orderId + "~" + razorpay_paymentId + "~" + razorpay_signature +""
                window.location.href = "/Home/SaveOrder?data=" + data + "";
                }
            });
        };

    return {
        PlaceOrder: PlaceOrder,
        SavePaymentMethod: SavePaymentMethod
    };
}();