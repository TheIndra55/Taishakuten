using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Assistant.Embedded.V1Alpha2;
using Grpc.Auth;
using Grpc.Net.Client;
using Kurisu.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Kurisu.External.GoogleAssistant
{
    public class Assistant : BaseCommandModule
    {
        // convars
        [ConVar("assistant_endpoint")]
        public static string Endpoint { get; set; } = "https://embeddedassistant.googleapis.com";

        [ConVar("assistant_client_id", HelpText = "The Google Assistant project client id")]
        public static string ClientId { get; set; }

        [ConVar("assistant_client_secret", HelpText = "The Google Assistant project client secret")]
        public static string ClientSecret { get; set; }

        [ConVar("assistant_device_model")]
        public static string DeviceModelId { get; set; }

        [ConVar("assistant_device_id")]
        public static string DeviceId { get; set; }

        // variables
        private static UserCredential UserCredentials { get; set; }

        private static AudioOutConfig AudioOutConfig { get; set; } = new AudioOutConfig
        {
            Encoding = AudioOutConfig.Types.Encoding.OpusInOgg,
            SampleRateHertz = 16000,
            VolumePercentage = 100
        };

        [Command("ga"), Aliases("assistant"), Hidden, RequireOwner]
        public async Task AssistCommand(CommandContext ctx, [RemainingText] string input)
        {
            var secrets = new ClientSecrets
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret
            };

            // check if already authorized, else authorize
            if(UserCredentials == null)
            {
                UserCredentials = await Authorize(secrets);
            }

            // create the request
            var request = new AssistRequest();
            request.Config = CreateConfig("en-US");
            request.Config.TextQuery = input;

            var channel = GrpcChannel.ForAddress(Endpoint, new GrpcChannelOptions
            {
                Credentials = UserCredentials.ToChannelCredentials()
            });
            var client = new EmbeddedAssistant.EmbeddedAssistantClient(channel);

            var assist = client.Assist();
            // send the gRPC request to google assistant
            await assist.RequestStream.WriteAsync(request);

            // first element is the response without audio
            await assist.ResponseStream.MoveNext(CancellationToken.None);
            var response = assist.ResponseStream.Current;

            // read audio stream
            var audioStream = new MemoryStream();
            while (await assist.ResponseStream.MoveNext(CancellationToken.None))
            {
                var current = assist.ResponseStream.Current;
                current.AudioOut.AudioData.WriteTo(audioStream);
            }
            audioStream.Position = 0;

            await ctx.RespondWithFileAsync(audioStream, file_name: "response.ogg", content: response.DialogStateOut?.SupplementalDisplayText);
            audioStream.Dispose();
        }

        private async Task<UserCredential> Authorize(ClientSecrets secrets)
        {
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[] { "https://www.googleapis.com/auth/assistant-sdk-prototype" },
                "user",
                CancellationToken.None);
        }

        private AssistConfig CreateConfig(string language = "en-US")
        {
            var config = new AssistConfig();

            config.AudioOutConfig = AudioOutConfig;
            config.DialogStateIn = new DialogStateIn
            {
                LanguageCode = language
            };

            config.DeviceConfig = new DeviceConfig
            {
                DeviceModelId = DeviceModelId,
                DeviceId = DeviceId
            };

            return config;
        }
    }
}
