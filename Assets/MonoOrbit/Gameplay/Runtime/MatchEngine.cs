using System;
using System.Collections.Generic;

namespace MonoOrbit.Gameplay
{
    /// <summary>
    /// Pure, deterministic match resolution. Must run only on the authoritative host
    /// when networking is introduced. No Unity objects, timing or visual effects here.
    /// </summary>
    public sealed class MatchEngine
    {
        private readonly IRandomSource _random;
        private readonly BoardGenerator _generator;

        public MatchEngine(IRandomSource random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _generator = new BoardGenerator(random);
        }

        public BoardState CreateNewBoard() => _generator.Create();

        public MoveResult TrySwap(BoardState board, CellPosition from, CellPosition to)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (!MoveValidator.IsLegalSwap(board, from, to))
                return MoveResult.Rejected(board);

            var current = board.Swapped(from, to);
            var cascades = new List<CascadeStep>();
            var totalScore = 0;

            for (var index = 1; index <= MatchRules.MaxCascadeSteps; index++)
            {
                var matches = MatchFinder.FindMatches(current);
                if (matches.Count == 0)
                {
                    var regenerated = false;
                    if (!MoveValidator.HasAvailableMove(current))
                    {
                        // This regenerates a playable board; it is not a piece-preserving shuffle.
                        current = _generator.Create(current.Width, current.Height);
                        regenerated = true;
                    }

                    return MoveResult.Successful(current, cascades, totalScore, regenerated);
                }

                var score = checked(matches.Count * MatchRules.PointsPerClearedCell * index);
                var next = ClearAndRefill(current, matches);
                cascades.Add(new CascadeStep(index, current, next, matches, score));
                totalScore = checked(totalScore + score);
                current = next;
            }

            throw new InvalidOperationException("Cascade limit exceeded. Match was not committed.");
        }

        private BoardState ClearAndRefill(BoardState board, IReadOnlyList<CellPosition> removed)
        {
            var removedMask = new bool[checked(board.Width * board.Height)];
            for (var i = 0; i < removed.Count; i++)
                removedMask[removed[i].Y * board.Width + removed[i].X] = true;

            var cells = new WeatherElement[checked(board.Width * board.Height)];
            for (var x = 0; x < board.Width; x++)
            {
                var writeY = 0;
                for (var readY = 0; readY < board.Height; readY++)
                {
                    if (removedMask[readY * board.Width + x]) continue;
                    cells[writeY * board.Width + x] = board.Get(new CellPosition(x, readY));
                    writeY++;
                }

                for (; writeY < board.Height; writeY++)
                    cells[writeY * board.Width + x] = (WeatherElement)(_random.Next(6) + 1);
            }

            return new BoardState(board.Width, board.Height, cells);
        }
    }
}
