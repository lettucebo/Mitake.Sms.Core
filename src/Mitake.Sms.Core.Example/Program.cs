using Microsoft.Extensions.Configuration;
using Mitake.Sms.Core.Models;

namespace Mitake.Sms.Core.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            await SendSingleSmsAsync(config).ConfigureAwait(false);
        }

        private static async Task SendSingleSmsAsync(IConfigurationRoot config)
        {
            using (var smsService = new SmsService(config["SmsAccount"], config["SmsPassword"]))
            {
                var model = new SmsModel()
                {
                    Mobile = config["Mobile"],
                    Content = "This is a test msg."
                };

                var sendResult = await smsService.SendSmsAsync(model).ConfigureAwait(false);

                Console.WriteLine(
                    $"Send result: {sendResult.Status}, {sendResult.Message}, BatchId: {sendResult.Payload.BatchId}");

                Console.WriteLine("Connection successfully closed.");
            }
        }

        private static async Task SendMultiSmsAsync(IConfigurationRoot config)
        {
            using (var smsService = new SmsService(config["SmsAccount"], config["SmsPassword"]))
            {
                var models = new List<SmsModel>()
                {
                    new SmsModel()
                    {
                        Mobile = config["Mobile"],
                        Content = "This is a test msg for Money.",
                        Name = "Money"
                    },
                    new SmsModel()
                    {
                        Mobile = config["Mobile"],
                        Content = "This is a test msg for Din but is Money.",
                        Name = "Din"
                    }
                };

                var sendResult = await smsService.SendBulkSmsAsync(models).ConfigureAwait(false);

                Console.WriteLine(
                    $"Send result: {sendResult.Status}, {sendResult.Message}");

                Console.WriteLine("Connection successfully closed.");
            }
        }
    }
}