using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Garaaz.Models.DeleteData
{
    public class RepoDataDelete
    {
        private readonly garaazEntities db = new garaazEntities();

        #region Delete accounts for distributor
        public bool DistributorAccountsDelete(DailySale model)
        {
            // save workshop data
            SqlParameter[] sqlParameters = {
                            new SqlParameter("@DistributorId",model.DistributorId)
                        };
            return SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "Sp_DeleteAccountsData", sqlParameters) > 0;
        }
        #endregion
    }
}