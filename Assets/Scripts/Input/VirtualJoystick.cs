using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private float movementRange = 75f;
    [SerializeField] private bool snapToCardinalDirections;
    [SerializeField] private PlayerController player;

    private Vector2 currentInput;

    private void Start()
    {
        if (background == null)
        {
            background = transform as RectTransform;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null)
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            return;
        }

        currentInput = localPoint / movementRange;
        currentInput = Vector2.ClampMagnitude(currentInput, 1f);

        if (snapToCardinalDirections && currentInput != Vector2.zero)
        {
            if (Mathf.Abs(currentInput.x) > Mathf.Abs(currentInput.y))
            {
                currentInput = new Vector2(Mathf.Sign(currentInput.x), 0f);
            }
            else
            {
                currentInput = new Vector2(0f, Mathf.Sign(currentInput.y));
            }
        }

        if (handle != null)
        {
            handle.anchoredPosition = currentInput * movementRange;
        }

        if (player != null)
        {
            player.SetVirtualJoystickInput(currentInput);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        currentInput = Vector2.zero;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        if (player != null)
        {
            player.ClearVirtualJoystickInput();
        }
    }
}
