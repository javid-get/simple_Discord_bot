namespace Games.Main;

/** <summary>
  Represents an object that lives on the grid.
</summary> */
public abstract class Piece {
  public Tile home;
  public Piece(Tile home) { this.home = home; }

  public abstract char Symbol { get; }

  public abstract void interact(Tile tile);

  public static char symbolize(Piece? piece) {
    if (piece == null)
      return '.';
    else
      return piece.Symbol;
  }

  public class PlayerCharacter : Piece {
    public readonly string name;
    public PlayerCharacter(string name, Tile home) : base(home) {
      this.name = name;
    }

    public override char Symbol { get {
      return '&';
    } }

    public override void interact(Tile tile) {
      Piece? piece = tile.Occupant;

      if (piece == null)
        home.swap(tile);
    }
  }

}
