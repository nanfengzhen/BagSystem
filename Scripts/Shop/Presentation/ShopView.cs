using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BagSystem.Shop
{
    public sealed class ShopView : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private ProductCardView cardPrefab;
        [SerializeField] private ProductDetailView detail;
        [SerializeField] private CategoryButtonView[] categories;
        [SerializeField] private QuantitySelectorView quantity;
        [SerializeField] private ConfirmDialogView dialog;
        [SerializeField] private TMP_Text balance, capacity, productCount, purchaseLabel, previewStatus;
        [SerializeField] private Image capacityFill;
        [SerializeField] private Button purchase, recharge, inventoryTab, close;
        private readonly List<ProductCardView> cards = new List<ProductCardView>();
        public event Action<ShopCategory> CategoryRequested;
        public event Action<string> ProductRequested;
        public event Action<int> QuantityChanged;
        public event Action PurchaseRequested, RechargeRequested, InventoryRequested, CloseRequested;
        public QuantitySelectorView Quantity => quantity;
        public ConfirmDialogView Dialog => dialog;
        public ProductDetailView Detail => detail;
        public int VisibleProductCount => cards.Count;

        public void Configure(RectTransform grid, ProductCardView prefab, ProductDetailView details,
            CategoryButtonView[] categoryViews, QuantitySelectorView selector, ConfirmDialogView dialogs,
            TMP_Text balanceText, TMP_Text capacityText, TMP_Text countText, TMP_Text buyText, TMP_Text status,
            Image fill, Button buy, Button topUp, Button bag, Button exit)
        {
            content = grid; cardPrefab = prefab; detail = details; categories = categoryViews; quantity = selector; dialog = dialogs;
            balance = balanceText; capacity = capacityText; productCount = countText; purchaseLabel = buyText; previewStatus = status;
            capacityFill = fill; purchase = buy; recharge = topUp; inventoryTab = bag; close = exit;
        }
        private void Awake()
        {
            cards.AddRange(content.GetComponentsInChildren<ProductCardView>(true));
            foreach (var category in categories) category.Clicked += OnCategory;
            quantity.Changed += OnQuantity;
            purchase.onClick.AddListener(OnPurchase); recharge.onClick.AddListener(OnRecharge);
            inventoryTab.onClick.AddListener(OnInventory); close.onClick.AddListener(OnClose);
        }
        private void OnDestroy()
        {
            foreach (var category in categories) if (category != null) category.Clicked -= OnCategory;
            if (quantity != null) quantity.Changed -= OnQuantity;
            if (purchase != null) purchase.onClick.RemoveListener(OnPurchase);
            if (recharge != null) recharge.onClick.RemoveListener(OnRecharge);
            if (inventoryTab != null) inventoryTab.onClick.RemoveListener(OnInventory);
            if (close != null) close.onClick.RemoveListener(OnClose);
        }
        private void OnCategory(ShopCategory value) { CategoryRequested?.Invoke(value); }
        private void OnQuantity(int value) { QuantityChanged?.Invoke(value); }
        private void OnPurchase() { PurchaseRequested?.Invoke(); }
        private void OnRecharge() { RechargeRequested?.Invoke(); }
        private void OnInventory() { InventoryRequested?.Invoke(); }
        private void OnClose() { CloseRequested?.Invoke(); }
        private void OnProduct(string id) { ProductRequested?.Invoke(id); }

        public void RenderProducts(IReadOnlyList<ShopProduct> products, ShopCategory selected)
        {
            foreach (var card in cards)
            { card.Clicked -= OnProduct; card.gameObject.SetActive(false); Destroy(card.gameObject); }
            cards.Clear();
            foreach (var product in products)
            {
                var card = Instantiate(cardPrefab, content, false);
                card.name = "Product_" + product.productId; card.gameObject.SetActive(true);
                card.Bind(product); card.Clicked += OnProduct; cards.Add(card);
            }
            foreach (var category in categories) category.SetSelected(category.Category == selected);
            productCount.text = products.Count + " 件商品  ·  默认排序";
            var scroll = content.GetComponentInParent<ScrollRect>();
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
        public void RenderSelection(string id)
        { foreach (var card in cards) card.SetSelected(card.name == "Product_" + id); }
        public void RenderPreviewStatus(ShopCatalog data)
        {
            balance.text = data.PreviewBalance.ToString("N0");
            capacity.text = data.PreviewUsedSlots + " / " + data.PreviewCapacity;
            capacityFill.fillAmount = (float)data.PreviewUsedSlots / data.PreviewCapacity;
            previewStatus.text = "UI PREVIEW  ·  余额 / 持有量 / 容量均为预览数据";
        }
        public void RenderTotal(long total, bool enabled)
        { purchase.interactable = enabled; purchaseLabel.text = enabled ? "购买  /  " + total.ToString("N0") + " 金币" : "请选择商品"; quantity.SetInteractable(enabled); }
    }
}
