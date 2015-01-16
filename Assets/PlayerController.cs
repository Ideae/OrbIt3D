using UnityEngine;
using System.Collections;
using OrbItProcs;
public class PlayerController : MonoBehaviour {
    public float lookSensitivity = 0.5f;
    public bool followNode = true;
    public static PlayerController _player;
    public static PlayerController player { get { return _player; } }
	// Use this for initialization
	void Start () {
        //player = GameObject.Find("Player") ?? gameObject;
        playerNode = OrbIt.game.room.spawnNode(OrbIt.game.room.groups.player);
        _player = this;
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
            if (!followNode)
            {
                transform.position = transform.position + v;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                playerNode.rigidbody.velocity = playerNode.rigidbody.velocity + v * 1f;
            }
            else
            {
                playerNode.transform.position = playerNode.transform.position + v;
            }
        }
        if (followNode)
        {
            if (playerNode.gameobject != gameObject) transform.position = playerNode.transform.position;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerNode.rigidbody.velocity = Vector3.zero;
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            followNode = !followNode;
        }
        //bool? l = Input.GetKeyDown(KeyCode.Mouse1) ? true : (Input.GetKeyUp(KeyCode.Mouse1) ? false : (bool?)null);
        //if (l != null) Screen.lockCursor = (bool)l;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Node n = OrbIt.game.room.spawnNode(transform.position);
            Vector3 vel = transform.rotation * Vector3.forward * 10f;
            n.rigidbody.velocity = vel;
            SpawnNodes.SetRadius(n);
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            Vector3 mouseDelta = Input.mousePosition - oldMouse;
            Vector3 r = new Vector3(-mouseDelta.y, mouseDelta.x, 0) * lookSensitivity;
            transform.Rotate(r);
        }
        oldMouse = Input.mousePosition;
	}
    Vector3 oldMouse = Vector3.zero;
}
