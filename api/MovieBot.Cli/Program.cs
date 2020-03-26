using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MovieBot.Cli
{
    class Program
    {
        const ulong BotTestChannel = 692578131646611466;

        const ulong GeneralChannel = 691009601402962126;

        private static DiscordSocketClient _client;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            _client = new DiscordSocketClient();


            _client.MessageReceived += MessageReceived;

            _client.Ready += Ready;


            // Remember to keep token private or to read it from an 
            // external source! In this case, we are reading the token 
            // from an environment variable. If you do not know how to set-up
            // environment variables, you may find more information on the 
            // Internet or by using other methods such as reading from 
            // a configuration.
            await _client.LoginAsync(TokenType.Bot,
                Environment.GetEnvironmentVariable("DISCORD_TOKEN"));

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static async Task MessageReceived(SocketMessage message)
        {

            if (message.Content == "!movie-bot" || message.Content == "!movie-bot help")
            {
                var builder = new StringBuilder();

                builder.AppendLine("MovieBot Help");
                builder.AppendLine();
                builder.AppendLine("All commands begin with \"!movie-bot\"");
                builder.AppendLine();
                builder.AppendLine("Command List:");
                builder.AppendLine();
                builder.AppendLine("`!movie-bot ping`");
                builder.AppendLine("`!movie-bot echo`");

                await message.Channel.SendMessageAsync(builder.ToString());
            }

            else if (message.Content == "!movie-bot ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }

            else if (message.Content.StartsWith("!movie-bot echo ", StringComparison.CurrentCulture))
            {
                await message.Channel.SendMessageAsync(message.Content.Remove(0, 6));
            }

            else if (message.Content.StartsWith("!movie-bot ") && message.Content.Contains("fbi", StringComparison.CurrentCultureIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Reported to FBI!");
            }


            else if (message.Content.StartsWith("!movie-bot"))
            {
                var response = "I do not understand the command." + Environment.NewLine + ">>> " + message.Content;
                if(message.Source == MessageSource.User)
                {
                   response = message.Author.Mention + ", " + response;
                }
                await message.Channel.SendMessageAsync(response);
            }
        }

        private static async Task Ready()
        {
            var botTestChannel = _client.GetChannel(GeneralChannel) as SocketTextChannel;
        }
    }
}
