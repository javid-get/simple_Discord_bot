// This namespace is meant to house multiple games. (Only one game is implemented).
namespace Games {
  // (only the main game is implemented)
  public enum GameType : byte {
    // This is a bare-bones "game" where players can move a character around a 2D grid.
    Main,

    // I didn't implement Chess.
    Chess

    // (other games would be listed here had there been more)
  }
}