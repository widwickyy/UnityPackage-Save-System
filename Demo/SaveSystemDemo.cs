using UnityEngine;
using SaveSystem;

/// <summary>
/// Demo script showing basic usage of the SaveSystem
/// </summary>
public class SaveSystemDemo : MonoBehaviour
{
    [SerializeField] private string _saveKey = "demo_save";
    
    private void Start()
    {
        // Ensure the save system is initialized
        if (!SaveManager.IsInitialized)
        {
            SaveManager.Initialize();
        }
        
        RunDemo();
    }
    
    private void RunDemo()
    {
        Debug.Log("=== SaveSystem Demo ===");
        
        // 1. Save simple data
        Debug.Log("Saving simple data...");
        SaveManager.Save("int_test", 42);
        SaveManager.Save("string_test", "Hello Save System!");
        SaveManager.Save("float_test", 3.14f);
        
        // 2. Load simple data
        Debug.Log("Loading simple data...");
        int intValue = SaveManager.Load<int>("int_test");
        string stringValue = SaveManager.Load<string>("string_test");
        float floatValue = SaveManager.Load<float>("float_test");
        
        Debug.Log($"Loaded int: {intValue}");
        Debug.Log($"Loaded string: {stringValue}");
        Debug.Log($"Loaded float: {floatValue}");
        
        // 3. Save complex object
        Debug.Log("Saving complex object...");
        var playerData = new PlayerData
        {
            PlayerName = "Hero",
            Level = 10,
            Health = 85.5f,
            Inventory = new string[] { "Sword", "Shield", "Potion" },
            Position = new Vector3Data { X = 100, Y = 200, Z = 300 }
        };
        SaveManager.Save(_saveKey, playerData);
        
        // 4. Load complex object
        Debug.Log("Loading complex object...");
        PlayerData loadedPlayer = SaveManager.Load<PlayerData>(_saveKey);
        
        Debug.Log($"Player Name: {loadedPlayer.PlayerName}");
        Debug.Log($"Level: {loadedPlayer.Level}");
        Debug.Log($"Health: {loadedPlayer.Health}");
        Debug.Log($"Inventory: [{string.Join(", ", loadedPlayer.Inventory)}]");
        Debug.Log($"Position: ({loadedPlayer.Position.X}, {loadedPlayer.Position.Y}, {loadedPlayer.Position.Z})");
        
        // 5. Check existence
        Debug.Log($"Save exists: {SaveManager.Exists(_saveKey)}");
        Debug.Log($"Non-existent save: {SaveManager.Exists("nonexistent_key")}");
        
        // 6. Delete save
        Debug.Log("Deleting save...");
        SaveManager.Delete(_saveKey);
        Debug.Log($"Save exists after delete: {SaveManager.Exists(_saveKey)}");
        
        Debug.Log("=== Demo Complete ===");
    }
}

/// <summary>
/// Example player data class for demonstration
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string PlayerName;
    public int Level;
    public float Health;
    public string[] Inventory;
    public Vector3Data Position;
}

/// <summary>
/// Serializable wrapper for Vector3 since Unity's Vector3 is not serializable by default
/// </summary>
[System.Serializable]
public class Vector3Data
{
    public float X;
    public float Y;
    public float Z;
    
    public static Vector3Data FromVector3(Vector3 vector)
    {
        return new Vector3Data { X = vector.x, Y = vector.y, Z = vector.z };
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }
}
