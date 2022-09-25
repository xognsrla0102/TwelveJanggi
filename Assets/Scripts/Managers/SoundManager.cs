using System.Collections.Generic;
using UnityEngine;

public enum ESoundType
{
    BGM,
    SFX,
    NUMS
}

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource[] sources = new AudioSource[(int)ESoundType.NUMS];
    // 인스펙터에서 넣을 소리들
    [SerializeField] private AudioClip[] sounds;

    // 실제로 쓰일 소리들
    private Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    public AudioSource BgmSource => sources[(int)ESoundType.BGM];
    public AudioSource SfxSource => sources[(int)ESoundType.SFX];

    private void Start()
    {
        foreach (var sound in sounds)
        {
            if (clips.ContainsKey(sound.name) == false)
            {
                clips.Add(sound.name, sound);
            }
        }
    }

    public void PlayBGM(string name)
    {
        AudioSource bgmSource = BgmSource;

        // 해당 BGM이 이미 재생 중이라면 무시 
        if (bgmSource.clip == clips[name])
        {
            return;
        }

        // 다른 BGM일 경우 중지
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }

        bgmSource.clip = clips[name];
        bgmSource.Play();
    }

    public void PlaySND(string name)
    {
        AudioSource sfxSource = SfxSource;
        sfxSource.clip = clips[name];
        sfxSource.PlayOneShot(sfxSource.clip);
    }
}
