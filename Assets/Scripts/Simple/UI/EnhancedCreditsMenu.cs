using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Credits menu controller with back button functionality - the improved version
/// </summary>
public class EnhancedCreditsMenu : MonoBehaviour
{
    [Header("Credits UI")]
    [SerializeField, Tooltip("Credits text content (Text or TextMeshPro)")]
    private Text creditsText;
    
    [SerializeField, Tooltip("Credits text content (TextMeshPro)")]
    private TextMeshProUGUI creditsTextMeshPro;
    
    [SerializeField, Tooltip("Back button")]
    private Button backButton;
    
    [Header("Credits Content")]
    [SerializeField, TextArea(10, 20), Tooltip("Credits text to display")]
    private string creditsContent = @"GAME TITLE
Out of Time

DEVELOPMENT TEAM
Lead Developer: [Your Name]
Game Designer: [Team Member]
Artist: [Team Member]
Sound Designer: [Team Member]

SPECIAL THANKS
Unity Technologies
Game Jam Community
Playtesters and Friends

TECHNICAL CREDITS
Built with Unity 6
C# Scripting
2D Graphics

© 2024 [Your Studio Name]
All Rights Reserved

Thank you for playing!";

    void Start()
    {
        SetupCredits();
        SetupButton();
    }
    
    void SetupCredits()
    {
        if (creditsText != null)
        {
            creditsText.text = creditsContent;
        }
        
        if (creditsTextMeshPro != null)
        {
            creditsTextMeshPro.text = creditsContent;
        }
    }
    
    void SetupButton()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
    }
    
    public void GoBack()
    {
        
        // Find the main menu controller and show main menu
        EnhancedMainMenu mainMenu = FindFirstObjectByType<EnhancedMainMenu>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        else
        {
        }
    }
    
    void OnDestroy()
    {
        if (backButton != null)
            backButton.onClick.RemoveListener(GoBack);
    }
}
