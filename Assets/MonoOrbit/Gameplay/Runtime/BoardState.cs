using System;
using System.Collections.Generic;

namespace MonoOrbit.Gameplay
{
    /// <summary>
    /// Immutable, row-major board snapshot. Y=0 is the bottom row.
    /// Copies input data so no caller can mutate the authoritative state.
    /// </summary>
    public sealed class BoardState
    {
        private readonly WeatherElement[] _cells;

        public int Width { get; }
        public int Height { get; }

        public BoardState(int width, int height, IReadOnlyList<WeatherElement> cells)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (cells == null) throw new ArgumentNullException(nameof(cells));
            if (cells.Count != checked(width * height))
                throw new ArgumentException("The cell count does not match the board dimensions.", nameof(cells));

            Width = width;
            Height = height;
            _cells = new WeatherElement[cells.Count];
            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i] > WeatherElement.Cloud)
                    throw new ArgumentOutOfRangeException(nameof(cells), "Unknown element ID.");
                _cells[i] = cells[i];
            }
        }

        public WeatherElement Get(CellPosition position)
        {
            if (!Contains(position)) throw new ArgumentOutOfRangeException(nameof(position));
            return _cells[position.Y * Width + position.X];
        }

        public bool Contains(CellPosition position) =>
            position.X >= 0 && position.X < Width &&
            position.Y >= 0 && position.Y < Height;

        public WeatherElement[] ToArray() => (WeatherElement[])_cells.Clone();

        internal BoardState Swapped(CellPosition a, CellPosition b)
        {
            if (!Contains(a)) throw new ArgumentOutOfRangeException(nameof(a));
            if (!Contains(b)) throw new ArgumentOutOfRangeException(nameof(b));

            var cells = ToArray();
            var firstIndex = a.Y * Width + a.X;
            var secondIndex = b.Y * Width + b.X;
            var temp = cells[firstIndex];
            cells[firstIndex] = cells[secondIndex];
            cells[secondIndex] = temp;
            return new BoardState(Width, Height, cells);
        }
    }
}
