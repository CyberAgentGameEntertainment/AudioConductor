// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class ColorSelectPopupWindowContent : PopupWindowContent
    {
        private readonly Rect _activatorRect;
        private readonly GUIStyle _boldStyle;
        private readonly Action<int> _onSelected;
        private readonly int _selectedIndex;

        private readonly GUIStyle _style;
        private Vector2 _elementSize;

        private int _itemCount;

        private Vector2 _scrollPosition;
        private Vector2 _windowSizeCache;

        public ColorSelectPopupWindowContent(Rect activatorRect, int selectedIndex, Action<int> onSelected)
        {
            _activatorRect = activatorRect;
            _selectedIndex = selectedIndex;
            _onSelected = onSelected;

            var isDarkTheme = EditorGUIUtility.isProSkin;

            // Picked up colors from EditorGUI.Popup.
            var hoverBackground = isDarkTheme
                ? TextureGenerator.CreateTexture(2, 2, new Color32(33, 81, 201, 255))
                : TextureGenerator.CreateTexture(2, 2, new Color32(42, 98, 216, 255));

            var normalTextColor = isDarkTheme
                ? new Color32(235, 235, 235, 255)
                : new Color32(35, 35, 35, 255);

            var hoverTextColor = isDarkTheme
                ? new Color32(231, 238, 250, 255)
                : new Color32(255, 255, 255, 255);

            _style = new GUIStyle(GUI.skin.FindStyle("WhiteLabel"))
            {
                normal =
                {
                    textColor = normalTextColor
                },
                hover =
                {
                    textColor = hoverTextColor,
                    background = hoverBackground
                }
            };
            _boldStyle = new GUIStyle(GUI.skin.FindStyle("WhiteBoldLabel"))
            {
                normal =
                {
                    textColor = normalTextColor
                },
                hover =
                {
                    textColor = hoverTextColor,
                    background = hoverBackground
                }
            };
        }

        public override void OnOpen()
        {
            base.OnOpen();
            EditorApplication.update += Update;
        }

        public override void OnClose()
        {
            EditorApplication.update -= Update;
            base.OnClose();
        }

        public override Vector2 GetWindowSize()
        {
            var colorDefineContents = ColorDefineListRepository.instance.ColorDefineContents;
            if (colorDefineContents == null)
                return Vector2.zero;

            if (_itemCount == colorDefineContents.Length)
                return _windowSizeCache;

            _itemCount = colorDefineContents.Length;

            var maxSize = Vector2.zero;
            foreach (var content in colorDefineContents)
            {
                var size = GUI.skin.label.CalcSize(content);
                if (maxSize.x < size.x)
                    maxSize.x = size.x;
                if (maxSize.y < size.y)
                    maxSize.y = size.y;
            }

            _elementSize.y = Mathf.Ceil(maxSize.y + GUI.skin.label.margin.top);
            _elementSize.x = maxSize.x + 16;

            var width = Mathf.Max(maxSize.x, _activatorRect.width);
            var height = Mathf.Clamp(colorDefineContents.Length * (_elementSize.y + GUI.skin.label.margin.bottom), 50,
                                     300);
            const float paddingHeight = 10;
            const float paddingWidth = 10;
            _windowSizeCache = new Vector2(width + paddingWidth, height + paddingHeight);
            return _windowSizeCache;
        }

        public override void OnGUI(Rect rect)
        {
            var colorDefineContents = ColorDefineListRepository.instance.ColorDefineContents;
            if (colorDefineContents == null || colorDefineContents.Length == 0)
                return;

            var areaRect = new Rect(rect.xMin + 3, rect.yMin + 3, rect.width - 6, rect.height - 6);
            using var areaScope = new GUILayout.AreaScope(areaRect);
            GUILayoutUtility.GetRect(areaRect.width, 1);

            using var scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition, false, false);
            _scrollPosition = scrollViewScope.scrollPosition;
            var yPositionDrawRange = new Vector2(_scrollPosition.y - 21, _scrollPosition.y + areaRect.height);

            for (var i = 0; i < colorDefineContents.Length; i++)
            {
                var content = colorDefineContents[i];
                var elementRect =
                    EditorGUILayout.GetControlRect(GUILayout.Width(_elementSize.x), GUILayout.Height(_elementSize.y));
                if (elementRect.height > 1)
                {
                    if (elementRect.y < yPositionDrawRange.x || elementRect.y > yPositionDrawRange.y)
                        continue;
                }
                else
                    continue;

                GUI.SetNextControlName(content.text);
                var style = _selectedIndex == i ? _boldStyle : _style;
                if (GUI.Button(elementRect, content, style))
                {
                    editorWindow.Close();
                    _onSelected?.Invoke(i);
                }
            }
        }

        private void Update()
        {
            editorWindow.Repaint();
        }
    }
}
