using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieBot.Worker.Constants;
using MovieBot.Worker.Models;
using NLog;

namespace MovieBot.Worker.Services
{
    public class BotCommandsService : IBotCommandsService
    {
        private const string EchoText = "!movie-bot echo ";
        private readonly IPollService _pollService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BotCommandsService> _logger;

        public BotCommandsService(IPollService pollService, IConfiguration configuration, ILogger<BotCommandsService> logger, MovieBotDetails details)
        {
            _pollService = pollService;
            _configuration = configuration;
            _logger = logger;
            _logger.LogInformation($"Environment: {details.EnvironmentName}");
        }

        public async Task ProcessCommands(IMessage message)
        {
            var included = _configuration.GetSection("Bot:Included").Get<ulong[]>() ?? Enumerable.Empty<ulong>();
            var excluded = _configuration.GetSection("Bot:Excluded").Get<ulong[]>() ?? Enumerable.Empty<ulong>();

            _logger.LogInformation($"Included: {string.Join(", ", included.Select(x => x.ToString()))}.");
            _logger.LogInformation($"Excluded: { string.Join(", ", excluded.Select(x => x.ToString()))}.");
            
            if (
                (included.Any() && included.All(i => i != message.Channel.Id))
                || excluded.Any(e => e == message.Channel.Id))
            {
                return;
            }
            
            if (message.Content == "!movie-bot" || message.Content == "!movie-bot help")
            {
                await Help(message);
            }

            else if (message.Content == "!movie-bot ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }

            else if (message.Content.StartsWith(EchoText, StringComparison.CurrentCulture))
            {
                await message.Channel.SendMessageAsync(message.Content.Remove(0, EchoText.Length));
            }

            else if (message.Content.StartsWith("!movie-bot ") && message.Content.Contains("fbi", StringComparison.CurrentCultureIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Reported to FBI!");
            }

            else if (message.Content == "!movie-bot popcorn")
            {
                await message.Channel.SendMessageAsync(":popcorn::popcorn::popcorn::popcorn::popcorn:");
            }

            else if (message.Content.StartsWith("!movie-bot ban"))
            {
                await Ban(message);
            }

            else if (message.Content.StartsWith(CommandText.Poll))
            {
                await Poll(message);
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

        private async Task Poll(IMessage message)
        {

            var poll = MapToPoll(message);

            var builder = new StringBuilder();
            var reactions = new List<IEmote>();
            if (poll != null)
            {
                builder.AppendLine($"**POLL**");
                builder.AppendLine();
                builder.AppendLine($"> {poll.Question}");
                var i = 0;
                foreach (var answer in poll.Answers)
                {
                    builder.AppendLine();
                    var emoji = Indicators.All[i];
                    answer.Reaction = emoji;
                    reactions.Add(new Emoji(emoji));
                    builder.AppendLine($"{emoji} - *{answer.Answer}*");
                    i++;
                }
                builder.AppendLine();
                var minutes = TimeSpan.FromSeconds(poll.Expires);
                builder.AppendLine($"Poll ends in {minutes.TotalMinutes} minute(s).");
                builder.AppendLine();
                builder.AppendLine("Please choose:");
            }
            else
            {
                builder.AppendLine("POLL Instructions:");
                builder.AppendLine();
                builder.AppendLine("!movie-bot poll `Question` | `Answer 1` | `Answer 2`");
                builder.AppendLine("Limit of 10 answers.");
            }

            var res = await message.Channel.SendMessageAsync(builder.ToString());

            if (poll != null)
            {
                poll.PollMessageId = res.Id;
                await _pollService.Add(poll);
            }

            var sentMessage =  await message.Channel.GetMessageAsync(res.Id) as IUserMessage; 
            await sentMessage.AddReactionsAsync(reactions.ToArray());
        }

        private async Task Ban(IMessage message)
        {
            if (message.MentionedUserIds.Any())
            {
                var users = new List<IUser>();
                foreach (var uId in message.MentionedUserIds)
                {
                    var user = await message.Channel.GetUserAsync(uId);
                    users.Add(user);
                }

                var builder = new StringBuilder();

                builder.AppendLine(string.Join(" ", users.Select(u => u.Mention)));

                builder.AppendLine(
                    "> Well, there's this passage I got memorized, sorta fits the occasion. \"Ezekiel 25:17\". \"The path of the righteous man is beset on all sides by the iniquities of the selfish and the tyranny of evil men. Blessed is he who in the name of cherish and good will shepherds the weak through the valley of darkness for he is truly his keeper and the finder of lost children. And I will strike down upon thee with great vengeance and furious anger those who attempt to poison and destroy my brothers. And you will know my name is the Lord when I lay my vengeance upon thee.\" ");

                var imgPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Static Assets/pulpfiction_new.jpg");
                await message.Channel.SendFileAsync(imgPath, builder.ToString());
            }
        }
        
        private Poll MapToPoll(IMessage message)
        {            
            var pollArgs = message.Content.
                Remove(0, CommandText.Poll.Length)
                .Split("|")
                .Select(t => t.Trim())
                .ToList();
            
            if (pollArgs?.Count() > 1)
            {
                var answers = pollArgs.Skip(1).Select(pa => new PollAnswer()
                {
                    Answer = pa
                }).ToList();
                
                return new Poll
                {
                    MessageId = message.Id,
                    ChannelId = message.Channel.Id,
                    Question = pollArgs.ElementAt(0),
                    CreatorId = message.Author.Id,
                    Answers = answers,
                };
            }

            return null;
        }

        private async Task Help(IMessage message)
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
    }
}
