using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.XR;


/*
 * Functions for getting the gaze position and 
 * object of focus.
 */
public class GazeManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() 
    {
        this.transform.position = GetGazeOrigin();
        Debug.Log(GetGazeOrigin());
        Debug.Log(GetFocusedObject().name);
    }


    private GameObject GetFocusedObject() {
        // Check whether Tobii XR has any focused objects.
        if (TobiiXR.FocusedObjects.Count > 0)
        {
            // The object being focused by the user, determined by G2OM.
            var focusedObject = TobiiXR.FocusedObjects[0];
            return focusedObject.GameObject; 
        } 
        return null;
    }

    private void GetGaze3DPosition() {
    }

    private Vector3 GetGazeOrigin() {
        var gazeRay = TobiiXR.EyeTrackingData.GazeRay;

        // Check if gaze ray is valid
        if(TobiiXR.EyeTrackingData.GazeRay.IsValid)
        {
            // Eye Origin is in World Space
            var rayOrigin = TobiiXR.EyeTrackingData.GazeRay.Origin;

            return rayOrigin;
        }

        return Vector3.zero;
    }

    private Vector3 GetGazeDirection() {
        var gazeRay = TobiiXR.EyeTrackingData.GazeRay;

        // Check if gaze ray is valid
        if(TobiiXR.EyeTrackingData.GazeRay.IsValid)
        {
            // Eye Direction is a normalized direction vector in World Space
            var rayDirection = TobiiXR.EyeTrackingData.GazeRay.Direction;

            return rayDirection;
        }
        return Vector3.zero;
    }

}
