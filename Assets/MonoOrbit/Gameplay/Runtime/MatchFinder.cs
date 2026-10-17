using System;
using System.Collections.Generic;

namespace MonoOrbit.Gameplay
{
    public static class MatchFinder
    {
        /// <summary>Returns unique cleared positions in deterministic row-major order.</summary>
        public static IReadOnlyList<CellPosition> FindMatches(BoardState board)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));

            var hits = new bool[checked(board.Width * board.Height)];

            for (var y = 0; y < board.Height; y++)
            {
                var x = 0;
                while (x < board.Width)
                {
                    var kind = board.Get(new CellPosition(x, y));
                    var end = x + 1;
                    while (end < board.Width && board.Get(new CellPosition(end, y)) == kind) end++;
                    if (kind != WeatherElement.Empty && end - x >= 3)
                        for (var i = x; i < end; i++) hits[y * board.Width + i] = true;
                    x = end;
                }
            }

            for (var x = 0; x < board.Width; x++)
            {
                var y = 0;
                while (y < board.Height)
                {
                    var kind = board.Get(new CellPosition(x, y));
                    var end = y + 1;
                    while (end < board.Height && board.Get(new CellPosition(x, end)) == kind) end++;
                    if (kind != WeatherElement.Empty && end - y >= 3)
                        for (var i = y; i < end; i++) hits[i * board.Width + x] = true;
                    y = end;
                }
            }

            var result = new List<CellPosition>();
            for (var y = 0; y < board.Height; y++)
                for (var x = 0; x < board.Width; x++)
                    if (hits[y * board.Width + x])
                        result.Add(new CellPosition(x, y));

            return result.ToArray();
        }

        /// <summary>Checks whether a match crosses a particular cell.</summary>
        public static bool HasMatchAt(BoardState board, CellPosition position)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!board.Contains(position)) return false;

            var kind = board.Get(position);
            if (kind == WeatherElement.Empty) return false;

            var run = 1;
            for (var x = position.X - 1; x >= 0 && board.Get(new CellPosition(x, position.Y)) == kind; x--) run++;
            for (var x = position.X + 1; x < board.Width && board.Get(new CellPosition(x, position.Y)) == kind; x++) run++;
            if (run >= 3) return true;

            run = 1;
            for (var y = position.Y - 1; y >= 0 && board.Get(new CellPosition(position.X, y)) == kind; y--) run++;
            for (var y = position.Y + 1; y < board.Height && board.Get(new CellPosition(position.X, y)) == kind; y++) run++;
            return run >= 3;
        }
    }
}
