// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.WaveChunkReader.Tests
{
    internal class WaveChunkReaderTests
    {
        // Builds a minimal valid WAV: RIFF header + FMT chunk + DATA chunk
        private static MemoryStream BuildWav(short channels = 2, int samplingRate = 44100, short bitsPerSample = 16,
            byte[]? dataPayload = null)
        {
            dataPayload ??= new byte[channels * (bitsPerSample / 8) * 4]; // 4 sample frames
            var fmtSize = 16u;
            var dataSize = (uint)dataPayload.Length;
            // 4 (WAVE) + 8 (FMT id+size) + fmtSize + 8 (DATA id+size) + dataSize
            var formChunkSize = 4 + 8 + fmtSize + 8 + dataSize;

            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);

            // RIFF header
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });

            // FMT chunk
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1); // PCM
            w.Write((ushort)channels);
            w.Write((uint)samplingRate);
            w.Write((uint)(samplingRate * channels * bitsPerSample / 8)); // avgBytesPerSec
            w.Write((ushort)(channels * bitsPerSample / 8)); // blockAlign
            w.Write((ushort)bitsPerSample);

            // DATA chunk
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);

            ms.Position = 0;
            return ms;
        }

        // Builds WAV with an odd-size chunk inserted before DATA
        private static MemoryStream BuildWavWithOddChunk(string chunkId, uint oddPayloadSize)
        {
            var paddedSize = oddPayloadSize + (oddPayloadSize & 1);
            var fmtSize = 16u;
            var dataPayload = new byte[8]; // minimal data
            var dataSize = (uint)dataPayload.Length;
            // 4 + 8+fmtSize + 8+oddPayloadSize + padding + 8+dataSize
            var formChunkSize = 4u + 8 + fmtSize + 8 + oddPayloadSize + (oddPayloadSize & 1) + 8 + dataSize;

            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);

            // RIFF header
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });

            // FMT chunk
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2); // channels
            w.Write((uint)44100);
            w.Write((uint)(44100 * 2 * 2));
            w.Write((ushort)4);
            w.Write((ushort)16);

            // odd-size chunk (e.g. JUNK)
            foreach (var c in chunkId.ToCharArray()) w.Write(c);
            w.Write(oddPayloadSize);
            w.Write(new byte[paddedSize]); // write payload + padding

            // DATA chunk
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);

            ms.Position = 0;
            return ms;
        }

        [Test]
        public void Execute_ValidWav_ReadsCorrectly()
        {
            var channels = (short)2;
            var samplingRate = 44100;
            var bitsPerSample = (short)16;
            var dataPayload = new byte[channels * (bitsPerSample / 8) * 100]; // 100 frames
            using var ms = BuildWav(channels, samplingRate, bitsPerSample, dataPayload);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.Channels, Is.EqualTo(channels));
            Assert.That(reader.SamplingRate, Is.EqualTo(samplingRate));
            Assert.That(reader.SamplingBit, Is.EqualTo(bitsPerSample));
            Assert.That(reader.SampleFrames, Is.EqualTo(100));
        }

        [Test]
        public void Execute_InvalidRiffHeader_ThrowsWaveParseException()
        {
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            w.Write(new[] { 'X', 'X', 'X', 'X' }); // invalid RIFF
            w.Write(36u);
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.Throws<WaveParseException>(() => reader.Execute(ms));
        }

        [Test]
        public void Execute_InvalidWaveType_ThrowsWaveParseException()
        {
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(36u);
            w.Write(new[] { 'A', 'I', 'F', 'F' }); // not WAVE
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.Throws<WaveParseException>(() => reader.Execute(ms));
        }

        [Test]
        public void Execute_FmtChunkSizeTooSmall_ThrowsWaveParseException()
        {
            // Build WAV with FMT chunk that has chunkSize=0
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            var fmtSize = 0u;
            var dataSize = 8u;
            var formChunkSize = 4u + 8 + fmtSize + 8 + dataSize;

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize); // chunkSize=0
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(new byte[dataSize]);
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.Throws<WaveParseException>(() => reader.Execute(ms));
        }

        [Test]
        public void Execute_DataBeforeFmt_ZeroDivision_ThrowsWaveParseException()
        {
            // DATA chunk appears before FMT — SamplingBit=0, Channels=0 → divide by zero guard
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            var dataPayload = new byte[8];
            var dataSize = (uint)dataPayload.Length;
            var fmtSize = 16u;
            var formChunkSize = 4u + 8 + dataSize + 8 + fmtSize;

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });

            // DATA first
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);

            // FMT after
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)176400);
            w.Write((ushort)4);
            w.Write((ushort)16);
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.Throws<WaveParseException>(() => reader.Execute(ms));
        }

        [Test]
        public void Execute_OddSizeJunkChunk_PaddingHandledCorrectly()
        {
            // JUNK chunk with odd size=3 → padded to 4; reader should reach DATA and parse correctly
            using var ms = BuildWavWithOddChunk("JUNK", 3);

            var reader = new WaveChunkReader();
            Assert.DoesNotThrow(() => reader.Execute(ms));
            Assert.That(reader.Channels, Is.EqualTo(2));
        }

        [Test]
        public void Execute_FormChunkSizeLargerThanStream_ThrowsWaveParseException()
        {
            // formChunkSize claims more bytes than stream contains
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            var fmtSize = 16u;
            var dataPayload = new byte[8];
            var dataSize = (uint)dataPayload.Length;
            var actualFormChunkSize = 4u + 8 + fmtSize + 8 + dataSize;

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(actualFormChunkSize + 1000u); // claim more than actual
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)176400);
            w.Write((ushort)4);
            w.Write((ushort)16);
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.Throws<WaveParseException>(() => reader.Execute(ms));
        }

        [Test]
        public void Execute_ChunkSizeUintMaxValue_ThrowsWaveParseException()
        {
            // chunkSize = uint.MaxValue → uint overflow guard
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            var fmtSize = 16u;
            var formChunkSize = 4u + 8 + fmtSize + 8 + uint.MaxValue; // wraps to small value

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            // Use a plausible formChunkSize that won't cause RIFF header rejection
            var realFormChunkSize = 4u + 8 + fmtSize + 8 + 8u;
            w.Write(realFormChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)176400);
            w.Write((ushort)4);
            w.Write((ushort)16);
            // chunk with uint.MaxValue size
            w.Write(new[] { 'J', 'U', 'N', 'K' });
            w.Write(uint.MaxValue);
            w.Write(new byte[8]);
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.Throws<WaveParseException>(() => reader.Execute(ms));
        }
    }
}
