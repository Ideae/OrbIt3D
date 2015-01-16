using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class RuntimeInspector : MonoBehaviour {
    public Font font;
    RectTransform containerRect;
	void Start () {
        containerRect = GetComponent<RectTransform>();

        float width = 100, height = 20, padding = 5;

        for (int i = 0; i < 3; i++)
        {
            GameObject labelGO = new GameObject("label" + i);
            labelGO.transform.SetParent(transform, false);
            Text text = labelGO.AddComponent<Text>();
            text.font = font;
            text.fontSize = 15;
            text.text = "Test text";

            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.localScale = new Vector3(1, 1, 1);
            float x = -containerRect.rect.width / 2f + containerRect.rect.width / 5f;
            float y = containerRect.rect.height / 2f - containerRect.rect.height / 5f - height * i - padding * i;
            labelRect.offsetMin = new Vector2(x, y);
            labelRect.offsetMax = new Vector2(x + width, y + height);
        }

	}
	
	void Update () {
	    
	}
}
