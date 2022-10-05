namespace simple_Discord_bot;

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.EventArgs;

/** <summary>
  This class is a module that contains all of the commands for the main game.
</summary> */
public class MainGameModule : BaseCommandModule {
  // A class to keep track of the games
  public class GameDictionary {
    private Dictionary<ulong, MainGameAdapter> userGames = new Dictionary<ulong, MainGameAdapter>();

    public MainGameAdapter getGame(DiscordMember member) {
      userGames.TryAdd(member.Id, new MainGameAdapter(member.DisplayName));
      return userGames[member.Id];
    }

    public MainGameAdapter? this[ulong hostId] {
      get {
        if (userGames.ContainsKey(hostId))
          return userGames[hostId];
        else
          return null;
      }
    }
  }

  // This property is set via dependency injection in MainAsync()
  public GameDictionary? GameDict { private get; set; }

  /** <summary>
    Creates a new game for the user that used the command.
  </summary> */
  [Command("new-game")]
  public async Task NewGame(CommandContext ctx) {
    // guard
    if (ctx.Member == null) {
      await ctx.RespondAsync("Incorrect context; make a new game in a guild.");
      return;
    }

    GameDict!.getGame(ctx.Member);
    await ctx.RespondAsync($"Made new game for __{ctx.Member.DisplayName}__.");
  }

  /** <summary>
    Opens a window to see the host's game.
  </summary> */
  [Command("show-game")]
  public async Task ShowGame(CommandContext ctx, DiscordMember host) {
    // guard
    if (ctx.Member == null) {
      await ctx.RespondAsync("Incorrect context; show the game in a guild.");
      return;
    }

    MainGameAdapter? game = GameDict![host.Id];
    if (game == null) {
      await ctx.RespondAsync("Game nonexistent.");
      return;
    }

    string boardStr = $"```{game.printBoard()}```";
    string text = $"__{game.ownerName}__\n{boardStr}";
    var message = await new DiscordMessageBuilder()
      .WithContent(text)
      .AddComponents(
        new InteractivityIdentifier(host.Id, host.Id, InteractivityIdentifier.Type.RefreshButton)
          .createButton()
      )
      .SendAsync(ctx.Channel);
  }


  /** <summary>
    The user joins the host's game and is sent a private message with a window to view the game.
  </summary> */
  [Command("join-game")]
  public async Task JoinGame(CommandContext ctx, DiscordMember host) {
    // guard
    if (ctx.Member == null) {
      await ctx.RespondAsync("Incorrect contex; join the game in a guild.");
      return;
    }

    MainGameAdapter? game = GameDict![host.Id];
    if (game == null) {
      await ctx.RespondAsync("Game nonexistent.");
      return;
    }

    bool successJoinGame = false;
    lock (game) {
      successJoinGame = game.registerUser(ctx.Member);
    }

    // guard
    if (successJoinGame == false) {
      await ctx.RespondAsync("> failed to join game");
      return;
    }

    var msg = ctx.Member.SendMessageAsync(getPlayerWindow(host, ctx.Member));
  }

  // helper method for printing a game's board into a message\window
  private DiscordMessageBuilder getPlayerWindow(DiscordMember host, DiscordMember player) {
    MainGameAdapter game = GameDict![host.Id]!;
    string windowStr = $"```\n{game.printBoard()}\n```";
    return new DiscordMessageBuilder()
      .WithContent(windowStr)
      .AddComponents(createButtons(host.Id, player.Id));
  }

  /** <summary>
    Handles interaction events such as pressing a button.
  </summary> */
  public static async Task handleInteraction(
    ComponentInteractionCreateEventArgs args, GameDictionary games
  ) {
    // parse the id into an object for easier use
    InteractivityIdentifier id; try {
      id = new InteractivityIdentifier(args.Id);
    } catch (System.FormatException) {
      await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
      await args.Message.DeleteAsync();
      return;
    } catch (System.OverflowException) {
      await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
      await args.Message.DeleteAsync();
      return;
    }

    // get game state associated with identifier
    MainGameAdapter? game = games[id.hostId];
    if (game == null) {
      await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
      await args.Message.DeleteAsync();
    } else if (id.type != InteractivityIdentifier.Type.RefreshButton) /* "if changing the game state by moving a character" */ {
      // lock game state to prevent race conditions
      lock (game) {
        game.interact(id.playerId, id.Direction);
      }

      await args.Interaction.CreateResponseAsync(
        InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
          .WithContent($"```\n{game.printBoard()}\n```")
          .AddComponents(createButtons(id.hostId, id.playerId))
      );
    } else /* "otherwise we are reading from the game state by refreshing a window" */ {
      await args.Interaction.CreateResponseAsync(
        InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
          // reading from the game doesn't require a lock
          .WithContent($"__{game.ownerName}__\n```{game.printBoard()}```")
          .AddComponents(
            new InteractivityIdentifier(id.hostId, id.hostId, InteractivityIdentifier.Type.RefreshButton)
              .createButton()
          )
      );
    }
  }

  // helper method for building responses
  private static DiscordComponent[] createButtons(ulong hostId, ulong playerId) {
    var buttons = new DiscordComponent[4];
    buttons[0] = new InteractivityIdentifier(hostId, playerId, InteractivityIdentifier.Type.NorthButton)
      .createButton();
    buttons[1] = new InteractivityIdentifier(hostId, playerId, InteractivityIdentifier.Type.SouthButton)
      .createButton();
    buttons[2] = new InteractivityIdentifier(hostId, playerId, InteractivityIdentifier.Type.EastButton)
      .createButton();
    buttons[3] = new InteractivityIdentifier(hostId, playerId, InteractivityIdentifier.Type.WestButton)
      .createButton();
    return buttons;
  }

}
