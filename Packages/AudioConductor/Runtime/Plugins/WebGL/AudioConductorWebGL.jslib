// Capture Unity's stock JS_Sound_Create_Channel before the override below replaces it, so the override
// can delegate the channel allocation + id bookkeeping to the engine instead of re-implementing it.
// Unity's engine jslib is linked before this plugin, so the symbol is already present here.
LibraryManager.library.$acStockCreateChannel = LibraryManager.library['JS_Sound_Create_Channel'];

mergeInto(LibraryManager.library, {
    $audioConductorListener: null,

    // Per-player scheduling state, keyed by the C# AudioClipPlayer's stable playerId. Each bucket
    // owns that player's two crossover slots (slot0/slot1), so concurrently-playing players never
    // share state. Fields per bucket:
    //   slot0Channel / slot1Channel : the bound Unity channelInstance for each slot (-1 = unbound)
    //   pendingEndTime0 / pendingEndTime1 : scheduled end (dspTime)for each slot (-1 = none)
    //   slot0StartOffset / slot1StartOffset : forced MediaElement start offset (seconds)per slot
    //   nativeLoopPending : a buffer-backed native loop is queued for the next bind
    //   nativeLoopStart / nativeLoopEnd : loopStart/loopEnd (seconds)for the native loop
    // Native-loop "active" tracking is per channel (channel.acNativeLoop), not per player, so a
    // foreign or sibling channel can never clear another channel's loop suppression.
    $acPlayers: {},

    // FIFO of {playerId, slot} awaiting their FIRST channel bind. C# cannot know the JS channelInstance
    // (it is minted inside JS_Sound_Create_Channel), so the binding is correlated by creation order:
    // AudioConductor_PrepareScheduledPlaySlot pushes a request right before its PlayScheduled (only when the
    // slot has no channel yet, so replays never enqueue), and JS_Sound_Create_Channel binds the next
    // freshly created channel to the queue front. Unity allocates a source's channel immediately before
    // that source's first JS_Sound_Play, so the binding is in place before any play resolves.
    $acPendingBinds: [],

    // Reverse map channelInstance -> {playerId, slot}, set at creation (JS_Sound_Create_Channel). Lets
    // every JS_Sound_Play (incl. the multiple calls per PlayScheduled and replays) resolve its
    // player/slot, and lets JS_Sound_ReleaseInstance clear the slot binding. A channelInstance absent
    // from this map is a foreign AudioSource.
    $acChannelMap: {},

    $acGetPlayer__deps: ['$acPlayers'],
    $acGetPlayer: function (playerId) {
        var p = acPlayers[playerId];
        if (!p) {
            p = {
                slot0Channel: -1, slot1Channel: -1,
                pendingEndTime0: -1, pendingEndTime1: -1,
                slot0StartOffset: 0, slot1StartOffset: 0,
                nativeLoopPending: false, nativeLoopStart: 0, nativeLoopEnd: 0
            };
            acPlayers[playerId] = p;
        }
        return p;
    },

    AudioConductor_RegisterVisibilityChange__deps: ['$audioConductorListener'],
    AudioConductor_RegisterVisibilityChange: function (callbackPtr) {
        if (audioConductorListener) {
            document.removeEventListener("visibilitychange", audioConductorListener);
            audioConductorListener = null;
        }
        audioConductorListener = function () {
            {{{ makeDynCall('vi', 'callbackPtr') }}}(document.hidden ? 1 : 0);
        };
        document.addEventListener("visibilitychange", audioConductorListener);
    },

    AudioConductor_UnregisterVisibilityChange__deps: ['$audioConductorListener'],
    AudioConductor_UnregisterVisibilityChange: function () {
        if (audioConductorListener) {
            document.removeEventListener("visibilitychange", audioConductorListener);
            audioConductorListener = null;
        }
    },

    AudioConductor_IsDocumentHidden: function () {
        return document.hidden ? 1 : 0;
    },

    AudioConductor_IsAudioContextRunning: function () {
        // WEBAudio is the state object of Unity's built-in audio library, merged into
        // the same emscripten module scope as this plugin.
        if (typeof WEBAudio === 'undefined' || !WEBAudio.audioContext)
            return 1; // fail-open: behave as before this guard existed
        return WEBAudio.audioContext.state === 'running' ? 1 : 0;
    },

    // Reports whether the WebGL audio backend exposes the FakeMod dsp clock
    // (_GetFakemodTimeInSeconds). AudioConductor's loop scheduling (native loop, crossover
    // alignment, scheduled stops) is expressed in dspTime and depends on this function. It is
    // present on 2022.3 and on Unity 6000 builds from the version that reintroduced the
    // AudioContext suspend/resume block (around 6000.0.39/0.40 and every 6000.1+ stream), but
    // absent from early 6000.0 patches that rebuilt WebGL audio on audioContext.currentTime.
    // C# queries this once to decide between the AudioConductor scheduling path and the stock
    // AudioSource.SetScheduledEndTime path, so unsupported backends fall back instead of crashing.
    AudioConductor_HasFakemodTime: function () {
        return (typeof _GetFakemodTimeInSeconds === 'function') ? 1 : 0;
    },

    // Reports the playback backend of a player slot's channel: -1 = source not created yet,
    // 0 = MediaElementAudioSourceNode, 1 = AudioBufferSourceNode. The backend is not fixed by
    // loadType: Unity 6000 decodes CompressedInMemory/Streaming into an AudioBuffer on Chromium
    // (window.chrome -> decompress) and for small clips, so it varies by browser and version.
    // C# queries this once after the first play to choose native-loop (buffer)vs crossover
    // (MediaElement)scheduling.
    AudioConductor_SlotBackend__sig: 'iii',
    AudioConductor_SlotBackend__deps: ['$acPlayers', '$acGetPlayer'],
    AudioConductor_SlotBackend: function (playerId, slot) {
        var player = acGetPlayer(playerId);
        var ch = slot == 0 ? player.slot0Channel : player.slot1Channel;
        if (ch < 0) return -1;
        var channel = WEBAudio.audioInstances[ch];
        if (!channel || !channel.source) return -1;
        return channel.source.mediaElement ? 0 : 1;
    },

    // Applies a native AudioBufferSourceNode loop (loopStart/loopEnd) to a slot's already-playing
    // source. Used when C# detects the backend is a buffer: the source then loops the partial
    // region itself with no re-scheduling, avoiding the crossover restart that Unity 6000 would
    // otherwise pitch-shift (playbackRate forced to outputRate/clipRate) and double-play.
    AudioConductor_ApplyNativeLoopToSlot__proxy: 'sync',
    AudioConductor_ApplyNativeLoopToSlot__sig: 'viiff',
    AudioConductor_ApplyNativeLoopToSlot__deps: ['$acPlayers', '$acGetPlayer'],
    AudioConductor_ApplyNativeLoopToSlot: function (playerId, slot, loopStartSecs, loopEndSecs) {
        var player = acGetPlayer(playerId);
        var ch = slot == 0 ? player.slot0Channel : player.slot1Channel;
        if (ch < 0) return;
        var channel = WEBAudio.audioInstances[ch];
        if (!channel || !channel.source || channel.source.mediaElement) return;
        player.nativeLoopStart = loopStartSecs;
        player.nativeLoopEnd = loopEndSecs;
        channel.acNativeLoop = true;
        channel.loop = true;
        channel.loopStart = loopStartSecs;
        channel.loopEnd = loopEndSecs;
        channel.source.loop = true;
        channel.source.loopStart = loopStartSecs;
        channel.source.loopEnd = loopEndSecs;
    },

    // Stores pending native loop parameters for the player's next JS_Sound_Play bind.
    // C# calls this before PlayScheduled(); JS_Sound_Play marks the freshly bound channel
    // (channel.acNativeLoop) and re-applies the loop on subsequent calls on that channel
    // (Unity calls JS_Sound_Play multiple times per PlayScheduled in 2022.3 WebGL).
    AudioConductor_SetPendingNativeLoop__proxy: 'sync',
    AudioConductor_SetPendingNativeLoop__sig: 'viff',
    AudioConductor_SetPendingNativeLoop__deps: ['$acPlayers', '$acGetPlayer'],
    AudioConductor_SetPendingNativeLoop: function (playerId, loopStartSecs, loopEndSecs) {
        var player = acGetPlayer(playerId);
        player.nativeLoopPending = true;
        player.nativeLoopStart = loopStartSecs;
        player.nativeLoopEnd = loopEndSecs;
    },

    // Clears native loop tracking for the player. C# calls this when a non-loop PlayScheduled occurs
    // so subsequent JS_Sound_SetLoop(ch, 0) calls are not suppressed for that player's channels.
    AudioConductor_ClearNativeLoop__proxy: 'sync',
    AudioConductor_ClearNativeLoop__sig: 'vi',
    AudioConductor_ClearNativeLoop__deps: ['$acPlayers', '$acGetPlayer'],
    AudioConductor_ClearNativeLoop: function (playerId) {
        var player = acGetPlayer(playerId);
        player.nativeLoopPending = false;
        var ch0 = player.slot0Channel, ch1 = player.slot1Channel;
        if (ch0 >= 0 && WEBAudio.audioInstances[ch0]) WEBAudio.audioInstances[ch0].acNativeLoop = false;
        if (ch1 >= 0 && WEBAudio.audioInstances[ch1]) WEBAudio.audioInstances[ch1].acNativeLoop = false;
    },

    // Removes any pending first-bind requests for this player/slot from acPendingBinds. C# calls this
    // when a play is stopped or the player is reset/returned to the pool: if the play was stopped
    // before its channel was created (same-frame play+stop, or stopped while the AudioContext was
    // suspended), JS_Sound_Create_Channel never runs to consume the entry pushed by
    // PrepareScheduledPlaySlot, so it would otherwise stay stranded forever and a later unrelated
    // channel creation would mis-bind to this player/slot. No-op when the slot is already bound
    // (the entry was consumed) or was never pushed.
    AudioConductor_CancelPendingBind__proxy: 'sync',
    AudioConductor_CancelPendingBind__sig: 'vii',
    AudioConductor_CancelPendingBind__deps: ['$acPendingBinds'],
    AudioConductor_CancelPendingBind: function (playerId, slot) {
        for (var i = acPendingBinds.length - 1; i >= 0; i--) {
            if (acPendingBinds[i].playerId === playerId && acPendingBinds[i].slot === slot)
                acPendingBinds.splice(i, 1);
        }
    },

    // Clears both slots' scheduled end times for the player. C# calls this on Stop/ResetState so a
    // pooled player (its playerId is stable across Rent/Return) does not carry a previous play's
    // future pendingEndTime into its next play and arm a spurious stop. No-op when the player has
    // never scheduled an end (values already -1).
    AudioConductor_ClearScheduledEndTimes__proxy: 'sync',
    AudioConductor_ClearScheduledEndTimes__sig: 'vi',
    AudioConductor_ClearScheduledEndTimes__deps: ['$acPlayers', '$acGetPlayer'],
    AudioConductor_ClearScheduledEndTimes: function (playerId) {
        var player = acGetPlayer(playerId);
        player.pendingEndTime0 = -1;
        player.pendingEndTime1 = -1;
    },

    // Records the player slot and its playback start offset (seconds)for the next JS_Sound_Play call,
    // and enqueues a first-bind request when the slot has no channel yet. C# calls this before
    // PlayScheduled(). The slot lets the scheduled stop find the right channel; the offset lets
    // JS_Sound_Play force MediaElement playback to the intended start position (loopStart for loop
    // iterations) regardless of the fluctuating offset Unity hands to JS_Sound_Play across its
    // multiple calls per PlayScheduled. Replays (slot already bound) do not enqueue, so the FIFO
    // never accumulates stale entries that a later first bind could mis-consume.
    AudioConductor_PrepareScheduledPlaySlot__proxy: 'sync',
    AudioConductor_PrepareScheduledPlaySlot__sig: 'viif',
    AudioConductor_PrepareScheduledPlaySlot__deps: ['$acPlayers', '$acGetPlayer', '$acPendingBinds'],
    AudioConductor_PrepareScheduledPlaySlot: function (playerId, slot, startOffsetSecs) {
        var player = acGetPlayer(playerId);
        if (slot == 0) player.slot0StartOffset = startOffsetSecs;
        else player.slot1StartOffset = startOffsetSecs;
        // Drop this slot's previous scheduled end so a stale future value cannot arm a spurious
        // self-stop on the source about to start. The play paths that do not re-set it (native loop,
        // backend-detect-pending) would otherwise read the prior play's value in JS_Sound_Play and
        // stop the fresh source mid-loop. Non-loop/MediaElement plays re-set it via
        // AudioConductor_SetScheduledEndTime before the deferred JS_Sound_Play runs. The outgoing slot
        // is intentionally left intact so the crossover alignment/stop still sees its end.
        if (slot == 0) player.pendingEndTime0 = -1;
        else player.pendingEndTime1 = -1;
        var ch = slot == 0 ? player.slot0Channel : player.slot1Channel;
        if (ch < 0) acPendingBinds.push({ playerId: playerId, slot: slot });
    },

    // Records (and, if the channel is already live, applies) a slot's scheduled end time.
    // Unity 2022.3 WebGL omits JS_Sound_SetScheduledEndTime (the WASM never imports it), so this
    // replaces it. endTime is in dspTime (C# _scheduledEndTime). The stop delay is computed against
    // dspTime, not audioContext.currentTime — see JS_Sound_Play for the full rationale.
    // In Unity 2022.3 WebGL, audio commands (JS_Sound_Play)run after game logic in the same frame,
    // so the slot->channel binding may not exist yet when this runs; the value is stored on the
    // player bucket and JS_Sound_Play applies it after playSoundClip.
    AudioConductor_SetScheduledEndTime__proxy: 'sync',
    AudioConductor_SetScheduledEndTime__sig: 'viid',
    AudioConductor_SetScheduledEndTime__deps: ['$acPlayers', '$acGetPlayer'],
    AudioConductor_SetScheduledEndTime: function (playerId, slot, endTime) {
        if (WEBAudio.audioWebEnabled == 0) return;
        var player = acGetPlayer(playerId);
        if (slot == 0) player.pendingEndTime0 = endTime;
        else player.pendingEndTime1 = endTime;
        var ch = slot == 0 ? player.slot0Channel : player.slot1Channel;
        if (ch < 0) return;
        var channel = WEBAudio.audioInstances[ch];
        if (!channel || !channel.source) return;
        // Future-only: a past endTime (stale after a ManualUpdate stall) must not become stop(0) and
        // silence a still-playing source. A later re-arm with a fresh future endTime stops it correctly.
        // Allow a 0.1 ms grace margin (delta > -1e-4) to absorb JS execution lag: TryRearmSchedule sets
        // _scheduledEndTime = _dspClock.DspTime for an immediate boundary, but by the time JS evaluates
        // _GetFakemodTimeInSeconds the clock has advanced slightly, making delta zero or marginally negative.
        // A ManualUpdate stall produces a delta of at least one frame (~16 ms), well outside this margin.
        var now = _GetFakemodTimeInSeconds();
        var delta = endTime - now;
        if (delta > 0) channel.stop(delta);
        else if (delta > -1e-4) channel.stop(0);
    },

    // Overrides Unity's JS_Sound_Create_Channel (delegating allocation to the captured stock impl) to
    // bind the channel to a pending AudioConductor slot at creation. Unity allocates a channel before that
    // source's first JS_Sound_Play, so binding here puts the channelInstance->{playerId,slot} mapping
    // in place BEFORE any play runs - JS_Sound_Play then resolves purely via acChannelMap, with no
    // play-order guessing. A foreign AudioSource creates its channel with no pending bind queued, so
    // it is never mapped and always takes the stock passthrough in JS_Sound_Play.
    JS_Sound_Create_Channel__proxy: 'sync',
    JS_Sound_Create_Channel__sig: 'vii',
    JS_Sound_Create_Channel__deps: ['$jsAudioCreateChannel', '$acStockCreateChannel', '$acPlayers', '$acGetPlayer', '$acPendingBinds', '$acChannelMap'],
    JS_Sound_Create_Channel: function (callback, userData) {
        var ch = acStockCreateChannel(callback, userData);
        if (ch && acPendingBinds.length > 0) {
            var mapping = acPendingBinds.shift();
            acChannelMap[ch] = mapping;
            var player = acGetPlayer(mapping.playerId);
            if (mapping.slot == 0) player.slot0Channel = ch;
            else player.slot1Channel = ch;
        }
        return ch;
    },

    // Overrides Unity's JS_Sound_Play. For channels NOT driven by AudioConductor this is a faithful
    // passthrough to Unity's stock behaviour (stop the old source, then playSoundClip), so unrelated
    // AudioSources are never disturbed and no AudioConductor/FakeMod code runs for them. For channels
    // AudioConductor manages it additionally:
    //   1. Resolves the player/slot via the reverse map (bound at JS_Sound_Create_Channel).
    //   2. Configures the native AudioBufferSourceNode loop (buffer-backed clips) when pending.
    //   3. Forces the crossover start offset (Streaming/CompressedInMemory) so loopStart survives.
    //   4. Aligns the loop crossover start and schedules the stops.
    // Unity calls JS_Sound_Play multiple times per PlayScheduled in 2022.3 WebGL; the native loop /
    // forced offset are re-applied on each call so they survive JS_Sound_SetLoop(ch, 0) issued between
    // calls.
    // When the FakeMod dsp clock is unavailable (AudioConductor_HasFakemodTime returns 0), the C#
    // side keeps the slot/native-loop state empty, so every channel takes the passthrough branch and
    // this override stays inert - in particular it never calls _GetFakemodTimeInSeconds, which is
    // absent from those WebGL audio backends and would otherwise throw.
    JS_Sound_Play__deps: ['$acPlayers', '$acGetPlayer', '$acChannelMap'],
    JS_Sound_Play__proxy: 'sync',
    JS_Sound_Play__sig: 'viiii',
    JS_Sound_Play: function (bufferInstance, channelInstance, offset, delay) {
        if (WEBAudio.audioWebEnabled == 0) return;

        // A channel is AudioConductor-managed iff it was bound at creation (JS_Sound_Create_Channel).
        // Anything not in the reverse map is a foreign AudioSource and must behave exactly as stock.
        var mapping = acChannelMap[channelInstance];

        if (mapping === undefined) {
            // Stock passthrough for foreign AudioSources. The suspend-resume queue
            // (soundsPendingContextResume / contextIsRunning) only exists on the WebGL audio
            // backends that ship it (Unity 2022.3 and the 6000 line that received the deferred-stop
            // fix); 6000.0.x has neither, so feature-detect instead of assuming. When the queue is
            // present and the context is suspended, defer with stopDelay (the field the 6000 fix
            // reads on resume); otherwise play directly, matching the stock backend that lacks it.
            _JS_Sound_Stop(channelInstance, 0);
            var stockClip = WEBAudio.audioInstances[bufferInstance];
            var stockChannel = WEBAudio.audioInstances[channelInstance];
            if (!stockClip) { console.log("Trying to play sound which is not loaded."); return; }
            try {
                if (WEBAudio.contextIsRunning === false && Array.isArray(WEBAudio.soundsPendingContextResume)) {
                    WEBAudio.soundsPendingContextResume.push({channel: stockChannel, clip: stockClip,
                        startTime: WEBAudio.audioContext.currentTime + delay, stopDelay: -1.0, offset: offset});
                } else {
                    stockChannel.playSoundClip(stockClip, WEBAudio.audioContext.currentTime + delay, offset);
                }
            } catch(e) { console.error('playSoundClip error. Exception: ' + e); }
            return;
        }

        // ---- AudioConductor-managed path (any FakeMod-capable backend: 2022.3 and 6000+) ----
        // Cancel the old source's pending start timeout before _JS_Sound_Stop runs. On the 2nd+
        // JS_Sound_Play per PlayScheduled, _JS_Sound_Stop runs while the prior source still has a
        // live playTimeout; _pauseMediaElement() then defers (pauseRequested=true) instead of
        // pausing, so the old <audio> briefly plays -> audible double-play. Cancelling lets
        // _JS_Sound_Stop pause immediately.
        var prevChannel = WEBAudio.audioInstances[channelInstance];
        if (prevChannel && prevChannel.source && prevChannel.source.playTimeout) {
            clearTimeout(prevChannel.source.playTimeout);
            prevChannel.source.playTimeout = null;
        }
        _JS_Sound_Stop(channelInstance, 0);
        var soundClip = WEBAudio.audioInstances[bufferInstance];
        var channel = WEBAudio.audioInstances[channelInstance];
        if (!soundClip) { console.log("Trying to play sound which is not loaded."); return; }

        var slot = mapping.slot;
        var otherSlot = slot == 0 ? 1 : 0;
        var player = acGetPlayer(mapping.playerId);

        // (2) Native loop (buffer-backed clips): configure on the channel before playSoundClip.
        var configureLoop = false;
        if (player.nativeLoopPending) {
            player.nativeLoopPending = false;
            channel.acNativeLoop = true;
            configureLoop = true;
        } else if (channel.acNativeLoop) {
            configureLoop = true;
        }
        if (configureLoop) {
            channel.loop = true;
            channel.loopStart = player.nativeLoopStart;
            channel.loopEnd = player.nativeLoopEnd;
        } else {
            // Clear any stale native-loop flag so a non-loop play is not loop-suppressed, then
            // (3) force the crossover start offset for MediaElement slots. Unity re-invokes
            // JS_Sound_Play per PlayScheduled with a fluctuating offset (0, loopStart, ...) and the
            // LAST call's offset wins, which resets MediaElement playback to the clip start and
            // defeats loopStart. Override it with the offset C# supplied via PrepareScheduledPlaySlot.
            // Native loop (DecompressOnLoad)seeks via loopStart/loopEnd instead, so it is excluded.
            channel.acNativeLoop = false;
            offset = slot == 0 ? player.slot0StartOffset : player.slot1StartOffset;
        }

        // (4a) Loop crossover (Streaming/CompressedInMemory): align this incoming source's start to
        // the OUTGOING slot's scheduled end so they meet exactly. Unity's native PlayScheduled can
        // hand us delay=0 on the first loop (a frame stall pushes dspTime past the boundary before
        // the queued audio command runs), which would start this source early and overlap the
        // outgoing one. Deriving the start from the outgoing pendingEnd against a single dspNow
        // reading fixes that and absorbs dsp->ctx offset drift from AudioContext suspend/resume.
        var dspNow = _GetFakemodTimeInSeconds();
        var outgoingEnd = otherSlot == 0 ? player.pendingEndTime0 : player.pendingEndTime1;
        var startTime = WEBAudio.audioContext.currentTime + delay;
        if (outgoingEnd > 0) {
            var alignedStart = WEBAudio.audioContext.currentTime + Math.max(0, outgoingEnd - dspNow);
            if (alignedStart > startTime) startTime = alignedStart;
        }
        try {
            if (WEBAudio.contextIsRunning) {
                channel.playSoundClip(soundClip, startTime, offset);
            } else {
                WEBAudio.soundsPendingContextResume.push({channel: channel, clip: soundClip,
                    startTime: startTime, stopDelay: -1.0, offset: offset});
            }
        } catch(e) { console.error('playSoundClip error. Exception: ' + e); }

        // (2 cont.) Re-apply the native loop on the freshly created source node so it survives a
        // JS_Sound_SetLoop(ch, 0) issued after start().
        if (configureLoop && channel.source) {
            channel.source.loop = true;
            channel.source.loopStart = player.nativeLoopStart;
            channel.source.loopEnd = player.nativeLoopEnd;
        }

        // (4b) Schedule the stops for the crossover. Guard on channel.source: if the context is
        // suspended, playSoundClip queued to soundsPendingContextResume (no source yet) and
        // TryRearmSchedule re-applies on resume. Stops use channel.stop(delay) (Unity's wrapper),
        // which handles both AudioBufferSourceNode and MediaElementAudioSourceNode. All delays are
        // dspTime-based (pendingEnd is dspTime) so they stay aligned with PlayScheduled.
        if (channel.source) {
            var pendingEnd = slot == 0 ? player.pendingEndTime0 : player.pendingEndTime1;
            // Only arm the self-stop when this slot's end is still in the future. After a ManualUpdate
            // stall the scheduled end can be computed from a past base time, leaving pendingEnd already
            // elapsed; stop(0) would then instantly silence this freshly started source.
            if (pendingEnd > dspNow)
                channel.stop(pendingEnd - dspNow);
            // Re-sync the outgoing slot's stop to this same instant (same dspNow) so it ends exactly
            // where this source starts, rather than at the ctx time it was armed with on its own
            // earlier play (which drifts via offset drift / the delay=0 race). Same future-only guard:
            // a stale past outgoingEnd must not stop(0) a source that is still legitimately playing.
            var otherCh = otherSlot == 0 ? player.slot0Channel : player.slot1Channel;
            if (otherCh >= 0 && outgoingEnd > dspNow) {
                var otherChannel = WEBAudio.audioInstances[otherCh];
                if (otherChannel && otherChannel.source)
                    otherChannel.stop(outgoingEnd - dspNow);
            }
        }
    },

    // Overrides Unity's JS_Sound_SetLoop to suppress loop=false resets for a channel that has an
    // active AudioConductor native loop. Unity calls JS_Sound_SetLoop(ch, audioSource.loop) after
    // PlayScheduled; without this guard the reset clears the source.loop flag that
    // AudioBufferSourceNode relies on for seamless WebAudio looping.
    JS_Sound_SetLoop__proxy: 'sync',
    JS_Sound_SetLoop__sig: 'vii',
    JS_Sound_SetLoop: function (channelInstance, loop) {
        if (WEBAudio.audioWebEnabled == 0) return;
        var channel = WEBAudio.audioInstances[channelInstance];
        if (!loop && channel && channel.acNativeLoop) return;
        if (channel)channel.setLoop(loop);
    },

    // Overrides Unity's JS_Sound_ReleaseInstance to clear AudioConductor channel tracking when the
    // underlying AudioSource is destroyed (AudioClipPlayer.Destroy -> GameObject/AudioSource
    // teardown -> here). Without this, a later unrelated AudioSource that reuses this instance id
    // could be mistaken for a loop slot (crossover stops) or have its loop=false suppressed.
    // Body mirrors Unity 2022.3 Audio.js JS_Sound_ReleaseInstance after the clearing.
    JS_Sound_ReleaseInstance__proxy: 'sync',
    JS_Sound_ReleaseInstance__sig: 'vi',
    JS_Sound_ReleaseInstance__deps: ['$acPlayers', '$acChannelMap'],
    JS_Sound_ReleaseInstance: function (instance) {
        var mapping = acChannelMap[instance];
        if (mapping !== undefined) {
            var player = acPlayers[mapping.playerId];
            if (player) {
                if (mapping.slot == 0) player.slot0Channel = -1;
                else player.slot1Channel = -1;
            }
            delete acChannelMap[instance];
        }
        var object = WEBAudio.audioInstances[instance];
        if (object)object.release();
        delete WEBAudio.audioInstances[instance];
    }
});
