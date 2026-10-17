using System;
using MonoOrbit.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace MonoOrbit.Presentation
{
    /// <summary>
    /// Builds a responsive, portrait-first canvas. Static geometry and sprites are
    /// initialized once; match results only update the existing cell views.
    /// </summary>
    internal sealed class MonoOrbitScreenLayout
    {
        private readonly Canvas _canvas;
        private readonly RectTransform _boardFrame;
        private readonly RectTransform _gridRect;
        private readonly GridLayoutGroup _grid;
        private float _lastWidth;
        private float _lastHeight;

        public readonly MatchCellView[,] Cells =
            new MatchCellView[MatchRules.Width, MatchRules.Height];

        public Text ScoreText { get; }
        public Text StatusText { get; }
        public Text ComboText { get; }
        public RectTransform InnerOrbit { get; }
        public RectTransform OuterOrbit { get; }

        public MonoOrbitScreenLayout(Transform parent, MonoOrbitArt art,
            Action<CellPosition> clicked, Action<CellPosition, CellPosition> swiped,
            Action restart)
        {
            EnsureEventSystem();

            var canvasObject = new GameObject("Mono Orbit UI",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 20;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var backdrop = Image(canvasObject.transform, "Night atmosphere",
                art.Background, Color.white);
            Stretch(backdrop.rectTransform);

            OuterOrbit = Image(canvasObject.transform, "Outer orbit",
                art.Ring, Color.white).rectTransform;
            Anchor(OuterOrbit, new Vector2(0.5f, 0.5f),
                new Vector2(1540f, 1540f), new Vector2(0f, -35f));

            InnerOrbit = Image(canvasObject.transform, "Inner orbit",
                art.Ring, new Color(0.64f, 0.83f, 1f, 0.62f)).rectTransform;
            Anchor(InnerOrbit, new Vector2(0.5f, 0.5f),
                new Vector2(1160f, 1160f), new Vector2(0f, -35f));

            var accent = new Color(0.55f, 0.91f, 0.98f);
            var white = new Color(0.90f, 0.96f, 1f);
            var muted = new Color(0.53f, 0.68f, 0.81f);

            Text(canvasObject.transform, "Brand", "M O N O  /  O R B I T",
                57, white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(980f, 90f),
                new Vector2(0f, -123f), FontStyle.Bold);

            Text(canvasObject.transform, "Brand eyebrow", "ELEMENTAL DUEL • WEATHER SYSTEM",
                24, accent, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(980f, 50f),
                new Vector2(0f, -191f));

            var badge = Image(canvasObject.transform, "Mode badge",
                art.Tile, new Color(0.55f, 0.85f, 0.98f, 0.58f));
            Anchor(badge.rectTransform, new Vector2(0.5f, 1f),
                new Vector2(320f, 66f), new Vector2(0f, -254f));

            Text(badge.transform, "Mode text", "TRAINING  /  OFFLINE",
                22, white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(320f, 54f), Vector2.zero, FontStyle.Bold);

            var scorePanel = Image(canvasObject.transform, "Score panel",
                art.Tile, new Color(0.75f, 0.89f, 1f, 0.85f));
            Anchor(scorePanel.rectTransform, new Vector2(0.5f, 1f),
                new Vector2(720f, 150f), new Vector2(0f, -373f));

            Text(scorePanel.transform, "Score title", "YOUR SCORE",
                24, muted, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f),
                new Vector2(610f, 42f), new Vector2(0f, 33f), FontStyle.Bold);

            ScoreText = Text(scorePanel.transform, "Score value", "0",
                67, white, TextAnchor.MiddleLeft, new Vector2(0.5f, 0.5f),
                new Vector2(600f, 82f), new Vector2(0f, -20f), FontStyle.Bold);

            var frameHalo = Image(canvasObject.transform, "Board halo", art.Glow,
                new Color(0.22f, 0.57f, 0.86f, 0.35f));
            Anchor(frameHalo.rectTransform, new Vector2(0.5f, 0.5f),
                new Vector2(1140f, 1140f), new Vector2(0f, -70f));

            var frame = Image(canvasObject.transform, "Board shell", art.Tile,
                new Color(0.45f, 0.69f, 0.93f));
            _boardFrame = frame.rectTransform;
            Anchor(_boardFrame, new Vector2(0.5f, 0.5f),
                new Vector2(980f, 980f), new Vector2(0f, -70f));

            var center = Image(frame.transform, "Board inset", art.Tile,
                new Color(0.25f, 0.44f, 0.70f, 0.95f));
            Stretch(center.rectTransform, 14f);

            var gridObject = new GameObject("Grid 9x9", typeof(RectTransform),
                typeof(GridLayoutGroup));
            gridObject.transform.SetParent(center.transform, false);
            _gridRect = gridObject.GetComponent<RectTransform>();
            Stretch(_gridRect, 12f);

            _grid = gridObject.GetComponent<GridLayoutGroup>();
            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = MatchRules.Width;
            _grid.childAlignment = TextAnchor.MiddleCenter;
            _grid.spacing = new Vector2(5f, 5f);
            _grid.padding = new RectOffset(6, 6, 6, 6);

            // Unity GridLayoutGroup fills top to bottom, while our domain's Y=0
            // is the bottom row. Reverse Y during view creation only.
            for (var y = MatchRules.Height - 1; y >= 0; y--)
            {
                for (var x = 0; x < MatchRules.Width; x++)
                {
                    var cellObject = new GameObject(
                        "Cell " + x + ":" + y, typeof(RectTransform),
                        typeof(Image), typeof(MatchCellView));
                    cellObject.transform.SetParent(_gridRect, false);

                    var tile = cellObject.GetComponent<Image>();
                    tile.sprite = art.Tile;
                    tile.color = Color.white;
                    tile.raycastTarget = true;

                    var aura = Image(cellObject.transform, "Element glow",
                        art.Glow, Color.clear);
                    Stretch(aura.rectTransform, -7f);

                    var orb = Image(cellObject.transform, "Weather orb",
                        null, Color.white);
                    Stretch(orb.rectTransform, 4f);
                    orb.preserveAspect = true;

                    var cell = cellObject.GetComponent<MatchCellView>();
                    cell.Initialize(new CellPosition(x, y), orb, aura, clicked, swiped);
                    Cells[x, y] = cell;
                }
            }

            StatusText = Text(canvasObject.transform, "Instruction",
                "SWIPE ANY ADJACENT ORBS TO MATCH 3",
                28, muted, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(1000f, 72f),
                new Vector2(0f, 329f));

            ComboText = Text(canvasObject.transform, "Combo",
                "SIX WEATHER ELEMENTS  •  ONE ORBIT",
                22, accent, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(1000f, 48f),
                new Vector2(0f, 270f), FontStyle.Bold);

            var buttonPanel = Image(canvasObject.transform, "Restart button",
                art.Tile, new Color(0.35f, 0.71f, 0.94f));
            Anchor(buttonPanel.rectTransform, new Vector2(0.5f, 0f),
                new Vector2(620f, 98f), new Vector2(0f, 150f));
            var button = buttonPanel.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonPanel;
            var palette = button.colors;
            palette.normalColor = Color.white;
            palette.highlightedColor = new Color(0.84f, 1f, 1f);
            palette.pressedColor = new Color(0.54f, 0.80f, 0.94f);
            button.colors = palette;
            button.onClick.AddListener(() => restart?.Invoke());

            Text(buttonPanel.transform, "Restart label", "NEW CONSTELLATION",
                32, white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f),
                new Vector2(600f, 86f), Vector2.zero, FontStyle.Bold);

            Text(canvasObject.transform, "Footer",
                "MONO ORBIT  ·  MATCH ENGINE v1",
                19, muted, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(920f, 50f),
                new Vector2(0f, 57f));

            UpdateSize();
        }

        public void UpdateSize()
        {
            var canvasRect = (RectTransform)_canvas.transform;
            var width = canvasRect.rect.width;
            var height = canvasRect.rect.height;
            if (width < 1f || height < 1f) return;
            if (Mathf.Abs(width - _lastWidth) < 1f &&
                Mathf.Abs(height - _lastHeight) < 1f) return;

            _lastWidth = width;
            _lastHeight = height;

            // Never let the board overlap the score header or bottom action.
            var maxWidth = width - 64f;
            var maxHeight = height - 790f;
            var boardSize = Mathf.Max(180f, Mathf.Min(980f,
                Mathf.Min(maxWidth, maxHeight)));
            _boardFrame.sizeDelta = new Vector2(boardSize, boardSize);

            // Frame 14 + inset 12 + grid padding 6 on each side.
            var innerSize = boardSize - 52f;
            var cellSize = (innerSize - 12f - 8f * _grid.spacing.x) / MatchRules.Width;
            _grid.cellSize = new Vector2(cellSize, cellSize);
            _gridRect.sizeDelta = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var system = new GameObject("Mono Orbit EventSystem",
                typeof(EventSystem), typeof(InputSystemUIInputModule));
            system.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        private static RectTransform Rect(Transform parent, string name)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image Image(Transform parent, string name, Sprite sprite, Color color)
        {
            var rect = Rect(parent, name);
            var result = rect.gameObject.AddComponent<Image>();
            result.sprite = sprite;
            result.color = color;
            result.raycastTarget = false;
            return result;
        }

        private static Text Text(Transform parent, string name, string value,
            int size, Color color, TextAnchor align, Vector2 anchor,
            Vector2 dimensions, Vector2 position, FontStyle style = FontStyle.Normal)
        {
            var rect = Rect(parent, name);
            Anchor(rect, anchor, dimensions, position);

            var text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = align;
            text.text = value;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void Anchor(RectTransform rect, Vector2 anchor,
            Vector2 dimensions, Vector2 position)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
