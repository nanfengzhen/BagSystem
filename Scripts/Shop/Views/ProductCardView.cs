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

        //初始化按钮
        private void Awake() 
        { 
            button.onClick.AddListener(HandleClick); 
        }
        private void OnDestroy() 
        { 
            if (button != null) button.onClick.RemoveListener(HandleClick); 
        }

        //点击按钮触发发送请求
        private void HandleClick() 
        { 
            Clicked?.Invoke(productId);
        }

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
        { 
            selectionBorder.SetActive(selected); selectedTag.SetActive(selected); 
        }
    }
}
