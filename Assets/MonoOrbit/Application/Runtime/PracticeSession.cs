using System;
using MonoOrbit.Gameplay;

namespace MonoOrbit.Application
{
    /// <summary>
    /// Local training session only. Competitive modes will use a separately
    /// authoritative, revisioned application service backed by the network host.
    /// </summary>
    public sealed class PracticeSession
    {
        private readonly MatchEngine _engine;

        public BoardState Board { get; private set; }
        public int Score { get; private set; }
        public int Revision { get; private set; }

        public PracticeSession(uint seed)
        {
            _engine = new MatchEngine(new DeterministicRandom(seed));
            StartNewGame();
        }

        public void StartNewGame()
        {
            Board = _engine.CreateNewBoard();
            Score = 0;
            Revision = 0;
        }

        public MoveResult TrySwap(CellPosition from, CellPosition to)
        {
            var result = _engine.TrySwap(Board, from, to);
            if (!result.Accepted) return result;

            Board = result.FinalBoard;
            Score = checked(Score + result.ScoreDelta);
            Revision++;
            return result;
        }
    }
}
