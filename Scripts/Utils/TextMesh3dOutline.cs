using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
public class TextMesh3DOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.black;
    public float outlineWidth = 0.004f;
    
    private TextMesh originalTextMesh;
    private GameObject[] outlineObjects;
    private TextMesh[] outlineTextMeshes;
    
    void Start()
    {
        originalTextMesh = GetComponent<TextMesh>();
        CreateOutline();
    }
    
    void Update()
    {
        // Update outline text if original text changes
        if (outlineTextMeshes != null && outlineTextMeshes.Length > 0)
        {
            for (int i = 0; i < outlineTextMeshes.Length; i++)
            {
                if (outlineTextMeshes[i].text != originalTextMesh.text)
                {
                    outlineTextMeshes[i].text = originalTextMesh.text;
                }
            }
        }
    }
    
    void CreateOutline()
    {
        // Create 8 directions for the outline
        Vector3[] directions = new Vector3[]
        {
            new Vector3(outlineWidth, 0, 0),
            new Vector3(-outlineWidth, 0, 0),
            new Vector3(0, outlineWidth, 0),
            new Vector3(0, -outlineWidth, 0),
            new Vector3(outlineWidth, outlineWidth, 0),
            new Vector3(-outlineWidth, outlineWidth, 0),
            new Vector3(outlineWidth, -outlineWidth, 0),
            new Vector3(-outlineWidth, -outlineWidth, 0)
        };
        
        outlineObjects = new GameObject[directions.Length];
        outlineTextMeshes = new TextMesh[directions.Length];
        
        for (int i = 0; i < directions.Length; i++)
        {
            // Create outline object
            outlineObjects[i] = new GameObject("Outline " + i);
            outlineObjects[i].transform.parent = transform;
            outlineObjects[i].transform.localPosition = directions[i];
            outlineObjects[i].transform.localRotation = Quaternion.identity;
            outlineObjects[i].transform.localScale = Vector3.one;
            
            // Add TextMesh component
            outlineTextMeshes[i] = outlineObjects[i].AddComponent<TextMesh>();
            
            // Copy properties from original TextMesh
            outlineTextMeshes[i].text = originalTextMesh.text;
            outlineTextMeshes[i].font = originalTextMesh.font;
            outlineTextMeshes[i].fontSize = originalTextMesh.fontSize;
            outlineTextMeshes[i].fontStyle = originalTextMesh.fontStyle;
            outlineTextMeshes[i].alignment = originalTextMesh.alignment;
            outlineTextMeshes[i].anchor = originalTextMesh.anchor;
            outlineTextMeshes[i].characterSize = originalTextMesh.characterSize;
            outlineTextMeshes[i].lineSpacing = originalTextMesh.lineSpacing;
            outlineTextMeshes[i].tabSize = originalTextMesh.tabSize;
            outlineTextMeshes[i].richText = originalTextMesh.richText;
            outlineTextMeshes[i].offsetZ = -0.01f; // Place slightly behind original text
            
            // Set outline color
            outlineTextMeshes[i].color = outlineColor;
            
            // Make sure the outline renders behind the original text
            MeshRenderer renderer = outlineTextMeshes[i].GetComponent<MeshRenderer>();
            renderer.material = originalTextMesh.GetComponent<MeshRenderer>().material;
            renderer.sortingOrder = originalTextMesh.GetComponent<MeshRenderer>().sortingOrder - 1;
        }
    }
    
    public void UpdateOutlineColor(Color newColor)
    {
        outlineColor = newColor;
        if (outlineTextMeshes != null)
        {
            for (int i = 0; i < outlineTextMeshes.Length; i++)
            {
                outlineTextMeshes[i].color = outlineColor;
            }
        }
    }
    
    public void UpdateOutlineWidth(float newWidth)
    {
        if (Mathf.Approximately(outlineWidth, newWidth)) return;
        
        outlineWidth = newWidth;
        // Recreate the outline with new width
        for (int i = 0; i < outlineObjects.Length; i++)
        {
            Destroy(outlineObjects[i]);
        }
        CreateOutline();
    }
}