// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.ObjectModel;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed partial class CueListTreeView
    {
        public enum ColumnType
        {
            Name,
            Color,
            Category,
            ThrottleType,
            ThrottleLimit,
            Volume,
            VolumeRange,
            PlayType
        }

        public static readonly ReadOnlyCollection<int> VolumeColumnGroup = Array.AsReadOnly(new[]
        {
            (int)ColumnType.Volume,
            (int)ColumnType.VolumeRange
        });

        public static readonly ReadOnlyCollection<int> PlayInfoColumnGroup = Array.AsReadOnly(new[]
        {
            (int)ColumnType.Category,
            (int)ColumnType.PlayType
        });

        public static readonly ReadOnlyCollection<int> ThrottleColumnGroup = Array.AsReadOnly(new[]
        {
            (int)ColumnType.ThrottleType,
            (int)ColumnType.ThrottleLimit
        });

        public static readonly ReadOnlyCollection<int> MemoColumnGroup = Array.AsReadOnly(new[]
        {
            (int)ColumnType.Color
        });
    }

    internal static class ColumnTypeExtensions
    {
        public static GUIContent CreateHeaderContent(this CueListTreeView.ColumnType columnType)
        {
            return columnType switch
            {
                CueListTreeView.ColumnType.Name => new GUIContent("Name",
                    Localization.Localization.Tr("cue_list.column_name")),
                CueListTreeView.ColumnType.Color => new GUIContent("Color",
                    Localization.Localization.Tr("cue_list.column_color")),
                CueListTreeView.ColumnType.Category => new GUIContent("Category",
                    Localization.Localization.Tr("cue_list.column_category")),
                CueListTreeView.ColumnType.ThrottleType => new GUIContent("Throttle Type",
                    Localization.Localization.Tr("cue_list.column_throttle_type")),
                CueListTreeView.ColumnType.ThrottleLimit => new GUIContent("Throttle Limit",
                    Localization.Localization.Tr("cue_list.column_throttle_limit")),
                CueListTreeView.ColumnType.Volume => new GUIContent("Volume",
                    Localization.Localization.Tr("cue_list.column_volume")),
                CueListTreeView.ColumnType.VolumeRange => new GUIContent("Volume Range",
                    Localization.Localization.Tr("cue_list.column_volume_range")),
                CueListTreeView.ColumnType.PlayType => new GUIContent("Play Type",
                    Localization.Localization.Tr("cue_list.column_play_type")),
                _ => throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null)
            };
        }

        public static MultiColumnHeaderState.Column CreateColumn(this CueListTreeView.ColumnType columnType)
        {
            var column = new MultiColumnHeaderState.Column
            {
                headerTextAlignment = TextAlignment.Center,
                allowToggleVisibility = false,
                minWidth = 100,
                canSort = false,
                autoResize = true,
                userData = (int)columnType
            };

            switch (columnType)
            {
                case CueListTreeView.ColumnType.Name:
                    column.headerContent = columnType.CreateHeaderContent();
                    break;
                case CueListTreeView.ColumnType.Color:
                    column.headerContent = columnType.CreateHeaderContent();
                    break;
                case CueListTreeView.ColumnType.Category:
                    column.headerContent = columnType.CreateHeaderContent();
                    break;
                case CueListTreeView.ColumnType.ThrottleType:
                    column.headerContent = columnType.CreateHeaderContent();
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                case CueListTreeView.ColumnType.ThrottleLimit:
                    column.headerContent = columnType.CreateHeaderContent();
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                case CueListTreeView.ColumnType.Volume:
                    column.headerContent = columnType.CreateHeaderContent();
                    column.minWidth = 150;
                    column.width = 150;
                    column.maxWidth = 150;
                    break;
                case CueListTreeView.ColumnType.VolumeRange:
                    column.headerContent = columnType.CreateHeaderContent();
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                case CueListTreeView.ColumnType.PlayType:
                    column.headerContent = columnType.CreateHeaderContent();
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }

            return column;
        }
    }
}
