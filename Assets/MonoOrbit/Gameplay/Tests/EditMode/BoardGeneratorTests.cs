using NUnit.Framework;

namespace MonoOrbit.Gameplay.Tests
{
    public sealed class BoardGeneratorTests
    {
        [Test]
        public void EverySeedProducesStablePlayableNineByNineBoard()
        {
            for (uint seed = 1; seed <= 100; seed++)
            {
                var board = new BoardGenerator(new DeterministicRandom(seed)).Create();
                Assert.That(board.Width, Is.EqualTo(9));
                Assert.That(board.Height, Is.EqualTo(9));
                Assert.That(MatchFinder.FindMatches(board), Is.Empty, "Seed " + seed);
                Assert.That(MoveValidator.HasAvailableMove(board), Is.True, "Seed " + seed);
            }
        }

        [Test]
        public void SameSeedProducesIdenticalBoard()
        {
            var first = new BoardGenerator(new DeterministicRandom(123u)).Create();
            var second = new BoardGenerator(new DeterministicRandom(123u)).Create();

            CollectionAssert.AreEqual(first.ToArray(), second.ToArray());
        }

        [Test]
        public void BoardDefensivelyCopiesInputAndOutput()
        {
            var elements = new[] { WeatherElement.Sun, WeatherElement.Rain,
                WeatherElement.Snow, WeatherElement.Wind };
            var board = new BoardState(2, 2, elements);
            elements[0] = WeatherElement.Thunder;
            var output = board.ToArray();
            output[1] = WeatherElement.Thunder;

            Assert.That(board.Get(new CellPosition(0, 0)), Is.EqualTo(WeatherElement.Sun));
            Assert.That(board.Get(new CellPosition(1, 0)), Is.EqualTo(WeatherElement.Rain));
        }
    }
}
