using UnityEngine;
using UnityEditor;
using System.Collections;
using OrbItProcs;
using System.Reflection;
using System;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;
public class OrbItInspector : EditorWindow 
{
    public static EditorWindow orbitInspector;
    public bool AutoRefresh = false;
    public Dictionary<Tuple<object, int>, bool> foldoutToggles = new Dictionary<Tuple<object, int>, bool>();
    private GameObject oldGameObject = null;

    [MenuItem ("Window/OrbItInspector")]
    public static void ShowWindow() 
    {
        orbitInspector = EditorWindow.GetWindow(typeof(OrbItInspector));
        //EditorWindow.GetWindowWithRect(typeof(OrbItInspector), new Rect(0, 0, 500, 500));
    }
    void OnEnable() { }
    void OnInspectorUpdate()
    {
        if (AutoRefresh)
        {
            Repaint();
        }
    }
    
    void OnGUI()
    {
        GameObject gameObject = Selection.activeGameObject;
        if (gameObject != oldGameObject)
        {
            foldoutToggles = new Dictionary<Tuple<object, int>, bool>();
        }
        if (gameObject != null)
        {
            //ShowNodeComponents(gameObject);
            ShowGameObjectScripts(gameObject);
        }
        oldGameObject = gameObject;
    }
    void ShowNodeComponents(GameObject gameObject)
    {
        var nodescript = gameObject.GetComponent<NodeScript>();
        if (nodescript != null && nodescript.node != null)
        {
            Node node = nodescript.node;
            ShowObjectProperties("Body", node.body);
            if (node.comps != null)
            {
                foreach (Type t in node.comps.Keys)
                {
                    ShowObjectProperties(t.ToString().LastWord('.'), node.comps[t]);
                }
            }
        }
    }
    void ShowGameObjectScripts(GameObject gameObject)
    {
        MonoBehaviour[] comps = gameObject.GetComponents<MonoBehaviour>();
        if (comps.Length > 0)
        {
            foreach(MonoBehaviour mono in comps)
            {
                string scriptname = mono.GetType().ToString().LastWord('.');
                ShowObjectProperties(scriptname, mono);
            }
        }
    }
    void ShowObjectProperties(string name, object obj)
    {
        Type type = obj.GetType();
        
        Tuple<object, int> key = new Tuple<object, int>(obj, EditorGUI.indentLevel);
        if (!foldoutToggles.ContainsKey(key)) foldoutToggles[key] = false;

        foldoutToggles[key] = EditorGUILayout.Foldout(foldoutToggles[key], name);
        if (foldoutToggles[key])
        {
            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;
            var fields = type.GetFields(flags);
            if (obj is OrbItProcs.Component)
            {
                flags ^= BindingFlags.DeclaredOnly;
            }
            var props = type.GetProperties(flags);
            if (props.Length > 0 || fields.Length > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var prop in props)
                {
                    FPInfo fpinfo = new FPInfo(prop);
                    ShowPropertyControl(obj, fpinfo);
                }
                foreach (var field in fields)
                {
                    FPInfo fpinfo = new FPInfo(field);
                    ShowPropertyControl(obj, fpinfo);
                }
                EditorGUI.indentLevel--;
            }
        }
    }
    void ShowPropertyControl(object obj, FPInfo fpinfo)
    {
        Type ptype = fpinfo.FPType;
        object val = fpinfo.GetValue(obj);
        if (val == null) return;
        if (ptype == typeof(int))
        {
            int ret = EditorGUILayout.IntField(fpinfo.Name, (int)val);
            if (ret != (int)val) fpinfo.SetValue(obj, ret);
        }
        else if (ptype == typeof(float))
        {
            float ret = EditorGUILayout.FloatField(fpinfo.Name, (float)val);
            if (ret != (float)val) 
                fpinfo.SetValue(obj, ret);
        }
        else if (ptype == typeof(bool))
        {
            bool ret = EditorGUILayout.Toggle(fpinfo.Name, (bool)val);
            if (ret != (bool)val) fpinfo.SetValue(obj, ret);
        }
        else if (ptype == typeof(Vector2))
        {
            Vector2 ret = EditorGUILayout.Vector2Field(fpinfo.Name, (Vector2)val);
            if (ret != (Vector2)val) fpinfo.SetValue(obj, ret);
        }
        else if (ptype == typeof(Vector3))
        {
            Vector3 ret = EditorGUILayout.Vector3Field(fpinfo.Name, (Vector3)val);
            if (ret != (Vector3)val) fpinfo.SetValue(obj, ret);
        }
        else if (ptype == typeof(string))
        {
            string ret = EditorGUILayout.TextField(fpinfo.Name, (string)val);
            if (ret != (string)val) fpinfo.SetValue(obj, ret);
        }
        else if (ptype.IsEnum)
        {
            Enum e = (Enum) val;
            object enumval = EditorGUILayout.EnumPopup(fpinfo.Name, e);
            if (!e.Equals(enumval))
            {
                fpinfo.SetValue(enumval, obj);
            }
        }
        else if (ptype.IsClass)
        {
            string name = fpinfo.Name;
            ShowObjectProperties(name, val);
        }
    }
}
