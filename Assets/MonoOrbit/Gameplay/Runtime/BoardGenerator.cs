using System;

namespace MonoOrbit.Gameplay
{
    public sealed class BoardGenerator
    {
        private static readonly WeatherElement[] Spawnable =
        {
            WeatherElement.Sun, WeatherElement.Rain, WeatherElement.Snow,
            WeatherElement.Wind, WeatherElement.Thunder, WeatherElement.Cloud
        };

        private readonly IRandomSource _random;

        public BoardGenerator(IRandomSource random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>Creates a match-free board with at least one legal move.</summary>
        public BoardState Create(int width = MatchRules.Width, int height = MatchRules.Height)
        {
            if (width < 3 || height < 3)
                throw new ArgumentOutOfRangeException(nameof(width), "Board dimensions must be at least 3.");

            for (var attempt = 0; attempt < 128; attempt++)
            {
                var cells = new WeatherElement[checked(width * height)];
                for (var y = 0; y < height; y++)
                    for (var x = 0; x < width; x++)
                        cells[y * width + x] = PickWithoutTriple(cells, width, x, y);

                var board = new BoardState(width, height, cells);
                if (MoveValidator.HasAvailableMove(board)) return board;
            }

            throw new InvalidOperationException("Could not generate a playable board.");
        }

        private WeatherElement PickWithoutTriple(WeatherElement[] cells, int width, int x, int y)
        {
            var choices = new WeatherElement[Spawnable.Length];
            var count = 0;
            foreach (var kind in Spawnable)
            {
                if (x >= 2 && cells[y * width + x - 1] == kind &&
                    cells[y * width + x - 2] == kind) continue;
                if (y >= 2 && cells[(y - 1) * width + x] == kind &&
                    cells[(y - 2) * width + x] == kind) continue;

                choices[count++] = kind;
            }

            return choices[_random.Next(count)];
        }
    }
}
