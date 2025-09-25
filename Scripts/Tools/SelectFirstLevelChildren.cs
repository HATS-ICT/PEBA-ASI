using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SelectFirstLevelChildren : EditorWindow
{
    [MenuItem("Tools/Select First-Level Children")]
    static void SelectChildren()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("No parent GameObject selected!");
            return;
        }

        Transform parentTransform = Selection.activeGameObject.transform;
        List<GameObject> children = new List<GameObject>();

        foreach (Transform child in parentTransform)
        {
            children.Add(child.gameObject);
        }

        if (children.Count > 0)
        {
            Selection.objects = children.ToArray();
            Debug.Log("Selected " + children.Count + " first-level children.");
        }
        else
        {
            Debug.LogWarning("No first-level children found.");
        }
    }
}
