// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed partial class TrackInspectorView
    {
        private DragObject _dragObject;

        private bool _previewAreaEnabled = true;

        private AudioClipGUI.PreviewCache _previewCache;

        internal void SetPreviewAreaEnabled(bool enabled)
        {
            _previewAreaEnabled = enabled;
        }

        private void DrawPreviewArea()
        {
            if (_previewAreaEnabled == false)
                return;

            var clip = _audioClipField.value as AudioClip;
            if (clip == null)
                return;

            using var scope = new EditorGUILayout.VerticalScope();

            var audioRect = GetControlRect(200, 15);
            var markerRect = GetControlRect(15, 15);
            if (audioRect.width < 0)
                return;

            EditorGUI.DrawRect(audioRect, Const.DimColor);

            if (_previewCache == null || _previewCache.Clip != clip || _previewCache.Rect != audioRect)
            {
                _previewCache?.Dispose();
                _previewCache = AudioClipGUI.DoRenderPreview(clip, audioRect);
            }
            else
                AudioClipGUI.DoRenderPreview(_previewCache);

            DrawIndicator(audioRect);
            DrawMinMax(audioRect);
            DrawMarker(audioRect, markerRect);
        }

        private static Rect GetControlRect(float height, float paddingX)
        {
            var controlRect = EditorGUILayout.GetControlRect(false, height);
            var padding = new Vector2(paddingX, 0);
            return new Rect(controlRect.position + padding, controlRect.size - padding * 2);
        }

        private void DrawMinMax(Rect rect)
        {
            var isLoop = _isLoopField.value;
            var startSample = _startSampleField.value;
            var loopStartSample = _loopStartSampleField.value;
            var endSample = _endSampleField.value;

            var sec2px = rect.width / GetClipSamples();
            var min = (isLoop ? loopStartSample : startSample) * sec2px;
            var max = endSample * sec2px;

            var beforeMinRect = new Rect(rect.x, rect.y, min, rect.height);
            EditorGUI.DrawRect(beforeMinRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));

            var afterMaxRect = new Rect(rect.x + max + 1, rect.y, rect.width - (max + 1), rect.height);
            EditorGUI.DrawRect(afterMaxRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));

            var minRect = new Rect(rect.x + min, rect.y + 1, 1, rect.height - 2);
            EditorGUI.DrawRect(minRect, Color.white);

            var maxRect = new Rect(rect.x + max, rect.y + 1, 1, rect.height - 2);
            EditorGUI.DrawRect(maxRect, Color.white);
        }

        private void DrawMarker(Rect audioRect, Rect markerRect)
        {
            var isLoop = _isLoopField.value;
            var startSample = _startSampleField.value;
            var loopStartSample = _loopStartSampleField.value;
            var endSample = _endSampleField.value;
            var clipSamples = GetClipSamples();

            var sec2px = markerRect.width / clipSamples;
            var min = (isLoop ? loopStartSample : startSample) * sec2px;
            var max = endSample * sec2px;
            var minRect = new Rect(markerRect.x + min + 1 - 15, markerRect.y, 15, 15);
            var maxRect = new Rect(markerRect.x + max, markerRect.y, 15, 15);

            HandleMakerEvent(Event.current);

            EditorGUI.DrawRect(minRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.DrawTexture(minRect, Const.NextKeyIcon.image);

            EditorGUI.DrawRect(maxRect, new Color(0.15f, 0.15f, 0.15f));
            GUI.DrawTexture(maxRect, Const.PrevKeyIcon.image);

            #region LocalMethods

            void HandleMakerEvent(Event e)
            {
                if (e.type is EventType.Repaint or EventType.Layout or EventType.Used)
                    return;

                if (e.type is not (EventType.MouseDrag or EventType.MouseDown or EventType.MouseUp))
                    return;

                if (e.type is EventType.MouseUp && _dragObject != DragObject.None)
                {
                    _dragObject = DragObject.None;
                    e.Use();
                    return;
                }

                if (e.type is EventType.MouseDown)
                {
                    if (minRect.Contains(e.mousePosition))
                        _dragObject = DragObject.Start;
                    if (maxRect.Contains(e.mousePosition))
                        _dragObject = DragObject.End;
                    if (audioRect.Contains(e.mousePosition))
                        _dragObject = DragObject.Current;
                }

                var sample = (int)((e.mousePosition.x - markerRect.x) * (GetClipSamples() / markerRect.width));
                sample = Mathf.Clamp(sample, 0, clipSamples);
                switch (_dragObject)
                {
                    case DragObject.None:
                        break;
                    case DragObject.Start:
                        if (isLoop)
                            _loopStartSampleChangedSubject.OnNext(sample);
                        else
                            _startSampleChangedSubject.OnNext(sample);
                        break;
                    case DragObject.End:
                        _endSampleChangedSubject.OnNext(sample);
                        break;
                    case DragObject.Current:
                        if (_previewController == null || _previewController.IsPlaying == false)
                            _playRequestedSubject.OnNext(sample);
                        else
                            _previewController.SetCurrentSample(sample);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                e.Use();
            }

            #endregion
        }

        private void DrawIndicator(Rect rect)
        {
            if (_previewController == null)
                return;

            var sample = _previewController.GetCurrentSample();

            var rate = Mathf.InverseLerp(0, GetClipSamples(), sample);
            var time = Mathf.Lerp(0f, GetClipLength(), rate);

            var sec2px = rect.width / GetClipSamples();
            var ts = new TimeSpan(0, 0, 0, 0, (int)(time * 1000.0f));

            EditorGUI.DrawRect(new Rect(rect.x + sample * sec2px, rect.y, 1, rect.height), Color.green);

            EditorGUI.DropShadowLabel(
                                      new Rect(rect.x, rect.y, rect.width, 20),
                                      $"{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}"
                                     );
        }

        private int GetClipSamples()
        {
            var obj = _audioClipField.value;
            if (obj == null)
                return 0;

            var clip = (AudioClip)obj;
            return clip.samples;
        }

        private float GetClipLength()
        {
            var obj = _audioClipField.value;
            if (obj == null)
                return 0;

            var clip = (AudioClip)obj;
            return clip.length;
        }

        private static class Const
        {
            public static readonly Color DimColor = new(0.15f, 0.15f, 0.15f, 1);

            public static readonly GUIContent NextKeyIcon =
                EditorGUIUtility.TrIconContent("Animation.NextKey", "StartTime");

            public static readonly GUIContent PrevKeyIcon =
                EditorGUIUtility.TrIconContent("Animation.PrevKey", "EndTime");
        }

        private enum DragObject
        {
            None,
            Start,
            End,
            Current
        }
    }
}
