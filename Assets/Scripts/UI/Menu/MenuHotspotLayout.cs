using UnityEngine;

public class MenuHotspotLayout : MonoBehaviour
{
    [System.Serializable]
    private struct HotspotBinding
    {
        public string label;
        public RectTransform target;
        public Rect normalizedRect;
    }

    [Header("Menu Artwork Root")]
    [SerializeField] private RectTransform artworkRect;

    [Header("Tap Zones")]
    [SerializeField] private HotspotBinding newGame = new HotspotBinding
    {
        label = "NewGame",
        normalizedRect = new Rect(0.302f, 0.627f, 0.392f, 0.098f)
    };

    [SerializeField] private HotspotBinding loadGame = new HotspotBinding
    {
        label = "LoadGame",
        normalizedRect = new Rect(0.299f, 0.489f, 0.398f, 0.096f)
    };

    [SerializeField] private HotspotBinding settings = new HotspotBinding
    {
        label = "Settings",
        normalizedRect = new Rect(0.300f, 0.354f, 0.395f, 0.095f)
    };

    private void Reset()
    {
        artworkRect = transform as RectTransform;
    }

    private void Start()
    {
        RefreshLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        RefreshLayout();
    }

    [ContextMenu("Refresh Menu Hotspots")]
    public void RefreshLayout()
    {
        if (artworkRect == null)
        {
            artworkRect = transform as RectTransform;
        }

        ApplyBinding(newGame);
        ApplyBinding(loadGame);
        ApplyBinding(settings);
    }

    public void AssignTargets(RectTransform newGameButton, RectTransform loadGameButton, RectTransform settingsButton)
    {
        newGame.target = newGameButton;
        loadGame.target = loadGameButton;
        settings.target = settingsButton;
        RefreshLayout();
    }

    private void ApplyBinding(HotspotBinding binding)
    {
        if (binding.target == null)
        {
            return;
        }

        binding.target.anchorMin = new Vector2(binding.normalizedRect.xMin, binding.normalizedRect.yMin);
        binding.target.anchorMax = new Vector2(binding.normalizedRect.xMax, binding.normalizedRect.yMax);
        binding.target.offsetMin = Vector2.zero;
        binding.target.offsetMax = Vector2.zero;
        binding.target.localScale = Vector3.one;
    }
}
