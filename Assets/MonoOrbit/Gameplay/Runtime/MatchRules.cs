namespace MonoOrbit.Gameplay
{
    /// <summary>
    /// Initial competitive rules; version these constants when networking is added.
    /// </summary>
    public static class MatchRules
    {
        public const int Width = 9;
        public const int Height = 9;
        public const int PointsPerClearedCell = 10;
        public const int MaxCascadeSteps = 128;
    }
}
