using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BagSystem.Shop
{
    public sealed class ProductDetailView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title, type, description, unitPrice, owned;
        public void Configure(Image i, TMP_Text n, TMP_Text t, TMP_Text d, TMP_Text p, TMP_Text o)
        { icon = i; title = n; type = t; description = d; unitPrice = p; owned = o; }
        public void Render(ShopProduct p)
        {
            icon.enabled = true; icon.sprite = p.icon;
            title.text = p.displayName; type.text = p.typeLabel;
            description.text = p.description;
            unitPrice.text = "单价  " + p.unitPrice.ToString("N0") + " 金币";
            owned.text = "已拥有  " + p.previewOwned;
        }
        public void Clear()
        {
            icon.enabled = false; title.text = "请选择商品";
            type.text = description.text = unitPrice.text = owned.text = string.Empty;
        }
    }
}
