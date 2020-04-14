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
using MovieBot.Worker.Constants;
using MovieBot.Worker.Services;

namespace MovieBot.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private DiscordSocketClient _client;
        private readonly IBotCommandsService _botCommandsService;
        private readonly IPollService _pollService;
        private bool _ready = false;
        private readonly IPromptService _promptService;

        public Worker(ILogger<Worker> logger, IBotCommandsService botCommandsServiceService, IPollService pollService, IPromptService promptService)
        {
            _botCommandsService = botCommandsServiceService;
            _pollService = pollService;
            _promptService = promptService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _client = new DiscordSocketClient();

                _client.Log += Log;
                _client.ReactionAdded += ReactionAdded;
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

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> cached, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var message = await cached.GetOrDownloadAsync();
            if (message.Author.Id == BotConstants.BotUserId &&
                reaction.UserId != BotConstants.BotUserId)
            {
                await _pollService.ProcessPollConfigResponse(channel, message, reaction);
                await _promptService.ProcessPendingPrompts(message, reaction, _client.Rest);
            }
        }
    }
}
