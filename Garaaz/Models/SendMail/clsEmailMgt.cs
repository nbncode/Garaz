using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Garaaz.Models
{
    public class clsEmailMgt
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General general = new General();
        RepoMailSender sendMail = new RepoMailSender();

        // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
        #endregion
        #region Order Mangement
        public string SendMailOrderPlaced(string Heading, string msg, string Username, string Email, string Phone,
             OrderTable model, string Sign, bool SendToUser)
        {
            Heading = Heading.Replace("@Username", Username);
            Heading = Heading.Replace("@Phone", Phone);
            Heading = Heading.Replace("@OrderNumber", model.OrderNo);

            msg = msg.Replace("@Username", Username);
            msg = msg.Replace("@Phone", Phone);
            msg = msg.Replace("@Email", Email);
            msg = msg.Replace("@summary", GetSummary(model, Sign));
            msg = msg.Replace("@OrderNumber", model.OrderNo);

            string html = sendMail.GetHtml(Username, Heading, msg);
            if (SendToUser)
            {
                var str = sendMail.MailSender(Email, html, Heading);
                return str;
            }
            else
            {
                // var str = sendMail.MailSender(RestaurantEmail, html, Heading);
                return string.Empty;
            };

        }

        public string GetSummary(OrderTable model, string Sign)
        {
            var address = db.DeliveryAddresses.Where(x => x.DeliveryAddressId == model.DeliveryAddressId).FirstOrDefault();
            string Address = string.Empty;
            if (address != null)
            {
                Address = (!string.IsNullOrEmpty(address.Address) ? address.Address : "")
                    + (!string.IsNullOrEmpty(address.City) ? ", " + address.City : "")
                    + (!string.IsNullOrEmpty(address.state) ? ", " + address.state : "")
                    + (!string.IsNullOrEmpty(address.PinCode) ? ", " + address.PinCode : "");
            }
            //Mail Table 1
            string str = "<table style='width:100%;'><tbody><tr><td style='font-size: 16px;line-height: 17px;text-align: left;text-align:left;width:40%'>Order ID: " + model.OrderNo + "</td>";
            str += "<td style='text-align:right;font-size: 12px;line-height: 20px;'>Date: " + model.OrderDate.Value.ToString("dd MMM, yyyy") + "<br />" + Address + "</td></tr></tbody></table>";

            //Mail Table 2
            str += "<table style='border-spacing: 0;border-collapse: collapse;width:100%;'><tr><td>";

            //inner table 1
            str += "<table style='width:100%;'>";
            foreach (var item in model.OrderDetails)
            {
                if (item.ProductID > 0)
                {
                    var product = db.Products.Where(x => x.ProductId == item.ProductID).FirstOrDefault();
                    if (product != null)
                    {
                        str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>" + product.ProductName;
                        product.Description = !string.IsNullOrEmpty(product.Description) ? " - " + product.Description : "";
                        str += " ("+ product.Description + ")";
                    }
                }
                str += "<span style='font-size:12px;'> (" + item.Qty + " x " + Sign + item.UnitPrice + ")</span>";
                str += "</td><td width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + item.TotalPrice + "</td></tr>";
            }
            str += "</table>";

            // Inner Table 2
            str += "<table style='width:100%;border-spacing: 0;border-collapse: collapse;'>";
            str += "<tr style='border-top: 1px solid black;'><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Sub Total</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.SubTotal + "</td></tr>";

            if (model.Discount > 0)
            {
                str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Discount</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'> -" + Sign + model.Discount + "</td></tr>";
            }

            if (model.PackingCharge > 0)
            {
                str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Packaging Charge</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.PackingCharge + "</td></tr>";
            }
            if (model.DeliveryCharge > 0)
            {
                str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Delivery Charge</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.DeliveryCharge + "</td></tr>";
            }
            //if (model.Taxes > 0)
            //{
            //    str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Taxes</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.Taxes + "</td></tr>";
            //}
            str += "<table border='0' width='100%' cellpadding='0' cellspacing='0' align='center' style='margin-top:5px;border-spacing:0;border-collapse:collapse'></table>";
            str += "</table>";
            str += "</td></tr>";

            str += "<tr><td style='border-collapse:collapse;font-family:sans-serif;font-size:12px;line-height:17px;color:#7d7d76;padding-top:10px;text-align:left'></td></tr>";

            str += "<tr style='border-top: 1px solid black;'><td style='border-collapse:collapse'><table border='0' width='100%' cellpadding='0' cellspacing='0' align='center' style='border-spacing:0;border-collapse:collapse'><tbody><tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:21px;color:#2d2d2a;padding-bottom:15px;text-align:left'></td><td width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:21px;color:#2d2d2a;padding-top: 15px;text-align:right'>" + Sign + model.OrderTotal + "</td></tr></tbody></table></td></tr>";

            str += "</table>";
            return str;
        }
        #endregion

        #region 
        public string SendMailBackorder(string Heading, string msg, string Username, string Email,
         List<CustomerBackOrder> lstorder, string Sign, bool SendToUser)
        {
            CustomerBackOrder model = lstorder.FirstOrDefault();
            Heading = Heading.Replace("@Partyname", model.PartyName);
            Heading = Heading.Replace("@Code", model.PartyCode);
            Heading = Heading.Replace("@Orderqty", model.OrderedQty);
            Heading = Heading.Replace("@OrderNumber", model.CONo);

            msg = msg.Replace("@Partyname", model.PartyName);
            msg = msg.Replace("@Code", model.PartyCode);
            msg = msg.Replace("@Orderqty", model.OrderedQty);
            msg = msg.Replace("@summary", GetBackSummary(lstorder, Sign));
            msg = msg.Replace("@OrderNumber", model.CONo);

            string html = sendMail.GetHtml(Username, Heading, msg);
            if (SendToUser)
            {
                var str = sendMail.MailSender(Email, html, Heading);
                return str;
            }
            else
            {
                // var str = sendMail.MailSender(RestaurantEmail, html, Heading);
                return string.Empty;
            };

        }

        public string SendMailrajectBackorder(string Heading, string msg, string Username, string Email,
          List<CustomerBackOrder> lstorder, string Sign, bool SendToUser)
        {
             CustomerBackOrder  model = lstorder.FirstOrDefault();
            Heading = Heading.Replace("@Partyname", model.PartyName);
            Heading = Heading.Replace("@Code", model.PartyCode);
            Heading = Heading.Replace("@Orderqty", model.OrderedQty);
            Heading = Heading.Replace("@OrderNumber", model.CONo);

            msg = msg.Replace("@Partyname", model.PartyName);
            msg = msg.Replace("@Code", model.PartyCode);
            msg = msg.Replace("@Orderqty", model.OrderedQty);
            msg = msg.Replace("@summary", GetBackSummary(lstorder, Sign));
            msg = msg.Replace("@OrderNumber", model.CONo);

            string html = sendMail.GetHtml(Username, Heading, msg);
            if (SendToUser)
            {
                var str = sendMail.MailSender(Email, html, Heading);
                return str;
            }
            else
            {
                // var str = sendMail.MailSender(RestaurantEmail, html, Heading);
                return string.Empty;
            };

        }
        public string GetBackSummary(List<CustomerBackOrder> lstorder, string Sign)
        {
            CustomerBackOrder model = lstorder.FirstOrDefault();
            decimal TotalPrice = 0;
            //Mail Table 1
            string str = "<table style='width:100%;'><tbody><tr><td style='font-size: 16px;line-height: 17px;text-align: left;text-align:left;width:40%'>Order ID: " + model.CONo + "</td>";
            str += "<td style='text-align:right;font-size: 12px;line-height: 20px;'>Date: " + model.CODate.Value.ToString("dd MMM, yyyy") + "<br />"  + "</td></tr></tbody></table>";

            //Mail Table 2
            str += "<table style='border-spacing: 0;border-collapse: collapse;width:100%;'><tr><td>";

            //inner table 1
            str += "<table style='width:100%;'>";
            foreach (var item in lstorder)
            {
                decimal unitprice = 0;
                if (!string.IsNullOrEmpty(item.PartNum))
                {
                    var product = db.Products.Where(x => x.PartNo == item.PartNum).FirstOrDefault();
                    if (product != null)
                    {
                        unitprice = Convert.ToDecimal(product.Price!=null? product.Price:0);
                        str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>" + product.ProductName;
                        product.Description = !string.IsNullOrEmpty(product.Description) ? " - " + product.Description : "";
                        str += " (" + product.Description + ")";
                    }
                }
                decimal SubTotal = Convert.ToInt32(item.PendingOrCancelledQty) * unitprice;
                str += "<span style='font-size:12px;'> (" + item.PendingOrCancelledQty + " x " + Sign + unitprice + ")</span>";
                str += "</td><td width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + SubTotal + "</td></tr>";
                // add Totalprice
                TotalPrice = TotalPrice + SubTotal;
            }
            str += "</table>";

            
            // Inner Table 2
            str += "<table style='width:100%;border-spacing: 0;border-collapse: collapse;'>";
            str += "<tr style='border-top: 1px solid black;'><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Sub Total</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + TotalPrice + "</td></tr>";

            //if (model.Discount > 0)
            //{
            //    str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Discount</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'> -" + Sign + model.Discount + "</td></tr>";
            //}

            //if (model.PackingCharge > 0)
            //{
            //    str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Packaging Charge</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.PackingCharge + "</td></tr>";
            //}
            //if (model.DeliveryCharge > 0)
            //{
            //    str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Delivery Charge</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.DeliveryCharge + "</td></tr>";
            //}
            //if (model.Taxes > 0)
            //{
            //    str += "<tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:left'>Taxes</td><td valign='middle' width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:30px;color:#2d2d2a;text-align:right'>" + Sign + model.Taxes + "</td></tr>";
            //}
            str += "<table border='0' width='100%' cellpadding='0' cellspacing='0' align='center' style='margin-top:5px;border-spacing:0;border-collapse:collapse'></table>";
            str += "</table>";
            str += "</td></tr>";

            str += "<tr><td style='border-collapse:collapse;font-family:sans-serif;font-size:12px;line-height:17px;color:#7d7d76;padding-top:10px;text-align:left'></td></tr>";

            str += "<tr style='border-top: 1px solid black;'><td style='border-collapse:collapse'><table border='0' width='100%' cellpadding='0' cellspacing='0' align='center' style='border-spacing:0;border-collapse:collapse'><tbody><tr><td width='70%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:21px;color:#2d2d2a;padding-bottom:15px;text-align:left'>"+ TotalPrice + "</td><td width='30%' style='border-collapse:collapse;font-family:sans-serif;font-size:14px;line-height:21px;color:#2d2d2a;padding-top: 15px;text-align:right'>" + Sign + TotalPrice + "</td></tr></tbody></table></td></tr>";

            str += "</table>";
            return str;
        }
        #endregion
        //#region Account Mgt
        //public bool SendRegistrationEmail(string Heading, string msg, string Token, string Email, string Name, string UserId)
        //{

        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    string link = sitepath + "confirm-user?activation_id=" + UserId + "&key=" + Token;
        //    link = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + link + ">Click Here</a>";

        //    msg = msg.Replace("@Username", Name);
        //    msg = msg.Replace("@Email", Email);
        //    msg = msg.Replace("@Link", link);
        //    // msg = msg.Insert(0, adminEmail);

        //    string html = Repohtml.GetHtml(Name, Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);
        //    string adminEmail = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    clsGeneralSettings cls = new clsGeneralSettings();
        //    List<GeneralSettingsModel> list = cls.getAllGeneralSettings();
        //    if (list[0].IsHotelAdd)
        //    {
        //        var adminMail = sendMail.MailSender(adminEmail, html, Heading);
        //    }
        //    return true;
        //}

        ////public bool ReSendRegistrationEmail(string Token, string Email, string Name, string UserId)
        ////{
        ////    string Heading = "Confirmation Link";
        ////    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();

        ////    string msg = "Click on link below to confirm your account.<br/>";
        ////    string link = sitepath + "member#/apps/confirmation?activation_id=" + UserId + "&key=" + Token;

        ////    msg = msg + "<a href=" + link + ">" + link + "</a>";

        ////    string html = Repohtml.GetHtml(Name, Heading, msg);

        ////    var str = sendMail.MailSender(Email, html, Heading);

        ////    return true;
        ////}

        //public string SendForgotPasswordEmail(string Heading, string msg, string Token, string Email, string Name, int Min, string UserId)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    string link = sitepath + "reset-password?activation_id=" + UserId + "&key=" + Token;
        //    link = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + link + ">Click Here</a>";

        //    msg = msg.Replace("@Username", Name);
        //    msg = msg.Replace("@Email", Email);
        //    msg = msg.Replace("@Link", link);
        //    msg = msg.Replace("@ExpireMin", Min.ToString());

        //    string html = Repohtml.GetHtml(Name, Heading, msg);

        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}




        //#endregion

        //#region claim listing
        //public string SendClaimRestaurantEmailToAdmin(string Heading, string ResName, string Name, string UsrEmail)
        //{
        //    clsGeneralSettings cls = new clsGeneralSettings();
        //    List<GeneralSettingsModel> list = cls.getAllGeneralSettings();
        //    if (list[0].IsClaimListing)
        //    {
        //        string Email = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //        string msg = "<table>";
        //        msg += "<tr><td>Restaurant Name</td><td>" + ResName + "</td></tr>";
        //        msg += "<tr><td>UserEmail</td><td>" + UsrEmail + "</td></tr>";
        //        msg += "<tr><td>Name</td><td>" + Name + "</td></tr></table>";

        //        string html = Repohtml.GetHtml("Admin", Heading, msg);
        //        var str = sendMail.MailSender(Email, html, Heading);
        //        return str;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public string SendContactToClaimRequestToAdmin(string Heading, string ResName, string Name, string UsrEmail, string usrPhone)
        //{
        //    string Email = ConfigurationManager.AppSettings["contactFormEmail"].ToString();

        //    string msg = "<table>";
        //    msg += "<tr><td>Restaurant Name</td><td>" + ResName + "</td></tr>";
        //    msg += "<tr><td>UserEmail</td><td>" + UsrEmail + "</td></tr>";
        //    msg += "<tr><td>Name</td><td>" + Name + "</td></tr>";
        //    msg += "<tr><td>Phone</td><td>" + usrPhone + "</td></tr></table>";

        //    string html = Repohtml.GetHtml("Admin", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        ///******************************Claim Listing Verification Code*************************/
        //public string SendClaimRestaurantEmailToUser(string Heading, string msg, string UserId, string Code, string Name, string Email)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    string link = sitepath + "confirm-restaurant?activation_id=" + UserId + "&key=" + Code;
        //    link = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + link + ">Click Here</a>";
        //    msg = msg.Replace("@FullName", Name);
        //    msg = msg.Replace("@Email", Email);
        //    msg = msg.Replace("@Link", link);

        //    string html = Repohtml.GetHtml(Name, Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        ///******************************Claim Listing Success Confirmation*************************/
        //public string SendClaimListingSuccessConfirmation(string Heading, string msg, string Name, string Email, string RestaurantName, string RestaurantAddress)
        //{

        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@RestaurantName", RestaurantName);
        //    msg = msg.Replace("@RestaurantAddress", RestaurantAddress);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);
        //    Heading = Heading.Replace("@RestaurantAddress", RestaurantAddress);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        ///******************************Claim Listing Link To Restaurant Admin*************************/
        //public string SendClaimListingLinkToRestaurantAdmin(string Heading, string msg, string Name, string Email, string RestaurantName)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();

        //    string Link = sitepath + ConfigurationManager.AppSettings["restaurentDashboardLink"].ToString();

        //    Link = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + Link + ">Click Here</a>";
        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@RestaurantName", RestaurantName);
        //    msg = msg.Replace("@Link", Link);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        ///******************************Claim Listing Failure*************************/
        //public string SendClaimListingFailure(string Heading, string msg, string Name, string Email, string RestaurantName)
        //{

        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@RestaurantName", RestaurantName);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}


        //#endregion

        //#region AddRestaurant
        //public string SendAddRestaurantEmail(string Heading, string ResName, string Name, string Phone)
        //{
        //    string Email = ConfigurationManager.AppSettings["contactFormEmail"].ToString();

        //    string msg = "<table>";
        //    msg += "<tr><td>Restaurant Name</td><td>" + ResName + "</td></tr>";
        //    msg += "<tr><td>Phone</td><td>" + Phone + "</td></tr>";
        //    msg += "<tr><td>Name</td><td>" + Name + "</td></tr></table>";

        //    string html = Repohtml.GetHtml("Admin", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        ///******************************Restaurant Request Failure*************************/
        //public string SendRestaurantRequestFailure(string Heading, string msg, string Name, string Email)
        //{

        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        ///******************************Restaurant Request Success*************************/
        //public string SendRestaurantRequestSuccess(string Heading, string msg, string Name, string Email, string RestaurantName, string RestaurantLink)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();

        //    string DashboardLink = sitepath + ConfigurationManager.AppSettings["restaurentDashboardLink"].ToString();
        //    RestaurantLink = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + RestaurantLink + ">Click Here</a>";

        //    DashboardLink = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + DashboardLink + ">Click Here</a>";

        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@RestaurantName", RestaurantName);
        //    msg = msg.Replace("@Link", DashboardLink);
        //    msg = msg.Replace("@RestaurantLink", RestaurantLink);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}
        ///******************************Restaurant Add Mail To User *************************/
        //public string SendRestaurantAddMailToUser(string Heading, string msg, string Name, string Email, string RestaurantName, string Link)
        //{
        //    //string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();

        //    //string Link = sitepath + ConfigurationManager.AppSettings["restaurentDashboardLink"].ToString();

        //    Link = "<a style='background: #5292ff; text-transform: uppercase; color: #fff; text-decoration: none; padding: 12px 40px; letter-spacing: 1px; border-radius: 30px; display: inline-block; font-weight: 500;' href=" + Link + ">Click Here</a>";
        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@RestaurantName", RestaurantName);
        //    msg = msg.Replace("@Link", Link);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}
        //#endregion

        //#region RestaurantReport
        //public string SendRestaurantReportEmail(string Heading, string email, string text, string attributes, string restaurantName, string restURL)
        //{
        //    string Email = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    string rsturl = restURL.Substring(1);
        //    string msg = "<table>";
        //    msg += "<tr><td>Restaurant Name</td><td><a href=" + sitepath + rsturl + ">" + restaurantName + "</a></td></tr>";
        //    if (!string.IsNullOrEmpty(email))
        //        msg += "<tr><td>User Email</td><td>" + email + "</td></tr>";
        //    if (!string.IsNullOrEmpty(text))
        //        msg += "<tr><td>Text</td><td>" + text + "</td></tr>";
        //    msg += "<tr><td>Attributes </td ><td>" + attributes + " </td></tr></table>";

        //    string html = Repohtml.GetHtml("Admin", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}
        //#endregion

        //#region Contact form
        //public string sendContactMail(string name, string email, string subject, string message, string Phone)
        //{
        //    string contactFormEmail = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    string sitename = ConfigurationManager.AppSettings["sitename"].ToString();
        //    string Heading = "Contact form from " + sitename;
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    string msg = "<table><tr><td>Name</td><td>" + name + "</td></tr>";
        //    msg += "<tr><td>Email</td><td>" + email + "</td></tr>";
        //    msg += "<tr><td>Subject</td><td>" + subject + "</td></tr>";
        //    msg += "<tr><td>Phone</td><td>" + Phone + "</td></tr>";
        //    msg += "<tr><td>Message</td><td>" + message + "</td></tr></table>";

        //    string html = Repohtml.GetHtml("Admin", Heading, msg);

        //    return sendMail.MailSender(contactFormEmail, html, Heading);
        //}
        //#endregion

        //#region Advertise form
        //public string sendadvertiseMail(string cityname, string email, string fullname, string resname, string phone)
        //{
        //    string contactFormEmail = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    string sitename = ConfigurationManager.AppSettings["sitename"].ToString();
        //    string Heading = "Contact form from " + sitename;
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    string msg = "<table><tr><td>City</td><td>" + cityname + "</td></tr>";
        //    msg += "<tr><td>Email</td><td>" + email + "</td></tr>";
        //    msg += "<tr><td>Full Name</td><td>" + fullname + "</td></tr>";
        //    msg += "<tr><td>Restaurant</td><td>" + resname + "</td></tr>";
        //    msg += "<tr><td>Phone</td><td>" + phone + "</td></tr></table>";

        //    string html = Repohtml.GetHtml("Admin", Heading, msg);

        //    return sendMail.MailSender(contactFormEmail, html, Heading);
        //}
        //#endregion

        //#region User Activity EMails
        //public string SendEmailOnCommentLike(string Heading, string msg, string likedUsername, string Username,
        //    string Email, string comment, string UserprofileLink, string commentLink)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    msg = msg.Replace("@likedUsername", likedUsername);
        //    msg = msg.Replace("@Username", "<a href='" + UserprofileLink + "'>" + Username + "</a>");
        //    msg = msg.Replace("@commentLink", "<a href='" + sitepath + commentLink + "'>Comment</a>");
        //    msg = msg.Replace("@comment", comment);
        //    Heading = Heading.Replace("@Username", Username);
        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        //public string SendEmailOnRestaurantPhotoLike(string Heading, string msg, string likedUsername, string Username, string UserprofileLink,
        //    string Email, string RestaurantName, string Restauranturl, string Photo, string PhotoLink)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    msg = msg.Replace("@likedUsername", likedUsername);
        //    msg = msg.Replace("@Username", "<a href='" + UserprofileLink + "'>" + Username + "</a>");
        //    msg = msg.Replace("@RestaurantName", "<a href='" + Restauranturl + "'>" + RestaurantName + "</a>");
        //    msg = msg.Replace("@Photo", "<a href='" + sitepath + PhotoLink + "'><img src='" + Photo + "' /></a>");
        //    Heading = Heading.Replace("@Username", Username);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);
        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        //public string SendEmailOnRestaurantPhotoComment(string Heading, string msg, string commentUsername, string Username, string UserprofileLink,
        //  string Email, string RestaurantName, string Restauranturl, string Photo, string PhotoLink, string comment, string commentLink)
        //{
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    msg = msg.Replace("@commentUsername", commentUsername);
        //    msg = msg.Replace("@Username", "<a href='" + UserprofileLink + "'>" + Username + "</a>");
        //    msg = msg.Replace("@RestaurantName", "<a href='" + Restauranturl + "'>" + RestaurantName + "</a>");
        //    msg = msg.Replace("@commentLink", "<a href='" + sitepath + commentLink + "'>Continue Reading</a>");
        //    msg = msg.Replace("@comment", "<a href='" + sitepath + commentLink + "'>" + comment + "</a>");
        //    msg = msg.Replace("@Photo", "<a href='" + sitepath + PhotoLink + "'><img src='" + Photo + "' /></a>");

        //    Heading = Heading.Replace("@Username", Username);
        //    Heading = Heading.Replace("@RestaurantName", RestaurantName);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}

        //public string SendEmailOnFollow(string Heading, string msg, string Username, string UserprofileLink,
        // string Email, string totalFollowers, string profileImg, bool alreadyFollowing)
        //{
        //    string sitename = ConfigurationManager.AppSettings["sitename"].ToString();
        //    msg = msg.Replace("@Username", "<a href='" + UserprofileLink + "'>" + Username + "</a>");
        //    msg = msg.Replace("@Photo", "<a href='" + UserprofileLink + "'><img src='" + profileImg + "' /></a>");
        //    msg = msg.Replace("@TotalFollowers", totalFollowers);
        //    if (alreadyFollowing)
        //        msg = msg.Replace("@AlreadyFollowing", "<a href='" + UserprofileLink + "'>Already Following</a>");
        //    else
        //        msg = msg.Replace("@AlreadyFollowing", "<a href='" + UserprofileLink + "'>Follow</a>");
        //    msg = msg.Replace("@SiteName", sitename);
        //    Heading = Heading.Replace("@Username", Username);
        //    Heading = Heading.Replace("@SiteName", sitename);
        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);

        //    return str;
        //}
        //#endregion



        //#region Book A Table
        //public string SendBookATableConfirmationCode(string Heading, string msg, string Name, string Email, string RestaurantName, string Code)
        //{
        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@ResName", RestaurantName);
        //    msg = msg.Replace("@Code", Code);
        //    msg = msg.Replace("@Email", Email);

        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@ResName", RestaurantName);
        //    Heading = Heading.Replace("@Email", Email);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(Email, html, Heading);
        //    string adminEmail = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    clsGeneralSettings cls = new clsGeneralSettings();
        //    List<GeneralSettingsModel> list = cls.getAllGeneralSettings();
        //    if (list[0].IsTableBook)
        //    {
        //        var adminMail = sendMail.MailSender(adminEmail, html, Heading);
        //    }
        //    return str;
        //}

        //public string SendBookATableMail(string Heading, string msg, string Name, string Email, string RestaurantName, string Phone, string BookingDate, string BookingTime, string NumGuest, string MailsendEmail)
        //{
        //    msg = msg.Replace("@Name", Name);
        //    msg = msg.Replace("@Email", Email);
        //    msg = msg.Replace("@ResName", RestaurantName);
        //    msg = msg.Replace("@Phone", Phone);
        //    msg = msg.Replace("@BookingDate", BookingDate);
        //    msg = msg.Replace("@BookingTime", BookingTime);
        //    msg = msg.Replace("@NumGuest", NumGuest);


        //    Heading = Heading.Replace("@Name", Name);
        //    Heading = Heading.Replace("@Email", Email);
        //    Heading = Heading.Replace("@ResName", RestaurantName);
        //    Heading = Heading.Replace("@Phone", Phone);
        //    Heading = Heading.Replace("@BookingDate", BookingDate);
        //    Heading = Heading.Replace("@BookingTime", BookingTime);
        //    Heading = Heading.Replace("@NumGuest", NumGuest);

        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(MailsendEmail, html, Heading);

        //    return str;
        //}
        //#endregion

        //#region Blogs
        //public string SendEmailOnBlogConfirmation(string Heading, string msg, string Username, string UserEmail)
        //{
        //    string Email = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    string sitepath = ConfigurationManager.AppSettings["sitepath"].ToString();
        //    msg = msg.Replace("@Name", Username);
        //    msg = msg.Replace("@BlogLink", "<a href='" + sitepath + "/blogFaq" + "'>Blogger FAQs</a>");
        //    msg = msg.Replace("@EMail", "<a href='" + "mailto:" + Email + "'>" + Email + "</a>");
        //    string html = Repohtml.GetHtml("", Heading, msg);
        //    var str = sendMail.MailSender(UserEmail, html, Heading);

        //    return str;
        //}

        //public string sendBlogPostMailToAdmin(string Url)
        //{
        //    string contactFormEmail = ConfigurationManager.AppSettings["contactFormEmail"].ToString();
        //    string Heading = "New Blog Post Request";
        //    string msg = "<table>";
        //    msg += "<tr><td>Url</td><td>" + Url + "</td></tr></table>";

        //    string html = Repohtml.GetHtml("Admin", Heading, msg);

        //    return sendMail.MailSender(contactFormEmail, html, Heading);
        //}

        //#endregion


        //public string SendMsg(string mobileNumber, string msg)
        //{
        //    try
        //    {
        //        //var client = new RestClient("http://58.65.138.38:8181/sc/smsApi/sendSms?username=ICW-Demo&password=654321&mobileNumber=918890783004&message=test&mask=ICW-Demo");
        //        //var request = new RestRequest(Method.POST);
        //        //request.AddHeader("postman-token", "fa8dfdd0-05b6-e1fe-e172-8d642b1e9aac");
        //        //request.AddHeader("cache-control", "no-cache");
        //        //IRestResponse response = client.Execute(request);
        //        var api = "http://58.65.138.38:8181/sc/smsApi/sendSms?username=ICW-Demo&password=654321&mobileNumber='" + mobileNumber + "'&message='" + msg + "'&mask=ICW-Demo";
        //        return api;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }

        //}

    }
}