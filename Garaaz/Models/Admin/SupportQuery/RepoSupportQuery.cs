using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class ResponseSupportQuery
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Subject required.")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Message required.")]
        public string Message { get; set; }
        public string UserId { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UpdateSupportQuery
    {
        [Required(ErrorMessage = "Id required.")]
        public int Id { get; set; }
    }
    public class RepoSupportQuery
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion

        #region SupportQuery
        #region Get All SupportQuery
        public List<ResponseSupportQuery> GetAllSupportQuery()
        {
            return (from S in db.SupportQueries
                    select new ResponseSupportQuery()
                    {
                        Id = S.Id,
                        Subject = S.Subject,
                        Message = S.Message,
                        UserId = S.UserId,
                        Phone = S.AspNetUser.UserName ?? "",
                        Email = S.AspNetUser.Email ?? "",
                    }).ToList();
        }

        #endregion

        #region Get Support Query By Id
        public ResponseSupportQuery GetSupportQueryById(int Id)
        {
            var data = db.SupportQueries.Where(x => x.Id == Id).FirstOrDefault();
            return new ResponseSupportQuery()
            {
                Id = data.Id,
                Subject = data.Subject,
                Message = data.Message,
                UserId = data.UserId,
                Phone = data.AspNetUser.UserName ?? "",
                Email = data.AspNetUser.Email ?? "",
            };
        }
        #endregion

        #region Save/Update Support Query 
        public bool SaveSupportQuery(ResponseSupportQuery model, List<ApplicationUser> superAdmins)
        {
            bool status = false;
            var old = db.SupportQueries.Where(m => m.Id == model.Id).FirstOrDefault();
            if (old != null)
            {
                old.Id = model.Id.Value;
                old.Subject = model.Subject;
                old.Message = model.Message;
                old.UserId = model.UserId;
                db.SaveChanges();
                return true;
            }
            else
            {
                var Data = new SupportQuery()
                {
                    Subject = model.Subject,
                    Message = model.Message,
                    UserId = model.UserId,
                    CreatedDate = DateTime.Now
                };
                db.SupportQueries.Add(Data);
                status = db.SaveChanges() > 0;
                if (status)
                {
                    if (superAdmins.Any())
                    {
                        foreach (var sa in superAdmins)
                        {
                            var notification = new Notification()
                            {
                                UserId = sa.Id.ToString(),
                                Type = Constants.SupportType,
                                Message = "New Query from support screen.",
                                IsRead = false,
                                CreatedDate = DateTime.Now,
                                RefUserId = Data.Id.ToString(),//userId,
                                WorkshopId = null,
                            };
                            db.Notifications.Add(notification);
                            db.SaveChanges();
                            //var superAdmin = db.UserDetails.Where(x => x.UserId == sa.Id).FirstOrDefault();
                            //if (mail != null)
                            //{
                            //    var resultNew = clsEmailMgt.SendMailOrderPlaced(mail.MailHeading, mail.MailHtml, $"{superAdmin.FirstName + " " + superAdmin.LastName}", superAdmin.AspNetUser.Email, superAdmin.AspNetUser.UserName, orderData, Constants.RupeeSign, true);
                            //}
                        }
                    }
                }
                return status;
            }
        }
        #endregion]

        #region Delete  Support Query 
        public bool DeleteSupportQuery(int Id)
        {
            var data = db.SupportQueries.Where(s => s.Id == Id).FirstOrDefault();
            if (data == null)
            {
                return false;
            }
            else
            {
                db.SupportQueries.Remove(data);
                return db.SaveChanges() > 0;
            }
        }
        #endregion

        #endregion
    }
}