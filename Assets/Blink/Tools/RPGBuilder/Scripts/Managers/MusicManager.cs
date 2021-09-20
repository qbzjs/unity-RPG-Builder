using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        public float fadeSpeed = 0.5f;
        public AudioSource audioSource;
        public float normalMusicVolume = 0.3f;

        public List<AudioClip> defaultMusicClips = new List<AudioClip>();

        private Coroutine musicFadeCoroutine;
        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void InitializeSceneMusic()
        {
            if (Camera.main == null) return;
            AudioSource camAudioSource = Camera.main.gameObject.GetComponent<AudioSource>();
            audioSource = camAudioSource != null ? camAudioSource : Camera.main.gameObject.AddComponent<AudioSource>();
            audioSource.volume = normalMusicVolume;
            if (defaultMusicClips.Count > 0)
            {
                HandleMusicFadeCoroutine(defaultMusicClips[GETRandomMusicIndex(0, defaultMusicClips.Count)]);
            }
        }

        public void HandleMusicFadeCoroutine(AudioClip clip)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(PlayGameMusic(clip));
        }

        void Update()
        {
            if (!RPGBuilderEssentials.Instance.isInGame) return;
            HandleNextMusic();
        }

        private void HandleNextMusic()
        {
            if (audioSource == null || audioSource.isPlaying) return;
            RPGGameScene.REGION_DATA currentPlayerRegionData = RegionManager.Instance.getPlayerRegion();
            if (currentPlayerRegionData == null)
            {
                if (defaultMusicClips.Count == 0) return;
                HandleMusicFadeCoroutine(defaultMusicClips[GETRandomMusicIndex(0, defaultMusicClips.Count)]);
            }
            else
            {
                if (currentPlayerRegionData.musicClips.Count == 0) return;
                HandleMusicFadeCoroutine(
                    currentPlayerRegionData.musicClips
                        [GETRandomMusicIndex(0, currentPlayerRegionData.musicClips.Count)]);
            }
        }

        public int GETRandomMusicIndex(int min, int max)
        {
            return Random.Range(min, max);
        }

        private IEnumerator PlayGameMusic(AudioClip clip)
        {
            if (audioSource.clip != null)
            {
                while (Math.Abs(audioSource.volume - 0f) > 0.05f)
                {
                    audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * fadeSpeed);
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                audioSource.volume = 0;
            }

            audioSource.clip = clip;
            audioSource.Play();
            
            while (Math.Abs(audioSource.volume - normalMusicVolume) > 0.05f)
            {
                audioSource.volume = Mathf.Lerp(audioSource.volume, normalMusicVolume, Time.deltaTime * fadeSpeed);
                yield return new WaitForEndOfFrame();
            }

            audioSource.volume = normalMusicVolume;
        }

        public void StopGameMusic()
        {
            audioSource.Stop();
        }
    }
}
