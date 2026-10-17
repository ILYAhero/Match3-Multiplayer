using System;
using MonoOrbit.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MonoOrbit.Presentation
{
    /// <summary>Touch and pointer adapter; game rules live in Gameplay.</summary>
    internal sealed class MatchCellView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private CellPosition _position;
        private Image _orb;
        private Image _aura;
        private Action<CellPosition> _clicked;
        private Action<CellPosition, CellPosition> _swiped;
        private WeatherElement _element;
        private bool _dragged;

        public RectTransform Rect => (RectTransform)transform;
        public WeatherElement Element => _element;

        public void Initialize(CellPosition position, Image orb, Image aura,
            Action<CellPosition> clicked, Action<CellPosition, CellPosition> swiped)
        {
            _position = position;
            _orb = orb;
            _aura = aura;
            _clicked = clicked;
            _swiped = swiped;
        }

        public void SetElement(WeatherElement element, MonoOrbitArt art)
        {
            _element = element;
            _orb.sprite = art.Orb(element);
            _orb.enabled = element != WeatherElement.Empty;
            var color = MonoOrbitArt.ElementColor(element);
            color.a = 0.42f;
            _aura.color = color;
            SetOpacity(1f);
            SetSelected(false);
        }

        public void SetOpacity(float opacity)
        {
            var orb = _orb.color;
            orb.a = opacity;
            _orb.color = orb;

            var aura = _aura.color;
            aura.a = Mathf.Min(opacity, 1f) * 0.42f;
            _aura.color = aura;
        }

        public void SetSelected(bool selected)
        {
            _orb.rectTransform.localScale = Vector3.one * (selected ? 1.16f : 1f);
            var aura = _aura.color;
            aura.a = selected ? 0.72f : 0.42f;
            _aura.color = aura;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_dragged && eventData.button == PointerEventData.InputButton.Left)
                _clicked?.Invoke(_position);
            _dragged = false;
        }

        public void OnBeginDrag(PointerEventData eventData) { _dragged = true; }
        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            _dragged = true;
            var drag = eventData.position - eventData.pressPosition;
            if (drag.sqrMagnitude < 35f * 35f) return;

            var delta = Mathf.Abs(drag.x) > Mathf.Abs(drag.y)
                ? new CellPosition(drag.x > 0f ? 1 : -1, 0)
                : new CellPosition(0, drag.y > 0f ? 1 : -1);

            _swiped?.Invoke(_position,
                new CellPosition(_position.X + delta.X, _position.Y + delta.Y));
        }
    }
}
