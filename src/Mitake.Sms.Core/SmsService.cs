using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Ci.Extension.Core;
using Ci.Result;
using Ci.Sequential;
using IniParser.Parser;
using Mitake.Sms.Core.Models;

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
        /// <returns></returns>
        public async Task<CiResult<SmsResponse>> SendSmsAsync(SmsModel model)
        {
            var dict = new Dictionary<string, string>()
            {
                { "clientid", model.ClientId.HasValue ? model.ClientId.Value.ToString() : GuidSequential.NewGuid().ToString() },
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
                Payload = responseModel.First()
            };

            result.Status = responseModel.First().Status.ToCiStatus();
            result.Message = responseModel.First().Status.GetDescription();

            return result;
        }

        /// <summary>
        /// 批次簡訊發送
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<CiResult<List<SmsResponse>>> SendBulkSmsAsync(List<SmsModel> models)
        {
            // Mitake only allows 500 messages per request.
            var modelChunks = models.Chunk(500);
            var resultList = new List<CiResult<List<SmsResponse>>>();

            foreach (var chunk in modelChunks)
            {
                var body = new StringBuilder();
                foreach (var model in chunk)
                {
                    var clientId = model.ClientId.HasValue ? model.ClientId.Value.ToString() : GuidSequential.NewGuid().ToString();
                    body.AppendLine(
                        $"{clientId}$${model.Mobile}$${string.Empty}$${string.Empty}$${model.Name}$$$${model.Content}");
                }

                var content = new StringContent(body.ToString());

                var response = await _smsClient
                    .PostAsync(
                        $"/api/mtk/SmBulkSend?username={_smsAccount}&password={_smsPassword}&Encoding_PostIn=UTF-8"
                        , content).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var responseModel = CreditResponseToModel(responseContent);
                var result = new CiResult<List<SmsResponse>>()
                {
                    Payload = responseModel
                };

                var status = responseModel.FirstOrDefault(x => x.Status.ToCiStatus() != CiStatus.Success)?.Status ?? responseModel.First().Status;

                result.Status = status.ToCiStatus();
                result.Message = status.GetDescription();

                resultList.Add(result);
            }

            if (resultList.Count == 1)
                return resultList.First();

            var finalResult = resultList.FirstOrDefault(x => x.Status != CiStatus.Success) ??
                              resultList.First();
            finalResult.Payload = resultList.SelectMany(x => x.Payload).ToList();
            return finalResult;
        }

        /// <summary>
        /// Converts a credit response string to a list of SmsResponse models.
        /// </summary>
        /// <param name="responseString">The response string to be parsed.</param>
        /// <returns>A list of SmsResponse models.</returns>
        private List<SmsResponse> CreditResponseToModel(string responseString)
        {
            var parser = new IniDataParser();
            var iniData = parser.Parse(responseString);

            var iniList = iniData.Sections.ToList();

            var result = new List<SmsResponse>();

            if (iniList.Count == 0)
            {
                var statusStr = iniData.Global.GetKeyData("statuscode").Value;
                result.Add(new SmsResponse()
                {
                    Status = CodeStringToFlag(statusStr),
                    MsgId = string.Empty,
                    BatchId = string.Empty,
                    Credit = -1,
                    Cost = -1,
                    IsDuplicate = false
                });

                return result;
            }

            foreach (var item in iniList)
            {
                var responseModel = new SmsResponse()
                {
                    Credit = double.Parse(item.Keys.FirstOrDefault(x => x.KeyName == "AccountPoint")?.Value ?? "-1"),
                    BatchId = item.SectionName,
                    Cost = double.Parse(item.Keys.FirstOrDefault(x => x.KeyName == "smsPoint")?.Value ?? "-1"),
                    IsDuplicate = (item.Keys.FirstOrDefault(x => x.KeyName == "Duplicate")?.Value ?? string.Empty) == "Y",
                    MsgId = item.Keys.FirstOrDefault(x => x.KeyName == "msgid")?.Value ?? string.Empty,
                };

                var statusStr = item.Keys.FirstOrDefault(x => x.KeyName == "statuscode")?.Value ?? string.Empty;

                responseModel.Status = CodeStringToFlag(statusStr);

                result.Add(responseModel);
            }

            return result;
        }

        private StatusFlag CodeStringToFlag(string statusStr)
        {
            return statusStr switch
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
        }

        public void Dispose()
        {
            _smsClient?.Dispose();
        }
    }
}