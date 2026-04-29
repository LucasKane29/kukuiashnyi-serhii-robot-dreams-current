using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private string clickSoundId = "click";
    [SerializeField] private string hoverSoundId = "";
    [SerializeField] private bool playHover = false;
    private AudioManager _audioManager;
    private static float lastClickTime;
    private const float CLICK_COOLDOWN = 0.05f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClick);
        _audioManager = IServiceLocator.Instance.GetService<AudioManager>();
    }

    private void PlayClick()
    {
        if (!button.interactable) return;
        if (Time.unscaledTime - lastClickTime < CLICK_COOLDOWN) return;

        _audioManager.PlaySound(clickSoundId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHover || !button.interactable) return;
        _audioManager.PlaySound(hoverSoundId);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayClick);
    }
}