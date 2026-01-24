using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Common
{
    /// <summary>
    /// Flyweightスクロールビューのビューポートコンポーネント
    /// RectTransformのサイズ変更を監視し、イベントを発火します
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(RectMask2D))]
    public class FlyweightScrollViewport : MonoBehaviour
    {
        private const float SizeEpsilon = 0.01f;

        /// <summary>
        /// ビューポートのRectTransform
        /// </summary>
        [SerializeField]
        private RectTransform rectTransform;

        private bool isDirty = false;
        private Vector2 lastSize;

        /// <summary>
        /// ビューポートのRectTransform
        /// </summary>
        public RectTransform RectTransform => rectTransform;

        /// <summary>
        /// RectTransformのサイズが変更されたときに発火するイベント
        /// </summary>
        public event Action OnRectTransformDimensionsChanged;

        /// <summary>
        /// Unity組み込みコールバック。RectTransformのサイズ変更時に呼び出されます
        /// </summary>
        private void Awake()
        {
            EnsureRectTransform();
            lastSize = rectTransform.rect.size;
        }

        private void OnEnable()
        {
            EnsureRectTransform();
            lastSize = rectTransform.rect.size;
        }

        private void OnRectTransformDimensionsChange()
        {
            EnsureRectTransform();

            var currentSize = rectTransform.rect.size;
            if (Mathf.Abs(currentSize.x - lastSize.x) < SizeEpsilon &&
                Mathf.Abs(currentSize.y - lastSize.y) < SizeEpsilon)
            {
                return;
            }

            lastSize = currentSize;
            isDirty = true;
        }

        private void Update()
        {
            if (!isDirty)
                return;
            OnRectTransformDimensionsChanged?.Invoke();
            isDirty = false;
        }

        private void EnsureRectTransform()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }
    }
}
