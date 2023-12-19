using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Ci.Result;
using Mitake.Sms.Core.Models;
using PhoneNumbers;

namespace Mitake.Sms.Core
{
    public class SmsService : IDisposable
    {
        private readonly HttpClient _smsClient;
        private const string RootUri = "https://smsapi.mitake.com.tw";

        private readonly string _smsAccount;
        private readonly string _smsPassword;

        public SmsService(string account, string password)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            _smsClient = new HttpClient(clientHandler) { BaseAddress = new Uri(RootUri) };
            _smsAccount = account;
            _smsPassword = password;
        }

        /// <summary>
        /// 簡訊發送
        /// </summary>
        /// <param name="model">簡訊內容</param>
        /// <param name="receiverPhone">接收人之手機號碼，多筆接收人時，請以半形逗點隔開( , )，如0912345678,0922333444</param>
        /// <param name="sendTime">簡訊預定發送時間，立即發送：請傳入NULL，預約發送：請傳入預計發送時間，若傳送時間小於系統接單時間，將不予傳送。</param>
        /// <returns></returns>
        public async Task<CiResult<SmsResponse>> SendSmsAsync(SmsModel model)
        {

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "username", _smsAccount },
                { "password", _smsPassword },
                { "dstaddr", model.Mobile },
                { "smbody", model.Content },
            });

            var response = await _smsClient.PostAsync("/api/mtk/SmSend", content).ConfigureAwait(false);
            var ss = await response.Content.ReadAsStringAsync();
            // var responseModel = CreditResponseToModel(response.Body.sendSMSResult);
            //
            var result = new CiResult<SmsResponse>();
            // {
            //     Payload = responseModel
            // };
            //
            // if (responseModel.Credit >= 0)
            //     result.Status = CiStatus.Success;
            // else if (responseModel.Credit == -301.0)
            // {
            //     result.Status = CiStatus.UnAuthorized;
            //     result.Message = "Session 資料不存在，請重新登入。";
            // }
            // else if (responseModel.Credit == -99.0)
            //     result.Message = "主機端發生不明錯誤，請與廠商窗口聯繫。";
            // else
            //     result.Message = "未知錯誤。";
            //
            return result;
        }

        /// <summary>
        /// 簡訊發送
        /// </summary>
        /// <param name="models"></param>
        /// <param name="subject"></param>
        /// <param name="sendTime"></param>
        /// <returns></returns>
        // public async Task<CiResult<SmsResponse>> SendPersonalizedSmsAsync(List<PersonalizedSmsModel> models, string subject = "", DateTime? sendTime = null)
        // {
        //     var xmlDoc = new XmlDocument();
        //     var repsElement = xmlDoc.CreateElement("REPS");
        //     xmlDoc.AppendChild(repsElement);
        //
        //     var phoneUtil = PhoneNumberUtil.GetInstance();
        //
        //     foreach (var model in models)
        //     {
        //         var interMobile = phoneUtil.Parse(model.Mobile, "TW");
        //         ;
        //
        //         var userElement = xmlDoc.CreateElement("USER");
        //         userElement.SetAttribute("NAME", model.Name);
        //         userElement.SetAttribute("MOBILE", phoneUtil.Format(interMobile, PhoneNumberFormat.E164));
        //         userElement.SetAttribute("EMAIL", model.Email);
        //         userElement.SetAttribute("SENDTIME", model.SendTime?.ToString("yyyyMMddHHmmss"));
        //
        //         var cdata = xmlDoc.CreateCDataSection(model.Content);
        //
        //         userElement.AppendChild(cdata);
        //         repsElement.AppendChild(userElement);
        //     }
        //
        //     var xmlStr = xmlDoc.OuterXml;
        //     string sendTimeStr = string.Empty;
        //     if (sendTime.HasValue)
        //         sendTimeStr = sendTime.Value.ToString("yyyyMMddHHmmss");
        //
        //     var response = await _smsClient.sendParamSMSAsync(_sessionKey, subject, xmlStr, sendTimeStr).ConfigureAwait(false);
        //     var responseModel = CreditResponseToModel(response.Body.sendParamSMSResult);
        //
        //     var result = new CiResult<SmsResponse>()
        //     {
        //         Payload = responseModel
        //     };
        //
        //     if (responseModel.Credit >= 0)
        //         result.Status = CiStatus.Success;
        //     else if (responseModel.Credit == -301.0)
        //     {
        //         result.Status = CiStatus.UnAuthorized;
        //         result.Message = $"Session 資料不存在，請重新登入。\r\n Server msg: {responseModel.Message}";
        //     }
        //     else if (responseModel.Credit == -99.0)
        //         result.Message = $"主機端發生不明錯誤，請與廠商窗口聯繫。\r\n Server msg: {responseModel.Message}";
        //     else
        //         result.Message = $"未知錯誤。\r\n Server msg: {responseModel.Message}";
        //
        //     return result;
        // }
        //
        // /// <summary>
        // /// 發送狀態查詢
        // /// </summary>
        // /// <param name="batchId"></param>
        // /// <param name="page"></param>
        // public async Task<getDeliveryStatusResponse> QueryByBatchId(string batchId, int page = 1)
        // {
        //     if (page < 1)
        //         throw new ArgumentOutOfRangeException(nameof(page));
        //
        //     var response = await _smsClient.getDeliveryStatusAsync(_sessionKey, batchId, page.ToString());
        //     return response;
        // }

        /// <summary>
        /// 將回應轉為 Model
        /// </summary>
        /// <param name="responseString"></param>
        /// <returns></returns>
        private SmsResponse CreditResponseToModel(string responseString)
        {
            var values = responseString.Split(',');
            var responseModel = new SmsResponse()
            {
                Credit = double.Parse(values[0]),
            };

            if (values.Length > 2)
            {
                responseModel.Sent = int.Parse(values[1]);
                responseModel.Cost = double.Parse(values[2]);
                responseModel.UnSend = int.Parse(values[3]);
                responseModel.BatchId = Guid.Parse(values[4]);
            }
            else
            {
                responseModel.Message = values[1];
            }

            return responseModel;
        }

        public void Dispose()
        {
            _smsClient?.Dispose();
        }
    }
}