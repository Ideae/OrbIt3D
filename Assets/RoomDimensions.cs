﻿using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class RoomDimensions : MonoBehaviour {

    public int width, height;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    transform.localScale = new Vector3(width, height, 0.01f) * 100; // 0.001?
        OrbIt.Width = width;
        OrbIt.Height = height;
        
	}
}
