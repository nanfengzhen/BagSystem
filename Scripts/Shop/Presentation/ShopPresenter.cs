using System;
using System.Collections.Generic;

namespace BagSystem.Shop
{
    public sealed class ShopPresenter : IDisposable
    {
        private readonly ShopCatalog catalog;
        private readonly ShopView view;
        private ShopProduct selected;
        private int quantity = 1;

        public long TotalPrice => selected == null ? 0 : (long)selected.unitPrice * quantity;
        public event Action<string, int> PurchaseConfirmed;

        public ShopPresenter(ShopCatalog data, ShopView screen) 
        {
            catalog = data; 
            view = screen; 
        }
        public void Initialize()
        {
            catalog.Validate();
            view.CategoryRequested += SelectCategory; 
            view.ProductRequested += SelectProduct;     //具体点击商品后的操作
            view.QuantityChanged += ChangeQuantity; 
            view.PurchaseRequested += RequestPurchase;
            view.RechargeRequested += ShowRecharge; 
            view.InventoryRequested += ShowInventory; 
            view.CloseRequested += ShowClose;
            view.Quantity.SetRange(1, catalog.MaxQuantity);
            view.RenderPreviewStatus(catalog); 
            SelectCategory(ShopCategory.All);
        }
        public void SelectCategory(ShopCategory category)
        {
            var list = new List<ShopProduct>();
            foreach (var p in catalog.Products) if (category == ShopCategory.All || p.category == category) list.Add(p);
            view.RenderProducts(list, category);
            if (list.Count > 0) SelectProduct(list[0].productId);
            else { selected = null; view.Detail.Clear(); view.RenderTotal(0, false); }
        }

        //选择商品后的的处理函数
        public void SelectProduct(string id)
        {
            selected = null;
            foreach (var product in catalog.Products) if (product.productId == id) { selected = product; break; }
            if (selected == null) throw new ArgumentException("Unknown productId: " + id);
            quantity = 1; view.Quantity.SetQuantity(quantity);
            view.RenderSelection(id); 
            view.Detail.Render(selected); 
            view.RenderTotal(TotalPrice, true);
        }
        public void ChangeQuantity(int value)
        { quantity = Math.Max(1, Math.Min(catalog.MaxQuantity, value)); view.RenderTotal(TotalPrice, selected != null); }
        public void RequestPurchase()
        {
            if (selected == null || view.Dialog.IsOpen) return;
            var product = selected; var count = quantity; var total = TotalPrice;
            view.Dialog.ShowPurchase(product, count, total, () => {
                PurchaseConfirmed?.Invoke(product.productId, count);
                view.Dialog.ShowMessage("购买请求预览", "已确认：" + product.displayName + " × " + count
                    + "\n合计 " + total.ToString("N0") + " 金币\n\n后续可在此接入购买业务。\n本次未扣款，也未增加库存。");
            });
        }
        private void ShowRecharge() { view.Dialog.ShowMessage("模拟充值入口", "充值界面尚未接入。\n当前余额仅用于商城界面预览。"); }
        private void ShowInventory() { view.Dialog.ShowMessage("背包入口", "背包界面将在下一阶段接入。\n当前仅实现商城界面与购买确认交互。"); }
        private void ShowClose() { view.Dialog.ShowMessage("关闭入口", "商城已预留关闭事件。\n接入主界面后，可在此切回上一页面。"); }
        public void Dispose()
        {
            view.CategoryRequested -= SelectCategory; 
            view.ProductRequested -= SelectProduct;
            view.QuantityChanged -= ChangeQuantity; 
            view.PurchaseRequested -= RequestPurchase;
            view.RechargeRequested -= ShowRecharge; 
            view.InventoryRequested -= ShowInventory; 
            view.CloseRequested -= ShowClose;
            view.Dialog.Hide();
        }
    }
}
