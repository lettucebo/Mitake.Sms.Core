using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Ci.Extension.Core;
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
            var dict = new Dictionary<string, string>()
            {
                { "username", _smsAccount },
                { "password", _smsPassword },
                { "dstaddr", model.Mobile },
                { "smbody", model.Content },
            };

            if (model.SendTime.HasValue)
                dict.Add("dlvtime", model.SendTime.Value.ToString("yyyyMMddHHmmss"));

            var content = new FormUrlEncodedContent(dict);

            var response = await _smsClient.PostAsync("/api/mtk/SmSend", content).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var responseModel = CreditResponseToModel(responseContent);
            var result = new CiResult<SmsResponse>()
            {
                Payload = responseModel
            };

            if (responseModel.Status is StatusFlag.Zero or StatusFlag.One or StatusFlag.Two or StatusFlag.Four)
            {
                result.Status = CiStatus.Success;
                result.Message = responseModel.Status.GetDescription();
            }
            else if (responseModel.Status is StatusFlag.C or StatusFlag.D or StatusFlag.E or StatusFlag.F or StatusFlag.H or StatusFlag.K or StatusFlag.L or StatusFlag.M or StatusFlag.N or StatusFlag.P or StatusFlag.R or StatusFlag.S )
            {
                result.Status = CiStatus.UnAuthorized;
                result.Message = responseModel.Status.GetDescription();
            }
            else
            {
                result.Status = CiStatus.Failure;
                result.Message = responseModel.Status.GetDescription();
            }

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
            var values = responseString.Split("\r\n");

            int removeIndex = 0;
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i].Contains("[") && values[i].Contains("]"))
                {
                    removeIndex = i;
                    break;
                }
            }

            values = values.Where((x, i) => i < removeIndex).ToArray();

            var responseModel = new SmsResponse()
            {
                Credit = double.Parse(values.FirstOrDefault(x => x.StartsWith("AccountPoint"))?.Split("=")[1] ?? "0"),
                BatchId = values.FirstOrDefault(x => x.StartsWith("msgid"))?.Split("=")[1] ?? string.Empty,
                Cost = double.Parse(values.FirstOrDefault(x => x.StartsWith("AccountPoint"))?.Split("=")[1] ?? "0"),
            };

            var statusStr = values.FirstOrDefault(x => x.StartsWith("statuscode"))?.Split("=")[1] ?? string.Empty;

            responseModel.Status = statusStr switch
            {
                "0" => StatusFlag.Zero,
                "1" => StatusFlag.One,
                "2" => StatusFlag.Two,
                "4" => StatusFlag.Four,
                "5" => StatusFlag.Five,
                "6" => StatusFlag.Six,
                "7" => StatusFlag.Seven,
                "8" => StatusFlag.Eight,
                "9" => StatusFlag.Nine,
                "a" => StatusFlag.A,
                "b" => StatusFlag.B,
                "c" => StatusFlag.C,
                "d" => StatusFlag.D,
                "e" => StatusFlag.E,
                "f" => StatusFlag.F,
                "h" => StatusFlag.H,
                "k" => StatusFlag.K,
                "l" => StatusFlag.L,
                "m" => StatusFlag.M,
                "n" => StatusFlag.N,
                "p" => StatusFlag.P,
                "r" => StatusFlag.R,
                "s" => StatusFlag.S,
                "t" => StatusFlag.T,
                "u" => StatusFlag.U,
                "v" => StatusFlag.V,
                "w" => StatusFlag.W,
                "x" => StatusFlag.X,
                "y" => StatusFlag.Y,
                "z" => StatusFlag.Z,
                _ => StatusFlag.Star
            };

            return responseModel;
        }

        public void Dispose()
        {
            _smsClient?.Dispose();
        }
    }
}