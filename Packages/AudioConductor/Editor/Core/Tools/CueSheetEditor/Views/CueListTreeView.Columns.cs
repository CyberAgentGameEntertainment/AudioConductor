// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

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
                    column.headerContent = new GUIContent("Name");
                    break;
                case CueListTreeView.ColumnType.Color:
                    column.headerContent = new GUIContent("Color");
                    break;
                case CueListTreeView.ColumnType.Category:
                    column.headerContent = new GUIContent("Category");
                    break;
                case CueListTreeView.ColumnType.ThrottleType:
                    column.headerContent = new GUIContent("Throttle Type");
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                case CueListTreeView.ColumnType.ThrottleLimit:
                    column.headerContent = new GUIContent("Throttle Limit");
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                case CueListTreeView.ColumnType.Volume:
                    column.headerContent = new GUIContent("Volume");
                    column.minWidth = 150;
                    column.width = 150;
                    column.maxWidth = 150;
                    break;
                case CueListTreeView.ColumnType.VolumeRange:
                    column.headerContent = new GUIContent("Volume Range");
                    column.width = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                case CueListTreeView.ColumnType.PlayType:
                    column.headerContent = new GUIContent("Play Type");
                    column.minWidth = column.minWidth;
                    column.maxWidth = column.minWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }

            return column;
        }
    }
}
