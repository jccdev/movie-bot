using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieBot.Worker.Constants;
using MovieBot.Worker.Exceptions;
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
        private readonly IPromptService _promptService;
        private readonly IRouletteService _rouletteService;

        public BotCommandsService(IPollService pollService, IConfiguration configuration, ILogger<BotCommandsService> logger, IPromptService promptService, IRouletteService rouletteService)
        {
            _pollService = pollService;
            _configuration = configuration;
            _logger = logger;
            _promptService = promptService;
            _rouletteService = rouletteService;
        }

        public async Task ProcessCommands(IMessage message)
        {
            var included = _configuration.GetSection("Bot:Included").Get<ulong[]>() ?? Enumerable.Empty<ulong>();
            var excluded = _configuration.GetSection("Bot:Excluded").Get<ulong[]>() ?? Enumerable.Empty<ulong>();
            
            if ((included.Any() && included.All(i => i != message.Channel.Id))
                || excluded.Any(e => e == message.Channel.Id))
            {
                return;
            }
            
            if (message.Content == "!movie-bot" || message.Content == "!movie-bot help")
            {
                await Help(message);
                return;
            }

            if (message.Content == "!movie-bot ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
                return;
            }

            if (message.Content.StartsWith(EchoText, StringComparison.CurrentCulture))
            {
                await message.Channel.SendMessageAsync(message.Content.Remove(0, EchoText.Length));
                return;
            }

            if (message.Content.StartsWith("!movie-bot ") && message.Content.Contains("fbi", StringComparison.CurrentCultureIgnoreCase))
            {
                await message.Channel.SendMessageAsync("Reported to FBI!");
                return;
            }

            if (message.Content == "!movie-bot popcorn")
            {
                await message.Channel.SendMessageAsync(":popcorn::popcorn::popcorn::popcorn::popcorn:");
                return;
            }

            if (message.Content.StartsWith("!movie-bot ban"))
            {
                await Ban(message);
                return;
            }

            if (message.Content.StartsWith(CommandText.Poll))
            {
                await Poll(message);
                return;
            }

            var cmdMatch = new Regex(@"^!movie-bot\s*(?:roulette|r)(.*)", RegexOptions.IgnoreCase).Match(message.Content);
            if (cmdMatch.Success)
            {
                await Roulette(message, cmdMatch);
                return;
            }
            
            if (message.Content.StartsWith("!movie-bot"))
            {
                var response = "I do not understand the command." + Environment.NewLine + ">>> " + message.Content;
                if (message.Source == MessageSource.User)
                {
                    response = message.Author.Mention + ", " + response;
                }
                await message.Channel.SendMessageAsync(response);
            }
        }

        private async Task Roulette(IMessage message, Match regMatch)
        {
            
            try
            {
                var cmdGroup = regMatch.Groups[1];
                var cmdValue = cmdGroup.Value.Trim();
                
                async Task InstructionMessage()
                {
                    var builder = new StringBuilder();
                    builder.AppendLine("**Roulette**");
                    builder.AppendLine();
                    builder.AppendLine("__*Spin:*__");
                    builder.AppendLine("`!movie-bot roulette spin`");
                    builder.AppendLine("`!movie-bot r spin`");
                    builder.AppendLine();
                    builder.AppendLine("__*List:*__");
                    builder.AppendLine("`!movie-bot roulette list`");
                    builder.AppendLine("`!movie-bot r list`");
                    builder.AppendLine();
                    builder.AppendLine("__*Add:*__");
                    builder.AppendLine("`!movie-bot roulette add title`");
                    builder.AppendLine("`!movie-bot r add title`");
                    builder.AppendLine();
                    builder.AppendLine("__*Remove:*__");
                    builder.AppendLine("`!movie-bot roulette remove title`");
                    builder.AppendLine("`!movie-bot r remove title`");
                    builder.AppendLine();
                    builder.AppendLine("__*Clear All:*__");
                    builder.AppendLine("`!movie-bot roulette clear`");
                    builder.AppendLine("`!movie-bot r clear`");
                    builder.AppendLine();
                    builder.AppendLine("**Limit of 20 selections.*");
                    await message.Channel.SendMessageAsync(builder.ToString());
                }

                if (string.IsNullOrWhiteSpace(cmdValue))
                {
                    await InstructionMessage();
                    return;
                }

                if (cmdValue.ToLower().Trim() == "list")
                {
                    var list = await _rouletteService.List();
                    var builder = new StringBuilder();
                    builder.AppendLine("**Roulette List**");
                    builder.AppendLine();
                    if (list.Any())
                    {
                        foreach (var title in list)
                        {
                            builder.AppendLine($"*{title}*");
                        }
                    }
                    else
                    {
                        builder.AppendLine("`No titles have been added.`");
                    }
                    await message.Channel.SendMessageAsync(builder.ToString());
                    return;
                }
                
                if (cmdValue.ToLower().Trim() == "spin")
                {
                    var winner = await _rouletteService.Spin();
                    var builder = new StringBuilder();
                    builder.AppendLine("**Roulette Winner**");
                    builder.AppendLine();
                    builder.AppendLine($"***{winner}***");
                    await message.Channel.SendMessageAsync(builder.ToString());
                    return;
                }
                
                if (cmdValue.ToLower().Trim() == "clear")
                {
                    await _rouletteService.Clear();
                    var builder = new StringBuilder();
                    builder.AppendLine("*Roulette Cleared*");
                    await message.Channel.SendMessageAsync(builder.ToString());
                    return;
                }
                
                var addReg = new Regex(@"^add\s+(.*)", RegexOptions.IgnoreCase);
                var addMatch = addReg.Match(cmdValue);
                if (addMatch.Success)
                {
                    var title = addMatch.Groups[1].Value.Trim();
                    await _rouletteService.AddTitle(title);
                    var builder = new StringBuilder();
                    builder.AppendLine("*Roulette Added*");
                    builder.AppendLine();
                    builder.AppendLine($"`{title}`");
                    await message.Channel.SendMessageAsync(builder.ToString());
                    return;
                }
                
                var removeReg = new Regex(@"^remove\s+(.*)", RegexOptions.IgnoreCase);
                var removeMatch = removeReg.Match(cmdValue);
                if (removeMatch.Success)
                {
                    var title = removeMatch.Groups[1].Value.Trim();
                    await _rouletteService.Remove(title);
                    var builder = new StringBuilder();
                    builder.AppendLine("*Roulette Removed*");
                    builder.AppendLine();
                    builder.AppendLine($"`{title}`");
                    await message.Channel.SendMessageAsync(builder.ToString());
                    return;
                }

                await InstructionMessage();
            }
            catch (RouletteException ex)
            {
                var builder = new StringBuilder();
                builder.AppendLine("**Roulette Error**");
                builder.AppendLine();
                builder.AppendLine($"`{ex.Message}`");
                await message.Channel.SendMessageAsync(builder.ToString());
            }
        }

        private async Task Poll(IMessage message)
        {
            var poll = MapToPoll(message);
            var builder = new StringBuilder();
            var prompt = default(Prompt);
            
            var reactions = new List<IEmote>();
            if (poll != null)
            {
                builder.AppendLine("**POLL Options**");
                builder.AppendLine();
                builder.AppendLine("Poll closes:");
                builder.AppendLine($"{Indicators.A} - 5 mins");
                builder.AppendLine();
                builder.AppendLine($"{Indicators.B} - 15 mins");
                builder.AppendLine();
                builder.AppendLine($"{Indicators.C} - 30 mins");
                builder.AppendLine();
                builder.AppendLine($"{Indicators.D} - 1 hr");
                builder.AppendLine();
                builder.AppendLine($"{Indicators.E} - I will close manually.");
                
                reactions.AddRange(new []
                {
                    new Emoji(Indicators.A),
                    new Emoji(Indicators.B),
                    new Emoji(Indicators.C),
                    new Emoji(Indicators.D),
                    new Emoji(Indicators.E), 
                });
                builder.AppendLine();
                builder.AppendLine("Please choose:");
            }
            else
            {
                builder.AppendLine("**POLL Instructions**");
                builder.AppendLine();
                builder.AppendLine("__*New Poll:*__");
                builder.AppendLine("`!movie-bot poll Question | Answer 1 | Answer 2`");
                builder.AppendLine("**Limit of 10 answers.*");


                prompt = await _promptService.CreatePollPrompt(message.Author.Id);

                if (prompt != default)
                {
                    builder.AppendLine();
                    builder.AppendLine("__*Close Polls:*__");
                    foreach (var reactionMap in prompt.Data.Poll.ClosePollReactionMap)
                    {
                        var emoji = new Emoji(reactionMap.Emoji);
                        reactions.Add(emoji);
                        builder.AppendLine($"{emoji} - {reactionMap.Question}");
                        builder.AppendLine();
                    }
                    builder.AppendLine("*Select Below:*");
                }

                builder.AppendLine();
            }

            var res = await message.Channel.SendMessageAsync(builder.ToString());

            if (poll != null)
            {
                poll.ConfigMessageId = res.Id;
                await _pollService.Add(poll);
            }

            if (prompt != default)
            {
                prompt.MessageId = res.Id;
                await _promptService.Add(prompt);
            }

            var sentMessage =  await message.Channel.GetMessageAsync(res.Id) as IUserMessage;
            if (reactions.Any())
            {
                await sentMessage.AddReactionsAsync(reactions.ToArray());
            }
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
