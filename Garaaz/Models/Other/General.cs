using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Garaaz.Models
{
    public class General
    {
        #region Is SuperAdmin
        public static bool IsSuperAdmin()
        {
            return GetUserRole() == Constants.SuperAdmin;
        }
        #endregion

        #region Is Distributor
        public static bool IsDistributor()
        {
            return GetUserRole() == Constants.Distributor;
        }
        #endregion

        #region Is Workshop
        public static bool IsWorkShop()
        {
            return GetUserRole() == Constants.Workshop;
        }
        #endregion

        #region Is Workshop User
        public static bool IsWorkShopUser()
        {
            return GetUserRole() == Constants.WorkshopUsers;
        }
        #endregion

        #region Is Outlet
        public static bool IsOutlet()
        {
            return GetUserRole() == Constants.DistributorOutlets;
        }
        #endregion

        #region Is RoIncharge
        public static bool IsRoIncharge()
        {
            return GetUserRole() == Constants.RoIncharge;
        }
        #endregion

        #region Is SalesExecutive
        public static bool IsSalesExecutive()
        {
            return GetUserRole() == Constants.SalesExecutive;
        }
        #endregion

        #region Is User
        public static bool IsUser()
        {
            return GetUserRole() == Constants.Users;
        }
        #endregion

        #region Get UserId
        public static string GetUserId()
        {
            Utils utils = new Utils();
            return utils.getStringCookiesValue(Constants.UserIdSession);
        }
        #endregion

        #region Get Username
        public static string GetUsername()
        {
            Utils utils = new Utils();
            return utils.getStringCookiesValue(Constants.UsernameSession);
        }
        #endregion

        #region Get Refresh Token
        public static string GetRefreshToken()
        {
            Utils utils = new Utils();

            return utils.getStringCookiesValue(Constants.UserRefreshTokenSession);
        }
        #endregion

        #region Get Token
        public static string GetToken()
        {
            Utils utils = new Utils();

            return utils.getStringCookiesValue(Constants.UserTokenSession);
        }
        #endregion

        #region Get UserRole
        public static string GetUserRole(bool isFromMobile = false)
        {
            if (isFromMobile || HttpContext.Current.Request.IsAuthenticated)
            {
                Utils utils = new Utils();
                return utils.getStringCookiesValue(Constants.UserRoleSession);
            }

            return "";
        }
        #endregion

        #region Web Request
        public static string GetToken(string url, string postData)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.8.1.3) Gecko/20070309 Firefox/2.0.0.3";
            request.ProtocolVersion = HttpVersion.Version10;
            request.KeepAlive = true;
            request.AllowAutoRedirect = false;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Accept = "text/xml,application/xml,application/xhtml+xml,text/html;q=0.9,text/plain;q=0.8,image/png,*/*;q=0.5";
            request.Timeout = 15000;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw;
            }
        }
        #endregion

        #region Excel File Upload

        /// <summary>
        /// Read excel file and return table with data.
        /// </summary>
        /// <param name="filePath">The full path of uploaded excel file.</param>
        /// <returns>Return DataTable.</returns>
        public static DataTable GetTableFromExcelFile(string filePath)
        {
            // Ref - https://www.connectionstrings.com/excel/
            var conString = $"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={filePath}; Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1;\"";

            var dataSet = new DataSet();
            using (var oleDbCon = new OleDbConnection(conString))
            {
                oleDbCon.Open();                

                var sheetName = oleDbCon.GetSheetName(); 
                var oleDbCmd = new OleDbCommand($"SELECT * FROM [{sheetName}]", oleDbCon);
                var oleDbAdapter = new OleDbDataAdapter(oleDbCmd);
                oleDbAdapter.Fill(dataSet);
            }

            try
            {
                File.Delete(filePath);
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            // Remove rows with empty values
            var table = dataSet.Tables[0];
            var rowsWithData = table.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.CompareOrdinal((field as string)?.Trim(), string.Empty) == 0)).ToList();

            if (rowsWithData.Count == 0) throw new Exception("No records were found in excel file.");

            var refinedTable = rowsWithData.CopyToDataTable();
            if (refinedTable.Rows.Count == 0) return refinedTable;

            // Trim column names
            foreach (DataColumn col in refinedTable.Columns)
            {
                col.ColumnName = col.ColumnName.Trim();
            }

            return refinedTable;
        }
        #endregion

        #region check Image url Local or Online
        public string CheckImageUrl(string img)
        {
            if (string.IsNullOrEmpty(img) || !img.StartsWith("http") && !File.Exists(HttpContext.Current.Server.MapPath(img)))
            {
                return ConfigurationManager.AppSettings["WebUrl"] + "/assets/images/NoPhotoAvailable.png";
            }

            return img.StartsWith("http") ? img : ConfigurationManager.AppSettings["WebUrl"] + img;
        }
        #endregion

        #region Get Current DistributorId

        public static int GetCurrentDistributorId()
        {
            var loginUserId = GetUserId();
            var db = new RepoUsers();

            var distributor = db.GetDistributorByUserId(loginUserId);
            return distributor?.DistributorId ?? 0;
        }

        public int GetDistributorId(string userId)
        {
            int distId = 0;
            RepoUsers db = new RepoUsers();
            var dist = db.GetDistributorByUserId(userId);
            if (dist != null)
                distId = dist.DistributorId;

            return distId;
        }
        public int GetWorkshopId(string userId)
        {
            int wsId = 0;
            var ru = new RepoUsers();
            var ws = ru.GetWorkshopByUserId(userId);
            if (ws != null)
                wsId = ws.WorkShopId;

            return wsId;
        }
        //public int getDistributorIdByUserId(string UserId, string role)
        //{
        //    RepoUsers db = new RepoUsers();
        //    return db.getDistributorIdByUserId(UserId, role);
        //}
        //public int GetWorkshopIdByUserId(string userId, string role)
        //{
        //    var ru = new RepoUsers();
        //    return ru.GetWorkshopIdByUserId(userId, role);
        //}
        #endregion

        #region Get OutletId by Userid
        public int GetOutletByUserId(string UserId)
        {
            int OutleId = 0;
            RepoUsers db = new RepoUsers();
            var disout = db.GetOutletByUserId(UserId);
            if (disout != null)
                OutleId = disout.OutletId;

            return OutleId;
        }
        #endregion

        #region Get Distributor User's Menue by Userid
        public List<Feature> GetDistributorUserMenue(string UserId)
        {
            List<Feature> lstfeature = new List<Feature>();
            garaazEntities db = new garaazEntities();
            var featureIds = db.UserFeatures.Where(x => x.UserId == UserId && x.Feature.HasValue && x.Feature.Value).Select(x => x.FeatureId).ToList();
            if (featureIds.Any())
            {
                var Features = db.Features.Where(f => featureIds.Contains(f.FeatureId)).ToList();
                foreach (var od in Features)
                {
                    Feature feature = new Feature();
                    feature.FeatureId = od.FeatureId;
                    feature.FeatureName = od.FeatureName;
                    feature.FeatureValue = od.FeatureValue;
                    feature.IsDefault = od.IsDefault;
                    lstfeature.Add(feature);
                }
            }
            return lstfeature;
        }
        #endregion

        #region Update Mobile Number
        public bool UpdateMobile(string mobilenumber)
        {
            Utils utils = new Utils();
            utils.setCookiesValue(mobilenumber, "UsernameSession");
            return true;
        }
        #endregion
    }
}