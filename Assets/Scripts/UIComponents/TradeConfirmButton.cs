using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeConfirmButton : MonoBehaviour
{
    void OnMouseDown()
    {
        TraderManager.Instance.ConcludeTrade();
        //TraderManager.Instance.SetActive(false);
    }
}
