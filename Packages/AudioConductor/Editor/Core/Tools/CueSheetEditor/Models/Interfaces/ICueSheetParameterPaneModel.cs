// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Enums;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ICueSheetParameterPaneModel
    {
        string Name { get; set; }

        IReadOnlyObservableProperty<string> NameObservable { get; }

        ThrottleType ThrottleType { get; set; }

        IReadOnlyObservableProperty<ThrottleType> ThrottleTypeObservable { get; }

        int ThrottleLimit { get; set; }

        IReadOnlyObservableProperty<int> ThrottleLimitObservable { get; }

        float Volume { get; set; }

        IReadOnlyObservableProperty<float> VolumeObservable { get; }

        float Pitch { get; set; }

        IReadOnlyObservableProperty<float> PitchObservable { get; }

        bool PitchInvert { get; set; }

        IReadOnlyObservableProperty<bool> PitchInvertObservable { get; }
    }
}
