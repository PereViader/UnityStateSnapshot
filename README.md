[![Test and publish](https://github.com/PereViader/UnityStateSnapshot/actions/workflows/TestAndPublish.yml/badge.svg)](https://github.com/PereViader/UnityStateSnapshot/actions/workflows/TestAndPublish.yml) ![Unity version 6000.0](https://img.shields.io/badge/Unity-6000.0-57b9d3.svg?style=flat&logo=unity) [![GitHub Release](https://img.shields.io/github/v/release/PereViader/UnityStateSnapshot?include_prereleases)](https://github.com/PereViader/UnityStateSnapshot/releases) [![openupm](https://img.shields.io/npm/v/com.pereviader.unitystatessnapshot?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pereviader.unitystatessnapshot/)


# Unity State Snapshot

> Effortless regression testing for Unity: capture, inspect, and verify complex UI hierarchies, components, and scene states against deterministic baselines.

---

## Overview

As Unity projects grow in complexity, writing assertions for deep UI hierarchies, component states, animations, physics setups, and level structures becomes tedious and fragile.

**Unity State Snapshot** captures complex Unity object graphs and game states into structured `.verified.json` snapshots. On subsequent test runs, the engine extracts the live state, compares it against the verified baseline, and highlights exact differences with **Git-style unified diffs**.

```csharp
[Test]
public void MainMenu_InitialState_MatchesSnapshot()
{
    var menuGo = Object.Instantiate(menuPrefab);

    // One-line snapshot verification:
    Snapshot.Verify(menuGo);
}
```

---

## Key Features

- 🎯 **Explicit `Snapshot` Static API**: Simple, non-intrusive static entry points (`Snapshot.Verify` and `Snapshot.Compare`) for `GameObject`, `Component`, `Scene`, `IEnumerable<GameObject>`, and custom DTOs.
- 📐 **Resolution-Agnostic Normalization**: Automatically normalizes `ScreenSpaceOverlay` Canvases and dynamic viewport dimensions (`pixelRect`, `renderingDisplaySize`), ensuring tests produce 100% byte-identical baselines in the Unity Editor, Play Mode, and headless CI batchmode (`640x480`).
- 🧩 **35+ Built-in Component Handlers**: Handlers with safe, non-leaking property extraction across UI (uGUI, TextMeshPro, UI Toolkit), 2D/3D Physics, 2D/3D Rendering, Audio, Animation, Navigation, Tilemaps, and Video.
- 🔍 **Rich Unified Line Diffs**: Failures output Git-style unified diffs (`--- Expected` / `+++ Received`) embedded directly in the test exception, alongside the file paths for easy review.
- ⚡ **AutoVerify Workflow**: Auto-accept new baselines seamlessly via `settings.AutoVerify(true)` or the `SNAPSHOT_AUTO_VERIFY=1` environment variable.
- 🛡️ **Non-Throwing `Snapshot.Compare` API**: Inspect diffs and comparison results programmatically without catching exceptions.
- 🧹 **Deterministic Scrubber Pipeline**: Normalizes IEEE 754 negative zero (`-0.0f` -> `0.0f`), strips runtime `(Clone)` and `(Instance)` suffixes, normalizes vector/color precisions, and sorts all JSON keys alphabetically.
- 🔒 **Safe Reflection Engine**: Automatically extracts public fields and `[SerializeField]` private fields while skipping base Unity engine properties (`destroyCancellationToken`, `didAwake`) and memory-leaking accessors (`renderer.material`, `meshFilter.mesh`).

---

## Installation & Setup

### 1. Requirements
- **Unity**: Version 6000.0 or higher.

### 2. Install the Package

[Install from OpenUPM](https://openupm.com/packages/com.pereviader.unitystatessnapshot/#modal-manualinstallation).

---

## Usage Guide

### 1. Basic Snapshot Verification

Call `Snapshot.Verify(...)` inside your NUnit EditMode or PlayMode test. The snapshot file will be automatically created in a `Snapshots~` subfolder next to your test file:

```csharp
using NUnit.Framework;
using UnityEngine;
using PereViader.UnityStateSnapshot;

[TestFixture]
public class InventoryTests
{
    [Test]
    public void InventoryView_OpenedState_MatchesSnapshot()
    {
        var inventoryGo = Object.Instantiate(inventoryPrefab);

        // Asserts state matches InventoryTests.InventoryView_OpenedState.verified.json
        Snapshot.Verify(inventoryGo);
    }
}
```

---

### 2. Custom Snapshot Names & `VerifySettings`

Configure float precision, custom directories, or specific member filters:

```csharp
[Test]
public void SettingsDialog_WithCustomSettings()
{
    var settings = new VerifySettings()
        .WithFloatPrecision(2)               // Round floats to 2 decimal places
        .IncludeDisabledObjects(true)        // Capture inactive child GameObjects
        .IncludeSerializedFields(true)       // Capture private [SerializeField] fields
        .UseDirectory("CustomSnapshots")     // Save snapshots in a custom folder
        .IgnoreComponent<AudioSource>()      // Exclude entire component types
        .IgnoreMember("LivePingMs");         // Exclude dynamic property names

    Snapshot.Verify(settingsDialog, "SettingsDialog_CustomState", settings);
}
```

---

### 3. Verifying Specific Targets

```csharp
// 1. Single Component
Snapshot.Verify(myTextMeshProComponent);

// 2. Multiple GameObjects / Root hierarchies
Snapshot.Verify(new[] { playerRoot, uiCanvasRoot });

// 3. Full Unity Scene
Snapshot.Verify(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

// 4. Extract state DTO without asserting
GameObjectSnapshotDTO dto = Snapshot.Extract(myGameObject, settings);
```

---

### 4. Non-Throwing Comparison (`Snapshot.Compare`)

Inspect differences programmatically without raising test exceptions:

```csharp
SnapshotComparisonResult result = Snapshot.Compare(playerObject);

if (!result.IsMatch)
{
    Debug.LogWarning($"Snapshot mismatch for {result.SnapshotName}:\n{result.Diff}");
    Debug.Log($"Expected baseline at: {result.VerifiedFilePath}");
    Debug.Log($"Received output at: {result.ReceivedFilePath}");
}
```

---

### 5. Auto-Accepting Baseline Changes (`AutoVerify`)

When intentionally changing UI or gameplay logic, update the verified baselines automatically:

#### In Code:
```csharp
var settings = new VerifySettings().AutoVerify(true);
Snapshot.Verify(myGameObject, settings);
```

#### Via Environment Variable (CI or Command Line):
```bash
SNAPSHOT_AUTO_VERIFY=1 unitycli.sh test --editmode
```

---

### 6. Ignoring Volatile Fields (`[SnapshotIgnore]`)

Decorate non-deterministic or transient members with `[SnapshotIgnore]`:

```csharp
public class PlayerState : MonoBehaviour
{
    public int Level = 10;            // Captured

    [SerializeField]
    private int _health = 100;        // Captured

    [SnapshotIgnore]
    public string SessionToken;       // Ignored

    [SnapshotIgnore]
    public float LiveFps => 120.5f;   // Ignored
}
```

---

### 7. Custom Component Handlers (`ISnapshotComponentHandler`)

Extend the snapshot engine with custom extractors:

```csharp
public class HealthHandler : ISnapshotComponentHandler
{
    public int Priority => 100; // Higher priority runs before default reflection

    public bool CanHandle(Type componentType)
    {
        return typeof(HealthComponent).IsAssignableFrom(componentType);
    }

    public void Extract(Component component, IDictionary<string, object> targetState, VerifySettings settings)
    {
        var health = (HealthComponent)component;
        targetState["Health"] = $"{health.CurrentHP}/{health.MaxHP}";
        targetState["IsAlive"] = health.IsAlive;
    }
}

// Register globally or per-test:
var settings = new VerifySettings().RegisterComponentHandler(new HealthHandler());
Snapshot.Verify(playerGo, settings);
```

---

## Supported Unity Components

| Subsystem | Built-in Handlers |
| :--- | :--- |
| **UI Toolkit** | `UIDocument` (with recursive `VisualElement` tree extraction, classes, layout, and values) |
| **uGUI Controls** | `Button`, `Toggle`, `Slider`, `Scrollbar`, `ScrollRect`, `Selectable` |
| **uGUI Graphics & Layout** | `Image`, `RawImage`, `Text`, `CanvasGroup`, `HorizontalLayoutGroup`, `VerticalLayoutGroup`, `GridLayoutGroup`, `ContentSizeFitter`, `AspectRatioFitter` |
| **Canvas** | `Canvas` *(with resolution-independent ScreenSpaceOverlay normalization)*, `CanvasScaler` |
| **TextMeshPro** | `TextMeshProUGUI`, `TMP_Text`, `TMP_InputField`, `TMP_Dropdown` |
| **Physics 3D** | `Rigidbody`, `CharacterController`, `BoxCollider`, `SphereCollider`, `CapsuleCollider`, `MeshCollider` |
| **Physics 2D** | `Rigidbody2D`, `BoxCollider2D`, `CircleCollider2D`, `CapsuleCollider2D`, `PolygonCollider2D`, `TilemapCollider2D` |
| **2D Tilemaps & Grids** | `Tilemap`, `Grid`, `TilemapRenderer` |
| **AI & Navigation** | `NavMeshAgent`, `NavMeshObstacle`, `OffMeshLink` |
| **Rendering & Cameras** | `SpriteRenderer`, `MeshFilter`, `MeshRenderer`, `SkinnedMeshRenderer`, `Camera`, `Light`, `LineRenderer`, `TrailRenderer`, `ParticleSystemRenderer` |
| **Animation & Audio** | `Animator` *(with active clips and parameters)*, `Animation`, `ParticleSystem`, `AudioSource`, `AudioListener` |
| **Video** | `VideoPlayer` |
| **Custom Scripts** | `MonoBehaviour` *(deep reflection for public fields, properties, `[SerializeField]` private fields, collections, and nested objects)* |

---

## Example Unified Diff Output

When a test detects a difference, `SnapshotMismatchException` displays a Git-style diff:

```text
SnapshotMismatchException: Snapshot mismatch for 'SettingsScreen_OpenedState'
--- Expected: Assets/MockApp/Tests/Snapshots~/SettingsScreen_OpenedState.verified.json
+++ Received: Assets/MockApp/Tests/Snapshots~/SettingsScreen_OpenedState.received.json
@@ -14,7 +14,7 @@
       "Components": [
         {
           "TypeName": "Slider",
-          "Value": "50"
+          "Value": "75"
         }
       ]
```

---

## License

MIT License. See [LICENSE](LICENSE) for details.
