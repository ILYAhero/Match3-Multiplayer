using NUnit.Framework;
using MonoOrbit.Gameplay;

namespace MonoOrbit.Application.Tests
{
    public sealed class PracticeSessionTests
    {
        [Test]
        public void InvalidSwapDoesNotChangeRevisionScoreOrBoard()
        {
            var session = new PracticeSession(13u);
            var original = session.Board;

            var result = session.TrySwap(new CellPosition(0, 0), new CellPosition(8, 8));

            Assert.That(result.Accepted, Is.False);
            Assert.That(session.Score, Is.Zero);
            Assert.That(session.Revision, Is.Zero);
            Assert.That(session.Board, Is.SameAs(original));
        }

        [Test]
        public void LegalSwapCommitsExactlyOnce()
        {
            var session = new PracticeSession(42u);
            FindLegalMove(session.Board, out var a, out var b);
            var result = session.TrySwap(a, b);

            Assert.That(result.Accepted, Is.True);
            Assert.That(session.Board, Is.SameAs(result.FinalBoard));
            Assert.That(session.Score, Is.EqualTo(result.ScoreDelta));
            Assert.That(session.Revision, Is.EqualTo(1));
        }

        [Test]
        public void StartingNewGameResetsScoreAndRevision()
        {
            var session = new PracticeSession(7u);
            FindLegalMove(session.Board, out var a, out var b);
            Assert.That(session.TrySwap(a, b).Accepted, Is.True);

            session.StartNewGame();

            Assert.That(session.Score, Is.Zero);
            Assert.That(session.Revision, Is.Zero);
            Assert.That(session.Board.Width, Is.EqualTo(9));
            Assert.That(MatchFinder.FindMatches(session.Board), Is.Empty);
            Assert.That(MoveValidator.HasAvailableMove(session.Board), Is.True);
        }

        private static void FindLegalMove(BoardState board, out CellPosition a, out CellPosition b)
        {
            for (var y = 0; y < board.Height; y++)
                for (var x = 0; x < board.Width; x++)
                {
                    a = new CellPosition(x, y);
                    b = new CellPosition(x + 1, y);
                    if (MoveValidator.IsLegalSwap(board, a, b)) return;
                    b = new CellPosition(x, y + 1);
                    if (MoveValidator.IsLegalSwap(board, a, b)) return;
                }

            Assert.Fail("The session started with an unplayable board.");
            a = default;
            b = default;
        }
    }
}
