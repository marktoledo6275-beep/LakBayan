using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ScenePortal : Interactable
{
    [SerializeField] private string targetSceneName = "MainWorld";
    [SerializeField] private bool useTriggerZone;
    [SerializeField] private bool rememberCurrentSceneAsReturnPoint = true;

    private void Reset()
    {
        Collider2D portalCollider = GetComponent<Collider2D>();
        portalCollider.isTrigger = useTriggerZone;
    }

    public override void Interact(PlayerController player)
    {
        if (SceneController.Instance == null)
        {
            Debug.LogWarning("ScenePortal could not load a scene because no SceneController exists in the project.", this);
            return;
        }

        if (rememberCurrentSceneAsReturnPoint && GameManager.Instance != null)
        {
            GameManager.Instance.SetLastWorldScene(SceneManager.GetActiveScene().name);
        }

        SceneController.Instance.LoadScene(targetSceneName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTriggerZone)
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            Interact(player);
        }
    }
}
