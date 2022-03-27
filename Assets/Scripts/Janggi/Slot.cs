using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{    
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 대상이 있다면
        if (eventData.pointerDrag != null)
        {
            // 자식이 있을 경우 다른 장기가 있다는 의미이므로 일단 무시 나중에 적일 경우 먹는 코드 추가
            if (transform.childCount != 0) return;

            // 그 대상의 부모를 이 슬롯으로 설정 및 위치 설정
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
    }
}
