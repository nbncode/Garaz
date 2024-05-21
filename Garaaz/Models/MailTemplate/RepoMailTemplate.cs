using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoMailTemplate
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General general = new General();

        // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
        #endregion

        public List<SelectListItem> GetTypes()
        {
            try
            {
                List<SelectListItem> types = new List<SelectListItem>() {
                        new SelectListItem(){Text = "--Select--",Value = ""},
                        new SelectListItem() { Text = "Pending", Value = "Pending" },
                        new SelectListItem() { Text = "Confirmed", Value = "Confirmed" },
                        new SelectListItem() { Text = "In Process", Value = "InProcess" },
                        new SelectListItem() { Text = "Out for delivery", Value = "OutForDelivery" },
                        new SelectListItem() { Text = "Delivered", Value = "Delivered" },
                        new SelectListItem() { Text = "In Review", Value = "InReview" },
                        new SelectListItem() { Text = "Back Order", Value = "BackOrder" },
                        new SelectListItem() { Text = "Order Cancel", Value = "OrderCancel" },
                        new SelectListItem() { Text = "Back Order Cancel", Value = "BackOrderCancel" },
                        new SelectListItem() { Text = "Back Order Accept", Value = "AcceptBackOrder" },
                        new SelectListItem() { Text = "OTP Sent", Value = "OtpSent" },
                };

                return types;
            }
            catch (Exception ee)
            {
                throw ee;
            }
        }
        public bool Add_Update_MailTemplate(MailTemplate model, out string resultMsg)
        {
            bool status = false; resultMsg = string.Empty;
            if (model.Id > 0)
            {
                var existing = db.MailTemplates.Where(x => x.Type == model.Type && x.Id != model.Id).FirstOrDefault();
                if (existing != null)
                {
                    resultMsg = $"Already exists for type: {model.Type}";
                    return status;
                }
                var data = db.MailTemplates.Where(x => x.Id == model.Id).FirstOrDefault();
                if (data != null)
                {
                    data.MailHeading = model.MailHeading;
                    data.MailHtml = model.MailHtml;
                    data.Description = model.Description;
                    data.SmsText = model.SmsText;
                    data.Type = model.Type;
                    db.SaveChanges();
                    resultMsg = $"Mail template Update Sucessfully";
                    return true;
                }
            }
            else
            {
                var existing = db.MailTemplates.Where(x => x.Type == model.Type).FirstOrDefault();
                if (existing != null)
                {
                    resultMsg = $"Already exists for type: {model.Type}";
                    return status;
                }
                model.CreatedDate = DateTime.Now;
                db.MailTemplates.Add(model);
                status = db.SaveChanges() > 0;
                resultMsg = $"Mail template Added Sucessfully";
            }
            return status;
        }

        public List<MailTemplate> GetAllMailTemplate()
        {
            return db.MailTemplates.ToList();
        }
        public MailTemplate GetMailTemplateById(int id)
        {
            return db.MailTemplates.Where(x => x.Id == id).FirstOrDefault();
        }
        public bool Delete(int id)
        {
            var data = db.MailTemplates.Where(x => x.Id == id).FirstOrDefault();
            db.MailTemplates.Remove(data);
            return db.SaveChanges() > 0;
        }
    }
}