using Ci.Result;
using Microsoft.Extensions.Configuration;
using Mitake.Sms.Core.Models;

namespace Mitake.Sms.Core.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            using (var smsService = new SmsService(config["SmsAccount"], config["SmsPassword"]))
            {
                var model = new SmsModel()
                {
                    Mobile = config["Mobile"],
                    Content = "This is a test msg."
                };
                

                await smsService.SendSmsAsync(model);

                // await smsService.QueryByBatchId("00000000-0000-0000-0000-000000000000");

                //var sendResult = await smsService.SendPersonalizedSmsAsync(sendList).ConfigureAwait(false);

                //Console.WriteLine(
                //    $"Send result: {sendResult.Status}, {sendResult.Message}, BatchId: {sendResult.Payload.BatchId}");

                Console.WriteLine("Connection successfully closed.");
            }
        }
    }
}