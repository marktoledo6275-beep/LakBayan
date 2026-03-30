using UnityEngine;
using UnityEngine.UI;

public class MenuCharacterAnimator : MonoBehaviour
{
    [SerializeField] private RectTransform characterRect;
    [SerializeField] private RectTransform shadowRect;
    [SerializeField] private Image shadowImage;
    [SerializeField] private float bobAmplitude = 12f;
    [SerializeField] private float bobSpeed = 1.75f;
    [SerializeField] private float swayAngle = 1.8f;
    [SerializeField] private float swaySpeed = 1.15f;
    [SerializeField] private float scalePulse = 0.02f;

    private Vector2 characterBasePosition;
    private Vector2 shadowBasePosition;
    private Vector3 characterBaseScale = Vector3.one;
    private Vector3 shadowBaseScale = Vector3.one;
    private Color shadowBaseColor = Color.white;

    public void Configure(RectTransform character, RectTransform shadow, Image shadowGraphic)
    {
        characterRect = character;
        shadowRect = shadow;
        shadowImage = shadowGraphic;
        CacheBaseState();
    }

    private void OnEnable()
    {
        CacheBaseState();
    }

    private void Update()
    {
        if (characterRect == null)
        {
            return;
        }

        float time = Time.unscaledTime;
        float bob = Mathf.Sin(time * bobSpeed) * bobAmplitude;
        float sway = Mathf.Sin(time * swaySpeed) * swayAngle;
        float pulse = 1f + Mathf.Sin(time * (bobSpeed * 0.55f)) * scalePulse;

        characterRect.anchoredPosition = characterBasePosition + new Vector2(0f, bob);
        characterRect.localRotation = Quaternion.Euler(0f, 0f, sway);
        characterRect.localScale = characterBaseScale * pulse;

        if (shadowRect != null)
        {
            float compression = Mathf.InverseLerp(-bobAmplitude, bobAmplitude, bob);
            shadowRect.anchoredPosition = shadowBasePosition + new Vector2(0f, bob * 0.08f);
            shadowRect.localScale = new Vector3(
                shadowBaseScale.x * Mathf.Lerp(1.08f, 0.9f, compression),
                shadowBaseScale.y * Mathf.Lerp(1.02f, 0.82f, compression),
                1f);
        }

        if (shadowImage != null)
        {
            float normalizedBob = Mathf.InverseLerp(-bobAmplitude, bobAmplitude, bob);
            Color color = shadowBaseColor;
            color.a = Mathf.Lerp(0.78f, 0.5f, normalizedBob);
            shadowImage.color = color;
        }
    }

    private void CacheBaseState()
    {
        if (characterRect != null)
        {
            characterBasePosition = characterRect.anchoredPosition;
            characterBaseScale = characterRect.localScale;
        }

        if (shadowRect != null)
        {
            shadowBasePosition = shadowRect.anchoredPosition;
            shadowBaseScale = shadowRect.localScale;
        }

        if (shadowImage != null)
        {
            shadowBaseColor = shadowImage.color;
        }
    }
}
