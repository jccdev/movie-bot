using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MovieBot.Worker
{
    // TODO implement this worker: https://devblogs.microsoft.com/dotnet/net-core-and-systemd/

    public class Worker : BackgroundService
    {
        private const ulong BotTestChannel = 692578131646611466;
        private const ulong GeneralChannel = 691009601402962126;
        private readonly ILogger<Worker> _logger;
        private DiscordSocketClient _client;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            _client.Ready += Ready;


            await _client.LoginAsync(TokenType.Bot,
                Environment.GetEnvironmentVariable("DISCORD_TOKEN"));

            await _client.StartAsync();
            await Task.Delay(-1, stoppingToken);
        }

        private Task Log(LogMessage msg)
        {
            _logger.LogDebug(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(IMessage message)
        {
            var echoText = "!movie-bot echo ";

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

            else if (message.Content.StartsWith(echoText, StringComparison.CurrentCulture))
            {
                await message.Channel.SendMessageAsync(message.Content.Remove(0, echoText.Length));
            }

            else if (message.Content.StartsWith("!movie-bot ") && message.Content.Contains("fbi", StringComparison.CurrentCultureIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Reported to FBI!");
            }


            else if (message.Content.StartsWith("!movie-bot"))
            {
                var response = "I do not understand the command." + Environment.NewLine + ">>> " + message.Content;
                if (message.Source == MessageSource.User)
                {
                    response = message.Author.Mention + ", " + response;
                }
                await message.Channel.SendMessageAsync(response);
            }
        }

        private async Task Ready()
        {
            var botTestChannel = _client.GetChannel(BotTestChannel) as SocketTextChannel;

            var cached = botTestChannel.GetCachedMessages();

            if (cached.Any())
            {
                foreach(var msg in cached)
                {
                    await MessageReceived(msg);
                }
            }
        }
    }
}
