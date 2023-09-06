// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Enums;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ICueInspectorModel
    {
        bool CanPreview { get; }

        string Name { get; set; }

        IReadOnlyObservableProperty<MixedValue<string>> NameObservable { get; }

        string Color { get; set; }

        IReadOnlyObservableProperty<MixedValue<string>> ColorObservable { get; }

        int CategoryId { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> CategoryIdObservable { get; }

        ThrottleType ThrottleType { get; set; }

        IReadOnlyObservableProperty<MixedValue<ThrottleType>> ThrottleTypeObservable { get; }

        int ThrottleLimit { get; set; }

        IReadOnlyObservableProperty<MixedValue<int>> ThrottleLimitObservable { get; }

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

        CuePlayType PlayType { get; set; }

        IReadOnlyObservableProperty<MixedValue<CuePlayType>> PlayTypeObservable { get; }

        ICueController PlayCue();

        void StopCue();
    }
}
