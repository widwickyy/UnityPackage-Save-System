# Lightweight Save System

A simple, extensible save system for Unity built on top of Newtonsoft JSON, under the `Widwickyy.SaveSystem` namespace.

## Features

- **Easy to use** - Static facade for quick access
- **Extensible** - Swappable serialization and storage backends
- **Versioned saves** - Built-in version field for future migrations
- **Optional AES encryption** - Encrypt save payloads with a user-provided key
- **Type-safe** - Generic methods with compile-time type checking
- **JSON based** - Human-readable save files

## Installation

### Via Unity Package Manager

1. Open your Unity project
2. Navigate to `Window > Package Manager`
3. Click the `+` button in the top-left corner
4. Select `Add package from git URL`
5. Enter: `https://github.com/widwickyy/UnityPackage-Save-System.git#1.0.0`

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
using Widwickyy.SaveSystem;

// Initialize the save system (call once at app start)
SaveManager.Initialize();

// Optional: initialize with AES encryption key
// SaveManager.Initialize(version: 1, encryptionKey: "your-secret-key");

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
        if (loadedData == null)
            return;

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

// Initialize with AES encryption key
SaveManager.Initialize(version: 1, encryptionKey: "your-secret-key");

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
using Widwickyy.SaveSystem;

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
using Widwickyy.SaveSystem;

public interface ISerializer
{
    string Serialize<T>(T data);
    T Deserialize<T>(string json);
}
```

### IStorage Interface

Implement your own storage backend:

```csharp
using Widwickyy.SaveSystem;

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
using Widwickyy.SaveSystem;

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

### AES Encryption

Enable encryption by passing an `encryptionKey` during initialization:

```csharp
SaveManager.Initialize(version: 1, encryptionKey: "my-strong-key");
```

Notes:

- Encrypted files are stored as Base64 text containing IV + ciphertext.
- Keep the same key between sessions or decryption will fail.
- Existing plain JSON saves are not auto-migrated when encryption is enabled.

### Custom Storage

```csharp
using UnityEngine;
using Widwickyy.SaveSystem;

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
using UnityEngine;
using Widwickyy.SaveSystem;

public class CustomSaveManager : MonoBehaviour
{
    private ISaveSystem _saveSystem;

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

The system internally wraps saved data in `SaveWrapper<T>` and includes a `version` field for forward compatibility.

Current behavior:

- `SaveManager.Load<T>()` returns only your `T` data (not the wrapper).
- Version metadata is written to disk automatically.
- Automatic migration hooks are not exposed yet in the public API.

If you need migration logic today, use a custom `ISaveSystem` implementation or read/write raw save payloads through a custom `IStorage` strategy.

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

## Demo Script

A demo script is included in `Runtime/Demo/SaveSystemDemo.cs` demonstrating:

- Initializing `SaveManager` with optional AES encryption
- Saving and loading example player data
- Logging loaded data to verify round-trip serialization

To try it, add `SaveSystemDemo` to any GameObject in a scene and press Play.

## Editor Tool

An editor window is included at `Tools > Save System > Save File Inspector` (`Editor/SaveFileInspectorWindow.cs`) to:

- List JSON save files in `Application.persistentDataPath`
- Inspect formatted payload content
- Optionally decrypt AES-encrypted payloads using your key

## Requirements

- Unity 2022.0 or later
- Newtonsoft.JSON (via Unity Package Manager: `com.unity.nuget.newtonsoft-json`)

## License

MIT License - feel free to use this in any project.
