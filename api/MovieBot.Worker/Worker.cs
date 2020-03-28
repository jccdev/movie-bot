using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
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
            _logger.LogInformation(msg.ToString());
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
                builder.AppendLine("`!movie-bot popcorn`");
                builder.AppendLine("`!movie-bot ban @name1 @name2`");

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

            else if(message.Content == "!movie-bot popcorn")
            {
                await message.Channel.SendMessageAsync(":popcorn::popcorn::popcorn::popcorn::popcorn:");
            }

            else if (message.Content.StartsWith("!movie-bot ban"))
            {
                if(message.MentionedUserIds.Any())
                {

                    var users = new List<IUser>();
                    foreach (var uId in message.MentionedUserIds)
                    {
                        var user = await message.Channel.GetUserAsync(uId);
                        users.Add(user);
                    }

                    var builder = new StringBuilder();

                    builder.AppendLine(string.Join(" ", users.Select(u => u.Mention)));

                    builder.AppendLine("> Well, there's this passage I got memorized, sorta fits the occasion. \"Ezekiel 25:17\". \"The path of the righteous man is beset on all sides by the iniquities of the selfish and the tyranny of evil men. Blessed is he who in the name of cherish and good will shepherds the weak through the valley of darkness for he is truly his keeper and the finder of lost children. And I will strike down upon thee with great vengeance and furious anger those who attempt to poison and destroy my brothers. And you will know my name is the Lord when I lay my vengeance upon thee.\" ");

                    var imgPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static Assets/pulpfiction_new.jpg");
                    await message.Channel.SendFileAsync(imgPath, builder.ToString());
                }
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
            var botTestChannel = _client.GetChannel(GeneralChannel) as SocketTextChannel;
        }
    }
}
