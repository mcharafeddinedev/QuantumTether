using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that holds all available upgrades for the roguelike system
/// </summary>
[CreateAssetMenu(fileName = "UpgradeLibrary", menuName = "Game/Upgrade Library")]
public class UpgradeLibrary : ScriptableObject
{
    [Header("Upgrade Pool")]
    [SerializeField, Tooltip("All available upgrades")]
    private List<RunUpgrade> allUpgrades = new List<RunUpgrade>();
    
    
    /// <summary>
    /// Gets all upgrades from the library
    /// </summary>
    public List<RunUpgrade> GetAllUpgrades()
    {
        return new List<RunUpgrade>(allUpgrades);
    }
    
    /// <summary>
    /// Gets upgrades by category
    /// </summary>
    public List<RunUpgrade> GetUpgradesByCategory(UpgradeCategory category)
    {
        return allUpgrades.FindAll(upgrade => upgrade.category == category);
    }
    
    /// <summary>
    /// Gets a random selection of upgrades (for upgrade panel)
    /// </summary>
    public List<RunUpgrade> GetRandomUpgrades(int count, List<string> excludeIds = null)
    {
        List<RunUpgrade> availableUpgrades = new List<RunUpgrade>(allUpgrades);
        
        // Remove excluded upgrades
        if (excludeIds != null && excludeIds.Count > 0)
        {
            availableUpgrades.RemoveAll(upgrade => excludeIds.Contains(upgrade.id));
        }
        
        // Shuffle and take the requested count
        List<RunUpgrade> result = new List<RunUpgrade>();
        int takeCount = Mathf.Min(count, availableUpgrades.Count);
        
        for (int i = 0; i < takeCount; i++)
        {
            int randomIndex = Random.Range(0, availableUpgrades.Count);
            result.Add(availableUpgrades[randomIndex]);
            availableUpgrades.RemoveAt(randomIndex);
        }
        
        return result;
    }
    
    /// <summary>
    /// Gets upgrade by ID
    /// </summary>
    public RunUpgrade GetUpgradeById(string id)
    {
        return allUpgrades.Find(upgrade => upgrade.id == id);
    }
}
