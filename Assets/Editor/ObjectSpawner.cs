using UnityEngine;
using UnityEditor;
using System.Collections;

public class ObjectSpawner : EditorWindow {
    static EditorWindow objectSpawner;
    static bool AutoRefresh = true;
    Vector3 mousePos = Vector3.zero;
    [MenuItem("Window/ObjectSpawner")]
    public static void ShowWindow()
    {
        objectSpawner = EditorWindow.GetWindow(typeof(ObjectSpawner));
    }
    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += SceneGUI;
    }
    void OnInspectorUpdate()
    {
        if (AutoRefresh)
        {
            Repaint();
        }
    }
    bool clicked = false;
    void SceneGUI(SceneView sceneView)
    {
        Event ev = Event.current;
        mousePos = ev.mousePosition;
        if (ev.type == EventType.MouseDown && ev.button == 0)
        {
            clicked = true;
        }
        if (ev.type == EventType.Used && ev.button == 0)
        {
            clicked = false;
        }
    }
    int count = 0;
    public enum tools
    {
        placeWall,
        placeBlock,
        placeKey,
        placePlayer,

    }
    public tools selectedTool = tools.placeWall;
    void OnGUI()
    {
        Vector2 mouse = mousePos;
        EditorGUILayout.Vector2Field("Mouse:", mouse);
        tools newtool = (tools)EditorGUILayout.EnumPopup("Select Tool:", selectedTool);
        if (selectedTool != newtool)
        {
            selectedTool = newtool;
        }
    }

}
