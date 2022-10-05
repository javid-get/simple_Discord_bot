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

  public GameDictionary? GameDict { private get; set; }

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

  private DiscordMessageBuilder getPlayerWindow(DiscordMember host, DiscordMember player) {
    MainGameAdapter game = GameDict![host.Id]!;
    string windowStr = $"```\n{game.printBoard()}\n```";
    return new DiscordMessageBuilder()
      .WithContent(windowStr)
      .AddComponents(createButtons(host.Id, player.Id));
  }

  // todo: make sure users with leftover buttons don't mess up the game
  public static async Task handleInteraction(
    ComponentInteractionCreateEventArgs args, GameDictionary games
  ) {
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

    MainGameAdapter? game = games[id.hostId];
    if (game == null) {
      await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
      await args.Message.DeleteAsync();
    } else if (id.type != InteractivityIdentifier.Type.RefreshButton) {
      lock (game) {
        game.interact(id.playerId, id.Direction);
      }

      await args.Interaction.CreateResponseAsync(
        InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
          .WithContent($"```\n{game.printBoard()}\n```")
          .AddComponents(createButtons(id.hostId, id.playerId))
      );
    } else {
      await args.Interaction.CreateResponseAsync(
        InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
          .WithContent($"__{game.ownerName}__\n```{game.printBoard()}```")
          .AddComponents(
            new InteractivityIdentifier(id.hostId, id.hostId, InteractivityIdentifier.Type.RefreshButton)
              .createButton()
          )
      );
    }
  }

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
