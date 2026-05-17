using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Footsteps")]
    public AudioClip footstepsClip;

    [Header("Trash")]
    public AudioClip pickingProgressClip;
    public AudioClip trashPickedClip;

    [Header("Empathy")]
    public AudioClip empathyLossClip;

    [Header("NPC Reactions - Happy")]
    public AudioClip npcHappyIbu;
    public AudioClip npcHappyAnak;
    public AudioClip npcHappyOrmas;

    [Header("NPC Reactions - Angry")]
    public AudioClip npcAngryIbu;
    public AudioClip npcAngryAnak;
    public AudioClip npcAngryOrmas;

    [Header("Game End")]
    public AudioClip winClip;
    public AudioClip loseClip;

    private AudioSource sfxSource;
    private AudioSource loopSource; // For looping sounds (footsteps, picking)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Main one-shot source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        // Looping source for footsteps/picking
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.playOnAwake = false;
        loopSource.loop = true;
    }

    private void Update()
    {
        float vol = AudioSettings.SFXVolume;
        sfxSource.volume = vol;
        loopSource.volume = vol;
    }

    // ---- Footsteps ----
    public void PlayFootstep()
    {
        if (loopSource.clip == footstepsClip && loopSource.isPlaying) return;
        loopSource.clip = footstepsClip;
        loopSource.Play();
    }

    public void StopFootstep()
    {
        if (loopSource.clip == footstepsClip && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    // ---- Picking Progress ----
    public void PlayPickingProgress()
    {
        if (loopSource.clip == pickingProgressClip && loopSource.isPlaying) return;
        loopSource.clip = pickingProgressClip;
        loopSource.Play();
    }

    public void StopPickingProgress()
    {
        if (loopSource.clip == pickingProgressClip && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    // ---- One-shots ----
    public void PlayTrashPicked()
    {
        if (trashPickedClip != null)
            sfxSource.PlayOneShot(trashPickedClip);
    }

    public void PlayEmpathyLoss()
    {
        if (empathyLossClip != null)
            sfxSource.PlayOneShot(empathyLossClip);
    }

    public void PlayWin()
    {
        StopAllLoops();
        if (winClip != null)
            sfxSource.PlayOneShot(winClip);
    }

    public void PlayLose()
    {
        StopAllLoops();
        if (loseClip != null)
            sfxSource.PlayOneShot(loseClip);
    }

    public void PlayNPCReaction(NPCType type, bool happy)
    {
        AudioClip clip = null;
        if (happy)
        {
            switch (type)
            {
                case NPCType.IbuIbu: clip = npcHappyIbu; break;
                case NPCType.AnakKecil: clip = npcHappyAnak; break;
                case NPCType.OknumOrmas: clip = npcHappyOrmas; break;
            }
        }
        else
        {
            switch (type)
            {
                case NPCType.IbuIbu: clip = npcAngryIbu; break;
                case NPCType.AnakKecil: clip = npcAngryAnak; break;
                case NPCType.OknumOrmas: clip = npcAngryOrmas; break;
            }
        }

        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    private void StopAllLoops()
    {
        if (loopSource.isPlaying)
            loopSource.Stop();
    }
}
