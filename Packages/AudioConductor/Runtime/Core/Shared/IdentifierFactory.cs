// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Runtime.Core.Shared
{
    internal static class IdentifierFactory
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
