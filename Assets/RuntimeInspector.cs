using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RuntimeInspector : MonoBehaviour {
    public Font font;
    RectTransform containerRect;
	void Start () {
        containerRect = GetComponent<RectTransform>();

        float width = 150, height = 30, padding = 0;

        for (int i = 0; i < 10; i++)
        {
            GameObject labelGO = new GameObject("label" + i);
            labelGO.transform.SetParent(transform, false);
            Text text = labelGO.AddComponent<Text>();
            text.font = font;
            text.fontSize = 20;
            text.text = "Test text";

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.localScale = new Vector3(3, 1, 1);
            float x = -containerRect.rect.width / 2f + containerRect.rect.width / 5f;
            float y = containerRect.rect.height / 2f - containerRect.rect.height / 10f - height * i - padding * i;
            labelRect.offsetMin = new Vector2(x, y);
            labelRect.offsetMax = new Vector2(x + width, y + height);
        }
	}
	
	void Update () {
	    
	}
}
