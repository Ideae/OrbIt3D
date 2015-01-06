using UnityEngine;
using System.Collections;

public class FlyScript : MonoBehaviour {
    OVRCameraRig cameraRig = null;
    public float steering = 0.1f;
    public float flyspeed = 0.1f;
	// Use this for initialization
	void Start () {
        cameraRig = GetComponent<OVRCameraRig>();
	}
	
	// Update is called once per frame
	void Update () {
	    //Debug.Log(OVRManager.display.GetHeadPose().orientation.eulerAngles);
        //transform.Rotate(OVRManager.display.GetHeadPose().orientation.eulerAngles); //DIZZY
        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 axis = Vector3.zero;
            float angle = 0f;
            OVRManager.display.GetHeadPose().orientation.ToAngleAxis(out angle, out axis);
            transform.Rotate(axis, angle * steering);
            transform.position += transform.rotation * Vector3.forward * flyspeed;

        }
	}
}
