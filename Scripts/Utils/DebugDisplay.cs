using UnityEngine;

public class DebugDisplay : MonoBehaviour
{
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Vector2 position = new Vector2(10, 10);
    [SerializeField] private int fontSize = 14;
    
    private GUIStyle style;
    
    private void Start()
    {
        style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = Color.white;
        style.richText = true;
    }
    
    private void OnGUI()
    {
        if (showDebugInfo)
        {
            GUI.Label(new Rect(position.x, position.y, 300, 200), OpenAIUtils.GetTokenUsageInfo(), style);
        }
    }
}