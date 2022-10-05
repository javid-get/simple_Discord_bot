namespace Games.Main;
using System.Collections.Generic;

/** <summary>
  This is less of a game and more of a proof of concept.
  Players of this game can move a character around in a two-dimensional grid.
</summary> */
public class Game {
  private List<Piece.PlayerCharacter> playerList;
  private Board board;
  public Game() {
    playerList = new List<Piece.PlayerCharacter>();
    board = new Board(10, 10);
  }

  public int PlayerCount {
    get { return playerList.Count; }
  }

  /// <summary>
  /// adds player to game
  /// </summary>
  /// <param name="name">name of player</param>
  /// <returns>identifier of new player, -1 on fail</returns>
  public int addPlayer(string name, int row, int column) {
    Tile home = board[row, column];
    if (home.Occupant != null)
      return -1;

    Piece.PlayerCharacter playerCharacter = new Piece.PlayerCharacter(name, home);
    home.Occupant = playerCharacter;
    playerList.Add(playerCharacter);
    return playerList.Count - 1;
  }

  public void interact(int player, Board.Direction direction) {
    Piece.PlayerCharacter piece = playerList[player];
    piece.interact(piece.home.adjacent(direction));
  }

  public string printBoard() { return board.ToString(); }
}
