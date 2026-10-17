using NUnit.Framework;

namespace MonoOrbit.Gameplay.Tests
{
    public sealed class MatchFinderTests
    {
        [Test]
        public void OverlappingCrossCountsCenterOnce()
        {
            // Rows are supplied bottom-first: two intersecting lines of Sun.
            var board = new BoardState(3, 3, new[]
            {
                WeatherElement.Rain, WeatherElement.Sun, WeatherElement.Rain,
                WeatherElement.Sun, WeatherElement.Sun, WeatherElement.Sun,
                WeatherElement.Rain, WeatherElement.Sun, WeatherElement.Rain
            });

            var cells = MatchFinder.FindMatches(board);

            Assert.That(cells.Count, Is.EqualTo(5));
            CollectionAssert.Contains(cells, new CellPosition(1, 1));
            Assert.That(MatchFinder.HasMatchAt(board, new CellPosition(1, 1)), Is.True);
            Assert.That(MatchFinder.HasMatchAt(board, new CellPosition(0, 0)), Is.False);
        }

        [Test]
        public void EmptyCellsNeverMatch()
        {
            var board = new BoardState(3, 3, new WeatherElement[9]);
            Assert.That(MatchFinder.FindMatches(board), Is.Empty);
            Assert.That(MatchFinder.HasMatchAt(board, new CellPosition(1, 1)), Is.False);
        }
    }
}
