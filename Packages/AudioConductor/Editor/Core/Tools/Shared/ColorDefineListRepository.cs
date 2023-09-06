// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Editor.Core.Models;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class ColorDefineListRepository : ScriptableSingleton<ColorDefineListRepository>
    {
        private static readonly ColorDefine None = new()
        {
            id = string.Empty,
            name = "( None )",
            color = new Color(0, 0, 0, 0)
        };

        private ColorDefine[] _colorDefines;
        public GUIContent[] ColorDefineContents { get; private set; }

        public void Update()
        {
            var settings = AudioConductorEditorSettingsRepository.instance.Settings;
            var enumerable = settings == null ? Enumerable.Empty<ColorDefine>() : settings.colorDefineList;

            _colorDefines = enumerable.Prepend(None).ToArray();
            var contents = new GUIContent[_colorDefines.Length];
            for (var i = 0; i < _colorDefines.Length; i++)
            {
                var colorDefine = _colorDefines[i];
                var content = new GUIContent(colorDefine.name,
                                             TextureGenerator.CreateTexture(16, 16, colorDefine.color));
                contents[i] = content;
            }

            ColorDefineContents = contents;
        }

        public string ToColorId(int index)
        {
            if (index < 0 || _colorDefines == null || _colorDefines.Length <= index)
                return null;

            return _colorDefines[index].id;
        }

        public int ToIndex(string colorId)
        {
            if (_colorDefines == null)
                return 0;

            for (var i = 0; i < _colorDefines.Length; i++)
                if (colorId == _colorDefines[i].id)
                    return i;

            return 0;
        }

        public string GetName(string colorId)
            => _colorDefines?.FirstOrDefault(colorDefine => colorDefine.id == colorId)?.name ?? string.Empty;
    }
}
