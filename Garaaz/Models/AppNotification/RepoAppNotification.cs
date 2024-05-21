using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;

namespace Garaaz.Models.AppNotification
{
    public class RepoAppNotification
    {
        #region Variables  
        garaazEntities db = new garaazEntities();
        #endregion

        #region Send Push Notification
        public void SendPushNotification(PushNotificationUserModel model)
        {
            int failed, success;
            if (!string.IsNullOrEmpty(model.OrderNumber))
            {
                var notificationModel = new PushNotification()
                {
                    Title = "New order placed.",
                    Message = $"Order number : {model.OrderNumber}",
                    FCMToken = new List<string>()
                };

                int distributorId = model.UserId.GetDistributorId(model.Role);

                var distributorFcmToken = (from d in db.DistributorUsers.AsNoTracking()
                                           join u in db.UserDetails.AsNoTracking() on d.UserId equals u.UserId
                                           where d.DistributorId == distributorId && u.FcmToken != null
                                           select u).FirstOrDefault()?.FcmToken;
                if (distributorFcmToken != null)
                {
                    notificationModel.FCMToken.Add(distributorFcmToken);
                }

                var ws = db.DistributorWorkShops.FirstOrDefault(w => w.UserId == model.UserId);
                if (ws != null)
                {
                    var salesFcmTokens = (from se in db.SalesExecutiveWorkshops.AsNoTracking()
                                          join u in db.UserDetails.AsNoTracking() on se.UserId equals u.UserId
                                          where se.WorkshopId == ws.WorkShopId && u.FcmToken != null
                                          select u.FcmToken).ToList();

                    notificationModel.FCMToken.AddRange(salesFcmTokens);

                    var roUserFcmToken = (from o in db.DistributorsOutlets.AsNoTracking()
                                          join u in db.UserDetails.AsNoTracking() on o.UserId equals u.UserId
                                          where o.OutletId == ws.WorkShop.outletId && u.FcmToken != null
                                          select u.FcmToken).FirstOrDefault();

                    if (roUserFcmToken != null)
                    {
                        notificationModel.FCMToken.Add(roUserFcmToken);
                    }
                }
                var response = SendNotificationFromFirebaseCloud(notificationModel);
                success = response.success;
                failed = response.failure;
            }
            else if (model.SchemeId > 0)
            {
                var scheme = db.Schemes.FirstOrDefault(s => s.SchemeId == model.SchemeId);
                if (scheme == null) { return; }

                var notificationModel = new PushNotification()
                {
                    Title = "New scheme created.",
                    Message = $"Scheme name : {scheme.SchemeName}",
                    FCMToken = new List<string>()
                };
                var workshopFcmTokens = (from dw in db.DistributorWorkShops.AsNoTracking()
                                         join u in db.UserDetails.AsNoTracking() on dw.UserId equals u.UserId
                                         join tw in db.TargetWorkShops.AsNoTracking() on dw.WorkShopId equals tw.WorkShopId
                                         where tw.SchemeId == model.SchemeId && u.FcmToken != null
                                         select u.FcmToken).ToList();

                notificationModel.FCMToken.AddRange(workshopFcmTokens);

                var response = SendNotificationFromFirebaseCloud(notificationModel);
                success = response.success;
                failed = response.failure;
            }

        }
        #endregion

        public FCMResponse SendNotificationFromFirebaseCloud(PushNotification model)
        {
            var response = new FCMResponse();
            // refrence url
            //https://stackoverflow.com/questions/37412963/send-push-to-android-by-c-sharp-using-fcm-firebase-cloud-messaging
            var result = "-1";
            var webAddr = "https://fcm.googleapis.com/fcm/send";
            foreach (var token in model.FCMToken)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                // use server-key as key
                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "key=AAAAkP2K2fA:APA91bGFutDyVcV3Hh2mlVdLLx2EZjmcpqgRXs1u6rExBIX1n72OhQ0lUIPja_KVyAWa0XiklPfTT43B6Baw-8fVEh6kzI5DBt71MToXimbfMg1G1fC3EMLgssNQ0Pfop_69hG0UD3n2");
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string strNJson = "{\"to\": \"" + token + "\",\"notification\": {\"body\": \"" + model.Message + "\",\"title\":\"" + model.Title + "" + "\",\"content_available\":\"" + true + "\",\"priority\":\"high\",\"pushType\":\"user_challenge\"},\"data\":{\"body\": \"" + model.Message + "\",\"title\":\"" + "Testing" + "\",\"content_available\":\"" + true + "\",\"priority\":\"high\"}}";
                    streamWriter.Write(strNJson);
                    streamWriter.Flush();
                }


                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                var FCMResponse = JsonConvert.DeserializeObject<FCMResponse>(result);
                response.success += FCMResponse.success;
                response.failure += FCMResponse.failure;
                //response.results.AddRange(new List<string> { FCMResponse.results.ToList() });
            }
            return response;
        }
    }
}