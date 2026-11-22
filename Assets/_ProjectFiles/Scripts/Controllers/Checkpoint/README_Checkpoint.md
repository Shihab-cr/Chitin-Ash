# Checkpoint Menu: quick setup

The system follows the team's MVC structure. Each part is in a `Checkpoint` subfolder:

| Folder | Scripts |
|---|---|
| `Models/Checkpoint` | `ProgressionModel` (main model, pure C#, static Instance), `PlayerLevelModel`, `SkillModel`, `QuestModel`, `ProgressionEvents` (event bus) |
| `Views/Checkpoint` | `CheckpointMenuView`, `StatsPanelView`, `SkillsPanelView`, `SkillSlotView`, `QuestsPanelView`, `QuestEntryView`. These hold the UI placeholders. |
| `Controllers/Checkpoint` | `ProgressionController`, `CheckpointController`, `CheckpointMenuController`, `StatsPanelController`, `SkillsPanelController`, `QuestsPanelController`, `PlayerStatsController` |
| `DataSaving/Checkpoint` | `ProgressionSaveData`, `ProgressionSaveSystem` (`chitin_progression.json`) |
| `ScriptableObjects/Checkpoint` | `LevelProgressionSO`, `SkillDefinitionSO`, `QuestDefinitionSO`, `ProgressionTypes` |

## Setup
1. Create the data assets with **Create > Chitin & Ash > Progression**: one Level Progression, one Skill per skill (drop in its icon), and one Quest per goal.
2. Add one **ProgressionController** object to the gameplay scene, and assign the assets to it.
3. Add **PlayerStatsController** to the Player, which must be tagged `Player`. Wire its UnityEvents to the stamina, damage and dodge scripts.
4. For each checkpoint, add a Collider and a **CheckpointController**. Set its id, its name, and optionally a respawn point, a prompt and the `OnRested` hooks.
5. In the Canvas:
   - Add **CheckpointMenuController**. The Canvas must stay active.
   - Add the menu panel with **CheckpointMenuView**.
   - Add the three tab panels, each with its View and Controller.
   - Drag in the buttons, texts, icons and prefabs. Empty fields are skipped.

## Teammates: report progress with one line
```csharp
ProgressionEvents.ReportEnemyDefeated("BlackAnt");
ProgressionEvents.ReportEnemyConsumed("Snail");
ProgressionEvents.ReportEliteDefeated("BullAnt");
ProgressionEvents.ReportRedAphidsCollected(1);
```

## Two hooks still needed in team scripts
- `PauseMenuController.Update()`: add `if (CheckpointMenuController.BlocksPauseMenu) return;`
- `MainMenuController.HandleConfirmNewGame()`: add `ProgressionSaveSystem.DeleteSave();`

The full explanation is in the Checkpoint System Documentation (Word).
