// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Runtime.CompilerServices;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("AudioConductor.Tests")]
[assembly: InternalsVisibleTo("AudioConductor.PlayModeTests")]
[assembly: InternalsVisibleTo("AudioConductor.Editor")]
[assembly: InternalsVisibleTo("AudioConductor.Editor.Tests")]
#endif
