# My Project

A Unity crafting game prototype using the Universal Render Pipeline (URP) and the new Input System.

## Table of Contents

1. [Quick Start](#quick-start)
2. [Project Structure](#project-structure)
3. [Scripts Overview](#scripts-overview)
   - [PlayerController](#playercontroller)
   - [CameraPosController](#cameraposcontroller)
   - [CameraMover](#cameramover)
   - [ItemSelector](#itemselector)
   - [Crafting](#crafting)
   - [CraftingRecipe (ScriptableObject)](#craftingrecipe-scriptableobject)
4. [How the Systems Work Together](#how-the-systems-work-together)
5. [Setting Up a Scene](#setting-up-a-scene)
   - [Setting Up Camera Positions](#setting-up-camera-positions)
   - [Setting Up Crafting Stations](#setting-up-crafting-stations)
   - [Setting Up the Player](#setting-up-the-player)
6. [Creating New Recipes](#creating-new-recipes)
7. [Adding New Items](#adding-new-items)
8. [Adding New Camera Positions](#adding-new-camera-positions)
9. [Troubleshooting](#troubleshooting)
10. [Known Issues](#known-issues)

---

## Quick Start

1. Open the project in Unity (version matching the project settings).
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.
4. Click on objects in the world to interact with them (cycle items, change camera, craft).

---

## Project Structure

```
Assets/
  PlayerController.cs          -- Player state (HP, held item reference)
  CameraPosController.cs       -- Camera position manager with smooth lerping
  CameraMover.cs               -- Triggers camera position changes
  ItemSelector.cs              -- Cycles through item prefabs on click
  Crafting.cs                  -- Crafting station logic
  CraftingRecipe.cs            -- ScriptableObject for recipe definitions
  InputSystem_Actions.inputactions  -- Input mapping configuration
  TEST.prefab                  -- Test item prefab (box mesh)
  TEST 1.prefab                -- Test item prefab variant (box mesh, different material)
  TEST.mat                     -- Material for TEST prefab
  TEST 1.mat                   -- Material for TEST 1 prefab
  TEST.asset                   -- Example crafting recipe
  Scenes/
    SampleScene.unity          -- Main scene
  Settings/                    -- URP rendering settings
  TutorialInfo/                -- Unity tutorial boilerplate
  Logs/                        -- Editor logs
```

---

## Scripts Overview

### PlayerController

**File:** `Assets/PlayerController.cs`

A simple data holder attached to the player GameObject. It stores:

| Field | Type | Description |
|-------|------|-------------|
| `playerHp` | int | Current health points of the player |
| `itemHeldIndex` | GameObject | Reference to the item the player is currently holding |

**How to use:**
- Attach this script to your player GameObject.
- Set `playerHp` to your desired starting health in the Inspector.
- The `itemHeldIndex` field gets automatically updated by the Crafting system when something is successfully crafted (see [Crafting](#crafting) below).
- Use this reference in other scripts to know what item the player currently holds.

**Example usage in another script:**
```csharp
public class SomeOtherScript : MonoBehaviour {
    public PlayerController player;

    void SomeMethod() {
        Debug.Log("Player HP: " + player.playerHp);
        if (player.itemHeldIndex != null) {
            Debug.Log("Holding: " + player.itemHeldIndex.name);
        }
    }
}
```

---

### CameraPosController

**File:** `Assets/CameraPosController.cs`

Manages a list of camera positions and smoothly interpolates the camera between them.

| Field | Type | Description |
|-------|------|-------------|
| `pCam` | Camera | The main game camera to move |
| `positions` | List<Transform> | Ordered list of transforms representing camera positions |
| `lerpDuration` | float | Time in seconds for the camera to travel between positions (default: 1.0) |
| `currentIndex` | int | Current position index in the list (starts at 0) |
| `isLerping` | bool (read-only) | True while the camera is in the middle of a transition |

**How it works:**
1. On `Start()`, the camera is placed at the first position in the list (index 0).
2. When a left mouse click hits this GameObject or any of its children, the controller advances to the next position in the list and lerps the camera there.
3. The list wraps around: after the last position, it goes back to the first.
4. The lerp uses `Vector3.Lerp` for position and `Quaternion.Slerp` for rotation, with `SmoothStep` easing for a natural feel.

**Important notes:**
- The `FixedUpdate()` method exists but is currently empty. It was likely intended for physics-based camera movement in the future.
- Both `CameraPosController` and `CameraMover` have overlapping click-detection logic. `CameraMover` has priority (it checks first and exits early).

---

### CameraMover

**File:** `Assets/CameraMover.cs`

A trigger script that tells a `CameraPosController` to move the camera to the next position.

| Field | Type | Description |
|-------|------|-------------|
| `moveNow` | bool | Set to `true` from code to trigger a camera move this frame |
| `controller` | CameraPosController | Reference to the camera position controller to control |

**How it works:**
1. In `Update()`, it first checks if `moveNow` is `true`. If so, it triggers movement and resets the flag.
2. If `moveNow` was not set, it checks for a left mouse click. If the click raycast hits this GameObject or any child, it triggers movement.
3. The movement is delegated to the linked `CameraPosController` -- it increments the index and starts the lerp coroutine.

**Programmatic triggering:**
```csharp
// From any script that has a reference to this CameraMover:
cameraMover.moveNow = true;
```

**Why both CameraPosController and CameraMover?**
- `CameraPosController` handles the actual camera movement logic (lerping, position storage).
- `CameraMover` acts as an event trigger -- it can be activated either by mouse clicks or by code (e.g., when crafting completes).
- This separation allows the Crafting system to trigger camera changes without direct mouse interaction.

---

### ItemSelector

**File:** `Assets/ItemSelector.cs`

Cycles through a list of item prefabs and spawns the currently selected one at its transform's position.

| Field | Type | Description |
|-------|------|-------------|
| `prefabs` | List<GameObject> | Ordered list of item prefabs to cycle through |
| `currentIndex` | int | Index of the currently selected prefab (starts at 0) |

**How it works:**
1. On `Start()`, the first prefab in the list is spawned.
2. On every left mouse click that hits this GameObject or any child, it advances to the next prefab in the list (wrapping around).
3. `SpawnItem()` destroys the currently spawned instance and instantiates the new one at this transform's position and rotation, parenting it to this GameObject.

**Key behaviors:**
- Only one item instance exists at a time. Spawning a new item automatically destroys the old one.
- The spawned item is parented to this GameObject's transform, so it moves with it.
- The click detection uses raycasting from the main camera -- the object must have a collider.

**Example: Setting up a crafting slot**
1. Create an empty GameObject in your scene.
2. Add a Collider component (Box Collider, Sphere Collider, etc.) so it can be clicked.
3. Attach the `ItemSelector` script.
4. Drag your item prefabs into the `Prefabs` list.
5. Position this GameObject where you want the item to appear when spawned.

---

### Crafting

**File:** `Assets/Crafting.cs`

The main crafting station logic. It reads the currently selected items from slot `ItemSelector` components, checks them against defined recipes, and spawns the output if there's a match.

| Field | Type | Description |
|-------|------|-------------|
| `slots` | List<ItemSelector> | The crafting slots -- drag ItemSelector components here |
| `recipes` | List<CraftingRecipe> | Available crafting recipes |
| `outputSpawnPoint` | Transform | Where the crafted item spawns |
| `player` | PlayerController | Reference to the player (updates their held item) |
| `move` | CameraMover | Reference to a CameraMover (triggers camera change on craft) |

**How it works:**
1. On a left mouse click that hits this GameObject or any child, it calls `TryCraft()`.
2. `TryCraft()` collects the currently selected prefab from each slot (in order).
3. It loops through all recipes and checks if the collected items match the recipe's ingredients (exact order match).
4. If a match is found:
   - The output prefab spawns at `outputSpawnPoint`.
   - The player's `itemHeldIndex` is set to the output prefab.
   - The linked `CameraMover.moveNow` is set to `true` (triggers a camera move).
   - A log message is printed to the console.
5. If no match is found, a "No matching recipe found" message is logged.

**Important -- ingredient matching:**
- Ingredients must match in exact order and exact prefab reference.
- The `Matches()` method uses reference equality (`current[i] != required[i]`), not name comparison.
- This means you must use the exact same prefab asset in both the recipe and the slots.

**Example: Setting up a crafting station**
1. Create an empty GameObject for the crafting station.
2. Add a Collider so it can be clicked.
3. Attach the `Crafting` script.
4. Create child GameObjects for each slot, each with an `ItemSelector` component.
5. Drag the `ItemSelector` components into the `Slots` list.
6. Create CraftingRecipe assets (see [Creating New Recipes](#creating-new-recipes)).
7. Drag the recipes into the `Recipes` list.
8. Create an empty GameObject for the output spawn point and drag it into `Output Spawn Point`.
9. Drag your Player GameObject (with PlayerController) into `Player`.
10. Drag a CameraMover GameObject into `Move`.

---

### CraftingRecipe (ScriptableObject)

**File:** `Assets/CraftingRecipe.cs`

A ScriptableObject asset that defines a single crafting recipe.

| Field | Type | Description |
|-------|------|-------------|
| `ingredients` | List<GameObject> | The required item prefabs in exact order |
| `output` | GameObject | The prefab that spawns when the recipe is crafted |

**How to create a new recipe:**
1. In the Unity Editor, right-click in the Project window.
2. Navigate to `Create > Crafting > Recipe`.
3. A new CraftingRecipe asset is created. Rename it.
4. In the Inspector:
   - Drag your item prefabs into `Ingredients` (in the order they must appear in the slots).
   - Drag the output prefab into `Output`.

**Example recipe from the project:**
The included `TEST.asset` recipe requires:
- Slot 1: TEST prefab
- Slot 2: TEST prefab
- Slot 3: TEST 1 prefab
- Slot 4: TEST 1 prefab
- Slot 5: TEST 1 prefab

Output: TEST 1 prefab

---

## How the Systems Work Together

```
     [Player Clicks Object]
            |
            v
   +------------------+
   |  Raycast Check   |  -- Does the click hit the object?
   +------------------+
            | Yes
            v
   +------------------+
   |  Which script?   |
   +------------------+
      |            |           |
      v            v           v
   ItemSelector   CameraMover   Crafting
   (cycles       (triggers     (checks slots
    items)        camera move)  vs recipes)
                        |           |
                        v           v
             CameraPosController   PlayerController
             (lerps camera)        (updates held item)
```

**The flow for crafting specifically:**
1. The player places items into the crafting slots by clicking on `ItemSelector` GameObjects.
2. The player clicks on the `Crafting` station GameObject.
3. `Crafting.TryCraft()` reads all slot selections and checks against recipes.
4. On a successful craft:
   - The output spawns at the designated spawn point.
   - The player's `itemHeldIndex` is updated to the crafted item.
   - The camera moves to the next position (via `CameraMover`).
5. On a failed craft, nothing spawns and a console message is shown.

---

## Setting Up a Scene

### Setting Up Camera Positions

1. Create empty GameObjects in your scene for each camera position you want.
2. Position and rotate each one where you want the camera to look from.
3. Create a GameObject with `CameraPosController` attached (usually on the camera itself or a dedicated manager object).
4. In the Inspector:
   - Drag your main camera into the `P Cam` field.
   - Drag all the position GameObjects into the `Positions` list (in order).
   - Adjust `Lerp Duration` if you want faster or slower camera transitions.
5. Create a GameObject with `CameraMover` attached.
6. Drag the `CameraPosController` into the `Controller` field.

### Setting Up Crafting Stations

1. **Create slots:**
   - Create empty GameObjects as children of the crafting station.
   - Add a Collider to each slot (so they can be clicked).
   - Attach `ItemSelector` to each slot.
   - Fill in the `Prefabs` list with your item options.

2. **Configure the station:**
   - Create an empty GameObject for the station itself.
   - Add a Collider to the station.
   - Attach `Crafting` script.
   - Drag all slot `ItemSelector` components into the `Slots` list.
   - Create `CraftingRecipe` assets and add them to the `Recipes` list.
   - Create a spawn point (empty GameObject) and drag it into `Output Spawn Point`.
   - Drag your Player into `Player`.
   - Drag your CameraMover into `Move`.

3. **Test:**
   - Place items in the slots by clicking on them.
   - Click the crafting station.
   - Check the console for "Crafted: ..." or "No matching recipe found".

### Setting Up the Player

1. Create or select your player GameObject.
2. Attach `PlayerController` script.
3. Set `Player HP` to your desired value.
4. The `Item Held Index` will be set automatically by the crafting system.

---

## Creating New Recipes

1. Right-click in the Project window.
2. Go to `Create > Crafting > Recipe`.
3. Name the asset descriptively (e.g., "WoodenSword", "IronAxe").
4. In the Inspector:
   - Drag the exact prefab assets for each ingredient into the `Ingredients` list, in the required slot order.
   - Drag the output prefab into the `Output` field.
5. Drag the recipe asset into a Crafting station's `Recipes` list.

**Tip:** Make sure the prefab GUIDs match exactly. If you duplicate a prefab, you need to update the recipe to reference the new prefab.

---

## Adding New Items

1. Create or import your item prefab (must have a MeshFilter, MeshRenderer, and Collider).
2. Place the prefab in your project's Assets folder.
3. To add it as an option in a slot:
   - Select the `ItemSelector` component.
   - Increase the `Prefabs` list size.
   - Drag your new prefab into the new slot.
4. To use it in a recipe:
   - Create or edit a `CraftingRecipe` asset.
   - Drag your prefab into the `Ingredients` or `Output` field as needed.

---

## Adding New Camera Positions

1. Create an empty GameObject in your scene.
2. Position and rotate it to the desired camera viewpoint.
3. Select your `CameraPosController` GameObject.
4. Increase the `Positions` list size.
5. Drag the new position GameObject into the list (at the desired order).

---

## Troubleshooting

**"No matching recipe found" when I know the items are correct:**
- Check that the prefab references in your slots exactly match the prefab references in the recipe.
- The match uses reference equality, not name matching. If you duplicated a prefab, update the recipe.
- Check that the order of items in the slots matches the order of ingredients in the recipe.

**The camera doesn't move when I click:**
- Make sure the `CameraPosController` has a valid camera assigned to `P Cam`.
- Make sure the `Positions` list is not empty.
- Make sure the object you're clicking has a Collider.
- Check that `CameraMover` has a valid `Controller` reference.

**The crafted item doesn't spawn:**
- Make sure `Output Spawn Point` is assigned and has a valid Transform.
- Check the console for errors.
- Verify the output prefab is not null in the recipe.

**Items don't appear when I click a slot:**
- Make sure the `ItemSelector` has items in its `Prefabs` list.
- Make sure the GameObject has a Collider.
- Make sure the raycast hits the object (the camera needs a clear line of sight).

**The player's held item doesn't update after crafting:**
- Make sure the `Crafting` script has a valid reference to your `PlayerController` (drag the player GameObject into the `Player` field).

---

## Known Issues

1. **Duplicate click detection:** Both `CameraPosController` and `CameraMover` check for left mouse clicks. `CameraMover` has priority because it checks first in `Update()` and returns early. This means if a click hits a `CameraMover` object, the `CameraPosController` on the same object will not respond.

2. **Empty FixedUpdate():** `CameraPosController.FixedUpdate()` is empty. If you plan to add physics-based camera movement, this is where you'd do it.

3. **No input system usage:** Despite having `InputSystem_Actions.inputactions` in the project, all scripts use the legacy `Mouse.current` from the old Input System API. Consider migrating to the new Input System actions for better flexibility.

4. **No inventory system:** Items are spawned and destroyed on the fly. There's no persistent inventory -- the player's `itemHeldIndex` is the only inventory tracking.

5. **No item destruction after crafting:** The Crafting system spawns the output but does not remove the ingredients from the slots. Slots retain their selected items after crafting.

6. **Single camera position per object:** Both `CameraPosController` and `ItemSelector` are attached per-object. If you have multiple camera position objects in a scene, each one manages its own independent camera movement.

7. **No validation on prefab references:** The system will not warn you if a recipe references a prefab that has been deleted or if a slot's prefab list is empty.
