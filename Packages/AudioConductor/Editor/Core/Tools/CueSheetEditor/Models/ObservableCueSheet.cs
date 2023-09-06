// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class ObservableCueSheet
    {
        private readonly CueSheet _cueSheet;

        private readonly ObservableProperty<string> _name;
        private readonly ObservableProperty<ThrottleType> _throttleType;
        private readonly ObservableProperty<int> _throttleLimit;
        private readonly ObservableProperty<float> _volume;
        private readonly ObservableProperty<float> _pitch;
        private readonly ObservableProperty<bool> _pitchInvert;

        public ObservableCueSheet([NotNull] CueSheet cueSheet)
        {
            _cueSheet = cueSheet;

            // ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
            _name = new(_cueSheet.name);
            _throttleType = new(_cueSheet.throttleType);
            _throttleLimit = new(_cueSheet.throttleLimit);
            _volume = new(_cueSheet.volume);
            _pitch = new(_cueSheet.pitch);
            _pitchInvert = new(_cueSheet.pitchInvert);
            // ReSharper enable ArrangeObjectCreationWhenTypeNotEvident
        }

        public string Id => _cueSheet.Id;

        public string Name
        {
            get => _name.Value;
            set => _name.SetValueAndNotify(_cueSheet.name = value);
        }

        public IReadOnlyObservableProperty<string> NameObservable => _name;

        public ThrottleType ThrottleType
        {
            get => _throttleType.Value;
            set => _throttleType.SetValueAndNotify(_cueSheet.throttleType = value);
        }

        public IReadOnlyObservableProperty<ThrottleType> ThrottleTypeObservable => _throttleType;

        public int ThrottleLimit
        {
            get => _throttleLimit.Value;
            set => _throttleLimit.SetValueAndNotify(_cueSheet.throttleLimit = value);
        }

        public IReadOnlyObservableProperty<int> ThrottleLimitObservable => _throttleLimit;

        public float Volume
        {
            get => _volume.Value;
            set => _volume.SetValueAndNotify(_cueSheet.volume = value);
        }

        public IReadOnlyObservableProperty<float> VolumeObservable => _volume;

        public float Pitch
        {
            get => _pitch.Value;
            set => _pitch.SetValueAndNotify(_cueSheet.pitch = value);
        }

        public IReadOnlyObservableProperty<float> PitchObservable => _pitch;

        public bool PitchInvert
        {
            get => _pitchInvert.Value;
            set => _pitchInvert.SetValueAndNotify(_cueSheet.pitchInvert = value);
        }

        public IReadOnlyObservableProperty<bool> PitchInvertObservable => _pitchInvert;
    }
}
