using UnityEngine;
using System.Collections;
using OrbItProcs;
public class PlayerController : MonoBehaviour {
    public float lookSensitivity = 0.5f;
	// Use this for initialization
	void Start () {
        //player = GameObject.Find("Player") ?? gameObject;
        playerNode = OrbIt.game.room.spawnNode(OrbIt.game.room.groups.player);
        
	}
    Node playerNode;
	// Update is called once per frame
	void Update () {
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        Vector3 v = Vector3.zero;
        if (vert != 0)
        {
            v = transform.rotation * Vector3.forward * vert;
        }
        if (horiz != 0)
        {
            v += transform.rotation * Vector3.right * horiz;
        }
        if (v != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                playerNode.rigidbody.velocity = playerNode.rigidbody.velocity + v * 1f;
            }
            else
            {
                playerNode.gameobject.transform.position = playerNode.gameobject.transform.position + v;
            }
        }
        if (playerNode.gameobject != gameObject) transform.position = playerNode.gameobject.transform.position;
        //bool? l = Input.GetKeyDown(KeyCode.Mouse1) ? true : (Input.GetKeyUp(KeyCode.Mouse1) ? false : (bool?)null);
        //if (l != null) Screen.lockCursor = (bool)l;

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector3 mouseDelta = Input.mousePosition - oldMouse;
            Vector3 r = new Vector3(-mouseDelta.y, mouseDelta.x, 0) * lookSensitivity;
            transform.Rotate(r);
        }
        oldMouse = Input.mousePosition;
	}
    Vector3 oldMouse = Vector3.zero;
}
