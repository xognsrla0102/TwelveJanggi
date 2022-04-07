using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public Vector2 pos;

    public void OnDrop(PointerEventData eventData)
    {

        // 드래그 대상이 없다면.
        if (eventData.pointerDrag == null) return;

        var curUnit = eventData.pointerDrag.GetComponent<Janggi>();

        if (curUnit == null) return;
        if (!curUnit.isCaptive)
        {
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
                //자식이 같은 팀일경우엔 넘어간다.
                if (transform.GetComponentInChildren<Janggi>().teamType == curUnit.teamType) return;

                var captrans = GameObject.Find("Content").transform;
                var childObj = transform.GetChild(0);
                childObj.SetParent(captrans);

                childObj.GetComponent<Janggi>().teamType = Janggi.Team_Type.BLUE;
                childObj.GetComponent<Janggi>().SetColor();
                childObj.GetComponent<RectTransform>().localPosition = Vector3.zero;
                childObj.GetComponent<RectTransform>().localRotation = Quaternion.identity;
            }
        }
        else
        {
            if (pos.y == 0 || transform.childCount !=0) return;
        }
        // 그 대상의 부모를 이 슬롯으로 설정 및 위치 설정

        eventData.pointerDrag.transform.SetParent(transform);
        eventData.pointerDrag.GetComponent<RectTransform>().localPosition = Vector3.zero;


    }
}
