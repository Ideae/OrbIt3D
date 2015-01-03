using UnityEngine;
using System.Collections;
using OrbItProcs;
public class NodeScript : MonoBehaviour {
    public Node node;
    public void InitNodeScript(Node node)
    {
        this.node = node;
    }
    public static void AddNodeScript(GameObject gameObject, Node node)
    {
        NodeScript script = gameObject.AddComponent<NodeScript>();
        script.InitNodeScript(node);
    }
}
