using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{

     public Vector2 pos;
     public bool canPut;
    public void OnDrop(PointerEventData eventData)
    {

        // 드래그 대상이 없다면.
        if (eventData.pointerDrag == null) return;

        var selectJanggi = eventData.pointerDrag.GetComponent<Janggi>();

        if (selectJanggi == null) return;
        
        if (selectJanggi.isCaptive == false)
        {   
  
            if (canPut == false) return;
            // 자식이 있을 경우 다른 장기가 있다는 의미이므로 일단 무시 나중에 적일 경우 먹는 코드 추가
            if (transform.childCount != 0)
            {
                //자식이 같은 팀일경우엔 넘어간다.
                if (transform.GetComponentInChildren<Janggi>().teamType == selectJanggi.teamType) return;

                var captrans = GameObject.Find("Content").transform;
                var childObj = transform.GetChild(0).transform;
                childObj.SetParent(captrans);

                var childObjJanggi = childObj.GetComponent<Janggi>();

                childObjJanggi.teamType = Janggi.Team_Type.BLUE;
                childObjJanggi.SetColor();

                var childRectTransform = childObj.GetComponent<RectTransform>();


                if(childObjJanggi.janggiType == Janggi.Janggi_Type.HOO)
                {
                    childObjJanggi.janggiType = Janggi.Janggi_Type.JA;
                    childObjJanggi.SetName();
                    childObjJanggi.SetDir();                  
                }

                childRectTransform.localPosition = Vector3.zero;
                childRectTransform.localRotation = Quaternion.identity;
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
