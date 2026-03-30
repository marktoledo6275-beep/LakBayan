using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private bool canInteract = true;
    [SerializeField] private string interactionPrompt = "Interact";

    /// <summary>
    /// Tells the player controller if this object is currently available for interaction.
    /// </summary>
    public bool CanInteract => canInteract && enabled && gameObject.activeInHierarchy;

    /// <summary>
    /// Simple label that can be reused by touch prompts or tooltips.
    /// </summary>
    public string InteractionPrompt => interactionPrompt;

    /// <summary>
    /// Override this in NPCs or world objects to define what happens on interaction.
    /// </summary>
    public abstract void Interact(PlayerController player);

    /// <summary>
    /// Enables or disables interaction at runtime.
    /// </summary>
    public void SetInteractableState(bool isEnabled)
    {
        canInteract = isEnabled;
    }
}
