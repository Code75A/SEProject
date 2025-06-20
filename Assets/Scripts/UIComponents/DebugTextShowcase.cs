using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugTextShowcase : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshPro text;
    void Update()
    {
        text.text = "";
        foreach (var pawn in PawnManager.Instance.pawns)
        {
            text.text += pawn.id + " " + pawn.isOnTask +" " + pawn.Instance.GetComponent<PawnInteractController>().moveFinished + "\n";

        }
    }
}
