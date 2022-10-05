namespace Games.Main;

/** <summary>
  A space on the grid that a piece can occupy.
</summary> */
public class Tile {
  private readonly Board owner;
  public readonly int row;
  public readonly int col;
  private Piece? occupant;
  public Tile(Board owner, int row, int col) {
    this.owner = owner;
    this.row = row;
    this.col = col;
    this.occupant = null;
  }

  /// <summary>
  /// setting only works if value's home is this object
  /// </summary>
  /// <value>occupant</value>
  public Piece? Occupant {
    get { return occupant; }
    set {
      if (value != null && value.home == this)
        occupant = value;
    }
  }

  public Tile adjacent(Board.Direction direction) {
    int adjacentRow = row;
    int adjacentColumn = col;

    switch (direction) {
      case Board.Direction.North:
        --adjacentRow; break;
      case Board.Direction.South:
        ++adjacentRow; break;
      case Board.Direction.East:
        ++adjacentColumn; break;
      case Board.Direction.West:
        --adjacentColumn; break;
    }

    return owner[adjacentRow, adjacentColumn];
  }

  public void swap(Tile tile) {
    Piece? pivot = this.occupant;
    this.occupant = tile.occupant;
    tile.occupant = pivot;

    if (this.occupant != null)
      this.occupant.home = this;
    if (tile.occupant != null)
      tile.occupant.home = tile;
  }
}
