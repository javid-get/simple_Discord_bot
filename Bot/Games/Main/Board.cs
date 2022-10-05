namespace Games.Main;

public class Board {
  private Tile[,] table;
  public Board(int rowRank, int columnRank) {
    this.table = new Tile[rowRank, columnRank];

    for (int row = 0; row < rowRank; ++row)
      for (int col = 0; col < columnRank; ++col)
        table[row, col] = new Tile(this, row, col);
  }

  public int RowRank {
    get { return table.GetLength(0); }
  }

  public int ColRank {
    get { return table.GetLength(1); }
  }

  public Tile this[int row, int column] {
    get {
      row = wrap(row, table.GetLength(0));
      column = wrap(column, table.GetLength(1));
      return table[row, column];
    }
  }

  public static int wrap(int index, int length) {
    index %= length;
    if (index < 0)
      index += length;
    return index;
  }

  private void forEachTile(System.Action<Tile, int, int> withTileRowCol) {
    for (int row = 0; row < RowRank; ++row)
      for (int col = 0; col < ColRank; ++col)
        withTileRowCol(table[row, col], row, col);
  }

  public override string ToString() {
    var buffer = new System.Text.StringBuilder();
    for (int row = 0; row < RowRank; ++row) {
      for (int col = 0; col < ColRank; ++col) {
        buffer
          .Append(' ')
          .Append(Piece.symbolize(table[row, col].Occupant))
          .Append(' ');
      }
      buffer.Append('\n');
    }

    return buffer.ToString();
  }

  public enum Direction : byte { North, South, East, West, Null }
}
