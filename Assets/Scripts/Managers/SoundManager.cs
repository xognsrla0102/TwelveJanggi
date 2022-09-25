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
    // �ν����Ϳ��� ���� �Ҹ���
    [SerializeField] private AudioClip[] sounds;

    // ������ ���� �Ҹ���
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

        // �ش� BGM�� �̹� ��� ���̶�� ���� 
        if (bgmSource.clip == clips[name])
        {
            return;
        }

        // �ٸ� BGM�� ��� ����
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
