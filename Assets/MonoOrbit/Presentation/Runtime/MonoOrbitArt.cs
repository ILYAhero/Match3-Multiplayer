using System;
using System.Collections.Generic;
using MonoOrbit.Gameplay;
using UnityEngine;

namespace MonoOrbit.Presentation
{
    /// <summary>
    /// Generates cached, resolution-independent-feeling placeholder-free vector-like
    /// raster art. One texture per element, never recreated per tile or frame.
    /// Replaceable by authored sprites without changing input or game logic.
    /// </summary>
    internal sealed class MonoOrbitArt : IDisposable
    {
        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();
        private readonly Dictionary<WeatherElement, Sprite> _orbs = new Dictionary<WeatherElement, Sprite>();

        public Sprite Background { get; }
        public Sprite Tile { get; }
        public Sprite Ring { get; }
        public Sprite Glow { get; }

        public MonoOrbitArt()
        {
            Background = Make("Orbit.Background", 256, (x, y) =>
            {
                var radial = Mathf.Clamp01(Mathf.Sqrt(x * x * 0.55f + y * y * 0.75f));
                var wave = Mathf.Exp(-Mathf.Pow((y + 0.35f - 0.13f * Mathf.Sin(x * 3.5f)) * 2.2f, 2f));
                var top = new Color(0.045f, 0.09f, 0.18f);
                var bottom = new Color(0.014f, 0.027f, 0.065f);
                var baseColor = Color.Lerp(top, bottom, Mathf.Clamp01((y + 1f) * 0.52f));
                baseColor = Color.Lerp(baseColor, new Color(0.08f, 0.13f, 0.24f), wave * 0.28f);
                return Color.Lerp(baseColor, new Color(0.006f, 0.013f, 0.037f), radial * 0.62f);
            });

            Tile = Make("Orbit.Tile", 128, (x, y) =>
            {
                var d = RoundRect(x, y, 0.87f, 0.87f, 0.15f);
                var alpha = SmoothInside(d, 0.022f);
                var rim = SmoothInside(d, 0.022f) - SmoothInside(d + 0.042f, 0.022f);
                var topLight = Mathf.Clamp01((y + 1f) * 0.5f);
                var color = Color.Lerp(new Color(0.055f, 0.10f, 0.19f),
                    new Color(0.095f, 0.16f, 0.29f), topLight * 0.62f);
                color = Color.Lerp(color, new Color(0.22f, 0.35f, 0.52f), rim * 0.33f);
                color.a = alpha;
                return color;
            });

            Ring = Make("Orbit.Ring", 256, (x, y) =>
            {
                var radius = Mathf.Sqrt(x * x + y * y);
                var edge = Mathf.Clamp01(1f - Mathf.Abs(radius - 0.81f) * 110f);
                var angle = Mathf.Atan2(y, x);
                var accents = Mathf.Pow(Mathf.Clamp01(Mathf.Sin(angle * 3f + 0.8f)), 18f);
                var color = Color.Lerp(new Color(0.11f, 0.48f, 0.70f),
                    new Color(0.38f, 0.85f, 0.95f), accents);
                color.a = edge * (0.17f + accents * 0.30f);
                return color;
            });

            Glow = Make("Orbit.Glow", 128, (x, y) =>
            {
                var radius = Mathf.Sqrt(x * x + y * y);
                return new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - radius), 2f) * 0.72f);
            });

            foreach (WeatherElement kind in Enum.GetValues(typeof(WeatherElement)))
            {
                if (kind == WeatherElement.Empty) continue;
                var element = kind;
                _orbs[element] = Make("Orbit.Orb." + element, 192,
                    (x, y) => DrawOrb(element, x, y));
            }
        }

        public Sprite Orb(WeatherElement kind) =>
            _orbs.TryGetValue(kind, out var sprite) ? sprite : null;

        public static Color ElementColor(WeatherElement kind)
        {
            switch (kind)
            {
                case WeatherElement.Sun: return new Color(1f, 0.70f, 0.35f);
                case WeatherElement.Rain: return new Color(0.35f, 0.70f, 1f);
                case WeatherElement.Snow: return new Color(0.62f, 0.96f, 1f);
                case WeatherElement.Wind: return new Color(0.43f, 0.93f, 0.78f);
                case WeatherElement.Thunder: return new Color(0.70f, 0.55f, 1f);
                case WeatherElement.Cloud: return new Color(1f, 0.53f, 0.76f);
                default: return Color.clear;
            }
        }

        private static Color DrawOrb(WeatherElement element, float x, float y)
        {
            var radius = Mathf.Sqrt(x * x + y * y);
            if (radius > 0.995f) return Color.clear;

            var baseColor = ElementColor(element);
            var inner = Mathf.Clamp01(1f - radius / 0.86f);
            var illumination = 0.29f + 0.56f * Mathf.Pow(inner, 0.65f);
            var color = Color.Lerp(new Color(0.035f, 0.058f, 0.13f), baseColor, illumination);
            var upperHighlight = Mathf.Pow(Mathf.Clamp01(1f -
                Mathf.Sqrt((x + 0.27f) * (x + 0.27f) + (y - 0.40f) * (y - 0.40f)) * 1.8f), 3f);
            color = Color.Lerp(color, Color.white, upperHighlight * 0.35f);

            var edge = Mathf.Abs(radius - 0.79f);
            var glassRim = Mathf.Clamp01(1f - edge * 30f);
            color = Color.Lerp(color, Color.white, glassRim * 0.30f);

            var glyph = Glyph(element, x, y);
            color = Color.Lerp(color, new Color(0.96f, 0.99f, 1f), glyph * 0.93f);

            var bodyAlpha = SmoothInside(radius - 0.84f, 0.022f);
            var halo = Mathf.Pow(Mathf.Clamp01((0.995f - radius) / 0.18f), 1.8f) * 0.18f;
            color.a = Mathf.Clamp01(bodyAlpha + (1f - bodyAlpha) * halo);
            return color;
        }

        private static float Glyph(WeatherElement element, float x, float y)
        {
            const float halfWidth = 0.037f;
            switch (element)
            {
                case WeatherElement.Sun:
                {
                    var r = Mathf.Sqrt(x * x + y * y);
                    var core = 1f - Mathf.Clamp01((r - 0.22f) * 80f);
                    var angle = Mathf.Atan2(y, x);
                    var spoke = Mathf.Abs(Mathf.Sin(angle * 4f));
                    var rays = r > 0.28f && r < 0.43f ? 1f - Mathf.Clamp01((spoke - 0.15f) * 20f) : 0f;
                    return Mathf.Max(core, rays);
                }
                case WeatherElement.Rain:
                {
                    var inDrop = y < -0.04f
                        ? Mathf.Sqrt((x / 0.28f) * (x / 0.28f) +
                           ((y + 0.10f) / 0.32f) * ((y + 0.10f) / 0.32f)) < 1f
                        : Mathf.Abs(x) < 0.27f * Mathf.Clamp01((0.46f - y) / 0.50f);
                    return inDrop && y < 0.42f && y > -0.43f ? 1f : 0f;
                }
                case WeatherElement.Snow:
                {
                    var alpha = 0f;
                    for (var i = 0; i < 3; i++)
                    {
                        var a = i * Mathf.PI / 3f;
                        var along = x * Mathf.Cos(a) + y * Mathf.Sin(a);
                        var across = -x * Mathf.Sin(a) + y * Mathf.Cos(a);
                        if (Mathf.Abs(along) < 0.42f && Mathf.Abs(across) < halfWidth) alpha = 1f;
                    }
                    return alpha;
                }
                case WeatherElement.Wind:
                {
                    if (Mathf.Abs(x) > 0.43f) return 0f;
                    var line1 = Mathf.Abs(y - 0.20f - 0.07f * Mathf.Sin(x * 8f));
                    var line2 = Mathf.Abs(y + 0.01f - 0.06f * Mathf.Sin(x * 7f + 1.2f));
                    var line3 = Mathf.Abs(y + 0.22f - 0.05f * Mathf.Sin(x * 9f + 2.5f));
                    return Mathf.Min(line1, Mathf.Min(line2, line3)) < halfWidth ? 1f : 0f;
                }
                case WeatherElement.Thunder:
                {
                    // A thick, broken diagonal bolt with a sharp corner.
                    var top = DistanceToSegment(x, y, 0.14f, 0.43f, -0.14f, -0.02f);
                    var bottom = DistanceToSegment(x, y, -0.14f, -0.02f, 0.09f, -0.40f);
                    return Mathf.Min(top, bottom) < 0.09f ? 1f : 0f;
                }
                case WeatherElement.Cloud:
                {
                    var left = Circle(x + 0.20f, y + 0.02f, 0.22f);
                    var middle = Circle(x, y - 0.12f, 0.28f);
                    var right = Circle(x - 0.22f, y + 0.02f, 0.20f);
                    var baseLayer = Mathf.Abs(x) < 0.40f && y > -0.26f && y < 0.12f;
                    return (left || middle || right || baseLayer) ? 1f : 0f;
                }
                default: return 0f;
            }
        }

        private static bool Circle(float x, float y, float radius) =>
            x * x + y * y <= radius * radius;

        private static float DistanceToSegment(float x, float y,
            float ax, float ay, float bx, float by)
        {
            var vx = bx - ax;
            var vy = by - ay;
            var t = Mathf.Clamp01(((x - ax) * vx + (y - ay) * vy) / (vx * vx + vy * vy));
            var dx = x - (ax + t * vx);
            var dy = y - (ay + t * vy);
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private static float RoundRect(float x, float y, float halfX, float halfY, float radius)
        {
            var px = Mathf.Abs(x) - halfX + radius;
            var py = Mathf.Abs(y) - halfY + radius;
            return Mathf.Sqrt(Mathf.Max(px, 0f) * Mathf.Max(px, 0f) +
                              Mathf.Max(py, 0f) * Mathf.Max(py, 0f))
                + Mathf.Min(Mathf.Max(px, py), 0f) - radius;
        }

        private static float SmoothInside(float distance, float edge) =>
            Mathf.Clamp01(0.5f - distance / edge);

        private Sprite Make(string name, int size, Func<float, float, Color> pixel)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var colors = new Color[size * size];
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var nx = (x + 0.5f) / size * 2f - 1f;
                    var ny = (y + 0.5f) / size * 2f - 1f;
                    colors[y * size + x] = pixel(nx, ny);
                }

            texture.SetPixels(colors);
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), size);
            sprite.name = name;
            _created.Add(texture);
            _created.Add(sprite);
            return sprite;
        }

        public void Dispose()
        {
            foreach (var resource in _created)
                if (resource != null) UnityEngine.Object.Destroy(resource);
            _created.Clear();
            _orbs.Clear();
        }
    }
}
