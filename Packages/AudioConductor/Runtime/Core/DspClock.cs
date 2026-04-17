// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Runtime.CompilerServices;
using UnityEngine;

namespace AudioConductor.Core
{
    internal sealed class DspClock : IDspClock
    {
        public double DspTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => AudioSettings.dspTime;
        }
    }
}
