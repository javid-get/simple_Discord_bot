namespace simple_Discord_bot;
using System.Collections.Generic;
using DSharpPlus.Entities;

/** <summary>
  Holds game state and has keeps track of the host of the game.
  Basically has Discord-related functionality in addition to keeping track of game state.
</summary> */
public class MainGameAdapter {
  public string ownerName;
  private Games.Main.Game game;
  private Dictionary<ulong, int> playerRegistry;
  public MainGameAdapter(string ownerName) {
    this.ownerName = ownerName;
    this.game = new Games.Main.Game();
    this.playerRegistry = new Dictionary<ulong, int>();
  }

  public bool interact(ulong discordUserId, Games.Main.Board.Direction direction) {
    int playerIndex;
    bool success = playerRegistry.TryGetValue(discordUserId, out playerIndex);

    if (success)
      game.interact(playerIndex, direction);

    return success;
  }

  public bool registerUser(DiscordMember member) {
    if (playerRegistry.ContainsKey(member.Id))
      return false;

    int playerIdentifier = game.addPlayer(member.DisplayName, 5, 5);

    if (playerIdentifier >= 0)
      playerRegistry.Add(member.Id, playerIdentifier);

    return playerIdentifier >= 0;
  }

  public string printBoard() { return game.printBoard(); }
}
