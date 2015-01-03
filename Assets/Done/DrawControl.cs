using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using OrbItProcs;

namespace OrbItProcs
{
    public class DrawControl: IEnumerable<GameObject>
    {
        private Dictionary<string, GameObject> sprites;
        private Component component;
        public DrawControl(Component c)
        {
            component = c;
            sprites = new Dictionary<string, GameObject>();
        }
        public GameObject addSprite(string name, textures? texture = null, Layers layer = Layers.Under1)
        {
            GameObject gameobject;
            gameobject = new GameObject(name, typeof(SpriteRenderer));
            if (texture != null)
                gameobject.GetComponent<SpriteRenderer>().sprite = Assets.textureDict[(textures)texture];
            //if (component.parent != null)
            //{
                gameobject.transform.parent = component.parent.gameobject.transform;
                gameobject.transform.localPosition = new Vector3(0, 0, (int)layer);
            //}
            sprites[name] = gameobject;
            return gameobject;
        }

        
        public GameObject getSprite(string name) { return sprites.ContainsKey(name) ? sprites[name] : null; }
        public SpriteRenderer getSpriteRenderer(string name) { return sprites[name].GetComponent<SpriteRenderer>(); }

        public void setSpriteTexture(string name, textures texture)
        {
            sprites[name].GetComponent<SpriteRenderer>().sprite = Assets.textureDict[texture];
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            foreach (GameObject g in sprites.Values) yield return g;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            // Lets call the generic version here
            return this.GetEnumerator();
        }

    }
    
}
public static class DrawControlUtils
{
    public static GameObject SetTexture(this GameObject go, textures texture)
    {
        go.SpriteRenderer().sprite = Assets.textureDict[texture];
        return go;
    }
    public static GameObject SetPosition(this GameObject go, Vector3 position)
    {
        go.transform.position = new Vector3(position.x, position.y, go.transform.position.z);
        return go;
    }
    public static GameObject SetLocalPosition(this GameObject go, Vector3 position)
    {
        go.transform.localPosition = position;
        return go;
    }
    public static GameObject SetRotation(this GameObject go, float radians)
    {
        go.transform.eulerAngles = new Vector3(0, 0, radians * Mathf.Rad2Deg);
        return go;
    }
    public static GameObject SetScale(this GameObject go, float scale)
    {
        go.transform.localScale = new Vector3(scale, scale, 1);
        return go;
    }
    public static GameObject SetLayer(this GameObject go, Layers layer)
    {
        go.transform.position = go.transform.position.SetZ((int)layer);
        return go;
    }
    public static GameObject SetColor(this GameObject go, Color color)
    {
        go.SpriteRenderer().color = color;
        return go;
    }
    public static SpriteRenderer SpriteRenderer(this GameObject go)
    {
        return go.GetComponent<SpriteRenderer>();
    }
}
