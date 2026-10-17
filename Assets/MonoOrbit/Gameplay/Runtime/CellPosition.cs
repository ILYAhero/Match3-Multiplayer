using System;

namespace MonoOrbit.Gameplay
{
    public readonly struct CellPosition : IEquatable<CellPosition>
    {
        public int X { get; }
        public int Y { get; }

        public CellPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(CellPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is CellPosition other && Equals(other);
        public override int GetHashCode()
        {
            unchecked { return (X * 397) ^ Y; }
        }

        public static bool operator ==(CellPosition left, CellPosition right) => left.Equals(right);
        public static bool operator !=(CellPosition left, CellPosition right) => !left.Equals(right);
        public override string ToString() => $"({X}, {Y})";
    }
}
