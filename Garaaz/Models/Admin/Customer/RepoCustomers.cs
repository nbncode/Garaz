using Garaaz.Controllers;
using Garaaz.Models.Import;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Garaaz.Models
{
    public class RepoCustomers
    {
        private readonly garaazEntities _db = new garaazEntities();

        /// <summary>
        /// Save the user uploaded excel file.
        /// </summary>
        /// <param name="file">The file that have been uploaded by client.</param>
        /// <returns>Returns the path of successfully uploaded file.</returns>
        public string SaveExcelFile(HttpPostedFileBase file)
        {
            // Save the user uploaded file
            var newFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";
            var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/files/"), newFileName);
            file.SaveAs(path);
            return path;
        }

        #region Import Outlets

        /// <summary>
        /// Fetch and populate outlets.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The selected distributor Id.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateOutlets(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessOutlets(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessOutlets(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var importErrorMsg = new List<ImportErrorMessage>();

            try
            {
                foreach (DataRow row in refinedTable.Rows)
                {
                    var outletCode = refinedTable.ColumnValue(row, "OutletCode");
                    var outletName = refinedTable.ColumnValue(row, "OutletName");

                    try
                    {
                        if (string.IsNullOrWhiteSpace(outletCode))
                        {
                            throw new Exception("Outlet code missing");
                        }

                        if (string.IsNullOrWhiteSpace(outletName))
                        {
                            throw new Exception("Outlet name missing");
                        }

                        var address = refinedTable.ColumnValue(row, "Address");

                        var responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                        SqlParameter[] outletParameter = {
                        new SqlParameter("@distributorId",distributorId),
                        new SqlParameter("@OutletCode",outletCode),
                        new SqlParameter("@OutletName",outletName),
                        new SqlParameter("@Address",address),
                       responseParam
                        };

                        populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "dbo.usp_ImportOutlets", outletParameter) > 0;

                        var status = Convert.ToBoolean(responseParam.Value);
                        if (!status)
                        {
                            throw new Exception($"Outlet with outlet code '{outletCode}' already exists for another distributor.");
                        }

                        imported++;
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = !string.IsNullOrWhiteSpace(outletCode) ? outletCode : outletName,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }
                populated = imported > 0;
                #region Comment old code
                //using (var db = new garaazEntities())
                //{
                //    var outlets = db.Outlets.ToList();
                //    var distOutlets = db.DistributorsOutlets.ToList();

                //    foreach (DataRow row in refinedTable.Rows)
                //    {
                //        try
                //        {
                //            var outletName = refinedTable.ColumnValue(row, "OutletName");
                //            if (string.IsNullOrEmpty(outletName)) continue;

                //            var outlet = new Outlet
                //            {
                //                OutletName = outletName,
                //                Address = refinedTable.ColumnValue(row, "Address")
                //            };

                //            var existingOutlet = outlets.FirstOrDefault(o => o.OutletName.Equals(outletName, StringComparison.OrdinalIgnoreCase));
                //            if (existingOutlet != null)
                //            {
                //                outlet.OutletId = existingOutlet.OutletId;
                //                existingOutlet.Address = outlet.Address;
                //                db.SaveChanges();
                //                populated = true;
                //            }
                //            else
                //            {
                //                db.Outlets.Add(outlet);
                //                populated = db.SaveChanges() > 0;
                //            }

                //            // Create distributor outlet
                //            var existingDistOutlet = distOutlets.FirstOrDefault(d =>
                //                d.OutletId == outlet.OutletId && d.DistributorId == distributorId);
                //            if (existingDistOutlet != null) continue;

                //            var distOutlet = new DistributorsOutlet
                //            {
                //                DistributorId = distributorId,
                //                Outlet = outlet
                //            };
                //            db.DistributorsOutlets.Add(distOutlet);
                //            db.SaveChanges();
                //        }
                //        catch (Exception exc)
                //        {
                //            RepoUserLogs.LogException(exc);
                //        }
                //    }
                //}
                #endregion
                if (populated)
                {
                    SetFileUploadDate(distributorId, Constants.OutletImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        #endregion

        #region Import RO Incharge

        /// <summary>
        /// Fetch and populate RO Incharge.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The selected distributor Id.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateRoIncharge(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessRoIncharge(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessRoIncharge(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var rowCount = 1;
            var importErrorMsg = new List<ImportErrorMessage>();

            try
            {
                List<Outlet> outlets;
                List<AspNetUser> users;
                List<DistributorUser> distributorUsers;
                var sc = new SystemController();
                var repoOutlet = new RepoOutlet();

                var phoneNumbers = refinedTable.AsEnumerable().Select(r => Convert.ToString(r.Field<double>("PhoneNumber"))).ToList();

                using (var db = new garaazEntities())
                {
                    outlets = (from o in db.Outlets
                               join d in db.DistributorsOutlets on o.OutletId equals d.OutletId
                               where d.DistributorId == distributorId
                               select o).ToList();

                    users = db.AspNetUsers.AsNoTracking().Where(a => phoneNumbers.Contains(a.UserName)).ToList();
                    distributorUsers = db.DistributorUsers.AsNoTracking().Where(a => a.DistributorId == distributorId).ToList();
                }

                foreach (DataRow row in refinedTable.Rows)
                {
                    rowCount++;
                    var empCode = refinedTable.ColumnValue(row, "EmployeeCode");
                    try
                    {
                        var firstName = refinedTable.ColumnValue(row, "FirstName");
                        var lastName = refinedTable.ColumnValue(row, "LastName");
                        var phoneNumber = refinedTable.ColumnValue(row, "PhoneNumber");
                        var outletCode = refinedTable.ColumnValue(row, "OutletCode");

                        if (string.IsNullOrWhiteSpace(empCode))
                        {
                            throw new Exception($"Employee code missing on row {rowCount}");
                        }
                        if (string.IsNullOrWhiteSpace(firstName))
                        {
                            throw new Exception("First name missing");
                        }
                        if (string.IsNullOrWhiteSpace(lastName))
                        {
                            throw new Exception("Last name missing");
                        }
                        if (string.IsNullOrWhiteSpace(phoneNumber))
                        {
                            throw new Exception("Phone number missing");
                        }
                        if (string.IsNullOrWhiteSpace(outletCode))
                        {
                            throw new Exception("Outlet code missing");
                        }

                        var outlet = outlets.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.OutletCode) && o.OutletCode.Equals(outletCode, StringComparison.OrdinalIgnoreCase));
                        if (outlet == null)
                        {

                            throw new Exception($"Outlet code '{outletCode}' doesn't match for current distributor.");
                        }

                        // show error if user already register for another distributor
                        var roUserId = users.FirstOrDefault(a => a.UserName == phoneNumber)?.Id;
                        var disUser = distributorUsers.FirstOrDefault(a => a.UserId == roUserId);
                        if (disUser == null && !string.IsNullOrEmpty(roUserId))
                        {
                            throw new Exception($"User already exists for another distributor on row {rowCount}");
                        }
                        var distUserInfo = new clsDistributorUserInfo
                        {
                            UserId = roUserId,
                            EmployeeCode = empCode,
                            FirstName = firstName,
                            LastName = lastName,
                            PhoneNumber = phoneNumber,
                            Address = outlet.Address,
                            Latitude = "0",
                            Longitude = "0",
                            Password = "Temp@123",
                            Role = Constants.RoIncharge,
                            DistributorId = distributorId
                        };

                        var result = sc.DistributorUsersRegisterOrUpdate(distUserInfo);
                        if (result?.ResultFlag == 1 && result.Data is string userId)
                        {
                            var isNew = repoOutlet.SaveUserForOutlet(userId, distributorId, outlet.OutletId);
                            if (isNew)
                            {
                                // register WalkInCustomer workshop for RoIncharge
                                var wsd = new WorkShopData
                                {
                                    CustomerName = CustomerType.WalkInCustomer,
                                    CustomerType = CustomerType.WalkInCustomer,
                                    MobilePhone = string.Concat(phoneNumber + "_1"),
                                    BranchCode = outletCode,
                                    CustomerCode = string.Concat(outletCode + "WI")
                                };
                                var status = RegisterOrUpdateWorkshop(wsd, distributorId);
                                if (status.ResultFlag == 0)
                                {
                                    throw new Exception($"{status.Message} for outletCode'{outletCode}'.");
                                }
                            }

                            populated = true;
                            imported++;
                        }
                        else
                        {
                            if (result != null)
                            {
                                throw new Exception($"Cannot create RO Incharge with employee code '{empCode}'. More details - {result.Message}");
                            }
                            skipped++;
                        }
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = empCode,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }
                populated = imported > 0;
                if (populated)
                {
                    SetFileUploadDate(distributorId, Constants.RoInchargeImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        #endregion

        #region Import Sales Executive

        /// <summary>
        /// Fetch and populate sales executives.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The selected distributor Id.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateSalesExecutives(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessSalesExecutives(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessSalesExecutives(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var importErrorMsg = new List<ImportErrorMessage>();

            try
            {
                var excelRoCodes = refinedTable.AsEnumerable().Select(row => Convert.ToString(row["ROInchargeCode"]));

                // Get all RO users for this distributor
                var db = new garaazEntities();
                var distRoUsers = (from a in db.UserDetails
                                   join d in db.DistributorsOutlets on a.UserId equals d.UserId
                                   where a.IsDeleted.Value == false
                                   && d.DistributorId == distributorId
                                   && a.AspNetUser.AspNetRoles.Any(r => r.Name == Constants.RoIncharge)
                                   && excelRoCodes.Contains(a.ConsPartyCode)
                                   select new
                                   {
                                       a.ConsPartyCode,
                                       a.UserId
                                   }).ToList();


                var distSeUsers = (from a in db.UserDetails
                                   join d in db.DistributorUsers on a.UserId equals d.UserId
                                   where d.DistributorId == distributorId
                                   && a.AspNetUser.AspNetRoles.Any(r => r.Name == Constants.SalesExecutive)
                                   select a).ToList();

                foreach (DataRow row in refinedTable.Rows)
                {
                    var roInchargeCode = refinedTable.ColumnValue(row, "ROInchargeCode");
                    var roUserId = distRoUsers.FirstOrDefault(u => u.ConsPartyCode == roInchargeCode)?.UserId;

                    var userInfo = new clsDistributorUserInfo
                    {
                        EmployeeCode = refinedTable.ColumnValue(row, "EmployeeCode"),
                        FirstName = refinedTable.ColumnValue(row, "FirstName"),
                        LastName = refinedTable.ColumnValue(row, "LastName"),
                        PhoneNumber = refinedTable.ColumnValue(row, "Mobile"),
                        Password = "Temp@123",
                        RoInchargeId = roUserId ?? "",
                        DistributorId = distributorId,
                        Role = Constants.SalesExecutive
                    };

                    try
                    {
                        if (string.IsNullOrWhiteSpace(userInfo.EmployeeCode))
                        {
                            throw new Exception("EmployeeCode missing");
                        }

                        if (string.IsNullOrWhiteSpace(userInfo.FirstName))
                        {
                            throw new Exception("FirstName missing");
                        }
                        if (string.IsNullOrWhiteSpace(userInfo.LastName))
                        {
                            throw new Exception("LastName missing");
                        }
                        if (string.IsNullOrWhiteSpace(userInfo.PhoneNumber))
                        {
                            throw new Exception("Mobile missing");
                        }
                        if (string.IsNullOrWhiteSpace(userInfo.Password))
                        {
                            throw new Exception("Password missing");
                        }
                        if (string.IsNullOrWhiteSpace(roInchargeCode))
                        {
                            throw new Exception("ROInchargeCode missing");
                        }
                        if (string.IsNullOrWhiteSpace(userInfo.RoInchargeId))
                        {
                            throw new Exception($"No RO Incharge found with ROInchargeCode {roInchargeCode}");
                        }

                        // Register or update SE user
                        var seUserId = distSeUsers.FirstOrDefault(u => u.AspNetUser.UserName == userInfo.PhoneNumber)?.UserId;
                        userInfo.UserId = seUserId;
                        var sc = new SystemController();
                        var result = sc.DistributorUsersRegisterOrUpdate(userInfo);

                        if (result?.ResultFlag == 1)
                        {
                            var ru = new RepoUsers();
                            // Add or update RoIncharge for SalesExecutive
                            ru.SaveRoSalesExecutive(userInfo.RoInchargeId, result.Data.ToString());
                            imported++;
                            populated = true;
                        }
                        else
                        {
                            populated = false;
                            skipped++;
                            importErrorMsg.Add(new ImportErrorMessage
                            {
                                UniqueId = !string.IsNullOrWhiteSpace(userInfo.EmployeeCode) ? userInfo.EmployeeCode : userInfo.FirstName + " " + userInfo.LastName,
                                ErrorMsg = result.Message
                            });
                        }
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = !string.IsNullOrWhiteSpace(userInfo.EmployeeCode) ? userInfo.EmployeeCode : userInfo.FirstName + " " + userInfo.LastName,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.SalesExecutiveImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        #endregion

        #region Import Workshops

        /// <summary>
        /// Fetch and populate workshops.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The selected distributor Id.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateWorkshop(string path, int distributorId, out int totalRecords, out int imported, out int skipped, out string message)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessWorkshops(distributorId, refinedTable, out imported, out skipped, out message);

            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessWorkshops(int distributorId, DataTable refinedTable, out int imported, out int skipped, out string message)
        {
            var populated = false;
            string customerName = null, customerCode = null;

            message = null;
            imported = 0;
            skipped = 0;
            var importErrorMsg = new List<ImportErrorMessage>();
            var workshopCodes = new List<string>();

            try
            {
                foreach (DataRow row in refinedTable.Rows)
                {
                    try
                    {
                        customerCode = refinedTable.ColumnValue(row, "Customer Code");
                        customerName = refinedTable.ColumnValue(row, "Customer Name");
                        var branchCode = refinedTable.ColumnValue(row, "Branch Code");
                        var mobileNumber = refinedTable.ColumnValue(row, "Mobile No");

                        if (string.IsNullOrEmpty(customerCode))
                        {
                            throw new Exception("Customer Code is required for creating workshop");
                        }
                        if (string.IsNullOrWhiteSpace(branchCode))
                        {
                            throw new Exception("Branch Code is required for creating workshop");
                        }
                        if (string.IsNullOrWhiteSpace(mobileNumber))
                        {
                            throw new Exception("Mobile No is required for creating workshop");
                        }
                        // NOTE: Branch Code is Outlet Code and Branch Name is Outlet Name

                        var wsd = new WorkShopData
                        {
                            SNO = refinedTable.ColumnValue(row, "S No"),
                            CustomerName = customerName,
                            CustomerType = refinedTable.ColumnValue(row, "Customer Type"),
                            Address1 = refinedTable.ColumnValue(row, "Address 1"),
                            Address2 = refinedTable.ColumnValue(row, "Address 2"),
                            Address3 = refinedTable.ColumnValue(row, "Address 3"),
                            City = refinedTable.ColumnValue(row, "City"),
                            State = refinedTable.ColumnValue(row, "State"),
                            Country = refinedTable.ColumnValue(row, "Country"),
                            PinCode = refinedTable.ColumnValue(row, "Pin Code"),
                            PartyOwner1 = refinedTable.ColumnValue(row, "Owner Full Name (1)*"),
                            MobilePhone = mobileNumber,
                            AlternateNumber = refinedTable.ColumnValue(row, "Alternate No#"),
                            PartyOwner2 = refinedTable.ColumnValue(row, "Owner Full Name (2)"),
                            //MobilePhone = refinedTable.ColumnValue(row, "Mobile No"),
                            Email = refinedTable.ColumnValue(row, "Email Address"),
                            PanNo = refinedTable.ColumnValue(row, "Pan No#"),
                            GstinNo = refinedTable.ColumnValue(row, "GST No#"),
                            PaymentType = refinedTable.ColumnValue(row, "Payment terms (Cash/Credit)"),
                            DiscountPer = refinedTable.ColumnValue(row, "Discount (%)"),
                            CreditDays = refinedTable.ColumnValue(row, "Credit Days"),
                            BranchCode = branchCode,
                            BranchName = refinedTable.ColumnValue(row, "Branch Name"),
                            SalesExecName = refinedTable.ColumnValue(row, "Sales Executive Name"),
                            SalesExecNumber = refinedTable.ColumnValue(row, "Sales Executive Number"),
                            CustomerCode = customerCode
                        };

                        // save workshop data
                        SqlParameter[] workshopParameter = {
                            new SqlParameter("@Action",1),
                            new SqlParameter("@SNO",wsd.SNO),
                            new SqlParameter("@CustomerName",wsd.CustomerName),
                            new SqlParameter("@CustomerType",wsd.CustomerType),
                            new SqlParameter("@Address1",wsd.Address1),
                            new SqlParameter("@Address2",wsd.Address2),
                            new SqlParameter("@Address3",wsd.Address3),
                            new SqlParameter("@City",wsd.City),
                            new SqlParameter("@State",wsd.State),
                            new SqlParameter("@Country",wsd.Country),
                            new SqlParameter("@PinCode",wsd.PinCode),
                            new SqlParameter("@PartyOwner1",wsd.PartyOwner1),
                            new SqlParameter("@MobilePhone",wsd.MobilePhone),
                            new SqlParameter("@AlternateNumber",wsd.AlternateNumber),
                            new SqlParameter("@PartyOwner2",wsd.PartyOwner2),
                            new SqlParameter("@Email",wsd.Email),
                            new SqlParameter("@PanNo",wsd.PanNo),
                            new SqlParameter("@GstinNo",wsd.GstinNo),
                            new SqlParameter("@PaymentType",wsd.PaymentType),
                            new SqlParameter("@DiscountPer",wsd.DiscountPer),
                            new SqlParameter("@CreditDays",wsd.CreditDays),
                            new SqlParameter("@BranchCode",wsd.BranchCode),
                            new SqlParameter("@BranchName",wsd.BranchName),
                            new SqlParameter("@SalesExecName",wsd.SalesExecName),
                            new SqlParameter("@SalesExecNumber",wsd.SalesExecNumber),
                            new SqlParameter("@CustomerCode",wsd.CustomerCode),
                        };
                        populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportWorkshop", workshopParameter) > 0;

                        var resultModel = RegisterOrUpdateWorkshop(wsd, distributorId);

                        if (resultModel != null)
                        {
                            if (resultModel.ResultFlag == 1)
                            {
                                imported++;
                                workshopCodes.Add(wsd.CustomerCode);
                            }
                            else
                            {
                                skipped++;
                            }
                        }

                        #region Comment old code

                        //var existingWsd = allWorkshopData.FirstOrDefault(w => w.CustomerCode == wsd.CustomerCode);
                        //if (existingWsd != null)
                        //{
                        //    existingWsd.SNO = wsd.SNO;
                        //    existingWsd.CustomerName = wsd.CustomerName;
                        //    existingWsd.CustomerType = wsd.CustomerType;
                        //    existingWsd.Address1 = wsd.Address1;
                        //    existingWsd.Address2 = wsd.Address2;
                        //    existingWsd.Address3 = wsd.Address3;
                        //    existingWsd.City = wsd.City;
                        //    existingWsd.State = wsd.State;
                        //    existingWsd.Country = wsd.Country;
                        //    existingWsd.PinCode = wsd.PinCode;
                        //    existingWsd.PartyOwner1 = wsd.PartyOwner1;
                        //    existingWsd.MobilePhone = wsd.MobilePhone;
                        //    existingWsd.AlternateNumber = wsd.AlternateNumber;
                        //    existingWsd.PartyOwner2 = wsd.PartyOwner2;
                        //    //existingWsd.MobilePhone  = wsd.MobilePhone;
                        //    existingWsd.Email = wsd.Email;
                        //    existingWsd.PanNo = wsd.PanNo;
                        //    existingWsd.GstinNo = wsd.GstinNo;
                        //    existingWsd.PaymentType = wsd.PaymentType;
                        //    existingWsd.DiscountPer = wsd.DiscountPer;
                        //    existingWsd.CreditDays = wsd.CreditDays;
                        //    existingWsd.BranchCode = wsd.BranchCode;
                        //    existingWsd.BranchName = wsd.BranchName;
                        //    existingWsd.SalesExecName = wsd.SalesExecName;
                        //    existingWsd.SalesExecNumber = wsd.SalesExecNumber;
                        //    existingWsd.CustomerCode = wsd.CustomerCode;

                        //    db.SaveChanges();
                        //    populated = true;
                        //}
                        //else
                        //{
                        //    db.WorkShopDatas.Add(wsd);
                        //    populated = db.SaveChanges() > 0;
                        ////}

                        //if (wsd.CustomerCode == "WRJ010137084")
                        //{
                        //    wsd.CustomerCode = "WRJ010137084";
                        //}

                        #endregion
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = !string.IsNullOrWhiteSpace(customerCode) ? customerCode : customerName,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.WorkshopImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            // call mapping sale with workshops
            if (workshopCodes.Count > 0)
            {
                var thread = new Thread(() => MapSaleToWorkshop(workshopCodes, distributorId))
                { IsBackground = true };
                thread.Start();
                message = "After some time we will do these workshops mapping with the sale.";
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        /// <summary>
        /// Register the workshop.
        /// </summary>
        /// <param name="wsd">The instance of WorkShopData.</param>
        /// <param name="distributorId">The distributor id for which workshop will be added.</param>
        private static ResultModel RegisterOrUpdateWorkshop(WorkShopData wsd, int distributorId)
        {
            ResultModel result;
            using (var db = new garaazEntities())
            {
                if (string.IsNullOrWhiteSpace(wsd.BranchCode))
                {
                    throw new Exception("Cannot create workshop as branch code is missing");
                }

                var outlet = (from a in db.DistributorsOutlets.AsNoTracking()
                              join b in db.Outlets.AsNoTracking() on a.OutletId equals b.OutletId
                              where a.DistributorId == distributorId && b.OutletCode == wsd.BranchCode
                              select b
                              ).FirstOrDefault();
                var outletId = outlet?.OutletId;

                // Check for existing workshop as per cons party code
                var ru = new RepoUsers();
                var user = db.UserDetails.FirstOrDefault(u => u.ConsPartyCode == wsd.CustomerCode);

                var ws = ru.GetWorkshopByUserId(user?.UserId);

                if (ws != null)
                {
                    var distWorkshop = db.DistributorWorkShops.FirstOrDefault(dw => dw.WorkShopId == ws.WorkShopId && dw.DistributorId == distributorId);
                    if (distWorkshop == null)
                    {
                        throw new Exception($"Cannot update workshop '{wsd.CustomerName}' with employee code '{wsd.CustomerCode}' and phone number '{wsd.MobilePhone}' already exists for another distributor.");
                    }

                    var ud = new UserDetail
                    {
                        UserId = user?.UserId,
                        FirstName = wsd.PartyOwner1,
                        LastName = string.Empty,
                        Address = $"{wsd.Address1},{wsd.Address2},{wsd.Address3}",
                        Designations = wsd.Designation,
                        ConsPartyCode = wsd.CustomerCode
                    };
                    ru.AddOrUpdateUsers(ud);

                    var wsModel = new WorkShop
                    {
                        WorkShopName = wsd.CustomerName,
                        Address = $"{wsd.Address1},{wsd.Address2},{wsd.Address3}",
                        City = wsd.City,
                        LandlineNumber = wsd.AlternateNumber,
                        Gstin = wsd.GstinNo,
                        outletId = outletId,
                        //CategoryName = wsd.CustomerType,
                        Pincode = wsd.PinCode,
                        State = wsd.State,
                        Type = wsd.CustomerType,
                    };
                    if (int.TryParse(wsd.CreditDays, out int osDays))
                        wsModel.CriticalOutstandingDays = osDays;
                    if (decimal.TryParse(wsd.CreditLimit, out decimal creditLimit))
                        wsModel.CreditLimit = creditLimit;

                    var isUpdated = ru.UpdateWorkShop(wsModel, ws.WorkShopId);

                    MapSalesExecToWorkshop(wsd);

                    return new ResultModel
                    {
                        ResultFlag = isUpdated ? 1 : 0,
                        Message = isUpdated ? "Successfully updated!" : "Failed to update."
                    };
                }
                else
                {
                    if (outletId == null)
                    {
                        throw new Exception($"Cannot create workshop as outlet was not found for outlet code '{wsd.BranchCode}'.");
                    }

                    var clsWs = new clsWorkshop
                    {
                        DistributorId = distributorId,
                        PhoneNumber = wsd.MobilePhone,
                        Email = wsd.Email,
                        Password = "Abc123$",
                        FirstName = wsd.PartyOwner1,
                        LastName = string.Empty,
                        EmployeeCode = wsd.CustomerCode,
                        WorkShopName = wsd.CustomerName,
                        Address = $"{wsd.Address1},{wsd.Address2},{wsd.Address3}",
                        City = wsd.City,
                        LandlineNumber = wsd.AlternateNumber,
                        Gstin = wsd.GstinNo,
                        Pincode = wsd.PinCode,
                        OutletId = outletId.Value,
                        State = wsd.State,
                        WorkshopType = wsd.CustomerType,
                        IsApproved = true
                    };

                    if (int.TryParse(wsd.CreditDays, out int osDays))
                        clsWs.CriticalOutstandingDays = osDays;
                    if (decimal.TryParse(wsd.CreditLimit, out decimal creditLimit))
                        clsWs.CreditLimit = creditLimit;

                    var sc = new SystemController();
                    result = sc.WorkshopRegisterOrUpdate(clsWs);

                    if (result.ResultFlag == 1)
                    {
                        MapSalesExecToWorkshop(wsd);
                    }
                    else
                    {
                        throw new Exception(result.Message);
                    }
                }
            }

            return result;
        }

        private static void MapSalesExecToWorkshop(WorkShopData wsd)
        {
            using (var db = new garaazEntities())
            {
                var ru = new RepoUsers();
                var workshopUser = db.UserDetails.AsNoTracking().FirstOrDefault(u => u.ConsPartyCode == wsd.CustomerCode);
                var ws = ru.GetWorkshopByUserId(workshopUser?.UserId);

                var salesExecUser = (from u in db.UserDetails.AsNoTracking()
                                     join a in db.AspNetUsers.AsNoTracking() on u.UserId equals a.Id
                                     where a.UserName.Equals(wsd.SalesExecNumber)
                                     select u).FirstOrDefault();

                if (salesExecUser != null)
                {
                    // not need to varify workshop outlets
                    // check sales executive number is for this outlet
                    //var salesExecutives = (from o in db.DistributorsOutlets.AsNoTracking()
                    //                       join s in db.RoSalesExecutives.AsNoTracking() on o.UserId equals s.RoUserId
                    //                       where o.OutletId == ws.outletId && s.SeUserId == salesExecUser.UserId
                    // 
                    var seWorkshops = new List<SalesExecutiveWorkshop>
                    {
                        new SalesExecutiveWorkshop
                        {
                            UserId = salesExecUser.UserId,
                            WorkshopId = Convert.ToInt32(ws.WorkShopId)
                        }
                    };

                    ru.SaveWorkshopsForSalesExecutive(seWorkshops, salesExecUser.UserId, true);
                }
            }
        }
        /// <summary>
        /// Map sales data with import new workshops
        /// </summary>
        private static void MapSaleToWorkshop(List<string> workShopCodes, int distributorId)
        {
            using (var context = new garaazEntities())
            {
                var workshops = (from dw in context.DistributorWorkShops
                                 join u in context.UserDetails on dw.UserId equals u.UserId
                                 join c in workShopCodes on u.ConsPartyCode equals c
                                 where dw.DistributorId == distributorId
                                 select new
                                 {
                                     dw.WorkShopId,
                                     dw.UserId,
                                     u.ConsPartyCode
                                 }).AsNoTracking().ToList();

                var sales = context.DailySalesTrackerWithInvoiceDatas.Where(s => s.WorkShopId == null && workShopCodes.Contains(s.ConsPartyCode) && s.DistributorId == distributorId);
                if (sales.Count() == 0) { return; }
                foreach (var ws in workshops)
                {
                    var sale = sales.Where(s => s.ConsPartyCode.Equals(ws.ConsPartyCode, StringComparison.OrdinalIgnoreCase)).ToList();

                    sale.ForEach(a =>
                    {
                        a.WorkShopId = ws.WorkShopId; a.UserId = ws.UserId;
                    });
                    context.SaveChanges();
                }
            }
        }

        #endregion

        #region Import Products

        /// <summary>
        /// Fetch and populate products.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The id of the distributor.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateProducts(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessProducts(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessProducts(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;

            imported = 0;
            skipped = 0;
            var importErrorMsg = new ConcurrentBag<ImportErrorMessage>();

            var recImported = 0;
            var recSkipped = 0;

            try
            {
                // not need to delete now update base on partnum
                // Delete all product for passed distributor
                //SqlParameter[] param = {
                //        new SqlParameter("@Action",4),
                //        new SqlParameter("@DistributorId",distributorId),
                //    };
                //SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportProduct", param);

                // Fetch all product's groups for current distributor and all brands
                var groups = GetDistributorProductGroups(refinedTable, distributorId);
                var brands = GetAllBrands(refinedTable);

                Parallel.ForEach(refinedTable.AsEnumerable(), row =>
                {
                    string partNumber = string.Empty, partDescription = string.Empty;
                    try
                    {
                        partNumber = refinedTable.ColumnValue(row, "Part number");
                        partDescription = refinedTable.ColumnValue(row, "Part Description");
                        if (string.IsNullOrEmpty(partNumber))
                        {
                            throw new Exception("Part number missing");
                        }

                        var partCategoryCode = refinedTable.ColumnValue(row, "Part Category Code");
                        var productType = "";
                        if (partCategoryCode.Contains("M")) { productType = Constants.M; partCategoryCode = Constants.MFull; }
                        if (partCategoryCode.Contains("AA")) { productType = Constants.AA; partCategoryCode = Constants.AAFull; }
                        if (partCategoryCode.Contains("AG")) { productType = Constants.AG; partCategoryCode = Constants.AGFull; }
                        if (partCategoryCode.Contains("T")) { productType = Constants.T; partCategoryCode = Constants.TFull; }

                        var groupName = refinedTable.ColumnValue(row, "Part group");
                        var groupId = groups.FirstOrDefault(g => g.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase))?.GroupId;

                        var brandName = refinedTable.ColumnValue(row, "Brand");
                        var brandId = brands.FirstOrDefault(b => b.BrandName.Equals(brandName, StringComparison.OrdinalIgnoreCase))?.BrandId;

                        // Create or update product
                        var productParams = new[]{
                            new SqlParameter("@Action",3),
                            new SqlParameter("@DistributorId",distributorId),
                            new SqlParameter("@PartNo",partNumber),
                            new SqlParameter("@RootPartNum",refinedTable.ColumnValue(row, "Root part number")),
                            new SqlParameter("@Description", partDescription),
                            new SqlParameter("@PackQuantity",refinedTable.IntColumnValue(row, "PACK_QUANTITY")),
                            new SqlParameter("@TaxValue",refinedTable.ColumnValue(row, "Tax Value")),
                            new SqlParameter("@Price", refinedTable.DecimalColumnValue(row, "MRP")),
                            new SqlParameter("@PartCategoryCode",partCategoryCode),
                            new SqlParameter("@ProductType",productType),
                            new SqlParameter("@GroupId",groupId),
                            new SqlParameter("@BrandId",brandId)
                        };
                        populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportProduct", productParams) > 0;
                        Interlocked.Increment(ref recImported);
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        Interlocked.Increment(ref recSkipped);
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = !string.IsNullOrWhiteSpace(partNumber) ? partNumber : partDescription,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                });

                imported = recImported;
                skipped = recSkipped;

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.ProductImport);
                }

                #region Comment old code
                //using (var db = new garaazEntities())
                //{
                //    foreach (DataRow row in refinedTable.Rows)
                //    {
                //        try
                //        {
                //            var partNumber = refinedTable.ColumnValue(row, "Part number"); // Earlier it was PART_NUM
                //            if (string.IsNullOrEmpty(partNumber)) continue;

                //            var partCategoryCode = refinedTable.ColumnValue(row, "Part Category Code");
                //            var productType = "";
                //            if (partCategoryCode.Contains("M")) { productType = Constants.M; partCategoryCode = Constants.MFull; }
                //            if (partCategoryCode.Contains("AA")) { productType = Constants.AA; partCategoryCode = Constants.AAFull; }
                //            if (partCategoryCode.Contains("AG")) { productType = Constants.AG; partCategoryCode = Constants.AGFull; }
                //            if (partCategoryCode.Contains("T")) { productType = Constants.T; partCategoryCode = Constants.TFull; }

                //            var product = new Product
                //            {
                //                PartNo = partNumber,
                //                RootPartNum = refinedTable.ColumnValue(row, "Root part number"),
                //                Description = refinedTable.ColumnValue(row, "Part Description"), // Earlier it was PART_DESC
                //                PackQuantity = refinedTable.IntColumnValue(row, "PACK_QUANTITY"),
                //                TaxValue = refinedTable.ColumnValue(row, "Tax Value"),
                //                Price = refinedTable.DecimalColumnValue(row, "MRP"),
                //                DistributorId = distributorId,
                //                // part = refinedTable.IntColumnValue(row, "Part Category Description"),
                //                CreatedDate = DateTime.Now,
                //                PartCategoryCode = partCategoryCode,
                //                ProductType = productType

                //                // --- COMMENTED CODE FOR USER PROVIDED NEW FILE WITH LESS COLUMNS (24-09-2019) ---

                //                //BinLocation = refinedTable.ColumnValue(row, "BIN_LOCATION"),
                //                //PurchasePrice = refinedTable.DecimalColumnValue(row, "PURCHASE_PRICE"),
                //                //TaxPaidSellingPrice = refinedTable.DecimalColumnValue(row, "TAX_PAID_SELLING_PRICE"),
                //                //TaxableSellingPrice = refinedTable.DecimalColumnValue(row, "TAXABLE_SELLING_PRICE"),
                //                //CurrentStock = refinedTable.IntColumnValue(row, "CURRENT_STOCK"),
                //                //InventoryValue = refinedTable.DecimalColumnValue(row, "INVENTORY_VALUE"),
                //                //Abc = refinedTable.ColumnValue(row, "ABC"),
                //                //Fms = refinedTable.ColumnValue(row, "FMS"),
                //                //Xyz = refinedTable.ColumnValue(row, "XYZ"),
                //                //MovementCode = refinedTable.ColumnValue(row, "MOVEMENT_CODE"),
                //                //Margin = refinedTable.IntColumnValue(row, "MARGIN"),
                //                //SequenceNo = refinedTable.IntColumnValue(row, "SEQUENCE_NO"),
                //                //IssueIndicator = refinedTable.ColumnValue(row, "ISSUE_INDICATOR"),
                //                //StartDate = refinedTable.DateColumnValue(row, "START_DATE"),
                //                //CloseDate = refinedTable.DateColumnValue(row, "CLOSE_DATE"),
                //                //ModelsApplicable = refinedTable.ColumnValue(row, "MODELS_APPLICABLE"),
                //                //SalesTaxCategory = refinedTable.ColumnValue(row, "SALES_TAX_CATEGORY"),
                //                //TaxDesc = refinedTable.ColumnValue(row, "TAX_DESC"),
                //                //OnOrderQtyMul = refinedTable.IntColumnValue(row, "ON_ORDER_QTY_MUL"),
                //                //InTransitQty = refinedTable.IntColumnValue(row, "IN_TRANSIT_QTY"),
                //                //AllocQty = refinedTable.IntColumnValue(row, "ALLOC_QTY"),
                //                //FloatStock = refinedTable.IntColumnValue(row, "FLOAT_STOCK"),
                //                //MinimumLevel = refinedTable.ColumnValue(row, "MINIMUM_LEVEL"),
                //                //MaximumLevel = refinedTable.ColumnValue(row, "MAXIMUM_LEVEL"),
                //                //ReorderLevel = refinedTable.ColumnValue(row, "REORDER_LEVEL"),
                //                //Last12MonthAvgConsumption = refinedTable.IntColumnValue(row, "LAST_12_MONTH_AVG_CONSUMPTION"),
                //                //ReservationQty = refinedTable.ColumnValue(row, "RESERVATION_QTY"),
                //                //SafetyStock = refinedTable.ColumnValue(row, "SAFETY_STOCK"),
                //                //SeasonalPartYn = refinedTable.ColumnValue(row, "SEASONAL_PART_YN"),
                //                //DeadStockYn = refinedTable.ColumnValue(row, "DEAD_STOCK_YN"),
                //                //ReasonToEditInPo = refinedTable.ColumnValue(row, "REASON_TO_EDIT_IN_PO"),
                //                //VorPartYn = refinedTable.ColumnValue(row, "VOR_PART_YN"),
                //                //HsCode = refinedTable.IntColumnValue(row, "HS_CODE"),
                //                //QuarantineQty = refinedTable.ColumnValue(row, "quarantine_qty")

                //            };

                //            // Find GroupId based on GroupName. And create parent group if not found
                //            var parentGroup = refinedTable.ColumnValue(row, "Part group");
                //            if (!string.IsNullOrEmpty(parentGroup))
                //            {
                //                var pGroup = db.ProductGroups.FirstOrDefault(g => g.GroupName.Equals(parentGroup, StringComparison.OrdinalIgnoreCase));
                //                if (pGroup != null)
                //                {
                //                    product.GroupId = pGroup.GroupId;
                //                }
                //                else
                //                {
                //                    var prodGroup = new ProductGroup { GroupName = parentGroup, CreatedDate = DateTime.Now, DistributorId = product.DistributorId };
                //                    db.ProductGroups.Add(prodGroup);
                //                    db.SaveChanges();

                //                    product.GroupId = prodGroup.GroupId;
                //                }
                //            }

                //            // Find GroupId based on GroupName. And create group if not found
                //            /*var groupName = refinedTable.ColumnValue(row, "Part Category Description"); // Earlier it was PART_CATEG
                //            if (!string.IsNullOrEmpty(groupName))
                //            {
                //                var pGroup = db.ProductGroups.FirstOrDefault(g => g.GroupName.ToLower() == groupName.ToLower());
                //                if (pGroup != null)
                //                {
                //                    product.GroupId = pGroup.GroupId;
                //                }
                //                else
                //                {
                //                    //groupName = refinedTable.ColumnValue(row, "Part Category Code"); // Earlier it was PART_CATEG
                //                    pGroup = db.ProductGroups.FirstOrDefault(g => g.GroupName.ToLower() == groupName.ToLower());
                //                    if (pGroup != null)
                //                    {
                //                        product.GroupId = pGroup.GroupId;
                //                    }
                //                    else
                //                    {
                //                        var prodGroup = new ProductGroup { GroupName = groupName, CreatedDate = DateTime.Now, DistributorId = product.DistributorId, ParentId = parentId };
                //                        db.ProductGroups.Add(prodGroup);
                //                        db.SaveChanges();

                //                        product.GroupId = prodGroup.GroupId;
                //                    }
                //                }
                //            }*/

                //            // Create brand based on row's column value
                //            var brandName = refinedTable.ColumnValue(row, "Brand");
                //            if (!string.IsNullOrEmpty(brandName))
                //            {
                //                var brand = db.Brands.FirstOrDefault(x => string.Equals(x.Name, brandName, StringComparison.CurrentCultureIgnoreCase));
                //                if (brand != null)
                //                {
                //                    product.BrandId = brand.BrandId;
                //                }
                //                else
                //                {
                //                    var data = new Brand
                //                    {
                //                        Name = brandName.Trim(),
                //                        CreatedDate = DateTime.Now
                //                    };
                //                    db.Brands.Add(data);
                //                    db.SaveChanges();
                //                    product.BrandId = data.BrandId;
                //                }
                //            }

                //            // Check if product already exists
                //            var existingProduct = db.Products.FirstOrDefault(p => p.PartNo == product.PartNo && p.DistributorId == distributorId);
                //            if (existingProduct != null)
                //            {
                //                existingProduct.RootPartNum = product.RootPartNum;
                //                existingProduct.Description = product.Description;
                //                existingProduct.GroupId = product.GroupId;
                //                existingProduct.PackQuantity = product.PackQuantity;
                //                existingProduct.TaxValue = product.TaxValue;
                //                existingProduct.Price = product.Price;
                //                existingProduct.BrandId = product.BrandId;

                //                //existingProduct.BinLocation = product.BinLocation;
                //                //existingProduct.PurchasePrice = product.PurchasePrice;
                //                //existingProduct.TaxPaidSellingPrice = product.TaxPaidSellingPrice;
                //                //existingProduct.TaxableSellingPrice = product.TaxableSellingPrice;
                //                //existingProduct.CurrentStock = product.CurrentStock;
                //                //existingProduct.InventoryValue = product.InventoryValue;
                //                //existingProduct.Abc = product.Abc;
                //                //existingProduct.Fms = product.Fms;
                //                //existingProduct.Xyz = product.Xyz;
                //                //existingProduct.MovementCode = product.MovementCode;                        
                //                //existingProduct.Margin = product.Margin;
                //                //existingProduct.SequenceNo = product.SequenceNo;
                //                //existingProduct.IssueIndicator = product.IssueIndicator;
                //                //existingProduct.StartDate = product.StartDate;
                //                //existingProduct.CloseDate = product.CloseDate;
                //                //existingProduct.ModelsApplicable = product.ModelsApplicable;
                //                //existingProduct.SalesTaxCategory = product.SalesTaxCategory;
                //                //existingProduct.TaxDesc = product.TaxDesc;
                //                //existingProduct.OnOrderQtyMul = product.OnOrderQtyMul;
                //                //existingProduct.InTransitQty = product.InTransitQty;
                //                //existingProduct.AllocQty = product.AllocQty;
                //                //existingProduct.FloatStock = product.FloatStock;
                //                //existingProduct.MinimumLevel = product.MinimumLevel;
                //                //existingProduct.MaximumLevel = product.MaximumLevel;
                //                //existingProduct.ReorderLevel = product.ReorderLevel;
                //                //existingProduct.Last12MonthAvgConsumption = product.Last12MonthAvgConsumption;
                //                //existingProduct.ReservationQty = product.ReservationQty;
                //                //existingProduct.SafetyStock = product.SafetyStock;
                //                //existingProduct.SeasonalPartYn = product.SeasonalPartYn;
                //                //existingProduct.DeadStockYn = product.DeadStockYn;
                //                //existingProduct.ReasonToEditInPo = product.ReasonToEditInPo;
                //                //existingProduct.VorPartYn = product.VorPartYn;
                //                //existingProduct.HsCode = product.HsCode;
                //                //existingProduct.QuarantineQty = product.QuarantineQty;                        

                //                db.SaveChanges();
                //                populated = true;
                //            }
                //            else
                //            {
                //                db.Products.Add(product);
                //                populated = db.SaveChanges() > 0;
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            RepoUserLogs.LogException(e);
                //        }
                //    }
                //}
                #endregion                
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg.ToList());
        }

        /// <summary>
        /// Get distributor product groups if matches in table else create new.
        /// </summary>
        private static ConcurrentBag<ImportGroup> GetDistributorProductGroups(DataTable refinedTable, int distributorId)
        {
            var sheetGroupsName = refinedTable.AsEnumerable().Select(c => c.Field<string>("Part group")).Distinct().Where(b => !string.IsNullOrWhiteSpace(b)).Select(b => b);

            ConcurrentBag<ImportGroup> groups;
            using (var db = new garaazEntities())
            {
                try
                {
                    // Delete all groups who not in the sheet.
                    var unUsedGroups = db.ProductGroups.Where(g => !sheetGroupsName.Contains(g.GroupName) && g.DistributorId == distributorId).ToList();
                    if (unUsedGroups.Count > 0)
                    {
                        db.ProductGroups.RemoveRange(unUsedGroups);
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // not show any error because it's dependencies error
                }
                // Query without tracking for performance
                var prodGroups = db.ProductGroups.AsNoTracking().Where(g => g.DistributorId == distributorId).Select(g =>
                    new ImportGroup
                    {
                        GroupId = g.GroupId,
                        GroupName = g.GroupName
                    });
                groups = new ConcurrentBag<ImportGroup>(prodGroups);
            }

            // Create all prod groups if not exists and store locally
            var tableGroups = refinedTable.AsEnumerable().Select(c => c.Field<string>("Part group")).Distinct().Where(b => !string.IsNullOrWhiteSpace(b) && !groups.Select(x => x.GroupName).Contains(b));
            Parallel.ForEach(tableGroups, groupName =>
            {
                var groupParams = new[]
                {
                    new SqlParameter("@Action", 1),
                    new SqlParameter("@DistributorId", distributorId),
                    new SqlParameter("@GroupName", groupName),
                    new SqlParameter("RetVal", SqlDbType.Int) {Direction = ParameterDirection.ReturnValue}
                };
                SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure,
                    "usp_ImportProduct", groupParams);

                var groupId = (int)groupParams[3].Value;
                groups.Add(new ImportGroup { GroupId = groupId, GroupName = groupName });
            });

            return groups;
        }

        /// <summary>
        /// Get all brands if matches in table else create new.
        /// </summary>
        /// <param name="refinedTable"></param>
        /// <returns></returns>
        private static ConcurrentBag<ImportBrand> GetAllBrands(DataTable refinedTable)
        {
            ConcurrentBag<ImportBrand> brands;
            using (var db = new garaazEntities())
            {
                // Query without tracking for performance
                var prodBrands = db.Brands.AsNoTracking().Select(b => new ImportBrand
                {
                    BrandId = b.BrandId,
                    BrandName = b.Name
                });
                brands = new ConcurrentBag<ImportBrand>(prodBrands);
            }

            var tableBrands = refinedTable.AsEnumerable().Select(c => c.Field<string>("Brand")).Distinct().Where(b => !string.IsNullOrWhiteSpace(b) && !brands.Select(x => x.BrandName).Contains(b));
            Parallel.ForEach(tableBrands, brandName =>
            {
                var brandParams = new[]
                {
                    new SqlParameter("@Action", 2),
                    new SqlParameter("@BrandName", brandName),
                    new SqlParameter("RetVal", SqlDbType.Int) {Direction = ParameterDirection.ReturnValue}
                };

                SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure,
                    "usp_ImportProduct", brandParams);

                var brandId = (int)brandParams[3].Value;
                brands.Add(new ImportBrand { BrandId = brandId, BrandName = brandName });
            });

            return brands;
        }

        #endregion

        #region Import Daily Stock

        /// <summary>
        /// Fetch and populate daily stocks.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The id of the distributor.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateDailyStock(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessDailyStocks(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessDailyStocks(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var rowCount = 1;
            var importErrorMsg = new List<ImportErrorMessage>();

            try
            {
                SqlParameter[] param = {
                        new SqlParameter("@Action",1),
                        new SqlParameter("@distributorId",distributorId),
                    };
                populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportDailyStock_Temp", param) > 0;

                foreach (DataRow row in refinedTable.Rows)
                {
                    var partNumber = string.Empty;
                    var stockQty = string.Empty;
                    int? partQty = null;
                    try
                    {
                        rowCount++;
                        stockQty = refinedTable.ColumnValue(row, "Stock Quantity");
                        if (string.IsNullOrWhiteSpace(stockQty))
                        {
                            throw new Exception($"Stock Quantity missing on row {rowCount}");
                        }
                        // convert decimal to int string stockQty 
                        stockQty = stockQty.Trim().Contains('.') ? stockQty.Trim().Split('.')[0] : stockQty.Trim();

                        try
                        {
                            partQty = Convert.ToInt32(stockQty);
                        }
                        catch (Exception ex)
                        {
                            populated = false;
                            skipped++;
                            importErrorMsg.Add(new ImportErrorMessage
                            {
                                UniqueId = partNumber,
                                ErrorMsg = $"Stock Quantity is not a number on row {rowCount}"
                            });
                            continue;
                        }

                        partNumber = refinedTable.ColumnValue(row, "Part Num");
                        if (string.IsNullOrWhiteSpace(partNumber))
                        {
                            throw new Exception($"Part number missing on row {rowCount}");
                        }

                        var locationCode = refinedTable.ColumnValue(row, "Location Code");
                        if (string.IsNullOrWhiteSpace(locationCode))
                        {
                            throw new Exception($"Location code missing on row {rowCount}");
                        }
                        var rootPartNum = refinedTable.ColumnValue(row, "Root Part Num");
                        if (string.IsNullOrWhiteSpace(rootPartNum))
                        {
                            throw new Exception($"Root Part Num missing on row {rowCount}");
                        }

                        SqlParameter[] parameter =
                        {
                            new SqlParameter("@Action", 3),
                            new SqlParameter("@distributorId", distributorId),
                            new SqlParameter("@LocationCode", locationCode),
                            new SqlParameter("@PartNum", partNumber),
                            new SqlParameter("@partQty",partQty),
                            new SqlParameter("@RootPartNum",rootPartNum)
                        };
                        populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure,
                                        "usp_ImportDailyStock_Temp", parameter) > 0;
                        imported++;
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = partNumber,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }
                populated = imported > 0;
                if (populated)
                {
                    // Update quantity of product for particular distributor
                    SqlParameter[] param1 = {
                            new SqlParameter("@Action",2),
                            new SqlParameter("@distributorId",distributorId),
                        };
                    SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportDailyStock_Temp", param1);

                    SetFileUploadDate(distributorId, Constants.DailyStockImport);
                }
                //}
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }
            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }
        #endregion

        #region Import Daily Sales

        /// <summary>
        /// Read data from excel sheet and populate to database.
        /// </summary>
        /// <param name="path">The full path to uploaded excel file.</param>
        /// <param name="distributorId">The selected distributor Id.</param>
        /// <param name="dataUploadMsg">Message holding information about between which dates data was uploaded.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateDailySales(string path, int distributorId, out int totalRecords, out int imported, out int skipped, out string dataUploadMsg)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessDailySales(distributorId, refinedTable, out imported, out skipped, out dataUploadMsg);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessDailySales(int distributorId, DataTable refinedTable, out int imported, out int skipped, out string dataUploadMsg)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var importErrorMessages = new ConcurrentBag<ImportErrorMessage>();

            var recImported = 0;
            var recSkipped = 0;

            dataUploadMsg = string.Empty;
            var dates = new ConcurrentBag<DateTime>();

            // get distributor walkInCustomer workshops
            var db = new garaazEntities();

            var walkInCustomers = (from d in db.DistributorWorkShops.AsNoTracking()
                                   join u in db.UserDetails.AsNoTracking() on d.UserId equals u.UserId
                                   join w in db.WorkShops.Include(w => w.Outlet).AsNoTracking() on d.WorkShopId equals w.WorkShopId
                                   where d.DistributorId == distributorId
                                   select new
                                   {
                                       OutletCode = w.Outlet.OutletCode,
                                       w.WorkShopId,
                                       u.ConsPartyCode
                                   }).ToList();

            // For debugging, use MaxDegreeOfParallelism as 1 (It become similar to simple foreach)
            var options = new ParallelOptions { MaxDegreeOfParallelism = 1 };
            Parallel.ForEach(refinedTable.AsEnumerable(), options, (row, state, index) =>
              {
                  try
                  {
                      var locationCode = refinedTable.ColumnValue(row, "Loc Code");
                      var consPartyCode = refinedTable.ColumnValue(row, "Cons Party Code");
                      if (string.IsNullOrEmpty(locationCode) && string.IsNullOrEmpty(consPartyCode))
                      {
                          throw new Exception($"Location code and Cons Party Code missing on row {index + 1}");
                      }

                      // Create date using existing data in file
                      var day = refinedTable.ColumnValue(row, "Day");
                      if (!int.TryParse(day, out var iDay))
                      {
                          throw new Exception("Day is not a number");
                      }
                      if (iDay > 31)
                      {
                          throw new Exception("Day cannot be greater than 31");
                      }
                      var calMonthYear = refinedTable.ColumnValue(row, "Cal Month Year");
                      DateTime createdDate;
                      if (!string.IsNullOrEmpty(calMonthYear) && DateTime.TryParse(calMonthYear, out var date))
                      {
                          createdDate = new DateTime(date.Year, date.Month, iDay);
                          dates.Add(createdDate);
                      }
                      else
                      {
                          throw new Exception("'Cal Month Year' data is in wrong format. It should be similar to format Apr 2020.");
                      }
                      var documentNum = refinedTable.ColumnValue(row, "Document Num");
                      //if (string.IsNullOrEmpty(documentNum))
                      //{
                      //    throw new Exception($"DocumentNum missing on row {index + 1}");
                      //}

                      // find consPartyCode for customer WalkInCustomer
                      var partNum = refinedTable.ColumnValue(row, "Part Num");
                      var customerType = refinedTable.ColumnValue(row, "Cons Party Type Desc");
                      if (string.IsNullOrEmpty(consPartyCode) && customerType.Equals(CustomerType.WalkInCustomer, StringComparison.OrdinalIgnoreCase))
                      {
                          consPartyCode = walkInCustomers.FirstOrDefault(w => w.OutletCode.Equals(locationCode, StringComparison.OrdinalIgnoreCase))?.ConsPartyCode;
                      }

                      decimal retailQty = 0;
                      decimal returnQty = 0;
                      decimal netRetailQty = 0;
                      decimal retailSelling = 0;
                      decimal returnSelling = 0;
                      decimal netRetailSelling = 0;
                      decimal discountAmount = 0;
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Retail Qty"), out var iRetailQty))
                      {
                          retailQty = iRetailQty;
                      }
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Return Qty"), out var iReturnQty))
                      {
                          returnQty = iReturnQty;
                      }
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Net Retail Qty"), out var iNetRetailQty))
                      {
                          netRetailQty = iNetRetailQty;
                      }
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Retail Selling"), out var iRetailSelling))
                      {
                          retailSelling = iRetailSelling;
                      }
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Return Selling"), out var iReturnSelling))
                      {
                          returnSelling = iReturnSelling;
                      }
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Net Retail Selling"), out var iNetRetailSelling))
                      {
                          netRetailSelling = iNetRetailSelling;
                      }
                      if (decimal.TryParse(refinedTable.ColumnValue(row, "Discount Amount"), out var iDiscountAmount))
                      {
                          discountAmount = iDiscountAmount;
                      }

                      var responseParam = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };

                      // Save daily sale (one at a time)
                      SqlParameter[] saleParameter = {
                        new SqlParameter("@Action",1),
                        new SqlParameter("@DistributorId",distributorId),
                        new SqlParameter("@LocCode",locationCode),
                        new SqlParameter("@LocDesc",refinedTable.ColumnValue(row, "Loc Desc")),
                        new SqlParameter("@PartNum",partNum),
                        new SqlParameter("@PartDesc",refinedTable.ColumnValue(row, "Part Desc")),
                        new SqlParameter("@RootPartNum", refinedTable.ColumnValue(row, "Root Part Num")),
                        new SqlParameter("@PartCategory",refinedTable.ColumnValue(row, "Part Category")),
                        new SqlParameter("@Day",day),
                        new SqlParameter("@PartGroup",refinedTable.ColumnValue(row, "Part Group")),
                        new SqlParameter("@CalMonthYear",calMonthYear),
                        new SqlParameter("@ConsPartyCode",consPartyCode),
                        new SqlParameter("@ConsPartyName",refinedTable.ColumnValue(row, "Cons Party Name")),
                        new SqlParameter("@ConsPartyTypeDesc",customerType),
                        new SqlParameter("@DocumentNum",documentNum),
                        new SqlParameter("@Remarks",refinedTable.ColumnValue(row, "Remarks")),
                        new SqlParameter("@RetailQty",retailQty),
                        new SqlParameter("@ReturnQty",returnQty),
                        new SqlParameter("@NetRetailQty",netRetailQty),
                        new SqlParameter("@RetailSelling",retailSelling),
                        new SqlParameter("@ReturnSelling",returnSelling),
                        new SqlParameter("@NetRetailSelling",netRetailSelling),
                        new SqlParameter("@DiscountAmount",discountAmount),
                        new SqlParameter("@CoNo",refinedTable.ColumnValue(row, "Document Num")),
                        new SqlParameter("@CreatedDate",createdDate),
                        responseParam
                   };

                      SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportDailySales", saleParameter);
                      var status = Convert.ToBoolean(responseParam.Value);
                      if (!status)
                      {
                          throw new Exception($"Record of row {index + 1} already exist.");
                      }
                      populated = true;
                      Interlocked.Increment(ref recImported);
                  }
                  catch (Exception exc)
                  {
                      populated = false;
                      Interlocked.Increment(ref recSkipped);

                      RepoUserLogs.LogException(exc);
                      importErrorMessages.Add(new ImportErrorMessage
                      {
                          UniqueId = $"Row {index + 1}",
                          ErrorMsg = exc.Message
                      });
                  }
              });

            imported = recImported;
            skipped = recSkipped;

            var updateResponseParam = new SqlParameter("@ResponseCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
            // Now update ProductId, GroupId, UserId, WorkshopId in DailySales
            SqlParameter[] saleUpdateParameter = {
                        new SqlParameter("@Action",2),
                        new SqlParameter("@DistributorId",distributorId),
                        updateResponseParam
                };
            // Run process in background
            var appThread = new System.Threading.Thread(() => SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportDailySales", saleUpdateParameter)) { IsBackground = true };
            appThread.Start();

            if (imported > 0)
            {
                SetFileUploadDate(distributorId, Constants.DailySalesTrackerImport);
            }

            if (dates.Count > 0)
            {
                var minDate = dates.Min(d => d).ToString("dd MMM yyyy");
                var maxDate = dates.Max(d => d).ToString("dd MMM yyyy");
                dataUploadMsg = $" from {minDate} - {maxDate}";
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMessages.ToList());

            #region Old Code - Using EF
            //using (var db = new garaazEntities())
            //{
            //var userDetails = db.UserDetails.ToList();
            //var salesData = db.DailySalesTrackerWithInvoiceDatas;

            //foreach (DataRow row in refinedTable.Rows)
            //{
            //    try
            //    {
            //        var locationCode = refinedTable.ColumnValue(row, "Loc Code");
            //        if (string.IsNullOrEmpty(locationCode)) continue;

            //        string userId = null; int? workshopId = null; int? groupId = null; int? productId = null;
            //        var consPartyCode = refinedTable.ColumnValue(row, "Cons Party Code");
            //        if (!string.IsNullOrEmpty(consPartyCode))
            //        {
            //            var userDetail = userDetails.FirstOrDefault(x => x.ConsPartyCode == consPartyCode);
            //            userId = userDetail?.UserId;

            //            if (!string.IsNullOrEmpty(userId))
            //            {
            //                var workshop = db.WorkshopsUsers.FirstOrDefault(x => x.UserId == userId);
            //                workshopId = workshop?.WorkshopId;

            //                if (workshopId == null || workshopId == 0)
            //                {
            //                    var dw = db.DistributorWorkShops.FirstOrDefault(x => x.UserId == userId);
            //                    workshopId = dw?.WorkShopId;
            //                }
            //            }
            //        }

            //        var partNumber = refinedTable.ColumnValue(row, "Part Num");
            //        if (!string.IsNullOrEmpty(partNumber))
            //        {
            //            var product = db.Products.FirstOrDefault(x => x.PartNo == partNumber);
            //            if (product != null)
            //            {
            //                productId = product.ProductId;
            //                groupId = product.GroupId;
            //            }
            //        }

            //        // Create date using existing data in file
            //        var day = refinedTable.ColumnValue(row, "Day");
            //        var calMonthYear = refinedTable.ColumnValue(row, "Cal Month Year");
            //        var createdDate = DateTime.Now;
            //        try
            //        {
            //            if (!string.IsNullOrEmpty(day) && !string.IsNullOrEmpty(calMonthYear) && DateTime.TryParse(calMonthYear, out DateTime date))
            //            {
            //                createdDate = new DateTime(date.Year, date.Month, Convert.ToInt32(day));
            //            }
            //        }
            //        catch (Exception)
            //        {
            //            // User might provide wrong date or values
            //            // So handle exception silently                                
            //        }

            //        var dailySale = new DailySalesTrackerWithInvoiceData
            //        {
            //            LocCode = locationCode,
            //            LocDesc = refinedTable.ColumnValue(row, "Loc Desc"),
            //            PartNum = refinedTable.ColumnValue(row, "Part Num"),
            //            PartDesc = refinedTable.ColumnValue(row, "Part Desc"),
            //            RootPartNum = refinedTable.ColumnValue(row, "Root Part Num"),
            //            PartCategory = refinedTable.ColumnValue(row, "Part Category"),
            //            Day = day,
            //            PartGroup = refinedTable.ColumnValue(row, "Part Group"),
            //            CalMonthYear = calMonthYear,
            //            ConsPartyCode = refinedTable.ColumnValue(row, "Cons Party Code"),
            //            ConsPartyName = refinedTable.ColumnValue(row, "Cons Party Name"),
            //            ConsPartyTypeDesc = refinedTable.ColumnValue(row, "Cons Party Type Desc"),
            //            DocumentNum = refinedTable.ColumnValue(row, "Document Num"),
            //            Remarks = refinedTable.ColumnValue(row, "Remarks"),
            //            RetailQty = refinedTable.ColumnValue(row, "Retail Qty"),
            //            ReturnQty = refinedTable.ColumnValue(row, "Return Qty"),
            //            NetRetailQty = refinedTable.ColumnValue(row, "Net Retail Qty"),
            //            RetailSelling = refinedTable.ColumnValue(row, "Retail Selling"),
            //            ReturnSelling = refinedTable.ColumnValue(row, "Return Selling"),
            //            NetRetailSelling = refinedTable.ColumnValue(row, "Net Retail Selling"),
            //            DiscountAmount = refinedTable.ColumnValue(row, "Discount Amount"),

            //            // Remove existing columns from sheet
            //            //Region = region,
            //            //DealerCode = refinedTable.ColumnValue(row, "Dealer Code"),
            //            CoNo = refinedTable.ColumnValue(row, "Document Num"),
            //            DistributorId = distributorId,
            //            UserId = userId,
            //            WorkShopId = workshopId,
            //            GroupId = groupId,
            //            ProductId = productId,
            //            CreatedDate = createdDate
            //        };

            //        var existingDs = salesData.FirstOrDefault(d => d.LocCode == dailySale.LocCode && d.ConsPartyCode == dailySale.ConsPartyCode && d.DocumentNum == dailySale.DocumentNum && d.PartNum == dailySale.PartNum && d.Day == dailySale.Day && d.CalMonthYear == dailySale.CalMonthYear);

            //        if (existingDs != null)
            //        {
            //            //existingDs.Region = dailySale.Region;
            //            //existingDs.DealerCode = dailySale.DealerCode;
            //            existingDs.LocCode = dailySale.LocCode;
            //            existingDs.LocDesc = dailySale.LocDesc;
            //            //existingDs.PartNum = dailySale.PartNum;
            //            existingDs.PartDesc = dailySale.PartDesc;
            //            existingDs.RootPartNum = dailySale.RootPartNum;
            //            existingDs.PartCategory = dailySale.PartCategory;
            //            //existingDs.Day = day;
            //            existingDs.PartGroup = dailySale.PartGroup;
            //            //existingDs.CalMonthYear = calMonthYear;
            //            //existingDs.ConsPartyCode = dailySale.ConsPartyCode;
            //            existingDs.ConsPartyName = dailySale.ConsPartyName;
            //            existingDs.ConsPartyTypeDesc = dailySale.ConsPartyTypeDesc;
            //            existingDs.DocumentNum = dailySale.DocumentNum;
            //            existingDs.Remarks = dailySale.Remarks;
            //            existingDs.RetailQty = dailySale.RetailQty;
            //            existingDs.ReturnQty = dailySale.ReturnQty;
            //            existingDs.NetRetailQty = dailySale.NetRetailQty;
            //            existingDs.RetailSelling = dailySale.RetailSelling;
            //            existingDs.NetRetailSelling = dailySale.NetRetailSelling;
            //            existingDs.DiscountAmount = dailySale.DiscountAmount;
            //            existingDs.CoNo = dailySale.DocumentNum;
            //            existingDs.DistributorId = distributorId;
            //            existingDs.UserId = userId;
            //            existingDs.WorkShopId = workshopId;
            //            existingDs.GroupId = groupId;
            //            existingDs.ProductId = productId;
            //            existingDs.CreatedDate = createdDate;

            //            db.SaveChanges();
            //            populated = true;
            //        }
            //        else
            //        {
            //            db.DailySalesTrackerWithInvoiceDatas.Add(dailySale);
            //            populated = db.SaveChanges() > 0;
            //        }
            //    }
            //    catch (Exception exc)
            //    {
            //        RepoUserLogs.LogException(exc);
            //    }
            //}
            //}
            #endregion
        }

        #endregion

        #region Import Customer BackOrder

        /// <summary>
        /// Fetch and populate CustomerBackOrder.
        /// </summary>
        /// <param name="path">The full path to uploaded excel file.</param>
        /// <param name="distributorId">The id of the distributor.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateCustomerBackOrder(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessCustomerBackOrders(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessCustomerBackOrders(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var rowCount = 1;
            var importErrorMsg = new List<ImportErrorMessage>();

            const string colCoNumber = "CO Number";
            if (!refinedTable.Columns.Contains(colCoNumber))
            {
                throw new Exception($"Column '{colCoNumber}' is missing in file.");
            }
            var isAnyLocCodeMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<string>("Loc Code") == null || r.Field<string>("Loc Code") == string.Empty).ToList();

            if (isAnyLocCodeMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyLocCodeMissingRows.Count()} rows in sheet is missing Loc Code");
            }

            var isAnyCoNumberMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<string>(colCoNumber) == null || r.Field<string>(colCoNumber) == string.Empty).ToList();

            if (isAnyCoNumberMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyCoNumberMissingRows.Count()} rows in sheet is missing {colCoNumber}");
            }

            var isAnyPartyCodeMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<string>("Party Code") == null || r.Field<string>("Party Code") == string.Empty).ToList();

            if (isAnyPartyCodeMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyPartyCodeMissingRows.Count()} rows in sheet is missing Party Code");
            }

            var isAnyPartNumMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<string>("Part Number") == null || r.Field<string>("Part Number") == string.Empty).ToList();

            if (isAnyPartNumMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyPartNumMissingRows.Count()} rows in sheet is missing Part Number");
            }

            var isAnyCoDateMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<string>("CO Date") == null || r.Field<string>("CO Date") == string.Empty).ToList();

            if (isAnyCoDateMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyCoDateMissingRows.Count()} rows in sheet is missing CO Date");
            }

            try
            {
                // First delete all backOrder of this distributor then import file
                // and maintain old part-status
                // add new requiredment by client on 08/05/2020
                garaazEntities db = new garaazEntities();
                var distributorBackOrders = db.CustomerBackOrders.Where(b => b.DistributorId == distributorId).ToList();
                if (distributorBackOrders.Count > 0)
                {
                    db.CustomerBackOrders.RemoveRange(distributorBackOrders);
                    db.SaveChanges();
                }

                foreach (DataRow row in refinedTable.Rows)
                {
                    rowCount++;
                    var locationCode = string.Empty;
                    try
                    {
                        locationCode = refinedTable.ColumnValue(row, "Loc Code");

                        var coNumber = refinedTable.ColumnValue(row, colCoNumber);

                        var partyCode = refinedTable.ColumnValue(row, "Party Code");

                        var partNum = refinedTable.ColumnValue(row, "Part Number");

                        var coDate = refinedTable.DateColumnValue(row, "CO Date");

                        // Get old status
                        var partStatus = distributorBackOrders.Where(o => o.PartyCode == partyCode && o.PartNum == partNum && o.CONo == coNumber).FirstOrDefault()?.PartStatus;

                        var cbOrder = new CustomerBackOrder
                        {
                            CODate = coDate,
                            LocCode = locationCode,
                            PartyCode = partyCode,
                            PartyName = refinedTable.ColumnValue(row, "Party Name"),
                            PartNum = partNum,
                            PartDesc = refinedTable.ColumnValue(row, "Part Desc"),
                            Order = refinedTable.ColumnValue(row, "Order"),
                            CBO = refinedTable.ColumnValue(row, "CBO"),
                            StkMW = refinedTable.ColumnValue(row, "StkMW"),
                            ETA = refinedTable.ColumnValue(row, "ETA"),
                            Inv = refinedTable.ColumnValue(row, "Inv"),
                            Pick = refinedTable.ColumnValue(row, "Pick"),
                            Alloc = refinedTable.ColumnValue(row, "Alloc"),
                            BO = refinedTable.ColumnValue(row, "BO"),
                            AO = refinedTable.ColumnValue(row, "AO"),
                            Action = refinedTable.ColumnValue(row, "Action"),
                            Remark = refinedTable.ColumnValue(row, "Remark"),
                            PD = refinedTable.ColumnValue(row, "PD"),
                            DistributorId = distributorId,
                            PartStatus = partStatus
                        };

                        SqlParameter[] cboParameter = {
                            new SqlParameter("@Action",1),
                            new SqlParameter("@distributorId",distributorId),
                            new SqlParameter("@OrderNumber",coNumber),
                            new SqlParameter("@CODate",cbOrder.CODate),
                            new SqlParameter("@LocCode",cbOrder.LocCode),
                            new SqlParameter("@PartyCode",cbOrder.PartyCode),
                            new SqlParameter("@PartyName",cbOrder.PartyName),
                            new SqlParameter("@PartNum", cbOrder.PartNum),
                            new SqlParameter("@PartDesc",cbOrder.PartDesc),
                            new SqlParameter("@Order",cbOrder.Order),
                            new SqlParameter("@CBO",cbOrder.CBO),
                            new SqlParameter("@StkMW",cbOrder.StkMW),
                            new SqlParameter("@ETA", cbOrder.ETA),
                            new SqlParameter("@Inv",cbOrder.Inv),
                            new SqlParameter("@Pick",cbOrder.Pick),
                            new SqlParameter("@Alloc",cbOrder.Alloc),
                            new SqlParameter("@BO",cbOrder.BO),
                            new SqlParameter("@AO",cbOrder.AO),
                            new SqlParameter("@ActionStatus",cbOrder.Action),
                            new SqlParameter("@Remark",cbOrder.Remark),
                            new SqlParameter("@PD",cbOrder.PD),
                            new SqlParameter("@PartStatus",cbOrder.PartStatus)
                        };
                        populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportCBOrders", cboParameter) > 0;
                        imported++;
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = locationCode,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }

                SqlParameter[] cboUpdateParameter = {
                        new SqlParameter("@Action",2),
                        new SqlParameter("@distributorId",distributorId),
                };
                populated = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportCBOrders", cboUpdateParameter) > 0;

                #region Comment old code
                //using (var db = new garaazEntities())
                //{
                //    var userDetails = db.UserDetails.ToList();
                //    var dbCbos = db.CustomerBackOrders.ToList();

                //    foreach (DataRow row in refinedTable.Rows)
                //    {
                //        try
                //        {
                //            // change name coNumber to locationcode for check condition
                //            //var coNumber = refinedTable.ColumnValue(row, "CO No");
                //            var locationCode = refinedTable.ColumnValue(row, "Loc Code");
                //            if (string.IsNullOrEmpty(locationCode)) continue;

                //            var cbOrder = new CustomerBackOrder
                //            {
                //                CODate = refinedTable.DateColumnValue(row, "CO Date"),
                //                LocCode = locationCode,
                //                PartyCode = refinedTable.ColumnValue(row, "Party Code"),
                //                PartyName = refinedTable.ColumnValue(row, "Party Name"),
                //                PartNum = refinedTable.ColumnValue(row, "Part Number"),
                //                PartDesc = refinedTable.ColumnValue(row, "Part Desc"),
                //                Order = refinedTable.ColumnValue(row, "Order"),
                //                CBO = refinedTable.ColumnValue(row, "CBO"),
                //                StkMW = refinedTable.ColumnValue(row, "StkMW"),
                //                ETA = refinedTable.ColumnValue(row, "ETA"),
                //                Inv = refinedTable.ColumnValue(row, "Inv"),
                //                Pick = refinedTable.ColumnValue(row, "Pick"),
                //                Alloc = refinedTable.ColumnValue(row, "Alloc"),
                //                BO = refinedTable.ColumnValue(row, "BO"),
                //                AO = refinedTable.ColumnValue(row, "AO"),
                //                Action = refinedTable.ColumnValue(row, "Action"),
                //                Remark = refinedTable.ColumnValue(row, "Remark"),
                //                PD = refinedTable.ColumnValue(row, "PD"),

                //                // Remove old columns from sheet and add new columns as per new sheet format


                //                //CONo = coNumber,
                //                //PartyType = refinedTable.ColumnValue(row, "Party Type"),
                //                //CustomerOrderType = refinedTable.ColumnValue(row, "Customer Order Type"),
                //                //OrderType = refinedTable.ColumnValue(row, "Order Type"),
                //                //PartStatus = refinedTable.ColumnValue(row, "Part Status"),
                //                //BinLoc = refinedTable.ColumnValue(row, "Bin Loc"),
                //                //OrderedQty = refinedTable.ColumnValue(row, "Ordered Qty"),
                //                //ProcessedQty = refinedTable.ColumnValue(row, "Processed Qty"),
                //                //PendingOrCancelledQty = refinedTable.ColumnValue(row, "Pending/Cancelled Qty"),
                //                //StockQty = refinedTable.ColumnValue(row, "Stock Qty"),
                //                //SellingPrice = refinedTable.ColumnValue(row, "Selling Price"),

                //                DistributorId = distributorId
                //            };

                //            // Get WorkshopId by ConsPartyCode
                //            var userDetail = userDetails.FirstOrDefault(u => u.ConsPartyCode == cbOrder.PartyCode);
                //            if (userDetail != null)
                //            {
                //                var ru = new RepoUsers();
                //                var ws = ru.GetWorkshopByUserId(userDetail.UserId);
                //                cbOrder.WorkshopId = ws?.WorkShopId;
                //            }

                //            var existingCbo = dbCbos.FirstOrDefault(c => c.LocCode == cbOrder.LocCode && c.PartyCode == cbOrder.PartyCode && c.PartNum == cbOrder.PartNum && c.Remark == cbOrder.Remark);
                //            if (existingCbo != null)
                //            {
                //                existingCbo.CODate = cbOrder.CODate;
                //                existingCbo.PartyName = cbOrder.PartyName;
                //                existingCbo.PartDesc = cbOrder.PartDesc;
                //                existingCbo.Order = cbOrder.Order;
                //                existingCbo.CBO = cbOrder.CBO;
                //                existingCbo.StkMW = cbOrder.StkMW;
                //                existingCbo.ETA = cbOrder.ETA;
                //                existingCbo.Inv = cbOrder.Inv;
                //                existingCbo.Pick = cbOrder.Pick;
                //                existingCbo.Alloc = cbOrder.Alloc;
                //                existingCbo.BO = cbOrder.BO;
                //                existingCbo.AO = cbOrder.AO;
                //                existingCbo.Action = cbOrder.Action;
                //                existingCbo.Remark = cbOrder.Remark;
                //                existingCbo.PD = cbOrder.PD;
                //                existingCbo.WorkshopId = cbOrder.WorkshopId;

                //                // Add Old OrderNumber in model
                //                cbOrder.CONo = existingCbo.CONo;

                //                //existingCbo.PartyType = cbOrder.PartyType;
                //                //existingCbo.CustomerOrderType = cbOrder.CustomerOrderType;
                //                //existingCbo.OrderType = cbOrder.OrderType;
                //                //existingCbo.PartStatus = cbOrder.PartStatus;
                //                //existingCbo.BinLoc = cbOrder.BinLoc;
                //                //existingCbo.OrderedQty = cbOrder.OrderedQty;
                //                //existingCbo.ProcessedQty = cbOrder.ProcessedQty;
                //                //existingCbo.PendingOrCancelledQty = cbOrder.PendingOrCancelledQty;
                //                //existingCbo.StockQty = cbOrder.StockQty;
                //                //existingCbo.SellingPrice = cbOrder.SellingPrice;
                //                //existingCbo.DistributorId = cbOrder.DistributorId;
                //                //existingCbo.WorkshopId = cbOrder.WorkshopId;
                //                db.SaveChanges();
                //                populated = true;
                //            }
                //            else
                //            {
                //                // Add New OrderNumber in model
                //                cbOrder.CONo = Utils.Randomint();
                //                db.CustomerBackOrders.Add(cbOrder);
                //                populated = db.SaveChanges() > 0;
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            RepoUserLogs.LogException(e);
                //        }
                //    }
                //}
                #endregion

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.BackOrderImport);
                }
            }
            catch (Exception exc)
            {
                populated = false;
                importErrorMsg.Add(new ImportErrorMessage
                {
                    UniqueId = null,
                    ErrorMsg = exc.Message
                });
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        #endregion

        #region Import Outstanding

        /// <summary>
        /// Fetch and populate outstanding.
        /// </summary>
        /// <param name="path">The full path to excel file.</param>
        /// <param name="distributorId">The selected distributor Id.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateOutstanding(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessOutstanding(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private Tuple<bool, List<ImportErrorMessage>> ProcessOutstanding(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var rowCount = 1;
            var importErrorMsg = new List<ImportErrorMessage>();

            try
            {
                // Filter user details by party codes
                var partyCodes = from DataRow dr in refinedTable.Rows
                                 select Convert.ToString(dr["Party Code"]);
                var userDetails = _db.UserDetails.Where(u => partyCodes.Contains(u.ConsPartyCode)).ToList();

                var ru = new RepoUsers();
                var tuples = new List<Tuple<Outstanding, WorkShop>>();
                var newOsList = new List<Outstanding>();

                foreach (DataRow row in refinedTable.Rows)
                {
                    var partyCode = string.Empty;
                    try
                    {
                        rowCount++;
                        partyCode = refinedTable.ColumnValue(row, "Party Code");
                        if (string.IsNullOrEmpty(partyCode))
                        {
                            throw new Exception($"Party code missing on row {rowCount}");
                        }

                        var seOrBranchCode = refinedTable.ColumnValue(row, "Sales Executive/Branch Code");
                        var outstanding = new Outstanding
                        {
                            PartyCode = partyCode,
                            PartyName = refinedTable.ColumnValue(row, "Party Name"),
                            CustomerType = refinedTable.ColumnValue(row, "Customer Type"),
                            SalesExecutiveOrBranchCode = seOrBranchCode,
                            PendingBills = refinedTable.ColumnValue(row, "Pending Bills"),
                            LessThan7Days = refinedTable.ColumnValue(row, "(< 7 days )"),
                            C7To14Days = refinedTable.ColumnValue(row, "7 to 14 days"),
                            C14To21Days = refinedTable.ColumnValue(row, "14 to 21 days"),
                            C21To28Days = refinedTable.ColumnValue(row, "21 to 28 days"),
                            C28To35Days = refinedTable.ColumnValue(row, "28 to 35 days"),
                            C35To50Days = refinedTable.ColumnValue(row, "35 to 50 days"),
                            C50To70Days = refinedTable.ColumnValue(row, "50 to 70 days"),
                            MoreThan70Days = refinedTable.ColumnValue(row, "(> 70 days )"),
                            DistributorId = distributorId,
                            TotalOutstanding = 0,
                            CreatedDate = DateTime.Now
                        };

                        // Put PendingBills value in TotalOutstanding
                        if (decimal.TryParse(outstanding.PendingBills, out decimal dPendingBills))
                        {
                            outstanding.TotalOutstanding = dPendingBills;
                        }

                        // Get workshop by party code
                        var ws = ru.GetWorkshopByUserId(userDetails.FirstOrDefault(u => u.ConsPartyCode == outstanding.PartyCode)?.UserId);
                        outstanding.WorkshopId = ws?.WorkShopId;

                        using (var db = new garaazEntities())
                        {
                            var existingOs = db.Outstandings.FirstOrDefault(o => o.DistributorId == distributorId && o.PartyCode.Equals(partyCode, StringComparison.OrdinalIgnoreCase) && o.SalesExecutiveOrBranchCode.Equals(seOrBranchCode, StringComparison.OrdinalIgnoreCase));

                            if (existingOs != null)
                            {
                                existingOs.PartyName = outstanding.PartyName;
                                existingOs.CustomerType = outstanding.CustomerType;
                                existingOs.PendingBills = outstanding.PendingBills;
                                existingOs.LessThan7Days = outstanding.LessThan7Days;
                                existingOs.C7To14Days = outstanding.C7To14Days;
                                existingOs.C14To21Days = outstanding.C14To21Days;
                                existingOs.C21To28Days = outstanding.C21To28Days;
                                existingOs.C28To35Days = outstanding.C28To35Days;
                                existingOs.C35To50Days = outstanding.C35To50Days;
                                existingOs.C50To70Days = outstanding.C50To70Days;
                                existingOs.MoreThan70Days = outstanding.MoreThan70Days;

                                db.SaveChanges();
                                populated = true;
                                tuples.Add(new Tuple<Outstanding, WorkShop>(existingOs, ws));
                                imported++;
                            }
                            else
                            {
                                newOsList.Add(outstanding);
                                tuples.Add(new Tuple<Outstanding, WorkShop>(outstanding, ws));
                                imported++;
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = partyCode,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }

                if (newOsList.Count > 0)
                {
                    using (var db = new garaazEntities())
                    {
                        db.Outstandings.AddRange(newOsList);
                        populated = db.SaveChanges() > 0;
                    }
                }

                foreach (var tuple in tuples)
                {
                    UpdateWsOutstandingAmount(tuple.Item1, tuple.Item2);
                }

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.OutstandingImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        /// <summary>
        /// Update workshop's outstanding amount.
        /// </summary>
        private static void UpdateWsOutstandingAmount(Outstanding outstanding, WorkShop ws)
        {
            if (ws == null) return;

            using (var db = new garaazEntities())
            {
                var wsFromDb = db.WorkShops.FirstOrDefault(w => w.WorkShopId == ws.WorkShopId);
                if (wsFromDb == null) return;

                var outstandingAmount = 0.0M;
                var outstandingDays = Convert.ToInt32(ws.CriticalOutstandingDays);

                // Calculate outstanding amount
                var rangeByDays = Utils.RangeByDays();
                rangeByDays = rangeByDays.Where(r => r.Maximum > outstandingDays).ToList();

                foreach (var range in rangeByDays)
                {
                    decimal amount;
                    switch (range.Tag)
                    {
                        case "LessThan7Days":

                            if (decimal.TryParse(outstanding.LessThan7Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "7To14Days":
                            if (decimal.TryParse(outstanding.C7To14Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "14To21Days":
                            if (decimal.TryParse(outstanding.C14To21Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "21To28Days":
                            if (decimal.TryParse(outstanding.C21To28Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "28To35Days":
                            if (decimal.TryParse(outstanding.C28To35Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "35To50Days":
                            if (decimal.TryParse(outstanding.C35To50Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "50To70Days":
                            if (decimal.TryParse(outstanding.C50To70Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;

                        case "MoreThan70Days":
                            if (decimal.TryParse(outstanding.MoreThan70Days, out amount))
                            {
                                outstandingAmount += amount;
                            }
                            break;
                    }
                }

                wsFromDb.OutstandingAmount = outstandingAmount;
                wsFromDb.TotalOutstanding = outstanding.TotalOutstanding;
                db.SaveChanges();
            }
        }
        #endregion

        #region Import Account Ledger

        /// <summary>
        /// Fetch and populate account ledger.
        /// </summary>
        /// <param name="path">The full path to uploaded excel file.</param>
        /// <param name="distributorId">The id of the distributor.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateAccountLedger(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;

            var accountTuple = ProcessAccountLedgers(distributorId, refinedTable, out var accountImported, out var accountSkipped);

            // Create outstanding
            var codes = refinedTable.AsEnumerable().GroupBy(r => r.Field<string>("Code"));
            var osTuple = CreateOutstandingFromLedgers(distributorId, codes, out var outstandingImported, out var outstandingSkipped);

            if (!accountTuple.Item1)
            {
                imported = accountImported;
                skipped = accountSkipped;
                return accountTuple;
            }
            else if (!osTuple.Item1)
            {
                imported = outstandingImported;
                skipped = outstandingSkipped;
                return osTuple;
            }

            // Show account ledger message if both account ledger and outstanding import succeed
            imported = accountImported;
            skipped = accountSkipped;
            return accountTuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessAccountLedgers(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var rowCount = 1;
            var importErrorMsg = new List<ImportErrorMessage>();

            // Work for Local
            //var dateFormats = new[] { "dd-MM-yyyy", "d-MM-yyyy", "dd-M-yyyy", "d-M-yyyy", "dd/MM/yyyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "dd-MM-yyyy hh:mm:ss tt", "d-MM-yyyy hh:mm:ss tt", "d-M-yyyy hh:mm:ss tt", "dd-M-yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt", "d/MM/yyyy hh:mm:ss tt", "d/M/yyyy hh:mm:ss tt", "dd/M/yyyy hh:mm:ss tt" };   

            // Work for Live
            var dateFormats = new[] { "MM-dd-yyyy", "MM-d-yyyy", "M-dd-yyyy", "M-d-yyyy", "MM/dd/yyyy", "MM/d/yyyy", "M/dd/yyyy", "M/d/yyyy", "MM-dd-yyyy hh:mm:ss tt", "MM-d-yyyy hh:mm:ss tt", "M-d-yyyy hh:mm:ss tt", "M-dd-yyyy hh:mm:ss tt", "MM/dd/yyyy hh:mm:ss tt", "MM/d/yyyy hh:mm:ss tt", "M/d/yyyy hh:mm:ss tt", "M/dd/yyyy hh:mm:ss tt" };

            var isAnyDateMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<DateTime?>("Date") == null).ToList();

            if (isAnyDateMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyDateMissingRows.Count()} rows in sheet is missing Date");
            }

            var isAnyOpeningBalMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<double?>("Opening Balance") == null).ToList();

            if (isAnyOpeningBalMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyOpeningBalMissingRows.Count()} rows in sheet is missing Opening Balance");
            }

            var isAnyClosingBalMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<double?>("Closing Balance") == null).ToList();

            if (isAnyClosingBalMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyClosingBalMissingRows.Count()} rows in sheet is missing Closing Balance");
            }
            var isAnyVoucherNoMissingRows = refinedTable.AsEnumerable().Where(r => r.Field<string>("Vch No#") == null || r.Field<string>("Vch No#") == string.Empty).ToList();

            if (isAnyVoucherNoMissingRows.Count > 0)
            {
                throw new Exception($"{isAnyVoucherNoMissingRows.Count()} rows in sheet is missing Vch No#");
            }

            var datesWithCode = (from DataRow r in refinedTable.Rows
                                 select new AccountLedger
                                 {
                                     Date = refinedTable.DateColumnValue(r, "Date", dateFormats),
                                     Code = refinedTable.ColumnValue(r, "Code")
                                 }).ToList();

            var roWiseData = (from DataRow r in refinedTable.Rows
                              select new
                              {
                                  Code = refinedTable.ColumnValue(r, "Code"),
                                  Location = refinedTable.ColumnValue(r, "Location Code"),
                                  Closing = refinedTable.DecimalColumnValue(r, "Closing Balance"),
                                  Opening = refinedTable.DecimalColumnValue(r, "Opening Balance")
                              }).ToList();


            decimal? openingBal = null, closingBal = null;
            string oldCode = null; DateTime? oldDate = null;
            var account = new AccountLedger();

            try
            {
                foreach (DataRow row in refinedTable.Rows)
                {
                    var code = string.Empty;
                    try
                    {
                        rowCount++;

                        code = refinedTable.ColumnValue(row, "Code");
                        if (string.IsNullOrWhiteSpace(code))
                        {
                            throw new Exception($"Code missing on row {rowCount}");
                        }

                        var location = refinedTable.ColumnValue(row, "Location Code");
                        if (string.IsNullOrEmpty(location))
                        {
                            throw new Exception($"Location code missing on row {rowCount}");
                        }

                        var particulars = refinedTable.ColumnValue(row, "Particulars");
                        if (string.IsNullOrWhiteSpace(particulars))
                        {
                            throw new Exception($"Particulars missing on row {rowCount}");
                        }

                        var credit = refinedTable.DecimalColumnValue(row, "Credit");
                        var vchNo = refinedTable.ColumnValue(row, "Vch No#");

                        //if (string.IsNullOrWhiteSpace(vchNo) && !particulars.Equals("Opening Balance", StringComparison.OrdinalIgnoreCase) && !particulars.Equals("Closing Balance", StringComparison.OrdinalIgnoreCase) && !credit.HasValue)
                        //{
                        //    throw new Exception($"Voucher number missing on row {rowCount}");
                        //}

                        // Add Opening and closing balance rows
                        if (!string.IsNullOrWhiteSpace(oldCode) && oldCode != code)
                        {
                            // save Closing Balance row
                            account.Debit = closingBal;
                            account.Particulars = "Closing Balance";
                            account.Date = oldDate;

                            SaveOpeningClosingAccount(account);

                            openingBal = null;
                            closingBal = null;
                        }
                        if (string.IsNullOrEmpty(openingBal.ToString()) || string.IsNullOrEmpty(closingBal.ToString()) || oldCode != code)
                        {
                            // delete this workshop accounts before insert new data from startdate
                            var isDelete = DeleteOldRecordsForWs(datesWithCode, code, distributorId);


                            var openingClosing = roWiseData.Where(w => w.Code == code).GroupBy(a => a.Location).Select(g => new
                            {
                                Opening = g.Max(s => s.Opening.Value),
                                Closing = g.Max(s => s.Closing.Value)
                            });

                            openingBal = openingClosing.Sum(a => a.Opening);
                            closingBal = openingClosing.Sum(a => a.Closing);

                            // save Opening Balance row
                            account = new AccountLedger
                            {
                                PartyName = refinedTable.ColumnValue(row, "Party Ledger Name"),
                                Code = code,
                                Date = refinedTable.DateColumnValue(row, "Date", dateFormats),
                                Particulars = "Opening Balance",
                                Debit = openingBal,
                                DistributorId = distributorId
                            };

                            SaveOpeningClosingAccount(account);
                        }

                        oldDate = refinedTable.DateColumnValue(row, "Date", dateFormats);
                        oldCode = code;

                        SqlParameter[] alParameter = {
                            new SqlParameter("@Action",1),
                            new SqlParameter("@DistributorId",distributorId),
                            new SqlParameter("@Location",location),
                            new SqlParameter("@ConsPartyCode",code),
                            new SqlParameter("@PartyName",refinedTable.ColumnValue(row, "Party Ledger Name")),
                            new SqlParameter("@Date",oldDate),
                            new SqlParameter("@Particulars",particulars),
                            new SqlParameter("@VchType",refinedTable.ColumnValue(row, "Vch type")),
                            new SqlParameter("@VchNo",vchNo),
                            new SqlParameter("@Debit",refinedTable.DecimalColumnValue(row, "Debit")),
                            new SqlParameter("@Credit",credit),
                            new SqlParameter("@RunningBalance",refinedTable.DecimalColumnValue(row, "Running Balance")),
                            new SqlParameter("@DueDays",refinedTable.IntColumnValue(row, "Due Days"))
                        };

                        var isSave = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportAccountLedger", alParameter) > 0;
                        if (isSave == false)
                        {
                            importErrorMsg.Add(new ImportErrorMessage
                            {
                                UniqueId = $"On row {rowCount}",
                                ErrorMsg = $"The code {code} is not matched with any workshop"
                            });
                        }
                        if (!populated) { populated = isSave; }
                        imported++;
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = $"On row {rowCount}",
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }

                // set last workshop Closing row
                if (oldCode != null)
                {
                    // save Closing Balance row
                    account.Debit = closingBal;
                    account.Particulars = "Closing Balance";
                    account.Date = oldDate;

                    populated = SaveOpeningClosingAccount(account);

                    openingBal = null;
                    closingBal = null;
                }

                #region Comment old code
                //using (var db = new garaazEntities())
                //{
                //    var userDetails = db.UserDetails.ToList();
                //    foreach (DataRow row in refinedTable.Rows)
                //    {
                //        try
                //        {
                //            var isClosing = false;
                //            var location = refinedTable.ColumnValue(row, "Location");
                //            if (string.IsNullOrEmpty(location)) continue;

                //            var particulars = refinedTable.ColumnValue(row, "Particulars");
                //            if (particulars == "Closing Balance")
                //            {
                //                isClosing = true;
                //            }

                //            var code = refinedTable.ColumnValue(row, "Code");

                //// Get workshop by code
                //var ud = userDetails.FirstOrDefault(u => u.ConsPartyCode == code);
                //var ru = new RepoUsers();
                //var ws = ru.GetWorkshopByUserId(ud?.UserId);
                //if (ws == null) continue;

                //            var voucherNo = refinedTable.ColumnValue(row, "Vch No#");

                //            // Update or create new
                //            AccountLedger oldAccountLedger;
                //            if (particulars.Equals("Opening Balance", StringComparison.OrdinalIgnoreCase) || particulars.Equals("Closing Balance", StringComparison.OrdinalIgnoreCase))
                //            {
                //                oldAccountLedger = db.AccountLedgers.FirstOrDefault(a => a.WorkshopId == ws.WorkShopId && a.Particulars.Equals(particulars, StringComparison.OrdinalIgnoreCase));
                //            }
                //            else
                //            {
                //                oldAccountLedger = db.AccountLedgers.FirstOrDefault(a => a.WorkshopId == ws.WorkShopId && a.VchNo == voucherNo);
                //            }


                //            if (oldAccountLedger != null && !string.IsNullOrEmpty(oldAccountLedger.Particulars))
                //            {
                //                oldAccountLedger.Location = location;
                //                oldAccountLedger.PartyName = refinedTable.ColumnValue(row, "Party Name");
                //                oldAccountLedger.Code = code;
                //                oldAccountLedger.Date = Convert.ToDateTime(refinedTable.DateColumnValue(row, "Date"));
                //                //oldAccountLedger.Date = refinedTable.DateColumnValue(row, "Date", new[] { "d/MM/yyyy hh:mm:ss tt", "dd-MM-yyyy" });
                //                oldAccountLedger.Particulars = particulars;
                //                oldAccountLedger.VchType = refinedTable.ColumnValue(row, "Vch type");

                //                // Column name Vch No. is changed to Vch No# when data is read from excel file
                //                oldAccountLedger.VchNo = refinedTable.ColumnValue(row, "Vch No#");

                //                oldAccountLedger.Debit = refinedTable.DecimalColumnValue(row, "Debit");
                //                oldAccountLedger.Credit = refinedTable.DecimalColumnValue(row, "Credit");
                //                oldAccountLedger.DistributorId = distributorId;
                //                oldAccountLedger.IsClosing = isClosing;
                //                db.SaveChanges();
                //                populated = true;
                //            }
                //            else
                //            {
                //                var accountLedger = new AccountLedger
                //                {
                //                    Location = location,
                //                    PartyName = refinedTable.ColumnValue(row, "Party Name"),
                //                    Code = code,
                //                    Date = Convert.ToDateTime(refinedTable.DateColumnValue(row, "Date")),
                //                    //Date = refinedTable.DateColumnValue(row, "Date", new[] { "d/MM/yyyy hh:mm:ss tt", "dd-MM-yyyy" }),
                //                    Particulars = particulars,
                //                    VchType = refinedTable.ColumnValue(row, "Vch type"),

                //                    // Column name Vch No. is changed to Vch No# when data is read from excel file
                //                    VchNo = refinedTable.ColumnValue(row, "Vch No#"),
                //                    Debit = refinedTable.DecimalColumnValue(row, "Debit"),
                //                    Credit = refinedTable.DecimalColumnValue(row, "Credit"),
                //                    DistributorId = distributorId,
                //                    IsClosing = isClosing,
                //                    WorkshopId = ws.WorkShopId
                //                };

                //                db.AccountLedgers.Add(accountLedger);
                //                populated = db.SaveChanges() > 0;
                //            }
                //        }
                //        catch (Exception e)
                //        {
                //            RepoUserLogs.LogException(e);
                //        }
                //    }
                //}
                #endregion

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.AccountLedgerImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        private static bool SaveOpeningClosingAccount(AccountLedger ledger)
        {
            bool isSave = false;
            SqlParameter[] alParameter = {
                            new SqlParameter("@Action",1),
                            new SqlParameter("@DistributorId",ledger.DistributorId),
                            new SqlParameter("@Location",ledger.Location),
                            new SqlParameter("@ConsPartyCode",ledger.Code),
                            new SqlParameter("@PartyName",ledger.PartyName),
                            new SqlParameter("@Date",ledger.Date),
                            new SqlParameter("@Particulars",ledger.Particulars),
                            new SqlParameter("@VchType",ledger.VchType),
                            new SqlParameter("@VchNo",ledger.VchNo),
                            new SqlParameter("@Debit",ledger.Debit),
                            new SqlParameter("@Credit",ledger.Credit)
                        };

            isSave = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportAccountLedger", alParameter) > 0;
            return isSave;
        }

        // delete old data before insert new
        private static bool DeleteOldRecordsForWs(List<AccountLedger> ledger, string code, int distributorId)
        {
            var db = new garaazEntities();
            ledger = ledger.Where(a => a.Code.Equals(code, StringComparison.OrdinalIgnoreCase)).OrderBy(a => a.Date).ToList();
            var startDate = ledger.Select(a => a.Date).FirstOrDefault();

            var accountLedgers = db.AccountLedgers.Where(a => a.Particulars != "Opening Balance" && a.Particulars != "Closing Balance" && a.Code.Equals(code, StringComparison.OrdinalIgnoreCase) && a.DistributorId == distributorId && a.Date >= startDate).ToList();
            if (accountLedgers.Count() > 0)
            {
                db.AccountLedgers.RemoveRange(accountLedgers);
                db.SaveChanges();
                return true;
            }
            return false;
        }

        private static Tuple<bool, List<ImportErrorMessage>> CreateOutstandingFromLedgers(int distributorId, IEnumerable<IGrouping<string, DataRow>> codes, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;

            var rowCount = 0;
            var importErrorMsg = new List<ImportErrorMessage>();

            const string pOpeningBal = "Opening Balance";
            const string pClosingBal = "Closing Balance";

            try
            {
                foreach (var codeColumn in codes)
                {
                    var code = codeColumn.Key;

                    var accountEntries = new List<AccountEntry>();

                    var db = new garaazEntities();
                    var accountLedgers = db.AccountLedgers.Where(a => a.Code == code).OrderBy(a => a.Date).AsNoTracking().ToList();

                    var workshop = (from w in db.WorkShops.AsNoTracking()
                                    join d in db.DistributorWorkShops.AsNoTracking() on w.WorkShopId equals d.WorkShopId
                                    join u in db.UserDetails.AsNoTracking() on d.UserId equals u.UserId
                                    where u.ConsPartyCode == code
                                    select w).FirstOrDefault();

                    foreach (var accountLedger in accountLedgers)
                    {
                        rowCount++;

                        try
                        {
                            var rowDate = accountLedger.Date;
                            var particulars = accountLedger.Particulars;
                            var isClosingParticular = particulars.Equals(pClosingBal, StringComparison.OrdinalIgnoreCase);
                            var isOpeningParticular = particulars.Equals(pOpeningBal, StringComparison.OrdinalIgnoreCase);
                            if (rowDate.HasValue && !isClosingParticular)
                            {
                                accountEntries.Add(new AccountEntry
                                {
                                    Particulars = particulars,
                                    Debit = accountLedger.Debit ?? 0,
                                    Credit = accountLedger.Credit ?? 0,
                                    Date = rowDate.Value
                                });
                                imported++;
                            }

                            // Create outstanding
                            if (isClosingParticular && accountEntries.Count > 0)
                            {
                                var tblTotalOs = accountLedger.Debit ?? 0;
                                var totalOs = tblTotalOs;

                                accountEntries = accountEntries.OrderByDescending(a => a.Date).ToList();
                                var startDate = rowDate.Value; // This rowDate is date from 'Closing Balance' row
                                var endDate = startDate.AddDays(-6);
                                var osLessThan7days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs, true);

                                startDate = endDate;
                                endDate = endDate.AddDays(-7);
                                totalOs -= osLessThan7days;
                                var os7To14Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs);

                                startDate = endDate;
                                endDate = endDate.AddDays(-7);
                                totalOs -= os7To14Days;
                                var os14To21Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs);

                                startDate = endDate;
                                endDate = endDate.AddDays(-7);
                                totalOs -= os14To21Days;
                                var os21To28Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs);

                                startDate = endDate;
                                endDate = endDate.AddDays(-7);
                                totalOs -= os21To28Days;
                                var os28To35Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs);

                                startDate = endDate;
                                endDate = endDate.AddDays(-15);
                                totalOs -= os28To35Days;
                                var os35To50Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs);

                                startDate = endDate;
                                endDate = endDate.AddDays(-20);
                                totalOs -= os35To50Days;
                                var os50To70Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs);

                                startDate = endDate;
                                totalOs -= os50To70Days;
                                var osMoreThan70Days = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs, false, true);

                                // Get outstanding categorized into 'less than and more than' as per outstanding days
                                var outstandingDays = Convert.ToInt32(workshop.CriticalOutstandingDays);
                                totalOs = accountLedger.Debit ?? 0;

                                endDate = rowDate.Value.AddDays(outstandingDays * -1);
                                var osLessThanCriticalDay = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs, true);

                                startDate = endDate;
                                totalOs -= osLessThanCriticalDay;
                                var osMoreThanCriticalDay = GetOutstandingAsPerDate(accountEntries, startDate, endDate, totalOs, false, true);

                                var salesExecutiveOrBranchCode = accountLedger.Location;
                                var salesUser = (from u in db.UserDetails.AsNoTracking()
                                                 join se in db.RoSalesExecutives.AsNoTracking() on u.UserId equals se.SeUserId
                                                 join d in db.DistributorsOutlets.AsNoTracking() on se.RoUserId equals d.UserId
                                                 join o in db.Outlets.AsNoTracking() on d.OutletId equals o.OutletId
                                                 where o.OutletCode == salesExecutiveOrBranchCode
                                                 select u).FirstOrDefault();
                                if (salesUser != null)
                                {
                                    salesExecutiveOrBranchCode = salesUser.FirstName + " " + salesUser.LastName;
                                }

                                var outstanding = new Outstanding
                                {
                                    DistributorId = distributorId,
                                    WorkshopId = workshop.WorkShopId,
                                    CreatedDate = DateTime.Now,
                                    PartyCode = code,
                                    PartyName = workshop.WorkShopName,
                                    CustomerType = workshop.Type,
                                    SalesExecutiveOrBranchCode = salesExecutiveOrBranchCode,
                                    LessThan7Days = osLessThan7days.ToString(CultureInfo.InvariantCulture),
                                    C7To14Days = os7To14Days.ToString(CultureInfo.InvariantCulture),
                                    C14To21Days = os14To21Days.ToString(CultureInfo.InvariantCulture),
                                    C21To28Days = os21To28Days.ToString(CultureInfo.InvariantCulture),
                                    C28To35Days = os28To35Days.ToString(CultureInfo.InvariantCulture),
                                    C35To50Days = os35To50Days.ToString(CultureInfo.InvariantCulture),
                                    C50To70Days = os50To70Days.ToString(CultureInfo.InvariantCulture),
                                    MoreThan70Days = osMoreThan70Days.ToString(CultureInfo.InvariantCulture),
                                    LessThanCriticalDay = osLessThanCriticalDay,
                                    MoreThanCriticalDay = osMoreThanCriticalDay,
                                    TotalOutstanding = tblTotalOs,
                                    PendingBills = tblTotalOs.ToString(CultureInfo.InvariantCulture)
                                };

                                var existingOs = db.Outstandings.FirstOrDefault(o => o.DistributorId == distributorId && o.PartyCode.Equals(code, StringComparison.OrdinalIgnoreCase) && o.SalesExecutiveOrBranchCode.Equals(salesExecutiveOrBranchCode, StringComparison.OrdinalIgnoreCase));

                                if (existingOs != null)
                                {
                                    existingOs.PartyName = outstanding.PartyName;
                                    existingOs.CustomerType = outstanding.CustomerType;
                                    existingOs.LessThan7Days = outstanding.LessThan7Days;
                                    existingOs.C7To14Days = outstanding.C7To14Days;
                                    existingOs.C14To21Days = outstanding.C14To21Days;
                                    existingOs.C21To28Days = outstanding.C21To28Days;
                                    existingOs.C28To35Days = outstanding.C28To35Days;
                                    existingOs.C35To50Days = outstanding.C35To50Days;
                                    existingOs.C50To70Days = outstanding.C50To70Days;
                                    existingOs.MoreThan70Days = outstanding.MoreThan70Days;
                                    existingOs.LessThanCriticalDay = osLessThanCriticalDay;
                                    existingOs.MoreThanCriticalDay = osMoreThanCriticalDay;
                                    existingOs.TotalOutstanding = outstanding.TotalOutstanding;
                                    existingOs.PendingBills = outstanding.PendingBills;

                                    db.SaveChanges();
                                    populated = true;
                                }
                                else
                                {
                                    db.Outstandings.Add(outstanding);
                                    db.SaveChanges();
                                    populated = true;
                                }

                                UpdateWsOutstandingAmount(outstanding, workshop);
                                accountEntries.Clear();
                            }
                        }
                        catch (Exception exc)
                        {
                            accountEntries.Clear();
                            populated = false;
                            skipped++;
                            importErrorMsg.Add(new ImportErrorMessage
                            {
                                UniqueId = rowCount.ToString(),
                                ErrorMsg = exc.Message
                            });
                            RepoUserLogs.LogException(exc);
                        }
                    }

                }
                if (populated)
                {
                    SetFileUploadDate(distributorId, Constants.OutstandingImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        private static decimal GetOutstandingAsPerDate(IEnumerable<AccountEntry> accountEntries, DateTime startDate, DateTime endDate, decimal totalOs, bool useEndDateOnly = false, bool useStartDateOnly = false)
        {
            decimal pendingOs;

            if (useEndDateOnly)
            {
                var filteredEntries = accountEntries.Where(a => a.Date >= endDate).ToList();
                pendingOs = filteredEntries.Sum(a => a.Debit);
            }
            else if (useStartDateOnly)
            {
                var filteredEntries = accountEntries.Where(a => a.Date < startDate).ToList();
                pendingOs = filteredEntries.Sum(a => a.Debit);
            }
            else
            {
                var filteredEntries = accountEntries.Where(a => a.Date < startDate && a.Date >= endDate).ToList();
                pendingOs = filteredEntries.Sum(a => a.Debit);
            }

            if (pendingOs >= totalOs)
            {
                pendingOs = totalOs;
            }

            return pendingOs;
        }

        #endregion

        #region Import Request Part Filter

        /// <summary>
        /// Fetch and populate request part filter.
        /// </summary>
        /// <param name="path">The full path to uploaded excel file.</param>
        /// <param name="distributorId">The id of the distributor.</param>
        /// <param name="totalRecords">Total number of records in excel file.</param>
        /// <param name="imported">Number of records imported.</param>
        /// <param name="skipped">Number of records skipped.</param>
        public Tuple<bool, List<ImportErrorMessage>> FetchAndPopulateRequestPartFilter(string path, int distributorId, out int totalRecords, out int imported, out int skipped)
        {
            var refinedTable = General.GetTableFromExcelFile(path);
            totalRecords = refinedTable.Rows.Count;
            var tuple = ProcessRequestPartFilter(distributorId, refinedTable, out imported, out skipped);
            return tuple;
        }

        private static Tuple<bool, List<ImportErrorMessage>> ProcessRequestPartFilter(int distributorId, DataTable refinedTable, out int imported, out int skipped)
        {
            var populated = false;
            imported = 0;
            skipped = 0;
            var rowCount = 1;
            var importErrorMsg = new List<ImportErrorMessage>();

            try
            {
                foreach (DataRow row in refinedTable.Rows)
                {
                    var carmake = string.Empty;
                    try
                    {
                        rowCount++;
                        carmake = refinedTable.ColumnValue(row, "CarMake");
                        if (string.IsNullOrWhiteSpace(carmake))
                        {
                            throw new Exception($"CarMake missing on row {rowCount}");
                        }
                        var model = refinedTable.ColumnValue(row, "Model");
                        if (string.IsNullOrWhiteSpace(model))
                        {
                            throw new Exception($"Model missing on row {rowCount}");
                        }
                        var year = refinedTable.ColumnValue(row, "Year");
                        if (string.IsNullOrWhiteSpace(year))
                        {
                            throw new Exception($"Year missing on row {rowCount}");
                        }
                        var modification = refinedTable.ColumnValue(row, "Modification");
                        if (string.IsNullOrWhiteSpace(modification))
                        {
                            throw new Exception($"Modification missing on row {rowCount}");
                        }
                        var partGroup = refinedTable.ColumnValue(row, "PartGroup");
                        if (string.IsNullOrWhiteSpace(partGroup))
                        {
                            throw new Exception($"PartGroup missing on row {rowCount}");
                        }
                        var partDescription = refinedTable.ColumnValue(row, "PartDescription");
                        if (string.IsNullOrWhiteSpace(partDescription))
                        {
                            throw new Exception($"PartDescription missing on row {rowCount}");
                        }
                        var partNumber = refinedTable.ColumnValue(row, "PartNumber");
                        if (string.IsNullOrWhiteSpace(partNumber))
                        {
                            throw new Exception($"PartNumber missing on row {rowCount}");
                        }
                        var rootPartNumber = refinedTable.ColumnValue(row, "RootPartNumber");
                        if (string.IsNullOrWhiteSpace(rootPartNumber))
                        {
                            throw new Exception($"RootPartNumber missing on row {rowCount}");
                        }
                        //set year formate
                        String[] strYears = !string.IsNullOrEmpty(year) ? year.Split('-') : null;
                        string years = string.Empty;
                        if (strYears.Count() == 2)
                        {
                            int i = Convert.ToInt32(strYears[0]);
                            int y = Convert.ToInt32(strYears[1]);
                            for (i = Convert.ToInt32(strYears[0]); i <= y; i++)
                            {
                                if (string.IsNullOrEmpty(years))
                                {
                                    years = i.ToString();
                                }
                                else { years += "," + i; }
                            }
                        }
                        else
                        {
                            years = year;
                        }

                        SqlParameter[] alParameter = {
                            new SqlParameter("@Action",1),
                            new SqlParameter("@DistributorId",distributorId),
                            new SqlParameter("@CarMake",carmake),
                            new SqlParameter("@Model",model),
                            new SqlParameter("@Year",years),
                            new SqlParameter("@Modification",modification),
                            new SqlParameter("@PartGroup",partGroup),
                            new SqlParameter("@PartDescription",partDescription),
                            new SqlParameter("@PartNumber",partNumber),
                            new SqlParameter("@RootPartNumber",rootPartNumber),
                        };

                        var isSave = SqlHelper.ExecuteNonQuery(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ImportRequestPartFilter", alParameter) > 0;
                        if (!populated) { populated = isSave; }
                        imported++;
                    }
                    catch (Exception exc)
                    {
                        populated = false;
                        skipped++;
                        importErrorMsg.Add(new ImportErrorMessage
                        {
                            UniqueId = carmake,
                            ErrorMsg = exc.Message
                        });
                        RepoUserLogs.LogException(exc);
                    }
                }

                if (imported > 0)
                {
                    SetFileUploadDate(distributorId, Constants.RequestPartfilterImport);
                }
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }

            return new Tuple<bool, List<ImportErrorMessage>>(populated, importErrorMsg);
        }

        #endregion

        #region Import Coupon List
        public bool FetchAndPopulateCoupons(string path, int schemeId)
        {
            var populated = false;
            try
            {
                var refinedTable = General.GetTableFromExcelFile(path);
                if (refinedTable.Rows.Count == 0) return false;

                var rs = new RepoSchemes();
                var ru = new RepoUsers();

                var schemeWorkshops = new List<SchemeWorkshop>();

                foreach (DataRow row in refinedTable.Rows)
                {
                    try
                    {
                        var empCode = refinedTable.ColumnValue(row, "Cons Party Code");
                        var Count = refinedTable.ColumnValue(row, "Coupon Count");

                        // Get workshop based on ConsPartyCode                   
                        var userDetail = _db.UserDetails.FirstOrDefault(u => u.ConsPartyCode == empCode);
                        if (userDetail == null) continue;

                        var workshop = ru.GetWorkshopByUserId(userDetail.UserId);
                        if (workshop != null)
                        {
                            schemeWorkshops.Add(new SchemeWorkshop
                            {
                                NumberOfCoupon = Convert.ToInt32(Count),
                                Qualified = true,
                                SchemeId = schemeId,
                                WorkshopId = workshop.WorkShopId
                            });
                            populated = true;
                        }
                    }
                    catch (Exception exc)
                    {
                        RepoUserLogs.LogException(exc);
                    }
                }

                rs.GenerateAndSaveCoupons(schemeWorkshops);

            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
            }
            return populated;
        }
        #endregion

        #region Get FocusPart data

        /// <summary>
        /// Get list of ResponseFocusPartModel from excel file.
        /// </summary>
        /// <param name="filePath">The path to uploaded excel file.</param>
        /// <param name="errorMsg">Return error message holding information about missing groups and products.</param>
        /// <returns>Return list of ResponseFocusPartModel.</returns>
        public List<ResponseFocusPartModel> GetFocusPartFromExcel(string filePath, out string errorMsg)
        {
            errorMsg = string.Empty;
            var listRfpm = new ConcurrentBag<ResponseFocusPartModel>();

            var refinedTable = General.GetTableFromExcelFile(filePath);
            if (refinedTable.Rows.Count == 0) return listRfpm.ToList();

            var groupNames = refinedTable.AsEnumerable().Select(r => r.Field<string>("Group"));
            var groups = _db.ProductGroups.Where(p => groupNames.Contains(p.GroupName)).AsNoTracking().ToList();
            var groupIds = groups.Select(g => g.GroupId);
            var products = _db.Products.Where(p => p.GroupId != null && groupIds.Contains(p.GroupId.Value)).AsNoTracking().ToList();

            //var rs = new RepoSchemes();
            var missingInfos = new ConcurrentBag<string>();
            Parallel.ForEach(refinedTable.AsEnumerable(), row =>
            {
                var groupName = refinedTable.ColumnValue(row, "Group");
                if (string.IsNullOrEmpty(groupName)) return;

                var productName = refinedTable.ColumnValue(row, "Product");
                var group = groups.FirstOrDefault(g => g.GroupName == groupName);

                if (group != null)
                {
                    var product = products.FirstOrDefault(p => p.GroupId == group.GroupId && (p.Description == productName || p.PartNo == productName || p.ProductName == productName));
                    //var product = rs.GetProducts(group.GroupId).FirstOrDefault(p => p.ProductName == productName);
                    listRfpm.Add(new ResponseFocusPartModel
                    {
                        GroupId = group.GroupId,
                        ProductId = product?.ProductId
                    });

                    if (product == null)
                    {
                        missingInfos.Add($"Product '{productName}' not found in group '{groupName}'");
                    }
                }
                else
                {
                    missingInfos.Add($"Group '{groupName}' not found.");
                }
            });


            //foreach (DataRow row in refinedTable.Rows)
            //{
            //    var groupName = refinedTable.ColumnValue(row, "Group");
            //    if (string.IsNullOrEmpty(groupName)) continue;

            //    var productName = refinedTable.ColumnValue(row, "Product");
            //    var group = groups.FirstOrDefault(g => g.GroupName == groupName);

            //    if (group != null)
            //    {
            //        var product = products.FirstOrDefault(p => p.GroupId == group.GroupId && (p.Description == productName || p.PartNo == productName || p.ProductName == productName));
            //        //var product = rs.GetProducts(group.GroupId).FirstOrDefault(p => p.ProductName == productName);
            //        listRfpm.Add(new ResponseFocusPartModel
            //        {
            //            GroupId = group.GroupId,
            //            ProductId = product?.ProductId
            //        });

            //        if (product == null)
            //        {
            //            missingInfos.Add($"Product '{productName}' not found in group '{groupName}'");
            //        }
            //    }
            //    else
            //    {
            //        missingInfos.Add($"Group '{groupName}' not found.");
            //    }
            //}

            // Set error message
            if (missingInfos.Count > 0)
            {
                errorMsg = "Following groups or products not match as per selected part category-\n\n";
                foreach (var entry in missingInfos)
                {
                    errorMsg += $"\t• {entry}\n";
                }
            }

            return listRfpm.ToList();
        }
        public List<FocusGroupModel> GetFocusPartFromExcel(string filePath, int schemeId, int distributorId, out string errorMsg)
        {
            errorMsg = string.Empty;
            var response = new List<FocusGroupModel>();
            var listRfpm = new ConcurrentBag<ResponseFocusPartModel>();

            var refinedTable = General.GetTableFromExcelFile(filePath);
            if (refinedTable.Rows.Count == 0) return response;

            var groupNames = refinedTable.AsEnumerable().Select(r => r.Field<string>("Group"));
            var groups = _db.ProductGroups.Where(p => groupNames.Contains(p.GroupName) && p.DistributorId == distributorId).AsNoTracking().ToList();
            var groupIds = groups.Select(g => g.GroupId);
            var products = _db.Products.Where(p => p.GroupId != null && groupIds.Contains(p.GroupId.Value)).AsNoTracking().ToList();

            //var rs = new RepoSchemes();
            var missingInfos = new ConcurrentBag<string>();

            Parallel.ForEach(refinedTable.AsEnumerable(), row =>
            {
                var groupName = refinedTable.ColumnValue(row, "Group");
                if (string.IsNullOrEmpty(groupName)) return;

                var productName = refinedTable.ColumnValue(row, "Product");
                var group = groups.FirstOrDefault(g => g.GroupName == groupName);

                if (group != null)
                {
                    var product = products.FirstOrDefault(p => p.GroupId == group.GroupId && (p.Description == productName || p.PartNo == productName || p.ProductName == productName));

                    listRfpm.Add(new ResponseFocusPartModel
                    {
                        GroupId = group.GroupId,
                        Type = group.GroupName,
                        ProductId = product?.ProductId
                    });

                    if (product == null)
                    {
                        missingInfos.Add($"Product '{productName}' not found in group '{groupName}'");
                    }
                }
                else
                {
                    missingInfos.Add($"Group '{groupName}' not found.");
                }
            });

            // Set error message
            if (missingInfos.Count > 0)
            {
                errorMsg = "Following groups or products not match as per selected part category-\n\n";
                foreach (var entry in missingInfos)
                {
                    errorMsg += $"\t• {entry}\n";
                }
            }
            if (listRfpm.Count > 0)
            {
                var filterGroups = listRfpm.GroupBy(g => g.GroupId).Select(s => s.Key);
                foreach (var groupId in filterGroups)
                {
                    var group = listRfpm.Where(g => g.GroupId == groupId).ToList();
                    string[] strProductIds = group.Where(p => p.ProductId != null).Select(p => p.ProductId.ToString()).ToArray();

                    response.Add(new FocusGroupModel
                    {
                        SchemeId = schemeId,
                        GroupId = groupId,
                        GroupName = group.Select(p => p.Type).FirstOrDefault(),
                        ProductText = strProductIds != null ? strProductIds.Count() > 0 ? strProductIds.Count() + " parts selected" : "All parts selected" : "All parts selected",
                        ProductIds = string.Join(",", strProductIds)
                    });

                }
            }

            return response;
        }
        #endregion

        #region Get Workshops
        /// <summary>
        /// Get list of workshops from excel file.
        /// </summary>
        /// <param name="filePath">The path to uploaded excel file.</param>
        /// <returns>Return list of workshops.</returns>
        public List<WorkShop> GetWorkshopFromExcel(string filePath)
        {
            var listWs = new List<WorkShop>();


            var refinedTable = General.GetTableFromExcelFile(filePath);
            if (refinedTable.Rows.Count == 0) return listWs;

            var ru = new RepoUsers();
            foreach (DataRow row in refinedTable.Rows)
            {
                var consPartyCode = refinedTable.ColumnValue(row, "ConsPartyCode");
                if (string.IsNullOrEmpty(consPartyCode)) continue;

                // Get workshop based on ConsPartyCode                   
                var userDetail = _db.UserDetails.FirstOrDefault(u => u.ConsPartyCode == consPartyCode);
                if (userDetail == null) continue;

                var workshop = ru.GetWorkshopByUserId(userDetail.UserId);
                if (workshop != null)
                {
                    listWs.Add(workshop);
                }
            }

            return listWs;
        }
        #endregion

        #region Set File Upload Date

        /// <summary>
        /// Set file upload date.
        /// </summary>
        /// <param name="distributorId">The distributor Id used.</param>
        /// <param name="importDesc">The description for the import.</param>
        private static void SetFileUploadDate(int distributorId, string importDesc)
        {
            var model = new ResponseGeneralPurpose
            {
                Heading1 = importDesc,
                Heading2 = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Heading3 = $"For distributorId {distributorId}",
                Heading4 = distributorId.ToString()
            };
            var genUse = new GeneralUse();
            genUse.SetFileUploadDate(model);
        }

        #endregion

        #region Set all workshops credit limit
        public bool SetAllWorkshopsCreditLimit()
        {
            //Expample
            //Total outstanding     85,723
            //Credit Days 7
            //Avarage Sale of Last 3 Months     91,500
            //Credit Limit = (Average 3 Months Sale/No of days in a Month) * Credit Days ==	  21,350 
            //Outstanding > Credit days(7 days)     62,345
            //Critical Payment:	1.Total Outstanding - Credit Limit   64,373

            //    2.Pyments which are above credit Days(7 days)   62,345

            //>> Critical Amount is higher of 1 & 2     64,373

            var distributorId = 0;
            var isSuperAdmin = General.IsSuperAdmin();
            var isDistributor = General.IsDistributor();
            if (isDistributor)
            {
                distributorId = General.GetCurrentDistributorId();
            }

            // Set last three month's date and days
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddMonths(-3);
            var currentMonthDays = DateTime.DaysInMonth(endDate.Year, endDate.Month);

            // Get all workshops as per admin or distributor
            var workshops = isSuperAdmin ? _db.WorkShops.ToList() : (from w in _db.WorkShops
                                                                     join dw in _db.DistributorWorkShops on w.WorkShopId equals dw.WorkShopId
                                                                     where dw.DistributorId == distributorId
                                                                     select w).ToList();

            var wsIds = workshops.Select(w => w.WorkShopId);

            // Get sales between start date and end date for selected workshops only
            var dailySales = isSuperAdmin ? _db.DailySalesTrackerWithInvoiceDatas.Where(ds => ds.CreatedDate >= startDate && ds.CreatedDate <= endDate && ds.WorkShopId != null && wsIds.Contains(ds.WorkShopId.Value)).AsNoTracking().ToList() : _db.DailySalesTrackerWithInvoiceDatas.Where(ds => ds.DistributorId == distributorId && ds.CreatedDate >= startDate && ds.CreatedDate <= endDate && ds.WorkShopId != null && wsIds.Contains(ds.WorkShopId.Value)).AsNoTracking().ToList();

            var anyUpdated = false;
            foreach (var ws in workshops)
            {
                var sales = dailySales.Where(d => d.WorkShopId == ws.WorkShopId).ToList();

                var totalOutstanding = ws.TotalOutstanding ?? 0;
                var creditDays = ws.CriticalOutstandingDays ?? 0;
                var avgThreeMonthSale = sales.Count > 0 ? sales.Sum(x => Convert.ToDecimal(x.NetRetailSelling)) / 3 : 0;

                var creditLimit = avgThreeMonthSale > 0 && creditDays > 0 ? (avgThreeMonthSale / currentMonthDays) * creditDays : 0;
                ws.CreditLimit = creditLimit;

                var totalOutstandingCreditLimit = totalOutstanding - creditLimit;
                if (totalOutstandingCreditLimit > ws.OutstandingAmount)
                {
                    ws.OutstandingAmount = totalOutstandingCreditLimit;
                }
                anyUpdated = true;
            }

            if (anyUpdated)
            {
                _db.SaveChanges();
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Get list of DailySalesTrackerWithInvoiceData.
        /// </summary>
        public List<DailySalesTrackerWithInvoiceData> GetDailySales(int distributorId, string sDate, string eDate, string fAmt, string tAmt, string workshopId)
        {
            CultureInfo MyCultureInfo = CultureInfo.InvariantCulture;
            var data = _db.DailySalesTrackerWithInvoiceDatas.Take(50).ToList();
            if (!string.IsNullOrEmpty(workshopId))
            {
                data = data.Where(x => x.WorkShopId == Convert.ToInt32(workshopId)).ToList();
            }
            if (distributorId > 0)
            {
                data = data.Where(x => x.DistributorId == distributorId).ToList();
            }
            if (!string.IsNullOrEmpty(sDate) && !string.IsNullOrEmpty(eDate))
            {

                DateTime sd = DateTime.ParseExact(sDate, new[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, MyCultureInfo, DateTimeStyles.None);
                DateTime ed = DateTime.ParseExact(eDate, new[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, MyCultureInfo, DateTimeStyles.None);
                data = data.Where(x => x.CreatedDate >= sd && x.CreatedDate <= ed).ToList();
            }
            else if (!string.IsNullOrEmpty(sDate))
            {
                DateTime sd = DateTime.ParseExact(sDate, new[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, MyCultureInfo, DateTimeStyles.None);
                data = data.Where(x => x.CreatedDate >= sd).ToList();
            }
            else if (!string.IsNullOrEmpty(eDate))
            {
                DateTime ed = DateTime.ParseExact(eDate, new[] { "MM.dd.yyyy", "MM-dd-yyyy", "MM/dd/yyyy" }, MyCultureInfo, DateTimeStyles.None);
                data = data.Where(x => x.CreatedDate <= ed).ToList();
            }
            if (!string.IsNullOrEmpty(fAmt) && !string.IsNullOrEmpty(tAmt))
            {
                data = data.Where(x => Convert.ToDecimal(x.NetRetailSelling) >= Convert.ToDecimal(fAmt) && Convert.ToDecimal(x.NetRetailSelling) <= Convert.ToDecimal(tAmt)).ToList();
            }
            else if (!string.IsNullOrEmpty(fAmt))
            {
                data = data.Where(x => Convert.ToDecimal(x.NetRetailSelling) >= Convert.ToDecimal(fAmt)).ToList();
            }
            else if (!string.IsNullOrEmpty(tAmt))
            {
                data = data.Where(x => Convert.ToDecimal(x.NetRetailSelling) <= Convert.ToDecimal(tAmt)).ToList();
            }
            return data;
        }

        public List<DailySalesInvoiceModel> GetSalesData(string sortColumn, string sortColumnDirection, int skip, int pageSize, int draw, int distributorId, string startDate, string endDate, string fromAmt, string toAmt, int workshopId, out int recordsTotal)
        {
            List<DailySalesInvoiceModel> sales = new List<DailySalesInvoiceModel>();
            recordsTotal = 0;
            var sqlParams = new[] {
                new SqlParameter("@OrderBy",sortColumn),
                new SqlParameter("@OrderByDirection",sortColumnDirection),
                new SqlParameter("@Length",pageSize),
                new SqlParameter("@From",skip),
                new SqlParameter("@DistributorId",distributorId),
                new SqlParameter("@WorkshopId",workshopId),
                new SqlParameter("@StartDate",startDate),
                new SqlParameter("@EndDate",endDate),
                new SqlParameter("@FromAmt",fromAmt),
                new SqlParameter("@ToAmt",toAmt)

            };

            var ds = SqlHelper.ExecuteDataset(Connection.ConnectionString, CommandType.StoredProcedure, "usp_ShowSalesTrackerWithInvoice", sqlParams);
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var rows = ds.Tables[0].Rows[0];
                    recordsTotal = rows["TotalRecords"] != DBNull.Value ? Convert.ToInt32(rows["TotalRecords"]) : 0;
                }
                sales = ds.Tables[0].AsEnumerable().Select(dataRow => new DailySalesInvoiceModel
                {
                    DailySalesTrackerId = dataRow.Field<int>("DailySalesTrackerId"),
                    Region = dataRow.Field<string>("Region"),
                    DealerCode = dataRow.Field<string>("DealerCode"),
                    LocCode = dataRow.Field<string>("LocCode"),
                    LocDesc = dataRow.Field<string>("LocDesc"),
                    PartNum = dataRow.Field<string>("PartNum"),
                    PartDesc = dataRow.Field<string>("PartDesc"),
                    RootPartNum = dataRow.Field<string>("RootPartNum"),
                    PartCategory = dataRow.Field<string>("PartCategory"),
                    Day = dataRow.Field<string>("Day"),
                    PartGroup = dataRow.Field<string>("PartGroup"),
                    CalMonthYear = dataRow.Field<string>("CalMonthYear"),
                    ConsPartyCode = dataRow.Field<string>("ConsPartyCode"),
                    ConsPartyName = dataRow.Field<string>("ConsPartyName"),
                    ConsPartyTypeDesc = dataRow.Field<string>("ConsPartyTypeDesc"),
                    DocumentNum = dataRow.Field<string>("DocumentNum"),
                    Remarks = dataRow.Field<string>("Remarks"),
                    RetailQty = dataRow.Field<string>("RetailQty"),
                    ReturnQty = dataRow.Field<string>("ReturnQty"),
                    NetRetailQty = dataRow.Field<string>("NetRetailQty"),
                    RetailSelling = dataRow.Field<string>("RetailSelling"),
                    ReturnSelling = dataRow.Field<string>("ReturnSelling"),
                    NetRetailSelling = dataRow.Field<string>("NetRetailSelling"),
                    DiscountAmount = dataRow.Field<string>("DiscountAmount"),
                    UserId = dataRow.Field<string>("UserId"),
                    DistributorId = dataRow.Field<int?>("DistributorId"),
                    CreatedDate = dataRow.Field<DateTime?>("CreatedDate")?.ToString("dd/MM/yyyy"),
                    WorkShopId = dataRow.Field<int?>("WorkShopId"),
                    GroupId = dataRow.Field<int?>("GroupId"),
                    ProductId = dataRow.Field<int?>("ProductId"),
                    CoNo = dataRow.Field<string>("CoNo")
                }).ToList();
            }

            return sales;
            #region Comment old code
            //using (var db = new garaazEntities())
            //{
            //    db.Configuration.LazyLoadingEnabled = false;

            //     var salesTable = from s in db.DailySalesTrackerWithInvoiceDatas.AsNoTracking() select s;

            //    if (distributorId > 0) { salesTable = salesTable.Where(s =>s.DistributorId== distributorId); }

            //    if (!string.IsNullOrWhiteSpace(startDate))
            //    { salesTable = salesTable.Where(s =>s.CreatedDate>= Convert.ToDateTime(startDate)); }

            //    if (!string.IsNullOrWhiteSpace(endDate))
            //    { salesTable = salesTable.Where(s => s.CreatedDate <= Convert.ToDateTime(endDate)); }

            //    if (!string.IsNullOrWhiteSpace(fromAmt))
            //    { salesTable = salesTable.Where(s => decimal.Parse(s.NetRetailSelling) >= Convert.ToDecimal(fromAmt)); }

            //    if (!string.IsNullOrWhiteSpace(toAmt))
            //    { salesTable = salesTable.Where(s => Convert.ToDecimal(s.NetRetailSelling) <= Convert.ToDecimal(toAmt)); }

            //    if (workshopId>0)
            //    { salesTable = salesTable.Where(s => s.WorkShopId == workshopId); }

            //    recordsTotal = salesTable.Count();

            //    if (!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortColumnDirection))
            //    {
            //        salesTable = salesTable.OrderBy($"{sortColumn} {sortColumnDirection}").Skip(skip).Take(pageSize);
            //    }
            //    else
            //    {
            //        salesTable = salesTable.OrderBy(s => s.CreatedDate).Skip(skip).Take(pageSize);
            //    }

            //    return salesTable.ToList();
            //}
            #endregion
        }

        /// <summary>
        /// Get instance of DailySale containing select list of distributors.
        /// </summary>
        /// <param name="distributorId">The id of the distributor.</param>
        /// <returns>Return instance of DailySale.</returns>
        public DailySale GetDailySales(int distributorId)
        {
            // Create select list
            var selListItem = new SelectListItem { Value = "", Text = "-- Select Distributor --" };
            var newList = new List<SelectListItem> { selListItem };

            var ru = new RepoUsers();
            foreach (var dist in ru.GetAllDistributorsNew())
            {
                newList.Add(new SelectListItem { Value = dist.DistributorId.ToString(), Text = dist.DistributorName });
            }
            var distSelectList = new SelectList(newList, "Value", "Text", null);
            var ds = new DailySale
            {
                Distributors = distSelectList
            };

            // So as to keep the item in select list selected (for DistributorController case)
            if (distributorId > 0)
            {
                ds.DistributorId = distributorId;
            }

            return ds;
        }
    }
}