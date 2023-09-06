// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Runtime.CompilerServices;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Shared
{
    /// <summary>
    ///     Value range definition.
    /// </summary>
    public static class ValueRangeConst
    {
        /// <summary>
        ///     Limit of concurrent play.
        /// </summary>
        public static class ThrottleLimit
        {
            public const int Min = 0;
            public const int Max = 999;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Clamp(int value)
                => Mathf.Clamp(value, Min, Max);
        }

        /// <summary>
        ///     Volume of the audio.
        /// </summary>
        public static class Volume
        {
            public const float Min = 0;
            public const float Max = 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Clamp(float value)
                => Mathf.Clamp(value, Min, Max);
        }

        /// <summary>
        ///     Random range of the volume.
        /// </summary>
        public static class VolumeRange
        {
            public const float Min = 0;
            public const float Max = 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Clamp(float value)
                => Mathf.Clamp(value, Min, Max);
        }

        /// <summary>
        ///     Pitch of the audio.
        /// </summary>
        public static class Pitch
        {
            public const float Min = 0.01f;
            public const float Max = 3;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Clamp(float value)
                => Mathf.Clamp(value, Min, Max);
        }

        /// <summary>
        ///     Random range of the pitch.
        /// </summary>
        public static class PitchRange
        {
            public const float Min = 0;
            public const float Max = 1;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Clamp(float value)
                => Mathf.Clamp(value, Min, Max);
        }

        /// <summary>
        ///     Play start position.
        /// </summary>
        public static class StartSample
        {
            public const int Min = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Clamp(int value, int audioClipSamples)
                => Mathf.Clamp(value, Min, Mathf.Max(audioClipSamples - 1, Min));
        }

        /// <summary>
        ///     Play end position.
        /// </summary>
        public static class EndSample
        {
            public const int Min = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Clamp(int value, int audioClipSamples)
                => Mathf.Clamp(value, Min, Mathf.Max(audioClipSamples, Min));
        }

        /// <summary>
        ///     Loop start position.
        /// </summary>
        public static class LoopStartSample
        {
            public const int Min = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Clamp(int value, int audioClipSamples)
                => Mathf.Clamp(value, Min, Mathf.Max(audioClipSamples - 1, Min));
        }

        /// <summary>
        ///     Weight for random play of cue.
        /// </summary>
        public static class RandomWeight
        {
            public const int Min = 0;
            public const int Max = int.MaxValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Clamp(int value)
                => Mathf.Clamp(value, Min, Max);
        }

        /// <summary>
        ///     Fade-in/fade-out time.
        /// </summary>
        public static class FadeTime
        {
            public const float Min = 0;
            public const float Max = float.MaxValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float Clamp(float value)
                => Mathf.Clamp(value, Min, Max);
        }
    }
}
