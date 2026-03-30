using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private bool allowDiagonalMovement = true;
    [SerializeField] private float inputDeadZone = 0.15f;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 0.75f;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private Transform interactionOrigin;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animation")]
    [SerializeField] private bool useSpriteFlipForHorizontal = false;
    [SerializeField] private float animationDampTime = 0.05f;

    private Rigidbody2D playerRigidbody;
    private Vector2 movementInput;
    private Vector2 virtualJoystickInput;
    private Vector2 lastFacingDirection = Vector2.down;
    private bool interactionRequested;
    private bool canMove = true;

    /// <summary>
    /// Exposes whether the player is currently allowed to move.
    /// </summary>
    public bool CanMove => canMove;

    /// <summary>
    /// Exposes the last cardinal direction the player faced.
    /// </summary>
    public Vector2 FacingDirection => lastFacingDirection;

    /// <summary>
    /// Caches the required components when the player spawns.
    /// </summary>
    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Initializes safe Rigidbody2D settings for a top-down character.
    /// </summary>
    private void Reset()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerRigidbody.gravityScale = 0f;
        playerRigidbody.freezeRotation = true;
        playerRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (interactionOrigin == null)
        {
            interactionOrigin = transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Reads player input each frame and checks for interaction input.
    /// </summary>
    private void Update()
    {
        HandleInput();
        HandleInteractionInput();
    }

    /// <summary>
    /// Moves the Rigidbody2D using physics-friendly movement.
    /// </summary>
    private void FixedUpdate()
    {
        HandleMovement();
    }

    /// <summary>
    /// Combines keyboard input and virtual joystick input into one movement value.
    /// </summary>
    private void HandleInput()
    {
        if (WasInteractionPressedThisFrame())
        {
            interactionRequested = true;
        }

        if (!canMove)
        {
            movementInput = Vector2.zero;
            UpdateAnimation();
            return;
        }

        Vector2 keyboardInput = ReadKeyboardMovement();

        Vector2 combinedInput = virtualJoystickInput.sqrMagnitude > inputDeadZone * inputDeadZone
            ? virtualJoystickInput
            : keyboardInput;

        if (!allowDiagonalMovement && combinedInput.x != 0f && combinedInput.y != 0f)
        {
            if (Mathf.Abs(combinedInput.x) > Mathf.Abs(combinedInput.y))
            {
                combinedInput.y = 0f;
            }
            else
            {
                combinedInput.x = 0f;
            }
        }

        movementInput = Vector2.ClampMagnitude(combinedInput, 1f);

        if (movementInput.sqrMagnitude > inputDeadZone * inputDeadZone)
        {
            lastFacingDirection = GetAnimationDirection(movementInput);
        }

        UpdateAnimation();
    }

    /// <summary>
    /// Reads keyboard movement from the new Input System for editor testing and desktop fallback.
    /// </summary>
    private Vector2 ReadKeyboardMovement()
    {
        if (Keyboard.current == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical);
    }

    /// <summary>
    /// Reads interaction keys from the new Input System.
    /// </summary>
    private bool WasInteractionPressedThisFrame()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.eKey.wasPressedThisFrame ||
               Keyboard.current.enterKey.wasPressedThisFrame ||
               Keyboard.current.spaceKey.wasPressedThisFrame;
    }

    /// <summary>
    /// Processes interaction requests after movement input is read.
    /// </summary>
    private void HandleInteractionInput()
    {
        if (!interactionRequested)
        {
            return;
        }

        interactionRequested = false;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueOpen)
        {
            DialogueManager.Instance.AdvanceDialogue();
            return;
        }

        Vector2 origin = interactionOrigin != null ? (Vector2)interactionOrigin.position : playerRigidbody.position;
        Vector2 direction = lastFacingDirection.sqrMagnitude > 0f ? lastFacingDirection : Vector2.down;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactionDistance, interactionLayers);

        if (hit.collider == null)
        {
            Debug.Log("No interactable target was found in front of the player.", this);
            return;
        }

        Interactable interactable = hit.collider.GetComponent<Interactable>();

        if (interactable == null)
        {
            interactable = hit.collider.GetComponentInParent<Interactable>();
        }

        if (interactable != null && interactable.CanInteract)
        {
            interactable.Interact(this);
        }
    }

    /// <summary>
    /// Applies movement in FixedUpdate so the player moves smoothly and consistently.
    /// </summary>
    private void HandleMovement()
    {
        Vector2 targetPosition = playerRigidbody.position + movementInput * moveSpeed * Time.fixedDeltaTime;
        playerRigidbody.MovePosition(targetPosition);
    }

    /// <summary>
    /// Updates the animator parameters used by idle and walk blend trees or states.
    /// </summary>
    private void UpdateAnimation()
    {
        bool isMoving = movementInput.sqrMagnitude > inputDeadZone * inputDeadZone;

        if (animator != null)
        {
            animator.SetFloat("MoveX", lastFacingDirection.x, animationDampTime, Time.deltaTime);
            animator.SetFloat("MoveY", lastFacingDirection.y, animationDampTime, Time.deltaTime);
            animator.SetBool("IsMoving", isMoving);
        }

        UpdateSpriteDirection();
    }

    /// <summary>
    /// Converts any movement vector into one of the four cardinal directions for animation.
    /// </summary>
    private Vector2 GetAnimationDirection(Vector2 inputDirection)
    {
        if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
        {
            return new Vector2(Mathf.Sign(inputDirection.x), 0f);
        }

        return new Vector2(0f, Mathf.Sign(inputDirection.y));
    }

    /// <summary>
    /// Flips the sprite when facing left so one side-facing animation can serve both directions.
    /// </summary>
    private void UpdateSpriteDirection()
    {
        if (!useSpriteFlipForHorizontal || spriteRenderer == null)
        {
            return;
        }

        if (Mathf.Abs(lastFacingDirection.x) > 0.01f)
        {
            spriteRenderer.flipX = lastFacingDirection.x < 0f;
        }
    }

    /// <summary>
    /// Receives normalized input from a mobile virtual joystick.
    /// </summary>
    public void SetVirtualJoystickInput(Vector2 input)
    {
        virtualJoystickInput = Vector2.ClampMagnitude(input, 1f);
    }

    /// <summary>
    /// Clears the mobile joystick input when the player releases the stick.
    /// </summary>
    public void ClearVirtualJoystickInput()
    {
        virtualJoystickInput = Vector2.zero;
    }

    /// <summary>
    /// Lets a UI button request an interaction on mobile devices.
    /// </summary>
    public void TriggerInteractionButton()
    {
        interactionRequested = true;
    }

    /// <summary>
    /// Enables or disables movement, which is useful during dialogue or cutscenes.
    /// </summary>
    public void SetMovementEnabled(bool isEnabled)
    {
        canMove = isEnabled;

        if (!canMove)
        {
            movementInput = Vector2.zero;
            virtualJoystickInput = Vector2.zero;
            UpdateAnimation();
        }
    }

    /// <summary>
    /// Draws the interaction ray in the editor for easier setup.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector2 origin = interactionOrigin != null ? (Vector2)interactionOrigin.position : (Vector2)transform.position;
        Vector2 direction = lastFacingDirection.sqrMagnitude > 0f ? lastFacingDirection : Vector2.down;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + direction * interactionDistance);
        Gizmos.DrawWireSphere(origin + direction * interactionDistance, 0.08f);
    }
}
