using HtmlTableHelper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Garaaz.Models
{
    public class ResponseAccountLedger
    {
        public string WorkshopName { get; set; }
        public int WorkshopId { get; set; }
        public string ClosingBalance { get; set; }
        public decimal ClosingBalanceAmt { get; set; }
    }
    public class AccountLedgerModel
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public int WorkshopId { get; set; }
        public int DistributorId { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AccountLedgerDetail
    {
        public int AccountLeaderId { get; set; }
        public string Location { get; set; }
        public string PartyName { get; set; }
        public string Code { get; set; }
        public string Particulars { get; set; }
        public string VchType { get; set; }
        public string VchNo { get; set; }
        public string Debit { get; set; }
        public string Credit { get; set; }
        public string Date { get; set; }
        public bool IsClosing { get; set; }
    }

    public class RepoAccountLedger
    {
        readonly garaazEntities _db = new garaazEntities();
        private const string OpenBal = "Opening Balance";
        private const string CloseBal = "Closing Balance";

        public List<ResponseAccountLedger> GetAccountLedgerClosing(AccountLedgerModel model, out int totalRecords)
        {
            var respAccountLedgers = new List<ResponseAccountLedger>();
            var workshops = new List<WorkShop>();

            model.DistributorId = model.UserId.GetDistributorId(model.Role);
            model.WorkshopId = model.UserId.GetWorkshopId(model.Role);

            if (model.Role.Contains(Constants.SuperAdmin))
            {
                workshops = (from w in _db.WorkShops
                             join dw in _db.DistributorWorkShops on w.WorkShopId equals dw.WorkShopId
                             join ud in _db.UserDetails on dw.UserId equals ud.UserId
                             where ud.IsDeleted == false
                             select w).ToList();
            }
            else if (model.Role.Contains(Constants.Distributor) || model.Role.Contains(Constants.Users))
            {
                workshops = (from w in _db.WorkShops
                             join dw in _db.DistributorWorkShops on w.WorkShopId equals dw.WorkShopId
                             join ud in _db.UserDetails on dw.UserId equals ud.UserId
                             where dw.DistributorId == model.DistributorId && ud.IsDeleted == false
                             select w).ToList();
            }
            else if (model.Role.Contains(Constants.Workshop) || model.Role.Contains(Constants.WorkshopUsers))
            {
                workshops = _db.WorkShops.Where(w => w.WorkShopId == model.WorkshopId).ToList();
            }
            else if (model.Role.Contains(Constants.SalesExecutive))
            {
                var wIds = _db.SalesExecutiveWorkshops.Where(se => se.UserId == model.UserId).Select(w => w.WorkshopId).ToList();
                workshops = (from w in _db.WorkShops
                             where wIds.Contains(w.WorkShopId)
                             select w).Distinct().ToList();
            }
            else if (model.Role.Contains(Constants.RoIncharge))
            {
                var distOutlets = _db.DistributorsOutlets.FirstOrDefault(o => o.UserId == model.UserId);
                var outletId = distOutlets != null ? Convert.ToInt32(distOutlets.OutletId) : 0;
                workshops = (from w in _db.WorkShops
                             where w.outletId == outletId
                             select w).Distinct().ToList();
            }

            if (workshops.Count > 0)
            {
                foreach (var ws in workshops)
                {
                    var closingBalance = "0";

                    var accountLedger = ws.AccountLedgers.Where(a => a.IsClosing == true).OrderByDescending(a => a.AccountLeaderId).FirstOrDefault();
                    if (accountLedger != null)
                    {
                        closingBalance = accountLedger.Debit.HasValue ? $"{accountLedger.Debit:#,###}" : "0";
                    }

                    var responseAccountLedger = new ResponseAccountLedger
                    {
                        ClosingBalance = closingBalance,
                        WorkshopName = ws.WorkShopName ?? "",
                        WorkshopId = ws.WorkShopId,
                        ClosingBalanceAmt= accountLedger!=null ? accountLedger.Debit??0 : 0
                };

                    respAccountLedgers.Add(responseAccountLedger);
                }
            }

            respAccountLedgers = respAccountLedgers.OrderByDescending(a =>a.ClosingBalanceAmt).ToList();
            totalRecords = respAccountLedgers.Count;

            if (model.PageNumber > 0)
            {
                respAccountLedgers = respAccountLedgers.GetPaging(model.PageNumber, model.PageSize);
            }
            return respAccountLedgers;
        }

        public List<AccountLedgerDetail> GetAccountLedgerClosingDetails(AccountLedgerModel model, out int totalRecords)
        {
            return GetAccountLedgerDetails(model, out totalRecords);
        }

        /// <summary>
        /// Get HTML with table having Account Ledger details.
        /// </summary>
        /// <param name="model">The model that hold detail for fetching account ledgers.</param>
        /// <returns>Return string containing HTML table.</returns>
        public string GetAccountLedgerHtmlWithTable(AccountLedgerModel model)
        {
            var accountLedgers = GetAccountLedgerDetails(model, out _);

            var filteredAccountLedgers = accountLedgers.Select(a => new
            {
                a.Date,
                a.Particulars,
                a.VchType,
                a.VchNo,
                Debit = $"₹{a.Debit}",
                Credit = $"₹{a.Credit}"
            }).ToList();

            var htmlTable = filteredAccountLedgers.ToHtmlTable(new { @class = "table table-bordered" });

            // Replace double quote with single quote
            htmlTable = htmlTable.Replace("\"", "'");

            var html = $"<html><head><link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css' integrity='sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm' crossorigin='anonymous'></head><body>{htmlTable}</body></html>";

            return html;
        }

        private List<AccountLedgerDetail> GetAccountLedgerDetails(AccountLedgerModel model, out int totalRecords)
        {
            var accountLedgers = new List<AccountLedger>();
            if (model.WorkshopId > 0)
            {
                accountLedgers = _db.AccountLedgers.Where(a => a.WorkshopId == model.WorkshopId).OrderBy(a => a.Date).AsNoTracking().ToList();
            }

            if (model.PageNumber > 0)
            {
                accountLedgers = accountLedgers.GetPaging(model.PageNumber, model.PageSize);
            }

            List<AccountLedgerDetail> accountLedgerDetails;
            if (model.StartDate != null || model.EndDate != null)
            {
                var openingBalDate = accountLedgers.Where(a => a.Particulars == OpenBal).FirstOrDefault()?.Date;

                accountLedgerDetails = GetAccountLedgerDetailsByDate(accountLedgers, model, openingBalDate);
            }
            else
            {
                accountLedgerDetails = accountLedgers.Select(item => new AccountLedgerDetail
                {
                    AccountLeaderId = item.AccountLeaderId,
                    Location = item.Location,
                    PartyName = item.PartyName,
                    Code = item.Code,
                    Particulars = item.Particulars,
                    VchType = item.VchType,
                    VchNo = item.VchNo,
                    Debit = item.Debit.HasValue ? $"{item.Debit:#,###}" : "0",
                    Credit = item.Credit.HasValue ? $"{item.Credit:#,###}" : "0",
                    Date = item.Date?.ToString("dd MMM, yyyy"),
                    IsClosing = Convert.ToBoolean(item.IsClosing)
                }).ToList();
            }

            totalRecords = accountLedgerDetails.Count;

            return accountLedgerDetails;
        }

        private List<AccountLedgerDetail> GetAccountLedgerDetailsByDate(List<AccountLedger> accountLedgers, AccountLedgerModel alModel,DateTime? openingBalDate=null)
        {
            // Filter account ledgers by start date and end date
            var filteredAccountLedgers = accountLedgers.Where(a => a.Date >= alModel.StartDate && a.Date <= alModel.EndDate).OrderBy(a => a.Date).ToList();

            var accountLedgerDetails = new List<AccountLedgerDetail>();
            var accountLedger = _db.AccountLedgers.AsNoTracking().FirstOrDefault(a => a.WorkshopId == alModel.WorkshopId);
            if (accountLedger == null) return accountLedgerDetails;

            var location = accountLedger.Location;
            var partyName = accountLedger.PartyName;
            var code = accountLedger.Code;
            var closingBalRow = accountLedgers.FirstOrDefault(a => a.Particulars.Equals(CloseBal, StringComparison.Ordinal));

            // Prepare opening balance row
            var dbAccountLedgers = _db.AccountLedgers.Where(a => a.WorkshopId == alModel.WorkshopId && a.Date < alModel.StartDate).Select(a => new
            {
                a.Particulars,
                a.Date,
                a.Credit,
                a.Debit
            }).AsNoTracking().ToList();

            decimal totalOpeningCredit, totalOpeningDebit, openingBalance;
            var openingBalRow = filteredAccountLedgers.FirstOrDefault(a => a.Particulars.Equals(OpenBal, StringComparison.Ordinal));
            if (openingBalRow != null)
            {
                totalOpeningCredit = openingBalRow.Credit ?? 0;
                totalOpeningDebit = openingBalRow.Debit ?? 0;
                openingBalance = totalOpeningCredit - totalOpeningDebit;
            }
            else
            {
                totalOpeningCredit = dbAccountLedgers.Where(a => a.Particulars != CloseBal).Sum(a => a.Credit) ?? 0;
                totalOpeningDebit = dbAccountLedgers.Where(a => a.Particulars != CloseBal).Sum(a => a.Debit) ?? 0;
                openingBalance = totalOpeningCredit - totalOpeningDebit;
            }

            // Add opening balance row
            accountLedgerDetails.Add(new AccountLedgerDetail
            {
                Location = location,
                PartyName = partyName,
                Code = code,
                Particulars = OpenBal,
                Debit = openingBalance < 0 ? $"{Math.Abs(openingBalance):#,###}" : "0",
                Credit = openingBalance > 0 ? $"{Math.Abs(openingBalance):#,###}" : "0",
                Date = alModel.StartDate<openingBalDate ? openingBalDate?.ToString("dd MMM, yyyy") : alModel.StartDate?.ToString("dd MMM, yyyy")
            });

            // Add middle rows or records
            accountLedgerDetails.AddRange(filteredAccountLedgers.Where(a => a.Particulars != CloseBal && a.Particulars != OpenBal).Select(accLedger => new AccountLedgerDetail
            {
                AccountLeaderId = accLedger.AccountLeaderId,
                Location = accLedger.Location,
                PartyName = accLedger.PartyName,
                Code = accLedger.Code,
                Particulars = accLedger.Particulars,
                VchType = accLedger.VchType,
                VchNo = accLedger.VchNo,
                Debit = accLedger.Debit.HasValue ? $"{accLedger.Debit:#,###}" : "0",
                Credit = accLedger.Credit.HasValue ? $"{accLedger.Credit:#,###}" : "0",
                Date = accLedger.Date?.ToString("dd MMM, yyyy"),
                IsClosing = Convert.ToBoolean(accLedger.IsClosing)
            }).ToList());

            // Prepare closing balance row
            var totalClosingCredit = filteredAccountLedgers.Where(a => a.Particulars != CloseBal).Sum(a => a.Credit) ?? 0;
            var totalClosingDebit = filteredAccountLedgers.Where(a => a.Particulars != CloseBal).Sum(a => a.Debit) ?? 0;
            var closingBalance = openingBalRow != null ? totalClosingCredit - totalClosingDebit : totalClosingCredit + openingBalance - totalClosingDebit;

            // Add closing balance row
            accountLedgerDetails.Add(new AccountLedgerDetail
            {
                Location = location,
                PartyName = partyName,
                Code = code,
                Particulars = CloseBal,
                Debit = closingBalance < 0 ? $"{Math.Abs(closingBalance):#,###}" : "0",
                Credit = closingBalance > 0 ? $"{Math.Abs(closingBalance):#,###}" : "0",
                Date = closingBalRow == null || alModel.EndDate <= closingBalRow.Date ? alModel.EndDate?.ToString("dd MMM, yyyy") : closingBalRow.Date?.ToString("dd MMM, yyyy"),
                IsClosing = true
            });

            return accountLedgerDetails;
        }
    }
}