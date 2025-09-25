using System.Collections;
using UnityEngine;

public class DialogVisualizationSystem : MonoBehaviour
{
    public TextMesh textBubble;
    public float textVisibilityRadius = 20f;
    public string CurrentDialogText { get; set; } = "";
    public AudioSource calmingDownSound;
    
    private GameObject textBackground;
    private Color backgroundColor = new Color(0, 0, 0, 0.5f);
    private Color textColor = Color.white;
    private float textBubblePadding = 0.1f;
    private float maxTextWidth = 2.0f;
    
    private VictimController controller;
    private CapsuleCollider capsule;
    
    public void Initialize(VictimController controller, CapsuleCollider capsule)
    {
        this.controller = controller;
        this.capsule = capsule;
        calmingDownSound = GetComponent<AudioSource>();
        
        SetupTextBubble();
        StartCoroutine(UpdateTextBubble());
    }
    
    private void SetupTextBubble()
    {
        if (textBubble == null)
        {
            // Create text bubble parent object
            GameObject textObj = new GameObject("TextBubble");
            textObj.transform.parent = transform;
            
            // Calculate height based on the capsule collider
            float textHeight = capsule.height + 1.0f; // Increased from 0.5f to 1.0f for higher positioning
            textObj.transform.localPosition = new Vector3(0, textHeight, 0);
            textObj.transform.localRotation = Quaternion.identity;
            
            // Create background quad
            textBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            textBackground.name = "TextBackground";
            textBackground.transform.parent = textObj.transform;
            textBackground.transform.localPosition = new Vector3(0, 0, 0.01f);  // Slightly behind text
            textBackground.transform.localRotation = Quaternion.identity;
            
            // Remove collider from background
            Destroy(textBackground.GetComponent<Collider>());
            
            // Set background material
            Renderer backgroundRenderer = textBackground.GetComponent<Renderer>();
            Material bgMaterial = new Material(Shader.Find("Transparent/Diffuse"));
            if (bgMaterial == null)
            {
                // Fallback to another common shader if the first one isn't available
                bgMaterial = new Material(Shader.Find("UI/Default"));
            }
            backgroundRenderer.material = bgMaterial;
            backgroundRenderer.material.color = backgroundColor;
            
            // Create text mesh
            textBubble = textObj.AddComponent<TextMesh>();
            textBubble.alignment = TextAlignment.Center;
            textBubble.anchor = TextAnchor.MiddleCenter;
            textBubble.fontSize = 24;
            textBubble.characterSize = 0.05f;
            textBubble.color = textColor;
            
            // Make sure text renders in front of background
            MeshRenderer textRenderer = textBubble.GetComponent<MeshRenderer>();
            textRenderer.sortingOrder = backgroundRenderer.sortingOrder + 1;
            
            // Add outline to text for better readability
            TextMesh3DOutline outline = textObj.AddComponent<TextMesh3DOutline>();
            if (outline != null)
            {
                outline.outlineColor = Color.black;
                outline.outlineWidth = 0.004f;
            }
        }
    }
    
    public void UpdateTextBubbleRotation()
    {
        if (textBubble != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - textBubble.transform.position;
            textBubble.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }
    
    private IEnumerator UpdateTextBubble()
    {
        while (!controller.healthSystem.isDead)
        {
            if (textBubble != null && textBackground != null)
            {
                float distanceToCamera = Vector3.Distance(transform.position, Camera.main.transform.position);
                if (distanceToCamera <= textVisibilityRadius && !string.IsNullOrEmpty(CurrentDialogText))
                {
                    // Format text with line breaks if needed
                    string formattedText = FormatTextWithLineBreaks(CurrentDialogText, maxTextWidth);
                    textBubble.text = formattedText;
                    
                    // Resize background to fit text
                    Bounds textBounds = CalculateTextBounds(textBubble);
                    Vector3 backgroundSize = new Vector3(
                        Mathf.Max(textBounds.size.x + textBubblePadding * 2, 1.0f),  // Ensure minimum width
                        Mathf.Max(textBounds.size.y + textBubblePadding * 2, 0.5f),  // Ensure minimum height
                        1
                    );
                    textBackground.transform.localScale = backgroundSize;
                    
                    // Show the text and background
                    textBubble.gameObject.SetActive(true);
                    textBackground.SetActive(true);
                }
                else
                {
                    // Hide the text and background
                    textBubble.text = "";
                    textBubble.gameObject.SetActive(false);
                    textBackground.SetActive(false);
                }
            }
            
            yield return new WaitForSeconds(0.1f);  // Small delay to avoid updating every frame
        }
        
        if (textBubble != null)
        {
            textBubble.text = "";
            textBubble.gameObject.SetActive(false);
            textBackground.SetActive(false);
        }
    }
    
    // Helper method to format text with line breaks
    private string FormatTextWithLineBreaks(string text, float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        // Force a much smaller number of characters per line to ensure wrapping
        int charsPerLine = 25;  // Fixed value that should work well for most text
        
        if (text.Length <= charsPerLine)
            return text;
        
        // Split text into words
        string[] words = text.Split(' ');
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        string currentLine = "";
        
        foreach (string word in words)
        {
            // Check if adding this word would exceed the line length
            if (currentLine.Length + word.Length + 1 > charsPerLine)
            {
                // Add current line to result and start a new line
                result.AppendLine(currentLine);
                currentLine = word;
            }
            else
            {
                // Add word to current line
                if (string.IsNullOrEmpty(currentLine))
                    currentLine = word;
                else
                    currentLine += " " + word;
            }
        }
        
        // Add the last line
        if (!string.IsNullOrEmpty(currentLine))
            result.Append(currentLine);
        
        return result.ToString();
    }
    
    // Helper method to calculate text bounds
    private Bounds CalculateTextBounds(TextMesh textMesh)
    {
        Bounds bounds = new Bounds();
        
        if (string.IsNullOrEmpty(textMesh.text))
            return bounds;
        
        // Count lines
        string[] lines = textMesh.text.Split('\n');
        int lineCount = lines.Length;
        
        // Find the longest line
        float maxLineLength = 0;
        foreach (string line in lines)
        {
            maxLineLength = Mathf.Max(maxLineLength, line.Length);
        }
        
        // Approximate width based on longest line
        float width = maxLineLength * textMesh.characterSize * 1.1f;  // Increased multiplier for width
        
        // Approximate height based on font size and line count
        float height = textMesh.characterSize * 1.7f * lineCount;  // Increased multiplier for height
        
        bounds.size = new Vector3(width, height, 0.1f);
        return bounds;
    }
} 