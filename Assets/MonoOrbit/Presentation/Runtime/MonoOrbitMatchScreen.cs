using System.Collections;
using MonoOrbit.Application;
using MonoOrbit.Gameplay;
using UnityEngine;

namespace MonoOrbit.Presentation
{
    /// <summary>
    /// Training-mode composition root and visual presenter. The match engine
    /// stays authoritative; UI never edits the board or computes score.
    /// Multiplayer will provide its own session adapter in a later slice.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonoOrbitMatchScreen : MonoBehaviour
    {
        private PracticeSession _session;
        private MonoOrbitArt _art;
        private CozyWorldTheme _cozy;
        private MonoOrbitScreenLayout _layout;
        private CellPosition? _selected;
        private bool _busy;

        private void Start()
        {
            _session = new PracticeSession(unchecked((uint)System.Environment.TickCount));
            _art = new MonoOrbitArt();
            _cozy = Resources.Load<CozyWorldTheme>("CozyWorldTheme");
            _layout = new MonoOrbitScreenLayout(transform, _art, _cozy,
                HandleClick, HandleSwipe, Restart);

            Canvas.ForceUpdateCanvases();
            _layout.UpdateSize();
            DrawBoard(_session.Board);
            UpdateScore();
        }

        private void Update()
        {
            if (_layout == null) return;
            _layout.UpdateSize();
            _layout.InnerOrbit.Rotate(0f, 0f, 2.3f * Time.unscaledDeltaTime);
            _layout.OuterOrbit.Rotate(0f, 0f, -1.3f * Time.unscaledDeltaTime);
        }

        private void HandleClick(CellPosition position)
        {
            if (_busy) return;

            if (!_selected.HasValue)
            {
                Select(position);
                return;
            }

            var from = _selected.Value;
            if (from == position)
            {
                ClearSelection();
                return;
            }

            if (Mathf.Abs(from.X - position.X) +
                Mathf.Abs(from.Y - position.Y) == 1)
            {
                ClearSelection();
                TryMove(from, position);
            }
            else
            {
                Select(position);
            }
        }

        private void HandleSwipe(CellPosition from, CellPosition to)
        {
            if (_busy) return;
            ClearSelection();
            TryMove(from, to);
        }

        private void Select(CellPosition position)
        {
            ClearSelection();
            _selected = position;
            _layout.Cells[position.X, position.Y].SetSelected(true);
            _layout.StatusText.text = "SWIPE OR TAP AN ADJACENT ELEMENT";
        }

        private void ClearSelection()
        {
            if (!_selected.HasValue) return;
            var position = _selected.Value;
            _layout.Cells[position.X, position.Y].SetSelected(false);
            _selected = null;
        }

        private void TryMove(CellPosition from, CellPosition to)
        {
            var result = _session.TrySwap(from, to);
            if (!result.Accepted)
            {
                _layout.StatusText.text = "NO MATCH  ·  TRY ANOTHER SWAP";
                return;
            }

            _busy = true;
            StartCoroutine(PlayCascades(result));
        }

        private IEnumerator PlayCascades(MoveResult result)
        {
            for (var stepIndex = 0; stepIndex < result.Cascades.Count; stepIndex++)
            {
                var step = result.Cascades[stepIndex];
                DrawBoard(step.BeforeClear);

                _layout.ComboText.text = step.Index > 1
                    ? "CASCADE  ×" + step.Index
                    : "ELEMENTAL MATCH";
                _layout.StatusText.text = "+" + step.ScoreDelta + " POINTS";

                var elapsed = 0f;
                const float clearDuration = 0.21f;
                while (elapsed < clearDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    var fraction = Mathf.Clamp01(elapsed / clearDuration);
                    var opacity = 1f - fraction * fraction;
                    foreach (var cell in step.ClearedCells)
                        _layout.Cells[cell.X, cell.Y].SetOpacity(opacity);
                    yield return null;
                }

                DrawBoard(step.AfterRefill);
                var refillElapsed = 0f;
                const float refillDuration = 0.18f;
                while (refillElapsed < refillDuration)
                {
                    refillElapsed += Time.unscaledDeltaTime;
                    var t = Mathf.Clamp01(refillElapsed / refillDuration);
                    var eased = 1f - (1f - t) * (1f - t) * (1f - t);
                    var size = 0.74f + 0.26f * eased;
                    for (var y = 0; y < MatchRules.Height; y++)
                        for (var x = 0; x < MatchRules.Width; x++)
                            _layout.Cells[x, y].SetVisualScale(size);
                    yield return null;
                }
            }

            DrawBoard(result.FinalBoard);
            UpdateScore();

            if (result.WasRegenerated)
                _layout.ComboText.text = "NEW PLAYABLE CONSTELLATION";
            else if (result.Cascades.Count > 1)
                _layout.ComboText.text = "COMBO  ×" + result.Cascades.Count;
            else
                _layout.ComboText.text = "KEEP THE ORBIT MOVING";

            _layout.StatusText.text = "SWIPE ADJACENT ORBS TO MATCH 3";
            _busy = false;
        }

        private void DrawBoard(BoardState board)
        {
            for (var y = 0; y < board.Height; y++)
                for (var x = 0; x < board.Width; x++)
                    _layout.Cells[x, y].SetElement(
                        board.Get(new CellPosition(x, y)), _art, _cozy);
        }

        private void UpdateScore()
        {
            _layout.ScoreText.text = _session.Score.ToString("N0");
        }

        private void Restart()
        {
            if (_busy) return;
            ClearSelection();
            _session.StartNewGame();
            DrawBoard(_session.Board);
            UpdateScore();
            _layout.StatusText.text = "NEW CONSTELLATION READY";
            _layout.ComboText.text = "SIX WEATHER ELEMENTS  •  ONE ORBIT";
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _art?.Dispose();
        }
    }
}
