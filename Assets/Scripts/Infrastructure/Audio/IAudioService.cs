using UnityEngine;

namespace CKR.Infrastructure.Audio
{
    public interface IAudioService
    {
        void PlaySfx(AudioClip clip, float volume = 1f, float pitchJitter = 0f);
    }
}
