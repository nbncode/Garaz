var adminController = "/admin/";
var distributorController = "/distributor/";
var homeController = "/home/";
var apiController = "/api/system/";
var workshopController = "/workshop/";
var outletController = "/outlet/";
//var loading = new KTDialog({ 'type': 'loader', 'placement': 'top center', 'message': 'Loading ...' });

var common = {
    LoadSuccessMessage: function (Message)
    {
        //var str = '<div class="alert alert-success">' + Message + '</div>';
        //$("#main-content").prepend(str);
        //common.RemoveMessage();
        $('.alert').remove();
        $.notify(Message, {
            type: "success",
            z_index: 99999
        });
    },
    LoadErrorMessage: function (Message)
    {
        //var str = '<div class="alert alert-danger">' + Message + '</div>';
        //$("#main-content").prepend(str);
        //common.RemoveMessage();
        $('.alert').remove();
        $.notify(Message, {
            type: "danger",
            z_index: 99999
        });
    },
    RemoveMessage: function () {
        
        // Don't remove bootstrap alert for pages with import
        if (window.location.href.indexOf('Import') > -1 || window.location.href.indexOf('Import') > -1)
        {
            return;
        }

        setTimeout(function ()
        {
            $('.alert').fadeOut("slow", function ()
            {
                $('.alert').remove();
            });
        }, 5000);
    },
    showLoader: function ()
    {

        //loading.show();
        $(".kt-spinner").show();
    },
    hideLoader: function ()
    {
        $(".kt-spinner").hide();
        //loading.hide();
    },
    ShowMessage: function (data)
    {
        if (data.ResultFlag == "1" || data.ResultFlag == 1)
        {
            common.LoadSuccessMessage(data.Message);
        } else
        {
            common.LoadErrorMessage(data.Message);
        }
    },
    loadInit: function ()
    {
        $('.multiselect').multiselect();
        $('input.icheck_n').iCheck({
            checkboxClass: 'icheckbox_square-blue',
            radioClass: 'iradio_square-blue',
            increaseArea: '20%'
        });

        $("#cart-btn").click(function ()
        {
            common.bindCart();
        });
    },
    bindDatepicker: function ()
    {
        var arrows = {
            leftArrow: '<i class="la la-angle-left"></i>',
            rightArrow: '<i class="la la-angle-right"></i>'
        };

        $(".datepicker-txt").datepicker({
            autoclose: true,
            orientation: "bottom right",
            todayHighlight: true,
            templates: arrows
        });
    },
    bindFileUpload: function (callback)
    {
        var targetWsUploader = new qq.FineUploader({
            element: $('#manual-fine-uploader')[0],
            action: '/Home/upload',
            type: "post",
            minSizeLimit: 0,
            autoUpload: true,
            text: {
                uploadButton: '<i class="fa fa-file-excel-o"></i> Select Files'
            },
            validation: {
                acceptFiles: ".xls,.xlsx,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                allowedExtensions: ['xls', 'xlsx'],
                sizeLimit: 2000000 // 50 kB = 50 * 1024 bytes
            },
            callbacks: {
                onComplete: function (id, filename, responseJSON)
                {
                    console.log(responseJSON);
                    if (responseJSON.success == true)
                    {
                        var pathId = $("#pathId").val();
                        $("#" + pathId).val(responseJSON.filename);

                    } else
                    {
                        alert("Error: " + responseJSON.message);
                    }
                    callback(responseJSON.filename);
                }
            }
        });
    },
    bindCart: function ()
    {
        common.showLoader();
        doAjaxPost(homeController + "Cart", null, function (d)
        {
            common.hideLoader();
            $("#cart-items").html(d);
        });
    },
    ChangePassword: function ()
    {
        var userId = $(".userId").val();
        var nPassword = $(".newPassword").val();
        var cPassword = $(".confirmPassword").val();
        console.log('pw', userId, nPassword, cPassword);
        if (nPassword.length == 0)
        {
            common.LoadErrorMessage('Please enter new password.')
            return;
        }
        if (cPassword.length == 0)
        {
            common.LoadErrorMessage('Please enter confirm password.')
            return;
        }
        if (userId.length > 0 && nPassword.length > 0 && cPassword.length > 0)
        {
            if (nPassword.toString() != cPassword.toString())
            {
                common.LoadErrorMessage("Password and confirm password didn't match.")
                return;
            }
            var data = { UserId: userId, NewPassword: nPassword, ConfirmPassword: cPassword };
            common.showLoader();
            doAjaxPost(homeController + "ChangePassword", data, function (d)
            {
                common.hideLoader();
                if (d.ResultFlag == 1)
                {
                    $('#myModalPassword').modal('hide');
                    $("#myModalPassword input").val('');
                }
                common.ShowMessage(d);
            });
        }
        //else {
        //    common.LoadErrorMessage('Please Enter New Password and Confirm Password')
        //    return;
        //}

    },
    ChangePasswordPopUp: function (userId)
    {
        $(".userId").val(userId);
        $("#myModalPassword").modal('toggle');
    },
    ChangeMobilePopUp: function ()
    {
        $("#myModalMobile").modal('toggle');
    },
    ChangeMobile: function ()
    {
        var nMobile = $(".newMobile").val();
        var cMobile = $(".confirmMobile").val();
        console.log('mo', nMobile, cMobile);
        if (nMobile.length == 0)
        {
            common.LoadErrorMessage('Please enter new mobile number.')
            return;
        }
        if (nMobile.length != 10)
        {
            common.LoadErrorMessage('Invalid mobile number.')
            return;
        }
        if (cMobile.length == 0)
        {
            common.LoadErrorMessage('Please enter confirm mobile number.')
            return;
        }

        if (nMobile.length > 0 && cMobile.length > 0)
        {
            if (nMobile.toString() != cMobile.toString())
            {
                common.LoadErrorMessage("Mobile number and confirm mobile number didn't match.")
                return;
            }
            var data = { NewMobileNumber: nMobile, ConfirmMobileNumber: cMobile };
            common.showLoader();
            doAjaxPost(homeController + "ChangeMobileNumber", data, function (d)
            {
                common.hideLoader();
                if (d.ResultFlag == 1)
                {
                    $('#myModalMobile').modal('hide');
                    $("#myModalMobile input").val('');
                }
                d.Message = d.Data;
                common.ShowMessage(d);
            });
        }

    }
};

function doAjax(url, data, successCallback)
{
    $.ajax({
        type: "Get",
        data: data,
        url: url,
        success: function (data)
        {
            successCallback(data);
        },
        error: function (xhr, textStatus, error)
        {
            // Log error message
            var errorMessage = xhr.status + ': ' + xhr.statusText;
            console.log('%c ' + errorMessage, 'color: #FF0000; font-weight: bold;');
            console.log(xhr.responseText);

            alert("Some unexpected error occured.");
        }

    });
}

function doAjaxPost(url, data, successCallback)
{
    
    console.log(data);
    var auth = $("#access_token").val();
    $.ajax({
        type: "POST",
        data: data,
        beforeSend: function (xhr)
        {
            xhr.setRequestHeader('Authorization', 'bearer ' + auth);
        },
        url: url,
        success: function (data)
        {
           
            successCallback(data);
        },
        error: function (data)
        {
            
            if (data.status == 401)
            {
                setAccessToken(url, data, successCallback);
            }
        }

    });
}

function doAjaxPostJson(url, data, successCallback)
{
    var auth = $("#access_token").val();
    var dataJson = JSON.stringify(data);
    $.ajax({
        type: "POST",
        data: dataJson,
        contentType: 'application/json; charset=utf-8',
        beforeSend: function (xhr)
        {
            xhr.setRequestHeader('Authorization', 'bearer ' + auth);
        },
        url: url,
        success: function (data)
        {
            successCallback(data);
        },
        error: function (data)
        {
            if (data.status == 401)
            {
                setAccessToken(url, data, successCallback);
            }
        }

    });
}

function setAccessToken(url, data, successCallback)
{
    $.ajax({
        type: "POST",
        url: homeController + "GetTokenByRefreshToken",
        success: function (data)
        {
            doAjaxPost(url, data, successCallback);
        },
        error: function (data)
        {
            window.location.href = $("#Signout").attr("href");
        }
    });
}

function addDatepicker()
{
    $(".datepicker").datepicker();
}

function changeDateFormat()
{

    $(".datepicker-input").each(function ()
    {
        var txt = $(this).val();

        try
        {
            if (txt != "")
            {
                var d = new Date(txt);
                var newDate = (d.getMonth() + 1) + '/' + d.getDate() + '/' + d.getFullYear();
                $(this).val(newDate);
            }
        }
        catch (err)
        {
            $(this).val(txt);
        }
    });
}

function changeurl(url)
{
    $(url).attr("src", "/assets/images/NoPhotoAvailable.png");
}
$(function ()
{
    handleLoading();
});

function handleLoading() {
    
    // Don't set handler for page with dashboard
    if (window.location.href.indexOf('dashboard') > -1 || window.location.href.indexOf('Dashboard') > -1)
    {
        return;
    }

    $(document).ajaxStart(function ()
    {
        //loading.show();
        $(".kt-spinner").show();
    });
    $(document).ajaxStop(function ()
    {
        //loading.hide();
        $(".kt-spinner").hide();
    });
}

function doPost(url, data, method)
{
    if (method == null) method = 'POST';
    if (data == null) data = {};

    var form = $('<form>').attr({
        method: method,
        action: url
    }).css({
        display: 'none'
    });

    var addData = function (name, data)
    {
        if ($.isArray(data))
        {
            for (var i = 0; i < data.length; i++)
            {
                var value = data[i];
                addData(name + '[]', value);
            }
        } else if (typeof data === 'object')
        {
            for (var key in data)
            {
                if (data.hasOwnProperty(key))
                {
                    addData(name + '[' + key + ']', data[key]);
                }
            }
        } else if (data != null)
        {
            form.append($('<input>').attr({
                type: 'hidden',
                name: String(name),
                value: String(data)
            }));
        }
    };

    for (var key in data)
    {
        if (data.hasOwnProperty(key))
        {
            addData(key, data[key]);
        }
    }

    return form.appendTo('body');
}