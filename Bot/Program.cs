using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

/*

  This program is a Discord bot.

  The Discord bot, when running, allows users to join a game together.

  The game is very simple; it allows players to move a character around a 2D grid.

*/

namespace simple_Discord_bot {

  internal class Program {

    static void Main(string[] args) {
      MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync() {
      // create the discord client
      var discord = new DiscordClient(new DiscordConfiguration() {
        Token = System.IO.File.ReadAllText(@".secret\token.txt"),
        TokenType = TokenType.Bot,
        Intents = DiscordIntents.All,
        MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
      });

      // create dictionary to store games
      var gameDict = new MainGameModule.GameDictionary();

      // create services to inject into module
      var services = new ServiceCollection()
        .AddSingleton<Random>()
        .AddSingleton(gameDict)
        .BuildServiceProvider();

      // enable commands
      var commands = discord.UseCommandsNext(new CommandsNextConfiguration() {
        StringPrefixes = new[] { "$" },
        Services = services // dependency injection
      });

      /*
        register commands:
          utility module is for general-purpose commands
          main game module is for the main game commands
      */
      commands.RegisterCommands<UtilityModule>();
      commands.RegisterCommands<MainGameModule>();

      // enable interactivity
      // this includes stuff like buttons and drop-down menus
      discord.UseInteractivity(new InteractivityConfiguration() { 
        PollBehaviour = PollBehaviour.KeepEmojis,
        Timeout = TimeSpan.FromSeconds(30)
      });

      // handle the interaction-created event, like when a user presses a button
      discord.ComponentInteractionCreated += async (s, e) => {
        if (e.Message.Author.Id != s.CurrentUser.Id)
          return;

        // get the parts of the identifier
        string[] args = e.Id.Split(InteractivityIdentifier.Delimiter);

        Games.GameType gameType;
        try {
          gameType = (Games.GameType) byte.Parse(args[0]);
        } catch (FormatException) {
          // if the game type isn't parsed, delete the message the interaction came from
          await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
          await e.Message.DeleteAsync();
          return;
        }

        switch (gameType) {
          case Games.GameType.Main:
            await MainGameModule.handleInteraction(e, gameDict);
            break;
          default:
            await e.Interaction.CreateResponseAsync(
              InteractionResponseType.UpdateMessage,
              new DiscordInteractionResponseBuilder()
                .WithContent("Cleaned.")
            );
            break;
        }
      };
      
      await discord.ConnectAsync();
      await Task.Delay(-1); // -1 waits indefinitely
    }

  }

}