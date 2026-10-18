using System;
using MonoOrbit.Gameplay;
using UnityEngine;

namespace MonoOrbit.Presentation
{
    /// <summary>Optional authored graphics; the domain element IDs remain stable.</summary>
    [CreateAssetMenu(menuName = "Mono Orbit/Cozy World Theme")]
    public sealed class CozyWorldTheme : ScriptableObject
    {
        [SerializeField] private Sprite[] weatherSprites = new Sprite[6];
        [SerializeField] private float[] visibleScale = { 1f, 1f, 1f, 1f, 1f, 1f };
        [SerializeField] private Sprite cellBase;
        [SerializeField] private Sprite panel;
        [SerializeField] private Sprite banner;
        public Sprite CellBase => cellBase;
        public Sprite Panel => panel;
        public Sprite Banner => banner;
        public bool Ready => weatherSprites != null && weatherSprites.Length == 6 &&
            Array.TrueForAll(weatherSprites, sprite => sprite != null);

        public Sprite Get(WeatherElement element)
        {
            int index = (int)element - 1;
            return index >= 0 && index < 6 && weatherSprites != null &&
                index < weatherSprites.Length ? weatherSprites[index] : null;
        }
        public float GetScale(WeatherElement element)
        {
            int index = (int)element - 1;
            return index >= 0 && visibleScale != null && index < visibleScale.Length
                ? Mathf.Clamp(visibleScale[index], 0.5f, 2.5f) : 1f;
        }

#if UNITY_EDITOR
        public void Assign(Sprite[] sprites, float[] scales, Sprite baseCell, Sprite creamPanel, Sprite woodBanner)
        {
            if (sprites == null || scales == null || sprites.Length != 6 || scales.Length != 6)
                throw new ArgumentException("Six mapped elements required");
            weatherSprites = (Sprite[])sprites.Clone();
            visibleScale = (float[])scales.Clone();
            cellBase = baseCell;
            panel = creamPanel;
            banner = woodBanner;
        }
#endif
    }
}
