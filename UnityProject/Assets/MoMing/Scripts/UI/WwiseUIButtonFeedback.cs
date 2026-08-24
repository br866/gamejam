using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Runtime-only handler installed by WwiseUIFeedbackRouter. It does not alter
/// Button.onClick, so existing menu behavior and listener order remain intact.
/// </summary>
[AddComponentMenu("")]
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class WwiseUIButtonFeedback : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    ISelectHandler,
    ISubmitHandler
{
    private WwiseUIFeedbackRouter router;
    private Button button;
    private bool pointerInside;

    internal void Initialize(WwiseUIFeedbackRouter owner, Button target)
    {
        router = owner;
        button = target != null ? target : GetComponent<Button>();
    }

    private bool CanPlay()
    {
        if (button == null)
            button = GetComponent<Button>();

        return router != null
            && button != null
            && button.IsActive()
            && button.IsInteractable();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        if (CanPlay())
            router.PostHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && CanPlay())
            router.PostClick();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Pointer selection commonly follows pointer entry; do not play Hover twice.
        if (!pointerInside && CanPlay())
            router.PostHover();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlay())
            router.PostClick();
    }
}
