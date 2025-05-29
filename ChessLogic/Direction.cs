namespace ChessLogic
{
    public class Direction
    {
        public readonly static Direction North = new Direction(-1, 0); // Move up one row
        public readonly static Direction South = new Direction(1, 0); // Move down one row
        public readonly static Direction East = new Direction(0, 1); // Move right one column
        public readonly static Direction West = new Direction(0, -1); // Move left one column
        public readonly static Direction NorthEast = North + East; // Combination of North and East (diagonal)
        public readonly static Direction NorthWest = North + West; // Combination of North and West (diagonal)
        public readonly static Direction SouthEast = South + East; // Combination of South and East (diagonal)
        public readonly static Direction SouthWest = South + West; // Combination of South and West (diagonal)

        // Properties to represent the change in row and column for the direction
        public int RowDelta { get; }
        public int ColumnDelta { get; }

        // Constructor to initialize a direction with specific row and column deltas
        public Direction(int rowDelta, int columnDelta)
        {
            RowDelta = rowDelta; // Change in rows
            ColumnDelta = columnDelta; // Change in columns
        }

=        public static Direction operator +(Direction dir1, Direction dir2)
        {
            return new Direction(dir1.RowDelta + dir2.RowDelta, dir1.ColumnDelta + dir2.ColumnDelta);
        }

        public static Direction operator *(int scalar, Direction dir)
        {
            return new Direction(scalar * dir.RowDelta, scalar * dir.ColumnDelta);
        }
    }
}
