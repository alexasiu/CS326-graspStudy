using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PegInHole : MonoBehaviour
{

    public GameObject pegCheck_1;
    public GameObject pegCheck_2;

    public bool CheckPegInHole() {
        return (Check(pegCheck_1.GetComponent<TriggerDetector>()) && Check(pegCheck_2.GetComponent<TriggerDetector>()));
    }

    private bool Check(TriggerDetector checkObj) {
        return (checkObj.collided && checkObj.objCollider.name == "Peg");
    }

}
