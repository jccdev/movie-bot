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
using MovieBot.Worker.Services;

namespace MovieBot.Worker
{
    // TODO implement this worker: https://devblogs.microsoft.com/dotnet/net-core-and-systemd/

    public class Worker : BackgroundService
    {
        private const ulong BotTestChannel = 692578131646611466;
        private const ulong GeneralChannel = 691009601402962126;
        private readonly ILogger<Worker> _logger;
        private DiscordSocketClient _client;
        private readonly IBotCommandsService _botCommandsService;
        private readonly IPollService _pollService;
        private bool _ready = false;

        public Worker(ILogger<Worker> logger, IBotCommandsService botCommandsServiceService, IPollService pollService)
        {
            _botCommandsService = botCommandsServiceService;
            _pollService = pollService;
            _logger = logger;
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _client = new DiscordSocketClient();

                _client.Log += Log;
                _client.MessageReceived += MessageReceived;
                _client.Ready += () => Ready(stoppingToken);

                await _client.LoginAsync(TokenType.Bot,
                    Environment.GetEnvironmentVariable("DISCORD_TOKEN"));

                await _client.StartAsync();
            
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_ready)
                    {
                        await _pollService.HandleExpired(_client);
                    }
                    await Task.Delay(5000, stoppingToken);
                }
                _logger.LogInformation("Worker ended.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred in {nameof(ExecuteAsync)}");
                Environment.Exit(1);
            }
        }

        private Task Log(LogMessage msg)
        {
            _logger.LogInformation(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task MessageReceived(IMessage message)
        {
            await _botCommandsService.ProcessCommands(message);
        }

        private Task Ready(CancellationToken stoppingToken)
        {
            _ready = true;
            return Task.CompletedTask;
        }
    }
}
