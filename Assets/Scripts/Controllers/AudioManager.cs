using UnityEngine;
using TRKGeneric;

public class AudioManager : MonoSingleton<AudioManager>
{
    public AudioSource effects;
    public AudioSource music;

    public AudioClip[] effectClips;
    public AudioClip[] musicClips;

    public void PlayClip(int clipIndex)
    {
        effects.PlayOneShot(effectClips[clipIndex]);
    }
    public void GameOver()
    {
        PlayClip(2);
        //play game over music
        music.clip = musicClips[1];
        music.PlayDelayed(3.5f);
    }
}
