using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.05f;

    [Header("Settings")]
    public bool isDialogueActive = false;
    public bool isTyping = false;

    private List<DialogueLine> currentDialogueLines;
    private int currentLineIndex = 0;
    private Coroutine displayCoroutine;
    private System.Action onDialogueComplete;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        HideDialogue();
    }

    public void ShowDialogue(List<DialogueLine> lines, System.Action onComplete = null)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.Log("[对话] 对话内容为空");
            onComplete?.Invoke();
            return;
        }

        isDialogueActive = true;
        currentDialogueLines = lines;
        currentLineIndex = 0;
        onDialogueComplete = onComplete;

        if (PhoneManager.Instance != null)
        {
            PhoneManager.Instance.OpenDialogue();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogueLines[currentLineIndex];

        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;

        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);

        displayCoroutine = StartCoroutine(DisplayTextRoutine(line.dialogueText, line.displayDuration));
    }

    public void SkipOrNext()
    {
        if (isTyping)
        {
            isTyping = false;
        }
        else
        {
            currentLineIndex++;
            ShowNextLine();
        }
    }

    IEnumerator DisplayTextRoutine(string text, float duration)
    {
        isTyping = true;
        if (dialogueText != null)
            dialogueText.text = "";

        string displayedText = "";
        foreach (char c in text.ToCharArray())
        {
            if (!isTyping)
            {
                dialogueText.text = text;
                isTyping = false;
                break;
            }

            displayedText += c;
            dialogueText.text = displayedText;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        yield return new WaitForSeconds(duration);

        currentLineIndex++;
        ShowNextLine();
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        HideDialogue();

        if (PhoneManager.Instance != null)
        {
            PhoneManager.Instance.CloseDialogue();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        onDialogueComplete?.Invoke();
        Debug.Log("[对话] 对话结束");
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (isDialogueActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                SkipOrNext();
            }
        }
    }
}
