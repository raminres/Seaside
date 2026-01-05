using UnityEngine;
using UnityEngine.Audio;

namespace Seaside.Core
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;
        
        [Header("Mixer Parameters")]
        [SerializeField] private string _masterVolumeParam = "MasterVolume";
        [SerializeField] private string _musicVolumeParam = "MusicVolume";
        [SerializeField] private string _sfxVolumeParam = "SFXVolume";
        [SerializeField] private string _ambientVolumeParam = "AmbientVolume";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _ambientSource;

        [Header("Settings")]
        [SerializeField] private float _minVolume = -80f;
        [SerializeField] private float _maxVolume = 0f;

        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SFXVolumeKey = "SFXVolume";
        private const string AmbientVolumeKey = "AmbientVolume";

        protected override void Awake()
        {
            base.Awake();
            LoadVolumeSettings();
        }

        #region Volume Control

        /// <summary>
        /// Set master volume (0-1 range).
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            SetMixerVolume(_masterVolumeParam, volume);
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        }

        /// <summary>
        /// Set music volume (0-1 range).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            SetMixerVolume(_musicVolumeParam, volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        }

        /// <summary>
        /// Set SFX volume (0-1 range).
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SetMixerVolume(_sfxVolumeParam, volume);
            PlayerPrefs.SetFloat(SFXVolumeKey, volume);
        }

        /// <summary>
        /// Set ambient volume (0-1 range).
        /// </summary>
        public void SetAmbientVolume(float volume)
        {
            SetMixerVolume(_ambientVolumeParam, volume);
            PlayerPrefs.SetFloat(AmbientVolumeKey, volume);
        }

        private void SetMixerVolume(string parameter, float normalizedVolume)
        {
            if (_audioMixer == null) return;
            
            // Convert 0-1 to logarithmic dB scale
            float dB = normalizedVolume > 0.0001f 
                ? Mathf.Log10(normalizedVolume) * 20f 
                : _minVolume;
            
            dB = Mathf.Clamp(dB, _minVolume, _maxVolume);
            _audioMixer.SetFloat(parameter, dB);
        }

        private void LoadVolumeSettings()
        {
            SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
            SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
            SetSFXVolume(PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
            SetAmbientVolume(PlayerPrefs.GetFloat(AmbientVolumeKey, 1f));
        }

        public float GetMasterVolume() => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        public float GetMusicVolume() => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        public float GetSFXVolume() => PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        public float GetAmbientVolume() => PlayerPrefs.GetFloat(AmbientVolumeKey, 1f);

        #endregion

        #region Playback

        /// <summary>
        /// Play a music track, optionally with crossfade.
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (_musicSource == null || clip == null) return;

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        /// <summary>
        /// Stop current music.
        /// </summary>
        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        /// <summary>
        /// Play a one-shot sound effect.
        /// </summary>
        public void PlaySFX(AudioClip clip)
        {
            if (_sfxSource == null || clip == null) return;
            _sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Play SFX with volume scale.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale)
        {
            if (_sfxSource == null || clip == null) return;
            _sfxSource.PlayOneShot(clip, volumeScale);
        }

        /// <summary>
        /// Play SFX at a specific world position.
        /// </summary>
        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volumeScale);
        }

        /// <summary>
        /// Set ambient loop.
        /// </summary>
        public void PlayAmbient(AudioClip clip)
        {
            if (_ambientSource == null || clip == null) return;

            _ambientSource.clip = clip;
            _ambientSource.loop = true;
            _ambientSource.Play();
        }

        /// <summary>
        /// Stop ambient sound.
        /// </summary>
        public void StopAmbient()
        {
            if (_ambientSource != null)
            {
                _ambientSource.Stop();
            }
        }

        #endregion
    }
}
