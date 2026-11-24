using UnityEngine;

namespace BagSystem.Shop
{
    public sealed class ShopBootstrap : MonoBehaviour
    {
        [SerializeField] private ShopCatalog catalog;
        [SerializeField] private ShopView view;
        private ShopPresenter presenter;
        public ShopPresenter Presenter => presenter;
        public ShopView View => view;
        public void Configure(ShopCatalog data, ShopView screen) { catalog = data; view = screen; }
        private void Start()
        {
            if (catalog == null || view == null) throw new System.InvalidOperationException("ShopBootstrap缺少Catalog或View引用。");
            presenter = new ShopPresenter(catalog, view); presenter.Initialize();
        }
        private void OnDestroy() { if (presenter != null) presenter.Dispose(); }
    }
}
