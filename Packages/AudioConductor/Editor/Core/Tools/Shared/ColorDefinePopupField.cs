// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class ColorDefinePopupField : BindableElement, INotifyValueChanged<string>
    {
        private readonly IMGUIContainer _imguiContainer = new();
        private readonly Label _label = new();

        private string _value;

        public ColorDefinePopupField()
        {
            style.flexDirection = FlexDirection.Row;

            hierarchy.Add(_label);
            hierarchy.Add(_imguiContainer);

            _label.style.minWidth = 150;

            // DropdownField default margins
            style.marginLeft = 3;
            style.marginRight = 3;
            style.marginTop = 1;
            style.marginBottom = 1;
            _label.style.marginLeft = 0;
            _label.style.marginRight = 2;
            _label.style.marginTop = 0;
            _label.style.marginBottom = 0;
            _label.style.paddingLeft = 1;
            _label.style.paddingRight = 2;
            _label.style.paddingTop = 2;
            _label.style.paddingBottom = 0;
            _imguiContainer.style.flexGrow = 1;

            _imguiContainer.onGUIHandler = OnGUI;
        }

        public string label
        {
            get => _label.text;
            set => _label.text = value;
        }

        public bool showMixedValue { get; set; }

        public string value
        {
            get => _value;
            set
            {
                if (_value == value)
                    return;

                if (panel == null)
                {
                    SetValueWithoutNotify(value);
                    return;
                }

                using var pooled = ChangeEvent<string>.GetPooled(this.value, value);
                pooled.target = this;
                SetValueWithoutNotify(value);
                SendEvent(pooled);
            }
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _value = newValue;
        }

        private void OnGUI()
        {
            value = AudioConductorGUI.ColorDefine.Popup(_imguiContainer.contentRect, _value, showMixedValue);
        }

        public new class UxmlFactory : UxmlFactory<ColorDefinePopupField, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _label = new()
                { name = "label", defaultValue = "Label Name" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var container = (ColorDefinePopupField)ve;
                container.label = _label.GetValueFromBag(bag, cc);
            }
        }
    }
}
