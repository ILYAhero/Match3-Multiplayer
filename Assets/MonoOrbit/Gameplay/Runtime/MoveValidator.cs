using System;

namespace MonoOrbit.Gameplay
{
    public static class MoveValidator
    {
        public static bool IsLegalSwap(BoardState board, CellPosition a, CellPosition b)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!board.Contains(a) || !board.Contains(b)) return false;
            if (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y) != 1) return false;
            if (board.Get(a) == board.Get(b)) return false;
            if (board.Get(a) == WeatherElement.Empty || board.Get(b) == WeatherElement.Empty) return false;

            var swapped = board.Swapped(a, b);
            return MatchFinder.HasMatchAt(swapped, a) || MatchFinder.HasMatchAt(swapped, b);
        }

        public static bool HasAvailableMove(BoardState board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            for (var y = 0; y < board.Height; y++)
            {
                for (var x = 0; x < board.Width; x++)
                {
                    var origin = new CellPosition(x, y);
                    if (x + 1 < board.Width &&
                        IsLegalSwap(board, origin, new CellPosition(x + 1, y))) return true;
                    if (y + 1 < board.Height &&
                        IsLegalSwap(board, origin, new CellPosition(x, y + 1))) return true;
                }
            }

            return false;
        }
    }
}
