// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Threading.Tasks;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Runtime.Core
{
    public sealed partial class Conductor
    {
        /// <summary>
        ///     Registers a loaded <see cref="CueSheetAsset" /> and returns a handle for playback.
        ///     Multiple registrations of the same asset are allowed; each returns a distinct handle.
        /// </summary>
        /// <param name="asset">The CueSheet asset to register.</param>
        /// <returns>A handle that identifies this registration.</returns>
        public CueSheetHandle RegisterCueSheet(CueSheetAsset asset)
        {
            var id = ++_cueSheetHandleCounter;
            _cueSheets[id] = new CueSheetRegistration(asset);
            return new CueSheetHandle(id);
        }

        /// <summary>
        ///     Asynchronously loads and registers a CueSheet via the configured <see cref="ICueSheetProvider" />.
        /// </summary>
        /// <param name="key">The asset key passed to the provider.</param>
        /// <returns>A handle that identifies this registration.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no provider is configured.</exception>
        public async Task<CueSheetHandle> RegisterCueSheetAsync(string key)
        {
            if (_provider == null)
                throw new InvalidOperationException("ICueSheetProvider is not set.");

            var asset = await _provider.LoadAsync(key);
            if (asset == null)
                throw new InvalidOperationException($"Failed to load CueSheet with key '{key}'.");
            return RegisterCueSheet(asset);
        }

        /// <summary>
        ///     Unregisters the CueSheet identified by the handle.
        ///     If a provider is configured, <see cref="ICueSheetProvider.Release" /> is called.
        /// </summary>
        /// <param name="handle">The handle returned by <see cref="RegisterCueSheet" />.</param>
        public void UnregisterCueSheet(CueSheetHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (!_cueSheets.TryGetValue(handle.Id, out var registration))
                return;

            _provider?.Release(registration.Asset);
            _cueSheets.Remove(handle.Id);
        }
    }
}
