using System;
using System.Collections.Generic;
using UnityEngine;

namespace BagSystem.Shop
{
    public enum ShopCategory { All, Consumable, Equipment, Material }

    [Serializable]
    public sealed class ShopProduct
    {
        public string productId;
        public string displayName;
        public Sprite icon;
        [TextArea(2, 5)] public string description;
        public string typeLabel;
        public ShopCategory category;
        [Min(1)] public int unitPrice = 10;
        [Min(0)] public int previewOwned;
        public Color typeColor = new Color(0.68f, 0.75f, 0.84f);
    }

    [CreateAssetMenu(menuName = "BagSystem/Shop Catalog", fileName = "ShopCatalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [SerializeField] private List<ShopProduct> products = new List<ShopProduct>();
        [SerializeField, Min(1)] private int maxQuantity = 99;
        [SerializeField, Min(0)] private int previewBalance = 1280;
        [SerializeField, Min(0)] private int previewUsedSlots = 18;
        [SerializeField, Min(1)] private int previewCapacity = 40;
        public IReadOnlyList<ShopProduct> Products => products;
        public int MaxQuantity => maxQuantity;
        public int PreviewBalance => previewBalance;
        public int PreviewUsedSlots => previewUsedSlots;
        public int PreviewCapacity => previewCapacity;

        public void SetProducts(List<ShopProduct> values) { products = values; }
        public void Validate()
        {
            if (maxQuantity < 1 || previewCapacity < 1 || previewUsedSlots > previewCapacity)
                throw new InvalidOperationException("商城预览参数无效：请检查数量上限和背包容量。");
            var ids = new HashSet<string>();
            foreach (var p in products)
                if (p == null || string.IsNullOrWhiteSpace(p.productId) || !ids.Add(p.productId)
                    || p.icon == null || p.unitPrice < 1)
                    throw new InvalidOperationException("商城商品配置有空ID、重复ID、缺失图标或无效价格。");
        }
    }
}
