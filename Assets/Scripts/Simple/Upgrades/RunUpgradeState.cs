using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which upgrades have been taken and their stack counts during a run
/// </summary>
public class RunUpgradeState
{
    private Dictionary<string, int> stacksById = new Dictionary<string, int>();
    
    /// <summary>
    /// Checks if an upgrade can be taken (hasn't reached max stacks)
    /// </summary>
    public bool CanTake(RunUpgrade upgrade, int currentScore)
    {
        if (upgrade == null) return false;
        if (currentScore < upgrade.cost) return false;
        
        int currentStacks = GetStackCount(upgrade.id);
        return currentStacks < upgrade.maxStacks;
    }
    
    /// <summary>
    /// Adds a stack of an upgrade
    /// </summary>
    public void AddStack(string upgradeId)
    {
        if (stacksById.ContainsKey(upgradeId))
        {
            stacksById[upgradeId]++;
        }
        else
        {
            stacksById[upgradeId] = 1;
        }
    }
    
    /// <summary>
    /// Gets the current stack count for an upgrade
    /// </summary>
    public int GetStackCount(string upgradeId)
    {
        return stacksById.ContainsKey(upgradeId) ? stacksById[upgradeId] : 0;
    }
    
    /// <summary>
    /// Gets all taken upgrade IDs
    /// </summary>
    public List<string> GetTakenUpgradeIds()
    {
        return new List<string>(stacksById.Keys);
    }
    
    /// <summary>
    /// Resets state for new run
    /// </summary>
    public void Reset()
    {
        stacksById.Clear();
    }
    
    /// <summary>
    /// Gets total number of upgrades taken
    /// </summary>
    public int GetTotalUpgradesTaken()
    {
        int total = 0;
        foreach (var kvp in stacksById)
        {
            total += kvp.Value;
        }
        return total;
    }
}
