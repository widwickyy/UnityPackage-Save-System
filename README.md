# Lightweight Save System

A simple, extensible save system for Unity built on top of Newtonsoft JSON.

## Features

- **Easy to use** - Static facade for quick access
- **Extensible** - Swappable serialization and storage backends
- **Versioned saves** - Built-in version field for future migrations
- **Type-safe** - Generic methods with compile-time type checking
- **JSON based** - Human-readable save files

## Installation

### Via Unity Package Manager

1. Open your Unity project
2. Navigate to `Window > Package Manager`
3. Click the `+` button in the top-left corner
4. Select `Add package from git URL`
5. Enter: `https://github.com/widwickyy/unity-save-system.git#1.0.0`

### Via Unity Package Manager (Local)

1. Download or clone this repository
2. Open your Unity project
3. Navigate to `Window > Package Manager`
4. Click the `+` button
5. Select `Add package from disk`
6. Navigate to the folder containing `package.json`

### Via Assembly Definition (Development)

For development purposes, add this repository as a subfolder in your Assets folder.

## Quick Start

```csharp
using UnityEngine;
using SaveSystem;

// Initialize the save system (call once at app start)
SaveManager.Initialize();

public class PlayerData
{
    public string playerName;
    public int level;
    public float health;
}

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        // Save data
        var playerData = new PlayerData
        {
            playerName = "Hero",
            level = 5,
            health = 100f
        };
        SaveManager.Save("player", playerData);

        // Load data
        var loadedData = SaveManager.Load<PlayerData>("player");
        Debug.Log($"Welcome back, {loadedData.playerName}!");
    }
}
```

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        SaveManager                              │
│                    (Static Facade)                              │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     SaveSystem                                  │
│              (ISaveSystem Implementation)                       │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │              SaveWrapper<T>                             │    │
│  │   - version (int)                                       │    │
│  │   - data (T)                                            │    │
│  └─────────────────────────────────────────────────────────┘    │
│                          │                                      │
│          ┌───────────────┴───────────────┐                      │
│          ▼                               ▼                      │
│  ┌──────────────────┐         ┌──────────────────┐              │
│  │   ISerializer    │         │    IStorage      │              │
└──┼──────────────────┼─────────┼──────────────────┼──────────────┘
   ▼                  ▼         ▼                  ▼
  ┌─────────────────────┐       ┌──────────────────┐
  │ NewtonsoftSerializer│       │   FileStorage    │
  └─────────────────────┘       └──────────────────┘
```

## API Reference

### SaveManager

Static facade providing global access to the save system.

```csharp
// Initialize the save system (call once at startup)
SaveManager.Initialize();

// Save data
SaveManager.Save<T>(string key, T data);

// Load data (returns default(T) if not found)
T result = SaveManager.Load<T>(string key);

// Check if a save exists
bool exists = SaveManager.Exists(string key);

// Delete a save
SaveManager.Delete(string key);
```

### ISaveSystem Interface

For dependency injection and custom implementations:

```csharp
public interface ISaveSystem
{
    void Save<T>(string key, T data);
    T Load<T>(string key);
    bool Exists(string key);
    void Delete(string key);
}
```

### ISerializer Interface

Implement your own serializer:

```csharp
public interface ISerializer
{
    string Serialize<T>(T data);
    T Deserialize<T>(string json);
}
```

### IStorage Interface

Implement your own storage backend:

```csharp
public interface IStorage
{
    void Write(string key, string data);
    string Read(string key);
    bool Exists(string key);
    void Delete(string key);
}
```

## Advanced Usage

### Custom Serializer

```csharp
using Newtonsoft.Json;
using UnityEngine;

public class CustomSerializer : ISerializer
{
    public string Serialize<T>(T data)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
```

### Custom Storage

```csharp
using System.IO;
using UnityEngine;

public class PlayerPrefsStorage : IStorage
{
    public void Write(string key, string data)
    {
        PlayerPrefs.SetString(key, data);
        PlayerPrefs.Save();
    }

    public string Read(string key)
    {
        return PlayerPrefs.GetString(key, null);
    }

    public bool Exists(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public void Delete(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }
}
```

### Using Dependency Injection

```csharp
public class CustomSaveManager : MonoBehaviour
{
    [SerializeField] private SaveSystem _saveSystem;

    private void Start()
    {
        // Use a custom serializer and storage
        var serializer = new CustomSerializer();
        var storage = new PlayerPrefsStorage();

        _saveSystem = new SaveSystem(serializer, storage, version: 1);

        // Now use _saveSystem directly
        _saveSystem.Save("player", new PlayerData { playerName = "Hero" });
    }
}
```

## Versioned Saves

The `SaveWrapper<T>` class automatically wraps your data with version information:

```csharp
[System.Serializable]
public class SaveWrapper<T>
{
    public int version;
    public T data;
}
```

This allows for future save data migrations when your data format changes:

```csharp
public void MigrateData(string key)
{
    var wrapper = SaveManager.Load<SaveWrapper<LegacyPlayerData>>(key);

    switch (wrapper.version)
    {
        case 1:
            // Migrate from version 1 to current
            var migrated = new PlayerData
            {
                playerName = wrapper.data.name,
                level = wrapper.data.level,
                health = wrapper.data.health,
                newField = "default" // New field added in v2
            };
            SaveManager.Save(key, migrated);
            break;
        // Add more cases as versions increase
    }
}
```

## Save File Location

FileStorage saves to Unity's `Application.persistentDataPath`:

| Platform | Location                                                           |
| -------- | ------------------------------------------------------------------ |
| Windows  | `%APPDATA%/../Local/Unity/[CompanyName]/[ProductName]/`            |
| macOS    | `~/Library/Application Support/Unity/[CompanyName]/[ProductName]/` |
| Linux    | `~/.config/unity3d/[CompanyName]/[ProductName]/`                   |
| iOS      | `/Documents/`                                                      |
| Android  | `/data/data/[PackageName]/files/`                                  |

Files are saved with `.json` extension.

## Demo Scene

A demo scene is included demonstrating:

- Saving and loading simple data
- Saving and loading complex objects
- Checking for save existence
- Deleting saves

See the `Demo/` folder for the demo scene and example scripts.

## Requirements

- Unity 2021.3 or later
- Newtonsoft.JSON (via Unity Package Manager: `com.unity.nuget.newtonsoft-json`)

## Installation

1. Add Newtonsoft.JSON via Unity Package Manager:
   - Open `Window > Package Manager`
   - Click `+` > `Add package from git URL`
   - Enter: `com.unity.nuget.newtonsoft-json`
2. Add this save system package using your preferred method above.

## License

MIT License - feel free to use this in any project.
