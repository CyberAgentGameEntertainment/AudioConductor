// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class StubDspClock : IDspClock
    {
        public double DspTime { get; set; }
    }
}
