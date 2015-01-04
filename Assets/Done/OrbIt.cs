using UnityEngine;
using System.Collections;
using System;
using OrbItProcs;
using System.Collections.ObjectModel;

public class OrbIt : MonoBehaviour {

    #region ///////////////////// FIELDS ///////////////////
    public static OrbIt game;
    public static Assets assets;
    public Camera camera;
    //public static UserInterface ui;
    //public static GameTime gametime;
    public static bool soundEnabled = false;
    public static bool isFullScreen = false;
    public KeyManager keyManager;
    #endregion

    #region ///////////////////// PROPERTIES ///////////////////
    public static float Width { get; set; }
    public static float Height { get; set; }
    public static float Depth { get; set; }
    public static Vector3 origin { get { return roomIndicator.position; } set { roomIndicator.position = value; } }
    
    private static Transform roomIndicator;

    public static GameMode globalGameMode{get;set;}
    public Room room { get; set; }
    #endregion

    #region ///////////////////// EVENTS ///////////////////
    public static Action OnUpdate;
    #endregion

    void Awake()
    {
        game = this;
        roomIndicator = transform.FindChild("Room");
        Width = roomIndicator.GetComponent<RoomDimensions>().width;
        Height = roomIndicator.GetComponent<RoomDimensions>().height;
        Depth = roomIndicator.GetComponent<RoomDimensions>().depth;

    }

	// Use this for initialization
	void Start () {

#if UNITY_EDITOR
        if (!Application.isPlaying) return; //or whatever script needs to be run when in edition mode.
#endif
        room = new Room(this, new Vector3(Width, Height, Depth));
        keyManager = new KeyManager();

        globalGameMode = new GameMode(this);
        room.attatchToSidebar();

        Player.CreatePlayers(room);
//         ui = UserInterface.Start();
//         ui.Initialize();
        //(ui);
//         GlobalKeyBinds(ui);

	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
        if (!Application.isPlaying) return; //or whatever script needs to be run when in edition mode.
#endif
        //if (IsActive) ui.Update(gameTime);
        keyManager.Update();

        //if (!ui.IsPaused)
            room.Update();
        //else if (redrawWhenPaused) room.drawOnly();

        if (OnUpdate != null) OnUpdate.Invoke();
	}
    void OnGUI()
    {
        //room.camera.Work();
    }
}
