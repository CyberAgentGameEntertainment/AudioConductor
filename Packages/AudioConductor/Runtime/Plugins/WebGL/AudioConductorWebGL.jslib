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
    }
});
