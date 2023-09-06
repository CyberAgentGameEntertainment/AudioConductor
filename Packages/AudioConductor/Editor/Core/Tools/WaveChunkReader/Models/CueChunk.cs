// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Editor.Core.Tools.WaveChunkReader.Models
{
    public readonly struct CueChunk
    {
        public readonly uint cueNumber;
        public readonly uint position;
        public readonly string chunk;
        public readonly uint chunkStart;
        public readonly uint blockStart;
        public readonly uint sampleOffset;

        public CueChunk(uint cueNumber,
                        uint position,
                        char[] chunk,
                        uint chunkStart,
                        uint blockStart,
                        uint sampleOffset)
        {
            this.cueNumber = cueNumber;
            this.position = position;
            this.chunk = new string(chunk);
            this.chunkStart = chunkStart;
            this.blockStart = blockStart;
            this.sampleOffset = sampleOffset;
        }
    }
}
