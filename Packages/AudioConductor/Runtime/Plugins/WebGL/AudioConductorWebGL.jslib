mergeInto(LibraryManager.library, {
    $audioConductorListener: null,

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
    }
});
