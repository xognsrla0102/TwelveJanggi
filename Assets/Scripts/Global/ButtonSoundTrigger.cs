using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Selectable selectableObj;

    private void Awake()
    {
        selectableObj = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectableObj.enabled == false)
        {
            return;
        }

        SoundManager.Instance.PlaySND(SSfxName.BUTTON_OVER_SFX);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (selectableObj.enabled == false)
        {
            return;
        }

        SoundManager.Instance.PlaySND(SSfxName.BUTTON_CLICK_SFX);
    }
}
