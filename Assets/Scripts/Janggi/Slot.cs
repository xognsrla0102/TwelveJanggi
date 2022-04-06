using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public Vector2 pos;

    public void OnDrop(PointerEventData eventData)
    {
        
        // 드래그 대상이 있다면
        if (eventData.pointerDrag != null)
        {
            Janggi curUnit = eventData.pointerDrag.GetComponent<Janggi>();

            switch (curUnit.janggiType)
            {
                case Janggi.Janggi_Type.JA:
                    if (pos.y != curUnit.curpos.y - 1 || pos.x != curUnit.curpos.x) return;
                    break;
                case Janggi.Janggi_Type.HOO:
                    if (pos.y > curUnit.curpos.y && pos.x != curUnit.curpos.x || pos.y < curUnit.curpos.y - 1 || pos.y > curUnit.curpos.y + 1) return;
                    break;
                case Janggi.Janggi_Type.SANG:
                    if (pos.y > curUnit.curpos.y + 1 || pos.y < curUnit.curpos.y - 1 || pos.y == curUnit.curpos.y || pos.x == curUnit.curpos.x) return;
                    break;
                case Janggi.Janggi_Type.JANG:
                    if (pos.y > curUnit.curpos.y + 1 || pos.y < curUnit.curpos.y - 1 || pos.x > curUnit.curpos.x + 1 ||
                        pos.x < curUnit.curpos.x - 1 || pos.x != curUnit.curpos.x && pos.y != curUnit.curpos.y) return;
                    break;
                case Janggi.Janggi_Type.WANG:
                    if (pos.y > curUnit.curpos.y + 1 || pos.y < curUnit.curpos.y - 1 || pos.x > curUnit.curpos.x + 1 || pos.x < curUnit.curpos.x - 1) return;
                    break;
                default: Debug.Assert(false); break;
            }

            // 자식이 있을 경우 다른 장기가 있다는 의미이므로 일단 무시 나중에 적일 경우 먹는 코드 추가
            if (transform.childCount != 0)

            {
                Destroy(transform.GetChild(0).gameObject);
            }
            
            
            
            // 그 대상의 부모를 이 슬롯으로 설정 및 위치 설정

            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
    }
}
