// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
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

        // Builds WAV with a SMPL chunk inserted before DATA
        private static MemoryStream BuildWavWithSmplChunk(uint sampleLoops, bool sizeMismatch = false)
        {
            const uint fmtSize = 16u;
            var smplHeaderSize = 4u * 9; // 36 bytes: 9 fixed fields
            var loopsSize = sampleLoops * 4u * 6; // 24 bytes per loop
            var actualSmplSize = smplHeaderSize + loopsSize;
            // sizeMismatch: claim header-only size but write loop data too
            var claimedSmplSize = sizeMismatch ? smplHeaderSize : actualSmplSize;
            var dataPayload = new byte[8];
            var dataSize = (uint)dataPayload.Length;
            var formChunkSize = 4u + 8 + fmtSize + 8 + actualSmplSize + 8 + dataSize;

            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });

            // FMT chunk
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)(44100 * 2 * 2));
            w.Write((ushort)4);
            w.Write((ushort)16);

            // SMPL chunk
            w.Write(new[] { 's', 'm', 'p', 'l' });
            w.Write(claimedSmplSize);
            w.Write(0u); // manufacturer
            w.Write(0u); // product
            w.Write(0u); // samplePeriod
            w.Write(60u); // midiUnityNote
            w.Write(0u); // midiPitchFraction
            w.Write(0u); // smpteFormat
            w.Write(0u); // smpteOffset
            w.Write(sampleLoops);
            w.Write(0u); // samplerData
            for (var i = 0u; i < sampleLoops; i++)
            {
                w.Write(i); // identifier
                w.Write(0u); // type
                w.Write(100u * (i + 1)); // start
                w.Write(200u * (i + 1)); // end
                w.Write(0u); // fraction
                w.Write(0u); // playCount
            }

            // DATA chunk
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);

            ms.Position = 0;
            return ms;
        }

        // Builds WAV with a CUE chunk inserted before DATA
        private static MemoryStream BuildWavWithCueChunk(uint numCuePoints)
        {
            const uint fmtSize = 16u;
            var cueSize = 4u + numCuePoints * 24u; // 4 for numCuePoints + 24 per cue point
            var dataPayload = new byte[8];
            var dataSize = (uint)dataPayload.Length;
            var formChunkSize = 4u + 8 + fmtSize + 8 + cueSize + 8 + dataSize;

            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });

            // FMT chunk
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)(44100 * 2 * 2));
            w.Write((ushort)4);
            w.Write((ushort)16);

            // CUE chunk
            w.Write(new[] { 'c', 'u', 'e', ' ' });
            w.Write(cueSize);
            w.Write(numCuePoints);
            for (var i = 0u; i < numCuePoints; i++)
            {
                w.Write(i); // cueNumber
                w.Write(i * 1000u); // position
                w.Write(new[] { 'd', 'a', 't', 'a' }); // chunk
                w.Write(0u); // chunkStart
                w.Write(0u); // blockStart
                w.Write(i * 500u); // sampleOffset
            }

            // DATA chunk
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);

            ms.Position = 0;
            return ms;
        }

        // Builds WAV with an arbitrary extra chunk inserted before DATA
        private static MemoryStream BuildWavWithExtraChunk(string chunkId, byte[] payload)
        {
            const uint fmtSize = 16u;
            var extraSize = (uint)payload.Length;
            var dataPayload = new byte[8];
            var dataSize = (uint)dataPayload.Length;
            var formChunkSize = 4u + 8 + fmtSize + 8 + extraSize + 8 + dataSize;

            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });

            // FMT chunk
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)(44100 * 2 * 2));
            w.Write((ushort)4);
            w.Write((ushort)16);

            // extra chunk
            foreach (var c in chunkId.ToCharArray())
                w.Write(c);
            w.Write(extraSize);
            w.Write(payload);

            // DATA chunk
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);

            ms.Position = 0;
            return ms;
        }

        [Test]
        public void Execute_WavWithSmplChunk_SingleLoop_ReadsLoopInfo()
        {
            using var ms = BuildWavWithSmplChunk(1);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.LoopInfoList, Has.Count.EqualTo(1));
            Assert.That(reader.LoopInfoList[0].start, Is.EqualTo(100u));
            Assert.That(reader.LoopInfoList[0].end, Is.EqualTo(200u));
        }

        [Test]
        public void Execute_WavWithSmplChunk_MultipleLoops_ReadsAllLoopInfos()
        {
            using var ms = BuildWavWithSmplChunk(2);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.LoopInfoList, Has.Count.EqualTo(2));
        }

        [Test]
        public void Execute_WavWithSmplChunk_SizeMismatch_ThrowsApplicationException()
        {
            using var ms = BuildWavWithSmplChunk(1, true);

            var reader = new WaveChunkReader();
            Assert.Throws<ApplicationException>(() => reader.Execute(ms));
        }

        [Test]
        public void Execute_WavWithCueChunk_SingleCuePoint_ReadsCueChunkList()
        {
            using var ms = BuildWavWithCueChunk(1);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.CueChunkList, Has.Count.EqualTo(1));
            Assert.That(reader.LoopInfoList, Is.Empty);
        }

        [Test]
        public void Execute_WavWithCueChunk_TwoCuePoints_CreatesLoopInfo()
        {
            using var ms = BuildWavWithCueChunk(2);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.CueChunkList, Has.Count.EqualTo(2));
            Assert.That(reader.LoopInfoList, Has.Count.EqualTo(1));
        }

        [Test]
        public void Execute_WavWithCueChunk_ThreeCuePoints_NoLoopInfo()
        {
            using var ms = BuildWavWithCueChunk(3);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.CueChunkList, Has.Count.EqualTo(3));
            Assert.That(reader.LoopInfoList, Is.Empty);
        }

        [Test]
        public void Execute_WavWithInstChunk_SkipsCorrectly()
        {
            using var ms = BuildWavWithExtraChunk("INST", new byte[7]);

            var reader = new WaveChunkReader();
            Assert.DoesNotThrow(() => reader.Execute(ms));
            Assert.That(reader.Channels, Is.EqualTo(2));
        }

        [Test]
        public void Execute_WavWithResUChunk_SkipsCorrectly()
        {
            using var ms = BuildWavWithExtraChunk("RESU", new byte[8]);

            var reader = new WaveChunkReader();
            Assert.DoesNotThrow(() => reader.Execute(ms));
            Assert.That(reader.Channels, Is.EqualTo(2));
        }

        [Test]
        public void Execute_WavWithUnknownChunk_SkipsCorrectly()
        {
            using var ms = BuildWavWithExtraChunk("ABCD", new byte[4]);

            var reader = new WaveChunkReader();
            Assert.DoesNotThrow(() => reader.Execute(ms));
            Assert.That(reader.Channels, Is.EqualTo(2));
        }

        [Test]
        public void Execute_FmtChunkWithExtraParams_ReadsCorrectly()
        {
            // fmtSize=18 → 2 extra bytes after the standard 16
            const uint fmtSize = 18u;
            var dataPayload = new byte[8];
            var dataSize = (uint)dataPayload.Length;
            var formChunkSize = 4u + 8 + fmtSize + 8 + dataSize;

            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);

            w.Write(new[] { 'R', 'I', 'F', 'F' });
            w.Write(formChunkSize);
            w.Write(new[] { 'W', 'A', 'V', 'E' });
            w.Write(new[] { 'f', 'm', 't', ' ' });
            w.Write(fmtSize);
            w.Write((ushort)1);
            w.Write((ushort)2);
            w.Write((uint)44100);
            w.Write((uint)(44100 * 2 * 2));
            w.Write((ushort)4);
            w.Write((ushort)16);
            w.Write((ushort)0); // 2 extra bytes
            w.Write(new[] { 'd', 'a', 't', 'a' });
            w.Write(dataSize);
            w.Write(dataPayload);
            ms.Position = 0;

            var reader = new WaveChunkReader();
            Assert.DoesNotThrow(() => reader.Execute(ms));
            Assert.That(reader.Channels, Is.EqualTo(2));
            Assert.That(reader.SamplingRate, Is.EqualTo(44100));
        }

        [Test]
        public void HasLoop_WithLoopInfo_ReturnsTrue()
        {
            using var ms = BuildWavWithSmplChunk(1);

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.HasLoop(), Is.True);
        }

        [Test]
        public void HasLoop_WithoutLoopInfo_ReturnsFalse()
        {
            using var ms = BuildWav();

            var reader = new WaveChunkReader();
            reader.Execute(ms);

            Assert.That(reader.HasLoop(), Is.False);
        }
    }
}
