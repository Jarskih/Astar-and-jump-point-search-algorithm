using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public GameObject guiText;
    private List<TextMeshProUGUI> meshes = new List<TextMeshProUGUI>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddText(GameObject go, float gCost, int hCost, float fCost)
    {
        CreateText(gCost.ToString(), go, TextAlignmentOptions.Left);
        CreateText(hCost.ToString(), go, TextAlignmentOptions.Right);
    }

    void CreateText(string text, GameObject go, TextAlignmentOptions alignment)
    {
        var mesh = Instantiate(guiText, Vector3.zero, Quaternion.identity);
        
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint (Camera.main, go.transform.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle (mesh.GetComponent<RectTransform>(), screenPos, null, out var localPos);
 
        mesh.GetComponent<RectTransform>().anchoredPosition = localPos;

        mesh.transform.SetParent(this.transform);

        var textMesh = mesh.GetComponent<TextMeshProUGUI>();
        textMesh.text = text;
        textMesh.fontSize = 12;
        textMesh.alignment = alignment;
        meshes.Add(textMesh);
    }

    public void Reset()
    {
        foreach (var m in meshes)
        {
            m.text = "";
        }

        meshes.Clear();
    }
}
