// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

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

        bool CodeGenEnabled { get; set; }

        IReadOnlyObservableProperty<bool> CodeGenEnabledObservable { get; }

        string CodeGenOutputPath { get; set; }

        IReadOnlyObservableProperty<string> CodeGenOutputPathObservable { get; }

        string CodeGenNamespace { get; set; }

        IReadOnlyObservableProperty<string> CodeGenNamespaceObservable { get; }

        string CodeGenClassSuffix { get; set; }

        IReadOnlyObservableProperty<string> CodeGenClassSuffixObservable { get; }
    }
}
