// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class SliderAndIntegerField : VisualElement, INotifyValueChanged<int>
    {
        private readonly IntegerField _integer = new();
        private readonly SliderInt _slider = new();

        private int _value;

        public SliderAndIntegerField()
        {
            style.flexDirection = FlexDirection.Row;

            hierarchy.Add(_slider);
            hierarchy.Add(_integer);

            _slider.RegisterValueChangedCallback(OnValueChanged);
            _integer.RegisterValueChangedCallback(OnValueChanged);

            // Slider default margins
            style.marginLeft = 3;
            style.marginRight = 3;
            style.marginTop = 1;
            style.marginBottom = 1;

            _slider.style.flexGrow = 1;

            // Exclude overlap margins
            _slider.style.marginLeft = 0;
            _slider.style.marginTop = 0;
            _slider.style.marginBottom = 0;
            _integer.style.marginRight = 0;
            _integer.style.marginTop = 0;
            _integer.style.marginBottom = 0;
        }

        public string label
        {
            get => _slider.label;
            set => _slider.label = value;
        }

        public int lowValue
        {
            get => _slider.lowValue;
            set => _slider.lowValue = value;
        }

        public int highValue
        {
            get => _slider.highValue;
            set => _slider.highValue = value;
        }

        public bool showMixedValue
        {
            get => _slider.showMixedValue;
            set => _slider.showMixedValue = _integer.showMixedValue = value;
        }

        public int value
        {
            get => _value;
            set
            {
                if (this.value == value)
                    return;

                value = Mathf.Clamp(value, lowValue, highValue);

                if (panel == null)
                {
                    SetValueWithoutNotify(value);
                    return;
                }

                using var pooled = ChangeEvent<int>.GetPooled(this.value, value);
                pooled.target = this;
                SetValueWithoutNotify(value);
                SendEvent(pooled);
            }
        }

        public void SetValueWithoutNotify(int newValue)
        {
            _value = newValue;
            _slider.SetValueWithoutNotify(newValue);
            _integer.SetValueWithoutNotify(newValue);
        }

        public void SetFieldWidth(int width)
        {
            _integer.style.width = width;
        }

        public void SetIsDelayed(bool isDelayed)
        {
            _integer.isDelayed = isDelayed;
        }

        private void OnValueChanged(ChangeEvent<int> evt)
        {
            evt.StopPropagation();
            value = evt.newValue;
        }

        public new class UxmlFactory : UxmlFactory<SliderAndIntegerField, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new(){ name = "label", defaultValue = "Label Name"};
            private readonly UxmlIntAttributeDescription _value = new(){ name = "value", defaultValue = 0 };
            private readonly UxmlIntAttributeDescription _lowValue = new(){ name = "low-value", defaultValue = 0 };
            private readonly UxmlIntAttributeDescription _highValue = new(){ name = "high-value", defaultValue = 100 };
            private readonly UxmlIntAttributeDescription _fieldWidth = new() { name = "field-width", defaultValue = 70 };
            private readonly UxmlBoolAttributeDescription _isDelayed = new() { name = "is-delayed", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = (SliderAndIntegerField)ve;
                container.label = _label.GetValueFromBag(bag, cc);
                container.lowValue = _lowValue.GetValueFromBag(bag, cc);
                container.highValue = _highValue.GetValueFromBag(bag, cc);
                container.value = _value.GetValueFromBag(bag, cc);
                container.SetFieldWidth(_fieldWidth.GetValueFromBag(bag, cc));
                container.SetIsDelayed(_isDelayed.GetValueFromBag(bag, cc));
            }
        }
    }
}
