using System.Collections;
using UnityEngine;

public class AudioObject : InteractableObject
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip audioClip;
    public string npcTag = "NPCAway";
    public string playAwayTrigger = "playMusicAway";

    protected override void Start()
    {
        base.Start();
        interactType = InteractableType.Audio;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public override void OnInteract()
    {
        base.OnInteract();
        PlayAudioAndTriggerNPC();
    }

    void PlayAudioAndTriggerNPC()
    {
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
            Debug.Log($"[音频] 播放录音: {audioClip.name}");
        }
        else
        {
            Debug.Log("[音频] 播放录音 (无音频文件)");
        }

        GameObject[] npcs = GameObject.FindGameObjectsWithTag(npcTag);
        foreach (GameObject npc in npcs)
        {
            float distance = Vector3.Distance(transform.position, npc.transform.position);
            if (distance < 15f)
            {
                Animator animator = npc.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger(playAwayTrigger);
                    Debug.Log($"[NPC] 触发动画: {playAwayTrigger} on {npc.name}");
                }
                else
                {
                    Debug.LogWarning($"[NPC] {npc.name} 没有 Animator 组件");
                }
            }
        }
    }
}
