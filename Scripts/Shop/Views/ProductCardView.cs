using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BagSystem.Shop
{
    public sealed class ProductCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title, type, price;
        [SerializeField] private GameObject selectionBorder, selectedTag;
        private string productId;
        public event Action<string> Clicked;

        public void Configure(Button b, Image i, TMP_Text n, TMP_Text t, TMP_Text p, GameObject border, GameObject tag)
        { button = b; icon = i; title = n; type = t; price = p; selectionBorder = border; selectedTag = tag; }
        private void Awake() { button.onClick.AddListener(HandleClick); }
        private void OnDestroy() { if (button != null) button.onClick.RemoveListener(HandleClick); }
        private void HandleClick() { Clicked?.Invoke(productId); }
        public void Bind(ShopProduct p)
        {
            productId = p.productId;
            icon.sprite = p.icon;
            title.text = p.displayName;
            type.text = p.typeLabel;
            type.color = p.typeColor;
            price.text = p.unitPrice.ToString("N0");
            SetSelected(false);
        }
        public void SetSelected(bool selected)
        { selectionBorder.SetActive(selected); selectedTag.SetActive(selected); }
    }
}
