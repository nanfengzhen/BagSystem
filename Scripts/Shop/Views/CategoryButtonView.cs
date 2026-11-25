using System;
using UnityEngine;
using UnityEngine.UI;

namespace BagSystem.Shop
{
    public sealed class CategoryButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Sprite normal, selected;
        [SerializeField] private GameObject selectedMark;
        [SerializeField] private ShopCategory category;
        public ShopCategory Category => category;
        public event Action<ShopCategory> Clicked;
        private void Awake() { button.onClick.AddListener(HandleClick); }
        private void OnDestroy() { if (button != null) button.onClick.RemoveListener(HandleClick); }
        private void HandleClick() { Clicked?.Invoke(category); }
        public void SetSelected(bool value)
        {
            background.sprite = value ? selected : normal;
            selectedMark.SetActive(value);
        }
    }
}
