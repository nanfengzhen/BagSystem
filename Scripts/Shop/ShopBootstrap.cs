using UnityEngine;

namespace BagSystem.Shop
{

    //完成控制层的配置
    public sealed class ShopBootstrap : MonoBehaviour
    {
        [SerializeField] private ShopCatalog catalog;   //模型层
        [SerializeField] private ShopView view;     //视图层
        private ShopPresenter presenter;    //控制层

        private void Start()
        {
            if (catalog == null || view == null) throw new System.InvalidOperationException("ShopBootstrap缺少Catalog或View引用。");
            presenter = new ShopPresenter(catalog, view); 
            presenter.Initialize();
        }
        private void OnDestroy() 
        {
            if (presenter != null) presenter.Dispose(); 
        }
    }
}
