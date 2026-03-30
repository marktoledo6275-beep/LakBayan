using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text historicalFactLabelText;

    [Header("Typing Effect")]
    [SerializeField] private bool useTypingEffect = true;
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private bool hidePanelOnStart = true;

    private DialogueData currentDialogue;
    private MonoBehaviour currentSource;
    private PlayerController currentPlayer;
    private Action onDialogueFinished;
    private Coroutine typingCoroutine;
    private int currentLineIndex;
    private bool isTyping;

    public static DialogueManager Instance { get; private set; }

    public bool IsDialogueOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (hidePanelOnStart && dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartDialogue(DialogueData dialogue, MonoBehaviour source, PlayerController player = null, Action onComplete = null)
    {
        if (dialogue == null || !dialogue.HasLines)
        {
            Debug.LogWarning("Cannot start a dialogue that has no lines.", this);
            return;
        }

        currentDialogue = dialogue;
        currentSource = source;
        currentPlayer = player != null ? player : FindFirstObjectByType<PlayerController>();
        onDialogueFinished = onComplete;
        currentLineIndex = 0;

        if (currentPlayer != null)
        {
            currentPlayer.SetMovementEnabled(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowCurrentLine();
    }

    public void AdvanceDialogue()
    {
        if (!IsDialogueOpen || currentDialogue == null)
        {
            return;
        }

        if (isTyping)
        {
            CompleteTypingImmediately();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.Lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    public void CloseDialogueFromButton()
    {
        EndDialogue();
    }

    public void AdvanceDialogueFromButton()
    {
        AdvanceDialogue();
    }

    public bool IsCurrentSource(MonoBehaviour source)
    {
        return currentSource == source && IsDialogueOpen;
    }

    private void ShowCurrentLine()
    {
        if (currentDialogue == null || currentDialogue.Lines == null || currentLineIndex >= currentDialogue.Lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueEntryData line = currentDialogue.Lines[currentLineIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text = currentDialogue.SpeakerName;
        }

        if (historicalFactLabelText != null)
        {
            historicalFactLabelText.gameObject.SetActive(line.showAsHistoricalFact);
            historicalFactLabelText.text = line.showAsHistoricalFact ? "Historical Fact" : string.Empty;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (!useTypingEffect || dialogueText == null)
        {
            if (dialogueText != null)
            {
                dialogueText.text = line.text;
            }

            isTyping = false;
            return;
        }

        typingCoroutine = StartCoroutine(TypeLineRoutine(line.text));
    }

    private IEnumerator TypeLineRoutine(string targetText)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        float delay = charactersPerSecond <= 0f ? 0f : 1f / charactersPerSecond;

        for (int i = 0; i < targetText.Length; i++)
        {
            dialogueText.text += targetText[i];
            yield return delay > 0f ? new WaitForSeconds(delay) : null;
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteTypingImmediately()
    {
        if (currentDialogue == null || dialogueText == null)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueText.text = currentDialogue.Lines[currentLineIndex].text;
        isTyping = false;
    }

    private void EndDialogue()
    {
        Action finishedCallback = onDialogueFinished;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        currentDialogue = null;
        currentSource = null;
        currentLineIndex = 0;
        isTyping = false;
        onDialogueFinished = null;

        if (currentPlayer != null)
        {
            currentPlayer.SetMovementEnabled(true);
            currentPlayer = null;
        }

        finishedCallback?.Invoke();
    }
}
