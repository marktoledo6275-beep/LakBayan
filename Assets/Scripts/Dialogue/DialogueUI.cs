using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text speakerNameText;
    [SerializeField] private Text dialogueText;

    [Header("Optional References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private bool hidePanelOnStart = true;

    private string[] activeLines = new string[0];
    private int currentLineIndex;
    private NPCInteractable currentSpeaker;
    private Action onDialogueFinished;

    /// <summary>
    /// Provides easy global access to the active dialogue UI.
    /// </summary>
    public static DialogueUI Instance { get; private set; }

    /// <summary>
    /// Lets other systems know if a dialogue panel is currently visible.
    /// </summary>
    public bool IsDialogueOpen => dialoguePanel != null && dialoguePanel.activeSelf;

    /// <summary>
    /// Registers the singleton instance and prepares the panel state.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (hidePanelOnStart)
        {
            HideDialoguePanel();
        }
    }

    /// <summary>
    /// Cleans up the singleton reference when this object is removed.
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Starts a new dialogue sequence for the selected NPC.
    /// </summary>
    public void StartDialogue(NPCInteractable speaker, string[] lines, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        currentSpeaker = speaker;
        activeLines = lines;
        currentLineIndex = 0;
        onDialogueFinished = onComplete;

        ShowDialoguePanel();
        UpdateDialogueLine();

        if (player != null)
        {
            player.SetMovementEnabled(false);
        }
    }

    /// <summary>
    /// Advances to the next line or closes the dialogue when it reaches the end.
    /// </summary>
    public void AdvanceDialogue()
    {
        if (!IsDialogueOpen)
        {
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= activeLines.Length)
        {
            EndDialogue();
            return;
        }

        UpdateDialogueLine();
    }

    /// <summary>
    /// Closes the current dialogue and restores player movement.
    /// </summary>
    public void EndDialogue()
    {
        Action completedCallback = onDialogueFinished;

        HideDialoguePanel();

        activeLines = new string[0];
        currentLineIndex = 0;
        currentSpeaker = null;
        onDialogueFinished = null;

        if (player != null)
        {
            player.SetMovementEnabled(true);
        }

        completedCallback?.Invoke();
    }

    /// <summary>
    /// Lets UI buttons advance the current dialogue on mobile.
    /// </summary>
    public void AdvanceDialogueFromButton()
    {
        AdvanceDialogue();
    }

    /// <summary>
    /// Lets UI buttons close the current dialogue if needed.
    /// </summary>
    public void EndDialogueFromButton()
    {
        EndDialogue();
    }

    /// <summary>
    /// Checks whether the given NPC is the one currently speaking.
    /// </summary>
    public bool IsCurrentSpeaker(NPCInteractable speaker)
    {
        return currentSpeaker == speaker && IsDialogueOpen;
    }

    /// <summary>
    /// Stores a player reference at runtime when you want to assign it from code.
    /// </summary>
    public void SetPlayer(PlayerController targetPlayer)
    {
        player = targetPlayer;
    }

    /// <summary>
    /// Refreshes the speaker name and line text on the UI.
    /// </summary>
    private void UpdateDialogueLine()
    {
        if (speakerNameText != null)
        {
            speakerNameText.text = currentSpeaker != null ? currentSpeaker.DisplayName : string.Empty;
        }

        if (dialogueText != null)
        {
            dialogueText.text = activeLines[currentLineIndex];
        }
    }

    /// <summary>
    /// Shows the dialogue panel before a conversation starts.
    /// </summary>
    private void ShowDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the dialogue panel when no one is speaking.
    /// </summary>
    private void HideDialoguePanel()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}
