using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager sharedInstance;

    public int currentLevel = 1;
    public int maxLevels = 3;
    public int selectedLevel = 1;

    private HashSet<int> unlockedLevels = new HashSet<int> { 1 };

    public bool isTutorial = false;

    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetLevel(int level)
    {
        if (IsLevelUnlocked(level))
        {
            selectedLevel = level;
            Debug.Log($"Mission {level} selected.");
        }
        else
        {
            Debug.LogWarning($"Mission {level} cannot be selected because it is not unlocked.");
        }
    }

    public void StartSelectedLevel()
    {
        if (IsLevelUnlocked(selectedLevel))
        {
            currentLevel = selectedLevel;
            Debug.Log($"Starting mission {currentLevel}.");
        }
        else
        {
            Debug.LogWarning($"Cannot start mission {selectedLevel} because it is not unlocked.");
        }

    }

    public void NextLevel()
    {
        isTutorial = false;

        if (currentLevel < maxLevels)
        {
            currentLevel++;
            UnlockLevel(currentLevel);
            Debug.Log($"Advanced to next mission: {currentLevel}.");

            if (currentLevel > 1)
            {
                ResetLevelState(); // Switch off tutorial mode for all future levels
                InventoryManager.instance.ResetInventoryState();
                //InventoryManager.instance.ClearInventory();
                ResetDraggableItems();
            }

            Debug.Log($"Advanced to next mission: {currentLevel}.");
        }
        else
        {
            Debug.LogWarning("No more missions to unlock. Reached maxMissions.");
        }
    }


    public void ResetLevelState()
    {
        isTutorial = false;
    }


    public void UnlockLevel(int level)
    {
        if (level > 0 && level <= maxLevels)
        {
            if (!unlockedLevels.Contains(level))
            {
                unlockedLevels.Add(level);
                Debug.Log($"Mission {level} unlocked.");
            }
        }
        else
        {
            Debug.LogWarning($"Mission {level} is out of range (1 to {maxLevels}).");
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        return unlockedLevels.Contains(level);
    }

    public List<int> GetUnlockedLevels()
    {
        return new List<int>(unlockedLevels);
    }

    public void ResetDraggableItems()
    {
        DraggableItem[] draggableItems = FindObjectsOfType<DraggableItem>();

        foreach (DraggableItem item in draggableItems)
        {
            Destroy(item.gameObject);  
        }

        Debug.Log("Old draggable items fully destroyed!");
    }

}
