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

        if (GetGaze3DPoint() == Vector3.zero) return;
        this.transform.position = GetGaze3DPoint();
        // Debug.Log(Get3DPoint());

    }

    // Returns the gaze 3D point in World coordinates
    public Vector3 GetGaze3DPoint() {
        Vector3 trans = Vector3.zero;

        Tobii.G2OM.FocusedCandidate focus = GetFocusedObject();
        
        if (!focus.IsRayValid) return trans;

        RaycastHit hit;
        Physics.Raycast(focus.Origin, focus.Direction, out hit, Mathf.Infinity);

        return hit.point;
    }

    public GameObject GetGazeGameObject() {
        return GetFocusedObject().GameObject;
    }

    // Get the gaze object of focus
    public Tobii.G2OM.FocusedCandidate GetFocusedObject() {
        
        Tobii.G2OM.FocusedCandidate focus = 
                        new Tobii.G2OM.FocusedCandidate {
                            GameObject = null, 
                            IsRayValid = false, 
                            Origin = Vector3.zero,
                            Direction = Vector3.zero
                        };

        // Check whether Tobii XR has any focused objects.
        if (TobiiXR.FocusedObjects.Count > 0)
        {
            // The object being focused by the user, determined by G2OM.
            focus = TobiiXR.FocusedObjects[0];
        } 
        return focus;
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
