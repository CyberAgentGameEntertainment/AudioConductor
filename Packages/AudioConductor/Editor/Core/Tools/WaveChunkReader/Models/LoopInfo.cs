// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Editor.Core.Tools.WaveChunkReader.Models
{
    public readonly struct LoopInfo
    {
        public readonly uint identifier;
        public readonly uint type;
        public readonly uint start;
        public readonly uint end;
        public readonly uint fraction;
        public readonly uint playCount; // If 0, Infinite

        public LoopInfo(uint identifier, uint type, uint start, uint end, uint fraction, uint playCount)
        {
            this.identifier = identifier;
            this.type = type;
            this.start = start;
            this.end = end;
            this.fraction = fraction;
            this.playCount = playCount;
        }

        public LoopInfo(uint start, uint end)
        {
            identifier = default;
            type = default;
            this.start = start;
            this.end = end;
            fraction = default;
            playCount = default;
        }
    }
}
