using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BagSystem.Shop
{
    public sealed class ConfirmDialogView : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private TMP_Text heading, body, confirmLabel, cancelLabel;
        [SerializeField] private Button confirm, cancel, close;
        private Action confirmed;
        public bool IsOpen => overlay.activeSelf;
        public void Configure(GameObject root, TMP_Text h, TMP_Text b, TMP_Text okText, TMP_Text cancelText,
                              Button ok, Button no, Button x)
        { overlay = root; heading = h; body = b; confirmLabel = okText; cancelLabel = cancelText; confirm = ok; cancel = no; close = x; }
        private void Awake()
        { confirm.onClick.AddListener(Confirm); cancel.onClick.AddListener(Hide); close.onClick.AddListener(Hide); }
        private void OnDestroy()
        {
            if (confirm != null) confirm.onClick.RemoveListener(Confirm);
            if (cancel != null) cancel.onClick.RemoveListener(Hide);
            if (close != null) close.onClick.RemoveListener(Hide);
        }
        public void ShowPurchase(ShopProduct product, int quantity, long total, Action onConfirm)
        {
            heading.text = "确认购买";
            body.text = product.displayName + "  ×  " + quantity + "\n\n合计  " + total.ToString("N0") + " 金币\n\n<color=#AEB8C7>当前为商城界面预览，不扣金币、不增加库存。</color>";
            confirmLabel.text = "确认"; cancelLabel.text = "取消";
            cancel.gameObject.SetActive(true); confirmed = onConfirm;
            overlay.SetActive(true);
        }
        public void ShowMessage(string title, string message)
        {
            heading.text = title; body.text = message;
            confirmLabel.text = "知道了"; confirmed = null;
            cancel.gameObject.SetActive(false); overlay.SetActive(true);
        }
        private void Confirm()
        {
            var callback = confirmed;
            Hide(); callback?.Invoke();
        }
        public void Hide() { confirmed = null; overlay.SetActive(false); }
    }
}
