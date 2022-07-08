using System.Xml;
using Aptg.KonaKart.Models;
using AptgSmsServiceReference;
using Ci.Result;

namespace Aptg.KonaKart
{
    public class SmsService
    {
        private readonly SMSSoapClient _smsClient;
        private string _sessionKey;

        public SmsService()
        {
            _smsClient = new SMSSoapClient(SMSSoapClient.EndpointConfiguration.SMSSoap);
        }

        /// <summary>
        /// 建立連線
        /// </summary>
        /// <param name="account">登入帳號</param>
        /// <param name="password">密碼</param>
        /// <returns></returns>
        public async Task<CiResult<string>> CreateConnectionAsync(string account, string password)
        {
            var response = await _smsClient.getConnectionAsync(account, password).ConfigureAwait(false);
            var xml = new XmlDocument();
            xml.LoadXml(response.Body.getConnectionResult);
            var node = xml.SelectSingleNode("/SMS/GET_CONNECTION");
            var code = node.SelectSingleNode("CODE").InnerText;
            var key = node.SelectSingleNode("SESSION_KEY").InnerText;
            var description = node.SelectSingleNode("DESCRIPTION").InnerText;

            var result = new CiResult<string>()
            {
                Payload = key,
                Message = $"{code}:{description}"
            };

            if (code == "0")
            {
                result.Status = CiStatus.Success;
                _sessionKey = key;
            }

            return result;
        }

        // 設定連線金鑰
        public void SetSessionKey(string sessionKey)
        {
            _sessionKey = sessionKey;
        }

        /// <summary>
        /// 關閉連線
        /// </summary>
        /// <param name="sessionKey">連線金鑰。建立連線(getConnection)後取得</param>
        /// <returns></returns>
        public async Task<CiResult> CloseConnectionAsync(string sessionKey)
        {
            var response = await _smsClient.closeConnectionAsync(sessionKey).ConfigureAwait(false);
            var result = new CiResult();
            if (response.Body.closeConnectionResult == "1")
                result.Status = CiStatus.Success;

            return result;
        }

        /// <summary>
        /// 簡訊發送
        /// </summary>
        /// <param name="model">簡訊內容</param>
        /// <param name="receiverList">接收人之手機號碼，多筆接收人時，請以半形逗點隔開( , )，如0912345678,0922333444</param>
        /// <param name="sendTime">簡訊預定發送時間，立即發送：請傳入NULL，預約發送：請傳入預計發送時間，若傳送時間小於系統接單時間，將不予傳送。</param>
        /// <returns></returns>
        public async Task<CiResult<SmsResponse>> SendSmsAsync(SmsModel model, List<string> receiverList, DateTime? sendTime = null)
        {
            var receiverStr = string.Join(',', receiverList);
            string sendTimeStr = string.Empty;
            if (sendTime.HasValue)
                sendTimeStr = sendTime.Value.ToString("yyyyMMddHHmmss");

            var response = await _smsClient
                .sendSMSAsync(_sessionKey, model.Subject, model.Content, receiverStr, sendTimeStr)
                .ConfigureAwait(false);

            var responseModel = CreditResponseToModel(response.Body.sendSMSResult);

            var result = new CiResult<SmsResponse>()
            {
                Payload = responseModel
            };

            if (responseModel.Credit >= 0)
                result.Status = CiStatus.Success;
            else if (responseModel.Credit == -301.0)
            {
                result.Status = CiStatus.UnAuthorized;
                result.Message = "Session 資料不存在，請重新登入。";
            }
            else if (responseModel.Credit == -99.0)
                result.Message = "主機端發生不明錯誤，請與廠商窗口聯繫。";
            else
                result.Message = "未知錯誤。";

            return result;
        }

        public async Task<CiResult<SmsResponse>> SendPersonalizedSmsAsync(List<PersonalizedSmsModel> models, string subject = "", DateTime? sendTime = null)
        {
            var xmlDoc = new XmlDocument();
            var repsElement = xmlDoc.CreateElement("REPS");
            xmlDoc.AppendChild(repsElement);

            foreach (var model in models)
            {
                var userElement = xmlDoc.CreateElement("USER");
                userElement.SetAttribute("NAME", model.Name);
                userElement.SetAttribute("MOBILE", model.Mobile);
                userElement.SetAttribute("EMAIL", model.Email);
                userElement.SetAttribute("SENDTIME", model.SendTime?.ToString("yyyyMMddHHmmss"));

                var cdata = xmlDoc.CreateCDataSection(model.Content);

                userElement.AppendChild(cdata);
                repsElement.AppendChild(userElement);
            }

            var xmlStr = xmlDoc.OuterXml;
            string sendTimeStr = string.Empty;
            if (sendTime.HasValue)
                sendTimeStr = sendTime.Value.ToString("yyyyMMddHHmmss");

            var response = await _smsClient.sendParamSMSAsync(_sessionKey, subject, xmlStr, sendTimeStr).ConfigureAwait(false);
            var responseModel = CreditResponseToModel(response.Body.sendParamSMSResult);

            var result = new CiResult<SmsResponse>()
            {
                Payload = responseModel
            };

            if (responseModel.Credit >= 0)
                result.Status = CiStatus.Success;
            else if (responseModel.Credit == -301.0)
            {
                result.Status = CiStatus.UnAuthorized;
                result.Message = "Session 資料不存在，請重新登入。";
            }
            else if (responseModel.Credit == -99.0)
                result.Message = "主機端發生不明錯誤，請與廠商窗口聯繫。";
            else
                result.Message = "未知錯誤。";

            return result;
        }

        private SmsResponse CreditResponseToModel(string responseString)
        {
            var values = responseString.Split(',');
            var responseModel = new SmsResponse()
            {
                Credit = double.Parse(values[0]),
                Sent = int.Parse(values[1]),
                Cost = double.Parse(values[2]),
                UnSend = int.Parse(values[3]),
                BatchId = Guid.Parse(values[4])
            };

            return responseModel;
        }
    }
}
