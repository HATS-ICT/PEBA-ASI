using UnityEngine;
using UnityEditor;

public class MissingScriptCleaner : MonoBehaviour
{
    [MenuItem("Tools/Cleanup Missing Scripts")]
    private static void Cleanup()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            count += removed;
        }

        Debug.Log($"Removed {count} missing script references.");
    }
}
