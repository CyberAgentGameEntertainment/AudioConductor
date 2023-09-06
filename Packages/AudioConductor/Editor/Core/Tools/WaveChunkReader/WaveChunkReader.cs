// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AudioConductor.Editor.Core.Tools.WaveChunkReader.Models;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.WaveChunkReader
{
    /// <summary>
    ///     Reader for wav file chunk.
    /// </summary>
    public class WaveChunkReader
    {
        private readonly List<ChunkSummary> _chunkInfoList = new();
        private readonly List<CueChunk> _cueChunkList = new();
        private readonly List<LoopInfo> _loopInfoList = new();

        /// <summary>
        ///     Read Size.
        /// </summary>
        public uint ReadSize { get; private set; }

        /// <summary>
        ///     Number of Channels.
        /// </summary>
        public short Channels { get; private set; }

        /// <summary>
        ///     Sampling Rate (Hz).
        /// </summary>
        public int SamplingRate { get; private set; }

        /// <summary>
        ///     Sampling Bit.
        /// </summary>
        public int SamplingBit { get; private set; }

        /// <summary>
        ///     Number of Samples.
        /// </summary>
        public long Samples { get; private set; }

        /// <summary>
        ///     Number of Sample Frames.
        /// </summary>
        public long SampleFrames { get; private set; }

        /// <summary>
        ///     Summary of chunks.
        /// </summary>
        public IReadOnlyList<ChunkSummary> ChunkInfoList => _chunkInfoList;

        /// <summary>
        ///     Data chunk start position.
        /// </summary>
        public int DataChunkStartPosition { get; private set; }

        /// <summary>
        ///     Data chunk end position.
        /// </summary>
        public int DataChunkEndPosition { get; private set; }

        /// <summary>
        ///     Data chunk size.
        /// </summary>
        public int DataChunkSize { get; private set; }

        /// <summary>
        ///     Loop information.
        /// </summary>
        public IReadOnlyList<LoopInfo> LoopInfoList => _loopInfoList;

        /// <summary>
        ///     Cue chunks.
        /// </summary>
        public IReadOnlyList<CueChunk> CueChunkList => _cueChunkList;

        /// <summary>
        ///     Return true if the file has a loop information.
        /// </summary>
        /// <returns></returns>
        public bool HasLoop() => LoopInfoList?.Count != 0;

        /// <summary>
        ///     Read the wav file chunk.
        /// </summary>
        /// <param name="audioClip"></param>
        /// <exception cref="ArgumentNullException">If AudioClip is null</exception>
        /// <exception cref="NotSupportedException">If file isn't a wav format.</exception>
        public void Execute(AudioClip audioClip)
        {
            if (audioClip == null)
                throw new ArgumentNullException(nameof(audioClip));

            var audioClipPath = AssetDatabase.GetAssetPath(audioClip);
            Execute(audioClipPath);
        }

        /// <summary>
        ///     Read the wav file chunk.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ArgumentNullException">If AudioClip is null</exception>
        /// <exception cref="NotSupportedException">If file isn't a wav format.</exception>
        public void Execute(string path)
        {
            var extension = Path.GetExtension(path).ToLower();

            if (extension != ".wav")
                throw new NotSupportedException("File isn't wav format.");

            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Execute(fileStream);
        }

        /// <summary>
        ///     Read the wave file chunk.
        /// </summary>
        /// <param name="stream"></param>
        /// <exception cref="NotSupportedException">If file isn't a wav format.</exception>
        public void Execute(Stream stream)
        {
            using (var binary = new BinaryReader(stream, new UTF8Encoding(), true))
            {
                ReadChunkIdAndSize(binary, out var chunkId, out var chunkSize);
                var formType = binary.ReadChars(4);

                if (chunkId != "RIFF" || string.Join("", formType) != "WAVE")
                    throw new NotSupportedException();

                var formChunkSize = chunkSize;
                ReadSize = 12;
                while (ReadSize < formChunkSize)
                {
                    ReadSize += ReadChunkIdAndSize(binary, out chunkId, out chunkSize);

                    var readSize = chunkId.ToUpper() switch
                    {
                        "FMT " => ReadFormatChunk(binary, chunkSize),
                        "DATA" => ReadDataChunk(binary, chunkSize),
                        "SMPL" => ReadSmplChunk(binary, chunkSize),
                        "CUE " => ReadCueChunk(binary, chunkSize),
                        "JUNK" => ReadJunkChunk(binary, chunkSize),
                        "INST" => ReadInstChunk(binary, chunkSize),
                        "RESU" => ReadResUChunk(binary, chunkSize),
                        _      => ReadUnknownChunk(binary, chunkSize)
                    };

                    _chunkInfoList.Add(new ChunkSummary(chunkId, readSize));
                    ReadSize += readSize;
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
        }

        private static uint ReadChunkIdAndSize(BinaryReader binaryReader, out string chunkId, out uint chunkSize)
        {
            var localChunk = binaryReader.ReadChars(4);
            chunkId = string.Join("", localChunk);
            chunkSize = binaryReader.ReadUInt32();

            return 8;
        }

        private uint ReadFormatChunk(BinaryReader binaryReader, uint chunkSize)
        {
            var formatTag = binaryReader.ReadUInt16();
            var channels = binaryReader.ReadUInt16();
            var samplesPerSec = binaryReader.ReadUInt32();
            var avgBytesPerSec = binaryReader.ReadUInt32();
            var blockAlign = binaryReader.ReadUInt16();
            var bitsPerSample = binaryReader.ReadUInt16();

            const uint readSize = 16; // If LPCM (Linear PCM), it must be 16

            if (chunkSize > readSize)
                // read extra parameter
                binaryReader.ReadBytes((int)(chunkSize - readSize));

            Channels = (short)channels;
            SamplingBit = bitsPerSample;
            SamplingRate = (int)samplesPerSec;

            return chunkSize;
        }

        private uint ReadDataChunk(BinaryReader binaryReader, uint chunkSize)
        {
            var sampleNum = chunkSize / (SamplingBit / 8);
            var numSampleFrames = sampleNum / Channels;

            Samples = sampleNum;
            SampleFrames = numSampleFrames;

            binaryReader.BaseStream.Seek(chunkSize, SeekOrigin.Current);

            DataChunkStartPosition = (int)ReadSize;
            DataChunkSize = (int)chunkSize;
            DataChunkEndPosition = DataChunkStartPosition + DataChunkSize;

            return chunkSize;
        }

        private uint ReadSmplChunk(BinaryReader binaryReader, uint chunkSize)
        {
            var readByte = 0;

            var manufacturer = binaryReader.ReadUInt32();
            var product = binaryReader.ReadUInt32();
            var samplePeriod = binaryReader.ReadUInt32();
            var midiUnityNote = binaryReader.ReadUInt32();
            var midiPitchFraction = binaryReader.ReadUInt32();
            var smpteFormat = binaryReader.ReadUInt32();
            var smpteOffset = binaryReader.ReadUInt32();
            var sampleLoops = binaryReader.ReadUInt32();
            var samplerData = binaryReader.ReadUInt32();

            readByte += 4 * 9;

            for (var i = 0; i < sampleLoops; ++i)
            {
                var identifier = binaryReader.ReadUInt32();
                var type = binaryReader.ReadUInt32();
                var start = binaryReader.ReadUInt32();
                var end = binaryReader.ReadUInt32();
                var fraction = binaryReader.ReadUInt32();
                var playCount = binaryReader.ReadUInt32();

                _loopInfoList.Add(new LoopInfo(identifier, type, start, end, fraction, playCount));

                readByte += 4 * 6;
            }

            if (readByte != chunkSize)
                throw new ApplicationException("SMPL chunk size mismatch.");

            return chunkSize;
        }

        private uint ReadCueChunk(BinaryReader binaryReader, uint chunkSize)
        {
            // References: https://www.audiokinetic.com/ja/library/edge/?source=SDK&id=soundengine_markers.html
            // References: https://hgotoh.jp/wiki/doku.php/documents/tools/tools-101

            var numCuePoints = binaryReader.ReadUInt32();

            var cueList = new List<CueChunk>((int)numCuePoints);

            for (var i = 0; i < numCuePoints; ++i)
            {
                var cueNumber = binaryReader.ReadUInt32();
                var position = binaryReader.ReadUInt32();
                var chunk = binaryReader.ReadChars(4);
                var chunkStart = binaryReader.ReadUInt32();
                var blockStart = binaryReader.ReadUInt32();
                var sampleOffset = binaryReader.ReadUInt32();

                cueList.Add(new CueChunk(cueNumber, position, chunk, chunkStart, blockStart, sampleOffset));
            }

            _cueChunkList.AddRange(cueList);

            // Loop information may be included in the cue chunk.
            if (cueList.Count == 2)
                _loopInfoList.Add(new LoopInfo(cueList[0].sampleOffset, cueList[1].sampleOffset));

            return chunkSize;
        }

        private static uint ReadJunkChunk(BinaryReader binaryReader, uint chunkSize)
        {
            // References: https://www.daubnet.com/en/file-format-riff

            // If the size is odd, a 1-byte padding present.
            chunkSize += chunkSize & 1;

            binaryReader.BaseStream.Seek(chunkSize, SeekOrigin.Current);

            return chunkSize;
        }

        private static uint ReadInstChunk(BinaryReader binaryReader, uint chunkSize)
        {
            // If the size is odd, a 1-byte padding present.
            chunkSize += chunkSize & 1;

            // Unknown details, skip it.
            binaryReader.BaseStream.Seek(chunkSize, SeekOrigin.Current);

            return chunkSize;
        }

        private static uint ReadResUChunk(BinaryReader binaryReader, uint chunkSize)
        {
            // If the size is odd, a 1-byte padding present.
            chunkSize += chunkSize & 1;

            // Unknown details, skip it.
            binaryReader.BaseStream.Seek(chunkSize, SeekOrigin.Current);

            return chunkSize;
        }

        private static uint ReadUnknownChunk(BinaryReader binaryReader, uint chunkSize)
        {
            binaryReader.BaseStream.Seek(chunkSize, SeekOrigin.Current);

            return chunkSize;
        }
    }
}
