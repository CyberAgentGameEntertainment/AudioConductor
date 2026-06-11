var _audioConductorListener = null;

mergeInto(LibraryManager.library, {
    AudioConductor_RegisterVisibilityChange: function (callbackPtr) {
        if (_audioConductorListener) {
            document.removeEventListener("visibilitychange", _audioConductorListener);
            _audioConductorListener = null;
        }
        _audioConductorListener = function () {
            {{{ makeDynCall('vi', 'callbackPtr') }}}(document.hidden ? 1 : 0);
        };
        document.addEventListener("visibilitychange", _audioConductorListener);
    },

    AudioConductor_UnregisterVisibilityChange: function () {
        if (_audioConductorListener) {
            document.removeEventListener("visibilitychange", _audioConductorListener);
            _audioConductorListener = null;
        }
    },

    AudioConductor_IsDocumentHidden: function () {
        return document.hidden ? 1 : 0;
    }
});
