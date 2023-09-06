// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerVolumeControlSample : MonoBehaviour
{
    [SerializeField]
    private AudioMixer _audioMixer;

    public void SetBGMVolume(float value)
        => SetVolume("BGMVolume", value);

    public void SetSEVolume(float value)
        => SetVolume("SEVolume", value);

    public void SetVoiceVolume(float value)
        => SetVolume("VoiceVolume", value);

    private void SetVolume(string parameterName, float value)
    {
        // In the AudioMixer inspector, volumes need to be exposed to scripts
        var dB = Mathf.Clamp(20f * Mathf.Log10(Mathf.Clamp(value, 0f, 1f)), -80f, 0f);
        _audioMixer.SetFloat(parameterName, dB);
    }
}
