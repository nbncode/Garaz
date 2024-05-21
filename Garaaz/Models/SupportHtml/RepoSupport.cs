using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace Garaaz.Models
{
    public class RepoSupport
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        General general = new General();

        // In order - Gold, Orange, Green, Deep sky blue, hot pink, dim gray, tan, red, dark khaki and teal
        #endregion


        public List<Support> GetSupportHtml()
        {
            var data = db.Supports.ToList();
            return data;
        }
        public Support GetSupportTicket()
        {
            var data = db.Supports.FirstOrDefault();
            return data;
        }
        public Support GetSupportHtmlById(int Id)
        {
            return db.Supports.Where(x => x.Id == Id).FirstOrDefault();
        }
        public bool AddSupportHtml(Support model)
        {
            if (model.Id > 0)
            {
                var data = db.Supports.Where(x => x.Id == model.Id).FirstOrDefault();
                if (data != null)
                {
                    data.Heading = model.Heading;
                    data.Description = model.Description;
                    db.SaveChanges();
                    return true;
                }
            }
            model.CreatedDate = DateTime.Now;
            db.Supports.Add(model);
            return db.SaveChanges() > 0;
        }
        public bool RemoveSupportHtml(int Id)
        {
            var data = db.Supports.Where(x => x.Id == Id).FirstOrDefault();
            if (data != null)
            {
                db.Supports.Remove(data);
                return db.SaveChanges() > 0;
            }
            return false;
        }
    }
}