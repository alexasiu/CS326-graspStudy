using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;
using Tobii.G2OM;

namespace Tobii.XR
{
public class GazeTracker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(GetFocusedObjects());
    }

    //private List<FocusedCandidate> GetFocusedObjects() {
    //    List<FocusedCandidate> focusedObject = null;
    //    // Check whether Tobii XR has any focused objects.
    //    if (TobiiXR.FocusedObjects.Count > 0)
    //    {
    //        // The object being focused by the user, determined by G2OM.
    //        focusedObject = TobiiXR.FocusedObjects[0];
    //    }
    //    return focusedObject;
    //}



}
}
