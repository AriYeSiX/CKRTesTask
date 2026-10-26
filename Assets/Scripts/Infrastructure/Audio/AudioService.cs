using UnityEngine;

namespace CKR.Infrastructure.Audio
{
    /// <summary>
    /// Round-robin pool of AudioSources. One shot sounds never allocate a GameObject at runtime,
    /// which matters for the clicker where a sound can fire several times per second.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        [SerializeField, Min(1)] private int _voiceCount = 6;
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;

        private AudioSource[] _voices;
        private int _nextVoice;

        private void Awake()
        {
            _voices = new AudioSource[_voiceCount];

            for (var i = 0; i < _voiceCount; i++)
            {
                var voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.loop = false;
                voice.spatialBlend = 0f;
                _voices[i] = voice;
            }
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchJitter = 0f)
        {
            if (!clip || _voices == null || _voices.Length == 0)
                return;

            var voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;

            voice.pitch = pitchJitter > 0f ? 1f + Random.Range(-pitchJitter, pitchJitter) : 1f;
            voice.PlayOneShot(clip, Mathf.Clamp01(volume) * _masterVolume);
        }
    }
}
