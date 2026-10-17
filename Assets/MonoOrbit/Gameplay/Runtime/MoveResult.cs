using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonoOrbit.Gameplay
{
    /// <summary>One resolved cascade, with states suitable for presentation playback.</summary>
    public sealed class CascadeStep
    {
        public int Index { get; }
        public BoardState BeforeClear { get; }
        public BoardState AfterRefill { get; }
        public IReadOnlyList<CellPosition> ClearedCells { get; }
        public int ScoreDelta { get; }

        internal CascadeStep(int index, BoardState beforeClear, BoardState afterRefill,
            IReadOnlyList<CellPosition> clearedCells, int scoreDelta)
        {
            Index = index;
            BeforeClear = beforeClear;
            AfterRefill = afterRefill;
            var copy = new CellPosition[clearedCells.Count];
            for (var i = 0; i < copy.Length; i++) copy[i] = clearedCells[i];
            ClearedCells = new ReadOnlyCollection<CellPosition>(copy);
            ScoreDelta = scoreDelta;
        }
    }

    public sealed class MoveResult
    {
        public bool Accepted { get; }
        public BoardState FinalBoard { get; }
        public IReadOnlyList<CascadeStep> Cascades { get; }
        public int ScoreDelta { get; }
        public bool WasRegenerated { get; }

        private MoveResult(bool accepted, BoardState finalBoard,
            IReadOnlyList<CascadeStep> cascades, int scoreDelta, bool wasRegenerated)
        {
            Accepted = accepted;
            FinalBoard = finalBoard ?? throw new ArgumentNullException(nameof(finalBoard));
            var copy = new CascadeStep[cascades.Count];
            for (var i = 0; i < copy.Length; i++) copy[i] = cascades[i];
            Cascades = new ReadOnlyCollection<CascadeStep>(copy);
            ScoreDelta = scoreDelta;
            WasRegenerated = wasRegenerated;
        }

        internal static MoveResult Rejected(BoardState unchanged) =>
            new MoveResult(false, unchanged, Array.Empty<CascadeStep>(), 0, false);

        internal static MoveResult Successful(BoardState finalBoard, IReadOnlyList<CascadeStep> cascades,
            int scoreDelta, bool wasRegenerated) =>
            new MoveResult(true, finalBoard, cascades, scoreDelta, wasRegenerated);
    }
}
