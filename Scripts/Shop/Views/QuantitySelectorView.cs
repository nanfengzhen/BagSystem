using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BagSystem.Shop
{
    public sealed class QuantitySelectorView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField input;
        [SerializeField] private Button minus, plus;
        private int minimum = 1, maximum = 99, value = 1;
        public int Value => value;
        public event Action<int> Changed;
        private void Awake()
        {
            minus.onClick.AddListener(Decrease); plus.onClick.AddListener(Increase);
            input.onEndEdit.AddListener(CommitInput);
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.characterLimit = 6;
        }
        private void OnDestroy()
        {
            if (minus != null) minus.onClick.RemoveListener(Decrease);
            if (plus != null) plus.onClick.RemoveListener(Increase);
            if (input != null) input.onEndEdit.RemoveListener(CommitInput);
        }
        private void Decrease() { SetQuantity(value - 1, true); }
        private void Increase() { SetQuantity(value + 1, true); }
        private void CommitInput(string text)
        {
            int parsed;
            SetQuantity(int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : minimum, true);
        }
        public void SetRange(int min, int max)
        { minimum = min; maximum = max; SetQuantity(value); }
        public void SetQuantity(int quantity, bool notify = false)
        {
            value = Mathf.Clamp(quantity, minimum, maximum);
            input.SetTextWithoutNotify(value.ToString());
            minus.interactable = value > minimum;
            plus.interactable = value < maximum;
            if (notify) Changed?.Invoke(value);
        }
        public void SetInteractable(bool enabled)
        { input.interactable = enabled; minus.interactable = enabled && value > minimum; plus.interactable = enabled && value < maximum; }
    }
}
