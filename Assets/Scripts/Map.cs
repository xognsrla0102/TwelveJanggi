using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Map : MonoBehaviour
{
    public Slot[,] maps = new Slot[4,3];

    void Awake()
    {
        //자식의 순서를 정하기 위한 변수
        int child = 0;
        for (int y = 0; y < maps.GetLength(0); y++)
        {
            for (int x = 0; x < maps.GetLength(1); x++)
            {
                //n번째 자식을 maps[y,x]에 넣는다.
                maps[y, x] = gameObject.transform.GetChild(child).GetComponent<Slot>();

                maps[y, x].pos = new Vector2(x, y);
                child++;
            }
        }
    }
 

 
}
