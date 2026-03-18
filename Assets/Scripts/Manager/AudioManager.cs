using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public List<Sound> sounds;
    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    private AudioSource bgmSource;
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // 优化3: 预创建 AudioSource 池
    [Header("SFX Pool")]
    public int sfxPoolSize = 10;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private int poolIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        GameObject bgmObject = new GameObject("BGM Source");
        bgmObject.transform.parent = this.transform;
        bgmSource = bgmObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        foreach (Sound s in sounds)
        {
            if (!soundDictionary.ContainsKey(s.name))
            {
                soundDictionary.Add(s.name, s);
            }
        }

        // 优化2: 预创建 SFX AudioSource 池
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject("SFX_Pool_" + i);
            sfxObj.transform.parent = this.transform;
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            sfxObj.SetActive(false);
            sfxPool.Add(source);
        }
    }

    //BGM

    public void PlayMusic(string name, float fadeDuration = 1f)
    {
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogWarning("Music " + name + " not found!");
            return;
        }
        Sound s = soundDictionary[name];
        if (bgmSource.clip == s.clip && bgmSource.isPlaying)
            return;
        Debug.Log("Start Music");
        StartCoroutine(SwitchMusicCoroutine(s, fadeDuration));
    }

    private IEnumerator SwitchMusicCoroutine(Sound newSound, float duration)
    {
        if (bgmSource.isPlaying)
        {
            float startVolume = bgmSource.volume;
            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.deltaTime / (duration / 2);
                yield return null;
            }
            bgmSource.Stop();
        }
        bgmSource.clip = newSound.clip;
        bgmSource.pitch = newSound.pitch;

        bgmSource.Play();
        float targetVolume = newSound.volume * musicVolume * masterVolume;
        bgmSource.volume = 0;
        while (bgmSource.volume < targetVolume)
        {
            bgmSource.volume += targetVolume * Time.deltaTime / (duration / 2);
            yield return null;
        }
    }
    //SFX
    public void PlaySFX(string name)
    {

    }

    public void PlaySFX(string name, bool useRandomPitch)
    {
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogWarning("SFX " + name + " not found!");
            return;
        }
        Sound s = soundDictionary[name];

        // 从对象池获取 AudioSource
        AudioSource sfxSource = sfxPool[poolIndex];
        poolIndex = (poolIndex + 1) % sfxPool.Count;

        sfxSource.clip = s.clip;
        sfxSource.volume = s.volume * sfxVolume * masterVolume;
        if (useRandomPitch)
        {
            sfxSource.pitch = s.pitch * Random.Range(0.8f, 1.2f);
        }
        else
        {
            sfxSource.pitch = s.pitch;
        }

        sfxSource.Play();
        StartCoroutine(ReturnToPoolCoroutine(sfxSource, s.clip.length + 0.1f));
    }

    private IEnumerator ReturnToPoolCoroutine(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
    }

    //音量调节
    public void UpdateMixVolume()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            foreach (var kv in soundDictionary)

            {
                if (kv.Value.clip == bgmSource.clip)
                {
                    bgmSource.volume = kv.Value.volume * musicVolume * masterVolume;
                    break;
                }
            }
        }
    }





}
