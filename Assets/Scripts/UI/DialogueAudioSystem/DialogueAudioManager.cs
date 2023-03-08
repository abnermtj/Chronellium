using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueAudioManager : MonoBehaviour
{
    private const string AUDIO_TAG = "audio";
    // for when 23:27 SetCurrentAudioInfo(tagValue);

    [Header("Audio")]
    [SerializeField] private DialogueAudioInfoSO defaultAudioInfo;
    [SerializeField] private DialogueAudioInfoSO[] audioInfos;
    [SerializeField] private bool makePredictable;
    [Range(0, 1)]
    public float volume = 0.5f;
    private DialogueAudioInfoSO currentAudioInfo;
    private Dictionary<string, DialogueAudioInfoSO> audioInfoDictionary;
    private AudioSource audioSource;

    public void PlayDialogueSound(int currentDisplayedCharacters, char currentCharacter)
    {
        // set variables for the below based on our config
        AudioClip[] dialogueTypingSoundClips = currentAudioInfo.dialogueTypingSoundClips;
        int frequencyLevel = currentAudioInfo.frequencyLevel;
        float minPitch = currentAudioInfo.minPitch;
        float maxPitch = currentAudioInfo.maxPitch;
        bool stopAudioSource = currentAudioInfo.stopAudioSource;

        // play the sound based on the config
        if (currentDisplayedCharacters % frequencyLevel == 0)
        {
            if (stopAudioSource)
            {
                audioSource.Stop();
            }
            AudioClip soundClip = null;
            // create predictable audio from hashing
            if (makePredictable)
            {
                // Note: hashcode generated in this manner may differ based on platform, version and 
                // overall environment the game is running in.
                // int hashCode = currentCharacter.GetHashCode();
                int hashCode = GetStableHashCodeChar(currentCharacter);

                // sound clip
                int predictableIndex = hashCode % dialogueTypingSoundClips.Length;
                soundClip = dialogueTypingSoundClips[predictableIndex];
                // pitch
                int minPitchInt = (int)(minPitch * 100);
                int maxPitchInt = (int)(maxPitch * 100);
                int pitchRangeInt = maxPitchInt - minPitchInt;
                // cannot divide by 0, so if there is no range then skip the selection
                if (pitchRangeInt != 0)
                {
                    int predictablePitchInt = (hashCode % pitchRangeInt) + minPitchInt;
                    float predictablePitch = predictablePitchInt / 100f;
                    audioSource.pitch = predictablePitch;
                }
                else
                {
                    audioSource.pitch = minPitch;
                }

            }
            else
            {
                // sound clip
                int randomIndex = Random.Range(0, dialogueTypingSoundClips.Length);
                soundClip = dialogueTypingSoundClips[randomIndex];
                // pitch
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }

            // play sound
            audioSource.volume = volume;
            audioSource.PlayOneShot(soundClip);
        }
    }

    public void SetCurrentAudioInfo(string id)
    {
        DialogueAudioInfoSO audioInfo = null;
        audioInfoDictionary.TryGetValue(id, out audioInfo);
        if (audioInfo != null)
        {
            this.currentAudioInfo = audioInfo;
        }
        else
        {
            // use default if speaker AudioInfo not defined
            this.currentAudioInfo = defaultAudioInfo;
            Debug.LogWarning("Failed to find audio info for id: " + id + ".\n Reverting to default audio info.");
        }
    }

    private void InitializeAudioInfoDictionary()
    {
        audioInfoDictionary = new Dictionary<string, DialogueAudioInfoSO>();
        audioInfoDictionary.Add(defaultAudioInfo.id, defaultAudioInfo);
        foreach (DialogueAudioInfoSO audioInfo in audioInfos)
        {
            audioInfoDictionary.Add(audioInfo.id, audioInfo);
        }
    }

    private void Awake()
    {
        audioSource = this.gameObject.AddComponent<AudioSource>();
        currentAudioInfo = defaultAudioInfo;
    }

    private void Start()
    {
        InitializeAudioInfoDictionary();
    }

    private int GetStableHashCodeChar(char str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            hash1 = ((hash1 << 5) + hash1) ^ str;
            hash2 = ((hash2 << 5) + hash2) ^ str;

            // Note: Abs is used here. Double check if it is appropriate for generating a hash value.
            return System.Math.Abs(hash1 + (hash2 * 1566083941));
        }
    }
}
