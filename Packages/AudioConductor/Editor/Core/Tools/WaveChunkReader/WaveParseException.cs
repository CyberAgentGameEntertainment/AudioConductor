// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Editor.Core.Tools.WaveChunkReader
{
    /// <summary>
    ///     Exception thrown when a WAV file cannot be parsed due to malformed or unsupported data.
    /// </summary>
    public sealed class WaveParseException : Exception
    {
        public WaveParseException(string message) : base(message)
        {
        }

        public WaveParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
