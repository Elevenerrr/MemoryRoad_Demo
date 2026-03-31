using System.Collections;
using UnityEngine;

public class AudioObject : InteractableObject
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip audioClip;
    public string targetTag = "NPCAway";
    public float moveAwayDistance = 10f;

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

        PlayAudioAndMoveNPCs();
    }

    void PlayAudioAndMoveNPCs()
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

        GameObject[] npcs = GameObject.FindGameObjectsWithTag(targetTag);
        foreach (GameObject npc in npcs)
        {
            float distance = Vector3.Distance(transform.position, npc.transform.position);
            if (distance < 15f)
            {
                StartCoroutine(MoveNPCAway(npc.transform));
            }
        }
    }

    IEnumerator MoveNPCAway(Transform npc)
    {
        Vector3 awayDirection = (npc.position - transform.position).normalized;
        awayDirection.y = 0;
        awayDirection.Normalize();

        Vector3 targetPosition = npc.position + awayDirection * moveAwayDistance;

        float duration = 2f;
        float elapsed = 0f;
        Vector3 startPosition = npc.position;

        while (elapsed < duration)
        {
            if (npc != null)
            {
                npc.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (npc != null)
        {
            npc.position = targetPosition;
            Debug.Log($"[NPC] 远离到: {targetPosition}");
        }
    }
}
