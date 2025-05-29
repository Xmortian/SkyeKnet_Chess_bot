namespace ChessLogic
{
    // Class representing a position on the chessboard (row and column)
    public class Position
    {
        // Properties to store the row and column of the position
        public int Row { get; }
        public int Column { get; }

        // Constructor to initialize the position with a specific row and column
        public Position(int row, int column)
        {
            Row = row; // Set the row of the position
            Column = column; // Set the column of the position
        }

        // Method to determine the color of the square (White or Black)
        public Player SquareColor()
        {
            // If the sum of the row and column is even, the square is White, else Black
            if ((Row + Column) % 2 == 0)
            {
                return Player.White; // White square
            }
            return Player.Black; // Black square
        }

        // Override the Equals method to compare two Position objects
        public override bool Equals(object obj)
        {
            return obj is Position position &&
                   Row == position.Row &&  // Compare the rows
                   Column == position.Column; // Compare the columns
        }

        // Override the GetHashCode method to generate a unique hash code for the position
        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column); // Combine the row and column to create the hash code
        }

        // Equality operator to compare two positions
        public static bool operator ==(Position left, Position right)
        {
            return EqualityComparer<Position>.Default.Equals(left, right); // Use default equality comparer
        }

        // Inequality operator to compare two positions
        public static bool operator !=(Position left, Position right)
        {
            return !(left == right); // Return the opposite of the equality comparison
        }

        // Operator to add a Direction to a Position (moving the position in the specified direction)
        public static Position operator +(Position pos, Direction dir)
        {
            // Add the direction deltas to the current position to get the new position
            return new Position(pos.Row + dir.RowDelta, pos.Column + dir.ColumnDelta);
        }
        public string ToUCI()
        {
            char file = (char)('a' + Column);
            char rank = (char)('1' + (7 - Row));
            return $"{file}{rank}";
        }

    }
}
