// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;

namespace AudioConductor.Runtime.Core.Shared
{
    internal static class IdentifierFactory
    {
        public static string Create() => Guid.NewGuid().ToString();
    }
}
