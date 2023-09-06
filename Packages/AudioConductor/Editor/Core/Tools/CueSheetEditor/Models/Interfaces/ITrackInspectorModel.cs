// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ITrackInspectorModel
    {
        bool CanPreview { get; }

        string Name { get; set; }

        IReadOnlyObservableProperty<MixedValue<string>> NameObservable { get; }

        string Color { get; set; }

        IReadOnlyObservableProperty<MixedValue<string>> ColorObservable { get; }

        AudioClip AudioClip { get; set; }

        IReadOnlyObservableProperty<MixedValue<AudioClip>> AudioClipObservable { get; }

        float Volume { get; set; }

        IReadOnlyObservableProperty<MixedValue<float>> VolumeObservable { get; }

        float VolumeRange { get; set; }

        IReadOnlyObservableProperty<MixedValue<float>> VolumeRangeObservable { get; }

        float Pitch { get; set; }

        IReadOnlyObservableProperty<MixedValue<float>> PitchObservable { get; }

        float PitchRange { get; set; }

        IReadOnlyObservableProperty<MixedValue<float>> PitchRangeObservable { get; }

        bool PitchInvert { get; set; }

        IReadOnlyObservableProperty<MixedValue<bool>> PitchInvertObservable { get; }

        int StartSample { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> StartSampleObservable { get; }

        int EndSample { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> EndSampleObservable { get; }

        int LoopStartSample { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> LoopStartSampleObservable { get; }

        bool IsLoop { get; set; }

        IReadOnlyObservableProperty<MixedValue<bool>> IsLoopObservable { get; }

        int RandomWeight { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> RandomWeightObservable { get; }

        int Priority { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> PriorityObservable { get; }

        float FadeTime { get; set; }

        IReadOnlyObservableProperty<MixedValue<float>> FadeTimeObservable { get; }

        void AnalyzeWaveChunk();

        TrackPreviewController PlayClip(int? sample);
    }
}
