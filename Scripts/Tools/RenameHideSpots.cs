using UnityEngine;
using UnityEditor;

public class RenameHideSpots : EditorWindow
{
    [MenuItem("Tools/Rename Hide Spots")]
    static void RenameChildren()
    {
        // Define mapping of spot IDs to descriptions
        var spotDescriptions = new System.Collections.Generic.Dictionary<string, string>
        {
            { "hide_spot_5", "Behind cafe bar counter" },
            { "hide_spot_6", "Behind cafe bar counter" },
            { "hide_spot_7", "Behind cafe bar counter" },
            { "hide_spot_10", "Behind cafe bar counter" },
            { "hide_spot_11", "Behind cafe bar counter" },
            { "hide_spot_12", "Behind cafe bar counter" },
            { "hide_spot_77", "in between tables and chairs" },
            { "hide_spot_8", "in between tables and chairs" },
            { "hide_spot_13", "in between tables and chairs" },
            { "hide_spot_9", "in between tables and chairs" },
            { "hide_spot_15", "Room corner" },
            { "hide_spot_14", "Room corner" },
            { "hide_spot_18", "between chairs" },
            { "hide_spot_17", "behind wall corner, but in plain sight" },
            { "hide_spot_69", "behind kitchen counter" },
            { "hide_spot_70", "behind kitchen counter" },
            { "hide_spot_71", "hugging hallway wall but in plain sight" },
            { "hide_spot_72", "hugging hallway wall but in plain sight" },
            
        };

        GameObject parent = GameObject.Find("Hide Spots");
        
        if (parent == null)
        {
            Debug.LogError("Could not find 'Hide Spots' GameObject!");
            return;
        }

        Vector3 parentOriginalPosition = parent.transform.position;
        
        Transform[] children = new Transform[parent.transform.childCount];
        Vector3[] worldPositions = new Vector3[parent.transform.childCount];
        
        int childIndex = 0;
        foreach (Transform child in parent.transform)
        {
            children[childIndex] = child;
            worldPositions[childIndex] = child.position;
            childIndex++;
        }
        
        parent.transform.position = Vector3.zero;
        
        int index = 1;
        for (int i = 0; i < children.Length; i++)
        {
            children[i].position = worldPositions[i];
            
            string spotName = $"HideSpot_{index}";
            string spotID = $"hide_spot_{index}";
            children[i].gameObject.name = spotName;
            
            InterestPoint point = children[i].gameObject.GetComponent<InterestPoint>();
            if (point == null)
            {
                point = children[i].gameObject.AddComponent<InterestPoint>();
            }
            
            point.id = spotID;
            point.type = InterestPointType.HideSpot;
            
            if (spotDescriptions.TryGetValue(spotID, out string description))
            {
                point.description = description;
            }
            else
            {
                point.description = "in between tables and chairs";
            }
            
            index++;
        }
        
        Debug.Log($"Renamed and configured {index-1} hide spots successfully!");
        Debug.Log($"Parent 'Hide Spots' moved to origin (0,0,0) while preserving hide spot world positions.");
    }
}