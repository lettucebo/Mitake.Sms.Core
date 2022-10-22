using Microsoft.Extensions.Configuration;

namespace Aptg.KonaKart.Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            Console.WriteLine("Hello, World!");
            var smsService = new SmsService();
            var connResult = await smsService.CreateConnectionAsync(config["SmsAccount"], config["SmsPassword"]).ConfigureAwait(false);

            var sessionKey = connResult.Payload;

            await smsService.CloseConnectionAsync(sessionKey).ConfigureAwait(false);
        }
    }
}