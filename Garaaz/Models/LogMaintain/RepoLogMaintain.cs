using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Garaaz.Models
{
    public class RepoLogMaintain
    {
        #region Variables
        garaazEntities db = new garaazEntities();
        #endregion
        
        #region Save Login Detail
        public bool SaveLoginDetail(LoginTime model)
        {
            db.LoginTimes.Add(model);
            return db.SaveChanges() > 0;
        }
        #endregion


    }
}