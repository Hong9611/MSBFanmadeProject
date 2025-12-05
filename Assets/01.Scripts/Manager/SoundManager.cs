using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SoundEntry
{
    public string key;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = false;
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("등록된 사운드")]
    public List<SoundEntry> soundEntries = new List<SoundEntry>();

    [Header("BGM 설정")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    public bool bgmMute = false;
    public AudioSource bgmSourcePrefab;

    [Header("SFX 설정")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public bool sfxMute = false;
    public int sfxPoolSize = 10;
    public AudioSource sfxSourcePrefab;

    // 내부
    private Dictionary<string, SoundEntry> soundDict;
    private AudioSource bgmSource;
    private List<AudioSource> sfxPool;

    private Coroutine bgmFadeCoroutine;

    private void Awake()
    {
        soundDict = new Dictionary<string, SoundEntry>();
        foreach (var se in soundEntries)
        {
            if (se != null && !string.IsNullOrEmpty(se.key) && se.clip != null)
                soundDict[se.key] = se;
        }

        if (bgmSourcePrefab != null)
            bgmSource = Instantiate(bgmSourcePrefab, transform);
        else
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
        bgmSource.playOnAwake = false;
        ApplyBGMSettingsImmediate();

        sfxPool = new List<AudioSource>(sfxPoolSize);
        for (int i = 0; i < Mathf.Max(1, sfxPoolSize); i++)
        {
            AudioSource src;
            if (sfxSourcePrefab != null)
                src = Instantiate(sfxSourcePrefab, transform);
            else
                src = gameObject.AddComponent<AudioSource>();

            src.playOnAwake = false;
            src.loop = false;
            sfxPool.Add(src);
        }
        ApplySFXSettingsImmediate();
    }

    #region Public API - BGM
    public void PlayBGM(string key, float fadeTime = 0.5f, float targetVolume = -1f)
    {
        if (!soundDict.TryGetValue(key, out var se) || se.clip == null) return;
        if (targetVolume < 0f) targetVolume = se.volume;
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeToBGM(se.clip, fadeTime, targetVolume));
    }

    public void StopBGM(float fadeTime = 0.5f)
    {
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeOutAndStopBGM(fadeTime));
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        ApplyBGMSettingsImmediate();
    }

    public void SetBGMMute(bool mute)
    {
        bgmMute = mute;
        ApplyBGMSettingsImmediate();
    }

    public void CrossfadeBGM(string newKey, float duration = 1f, float newVolume = -1f)
    {
        if (!soundDict.TryGetValue(newKey, out var se) || se.clip == null) return;
        if (newVolume < 0f) newVolume = se.volume;
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossfadeCoroutine(se.clip, duration, newVolume));
    }
    #endregion

    #region Public API - SFX
    public AudioSource PlaySFX(string key, float volumeScale = 1f)
    {
        if (!soundDict.TryGetValue(key, out var se) || se.clip == null) return null;
        var src = GetFreeSFXSource();
        if (src == null) return null;
        src.clip = se.clip;
        src.volume = Mathf.Clamp01(se.volume * sfxVolume * volumeScale);
        src.loop = se.loop;
        src.Play();
        return src;
    }

    public void PlaySFXOneShot(string key, float volumeScale = 1f)
    {
        if (!soundDict.TryGetValue(key, out var se) || se.clip == null) return;
        var src = GetFreeSFXSource();
        if (src == null) return;
        src.PlayOneShot(se.clip, Mathf.Clamp01(se.volume * sfxVolume * volumeScale));
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        ApplySFXSettingsImmediate();
    }

    public void SetSFXMute(bool mute)
    {
        sfxMute = mute;
        ApplySFXSettingsImmediate();
    }
    #endregion

    #region Helpers
    private AudioSource GetFreeSFXSource()
    {
        var free = sfxPool.FirstOrDefault(a => !a.isPlaying);
        if (free != null) return free;

        var extra = gameObject.AddComponent<AudioSource>();
        extra.playOnAwake = false;
        sfxPool.Add(extra);
        ApplySFXSettingsToSource(extra);
        return extra;
    }

    private IEnumerator FadeToBGM(AudioClip newClip, float fadeTime, float targetVolume)
    {
        float startVol = bgmSource.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume * bgmVolume * (bgmMute ? 0f : 1f), t / fadeTime);
            yield return null;
        }
        bgmSource.volume = targetVolume * bgmVolume * (bgmMute ? 0f : 1f);
    }

    private IEnumerator FadeOutAndStopBGM(float fadeTime)
    {
        float startVol = bgmSource.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    private IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration, float newTargetVolume)
    {
        var newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.clip = newClip;
        newSource.loop = true;
        newSource.volume = 0f;
        newSource.Play();

        float startVolOld = bgmSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            bgmSource.volume = Mathf.Lerp(startVolOld, 0f, p);
            newSource.volume = Mathf.Lerp(0f, newTargetVolume * bgmVolume * (bgmMute ? 0f : 1f), p);
            yield return null;
        }

        bgmSource.Stop();
        Destroy(bgmSource);
        bgmSource = newSource;
    }

    private void ApplyBGMSettingsImmediate()
    {
        if (bgmSource == null) return;
        bgmSource.volume = bgmVolume * (bgmMute ? 0f : 1f);
        bgmSource.mute = bgmMute;
    }

    private void ApplySFXSettingsImmediate()
    {
        foreach (var src in sfxPool)
        {
            ApplySFXSettingsToSource(src);
        }
    }

    private void ApplySFXSettingsToSource(AudioSource src)
    {
        if (src == null) return;
        src.volume = sfxVolume;
        src.mute = sfxMute;
    }
    #endregion

    #region Utility
    public bool HasSound(string key) => soundDict != null && soundDict.ContainsKey(key);
    public AudioClip GetClip(string key) => HasSound(key) ? soundDict[key].clip : null;

    [ContextMenu("StopAllSFX")]
    public void StopAllSfx()
    {
        foreach (var s in sfxPool) s.Stop();
    }
    #endregion
}
