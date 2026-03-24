// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Core;

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class StubDspClock : IDspClock
    {
        public double DspTime { get; set; }
    }
}
