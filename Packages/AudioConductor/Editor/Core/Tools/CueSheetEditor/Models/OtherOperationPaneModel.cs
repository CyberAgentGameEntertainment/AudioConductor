// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class OtherOperationPaneModel : IOtherOperationPaneModel
    {
        private static readonly object[] CueSheetParameters =
        {
            "CueSheetName",
            "ThrottleLimit",
            "ThrottleType",
            "Volume",
            "Pitch",
            "PitchInvert"
        };

        private static readonly object[] CueParameters =
        {
            "CueName",
            "ColorId",
            "CategoryId",
            "ThrottleLimit",
            "ThrottleType",
            "Volume",
            "VolumeRange",
            "Pitch",
            "PitchRange",
            "PitchInvert",
            "PlayType",
            "TrackCount"
        };

        private static readonly object[] TrackParameters =
        {
            "TrackName",
            "ColorId",
            "ClipName",
            "Volume",
            "VolumeRange",
            "Pitch",
            "PitchRange",
            "PitchInvert",
            "StartSample",
            "EndSample",
            "LoopStartSample",
            "IsLoop",
            "RandomWeight",
            "Priority",
            "FadeTime"
        };

        private static readonly object[] CueListParameters = CueParameters.Concat(TrackParameters).ToArray();

        private readonly CueSheet _cueSheet;
        private readonly AutoIncrementHistory _history;
        private readonly IAssetSaveService _assetSaveService;

        public OtherOperationPaneModel([NotNull] CueSheet cueSheet,
                                       AutoIncrementHistory history,
                                       IAssetSaveService assetSaveService)
        {
            _cueSheet = cueSheet;
            _history = history;
            _assetSaveService = assetSaveService;
        }

        public string CueSheetName => _cueSheet.name;

        public string CreateCsvText()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItemsLine(builder, CueListParameters.Length, nameof(CueSheetParameters));
            CsvUtility.AppendRowItemsLine(builder, CueListParameters.Length, CueSheetParameters);
            CsvUtility.AppendRowItemsLine(builder, CueListParameters.Length,
                                          _cueSheet.name,
                                          _cueSheet.throttleLimit,
                                          _cueSheet.throttleType,
                                          _cueSheet.volume,
                                          _cueSheet.pitch,
                                          _cueSheet.pitchInvert
                                         );

            builder.AppendLine();

            CsvUtility.AppendRowItemsLine(builder, CueListParameters.Length, nameof(CueListParameters));
            CsvUtility.AppendRowItemsLine(builder, CueListParameters.Length, CueListParameters);
            foreach (var cue in _cueSheet.cueList)
            {
                CsvUtility.AppendRowItems(builder,
                                          cue.name,
                                          cue.colorId,
                                          cue.categoryId,
                                          cue.throttleLimit,
                                          cue.throttleType,
                                          cue.volume,
                                          cue.volumeRange,
                                          cue.pitch,
                                          cue.pitchRange,
                                          cue.pitchInvert,
                                          cue.playType,
                                          cue.trackList.Count
                                         );

                var preDelimiterNum = CueListParameters.Length - TrackParameters.Length;
                var count = 0;
                foreach (var track in cue.trackList)
                {
                    var delimiterNum = preDelimiterNum;
                    if (count == 0)
                        delimiterNum = 1;
                    CsvUtility.AppendDelimiter(builder, delimiterNum);

                    count++;

                    var clipName = track.audioClip == null ? string.Empty : track.audioClip.name;

                    CsvUtility.AppendRowItems(builder,
                                              track.name,
                                              track.colorId,
                                              clipName,
                                              track.volume,
                                              track.volumeRange,
                                              track.pitch,
                                              track.pitchRange,
                                              track.pitchInvert,
                                              track.startSample,
                                              track.endSample,
                                              track.loopStartSample,
                                              track.isLoop,
                                              track.randomWeight,
                                              track.priority,
                                              track.fadeTime);
                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }

        public bool ImportCsv(IReadOnlyList<string> lines)
        {
            var cueSheet = ParseCsv(lines);
            if (cueSheet == null)
                return false;

            var name = _cueSheet.name;
            var throttleType = _cueSheet.throttleType;
            var throttleLimit = _cueSheet.throttleLimit;
            var volume = _cueSheet.volume;
            var pitch = _cueSheet.pitch;
            var pitchInvert = _cueSheet.pitchInvert;
            var cueList = _cueSheet.cueList;
            _history.Register($"Import {Time.frameCount}", Redo, Undo);

            return true;

            #region LocalMethods

            void Redo()
            {
                _cueSheet.name = cueSheet.name;
                _cueSheet.throttleType = cueSheet.throttleType;
                _cueSheet.throttleLimit = cueSheet.throttleLimit;
                _cueSheet.volume = cueSheet.volume;
                _cueSheet.pitch = cueSheet.pitch;
                _cueSheet.pitchInvert = cueSheet.pitchInvert;
                _cueSheet.cueList = cueSheet.cueList;
                _assetSaveService.Save();
            }

            void Undo()
            {
                _cueSheet.name = name;
                _cueSheet.throttleType = throttleType;
                _cueSheet.throttleLimit = throttleLimit;
                _cueSheet.volume = volume;
                _cueSheet.pitch = pitch;
                _cueSheet.pitchInvert = pitchInvert;
                _cueSheet.cueList = cueList;
                _assetSaveService.Save();
            }

            #endregion
        }

        private static CueSheet ParseCsv(IReadOnlyList<string> lines)
        {
            var cueSheet = new CueSheet();

            try
            {
                // Skip header
                var row = 2;

                // Parameters
                row = ParseCueSheetParameters(cueSheet, lines, row);

                // Skip header
                row += 3;

                ParseCueList(cueSheet, lines, row);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }

            return cueSheet;
        }

        private static int ParseCueSheetParameters(CueSheet cueSheet, IReadOnlyList<string> lines, int currentRow)
        {
            var parameters = lines[currentRow++].Split(',');
            var index = 0;
            cueSheet.name = parameters[index++];
            cueSheet.throttleLimit = ValueRangeConst.ThrottleLimit.Clamp(Convert.ToInt32(parameters[index++]));
            cueSheet.throttleType = (ThrottleType)Enum.Parse(typeof(ThrottleType), parameters[index++]);
            cueSheet.volume = ValueRangeConst.Volume.Clamp(Convert.ToSingle(parameters[index++]));
            cueSheet.pitch = ValueRangeConst.Pitch.Clamp(Convert.ToSingle(parameters[index++]));
            cueSheet.pitchInvert = Convert.ToBoolean(parameters[index]);

            return currentRow;
        }

        private static void ParseCueList(CueSheet cueSheet, IReadOnlyList<string> lines, int currentRow)
        {
            var preDelimiterNum = CueListParameters.Length - TrackParameters.Length;
            for (var row = currentRow; row < lines.Count; ++row)
            {
                if (string.IsNullOrEmpty(lines[row]))
                    continue;

                var cueStrings = lines[row].Split(',');
                var columnIndex = 0;

                var cue = new Cue
                {
                    name = cueStrings[columnIndex++],
                    colorId = cueStrings[columnIndex++],
                    categoryId = Convert.ToInt32(cueStrings[columnIndex++]),
                    throttleLimit = ValueRangeConst.ThrottleLimit.Clamp(Convert.ToInt32(cueStrings[columnIndex++])),
                    throttleType = (ThrottleType)Enum.Parse(typeof(ThrottleType), cueStrings[columnIndex++]),
                    volume = ValueRangeConst.Volume.Clamp(Convert.ToSingle(cueStrings[columnIndex++])),
                    volumeRange = ValueRangeConst.VolumeRange.Clamp(Convert.ToSingle(cueStrings[columnIndex++])),
                    pitch = ValueRangeConst.Pitch.Clamp(Convert.ToSingle(cueStrings[columnIndex++])),
                    pitchRange = ValueRangeConst.PitchRange.Clamp(Convert.ToSingle(cueStrings[columnIndex++])),
                    pitchInvert = Convert.ToBoolean(cueStrings[columnIndex++]),
                    playType = (CuePlayType)Enum.Parse(typeof(CuePlayType), cueStrings[columnIndex++])
                };

                var trackCount = Convert.ToInt32(cueStrings[columnIndex]);

                for (var t = 0; t < trackCount; ++t)
                    cue.trackList.Add(ParseTrack(preDelimiterNum, lines[row + t].Split(',')));

                cueSheet.cueList.Add(cue);
                row += trackCount - 1;
            }
        }

        private static Track ParseTrack(int index, IReadOnlyList<string> columns)
        {
            var track = new Track
            {
                name = columns[index++],
                colorId = columns[index++]
            };

            var clipName = columns[index++];

            if (string.IsNullOrEmpty(clipName) == false)
            {
                var clipGUIDs = AssetDatabase.FindAssets(clipName + " t:AudioClip");
                var clipPath = clipGUIDs.Select(AssetDatabase.GUIDToAssetPath)
                                        .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == clipName);

                if (string.IsNullOrEmpty(clipPath))
                    Debug.LogWarning($"AudioClip not found : {clipName}");
                else
                    track.audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            }

            var samples = track.audioClip == null ? 0 : track.audioClip.samples;

            track.volume = ValueRangeConst.Volume.Clamp(Convert.ToSingle(columns[index++]));
            track.volumeRange = ValueRangeConst.VolumeRange.Clamp(Convert.ToSingle(columns[index++]));
            track.pitch = ValueRangeConst.Pitch.Clamp(Convert.ToSingle(columns[index++]));
            track.pitchRange = ValueRangeConst.PitchRange.Clamp(Convert.ToSingle(columns[index++]));
            track.pitchInvert = Convert.ToBoolean(columns[index++]);
            track.startSample = Mathf.Clamp(Convert.ToInt32(columns[index++]), 0, samples);
            track.endSample = Mathf.Clamp(Convert.ToInt32(columns[index++]), 0, samples);
            track.loopStartSample = Mathf.Clamp(Convert.ToInt32(columns[index++]), 0, samples);
            track.isLoop = Convert.ToBoolean(columns[index++]);
            track.randomWeight = ValueRangeConst.RandomWeight.Clamp(Convert.ToInt32(columns[index++]));
            track.priority = Convert.ToInt32(columns[index++]);
            track.fadeTime = ValueRangeConst.FadeTime.Clamp(Convert.ToSingle(columns[index]));

            return track;
        }
    }
}
