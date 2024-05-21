using Garaaz.Models;
using Garaaz.Models.Accounts;
using Garaaz.Models.Checkout;
using Garaaz.Models.DashboardOverview;
using Garaaz.Models.DashboardOverview.Cbo;
using Garaaz.Models.DashboardOverview.Collection;
using Garaaz.Models.DashboardOverview.Customer;
using Garaaz.Models.DashboardOverview.Inventory;
using Garaaz.Models.DashboardOverview.LoserAndGainer;
using Garaaz.Models.DashboardOverview.Outstanding;
using Garaaz.Models.DashboardOverview.Sale;
using Garaaz.Models.DashboardOverview.Wallet;
using Garaaz.Models.DistributorUpi;
using Garaaz.Models.Notifications;
using Garaaz.Models.Other;
using Garaaz.Models.Schemes;
using Garaaz.Models.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using SchemeMain = Garaaz.Models.DashboardOverview.Scheme.SchemeMain;

namespace Garaaz.Controllers
{
    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SystemController : ApiController
    {
        #region Variables
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public IRoleManagement _roleManagement;
        public IUserManagement _userManagement;
        private ApplicationRoleManager _roleManager;
        #endregion

        #region Construtor
        public SystemController()
        {
            if (_userManager == null)
            {
                setService();
            }
        }
        #endregion

        #region Uses Manger

        public ApplicationSignInManager SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    _signInManager = HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
                }
                return _signInManager;
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return _userManager;
            }
            private set
            {
                _userManager = value;
            }
        }

        public ApplicationRoleManager RoleManager
        {
            get
            {
                if (_roleManager == null)
                {
                    _roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
                }
                return _roleManager;
            }
            private set
            {
                _roleManager = value;
            }
        }

        #endregion

        #region Helper

        public void setService()
        {
            _userManagement = new UserManagement(UserManager, SignInManager, RoleManager);
            _roleManagement = new RoleManagement(UserManager, SignInManager, _userManagement, RoleManager);
        }

        #endregion

        #region GetUserRole
        public string GetUserRole()
        {
            return ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).OrderByDescending(o => o.Value).Select(c => c.Value).FirstOrDefault();
        }

        /// <summary>
        /// Get current logged in user's roles.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetUserRoles()
        {
            return ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).OrderByDescending(o => o.Value).Select(c => c.Value);
        }

        #endregion

        #region Auth Management
        [AllowAnonymous]
        [HttpPost]
        public ResultModel LoginUser(clsAuth model)
        {
            try
            {
                AuthManagement authManagement = new AuthManagement();
                var result = SignInManager.PasswordSignIn(model.Username, model.Password, false, shouldLockout: true);

                switch (result)
                {
                    case SignInStatus.Success:
                        var user = UserManager.FindByNameAsync(model.Username);
                        user.Wait();
                        if (!user.Result.EmailConfirmed)
                        {
                            return new ResultModel
                            {
                                Message = "Your account is not approved. Please contact administrator.",
                                ResultFlag = 0
                            };
                        }
                        else
                        {
                            var token = authManagement.GetToken(model.Username, model.Password);
                            if (string.IsNullOrEmpty(token.Error))
                            {
                                var general = new General();
                                RepoUsers repoUsers = new RepoUsers();
                                var userDetail = repoUsers.GetUserDetailByUserId(user.Result.Id);
                                token.UserId = user.Result.Id;
                                var roles = _userManager.GetRolesAsync(token.UserId);
                                roles.Wait();
                                token.Roles = roles.Result.OrderByDescending(o => o.ToString()).ToList();
                                token.Profile = general.CheckImageUrl(!string.IsNullOrEmpty(userDetail?.UserImage) ? userDetail?.UserImage : "/assets/images/NoPhotoAvailable.png");
                                token.FullName = repoUsers.getUserFullName(token.UserId, token.Roles);
                                //token.TempOrderId = userDetail.TempOrderId;
                                token.TempOrderId = 0;

                                Utils utils = new Utils();
                                RepoLogMaintain objLogMaintain = new RepoLogMaintain();

                                // save fcmToken
                                if (!string.IsNullOrEmpty(model.FCMToken))
                                {
                                    repoUsers.AddFCMToken(token.UserId, model.FCMToken);
                                }
                                var t = new System.Threading.Thread(() => objLogMaintain.SaveLoginDetail(new LoginTime
                                {
                                    UserId = token.UserId,
                                    CreatedDate = DateTime.Now,
                                    IpAddress = utils.GetIpUser()
                                }))
                                { IsBackground = true };
                                t.Start();

                                return new ResultModel
                                {
                                    Message = "Login successfully.",
                                    ResultFlag = 1,
                                    Data = token
                                };
                            }
                            else
                            {
                                return new ResultModel
                                {
                                    Message = "Your email address is not approved.",
                                    ResultFlag = 0
                                };
                            }
                        }
                    case SignInStatus.LockedOut:
                        return new ResultModel
                        {
                            Message = "Your are locked out. Please contact administrator.",
                            ResultFlag = 0
                        };
                    case SignInStatus.Failure:
                    default:
                        return new ResultModel
                        {
                            Message = "Username password do not match",
                            ResultFlag = 0
                        };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ResultModel GetTokenByRefreshToken(clsAuth model)
        {
            try
            {
                AuthManagement authManagement = new AuthManagement();

                var token = authManagement.GetTokenFromRefreshToken(model.RefreshToken);
                if (string.IsNullOrEmpty(token.Error))
                {

                    RepoUsers repoUsers = new RepoUsers();
                    token.UserId = model.UserId;

                    var roles = _userManager.GetRolesAsync(token.UserId);
                    roles.Wait();
                    token.Roles = roles.Result;
                    token.FullName = repoUsers.getUserFullName(token.UserId, token.Roles);
                    return new ResultModel
                    {
                        Message = "Login successfully.",
                        ResultFlag = 1,
                        Data = token
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = token.Error,
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception)
            {
                return new ResultModel
                {
                    Message = "Error",
                    ResultFlag = 0
                };
            }
        }

        #endregion

        #region Request OTP

        [AllowAnonymous]
        [HttpPost]
        public ResultModel RequestOtp(clsAuth modal)
        {
            try
            {
                var userId = _userManagement.GetUserIdByUsername(modal.Username);
                if (userId != null)
                {
                    RepoUsers UserOTP = new RepoUsers();
                    int Otp = UserOTP.GetOTP();

                    var result = UserOTP.addUpdateOtp(userId, Otp);
                    if (result == true)
                    {
                        return new ResultModel
                        {
                            Message = "OTP send to your mobile number",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Network error please try again later.",
                            ResultFlag = 0
                        };
                    }
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "This mobile number is not registered. Please try different mobile.",
                        ResultFlag = 0
                    };
                }
            }

            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region OTP Validate

        [AllowAnonymous]
        [HttpPost]
        public ResultModel OTPValidate(clsAuth modal)
        {
            ResultModel objResultModel = null;
            try
            {
                var userId = _userManagement.GetUserIdByUsername(modal.Username);
                RepoUsers getotp = new RepoUsers();
                int OTP = getotp.GetOTPByUserName(userId);
                if (Convert.ToInt32(modal.OTP) != OTP)
                {
                    objResultModel = new ResultModel
                    {

                        Message = "Please Enter Correct OTP",
                        ResultFlag = 0
                    };
                }
                else
                {
                    AuthManagement authManagement = new AuthManagement();
                    var user = UserManager.FindByNameAsync(modal.Username);
                    user.Wait();
                    if (!user.Result.EmailConfirmed)
                    {
                        return new ResultModel
                        {
                            Message = "Your account is not approved by administrator.",
                            ResultFlag = 0
                        };
                    }
                    else
                    {
                        RepoUsers repoUsers = new RepoUsers();
                        var userDetail = repoUsers.GetUserDetailByUserId(userId);
                        var token = authManagement.GetToken(modal.Username, Utils.Decrypt(userDetail.Password));
                        if (string.IsNullOrEmpty(token.Error))
                        {
                            var general = new General();
                            token.UserId = user.Result.Id;
                            var roles = _userManager.GetRolesAsync(token.UserId);
                            roles.Wait();
                            token.Roles = roles.Result.OrderByDescending(o => o.ToString()).ToList();
                            token.FullName = repoUsers.getUserFullName(token.UserId, token.Roles);
                            token.Profile = general.CheckImageUrl(!string.IsNullOrEmpty(userDetail?.UserImage) ? userDetail?.UserImage : "/assets/images/NoPhotoAvailable.png");
                            //token.TempOrderId = userDetail.TempOrderId;
                            token.TempOrderId = 0;
                            return new ResultModel
                            {
                                Message = "Login successfully.",
                                ResultFlag = 1,
                                Data = token
                            };
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = "Something goes wrong!! Please try again",
                                ResultFlag = 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
            return objResultModel;
        }
        #endregion

        #region User Management

        [HttpPost]
        public ResultModel ApproveUser(clsApproveUser model)
        {
            try
            {
                var result = _userManagement.SetApproved(model.UserId, model.Approved);
                if (result && model.Approved)
                {
                    return new ResultModel
                    {
                        Message = "User Approved Successfully.",
                        ResultFlag = 1
                    };
                }
                else if (result && !model.Approved)
                {
                    return new ResultModel
                    {
                        Message = "User UnApproved Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Something goes wrong!!.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel DeleteUser(clsApproveUser model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();
                _userManagement.LockUserAccount(model.UserId);
                var result = repoUsers.deleteUser(model.UserId);
                if (result)
                {

                    return new ResultModel
                    {
                        Message = "User Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel LockUser(clsApproveUser model)
        {
            try
            {
                var result = false;
                if (model.Locked)
                {
                    result = _userManagement.LockUserAccount(model.UserId);
                }
                else
                {
                    result = _userManagement.UnlockUserAccount(model.UserId);
                }

                if (result && model.Locked)
                {
                    return new ResultModel
                    {
                        Message = "User Locked Successfully.",
                        ResultFlag = 1
                    };
                }
                else if (result && !model.Locked)
                {
                    return new ResultModel
                    {
                        Message = "User UnLocked Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Something goes wrong!!.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ResultModel ChangePassword(clsApproveUser model)
        {
            try
            {
                var result = false;
                if (!string.IsNullOrEmpty(model.Code))
                {
                    result = _userManagement.ChangePassword(model.UserId, model.NewPassword, model.Code);
                }
                else
                {
                    result = _userManagement.ChangePassword(model.UserId, model.NewPassword);
                }

                // Update password of user
                var userDetails = new UserDetail
                {
                    UserId = model.UserId,
                    Password = Utils.Encrypt(model.NewPassword)
                };
                var repoUsers = new RepoUsers();
                repoUsers.UpdatePassword(userDetails);

                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Password Changed Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Password not changed. Password must be at least 6 characters. Password must have at least one non letter or digit character. Password must have at least one lowercase ('a'-'z'). Password must have at least one uppercase ('A'-'Z').",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel GetRoles()
        {
            try
            {
                return new ResultModel
                {
                    Data = _roleManagement.GetAllRoles(),
                    Message = "Data Found Successfully.",
                    ResultFlag = 1
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Add Distributor API
        [AllowAnonymous]
        [HttpPost]
        public ResultModel DistributorRegisterOrUpdate(clsDistributor model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();

                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        //Update user
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            IsDeleted = false,
                            UserId = model.UserId,
                            ConsPartyCode = model.EmployeeCode
                            //Password = Utils.Encrypt(model.Password)

                        };
                        var isExists = repoUsers.CheckEmpCodeExists(model.EmployeeCode, model.UserId);
                        if (isExists)
                        {
                            return new ResultModel
                            {
                                Message = $"{model.EmployeeCode} employee code already exists.",
                                ResultFlag = 0
                            };
                        }
                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);


                        if (resultDetails)
                        {
                            if (!model.IsFromDistributorUser)
                            {
                                var r = repoUsers.UpdateDistributor(new Distributor
                                {
                                    Address = model.Address,
                                    DistributorName = model.DistributorName,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude,
                                    City = model.City,
                                    State = model.State,
                                    Pincode = model.Pincode,
                                    LandlineNumber = model.LandlineNumber,
                                    Gender = model.Gender,
                                    Gstin = model.Gstin,
                                    FValue = model.FValue,
                                    MValue = model.MValue,
                                    SValue = model.SValue,
                                    Company = model.Company,
                                    UPIID = model.UPIID,
                                    BankName = model.BankName,
                                    IfscCode = model.IFSCCode,
                                    AccountNumber = model.AccNo,
                                }, model.UserId, model.BrandIds);
                            }
                            return new ResultModel
                            {
                                Message = "Registration Successfully Updated.",
                                ResultFlag = 1
                            };
                        }
                        return new ResultModel
                        {
                            Message = "User details updated successfully.",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        string UserId = "";
                        var roles = new[] { Garaaz.Models.Constants.Distributor };

                        //Register user
                        var result = _userManagement.RegisterUser(model.PhoneNumber, model.Email, model.Password, roles, model.IsApproved, model.EmployeeCode, out UserId);
                        if (result.Succeeded)
                        {
                            var userDetails = new UserDetail
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IsDeleted = false,
                                UserId = UserId,
                                ConsPartyCode = model.EmployeeCode,
                                Password = Utils.Encrypt(model.Password)
                            };

                            var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                            if (resultDetails)
                            {
                                var r = repoUsers.SaveDistributor(new Distributor
                                {
                                    Address = model.Address,
                                    DistributorName = model.DistributorName,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude,
                                    City = model.City,
                                    State = model.State,
                                    Pincode = model.Pincode,
                                    LandlineNumber = model.LandlineNumber,
                                    Gender = model.Gender,
                                    Gstin = model.Gstin,
                                    FValue = model.FValue,
                                    MValue = model.MValue,
                                    SValue = model.SValue,
                                    Company = model.Company,
                                    UPIID = model.UPIID,
                                    BankName = model.BankName,
                                    IfscCode = model.IFSCCode,
                                    AccountNumber = model.AccNo,
                                }, UserId, model.BrandIds);

                                return new ResultModel
                                {
                                    Message = "Registration Successfully.",
                                    ResultFlag = 1
                                };
                            }
                            else
                            {
                                return new ResultModel
                                {
                                    Message = "Registration failed!!",
                                    ResultFlag = 0
                                };
                            }
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = result.Errors.Count() > 0 ? result.Errors.FirstOrDefault() : "Registration failed!!",
                                ResultFlag = 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Add Distributor Users
        [AllowAnonymous]
        [HttpPost]
        public ResultModel DistributorUsersRegisterOrUpdate(clsDistributorUserInfo model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }

                var repoUsers = new RepoUsers();

                #region Update user if user id available

                if (!string.IsNullOrEmpty(model.UserId))
                {
                    var empCodeExist = repoUsers.CheckEmpCodeExists(model.EmployeeCode, model.UserId, Convert.ToString(model.DistributorId));
                    if (empCodeExist)
                    {
                        return new ResultModel
                        {
                            Message = $"{model.EmployeeCode} employee code already exists.",
                            ResultFlag = 0
                        };
                    }

                    // Update user
                    var userDetails = new UserDetail
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        IsDeleted = false,
                        UserId = model.UserId,
                        ConsPartyCode = model.EmployeeCode,
                        Designations = model.Designations
                        //Password = Utils.Encrypt(model.Password)
                    };

                    var userUpdated = repoUsers.AddOrUpdateUsers(userDetails);
                    if (userUpdated)
                    {
                        var distInfoUpdated = repoUsers.UpdateDistributorUserInfo(new DistributorUserInfo
                        {
                            Address = model.Address,
                            Latitude = model.Latitude,
                            Longitude = model.Longitude,
                            UserId = model.UserId
                        });

                        return new ResultModel
                        {
                            Data = model.UserId,
                            Message = distInfoUpdated ? "Successfully updated distributor user info!" : "Failed to update distributor user info.",
                            ResultFlag = distInfoUpdated ? 1 : 0
                        };
                    }

                    return new ResultModel
                    {
                        Data = model.UserId,
                        Message = "User details updated successfully!",
                        ResultFlag = 1
                    };
                }

                #endregion

                // Keep default role as users           
                string[] roles;
                if (string.IsNullOrEmpty(model.Role))
                {
                    roles = new[] { Models.Constants.Users };
                }
                else
                {
                    roles = model.Role == Models.Constants.RoIncharge
                        ? new[] { Models.Constants.Users, model.Role, Models.Constants.DistributorOutlets }
                        : new[] { Models.Constants.Users, model.Role };
                }

                // Register user
                var result = _userManagement.RegisterUser(model.PhoneNumber, model.Email, model.Password, roles, true, model.EmployeeCode, out string userId, Convert.ToString(model.DistributorId));
                if (result.Succeeded)
                {
                    var userDetails = new UserDetail
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        IsDeleted = false,
                        UserId = userId,
                        ConsPartyCode = model.EmployeeCode,
                        Password = Utils.Encrypt(model.Password),
                        Designations = model.Designations
                    };

                    var userAdded = repoUsers.AddOrUpdateUsers(userDetails);
                    if (userAdded)
                    {
                        var isSave = repoUsers.SaveDistributorUserInfo(new DistributorUserInfo
                        {
                            Address = model.Address,
                            Latitude = model.Latitude,
                            Longitude = model.Longitude,
                            UserId = userId
                        }, model.DistributorId);

                        return new ResultModel
                        {
                            Data = userId,
                            Message = isSave != false ? "Registration Successfully!" : "Registration failed.",
                            ResultFlag = isSave != false ? 1 : 0
                        };
                    }

                    return new ResultModel
                    {
                        Message = "Registration failed.",
                        ResultFlag = 0
                    };
                }

                return new ResultModel
                {
                    Message = result.Errors.Any() ? result.Errors.FirstOrDefault() : "Registration failed.",
                    ResultFlag = 0
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Distributor

        [HttpPost]
        public ResultModel GetDistributorUserById(clsDistributorUserInfo model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();

                var user = repoUsers.getUserById(model.UserId);

                if (user != null)
                {
                    var role = user.AspNetUser.AspNetRoles.FirstOrDefault().Name;
                    dynamic distributor;
                    if (role == Garaaz.Models.Constants.Distributor)
                    {
                        distributor = user.AspNetUser.DistributorUsers.FirstOrDefault().Distributor;
                    }
                    else
                    {
                        distributor = user.AspNetUser.DistributorUserInfoes.FirstOrDefault();
                    }
                    return new ResultModel
                    {
                        Message = "User Found",
                        ResultFlag = 1,
                        Data = new clsDistributorUserInfo
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.AspNetUser.UserName,
                            UserId = model.UserId,
                            Email = user.AspNetUser.Email,
                            Address = distributor.Address,
                            Latitude = distributor.Latitude,
                            Longitude = distributor.Longitude,
                            Role = role,
                            EmployeeCode = user.ConsPartyCode,
                            Designations = user.Designations
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }

        }

        [HttpPost]
        public ResultModel GetDistributorUserByRoleAndId(clsDistributorUserInfo model)
        {
            try
            {
                var repoUsers = new RepoUsers();
                var user = repoUsers.GetUserById(model.UserId, model.Role);
                var roIncharge = "";
                if (model.Role == Models.Constants.SalesExecutive)
                {
                    roIncharge = repoUsers.GetRoInchargeBySalesExecutive(model.UserId);
                }
                if (user != null)
                {
                    var distributor = user.AspNetUser.DistributorUserInfoes.FirstOrDefault();
                    return new ResultModel
                    {
                        Message = "User Found",
                        ResultFlag = 1,
                        Data = new clsDistributorUserInfo
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.AspNetUser.UserName,
                            UserId = model.UserId,
                            Email = user.AspNetUser.Email,
                            Address = distributor.Address,
                            Latitude = distributor.Latitude,
                            Longitude = distributor.Longitude,
                            Role = model.Role,
                            EmployeeCode = user.ConsPartyCode,
                            Designations = user.Designations,
                            RoInchargeId = roIncharge,
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User not found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }

        }

        #endregion

        #region Add WorkShop API

        [AllowAnonymous]
        [HttpPost]
        public ResultModel WorkshopRegisterOrUpdate(clsWorkshop model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();

                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        //Update user
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            IsDeleted = false,
                            UserId = model.UserId,
                            ConsPartyCode = model.EmployeeCode

                            //Password = Utils.Encrypt(model.Password)
                        };
                        var isExists = repoUsers.CheckEmpCodeExists(model.EmployeeCode, model.UserId, Convert.ToString(model.DistributorId));
                        if (isExists)
                        {
                            return new ResultModel
                            {
                                Message = $"{model.EmployeeCode} employee code already exists.",
                                ResultFlag = 0
                            };
                        }
                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                        if (resultDetails)
                        {
                            if (!model.IsFromDistributorUser)
                            {
                                var r = repoUsers.UpdateWorkShop(new WorkShop
                                {
                                    Address = model.Address,
                                    WorkShopName = model.WorkShopName,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude,
                                    City = model.City,
                                    State = model.State,
                                    Pincode = model.Pincode,
                                    Gender = model.Gender,
                                    LandlineNumber = model.LandlineNumber,
                                    Gstin = model.Gstin,
                                    CriticalOutstandingDays = model.CriticalOutstandingDays,
                                    outletId = model.OutletId,
                                    CategoryName = model.CategoryName,
                                    CreditLimit = model.CreditLimit,
                                    BillingName = model.BillingName,
                                    YearOfEstablishment = model.YearOfEstablishment,
                                    Type = model.WorkshopType,
                                    Make = model.Make,
                                    JobsUndertaken = model.JobsUndertaken,
                                    Premise = model.Premise,
                                    GaraazArea = model.GaraazArea,
                                    TwoPostLifts = model.TwoPostLifts,
                                    WashingBay = model.WashingBay,
                                    PaintBooth = model.PaintBooth,
                                    ScanningAndToolKit = model.ScanningAndToolKit,
                                    TotalOwners = model.TotalOwners,
                                    TotalChiefMechanics = model.TotalChiefMechanics,
                                    TotalEmployees = model.TotalEmployees,
                                    MonthlyVehiclesServiced = model.MonthlyVehiclesServiced,
                                    MonthlyPartPurchase = model.MonthlyPartPurchase,
                                    MonthlyConsumablesPurchase = model.MonthlyConsumablesPurchase,
                                    WorkingHours = model.WorkingHours,
                                    WeeklyOffDay = model.WeeklyOffDay,
                                    Website = model.Website,
                                    InsuranceCompanies = model.InsuranceCompanies,
                                    IsMoreThanOneBranch = model.IsMorethanOneBranch
                                }, model.UserId, model.DistributorId);
                            }
                            return new ResultModel
                            {
                                Message = "Registration Successfully Updated.",
                                ResultFlag = 1
                            };
                        }
                        return new ResultModel
                        {
                            Message = "User details updated successfully.",
                            ResultFlag = 1
                        };
                    }

                    string userId;
                    var roles = new[] { Garaaz.Models.Constants.Users, Garaaz.Models.Constants.Workshop };

                    if (model.PhoneNumber.Trim().Contains(' ') == true)
                    {
                        model.PhoneNumber = model.PhoneNumber.Trim().Split(' ')[0];
                    }
                    else
                    {
                        model.PhoneNumber = model.PhoneNumber.Trim().Contains(',') ? model.PhoneNumber.Trim().Split(',')[0] : model.PhoneNumber.Trim();
                    }

                    //Register user
                    var suffixNum = GetMaxSuffixForMobile(model.PhoneNumber);
                    var userName = suffixNum > 0 ? $"{model.PhoneNumber}_{suffixNum + 1}" : model.PhoneNumber;
                    var result = _userManagement.RegisterUser(userName, model.Email, model.Password, roles, model.IsApproved, model.EmployeeCode, out userId);

                    if (result.Errors.FirstOrDefault() == "Name " + userName + " is already taken.")
                    {
                        // check if this workshop already register with this distributor then show error otherwise register workshop for this distributor
                        var ru = new RepoUsers();
                        if (!ru.WorkshopRegisterWithDistributorId(userName, model.DistributorId))
                        {
                            for (var i = suffixNum; i < 1000; i++)
                            {
                                userName = model.PhoneNumber + "_" + i;

                                result = _userManagement.RegisterUser(userName, model.Email, model.Password, roles, model.IsApproved, model.EmployeeCode, out userId);
                                if (result.Errors.FirstOrDefault() == "Name " + userName + " is already taken.") { continue; }

                                i = 10001;
                            }
                        }
                    }

                    if (result.Succeeded)
                    {
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            IsDeleted = false,
                            UserId = userId,
                            Password = Utils.Encrypt(model.Password),
                            ConsPartyCode = model.EmployeeCode
                        };

                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                        if (resultDetails)
                        {
                            var r = repoUsers.SaveWorkShop(new WorkShop
                            {
                                Address = model.Address,
                                WorkShopName = model.WorkShopName,
                                Latitude = model.Latitude,
                                Longitude = model.Longitude,
                                City = model.City,
                                State = model.State,
                                Pincode = model.Pincode,
                                Gender = model.Gender,
                                LandlineNumber = model.LandlineNumber,
                                Gstin = model.Gstin,
                                CriticalOutstandingDays = model.CriticalOutstandingDays,
                                outletId = model.OutletId,
                                CategoryName = model.CategoryName,
                                CreditLimit = model.CreditLimit,
                                BillingName = model.BillingName,
                                YearOfEstablishment = model.YearOfEstablishment,
                                Type = model.WorkshopType,
                                Make = model.Make,
                                JobsUndertaken = model.JobsUndertaken,
                                Premise = model.Premise,
                                GaraazArea = model.GaraazArea,
                                TwoPostLifts = model.TwoPostLifts,
                                WashingBay = model.WashingBay,
                                PaintBooth = model.PaintBooth,
                                ScanningAndToolKit = model.ScanningAndToolKit,
                                TotalOwners = model.TotalOwners,
                                TotalChiefMechanics = model.TotalChiefMechanics,
                                TotalEmployees = model.TotalEmployees,
                                MonthlyVehiclesServiced = model.MonthlyVehiclesServiced,
                                MonthlyPartPurchase = model.MonthlyPartPurchase,
                                MonthlyConsumablesPurchase = model.MonthlyConsumablesPurchase,
                                WorkingHours = model.WorkingHours,
                                WeeklyOffDay = model.WeeklyOffDay,
                                Website = model.Website,
                                InsuranceCompanies = model.InsuranceCompanies,
                                IsMoreThanOneBranch = model.IsMorethanOneBranch,
                                MobileNumber = model.PhoneNumber
                            }, userId, model.DistributorId);

                            return new ResultModel
                            {
                                Message = "Registration Successfully.",
                                ResultFlag = 1
                            };
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = "Registration failed!!",
                                ResultFlag = 0
                            };
                        }
                    }
                    //  else
                    // {
                    return new ResultModel
                    {
                        Message = result.Errors.Count() > 0 ? result.Errors.FirstOrDefault() : "Registration failed!!",
                        ResultFlag = 0
                    };
                    // }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        private static int GetMaxSuffixForMobile(string mobile)
        {
            var db = new garaazEntities();
            var users = db.AspNetUsers.Where(a => a.UserName.Contains(mobile)).Select(a => a.UserName).ToList();

            var numbers = new List<int> { 0 };
            foreach (var user in users)
            {
                var phoneAndSuffix = user.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                if (phoneAndSuffix.Length < 2) continue;

                if (int.TryParse(phoneAndSuffix[1], out var result))
                {
                    numbers.Add(result);
                }
            }

            return numbers.Max();
        }

        #endregion

        #region Add WorkShop Users

        /// <summary>
        /// Register or update the workshop user.
        /// </summary>
        /// <param name="model">The clsWorkshop model.</param>
        /// <returns>Return ResultModel object.</returns>
        [AllowAnonymous]
        [HttpPost]
        public ResultModel WorkshopUserRegisterOrUpdate(clsWorkshop model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();

                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        // Update user details
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            IsDeleted = false,
                            UserId = model.UserId,
                            ConsPartyCode = model.EmployeeCode,
                            Designations = model.Designations
                            //Password = Utils.Encrypt(model.Password)
                        };
                        var isExists = repoUsers.CheckEmpCodeExists(model.EmployeeCode, model.UserId, Convert.ToString(model.DistributorId));
                        if (isExists)
                        {
                            return new ResultModel
                            {
                                Message = $"{model.EmployeeCode} employee code already exists.",
                                ResultFlag = 0
                            };
                        }
                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);

                        if (resultDetails)
                        {
                            // Update workshop user's address details
                            var userInfo = new DistributorUserInfo
                            {
                                Address = model.Address,
                                Latitude = model.Latitude,
                                Longitude = model.Longitude
                            };
                            var r = repoUsers.UpdateWorkshopUsers(userInfo, model.UserId);

                            return new ResultModel
                            {
                                Message = "Registration Successfully Updated.",
                                ResultFlag = 1
                            };
                        }
                        return new ResultModel
                        {
                            Message = "User details updated successfully.",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        string UserId = "";
                        var roles = new[] { Garaaz.Models.Constants.Users, Garaaz.Models.Constants.WorkshopUsers };

                        //Register user
                        var result = _userManagement.RegisterUser(model.PhoneNumber, model.Email, model.Password, roles, true, model.EmployeeCode, out UserId);
                        if (result.Succeeded)
                        {
                            var userDetails = new UserDetail
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IsDeleted = false,
                                UserId = UserId,
                                Password = Utils.Encrypt(model.Password),
                                ConsPartyCode = model.EmployeeCode,
                                Designations = model.Designations
                            };

                            var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                            if (resultDetails)
                            {
                                //DistributorUserInfo
                                var userInfo = new DistributorUserInfo
                                {
                                    Address = model.Address,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude,
                                    UserId = UserId
                                };

                                repoUsers.SaveWorkshopUsers(userInfo, UserId, model.WorkshopId);

                                return new ResultModel
                                {
                                    Message = "Registration Successfully.",
                                    ResultFlag = 1
                                };
                            }
                            else
                            {
                                return new ResultModel
                                {
                                    Message = "Registration failed!!",
                                    ResultFlag = 0
                                };
                            }
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = result.Errors.Count() > 0 ? result.Errors.FirstOrDefault() : "Registration failed!!",
                                ResultFlag = 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel SaveDistributorForWorkshop(ClsNewWorkshop clsNewWorkshop)
        {
            var refUserId = clsNewWorkshop.RefUserId;
            var distributorIds = clsNewWorkshop.DistributorId;
            var workshopId = clsNewWorkshop.WorkshopId;

            if (distributorIds == null)
            {
                return new ResultModel
                {
                    Message = "No distributor was selected.",
                    ResultFlag = 0
                };
            }

            garaazEntities db = new garaazEntities();
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    bool saved;

                    // First, delete all existing records matching with workshopId and userId
                    var dws = db.DistributorWorkShops.Where(d => d.WorkShopId == workshopId && d.UserId == refUserId);
                    if (dws != null)
                    {
                        db.DistributorWorkShops.RemoveRange(dws);
                        db.SaveChanges();
                    }

                    // Second, save new distributor workshops
                    foreach (var di in distributorIds)
                    {
                        var dwsToAdd = new DistributorWorkShop
                        {
                            DistributorId = di,
                            WorkShopId = workshopId,
                            UserId = refUserId
                        };
                        db.DistributorWorkShops.Add(dwsToAdd);
                    }
                    saved = db.SaveChanges() > 0;

                    transaction.Commit();

                    if (saved)
                    {
                        return new ResultModel
                        {
                            Message = "Distributor for workshop saved successfully.",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Failed to save the distributor for workshop.",
                            ResultFlag = 0
                        };
                    }

                }
                catch (Exception exc)
                {
                    transaction.Rollback();

                    return new ResultModel
                    {
                        Message = exc.Message,
                        ResultFlag = 0
                    };
                }
            }
        }


        #endregion

        #region Get WorkShop

        [HttpPost]
        public ResultModel GetWorkshopUserById(clsWorkshop model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();

                var user = repoUsers.getUserById(model.UserId);

                if (user != null)
                {
                    var role = user.AspNetUser.AspNetRoles.FirstOrDefault().Name;
                    dynamic workshop;
                    if (role == Garaaz.Models.Constants.Workshop)
                    {
                        workshop = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop;
                    }
                    else
                    {
                        workshop = user.AspNetUser.DistributorUserInfoes.FirstOrDefault();
                    }
                    return new ResultModel
                    {
                        Message = "User Found",
                        ResultFlag = 1,
                        Data = new clsWorkshop
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.AspNetUser.UserName,
                            UserId = model.UserId,
                            Email = user.AspNetUser.Email,
                            //Address = workshop != null ? workshop.Address : "",
                            //Latitude = workshop != null ? workshop.Latitude : "",
                            //Longitude = workshop != null ? workshop.Longitude : "",
                            Address = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Address : "",
                            Latitude = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Latitude : "",
                            Longitude = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Longitude : "",
                            Role = role,
                            WorkShopName = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WorkShopName,
                            City = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.City,
                            State = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.State,
                            Pincode = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Pincode,
                            Gender = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Gender,
                            LandlineNumber = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.LandlineNumber,
                            Gstin = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Gstin,
                            EmployeeCode = user.ConsPartyCode,
                            //CriticalOutstandingDays= role == Garaaz.Models.Constants.Workshop ? (workshop != null ? (workshop.CriticalOutstandingDays ?? 0) : 0) : 0,
                            CriticalOutstandingDays = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.CriticalOutstandingDays ?? 0) : 0,
                            OutletId = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.outletId ?? 0) : 0,
                            CategoryName = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.CategoryName != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.CategoryName : "",
                            CreditLimit = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.CreditLimit ?? 0) : 0,
                            BillingName = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.BillingName != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.BillingName : "",
                            YearOfEstablishment = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.YearOfEstablishment != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.YearOfEstablishment ?? 0) : 0,
                            WorkshopType = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Type != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Type : "",
                            Make = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Make != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Make : "",
                            JobsUndertaken = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.JobsUndertaken != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.JobsUndertaken : "",
                            Premise = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Premise != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Premise : "",
                            GaraazArea = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.GaraazArea != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.GaraazArea : "",
                            TwoPostLifts = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TwoPostLifts != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TwoPostLifts ?? 0) : 0,
                            WashingBay = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WashingBay != null ? Convert.ToBoolean(user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WashingBay) : false,
                            PaintBooth = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.PaintBooth != null ? Convert.ToBoolean(user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.PaintBooth) : false,
                            ScanningAndToolKit = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.ScanningAndToolKit != null ? Convert.ToBoolean(user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.ScanningAndToolKit) : false,
                            TotalOwners = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TotalOwners != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TotalOwners ?? 0) : 0,
                            TotalChiefMechanics = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TotalChiefMechanics != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TotalChiefMechanics ?? 0) : 0,
                            TotalEmployees = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TotalEmployees != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.TotalEmployees ?? 0) : 0,
                            MonthlyVehiclesServiced = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.MonthlyVehiclesServiced != null ? (user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.MonthlyVehiclesServiced ?? 0) : 0,
                            MonthlyPartPurchase = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.MonthlyPartPurchase != null ? Convert.ToDecimal(user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.MonthlyPartPurchase) : 0,
                            MonthlyConsumablesPurchase = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.MonthlyConsumablesPurchase != null ? Convert.ToDecimal(user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.MonthlyConsumablesPurchase) : 0,
                            WorkingHours = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WorkingHours != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WorkingHours : "",
                            WeeklyOffDay = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WeeklyOffDay != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.WeeklyOffDay : "",
                            Website = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Website != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.Website : "",
                            InsuranceCompanies = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.InsuranceCompanies != null ? user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.InsuranceCompanies : "",
                            IsMorethanOneBranch = user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.IsMoreThanOneBranch != null ? Convert.ToBoolean(user.AspNetUser.DistributorWorkShops.FirstOrDefault().WorkShop.IsMoreThanOneBranch) : false,
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }


        #endregion

        #region Get WorkShop User
        /// <summary>
        /// Get workshop's users by user id.
        /// </summary>
        /// <param name="model">The clsDistributorUserInfo model.</param>
        /// <returns>Returns result model.</returns>
        [HttpPost]
        public ResultModel GetWorkshopsUserById(clsDistributorUserInfo model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();

                var user = repoUsers.getUserById(model.UserId);

                if (user != null)
                {
                    var userInfo = user.AspNetUser.DistributorUserInfoes.FirstOrDefault();
                    return new ResultModel
                    {
                        Message = "User Found",
                        ResultFlag = 1,
                        Data = new clsDistributorUserInfo
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.AspNetUser.UserName,
                            UserId = model.UserId,
                            Email = user.AspNetUser.Email,
                            Address = userInfo != null ? userInfo.Address : string.Empty,
                            Latitude = userInfo != null ? userInfo.Latitude : string.Empty,
                            Longitude = userInfo != null ? userInfo.Longitude : string.Empty,
                            EmployeeCode = user.ConsPartyCode,
                            Designations = user.Designations
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }

        }

        #endregion

        #region Add Outlet And Outlet Users
        [AllowAnonymous]
        [HttpPost]
        public ResultModel OutletUserRegisterOrUpdate(clsOutlet model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();

                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        // Update user details
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            IsDeleted = false,
                            UserId = model.UserId,
                            ConsPartyCode = model.EmployeeCode,
                            Designations = model.Designations
                            //Password = Utils.Encrypt(model.Password)
                        };
                        var isExists = repoUsers.CheckEmpCodeExists(model.EmployeeCode, model.UserId, Convert.ToString(model.DistributorId));
                        if (isExists)
                        {
                            return new ResultModel
                            {
                                Message = $"{model.EmployeeCode} employee code already exists.",
                                ResultFlag = 0
                            };
                        }
                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);

                        if (resultDetails)
                        {
                            if (model.DistributorId > 0)
                            {
                                // Update outlet
                                var r = repoUsers.UpdateOutlet(new Outlet
                                {
                                    Address = model.Address,
                                    OutletName = model.OutletName,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude,
                                    OutletCode = model.OutletCode
                                }, model.UserId, model.DistributorId);
                            }
                            else
                            {
                                // Update outlet user's address details
                                var userInfo = new DistributorUserInfo
                                {
                                    Address = model.Address,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude
                                };
                                var r = repoUsers.UpdateOutletUsers(userInfo, model.UserId);
                            }

                            return new ResultModel
                            {
                                Message = "Registration Successfully Updated.",
                                ResultFlag = 1
                            };
                        }
                        return new ResultModel
                        {
                            Message = "User details updated successfully.",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        string UserId = "";
                        var roles = model.DistributorId > 0 ? new[] { Garaaz.Models.Constants.DistributorOutlets } : new[] { Garaaz.Models.Constants.Users, Garaaz.Models.Constants.OutletUsers };

                        //Register user
                        var result = _userManagement.RegisterUser(model.PhoneNumber, model.Email, model.Password, roles, true, model.EmployeeCode, out UserId);
                        if (result.Succeeded)
                        {
                            var userDetails = new UserDetail
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IsDeleted = false,
                                UserId = UserId,
                                Password = Utils.Encrypt(model.Password),
                                ConsPartyCode = model.EmployeeCode,
                                Designations = model.Designations
                            };

                            var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                            if (resultDetails)
                            {
                                if (model.DistributorId > 0)
                                {
                                    var r = repoUsers.SaveOutlet(new Outlet
                                    {
                                        Address = model.Address,
                                        OutletName = model.OutletName,
                                        Latitude = model.Latitude,
                                        Longitude = model.Longitude,
                                        OutletCode = model.OutletCode
                                    }, UserId, model.DistributorId);
                                }
                                else
                                {
                                    //DistributorUserInfo
                                    var userInfo = new DistributorUserInfo
                                    {
                                        Address = model.Address,
                                        Latitude = model.Latitude,
                                        Longitude = model.Longitude,
                                        UserId = UserId
                                    };

                                    repoUsers.SaveOutletUsers(userInfo, UserId, model.OutletId);
                                }

                                return new ResultModel
                                {
                                    Message = "Registration Successfully.",
                                    ResultFlag = 1
                                };
                            }
                            else
                            {
                                return new ResultModel
                                {
                                    Message = "Registration failed!!",
                                    ResultFlag = 0
                                };
                            }
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = result.Errors.Count() > 0 ? result.Errors.FirstOrDefault() : "Registration failed!!",
                                ResultFlag = 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get,Save & Delete Outlet
        [HttpPost]
        public ResultModel GetDistributorOutletById(OutletModel model)
        {
            try
            {
                var ro = new RepoOutlet();
                var outlet = ro.GetOutlet(model.OutletId);

                if (outlet != null)
                {
                    return new ResultModel
                    {
                        Message = "Outlet found",
                        ResultFlag = 1,
                        Data = new OutletModel
                        {
                            OutletId = outlet.OutletId,
                            OutletName = outlet.OutletName,
                            Address = outlet.Address,
                            Latitude = outlet.Latitude,
                            Longitude = outlet.Longitude,
                            OutletCode = outlet.OutletCode
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Outlet not found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }

        }

        public ResultModel OutletRegisterOrUpdate(OutletModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }

                var ro = new RepoOutlet();
                var tuple = ro.SaveOrUpdateOutlet(model);

                return new ResultModel
                {
                    Message = tuple.Item2,
                    ResultFlag = tuple.Item1 ? 1 : 0
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData?.Values["controller"].ToString(), this.ControllerContext.RouteData?.Values["action"].ToString(), ex);
            }
        }

        [HttpPost]
        public ResultModel DeleteOutlet(OutletModel model)
        {
            try
            {
                var ro = new RepoOutlet();
                var oDeleted = ro.DeleteOutlet(model.OutletId);
                return new ResultModel
                {
                    Message = oDeleted ? "Outlet deleted successfully!" : "Failed to delete outlet",
                    ResultFlag = oDeleted ? 1 : 0
                };
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException == null ? ex.Message : ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message;
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), msg, ex.StackTrace);
            }
        }


        #endregion

        #region Get Outlet User
        /// <summary>
        /// Get outlet's users by user id.
        /// </summary>
        /// <param name="model">The clsDistributorUserInfo model.</param>
        /// <returns>Returns result model.</returns>
        [HttpPost]
        public ResultModel GetOutletsUserById(clsDistributorUserInfo model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();

                var user = repoUsers.getUserById(model.UserId);

                if (user != null)
                {
                    var userInfo = user.AspNetUser.DistributorUserInfoes.FirstOrDefault();
                    return new ResultModel
                    {
                        Message = "User Found",
                        ResultFlag = 1,
                        Data = new clsDistributorUserInfo
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.AspNetUser.UserName,
                            UserId = model.UserId,
                            Email = user.AspNetUser.Email,
                            Address = userInfo != null ? userInfo.Address : string.Empty,
                            Latitude = userInfo != null ? userInfo.Latitude : string.Empty,
                            Longitude = userInfo != null ? userInfo.Longitude : string.Empty,
                            EmployeeCode = user.ConsPartyCode,
                            Designations = user.Designations
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }

        }

        #endregion

        #region Get WorkShop List
        [HttpPost]
        public ResultModel WorkShopList(RequestWorkshop obj)
        {
            List<ResponseWorkshop> listResponse = null;
            try
            {
                if (!string.IsNullOrEmpty(obj.searchText))
                {
                    RepoUsers repoUsers = new RepoUsers();
                    listResponse = repoUsers.getWorkshopByText(obj);
                    if (listResponse.Count > 0)
                    {
                        return new ResultModel
                        {
                            Message = "Workshop found",
                            ResultFlag = 1,
                            Data = listResponse
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "No Workshop found",
                            ResultFlag = 0,
                            Data = null
                        };
                    }
                }
                return new ResultModel
                {
                    Message = "Filter value can not be Empty",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Save WorkShop Distributors
        [HttpPost]
        public ResultModel AddWorkShopDistributorUser(DistributorWorkshopUser model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    RepoUsers repoUsers = new RepoUsers();
                    bool status = repoUsers.SaveWorkShopDistributorUsers(new DistributorWorkShop
                    {
                        DistributorId = model.DistributorId,
                        WorkShopId = model.WorkshopId
                    });
                    if (status == true)
                    {
                        return new ResultModel
                        {
                            Message = "Workshop User sucessfully save",
                            ResultFlag = 1,
                            Data = status
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Workshop  User  already exists or not saved",
                            ResultFlag = 0,
                            Data = status
                        };
                    }
                }
                return new ResultModel
                {
                    Message = "Invalid data",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Feature List
        [HttpPost]
        public ResultModel GetAllFeatures(IEnumerable<string> roles)
        {
            List<clsFeatures> listResponse = null;
            try
            {
                var repoUserPermission = new RepoUserFeaturePermission();
                var allFeatures = repoUserPermission.GetAllFeatures();
                var featuresByRole = repoUserPermission.GetFeatureByRole(roles, allFeatures);

                listResponse = featuresByRole.Select(x => new clsFeatures
                {
                    FeatureId = x.FeatureId,
                    FeatureName = x.FeatureName,
                    IsDefault = x.IsDefault
                }).ToList();

                if (listResponse.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Features found",
                        ResultFlag = 1,
                        Data = listResponse
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "No Features found",
                        ResultFlag = 0,
                        Data = null
                    };
                }

            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region User Permision Page APIs
        public ResultModel GetAllUserFeatures(RequestUserFeature model)
        {
            try
            {
                RepoUserFeaturePermission repoUserPermission = new RepoUserFeaturePermission();
                var data = repoUserPermission.getUserFeatures(model.UserId).ToList();
                var featureIds = "";
                foreach (var item in data)
                {
                    featureIds += featureIds.Length > 0 ? "," + item.FeatureId.ToString() : item.FeatureId.ToString();
                }
                clsAddUserFeature obj = new clsAddUserFeature
                {
                    FeatureIds = featureIds,
                    UserId = model.UserId
                };
                return new ResultModel
                {
                    Message = "",
                    ResultFlag = 1,
                    Data = obj
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel SaveUserFeatures(clsAddUserFeature model)
        {
            try
            {
                RepoUserFeaturePermission repoUsersPermission = new RepoUserFeaturePermission();
                string[] FeatureIdArr = model.FeatureIds != null ? model.FeatureIds.Split(',') : new string[0];
                List<UserFeature> lstUserFeature = new List<UserFeature>();
                foreach (var fid in FeatureIdArr)
                {
                    try
                    {
                        lstUserFeature.Add(new UserFeature
                        {
                            FeatureId = Convert.ToInt32(fid),
                            UserId = model.UserId
                        });
                    }
                    catch { }
                }
                bool status = repoUsersPermission.SaveUserFeatures(lstUserFeature, model.UserId);
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "User's Feature Updated Sucessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User's Feature Updated Sucessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Features Page APIs

        #region Get Features List
        public List<Feature> GetFeatureList()
        {
            RepoUserFeaturePermission repoUserFeaturePermission = new RepoUserFeaturePermission();
            return repoUserFeaturePermission.GetAllFeatures();
        }
        #endregion

        #region Save Features
        [HttpPost]
        public ResultModel SaveFeatures(clsFeatures model)
        {
            try
            {

                RepoUserFeaturePermission repoUsersPermission = new RepoUserFeaturePermission();
                bool status = repoUsersPermission.SaveFeatures(new Feature
                {
                    FeatureId = model.FeatureId,
                    FeatureName = model.FeatureName,
                    FeatureValue = model.FeatureValue,
                    IsDefault = model.IsDefault
                });
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Feature's Updated Sucessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User's Feature Updated Sucessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Features Name by User Id
        public ResultModel GetFeaturesNamebyUserId(int FeatureId)
        {
            RepoUserFeaturePermission repoUserFeaturePermission = new RepoUserFeaturePermission();
            clsFeatures obj = null;
            var data = repoUserFeaturePermission.getFeaturesNamebyId(FeatureId);
            obj = new clsFeatures
            {
                FeatureId = data.FeatureId,
                FeatureName = data.FeatureName,
                FeatureValue = data.FeatureValue,
                IsDefault = data.IsDefault,
            };
            if (obj != null)
            {
                return new ResultModel
                {
                    Message = "Features found",
                    ResultFlag = 1,
                    Data = obj
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "No Features found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Delete Features
        [HttpPost]
        public ResultModel DeleteFeatures(clsFeatures model)
        {
            try
            {
                RepoUserFeaturePermission repoUserFeaturePermission = new RepoUserFeaturePermission();
                var result = repoUserFeaturePermission.DeleteFeatures(model.FeatureId);
                if (result)
                {

                    return new ResultModel
                    {
                        Message = "User Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Notifications
        // POST: api/System/
        [HttpPost]
        public ResultModel GetNotifications(ClsNotification model)
        {
            try
            {
                var nu = new NotificationUtil();
                return new ResultModel
                {
                    Data = nu.GetNotifications(model.UserId, model.NumberOfNotification, model.GetAllNotification),
                    Message = "Notifications found successfully.",
                    ResultFlag = 1
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        // POST: api/System        
        [HttpPost]
        public ResultModel MarkNotificationRead(NotificationData model)
        {
            try
            {
                var nu = new NotificationUtil();
                var isMarkRead = nu.MarkNotificationRead(model.Id, model.UserId);

                if (isMarkRead)
                {
                    return new ResultModel
                    {
                        Message = "Notification marked as read successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Failed to mark notification as read.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        // POST: api/System/
        [HttpPost]
        public ResultModel AllNotifications(NotificationPagination model)
        {
            try
            {
                model.UserId = User.Identity.GetUserId();
                model.PageSize = 10;
                var nu = new NotificationUtil();
                int totalRecords = 0;
                var Data = nu.GetAllNotifications(model, out totalRecords);
                return new ResultModel
                {
                    Message = Data.Any() ? "Notifications Found Successfully!" : "Notifications not Found !",
                    ResultFlag = Data.Any() ? 1 : 0,
                    Data = new { NotificationList = Data, Count = totalRecords }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Get User By UserId
        [HttpPost]
        public ResultModel GetUserById(clsRegister model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();

                var user = repoUsers.getUserById(model.UserId);

                if (user != null)
                {
                    var distributor = user.AspNetUser.DistributorUsers.FirstOrDefault().Distributor;
                    return new ResultModel
                    {
                        Message = "User Found",
                        ResultFlag = 1,
                        Data = new clsDistributor
                        {
                            EmployeeCode = user.ConsPartyCode,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            PhoneNumber = user.AspNetUser.UserName,
                            UserId = model.UserId,
                            Email = user.AspNetUser.Email,
                            Address = distributor.Address,
                            DistributorName = distributor.DistributorName,
                            Latitude = distributor.Latitude,
                            Longitude = distributor.Longitude,
                            City = distributor.City,
                            State = distributor.State,
                            Pincode = distributor.Pincode,
                            Gender = distributor.Gender,
                            LandlineNumber = distributor.LandlineNumber,
                            Gstin = distributor.Gstin,
                            FValue = distributor.FValue != null ? Convert.ToDecimal(distributor.FValue) : 0,
                            MValue = distributor.MValue != null ? Convert.ToDecimal(distributor.MValue) : 0,
                            SValue = distributor.SValue != null ? Convert.ToDecimal(distributor.SValue) : 0,
                            Company = distributor.Company,
                            UPIID = distributor.UPIID,
                            BankName = distributor.BankName,
                            IFSCCode = distributor.IfscCode,
                            AccNo = distributor.AccountNumber
                        }
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "User Not Found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Mobile App Apis

        #region Register User Steps
        [AllowAnonymous]
        [HttpPost]
        public ResultModel RegisterUserStep1MobileApp(RegisterUserStep1Model model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();
                    string UserId = "";
                    model.Email = "";
                    model.Password = "Abc123$";
                    //Register user
                    var result = _userManagement.RegisterUser(model.PhoneNumber, model.Email, model.Password, null, false, out UserId);
                    if (result.Succeeded)
                    {
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Address = model.Address,
                            Latitude = model.Latitude,
                            Longitude = model.Longitude,
                            IsDeleted = false,
                            UserId = UserId,
                            Password = Utils.Encrypt(model.Password)
                        };

                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                        if (resultDetails)
                        {
                            return new ResultModel
                            {
                                Message = "Registration Successfully.",
                                ResultFlag = 1,
                                Data = UserId
                            };
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = "Registration failed!!",
                                ResultFlag = 0,
                                Data = null
                            };
                        }
                    }
                    else
                    {
                        string message;
                        if (result.Errors.Count() > 0)
                        {
                            var error = result.Errors.FirstOrDefault();
                            if (error.Contains("is already taken"))
                            {
                                message = "Mobile number already exists. Please try different mobile number or login with your mobile number.";
                            }
                            else
                            {
                                message = error;
                            }
                        }
                        else
                        {
                            message = "Registration failed!";
                        }

                        return new ResultModel
                        {
                            Message = message,
                            ResultFlag = 0
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ResultModel RegisterUserStep2MobileApp(RegisterUserStep2Model model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    try
                    {
                        RepoUsers repoUsers = new RepoUsers();
                        var userDetail = repoUsers.getUserById(model.UserId);

                        if (model.Role == Garaaz.Models.Constants.Distributor)
                        {
                            model.DistributorId = repoUsers.SaveDistributor(new Distributor
                            {
                                Address = userDetail.Address,
                                Latitude = userDetail.Latitude,
                                Longitude = userDetail.Longitude,
                                FValue = Garaaz.Models.Constants.FValue,
                                MValue = Garaaz.Models.Constants.MValue,
                                SValue = Garaaz.Models.Constants.SValue
                            }, model.UserId, "");

                        }
                        else if (model.Role == Garaaz.Models.Constants.Workshop)
                        {
                            model.WorkshopId = repoUsers.SaveWorkShop(new WorkShop
                            {
                                Address = userDetail.Address,
                                Latitude = userDetail.Latitude,
                                Longitude = userDetail.Longitude
                            });
                        }

                        var r = _userManager.AddToRolesAsync(model.UserId, model.Role);
                        r.Wait();

                        return new ResultModel
                        {
                            Message = "Role updated succesfully",
                            ResultFlag = 1,
                            Data = model
                        };
                    }
                    catch (Exception exc)
                    {
                        return new ResultModel
                        {
                            Message = $"Role Update Failed. Error - {exc.Message}",
                            ResultFlag = 0,
                            Data = null
                        };
                    }

                }

            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ResultModel RegisterUserStep3MobileApp(RegisterUserStep3Model model)
        {
            try
            {
                bool result = true;
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.Email))
                    {
                        try
                        {
                            _userManagement.SetEmail(model.UserId, model.Email);
                        }
                        catch
                        {
                            return new ResultModel
                            {
                                Message = "User Email Update Failed",
                                ResultFlag = 0
                            };
                        }
                    }
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        try
                        {
                            result = _userManagement.ChangePassword(model.UserId, model.Password);
                            if (!result)
                            {
                                return new ResultModel
                                {
                                    Message = "Password not changed. Password must be at least 6 characters. Password must have at least one non letter or digit character. Password must have at least one lowercase ('a'-'z'). Password must have at least one uppercase ('A'-'Z').",
                                    ResultFlag = 0
                                };
                            }
                        }
                        catch
                        {
                            return new ResultModel
                            {
                                Message = "User Password Update Failed",
                                ResultFlag = 0
                            };
                        }
                    }
                    if (result)
                    {
                        // Now update distributor or workshop details
                        RepoUsers repoUsers = new RepoUsers();
                        if (model.Role == Garaaz.Models.Constants.Distributor)
                        {
                            var distributor = new Distributor
                            {
                                DistributorName = model.Name,
                                Gstin = model.Gstin,
                                Pincode = model.Pincode,
                                State = model.State,
                                City = model.City,
                                Gender = model.Gender,
                                LandlineNumber = model.LandlineNumber
                            };
                            repoUsers.UpdateDistributor(distributor, model.DistributorId);
                            repoUsers.SaveDistributorLocation(new clsDistributorLocation { DistributorId = model.DistributorId, Location = model.Location });
                        }
                        else if (model.Role == Garaaz.Models.Constants.Workshop)
                        {
                            var workshop = new WorkShop
                            {
                                WorkShopName = model.Name,
                                Gstin = model.Gstin,
                                Pincode = model.Pincode,
                                State = model.State,
                                City = model.City,
                                Gender = model.Gender,
                                LandlineNumber = model.LandlineNumber
                            };
                            repoUsers.UpdateWorkShop(workshop, model.WorkshopId);
                        }

                        // Save notification message for Super admins
                        var superAdmins = _userManagement.GetSuperAdminList();
                        var nu = new NotificationUtil();
                        var t = new System.Threading.Thread(() => nu.SaveNotification(model.Role, model.UserId, model.WorkshopId, superAdmins))
                        { IsBackground = true };
                        t.Start();

                        return new ResultModel
                        {
                            Message = "User Registered successfully",
                            ResultFlag = 1,
                            Data = null
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Password not changed. Password must be at least 6 characters. Password must have at least one non letter or digit character. Password must have at least one lowercase ('a'-'z'). Password must have at least one uppercase ('A'-'Z').",
                            ResultFlag = 0,
                            Data = null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Save Register Brand
        [AllowAnonymous]
        [HttpPost]
        public ResultModel SaveRegisterBrand(clsRegisterBrand model)
        {
            try
            {
                RegisterBrandRequest obj = new RegisterBrandRequest();
                bool result = obj.SaveRegisterBrand(model);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Your brand saved successfully",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Something Wrong",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #endregion

        #region Register Old API

        [AllowAnonymous]
        [HttpPost]
        public ResultModel RegisterOrUpdateUser(clsRegister model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) && string.IsNullOrEmpty(model.Password) && string.IsNullOrEmpty(model.FirstName) && string.IsNullOrEmpty(model.LastName) && string.IsNullOrEmpty(model.Role))
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();
                    string[] str = new string[1];
                    str[0] = model.Role;

                    if (!string.IsNullOrEmpty(model.UserId))
                    {
                        //Update user
                        var userDetails = new UserDetail
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            IsDeleted = false,
                            UserId = model.UserId,
                            //Password = Utils.Encrypt(model.Password)
                        };
                        var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                        _roleManagement.UpdateUserRole(model.UserId, str);
                        return new ResultModel
                        {
                            Message = "User details updated successfully.",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        string UserId = "";

                        //Regsiter user
                        var result = _userManagement.RegisterUser(model.Username, model.Password, str, out UserId);
                        if (result.Succeeded)
                        {
                            var userDetails = new UserDetail
                            {
                                FirstName = model.FirstName,
                                LastName = model.LastName,
                                IsDeleted = false,
                                UserId = UserId,
                                DistributorId = model.IsFromWorkshopPage ? null : model.DistributorId,
                                Password = Utils.Encrypt(model.Password)
                            };

                            var resultDetails = repoUsers.AddOrUpdateUsers(userDetails);
                            if (resultDetails)
                            {
                                //if (model.IsFromWorkshopPage && !string.IsNullOrEmpty(model.DistributorId))
                                //{
                                //    var r = repoUsers.SaveSingleDistributorByWorkShopId(0, (int)model.DistributorId);
                                //}

                                return new ResultModel
                                {
                                    Message = "Registration Successfully.",
                                    ResultFlag = 1
                                };
                            }
                            else
                            {
                                return new ResultModel
                                {
                                    Message = "Registration failed!!",
                                    ResultFlag = 0
                                };
                            }
                        }
                        else
                        {
                            return new ResultModel
                            {
                                Message = result.Errors.Count() > 0 ? result.Errors.FirstOrDefault() : "Registration failed!!",
                                ResultFlag = 0
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel GetDistributorsListByWorkShop(clsDistributorsWorkShop model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();
                var data = repoUsers.getDistributorByWorkShopId(model.WorkShopId);

                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Distributors Found",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Distributors Not Found",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel SaveDistributorsListByWorkShop(clsDistributorsWorkShop model)
        {
            try
            {
                RepoUsers repoUsers = new RepoUsers();
                var data = repoUsers.SaveDistributorByWorkShopId(model.WorkShopId, model.Distributors);

                if (data)
                {
                    return new ResultModel
                    {
                        Message = "Distributors saved successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Distributors Not Saved",
                        ResultFlag = 0,
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Schemes API

        #region Add/Update Schemes
        [HttpPost]
        public ResultModel AddScheme(SchemeModel model)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveOrUpdateScheme(model);

                return new ResultModel
                {
                    Message = data != null ? "Scheme saved successfully!" : "Failed to save scheme.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region list of Scheme
        [HttpPost]
        public ResultModel GetAllSchemeByUserId(string UserId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetAllSchemeByUserId(UserId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Get Scheme By Id
        [HttpPost]
        public ResultModel GetSchemeBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetSchemeBySchemeId(SchemeId);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete Scheme By Id
        [HttpPost]
        public ResultModel DeleteScheme(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.DeleteScheme(SchemeId);
                if (data)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Deleted Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Not Deleted",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Add Category/Update Category/List Of Category
        [HttpPost]
        public ResultModel AddCategoryScheme(List<CategorySchemeModel> lstmodel)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveCategoryScheme(lstmodel);
                if (data)
                {
                    return new ResultModel
                    {
                        Message = "Category Save Successfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Category Save Failed",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region GetSchemesCategory
        [HttpPost]
        public ResultModel GetCategoryBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetCategoryScheme(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Category Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Category Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of GiftManagement
        [HttpPost]
        public ResultModel AddGifManagement(List<GiftManagementModel> giftManagementModels)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveGiftManagement(giftManagementModels);

                return new ResultModel
                {
                    Message = data ? "Gift saved successfully!" : "Failed to save gift.",
                    ResultFlag = data ? 1 : 0,
                    Data = data,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }

        #region list of GiftManagement
        [HttpPost]
        public ResultModel GetGiftManagementBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetGiftManagement(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Gift Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Gift  Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of AssuredGift
        [HttpPost]
        public ResultModel AddAssuredGift(object model)
        {
            try
            {
                var assuredGiftModels = JsonConvert.DeserializeObject<List<AssuredGiftModel>>(model.ToString());
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveAssuredGift(assuredGiftModels);

                return new ResultModel
                {
                    Message = data ? "Assured gifts saved successfully!" : "Failed to save assured gifts.",
                    ResultFlag = data ? 1 : 0,
                    Data = data,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region list of AssuredGift
        [HttpPost]
        public ResultModel GetAssuredGiftBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetAssuredGift(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Assured Gift Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Assured Gift Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of CashBack
        [HttpPost]
        public ResultModel AddCashBackRange(List<CashBackRangeModel> cbrModels, int schemeId)
        {
            // Ref - https://weblog.west-wind.com/posts/2012/may/08/passing-multiple-post-parameters-to-web-api-controller-methods#Use-both-POST-and-QueryString-Parameters-in-Conjunction

            try
            {
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveCashBackRange(cbrModels, schemeId);

                return new ResultModel
                {
                    Message = data ? "Cashback range saved successfully!" : "Failed to save cashback.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }

        [HttpPost]
        public ResultModel AddCashBack(List<CashBackModel> cashbackModels, int schemeId)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveCashBack(cashbackModels, schemeId);

                return new ResultModel
                {
                    Message = data ? "Cashback saved successfully!" : "Failed to save cashback.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region list of CashBack       

        [HttpPost]
        public ResultModel GetCashbackBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetCashBack(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Cashback Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Cashback Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of QualifyCriteria
        [HttpPost]
        public ResultModel AddQualifyCriteria(List<QualifyCriteriaModel> qcModels)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var qcSaved = repoSchemes.SaveQualifyCriteria(qcModels);

                return new ResultModel
                {
                    Message = qcSaved ? "Qualify Criteria saved successfully!" : "Failed to save Qualify Criteria.",
                    ResultFlag = qcSaved ? 1 : 0,
                    Data = qcSaved
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region List of QualifyCriteria
        [HttpPost]
        public ResultModel GetQualifyCriteriaBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetQualifyCriteria(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Qualify Criteria Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Qualify Criteria Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of Target Workshop
        [HttpPost]
        public ResultModel AddTargetWorkshop(List<TargetWorkshopModel> targetWorkshops, int schemeId)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var twsSaved = repoSchemes.SaveTargetWorkshop(targetWorkshops, schemeId);

                return new ResultModel
                {
                    Message = twsSaved ? "Target workshops saved successfully!" : "Failed to save target workshops.",
                    ResultFlag = twsSaved ? 1 : 0,
                    Data = twsSaved
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #region list of QualifyCriteria
        [HttpPost]
        public ResultModel GetTargetWorkshopBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetTargetWorkshop(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Target Workshop Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Target Workshop Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of Focus Part
        [HttpPost]
        public ResultModel AddFocusPart(List<FocusPartModel> focusParts)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveFocusPart(focusParts);

                return new ResultModel
                {
                    Message = data ? "Saved focus part successfully!" : "Failed to save focus part.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region list of Focus Part
        [HttpPost]
        public ResultModel GetFocusPartBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetFocusPart(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Focus Part Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Focus Part Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Add/Update/ List of TicketOfJoy
        [HttpPost]
        public ResultModel AddTicketOfJoy(List<TicketOfJoyModel> lstmodel)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveTicketOfJoy(lstmodel);
                if (data)
                {
                    return new ResultModel
                    {
                        Message = "TicketOfJoy Save Successfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "TicketOfJoy Save Failed",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #region list of TicketOfJoy
        [HttpPost]
        public ResultModel GetTicketOfJoyBySchemeId(int SchemeId)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetTicketOfJoy(SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "TicketOfJoy Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "TicketOfJoy Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
        #endregion

        #region Active/InActive
        [HttpPost]
        public ResultModel GetSchemeActive(SchemeActiveModel model)
        {
            try
            {
                if (model != null)
                {
                    RepoSchemes repoSchemes = new RepoSchemes();
                    var data = repoSchemes.GetSchemeActive(model);
                    if (data)
                    {
                        return new ResultModel
                        {
                            Message = "Scheme Found Sucessfully",
                            ResultFlag = 1,
                            Data = data
                        };
                    }
                }
                return new ResultModel
                {
                    Message = "Scheme Failed",
                    ResultFlag = 0,
                    Data = false
                };

            }
            catch
            {
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };
            }
        }

        [HttpPost]
        public ResultModel SaveSchemeActive(SchemeActiveModel model)
        {
            try
            {
                if (model != null)
                {
                    RepoSchemes repoSchemes = new RepoSchemes();
                    var data = repoSchemes.SchemeActive(model);
                    if (data)
                    {
                        return new ResultModel
                        {
                            Message = "Scheme Update Sucessfully",
                            ResultFlag = 1,
                            Data = data
                        };
                    }
                }
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };

            }
            catch
            {
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };
            }
        }
        #endregion

        #region AreBothApplicable
        [HttpPost]
        public ResultModel SaveAreBothApplicable(AreBothApplicableModel model)
        {
            try
            {
                if (model != null)
                {
                    var repoSchemes = new RepoSchemes();
                    var data = repoSchemes.SaveAreBothApplicable(model);

                    return new ResultModel
                    {
                        Message = data ? "Scheme updated successfully!" : "Failed to update scheme.",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };

            }
            catch
            {
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };
            }
        }

        [HttpPost]
        public ResultModel GetAreBothApplicable(AreBothApplicableModel model)
        {
            try
            {
                if (model != null)
                {
                    RepoSchemes repoSchemes = new RepoSchemes();
                    var data = repoSchemes.GetAreBothApplicable(model);
                    if (data)
                    {
                        return new ResultModel
                        {
                            Message = "Scheme Update Sucessfully",
                            ResultFlag = 1,
                            Data = data
                        };
                    }
                }
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };

            }
            catch
            {
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };
            }
        }
        #endregion

        #region Can take more than One Gift
        [HttpPost]
        public ResultModel SaveCanTakeMoreThanOneGift(CanTakeMoreThanOneModel model)
        {
            try
            {
                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.SaveCanTakeMoreThanOneGift(model);

                return new ResultModel
                {
                    Message = data ? "Scheme updated successfully!" : "Failed to update scheme",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Scheme Update Failed",
                    ResultFlag = 0,
                    Data = false
                };
            }
        }

        [HttpPost]
        public ResultModel GetCanTakeMoreThanOneGift(CanTakeMoreThanOneModel model)
        {
            try
            {
                if (model == null)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Id was not passed.",
                        ResultFlag = 0,
                        Data = null
                    };
                }

                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetCanTakeMoreThanOneGift(model);

                return new ResultModel
                {
                    Message = data ? "Successfully found that taking more than one gift allowed!" : "Failed to find taking more than one gift allowed.",
                    ResultFlag = data ? 1 : 0,
                    Data = model.MaxGiftsAllowed
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return new ResultModel
                {
                    Message = exc.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get all schemes per User Id and Role

        [HttpPost]
        public ResultModel GetSchemes(SchemeModel model)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.GetAllSchemeByUserIdAndRole(model.UserId, model.Role);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Found Sucessfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Not Found",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Scheme Description
        [AllowAnonymous]
        [HttpPost]
        public ResultModel GetSchemeDescription(SchemeDescriptionModel model)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();

                var rsd = new RepoSchemesDescription();
                var data = rsd.GetSchemeDescription(model.SchemeId, userId, role);

                return new ResultModel
                {
                    Message = data != null ? "Scheme Description Found Successfully" : "Scheme Description not Found",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }
        #endregion

        #region Scheme User Level
        [AllowAnonymous]
        [HttpPost]
        public ResultModel GetSchemeUserLevel(SchemeUserLevelModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoSchemesDescription();
                var data = obj.GetSchemeUserLevel(model);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Scheme User level Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme User level not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Scheme Focus Parts Groups
        [HttpPost]
        public ResultModel GetSchemeFocusPartsGroups(SchemeDescriptionModel model)
        {
            try
            {
                RepoSchemesDescription repoSchemes = new RepoSchemesDescription();
                var htmlWithTable = repoSchemes.GetSchemeFocusPartsGroupsHtml(model.SchemeId);

                return new ResultModel
                {
                    Message = !string.IsNullOrWhiteSpace(htmlWithTable) ? "HTML with table created successfully!" : "Failed to create HTML with table.",
                    ResultFlag = !string.IsNullOrWhiteSpace(htmlWithTable) ? 1 : 0,
                    Data = new { HtmlWithTable = htmlWithTable }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Scheme Focus-Parts-Groups Filter
        [HttpPost]
        public ResultModel SchemeFocusPartsGroupsFilter(FocusPartGroupsRequest model)
        {
            try
            {
                if (model.SchemeId == 0)
                {
                    return new ResultModel
                    {
                        Message = "SchemeId is required.",
                        ResultFlag = 0
                    };
                }
                model.PageSize = 10;
                var totalRecords = 0;
                var totalPage = 0;

                var rsd = new RepoSchemesDescription();
                var focusPartGroups = rsd.GetSchemeFocusPartsGroupsPagination(model, out totalRecords);

                totalPage = totalRecords / Convert.ToInt32(model.PageSize);
                if (totalRecords % model.PageSize > 0)
                {
                    totalPage += 1;
                }

                return new ResultModel
                {
                    Message = focusPartGroups.Count > 0 ? "Focus-parts-groups found successfully!" : "Failed to find Focus-parts-groups.",
                    ResultFlag = focusPartGroups.Count > 0 ? 1 : 0,
                    Data = new { focusPartGroups = focusPartGroups, Count = totalRecords, PageNumber = model.PageNumber, Totalpages = totalPage }
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }
        #endregion

        #endregion

        #region Product Group API

        #region Get All Product Group
        public ResultModel GetProductGroup()
        {
            try
            {
                ProductGroupPagePermissions obj = new ProductGroupPagePermissions();
                var data = obj.GetAllProductGroup(0);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Product Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Product not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Product not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get ProductGroup By DistributorId
        [HttpPost]
        public ResultModel GetProductGroupByDistributorId()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                ProductGroupPagePermissions obj = new ProductGroupPagePermissions();
                var data = obj.GetProductGroupByDistributorId(userId, role);
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Message = "Product Category Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Product Category not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Product Category Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Product Part Category
        [HttpPost]
        public ResultModel GetProductPartCategory()
        {
            try
            {
                // string userId = User.Identity.GetUserId(); GetProductGroupByDistributorId
                // string role = GetUserRole();
                ProductGroupPagePermissions obj = new ProductGroupPagePermissions();
                //  var data = obj.GetProductGroupByDistributorId(userId, role);
                var data = obj.GetProductPartCategory();
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Message = "Product Part Category Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Product Part Category not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Product Part Category Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get all product group based on group id
        [HttpPost]
        public ResultModel GetProductGroupByGroupId(clsProductGroup model)
        {
            var pgpp = new ProductGroupPagePermissions();
            var data = pgpp.GetAllProductGroupByGroupId(model.GroupId);
            if (data.Count > 0)
            {
                return new ResultModel
                {
                    Message = "Product groups found successfully",
                    ResultFlag = 1,
                    Data = data
                };
            }
            return new ResultModel
            {
                Message = "Product groups not found.",
                ResultFlag = 0,
                Data = null
            };
        }
        #endregion

        #region Get all product group based on group id
        [HttpPost]
        public ResultModel GetProductGroupByGroupIdMobile(clsProductGroup model)
        {
            var pgpp = new ProductGroupPagePermissions();
            var data = pgpp.GetAllProductGroupByGroupIdMobile(model.GroupId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Product groups found successfully",
                    ResultFlag = 1,
                    Data = data
                };
            }
            return new ResultModel
            {
                Message = "Product groups not found.",
                ResultFlag = 0,
                Data = null
            };
        }
        #endregion

        #region Get Product Group Name By Id
        public ResultModel GetProductGroupNameById(int GroupId)
        {
            var productGroupPagePermissions = new ProductGroupPagePermissions();
            var data = productGroupPagePermissions.GetProductGroupNameById(GroupId);

            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Product Group Name Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Product Group Name Not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save Product Group
        public ResultModel SaveProductGroup(clsProductGroup model)
        {
            try
            {
                // To insert null in table, we used below line
                // Using ternary operator will result in error as follow
                // Type of conditional expression cannot be determined because there is no implicit conversion between System.DBNull and int
                int? parentId = null;
                if (model.ParentId > 0) parentId = model.ParentId;

                ProductGroupPagePermissions productGroupPagePermissions = new ProductGroupPagePermissions();
                bool status = productGroupPagePermissions.SaveProductGroup(new ProductGroup
                {
                    GroupId = model.GroupId,
                    GroupName = model.GroupName,
                    CreatedDate = DateTime.Now,
                    ParentId = parentId,
                    //VariantId = model.VariantId,
                    ImagePath = model.ImagePath,
                    DistributorId = model.DistributorId,
                    BrandId = model.BrandId,
                });
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Product Group Updated Successfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Product Group Updated UnSuccessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete Product Group
        [HttpPost]
        public ResultModel DeleteProductGroup(clsProductGroup model)
        {
            try
            {
                ProductGroupPagePermissions productGroupPagePermissions = new ProductGroupPagePermissions();
                var result = productGroupPagePermissions.DeleteProductGroup(model.GroupId);
                if (result)
                {

                    return new ResultModel
                    {
                        Message = "Product Group Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Product Group Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region  Get Product Name for DropDownList
        public List<ProductGroup> ddlProductGroups()
        {
            ProductGroupPagePermissions productGroupPagePermissions = new ProductGroupPagePermissions();
            var data = productGroupPagePermissions.ddlProductGroups();
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }
        #endregion

        #region Get All ProductList Group by DistributorId
        [HttpPost]
        public ResultModel GetProductListGroupByDistributorId()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                ProductGroupPagePermissions obj = new ProductGroupPagePermissions();
                var data = obj.GetProductListGroupByDistributorId(userId, role);
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Message = "Product Group Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Product Group not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Product not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region  Get Product Name for DropDownList
        public List<ProductGroup> ddlProductGroupsDistributor()
        {
            string userId = User.Identity.GetUserId();
            string role = GetUserRole();
            int distributorId = userId.GetDistributorId(role);
            ProductGroupPagePermissions productGroupPagePermissions = new ProductGroupPagePermissions();
            var data = productGroupPagePermissions.ddlProductGroupsDistributor(distributorId);
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }
        #endregion

        #endregion

        #region Product API

        #region Get All Product
        public ResultModel GetProduct()
        {
            try
            {
                ProductPagePermission obj = new ProductPagePermission();
                var data = obj.GetAllProduct();
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Product Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Product not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    Message = "Product not Found " + ex.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Product Name By Id
        public ResultModel GetProductNameById(int ProductId)
        {
            ProductPagePermission productPagePermission = new ProductPagePermission();
            var data = productPagePermission.GetProductNameById(ProductId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Product Name Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Product Name Not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Product Name By Id Mobile
        public ResultModel GetProductNameByIdMobile(int ProductId)
        {
            ProductPagePermission productPagePermission = new ProductPagePermission();
            var data = productPagePermission.GetProductNameByIdMobile(ProductId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Product Name Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Product Name Not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save Product
        public ResultModel SaveProduct(clsProduct model)
        {
            try
            {
                ProductPagePermission productPagePermission = new ProductPagePermission();
                bool status = productPagePermission.SaveProduct(model);
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Product Updated Successfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Product Updated UnSuccessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete Product
        [HttpPost]
        public ResultModel DeleteProduct(clsProduct model)
        {
            try
            {
                ProductPagePermission productPagePermission = new ProductPagePermission();
                var result = productPagePermission.DeleteProduct(model.ProductId);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Product Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Product Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Product by group id
        [HttpPost]
        public ResultModel GetProductByGroupID(clsProductGroup model)
        {
            try
            {
                var obj = new ProductPagePermission();
                var data = obj.GetProductsByGroupID(model.GroupId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Products Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Products not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception exc)
            {
                return new ResultModel
                {
                    Message = exc.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Product by DistributorId
        public ResultModel GetProductByDistributorId(int DistributorId)
        {
            try
            {
                ProductPagePermission obj = new ProductPagePermission();
                var data = obj.GetProductbyDistributorId(DistributorId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Product Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Product not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    Message = "Product not Found " + ex.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #endregion

        #region Vehicle API

        #region Get All Vehicle
        public ResultModel GetVehicle()
        {
            try
            {
                var obj = new VehiclePagePermission();
                var data = obj.GetAllVehicle();
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Vehicle Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Vehicle not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Vehicle not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Vehicles by Brand ID
        [HttpPost]
        public ResultModel GetVehiclesByBrandID(clsVehicle model)
        {
            var vehiclePagePermission = new VehiclePagePermission();
            var data = vehiclePagePermission.GetVehiclesByBrandID(model.BrandId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Vehicles Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "no records found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Vehicle Name By Id
        public ResultModel GetVehicleNameById(int vehicleId)
        {
            var vehiclePagePermission = new VehiclePagePermission();
            var data = vehiclePagePermission.GetVehicleNameById(vehicleId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Vehicle Name Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Vehicle Name Not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save Vehicle
        public ResultModel SaveVehicle(clsVehicle model)
        {
            try
            {
                var VehiclePagePermission = new VehiclePagePermission();
                bool status = VehiclePagePermission.SaveVehicle(model);
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Vehicle Updated Successfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Vehicle Updated UnSuccessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete Vehicle
        [HttpPost]
        public ResultModel DeleteVehicle(clsVehicle model)
        {
            try
            {
                VehiclePagePermission VehiclePagePermission = new VehiclePagePermission();
                var result = VehiclePagePermission.DeleteVehicle(model.VehicleId);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Vehicle Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Vehicle Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region  Get Brand Name for DropDownList
        public List<clsBrand> ddlBrands()
        {
            var vpp = new VehiclePagePermission();
            var data = vpp.ddlBrands();
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }

        #endregion

        #region  Get ProductGroup Name for DropDownList
        public List<ProductGroup> ddlProductGroup()
        {
            var cls = new ProductGroupPagePermissions();
            var data = cls.ddlProductGroups();
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }

        #endregion

        #endregion

        #region Brand
        #region Get All Brand
        [AllowAnonymous]
        [HttpPost]
        public ResultModel GetAllBrand(clsBrand brand)
        {
            try
            {
                BrandPermissions obj = new BrandPermissions();
                var data = obj.GetAllBrand(brand.UserId, brand.Role, brand.SearchString);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Brand Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Brand not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Brand not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Brand Name By Id
        public ResultModel GetBrandNameById(int ProductId)
        {
            BrandPermissions brandPermissions = new BrandPermissions();
            var data = brandPermissions.GetBrandNameById(ProductId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Brand Name Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Brand Name Not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save Brand
        public ResultModel SaveBrand(clsBrand model)
        {
            try
            {
                BrandPermissions brandPermissions = new BrandPermissions();
                bool status = brandPermissions.SaveBrand(model);
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Brand Updated Successfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Brand Updated UnSuccessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete Brand
        [HttpPost]
        public ResultModel DeleteBrand(clsBrand model)
        {
            try
            {
                BrandPermissions brandPermissions = new BrandPermissions();
                var result = brandPermissions.DeleteBrand(model.BrandId.Value);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Brand Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Brand Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #endregion

        #region Variant API
        #region Get variant by VehicleId
        [HttpPost]
        public ResultModel GetVariantByVehicleID(clsVariant model)
        {
            try
            {
                var obj = new VariantPagePermission();
                var data = obj.GetVariantByVehicleID(model.VehicleId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Variant Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get variant by VehicleId Mobile
        [HttpPost]
        public ResultModel GetVariantByVehicleIDMobile(clsVariant model)
        {
            try
            {
                var obj = new VariantPagePermission();
                var data = obj.GetVariantByVehicleIDMobile(model.VehicleId);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Variant Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get all variants
        public ResultModel GetVariant()
        {
            try
            {
                var obj = new VariantPagePermission();
                var data = obj.GetAllVariant();
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Variant Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get variant name by Id
        public ResultModel GetVariantNameById(int variantId)
        {
            var variantPagePermission = new VariantPagePermission();
            var data = variantPagePermission.GetVariantNameById(variantId);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Variant name found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Variant Not Found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save variant
        public ResultModel SaveVariant(clsVariant model)
        {
            try
            {
                var variantPagePermission = new VariantPagePermission();
                bool status = variantPagePermission.SaveVariant(model);
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Variant Updated Successfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Variant Updated UnSuccessfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete variant
        [HttpPost]
        public ResultModel DeleteVariant(clsVariant model)
        {
            try
            {
                var variantPagePermission = new VariantPagePermission();
                var result = variantPagePermission.DeleteVariant(model.VariantId);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Variant Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Variant Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get vehicle name for dropdownlist
        public List<Vehicle> ddlVehicles()
        {
            var vpp = new VariantPagePermission();
            var data = vpp.ddlVehicles();
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }
        #endregion

        #region Get variant parent name for dropdownlist
        public List<Variant> ddlVariants()
        {
            var vpp = new VariantPagePermission();
            var data = vpp.ddlVariants();
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }
        #endregion

        #endregion

        #region Product Groups APIs
        [HttpPost]
        public ResultModel GetProductGroupsByVariantID(clsProductGroup model)
        {
            try
            {
                var obj = new ProductGroupPagePermissions();
                var data = obj.GetProductGroupsByVariantID(model.VariantId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Variant Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Product Groups Mobile APIs
        [HttpPost]
        public ResultModel GetProductGroupsByVariantIDMobile(clsProductGroup model)
        {
            try
            {
                var obj = new ProductGroupPagePermissions();
                var data = obj.GetProductGroupsByVariantIDMobile(model.VariantId);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Variant Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Variant not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Order Related APIs

        #region SaveOrder
        [HttpPost]
        public ResultModel SaveOrder(SaveOrderModel model)
        {
            try
            {
                // Check if user is workshop or not
                var roles = GetUserRoles();
                if (!roles.Any(s => s.Equals(Models.Constants.Workshop, StringComparison.OrdinalIgnoreCase) || s.Equals(Models.Constants.WorkshopUsers, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ResultModel
                    {
                        Message = "Placing order is allowed for workshop only.",
                        ResultFlag = 0,
                        Data = null
                    };
                }

                var repoOrder = new RepoOrder();
                var userId = User.Identity.GetUserId();
                var role = GetUserRole();
                var superAdmins = _userManagement.GetSuperAdminList();

                var orderSaved = repoOrder.ProcessCheckout(ref model, userId, role, superAdmins);

                return new ResultModel
                {
                    Message = orderSaved ? "Order saved successfully!" : "Failed to save order.",
                    ResultFlag = orderSaved ? 1 : 0,
                    Data = orderSaved ? model : null
                };

                //var orderSaved = repoOrder.SaveOrder(model.TempOrderId, model.PaymentMethod, model.RazorpayOrderId, model.RazorpayPaymentId, model.RazorpaySignature, out var soModel);

                //if (orderSaved)
                //{
                //    // delete tempOrderId from UserDetails
                //    repoOrder.RemoveTempOrderId(model.UserId);

                //    // Save Notifications Start
                //    var notify = new CustomerBackOrderModel { UserId = User.Identity.GetUserId(), Role = GetUserRole() };
                //    var rd = new RepoDashboard();
                //    var superAdmins = _userManagement.GetSuperAdminList();
                //    var thread = new System.Threading.Thread(() => rd.SaveOrderNotification(soModel.OrderId, notify.UserId, notify.Role, superAdmins, soModel.OrderNumber))
                //    { IsBackground = true };
                //    thread.Start();

                //    // Save Notifications End
                //    return new ResultModel
                //    {
                //        Message = "Order saved successfully!",
                //        ResultFlag = 1,
                //        Data = soModel
                //    };
                //}


            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }
        #endregion

        #region AddTocart
        //[HttpPost]
        //public ResultModel AddtoCart(AddtocartModel objModel)
        //{
        //    try
        //    {
        //        int tempOrderId;
        //        RepoOrder repoOrder = new RepoOrder();
        //        var addtocart = repoOrder.AddToCart(objModel, out tempOrderId);
        //        if (addtocart)
        //        {
        //            return new ResultModel()
        //            {
        //                Message = "Product added to cart.",
        //                ResultFlag = 1,
        //                Data = tempOrderId
        //            };
        //        }
        //        else
        //        {
        //            return new ResultModel()
        //            {
        //                Message = "Product added to cart failed.",
        //                ResultFlag = 0,
        //                Data = null
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
        //    }
        //}
        #endregion

        #region AddTocartMobile
        [HttpPost]
        public ResultModel AddtoCartMobile(AddtocartModel model)
        {
            try
            {
                string role = string.IsNullOrEmpty(model.Role) ? GetUserRole() : model.Role;

                var DistributorId = model.UserId.GetDistributorId(role);

                var repoOrder = new RepoOrder();
                Product product = repoOrder.GetProduct(DistributorId, model.PartNumber, role);

                if (product == null)
                {
                    return new ResultModel
                    {
                        Message = "This product is not available.",
                        ResultFlag = 0,
                        Data = null
                    };
                }

                model.UnitPrice = product.Price;
                model.ProductId = product.ProductId;


                var addtocart = repoOrder.AddToCart(model, out int tempOrderId);
                if (addtocart)
                {
                    // Save temporderid to user
                    var savechanges = repoOrder.AddTempOrderId(tempOrderId, model.UserId);

                    //// Save Notifications Start
                    //var rd = new RepoDashboard();
                    //var result = new System.Threading.Thread(() => rd.SaveNotificationAddToCart(tempOrderId, model.UserId, role))
                    //{ IsBackground = true };
                    //result.Start();
                    //// Save Notifications Ends

                    return new ResultModel
                    {
                        Message = "Product added to cart.",
                        ResultFlag = 1,
                        Data = tempOrderId
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Failed to add product to cart.",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get User's All Order
        [HttpPost]
        public ResultModel GetOrderByUserId(GetUserOrderModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.GetOrderByUserId(model.UserId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Order found sucessfully",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Order not found.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Order by OrderId and userId
        [HttpPost]
        public ResultModel GetOrderByOrderId(GetOrderModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.GetOrderByOrderId(model);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Order found sucessfully",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Order not found.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Order Status by OrderId
        [HttpPost]
        public ResultModel GetOrderStatus(OrderStatusRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.GetOrderStatusByOrderId(model.OrderId);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Order status found sucessfully",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Order status not found.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get cart
        [HttpPost]
        public ResultModel GetCart(GetCartModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field.",
                        ResultFlag = 0
                    };
                }

                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.GetCart(model);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Product cart found.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Product cart not found.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Remove cart
        [HttpPost]
        public ResultModel RemoveCart(RemoveCartModel model)
        {
            try
            {
                string Message = "";

                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.RemoveCart(model, out Message);
                if (data)
                {
                    return new ResultModel
                    {
                        Message = "Product cart Removed",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Product cart not Removed.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Update cart

        [HttpPost]
        public ResultModel UpdateCart(List<clsCartData> model)
        {
            try
            {
                var repoOrder = new RepoOrder();
                var isCartUpdated = repoOrder.UpdateCart(model);
                if (isCartUpdated)
                {
                    return new ResultModel
                    {
                        Message = "Cart was updated successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Failed to update cart.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Checkout Delivery Address
        [HttpPost]
        public ResultModel GetDeliveryAddress(GetCartModel model)
        {
            try
            {
                var rc = new RepoCheckout();
                var cdaModel = rc.GetDeliveryAddress(model);
                if (cdaModel != null)
                {
                    return new ResultModel
                    {
                        Message = "Delivery address found!",
                        ResultFlag = 1,
                        Data = cdaModel
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Failed to retrieve delivery address.",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel SaveDeliveryAddress(CheckoutDeliveryAddressModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    var rc = new RepoCheckout();
                    var saved = rc.SaveDeliveryAddress(model);
                    if (saved)
                    {
                        return new ResultModel
                        {
                            Message = "Delivery address saved successfully!",
                            ResultFlag = 1
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Failed to save delivery address.",
                            ResultFlag = 0
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Save payment method
        [HttpPost]
        public ResultModel SavePaymentMethod(PaymentMethodModel model)
        {
            try
            {
                var rc = new RepoCheckout();
                if (!model.PaymentMethod.Equals("CashOnDelivery", StringComparison.OrdinalIgnoreCase))
                {
                    string geneateSignature = rc.HmacSha256Digest(model.RazorOrderId, model.RazorPaymentId);
                    if (model.RazorSignature != geneateSignature)
                    {
                        return new ResultModel
                        {
                            Message = "Payment Failed!",
                            ResultFlag = 0
                        };
                    }
                }
                var isSaved = rc.SavePaymentMethod(model);
                if (isSaved)
                {
                    return new ResultModel
                    {
                        Message = "Payment method saved successfully!",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Failed to save payment method.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get grand total
        [HttpPost]
        public ResultModel GetGrandTotal(GetCartModel model)
        {
            try
            {
                var repoOrder = new RepoOrder();
                var data = repoOrder.GetGrandTotal(model);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Grand total value retrieved.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Failed to retrieve grand total value.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Order UserId by OrderId
        [HttpPost]
        public ResultModel GetOrderUserByOrderId(OrderStatusRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.GetOrderUserByOrderId(model.OrderId);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Order User found sucessfully",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Order User not found.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #endregion

        #region TargetGrowth API

        #region Get All TargetGrowth
        [HttpPost]
        public ResultModel GetTargetGrowth(TargetGrowthRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoSchemes obj = new RepoSchemes();
                var data = obj.GetTargetGrowthsBySchemeId(model.SchemeId);
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Target Growth Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Target Growth not Found ",
                    ResultFlag = 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    Message = "Target Growth not Found " + ex.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save TargetGrowth
        [HttpPost]
        public ResultModel SaveTargetGrowth(List<TargetGrowth> modelList)
        {
            try
            {
                RepoSchemes obj = new RepoSchemes();
                bool status = obj.SaveTargetGrowth(modelList);
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Target Growth Saved/Updated Successfully",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Target Growth Saved/Updated Failed",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #endregion

        #region Add/Update Scheme Target Criteria
        [HttpPost]
        public ResultModel UpdateSchemeCriteria(SchemesCriteria model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.UpdateSchemeCriteria(model);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Scheme Target Criteria Save Successfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Scheme Target Criteria Save Failed",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Add/Update Scheme Target Criteria
        [HttpPost]
        public ResultModel UpdateCashbackCriteria(CriteriaOnCashback model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }

                var repoSchemes = new RepoSchemes();
                var data = repoSchemes.UpdateCashbackCriteria(model);
                return new ResultModel
                {
                    Message = data != null ? "Cashback criteria for scheme saved successfully!" : "Failed to save cashback criteria for scheme.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Mobile Dashboard
        [HttpPost]
        public ResultModel GetDashboard(DashboardRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }

                model.Roles = GetUserRoles();
                RepoDashboard dashboard = new RepoDashboard();
                var data = dashboard.GetDashboard(model);

                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Dashboard found sucessfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Dashboard Not Found",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }
        #endregion

        #region Mobile Dashboard ProductType Details
        [HttpPost]
        public ResultModel DashboardProductTypeDetails(DashboardRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                string UpiId = "";
                RepoDashboard dashboard = new RepoDashboard();
                var data = dashboard.DashboardProductTypeDetails(model, out UpiId, false);

                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Dashboard Product Type Detail found sucessfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Dashboard Product Type Detail Not Found",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Available Stock
        [HttpPost]
        public ResultModel AvailableStock(StockModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field.",
                        ResultFlag = 0
                    };
                }

                var rd = new RepoDashboard();
                var data = rd.GetAvailableStock(model, out int totalRecord);

                return new ResultModel
                {
                    Message = data.Any() ? "Available stock data was retrieved successfully" : "Failed to retrieve stock data.",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { ProductList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Sales Return 
        [HttpPost]
        public ResultModel GetSalesReturn(SalesReturnRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }

                model.Roles = GetUserRoles();

                var dashboard = new RepoDashboard();
                var data = dashboard.SalesReturn(model, false, out int totalRecord);
                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sales return found successfully" : "Sales return not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = new { SalesReturnList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Customer Back Order
        [HttpPost]
        public ResultModel CustomerBackOrder(CustomerBackOrderModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field.",
                        ResultFlag = 0
                    };
                }

                var rd = new RepoDashboard();
                var data = rd.GetCustomerBackOrders(model.UserId, model.Role, model.StartDate, model.EndDate, model.PageNumber, out var totalRecord);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Customer back orders were retrieved successfully!" : "Failed to retrieve customer back orders.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = new { CustomerBackOrderList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }
        #endregion

        #region Customer Back Order Details By OrderNumber
        [HttpPost]
        public ResultModel CustomerBackOrderDetails(CustomerBackOrderModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.OrderNo))
                {
                    return new ResultModel
                    {
                        Message = "Please provide order number.",
                        ResultFlag = 0
                    };
                }
                var rd = new RepoDashboard();
                var data = rd.GetCustomerBackOrderDetails(model);
                return new ResultModel
                {
                    Message = data.Count > 0 ? "Customer back order details were retrieved successfully!" : "Failed to retrieve customer back order details.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = new { CustomerBackOrderList = data }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Invoice       
        [HttpPost]
        public ResultModel GetInvoice(SalesReturnRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }

                model.Roles = GetUserRoles();

                var dashboard = new RepoDashboard();
                var data = dashboard.GetInvoice(model, out int totalRecord);
                return new ResultModel
                {
                    Message = data.Count > 0 ? "Invoice found successfully!" : "Invoice not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = new { InvoiceList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region InvoiceDetail     
        [HttpPost]
        public ResultModel GetInvoiceDetail(InvoiceDetailRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }

                model.Roles = GetUserRoles();

                var dashboard = new RepoDashboard();
                var data = dashboard.GetInvoiceDetail(model, out int totalRecord);
                return new ResultModel
                {
                    Message = data.Count > 0 ? "Invoice detail found successfully!" : "Failed to find invoice detail.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = new { InvoiceList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get MGA Banner by DistributorId
        [HttpPost]
        public ResultModel GetMgaBanner(MgaBannerRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                RepoMgaCatalougeBanner obj = new RepoMgaCatalougeBanner();
                int totalRecord = 0;
                var data = obj.AllMgaBannerByUserId(model, out totalRecord);
                return new ResultModel
                {
                    Message = data.Any() ? "MGA Banner found sucessfully!" : "MGA Banner not found sucessfully!",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { MgaBannerList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get MGA Banner Catalog Product by BannerId
        [HttpPost]
        public ResultModel GetBannerProducts(MgaBannerProductRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                RepoMgaCatalougeBanner obj = new RepoMgaCatalougeBanner();
                int totalRecord = 0;
                var data = obj.AllProductByMgaBannerId(model, out totalRecord);
                return new ResultModel
                {
                    Message = data.Any() ? "MGA banner product found sucessfully!" : "MGA banner product not found sucessfully!",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { MgaBannerProductList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Reject Back Order
        [AllowAnonymous]
        [HttpPost]
        public ResultModel RejectBackOrder(CustomerBackOrderModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.OrderNo))
                {
                    return new ResultModel
                    {
                        Message = "Please provide order number.",
                        ResultFlag = 0
                    };
                }
                model.UserId = User.Identity.GetUserId();
                model.Role = GetUserRole();
                var rd = new RepoDashboard();
                var superAdmins = _userManagement.GetSuperAdminList();
                string message = string.Empty;
                var data = rd.RejectCustomerBackOrder(model.OrderNo, model.PartNumber, model.UserId, model.Role, superAdmins, out message);

                return new ResultModel
                {
                    //Message = data ? "Customer back orders were rejected successfully!" : "Failed to reject customer back orders.",
                    Message = message,
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };

            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Product Availablity Type
        [HttpPost]
        public ResultModel GetProductAvailablityType()
        {
            try
            {
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.ProductAvailablityType();
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Data = data,
                        Message = "Product availablity type  found.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Data = null,
                        Message = "Product availablity type not found.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel SaveProductAvailablityType(List<ProductAvailablityRequest> model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field.",
                        ResultFlag = 0
                    };
                }
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.SaveProductAvailablityType(model);
                return new ResultModel
                {
                    Data = data,
                    Message = "Product availablity type saved sucessfully.",
                    ResultFlag = 1
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Coupons
        [HttpPost]
        public ResultModel GetCoupons(CouponRequest model)
        {
            try
            {
                //var userId = User.Identity.GetUserId();

                //var roles = ((ClaimsIdentity)User.Identity).Claims
                // .Where(c => c.Type == ClaimTypes.Role)
                // .Select(c => c.Value);

                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                RepoOrder obj = new RepoOrder();
                int totalRecord = 0;
                var data = obj.GetCoupons(model, out totalRecord);
                return new ResultModel
                {
                    Message = data.Any() ? "Coupons found sucessfully!" : "Coupons not found sucessfully!",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { CouponsList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region FastMoving 
        [HttpPost]
        public ResultModel FastMoving(FastMovingRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }

                // Add Role
                //model.Role = ((ClaimsIdentity)User.Identity).Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

                int totalRecord = 0;
                RepoDashboard dashboard = new RepoDashboard();
                var data = dashboard.FastMoving(model, out totalRecord);

                return new ResultModel
                {
                    Message = data.Any() ? "Fast Moving found sucessfully." : "Fast Moving not found sucessfully.",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { FastMovingList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion  

        #region Fast Moving Products
        [HttpPost]
        public ResultModel FastMovingProducts(FastMovingProductRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                var obj = new ProductPagePermission();
                int totalRecord = 0;
                var data = obj.GetFastMovingProducts(model, out totalRecord);
                return new ResultModel
                {
                    Message = data.Any() ? "Products found sucessfully!" : "Products not found sucessfully!",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { FastMovingProductList = data, Count = totalRecord }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region GetBannerProductOnloadMore
        [HttpPost]
        public ResultModel GetBannerProductOnloadMore(ProductLoadMore model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                RepoMgaCatalougeProduct repo = new RepoMgaCatalougeProduct();
                var data = repo.GetBannerProductsOnLoadMore(model);
                return new ResultModel
                {
                    Message = data.Any() ? "Product found sucessfully!" : "Product not found sucessfully!",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Cancel Order By OrderId        
        public ResultModel CancelOrderById(OrderResponseModel model)
        {
            try
            {
                var cls = new RepoOrder();
                var isCancel = cls.CancelOrderById(model.OrderID, out string OrderNo);
                if (isCancel)
                {
                    CustomerBackOrderModel user = new CustomerBackOrderModel();
                    user.UserId = User.Identity.GetUserId();
                    user.Role = GetUserRole();
                    var superAdmins = _userManagement.GetSuperAdminList();
                    var rd = new RepoDashboard();
                    // Save Notification For Supar admin, Ro incharge and Distributors for Order Cancel.
                    var Data = new System.Threading.Thread(() => rd.SaveRejectOrderNotification(OrderNo, model.OrderID.ToString(), user.UserId, user.Role, superAdmins, Models.Constants.RejectMainOrder))
                    { IsBackground = true };
                    Data.Start();
                    return new ResultModel
                    {
                        Message = "Order Successfully Cancelled.",
                        ResultFlag = 1,
                        Data = isCancel
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Failed to cancel order",
                        ResultFlag = 0,
                        Data = isCancel
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Order Part AutoComplete
        [HttpPost]
        public ResultModel GetOrderPartAutoComplete(OrderPartAutoComplete model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                ProductGroupPagePermissions obj = new ProductGroupPagePermissions();
                var data = obj.OrderPartAutoComplete(model.SearchText, userId, role);
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Message = "Order Part Auto Complete Found Successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Order Part Auto Complete not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Order Part Auto Complete not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Order Part Details
        [HttpPost]
        public ResultModel GetOrderPartProducts(OrderPartAutoComplete model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                int totalRecords = 0;
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                ProductGroupPagePermissions obj = new ProductGroupPagePermissions();
                var data = obj.OrderPartProductDetail(model, userId, role, out totalRecords);
                return new ResultModel
                {
                    Message = data.Any() ? "Order Part Products Found Successfully!" : "Order Part Products not Found !",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { ProductList = data, Count = totalRecords }
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Order Part Products not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Outlet List
        [HttpPost]
        public ResultModel OutletList()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var role = GetUserRole();

                var repoOutlet = new RepoOutlet();
                var outlets = repoOutlet.GetOutletList(userId, role);

                return new ResultModel
                {
                    Message = outlets.Count > 0 ? "Outlets found successfully!" : "Failed to find outlets.",
                    ResultFlag = outlets.Count > 0 ? 1 : 0,
                    Data = outlets
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return new ResultModel
                {
                    Message = $"Failed to find outlets. Error: {exc.Message}",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Stock Color API
        [HttpPost]
        public ResultModel GetStockColor()
        {
            try
            {
                RepoOrder obj = new RepoOrder();
                var data = obj.GetStockColor();
                return new ResultModel
                {
                    Message = data.Any() ? "Stock Color Found Successfully!" : "Stock Color not Found !",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Stock Color not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Product Location
        [AllowAnonymous]
        [HttpPost]
        public ResultModel GetProductLocation(ProductLocationRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required field",
                        ResultFlag = 0
                    };
                }
                RepoOutlet obj = new RepoOutlet();
                var data = obj.GetProductLocation(model.ProductId, model.DistributorId);
                return new ResultModel
                {
                    Message = data.Any() ? "Product Location Found Successfully!" : "Product Location not Found !",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Login As apis for mobile

        #region Get Users Role Access by Role
        [HttpPost]
        public ResultModel GetUsersRoleAccess(GetUserOrderModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "User Id is required.",
                        ResultFlag = 0
                    };
                }

                RepoUsers obj = new RepoUsers();
                var data = obj.GetAllUsersRole(model);

                return new ResultModel
                {
                    Message = data.Any() ? "Roles Found Successfully!" : "Roles not Found !",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Roles not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Workshop Code AutoComplete
        [HttpPost]
        public ResultModel GetWorkshopCodeAutoComplete(AutoCompleteModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter workshopCode",
                        ResultFlag = 0
                    };
                }
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                AutoComplete obj = new AutoComplete();
                var data = obj.GetAutoCompleteWorkshopCode(model.SearchText, userId, role);
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Message = "Workshop code found successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Any workshop code not match",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Any workshop code not match",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Location Code Access By User Role
        [HttpPost]
        public ResultModel GetLocationCodeAccess()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                AutoComplete obj = new AutoComplete();
                var data = obj.GetLocationCodeByRole(userId, role);
                if (data.Any())
                {
                    return new ResultModel
                    {
                        Message = "Location code found successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Any location code not match",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Any location code not match",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region All Users Api

        [HttpPost]
        public ResultModel GetAllUsers(UsersPagination model)
        {

            try
            {
                if (string.IsNullOrEmpty(model.UserId))
                {
                    return new ResultModel
                    {
                        Message = "User Id is required.",
                        ResultFlag = 0
                    };
                }

                model.Role = GetUserRole();
                model.PageSize = 10;

                var totalRecords = 0;
                var totalPage = 0;

                var users = new List<UsersResponse>();
                if (!string.IsNullOrEmpty(model.UserId) && !string.IsNullOrEmpty(model.Role))
                {
                    var repoUsers = new RepoUsers();
                    users = repoUsers.GetAllUsers(model, out totalRecords);

                    totalPage = totalRecords / Convert.ToInt32(model.PageSize);
                    if (totalRecords % model.PageSize > 0)
                    {
                        totalPage += 1;
                    }
                }

                return new ResultModel
                {
                    Message = users.Count > 0 ? "Users found successfully!" : "Failed to find users.",
                    ResultFlag = users.Count > 0 ? 1 : 0,
                    Data = new { UserList = users, Count = totalRecords, PageNumber = model.PageNumber, Totalpages = totalPage }
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #endregion

        #region Login Without Password
        [HttpPost]
        public ResultModel LoginWithoutPassword(clsAuth modal)
        {
            try
            {
                if (string.IsNullOrEmpty(modal.Username))
                {
                    return new ResultModel
                    {
                        Message = "User Name Required",
                        ResultFlag = 0
                    };
                }
                var userId = _userManagement.GetUserIdByUsername(modal.Username);
                AuthManagement authManagement = new AuthManagement();
                var user = UserManager.FindByNameAsync(modal.Username);
                user.Wait();
                if (!user.Result.EmailConfirmed)
                {
                    return new ResultModel
                    {
                        Message = "Your account is not approved by administrator.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    RepoUsers repoUsers = new RepoUsers();
                    var userDetail = repoUsers.GetUserDetailByUserId(userId);
                    var token = authManagement.GetToken(modal.Username, Utils.Decrypt(userDetail.Password));
                    if (string.IsNullOrEmpty(token.Error))
                    {
                        var general = new General();
                        token.UserId = user.Result.Id;
                        var roles = _userManager.GetRolesAsync(token.UserId);
                        roles.Wait();
                        token.Roles = roles.Result.OrderByDescending(o => o.ToString()).ToList();
                        token.FullName = repoUsers.getUserFullName(token.UserId, token.Roles);
                        token.Profile = general.CheckImageUrl(!string.IsNullOrEmpty(userDetail?.UserImage) ? userDetail?.UserImage : "/assets/images/NoPhotoAvailable.png");
                        return new ResultModel
                        {
                            Message = "Login successfully.",
                            ResultFlag = 1,
                            Data = token
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Something goes wrong!! Please try again",
                            ResultFlag = 0
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Scheme Status By SchemeId        
        [HttpPost]
        public ResultModel GetSchemeStatus(SchemeUserLevelModel model)
        {
            try
            {
                model.UserId = User.Identity.GetUserId();
                //#if DEBUG
                //                model.UserId = "556118ad-2204-4fd4-ada2-5b2a5c7cc45a"; // Workshop-1020
                //#endif
                if (model.SchemeId < 1)
                {
                    return new ResultModel
                    {
                        Message = "Please provide correct Scheme Id.",
                        ResultFlag = 0
                    };
                }

                var obj = new RepoSchemesDescription();
                var data = obj.GetSchemeStatus(model);
                if (data != null)
                {
                    return new ResultModel
                    {
                        Message = "Successfully retrieved scheme's status.",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Failed to retrieve scheme's status.",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Total Sale's Growth
        [HttpPost]
        public ResultModel TotalSalesGrowth(SaleGrowthFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : e.Exception.Message).ToList();

                    return new ResultModel
                    {
                        Message = errors.Count > 0 ? string.Join(",", errors) : "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }

                var errorMsg = "Failed to get total sales growth.";
                var rd = new RepoDashboard();
                var data = rd.GetTotalSalesGrowthApp(model, ref errorMsg);

                return new ResultModel
                {
                    Message = data != null ? "Total Sales Growth retrieved successfully!" : errorMsg,
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }
        #endregion

        #region GetOutStandingDetail
        [HttpPost]
        public ResultModel GetOutStandingDetail()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                string role = GetUserRole();
                var obj = new RepoDashboard();
                var data = obj.GetOutstandingDetail(userId, role);
                return new ResultModel
                {
                    Message = "Outstanding Detail Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region GetCartItemsCount
        [HttpPost]
        public ResultModel GetCartItemsCount(CartItemCountRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoDashboard();
                var data = obj.GetCartItemCount(model.TempOrderId);
                return new ResultModel
                {
                    Message = "Cart item count Found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Apply PromoCode Api       
        public ResultModel ApplyPromoCode(PromoCodeModel model)
        {
            string Message = "";
            model.UserId = User.Identity.GetUserId();
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => !string.IsNullOrEmpty(e.ErrorMessage) ? e.ErrorMessage : e.Exception.Message);

                    return new ResultModel
                    {
                        Message = errors.Any() ? string.Join(",", errors) : "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }

                var cls = new RepoCheckout();
                var result = cls.ApplyPromoCode(model, out Message);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "PromoCode Successfully Apply.",
                        ResultFlag = 1,
                        Data = result
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = Message,
                        ResultFlag = 0,
                        Data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Remove PromoCode Api       
        public ResultModel RemovePromoCode(PromoCodeModel model)
        {
            model.UserId = User.Identity.GetUserId();
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var cls = new RepoCheckout();
                var result = cls.RemovePromoCode(model);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "PromoCode Successfully Removed.",
                        ResultFlag = 1,
                        Data = result
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Invalid Promocode.",
                        ResultFlag = 0,
                        Data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region AddMoneyInWallet
        [HttpPost]
        public ResultModel AddMoneyInWallet(AddMoneyRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoWallet();
                var data = obj.AddMoneyInWallet(model);
                string textType = string.Empty;
                if (model.Type.Equals("Cr", StringComparison.OrdinalIgnoreCase))
                    textType = "added";
                else
                    textType = "removed";

                return new ResultModel
                {
                    Message = data ? $"{Models.Constants.RupeeSign + model.WalletAmount}  {textType} to your wallet." : $"{Models.Constants.RupeeSign + model.WalletAmount} not {textType} to your wallet.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region GetWorkshopCoupons
        [HttpPost]
        public ResultModel GetWorkshopCoupons(WorkshopCouponRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                int totalRecord = 0;
                decimal wsCurAmt = 0;
                var obj = new RepoWallet();
                var data = obj.GetWorkshopCoupons(model, out totalRecord, out wsCurAmt);
                return new ResultModel
                {
                    Message = data.Any() ? "Workshop coupon found sucessfully." : "Workshop coupon not found.",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { WsCoupons = data, Count = totalRecord, WsCurAmount = wsCurAmt }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion        

        #region Add Workshop Coupon
        [HttpPost]
        public ResultModel GenerateCoupon(GenerateCouponRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoWallet();
                var data = obj.GenerateCoupon(model);
                return new ResultModel
                {
                    Message = data ? "Coupon Added sucessfully." : "This Amount is not available in this workshop.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion    

        #region Workshop Transaction List
        [HttpPost]
        public ResultModel GetWorkshopTransaction(WsTransactionRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoWallet();
                int totalRecord = 0;
                decimal wsCurAmt = 0;
                var data = obj.GetWorkshopTransaction(model, out totalRecord, out wsCurAmt);
                return new ResultModel
                {
                    Message = data.Any() ? "Transaction found sucessfully." : "Transaction not found ",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = new { WsTransaction = data, Count = totalRecord, WsCurAmount = wsCurAmt }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region SearchCouponNumber
        [HttpPost]
        public ResultModel SearchCouponNumber(CouponNoRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoWallet();
                var data = obj.SearchCouponNo(model.CouponNumber);
                return new ResultModel
                {
                    Message = data.Any() ? "Coupon number found sucessfully." : "Coupon number not found ",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region RedeemCouponNo
        [HttpPost]
        public ResultModel RedeemCouponNo(CouponNoRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                string msg = string.Empty;
                var obj = new RepoWallet();
                var data = obj.RedeemCouponNo(model.CouponNumber, out msg);
                return new ResultModel
                {
                    Message = data ? msg : "Coupon number not redeem.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region GetWsWallet
        [HttpPost]
        public ResultModel GetWsWallet(WalletRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                var obj = new RepoWallet();
                var data = obj.GetWorkShopWalletList(model);
                return new ResultModel
                {
                    Message = data.Any() ? "Workshop found sucessfully." : "Workshop not found ",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion    

        #region Get sales chart data
        [HttpPost]
        public ResultModel GetSalesChartData(FilterModel model)
        {
            try
            {
                model.UserId = User.Identity.GetUserId();
                model.Role = GetUserRole();

                string errorMsg = "No sales data available for selected criteria.";
                var rd = new RepoDashboard();
                var data = rd.GetSalesChartData(model, ref errorMsg);

                return new ResultModel
                {
                    Message = data != null ? "Chart data got successfully!" : errorMsg,
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };

            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Account Ledger Closing Api      
        [HttpPost]
        public ResultModel AccountLedgerClosing()
        {
            try
            {
                var model = new AccountLedgerModel
                {
                    UserId = User.Identity.GetUserId(),
                    Role = GetUserRole(),
                    PageSize = 10
                };

                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }

                var raLedger = new RepoAccountLedger();
                var accountLedgers = raLedger.GetAccountLedgerClosing(model, out var totalRecords);

                return new ResultModel
                {
                    Message = accountLedgers.Count > 0 ? "Account Ledgers found successfully!" : "Failed to find Account Ledgers.",
                    ResultFlag = accountLedgers.Count > 0 ? 1 : 0,
                    Data = new { ClosingList = accountLedgers, Count = totalRecords }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }

        #endregion

        #region Account Ledger Closing Details Api       
        [HttpPost]
        public ResultModel AccountLedgerClosingDetails(AccountLedgerModel model)
        {
            try
            {
                if (model.WorkshopId == 0)
                {
                    return new ResultModel
                    {
                        Message = "Please enter Workshop Id.",
                        ResultFlag = 0
                    };
                }

                model.PageSize = 10;
                var raLedger = new RepoAccountLedger();
                var accountLedgerDetails = raLedger.GetAccountLedgerClosingDetails(model, out var totalRecords);

                return new ResultModel
                {
                    Message = accountLedgerDetails.Count > 0 ? "Account Ledger details found successfully!" : "Failed to find Account Ledger details.",
                    ResultFlag = accountLedgerDetails.Count > 0 ? 1 : 0,
                    Data = new { ClosingDetailList = accountLedgerDetails, Count = totalRecords }
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Account Ledger Closing HTML Table
        [HttpPost]
        public ResultModel AccountLedgerClosingHtmlTable(AccountLedgerModel model)
        {
            try
            {
                if (model.WorkshopId == 0)
                {
                    return new ResultModel
                    {
                        Message = "Please enter Workshop Id.",
                        ResultFlag = 0
                    };
                }

                var repoAccountLedger = new RepoAccountLedger();
                var htmlWithTable = repoAccountLedger.GetAccountLedgerHtmlWithTable(model);
                return new ResultModel
                {
                    Message = !string.IsNullOrWhiteSpace(htmlWithTable) ? "HTML with table created successfully!" : "Failed to create HTML with table.",
                    ResultFlag = !string.IsNullOrWhiteSpace(htmlWithTable) ? 1 : 0,
                    Data = new { HtmlWithTable = htmlWithTable }
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }
        #endregion

        #region User Profile Api       
        [HttpPost]
        public ResultModel UserProfile()
        {
            try
            {
                string role = GetUserRole();
                var cls = new UserProfiles();
                var result = cls.GetUserProfile(User.Identity.GetUserId(), role);
                if (result != null)
                {
                    return new ResultModel
                    {
                        Message = "Data found successfully.",
                        ResultFlag = 1,
                        Data = result
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Data not found.",
                        ResultFlag = 0,
                        Data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region User Profile Update Api       
        [HttpPost]
        public ResultModel UpdateUserProfile(ResponseUserProfile model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    if (model.Role == Garaaz.Models.Constants.Workshop || model.Role == Garaaz.Models.Constants.WorkshopUsers)
                    {
                        if (string.IsNullOrEmpty(model.WorkShopName))
                        {
                            return new ResultModel
                            {
                                Message = "WorkShopName required.",
                                ResultFlag = 0
                            };
                        }

                    }
                }
                var cls = new UserProfiles();
                bool status = cls.UpdateUserProfile(model, User.Identity.GetUserId());
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Profile successfully updated.",
                        ResultFlag = 1,
                        Data = null
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Profile not update.",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Razor Pay order Create
        [HttpPost]
        public ResultModel CreateRazorPayOrder(RazorPayRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                var ro = new RepoOrder();
                var data = ro.CreateRazorPayOrderId(model.TempOrderId);
                return new ResultModel
                {
                    Message = !string.IsNullOrEmpty(data.OrderId) ? "Razorpay order created" : "Razorpay order created failed",
                    ResultFlag = !string.IsNullOrEmpty(data.OrderId) ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel CreateRazorPayOrderForOutstanding(RazorPayOutstandingRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                if (model.Amount <= 0)
                {
                    return new ResultModel
                    {
                        Message = "Amount should be atleast 1 ₹",
                        ResultFlag = 0
                    };
                }
                var ro = new RepoOrder();
                var data = ro.CreateRazorPayOrderId(model.Amount, model.UserId);
                return new ResultModel
                {
                    Message = !string.IsNullOrEmpty(data.OrderId) ? "Razorpay order created" : "Razorpay order created failed",
                    ResultFlag = !string.IsNullOrEmpty(data.OrderId) ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public ResultModel OutstandingPayment(RazorPayOutstandingPayment model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                var superAdmins = _userManagement.GetSuperAdminList();
                var ro = new RepoOrder();
                var data = ro.OutstandingPayment(model.Amount, model.UserId, model.RazorpayPaymentId, model.Role, superAdmins);
                return new ResultModel
                {
                    Message = data ? "Outstanding payment successfully" : "Outstanding payment failed",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Sent Mail Change on OrderStatus
        [HttpPost]
        public ResultModel SentmailonStatus(List<ResponseOrderModel> orderstatus)
        {
            try
            {
                foreach (var item in orderstatus)
                {
                    var ru = new RepoUsers();
                    var role = ru.GetRolesByUserId(item.UserID).FirstOrDefault();
                    var rd = new RepoDashboard();
                    var superAdmins = _userManagement.GetSuperAdminList();
                    var result = new System.Threading.Thread(() => rd.SentMailOnStatus(item.OrderID, item.UserID, item.OrderNo, item.OrderStatus, role, superAdmins))
                    { IsBackground = true };
                    result.Start();
                }

                return new ResultModel
                {
                    Message = "Sucessfully mail sent.",
                    ResultFlag = 1,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Support Html
        [HttpPost]
        public ResultModel SaveSupportHtml(Support model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Heading) || string.IsNullOrEmpty(model.Description))
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                var ro = new RepoSupport();
                var data = ro.AddSupportHtml(model);
                return new ResultModel
                {
                    Message = data ? "Support Added Successfully" : "Support Added  failed",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel GetSupportHtml()
        {
            try
            {
                var ro = new RepoSupport();
                var data = ro.GetSupportHtml();
                return new ResultModel
                {
                    Message = data.Any() ? "Support found successfully." : "Support not found.",
                    ResultFlag = data.Any() ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel GetSupportTicket()
        {
            try
            {
                var ro = new RepoSupport();
                var data = ro.GetSupportTicket();
                return new ResultModel
                {
                    Message = data != null ? "Support Ticket found successfully." : "Support Ticket not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        [HttpPost]
        public ResultModel DeleteSupportHtml(Support model)
        {
            try
            {
                RepoSupport rs = new RepoSupport();
                var result = rs.RemoveSupportHtml(model.Id);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Support Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Support Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Support Query

        #region Get All Support Query
        [AllowAnonymous]
        [HttpPost]
        public ResultModel GetAllSupportQuery()
        {
            try
            {
                RepoSupportQuery cls = new RepoSupportQuery();
                var data = cls.GetAllSupportQuery();
                if (data.Count > 0)
                {
                    return new ResultModel
                    {
                        Message = "Support query found successfully",
                        ResultFlag = 1,
                        Data = data
                    };
                }
                return new ResultModel
                {
                    Message = "Support query not found.",
                    ResultFlag = 0,
                    Data = null
                };
            }
            catch
            {
                return new ResultModel
                {
                    Message = "Support query not found. ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Support Query By Id
        public ResultModel GetSupportQueryById(UpdateSupportQuery model)
        {
            if (!ModelState.IsValid)
            {
                return new ResultModel
                {
                    Message = "Please enter all required fields.",
                    ResultFlag = 0
                };
            }
            RepoSupportQuery cls = new RepoSupportQuery();
            var data = cls.GetSupportQueryById(model.Id);
            if (data != null)
            {
                return new ResultModel
                {
                    Message = "Support query found",
                    ResultFlag = 1,
                    Data = data
                };
            }
            else
            {
                return new ResultModel
                {
                    Message = "Support query not found",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save Support Query
        public ResultModel SaveSupportQuery(ResponseSupportQuery model)
        {
            model.UserId = User.Identity.GetUserId();
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                else
                {
                    var superAdmins = _userManagement.GetSuperAdminList();
                    RepoSupportQuery cls = new RepoSupportQuery();
                    bool status = cls.SaveSupportQuery(model, superAdmins);
                    if (status)
                    {
                        return new ResultModel
                        {
                            Message = "Your query saved successfully. We will back to you in 24 hours.",
                            ResultFlag = 1,
                            Data = null
                        };
                    }
                    else
                    {
                        return new ResultModel
                        {
                            Message = "Support query not save/update",
                            ResultFlag = 0,
                            Data = null
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Delete Support Query
        [HttpPost]
        public ResultModel DeleteSupportQuery(UpdateSupportQuery model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                RepoSupportQuery cls = new RepoSupportQuery();
                var result = cls.DeleteSupportQuery(model.Id);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Support query deleted successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Support query not deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #endregion

        #region Accept Back Order
        [HttpPost]
        public ResultModel AcceptBackOrder(CustomerBackOrderModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.OrderNo))
                {
                    return new ResultModel
                    {
                        Message = "Please provide order number.",
                        ResultFlag = 0
                    };
                }
                model.UserId = User.Identity.GetUserId();
                model.Role = GetUserRole();
                var rd = new RepoDashboard();
                var superAdmins = _userManagement.GetSuperAdminList();
                string message = string.Empty;
                var data = rd.AcceptCustomerBackOrder(model.OrderNo, model.PartNumber, model.UserId, model.Role, superAdmins, out message);

                return new ResultModel
                {
                    //Message = data ? "Customer back orders were accepted successfully!" : "Failed to accept customer back orders.",
                    Message = message,
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };

            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get User's All Order Mobile
        [HttpPost]
        public ResultModel GetOrderByUserIdMobile(GetUserOrderModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                string role = GetUserRole();
                RepoOrder repoOrder = new RepoOrder();
                var data = repoOrder.GetOrderByUserIdMobile(model.UserId, role);
                return new ResultModel
                {
                    Data = data,
                    Message = "Order found sucessfully",
                    ResultFlag = 1
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get ChangePhoneNumber Mobile
        [HttpPost]
        public ResultModel ChangePhoneNumber(ChangePhoneNumberModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields.",
                        ResultFlag = 0
                    };
                }
                string userId = User.Identity.GetUserId();
                var result = _userManagement.UpdateUserName(userId, model.PhoneNumber);
                string msg = "";
                if (result.Errors.Any())
                {
                    msg = result.Errors.FirstOrDefault();
                }
                else
                {
                    General cls = new General();
                    cls.UpdateMobile(model.PhoneNumber);
                    msg = "User mobile number updated successfully.";
                }
                return new ResultModel
                {
                    Data = msg,
                    Message = result.Succeeded ? "User mobile number updated successfully." : "User mobile number updated failed.",
                    ResultFlag = result.Succeeded ? 1 : 0
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region User Profile Picture Save and Update Mobile Api       
        [HttpPost]
        public ResultModel SaveUserImage(ResponseUserImageModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                string Imagepath = "";
                var cls = new UserProfiles();
                bool status = cls.SaveUserImage(model, out Imagepath);
                var image = new ResponseImage
                {
                    Image = "http://garaaz.com" + Imagepath
                };
                if (status)
                {
                    return new ResultModel
                    {
                        Message = "Image successfully updated.",
                        ResultFlag = 1,
                        Data = image
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Image not update.",
                        ResultFlag = 0,
                        Data = null
                    };
                }
            }
            catch (Exception)
            {
                return new ResultModel
                {
                    Message = "File format not supported. Please use png, jpe, jpeg file format.",
                    ResultFlag = 0,
                    Data = null
                };
                // return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region  Get Product Name for Combo
        public List<ProductGroupComboList> ComboGroupsDistributor()
        {
            string userId = User.Identity.GetUserId();
            string role = GetUserRole();
            int distributorId = userId.GetDistributorId(role);
            ProductGroupPagePermissions productGroupPagePermissions = new ProductGroupPagePermissions();
            var data = productGroupPagePermissions.ComboProductGroupsDistributor(distributorId);
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }
        #endregion

        #region  Get Distributor Brand
        [HttpPost]
        public List<clsBrand> DistributorBrands(int distributorId)
        {
            var vpp = new VehiclePagePermission();
            var data = vpp.DistributorBrands(distributorId);
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }

        #endregion

        #region  Get ProductGroup Name for DropDownList
        public List<ProductGroupComboList> ComboProductGroup()
        {
            var cls = new ProductGroupPagePermissions();
            var data = cls.ComboProductGroups();
            if (data != null)
            {
                return data;
            }
            else
            {
                return data;
            }
        }

        #endregion

        #region ProductTypeDelete
        [HttpPost]
        public ResultModel DeleteProductType(ProductType model)
        {
            try
            {
                RepoProductType rs = new RepoProductType();
                var result = rs.RemoveProductType(model.Id);
                if (result)
                {
                    return new ResultModel
                    {
                        Message = "Product Type Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Product Type Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Update Focus part benifit type
        [HttpPost]
        public ResultModel UpdateFocusPartBenifitType(FocusPartBenifitTypeModel model)
        {
            try
            {
                RepoSchemes repoSchemes = new RepoSchemes();
                var data = repoSchemes.UpdateFocusPartBenifitType(model);
                if (data)
                {
                    return new ResultModel
                    {
                        Message = "Focus Part Benifit Type updated Successfully",
                        ResultFlag = 1,
                        Data = data,
                    };
                }
                return new ResultModel
                {
                    Message = "Focus Part Benifit Type update Failed",
                    ResultFlag = 0,
                    Data = null,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get LastUploaded Date Mobile
        [HttpPost]
        public ResultModel LastUploadDate(LastuploadModel model)
        {
            try
            {
                GeneralUse cls = new GeneralUse();
                var lastUpload = cls.GetLastFileUploadDate(model);

                return new ResultModel
                {
                    Message = "Lastupload date find successfully.",
                    ResultFlag = 1,
                    Data = lastUpload,
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Save groups by FMS

        [HttpPost]
        public async Task<ResultModel> SaveGroupsByFms(FmsPartsGroup fmsPartsGroup)
        {
            try
            {
                var rs = new RepoSchemes();
                var groupsSaved = await rs.SaveGroupsByFmsAsync(fmsPartsGroup).ConfigureAwait(false);

                return new ResultModel
                {
                    Message = groupsSaved ? "Groups saved successfully by FMS!" : "Failed to save groups by FMS.",
                    ResultFlag = groupsSaved ? 1 : 0,
                    Data = null
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return new ResultModel
                {
                    Message = exc.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }

        #endregion

        #region Dashboard

        #region Dashboard Main

        [HttpPost]
        public ResultModel FetchSalesInfo(DashboardFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();

                var saleMain = new SaleMain();
                var data = saleMain.GetSalesInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Sales found successfully!" : "Sales not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchOutstandingInfo(DashboardFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();

                var osMain = new OutstandingMain();
                var data = osMain.GetOutstandingInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Outstanding found successfully!" : "Outstanding not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchCollectionsInfo(DashboardFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }

                dbFilter.Roles = GetUserRoles();
                var colMain = new CollectionMain();
                var data = colMain.GetCollectionInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Collection found successfully!" : "Collection not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchInventoryInfo(DashboardFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();
                var invMain = new InventoryMain();
                var data = invMain.GetInventoryInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Inventory found successfully!" : "Inventory not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchCboInfo(CboDbFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();
                var cboMain = new CboMain();
                var data = cboMain.GetCboInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Customer back order found successfully!" : "Customer back order not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchSchemesInfo(DashboardFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }

                model.Roles = GetUserRoles();
                var schemeMain = new SchemeMain();
                var data = schemeMain.GetSchemesInfo(model);

                return new ResultModel
                {
                    Message = data != null ? "Schemes found successfully!" : "Schemes not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchWalletBalanceInfo(WalletDbFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();
                var walMain = new WalletMain();
                var data = walMain.GetWalletBalanceInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Wallet info found successfully!" : "Wallet info not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchCustomersInfo(CustomerDbFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();
                var cusMain = new CustomerMain();
                var data = cusMain.GetCustomerInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Customers info found successfully!" : "Customers info not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel FetchLoserAndGainerInfo(LooserGainerFilter dbFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                dbFilter.Roles = GetUserRoles();
                var lgMain = new LoserGainerMain();
                var data = lgMain.GetLoserAndGainerInfo(dbFilter);

                return new ResultModel
                {
                    Message = data != null ? "Losers and Gainers info found successfully!" : "Losers and Gainers info not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #region Sale APis

        [HttpPost]
        public ResultModel DashboardSaleByCategory(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetCategoryWiseSale(model);

                return new ResultModel
                {
                    Message = data != null ? "Sales found successfully!" : "Sales not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleBySubCategory(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetSubGroupWiseSale(model);

                return new ResultModel
                {
                    Message = data != null ? "Sales found successfully!" : "Sales not found.",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #region RoWise Sale Details

        [HttpPost]
        public ResultModel DashboardSaleDetailsRoWise(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetRoWiseSaleDetails(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsRoWiseBranchSale(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetRoWiseBranchSaleDetails(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsRoWiseCustomerSale(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetRoWiseCustomerSaleDetails(model, out _);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #region Sales Executive Wise Sale Details

        [HttpPost]
        public ResultModel DashboardSaleDetailsSeWise(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetSeWiseSaleDetails(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsSeWiseBranchSale(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetSeWiseSaleDetails(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsSeWiseCustomerSale(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetSeWiseCustomerSaleDetails(model, out _);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #region Part Group Wise Sale Details

        [HttpPost]
        public ResultModel DashboardSaleDetailsPgWise(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetPgWiseSaleDetails(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsPgWiseCustomerType(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetPgWiseCustomerSaleDetail(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsPgWiseBranchSale(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetPgWiseBranchSaleDetail(model);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DashboardSaleDetailsPgWiseCustomerSale(SaleDbFilter model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }
                model.Roles = GetUserRoles();
                var saleMain = new SaleMain();
                var data = saleMain.GetPgWiseCustomerSaleDetails(model, out _);

                return new ResultModel
                {
                    Message = data.Count > 0 ? "Sale details found successfully!" : "Sale details not found.",
                    ResultFlag = data.Count > 0 ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(ControllerContext.RouteData.Values["controller"].ToString(), ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Get Workshop Schemes Whose Both Applicable Lucky And Assured Gift
        [HttpPost]
        public ResultModel GetWorkshopTargetAchievedSchemes(GetWorkshopSchemeModel model)
        {
            // TODO:

            // - Send response from this api true or false need to change in app according this

            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "User Id is required.",
                        ResultFlag = 0
                    };
                }

                var rs = new RepoSchemes();
                //var data = rs.GetSchemesGiftAchievedByWorkshopId(model);
                var data = rs.ShowCashbackAndLuckyDraw(model);
                return new ResultModel
                {
                    Message = data ? "User applicable for gift-type selection!" : "User not applicable for gift-type selection.",
                    ResultFlag = data ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return new ResultModel
                {
                    Message = exc.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Save Workshop Schemes Selected Option After TargetAchived
        [HttpPost]
        public ResultModel SaveWorkshopSchemeSelectedOption(WorkshopSchemeSelectType model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter all required fields!",
                        ResultFlag = 0
                    };
                }

                var rs = new RepoSchemes();
                var result = rs.SaveWorkshopSchemeSelectedType(model);

                return new ResultModel
                {
                    Message = result ? "Data save successfully!" : "Failed to save.",
                    ResultFlag = result ? 1 : 0,
                    Data = null
                };
            }
            catch (Exception exc)
            {
                RepoUserLogs.LogException(exc);
                return new ResultModel
                {
                    Message = "Failed to save.",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region New Part Request

        public ResultModel GetAllNewPartRequest()
        {
            try
            {
                var repoNewPart = new RepoNewPart();
                var newPartRequests = repoNewPart.GetNewPartRequests(GetUserRole());

                return new ResultModel
                {
                    Message = newPartRequests.Count > 0 ? "Part requests found successfully!" : "Failed to find part requests.",
                    ResultFlag = newPartRequests.Count > 0 ? 1 : 0,
                    Data = newPartRequests
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel SaveNewPartRequest(NewPartRequestModel nprModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "Please enter required fields.",
                        ResultFlag = 0
                    };
                }
                string Imagepath = "";
                var repoNewPart = new RepoNewPart();
                var saved = repoNewPart.SaveNewPartRequest(nprModel, out Imagepath);

                var image = new ResponseImage
                {
                    Image = "http://garaaz.com" + Imagepath
                };
                return new ResultModel
                {
                    Message = saved ? "New part request saved successfully!" : "Failed to save new part request.",
                    ResultFlag = saved ? 1 : 0,
                    Data = string.IsNullOrEmpty(Imagepath) ? new ResponseImage() : image
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DeleteNewPartRequest(NewPartRequestModel nprModel)
        {
            try
            {
                var repoNewPart = new RepoNewPart();
                var deleted = repoNewPart.DeleteNewPartRequest(nprModel.Id);

                return new ResultModel
                {
                    Message = deleted == true ? "New part request deleted successfully!" : "Failed to delete new part request.",
                    ResultFlag = deleted == true ? 1 : 0,
                    Data = deleted
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #region Approved Part Request

        public ResultModel GetUserRequestsAndParts([FromUri]NewPartRequestModel nprModel)
        {
            try
            {
                var role = GetUserRole();
                var repoNewPart = new RepoNewPart();
                var userRp = repoNewPart.GetUserRequestsAndParts(nprModel, role);

                return new ResultModel
                {
                    Message = userRp.Requests.Count > 0 && userRp.RequestAndParts.Count > 0 ? "User requests and parts found successfully!" : "Failed to find user requests and parts.",
                    ResultFlag = userRp.Requests.Count > 0 && userRp.RequestAndParts.Count > 0 ? 1 : 0,
                    Data = userRp
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        [HttpPost]
        public ResultModel DeletePartForPartRequest(ApproveRequestParam arParam)
        {
            try
            {
                var repoNewPart = new RepoNewPart();
                var deleted = repoNewPart.DeletePartForPartRequest(arParam);

                return new ResultModel
                {
                    Message = deleted ? $"Part '{arParam.PartNumber}' deleted successfully!" : $"Failed to delete part '{arParam.PartNumber}'.",
                    ResultFlag = deleted ? 1 : 0,
                    Data = deleted
                };
            }
            catch (Exception exc)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), exc);
            }
        }

        #endregion

        #region Delete Request PartFilter
        [HttpPost]
        public ResultModel DeleteRequestPartFilter(clsPartfilter model)
        {
            try
            {
                RepoRequestPartFilter repoRequest = new RepoRequestPartFilter();
                var result = repoRequest.DeleteRequestPartFilter(model.Id);
                if (result)
                {

                    return new ResultModel
                    {
                        Message = "Record Deleted Successfully.",
                        ResultFlag = 1
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "Record Not Deleted.",
                        ResultFlag = 0
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Get Part Filters
        [HttpPost]
        public ResultModel GetPartFilters(ClsPartfilterRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "UserId is required.",
                        ResultFlag = 0
                    };
                }

                RepoRequestPartFilter obj = new RepoRequestPartFilter();
                var data = obj.GetPartRequestFilters(model);

                return new ResultModel
                {
                    Message = data != null ? "Part filters Found Successfully!" : "Part filters not Found !",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
                return new ResultModel
                {
                    Message = ex.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Auto Groups For PartFilter
        [HttpPost]
        public ResultModel AutoGroupsForPartFilter(RequestNewPart model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "UserId is required.",
                        ResultFlag = 0
                    };
                }

                RepoRequestPartFilter obj = new RepoRequestPartFilter();
                var data = obj.GroupAutoComplete(model);

                return new ResultModel
                {
                    Message = data != null ? "Part filter groups found successfully!" : "Part filter group not Found !",
                    ResultFlag = data != null ? 1 : 0,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
                return new ResultModel
                {
                    Message = ex.Message,
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Request Part Filter
        [HttpPost]
        public ResultModel GetRequestPartFilter(RequestNewPart model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "UserId is required.",
                        ResultFlag = 0
                    };
                }
                model.PageSize = 10;

                var totalRecords = 0;
                var totalPage = 0;

                RepoRequestPartFilter obj = new RepoRequestPartFilter();
                var partRequestList = obj.GetRequestPartFilters(model, out totalRecords);

                totalPage = totalRecords / Convert.ToInt32(model.PageSize);
                if (totalRecords % model.PageSize > 0)
                {
                    totalPage += 1;
                }
                return new ResultModel
                {
                    Message = partRequestList.Any() ? "Part filter Found Successfully!" : "Part filter not Found !",
                    ResultFlag = partRequestList.Any() ? 1 : 0,
                    Data = new { PartRequestList = partRequestList, Count = totalRecords, PageNumber = model.PageNumber, Totalpages = totalPage }
                };
            }
            catch (Exception ex)
            {
                RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
                return new ResultModel
                {
                    Message = "Part filter not Found ",
                    ResultFlag = 0,
                    Data = null
                };
            }
        }
        #endregion

        #region Get Distributors For WorkShop
        [HttpPost]
        public ResultModel GetDistributorsForWorkShop(WorkShopDistributorsRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel
                    {
                        Message = "UserName is required.",
                        ResultFlag = 0,
                        Data = new WorkShopDistributors().Distributors
                    };
                }

                var role = GetUserRole();
                if (role == Models.Constants.Workshop || role == Models.Constants.WorkshopUsers)
                {
                    var repoUser = new RepoUsers();
                    //var data = repoUser.GetDistributorsForWorkShop(model);
                    var distributors = repoUser.GetWorkshopDistributorsForApp(model.UserName);

                    return new ResultModel
                    {
                        Message = distributors.Count > 0 ? "Distributors found Successfully!" : "Distributors not found.",
                        ResultFlag = distributors.Count > 0 ? 1 : 0,
                        Data = distributors
                    };
                }
                return new ResultModel
                {
                    Message = "User role is not workshop or workshop user.",
                    ResultFlag = 0,
                    Data = new WorkShopDistributors().Distributors
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex);
            }
        }
        #endregion

        #region Set WorkshopUserId
        [HttpPost]
        public ResultModel SetWorkshopUserId(WorkShopDistributors model)
        {
            try
            {
                Utils utils = new Utils();
                utils.setCookiesValue(model.CurrentUserId, Models.Constants.UserIdSession);

                return new ResultModel
                {
                    Message = "UserId set Successfully.",
                    ResultFlag = 1
                };
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion       

        #region Get Distributor UPI Details
        [HttpPost]
        public ResultModel GetDistributorUPIDetails()
        {
            try
            {
                var roles = GetUserRoles();
                if (!roles.Any(s => s.Equals(Models.Constants.Workshop, StringComparison.OrdinalIgnoreCase) || s.Equals(Models.Constants.WorkshopUsers, StringComparison.OrdinalIgnoreCase)))
                {
                    return new ResultModel
                    {
                        Message = "Allowed for workshop only.",
                        ResultFlag = 0,
                        Data = null
                    };
                }
                var repoUpiId = new RepoDistributorUpi();
                var upiDetails = repoUpiId.GetDistributorUPIDetails(User.Identity.GetUserId());
                if (upiDetails != null)
                {
                    return new ResultModel
                    {
                        Message = "Upi details found successfully!!",
                        ResultFlag = 1,
                        Data = upiDetails
                    };
                }
                else
                {
                    return new ResultModel
                    {
                        Message = "failled to find Upi details",
                        ResultFlag = 0,
                        Data =null
                    };
                }
            }
            catch (Exception ex)
            {
                return RepoUserLogs.SendExceptionMailFromController(this.ControllerContext.RouteData.Values["controller"].ToString(), this.ControllerContext.RouteData.Values["action"].ToString(), ex.Message, ex.StackTrace);
            }
        }
        #endregion
    }
}
