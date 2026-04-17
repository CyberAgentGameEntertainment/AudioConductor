// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Core.Tools.WaveChunkReader.Models
{
    public readonly struct ChunkSummary
    {
        public readonly string id;

        public readonly uint size;

        public ChunkSummary(string id, uint size)
        {
            this.id = id;
            this.size = size;
        }
    }
}
