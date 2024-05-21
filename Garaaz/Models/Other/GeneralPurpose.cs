using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Garaaz.Models
{
    public class ResponseGeneralPurpose
    {
        public int Id { get; set; }
        public string Heading1 { get; set; }
        [Required]
        public string Heading2 { get; set; }
        public string Heading3 { get; set; }
        public string Heading4 { get; set; }
        public string Heading5 { get; set; }
        public string Heading6 { get; set; }
        public string Heading7 { get; set; }
        public string Heading8 { get; set; }
        public string Heading9 { get; set; }
        public string Heading10 { get; set; }
        public string Heading11 { get; set; }
        public bool? ShowSecondScreen { get; set; }
        public string SecondScreenText { get; set; }
        public bool? EnableSignUpMailDelay { get; set; }
        public int? DelayTime { get; set; }
    }
    public class LastuploadModel
    {
        public string UserId { get; set; }
        public string FileType { get; set; }
    }
    public class LastdateModel
    {
        public string LastUploadDate { get; set; }
    }

    public class GeneralUse
    {
        private readonly garaazEntities _db = new garaazEntities();
        private const string DateFormat = "dd MMM, yyyy, hh:mm:ss tt";

        public bool SetFileUploadDate(ResponseGeneralPurpose model)
        {
            if (model == null) return false;

            var data = new GeneralPurpose
            {
                Heading1 = model.Heading1,
                Heading2 = model.Heading2 ?? DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Heading3 = model.Heading3,
                Heading4 = model.Heading4,
                Heading5 = model.Heading5,
                Heading6 = model.Heading6,
                Heading7 = model.Heading7,
                Heading8 = model.Heading8,
                Heading9 = model.Heading9,
                Heading10 = model.Heading10,
                Heading11 = model.Heading11,
                ShowSecondScreen = model.ShowSecondScreen,
                SecondScreenText = model.SecondScreenText,
                EnableSignupMailDelay = model.EnableSignUpMailDelay,
                DelayTime = model.DelayTime
            };
            _db.GeneralPurposes.Add(data);
            return _db.SaveChanges() > 0;
        }

        public string GetFileUploadDate(string uploadType)
        {
            var fileUploadDate = string.Empty;
            var generalPurpose = _db.GeneralPurposes.Where(a => a.Heading1 == uploadType).OrderByDescending(a => a.ID).FirstOrDefault();
            if (generalPurpose == null) return fileUploadDate;

            if (DateTime.TryParse(generalPurpose.Heading2, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                fileUploadDate = date.ToString(DateFormat);
            }
            return fileUploadDate;
        }

        public string GetDistributorFileUploadDate(int distributorId, string uploadType)
        {
            var fileUploadDate = string.Empty;

            var generalPurpose = distributorId == 0 ? _db.GeneralPurposes.Where(a => a.Heading1 == uploadType).OrderByDescending(a => a.ID).FirstOrDefault() : _db.GeneralPurposes.Where(a => a.Heading1 == uploadType && a.Heading4 == distributorId.ToString()).OrderByDescending(a => a.ID).FirstOrDefault();
            if (generalPurpose == null) return fileUploadDate;

            if (DateTime.TryParse(generalPurpose.Heading2, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                fileUploadDate = date.ToString(DateFormat);
            }

            return fileUploadDate;
        }

        public LastdateModel GetLastFileUploadDate(LastuploadModel model)
        {
            LastdateModel lastUpload = new LastdateModel();
            var aspNetUser = _db.AspNetUsers.FirstOrDefault(u => u.Id == model.UserId);
            var roles = aspNetUser?.AspNetRoles.Select(r => r.Name).ToList();
            var role = string.Empty;
            if (roles.Contains(Constants.SuperAdmin))
            {
                role = Constants.SuperAdmin;
            }
            else if (roles.Contains(Constants.Distributor))
            {
                role = Constants.Distributor;
            }
            else if (roles.Contains(Constants.Workshop))
            {
                role = Constants.Workshop;
            }
            else if (roles.Contains(Constants.WorkshopUsers))
            {
                role = Constants.WorkshopUsers;
            }
            else if (roles.Contains(Constants.RoIncharge))
            {
                role = Constants.RoIncharge;
            }
            else if (roles.Contains(Constants.SalesExecutive))
            {
                role = Constants.SalesExecutive;
            }
            else if (roles.Contains(Constants.Users))
            {
                role = Constants.Users;
            }

            RepoUsers repoUser = new RepoUsers();
            int distributorId = repoUser.getDistributorIdByUserId(model.UserId, role);

            var fileUploadDate = string.Empty;

            var generalPurpose = distributorId == 0 ? _db.GeneralPurposes.Where(a => a.Heading1 == model.FileType).OrderByDescending(a => a.ID).FirstOrDefault() : _db.GeneralPurposes.Where(a => a.Heading1 == model.FileType && a.Heading4 == distributorId.ToString()).OrderByDescending(a => a.ID).FirstOrDefault();
            if (generalPurpose == null) return lastUpload;

            if (DateTime.TryParse(generalPurpose.Heading2, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                lastUpload.LastUploadDate = date.ToString(DateFormat);
            }

            return lastUpload;
        }
    }
}