using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum Sound {
        playerJump,
        enemyAttack,
        step,
        land
    }

    int enumLenght;

    [System.Serializable]
    class SoundAudioClip {
        public Sound sound;
        public AudioClip[] audioClip;
        public float cooldown = -1;
    }
    [SerializeField] SoundAudioClip[] listOfSoundAudioClips;

    [HideInInspector]
    public static AudioClip[][] audioClips;
    public static float[] cooldowns, lastTimePlayed;

    private void Awake() 
    {
        enumLenght = System.Enum.GetNames(typeof(Sound)).Length;
        audioClips = new AudioClip[enumLenght][];
        cooldowns = new float[enumLenght];
        lastTimePlayed = new float[enumLenght];
        foreach (SoundAudioClip soundAudioClip in listOfSoundAudioClips) {
            audioClips[(int)soundAudioClip.sound] = soundAudioClip.audioClip;
            cooldowns[(int)soundAudioClip.sound] = soundAudioClip.cooldown;
            lastTimePlayed[(int)soundAudioClip.sound] = 0f;
        }
    }

    public static void PlaySound(AudioSource audioSource, Sound sound) {
        if (audioClips[(int)sound] == null || audioClips[(int)sound].Length == 0)
            return;
        if (!CanPlaySound(sound))
            return;
        int index = Random.Range(0, audioClips[(int)sound].Length);
        audioSource.PlayOneShot(audioClips[(int)sound][index]);
    }

    static bool CanPlaySound(Sound sound){
        if (cooldowns[(int)sound] == -1)
            return true;
        if (lastTimePlayed[(int)sound] + cooldowns[(int)sound] < Time.time)
        {
            lastTimePlayed[(int)sound] = Time.time;
            return true;
        }
        else
            return false;
    }


}
