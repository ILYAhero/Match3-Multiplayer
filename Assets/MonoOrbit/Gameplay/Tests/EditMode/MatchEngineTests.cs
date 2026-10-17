using NUnit.Framework;

namespace MonoOrbit.Gameplay.Tests
{
    public sealed class MatchEngineTests
    {
        [Test]
        public void NonAdjacentSwapIsRejectedWithoutChangingBoard()
        {
            var engine = new MatchEngine(new DeterministicRandom(99u));
            var initial = engine.CreateNewBoard();
            var result = engine.TrySwap(initial, new CellPosition(0, 0), new CellPosition(8, 8));

            Assert.That(result.Accepted, Is.False);
            Assert.That(result.ScoreDelta, Is.Zero);
            Assert.That(result.Cascades, Is.Empty);
            Assert.That(result.FinalBoard, Is.SameAs(initial));
        }

        [Test]
        public void AcceptedMoveResolvesIntoPlayableBoardWithPositiveScore()
        {
            var engine = new MatchEngine(new DeterministicRandom(1u));
            var initial = engine.CreateNewBoard();
            FindLegalMove(initial, out var a, out var b);

            var result = engine.TrySwap(initial, a, b);

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.ScoreDelta, Is.GreaterThanOrEqualTo(30));
            Assert.That(result.Cascades.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(MatchFinder.FindMatches(result.FinalBoard), Is.Empty);
            Assert.That(MoveValidator.HasAvailableMove(result.FinalBoard), Is.True);
            Assert.That(MatchFinder.FindMatches(initial), Is.Empty, "Input board must remain immutable.");

            var sum = 0;
            foreach (var cascade in result.Cascades)
            {
                Assert.That(cascade.ClearedCells.Count, Is.GreaterThanOrEqualTo(3));
                sum += cascade.ScoreDelta;
            }
            Assert.That(result.ScoreDelta, Is.EqualTo(sum));
        }

        [Test]
        public void SameSeedAndMoveGiveIdenticalResolution()
        {
            var first = new MatchEngine(new DeterministicRandom(2026u));
            var second = new MatchEngine(new DeterministicRandom(2026u));
            var firstBoard = first.CreateNewBoard();
            var secondBoard = second.CreateNewBoard();
            FindLegalMove(firstBoard, out var a, out var b);

            var aResult = first.TrySwap(firstBoard, a, b);
            var bResult = second.TrySwap(secondBoard, a, b);

            Assert.That(aResult.Accepted, Is.True);
            Assert.That(aResult.ScoreDelta, Is.EqualTo(bResult.ScoreDelta));
            Assert.That(aResult.Cascades.Count, Is.EqualTo(bResult.Cascades.Count));
            CollectionAssert.AreEqual(aResult.FinalBoard.ToArray(), bResult.FinalBoard.ToArray());
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

            Assert.Fail("Generated board has no legal moves.");
            a = default;
            b = default;
        }
    }
}
