using Aptg.KonaKart.Models;
using Ci.Result;
using Microsoft.Extensions.Configuration;

namespace Aptg.KonaKart.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            var smsService = new SmsService();
            Console.WriteLine("Connecting to sms client...");
            var connResult = await smsService.CreateConnectionAsync(config["SmsAccount"], config["SmsPassword"])
                .ConfigureAwait(false);
            if (connResult.Status != CiStatus.Success)
            {
                Console.WriteLine($"Establish connection fail: {connResult.Status}, {connResult.Message}");
                return;
            }

            Console.WriteLine(
                $"Connection establish success, {connResult.Status}, {connResult.Message}, sessionKey: {connResult.Payload}");

            var sessionKey = connResult.Payload;

            var sendList = new List<PersonalizedSmsModel>()
            {
                new PersonalizedSmsModel()
                {
                    Name = "Demo",
                    Mobile = config["Mobile"],
                    Content = "This is a test msg."
                }
            };

            await smsService.QueryByBatchId("00000000-0000-0000-0000-000000000000");

            //var sendResult = await smsService.SendPersonalizedSmsAsync(sendList).ConfigureAwait(false);

            //Console.WriteLine(
            //    $"Send result: {sendResult.Status}, {sendResult.Message}, BatchId: {sendResult.Payload.BatchId}");


            await smsService.CloseConnectionAsync(sessionKey).ConfigureAwait(false);
            Console.WriteLine("Connection successfully closed.");
        }
    }
}