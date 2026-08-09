# Heroine Main Menu Action Plan

## Purpose

This document records why heroine-specific main menu items can be missing or execute the wrong operation in Unity, and how the Asset Tool should prevent and repair that state.

The issue was found while comparing `TestHeroine` and `Heroine3`.

## Current Problem

Unity builds the heroine main menu from every `ActionData` asset loaded from:

```text
Assets/Resources/Heroines/<HeroineId>/Actions/
```

The selected heroine profile points directly to this folder. Unity does not automatically merge missing menu actions from `TestHeroine` or a shared default folder.

`Heroine3` currently has only these action definitions:

- `Talk`
- `Tea`
- `Rest`
- `Walk`
- `Gift`
- `DressUp`
- `OutfitReaction`

It is missing menu actions present in `TestHeroine`, including:

- `Schedule`
- `Training`
- `Skill`
- `StatusDetail`
- `StillGallery`
- `MessageLog`
- `DebugBattle` when the debug entry is required

Some existing `Heroine3` actions also have the default `executionType = SimpleAction`. This is incorrect for actions that open another panel:

| Action ID | Required execution type |
| --- | --- |
| `Talk` | `OpenConversationGenres` |
| `DressUp` | `OpenOutfitPanel` |
| `OutfitReaction` | `OpenOutfitReactionPanel` |
| `Schedule` | `OpenSchedulePanel` |
| `StatusDetail` | `OpenStatusDetailPanel` |
| `StillGallery` | `OpenStillGalleryPanel` |
| `MessageLog` | `OpenMessageLogPanel` |
| `DebugBattle` | `OpenDebugBattlePanel` |
| `Training` | `OpenTrainingPanel` |
| `Skill` | `OpenSkillPanel` |
| `Tea`, `Rest`, `Walk`, `Gift` | `SimpleAction` |

Consequently, an item can be absent because no `ActionData` asset exists, or it can appear but run as a normal dialogue action because its `executionType` is wrong.

## Root Cause In The Tool Workflow

The Tool stores menu definitions separately from `ActionReactions`. Unity imports `Data/actions_export.json` before action reactions, creates missing `ActionData`, and updates existing menu display and navigation settings while preserving reactions and Unity-only image references.

Use `標準メニュー項目を準備` for a new heroine before export. It adds missing production menu definitions without overwriting definitions already stored for that heroine.

## Current Workflow

1. Select the heroine in AssetTool.
2. Open the `会話データ` tab.
3. Click `標準メニュー項目を準備`.
4. Export the heroine and confirm that `Data/actions_export.json` exists.
5. In Unity, run `FantasyLoveSim > Import Heroine Export` and select that export folder.
6. Confirm that the import result reports `Menu actions: 13` and that the main menu shows all production entries.

The `メニュー設定` tab displays and edits `ActionId`, display name, column, sort order, execution type, enabled state, and required state. Use `標準項目を準備` to add only missing items. Use `標準配置へ戻す` to repair the standard 13 items after confirming the warning dialog; custom action definitions are preserved. The validation area reports missing required items, duplicate IDs or sort orders, empty names, invalid columns, and unsupported execution types. The production-status page includes the same validation and links to this tab.

`DebugBattle` is intentionally excluded from the production template. Add it separately only when the selected heroine needs the development battle entry.

The menu definition and the action reaction text are related but must not be treated as the same data:

- Menu definition: ID, display name, display column, execution type, visibility, and ordering.
- Action reaction: result text, conditions, affection change, expression, voice, and still image.

## Implementation Status

The first Tool-side phase is implemented:

- `MenuActions` is stored in each heroine `profile.json`.
- The conversation tab provides `標準メニュー項目を準備`.
- The command adds the 13 missing production actions without overwriting existing definitions.
- Export writes `Data/actions_export.json` with string execution type names.
- Export warnings report missing, duplicate, unknown, and incorrectly configured standard actions.

Unity import of `actions_export.json` and a dedicated menu action editing table remain future work. Until those are implemented, use the recovery procedure below after exporting.

## Required Tool Changes

### 1. Add A Menu Action Model

Store heroine menu definitions independently from conversation entries. The minimum fields are:

```text
ActionId
DisplayName
DisplayColumn
ExecutionType
IsEnabled
IsRequired
```

Use stable string names for `ExecutionType` in JSON rather than Unity enum numbers. This avoids changing behavior if the Unity enum order changes.

### 2. Provide A Standard Menu Template

Add a command such as `Prepare standard menu actions` when creating or editing a heroine. It should add missing definitions without overwriting existing character-specific text or reactions.

The production template should include:

```text
Talk
Tea
Rest
Walk
Gift
DressUp
OutfitReaction
Schedule
Training
Skill
StatusDetail
StillGallery
MessageLog
```

`DebugBattle` should be controlled by a development/debug option and should not be required for a production heroine.

### 3. Export Menu Actions Separately

Export a dedicated file, for example:

```text
Data/actions_export.json
```

`action_reactions_export.json` should continue to describe reactions. It must not be the only source used to create menu actions.

### 4. Merge Safely In The Unity Importer

The Unity importer should process menu definitions before action reactions:

1. Find `ActionData` by `actionId`, not by asset filename.
2. Create a missing asset from the menu definition.
3. Apply `displayName`, `displayColumn`, and `executionType`.
4. Merge reaction data into the resulting action.
5. Preserve Unity-only Sprite references and other fields omitted from JSON.
6. Report duplicate action IDs as errors instead of choosing one silently.

When importing an old export that has no menu definition, do not replace the execution type of an existing Unity action with `SimpleAction`.

### 5. Add Validation

Export validation should report:

- Missing required menu action IDs.
- Duplicate action IDs.
- Unknown execution type names.
- Navigation actions configured as `SimpleAction`.
- Reactions whose `conditions.actionId` has no menu action definition.
- Scheduled event action IDs that do not match the supported schedule mapping.

Missing production actions and incorrect navigation execution types should be errors. Missing `DebugBattle` should be informational only.

## Heroine3 Recovery Procedure

Until the Tool changes are implemented, repair `Heroine3` in Unity as follows:

1. Back up `Assets/Resources/Heroines/Heroine3/Actions/`.
2. Copy the missing menu-only `ActionData` assets from `TestHeroine` into the `Heroine3` action folder.
3. Keep each `actionId` unchanged.
4. Set character-facing display names as required.
5. Correct `Talk`, `DressUp`, and `OutfitReaction` to the execution types listed above.
6. Keep the existing `Heroine3` reactions for `Tea`, `Rest`, `Walk`, and `Gift`.
7. Confirm that `Heroine3Profile.actionResourcePath` remains `Heroines/Heroine3/Actions`.
8. Reimport assets and test every menu item in Play Mode.

Do not solve the problem by changing `Heroine3` to load `TestHeroine/Actions`. That would couple the characters and could expose TestHeroine dialogue, stills, or future changes.

## Verification Checklist

- Every production menu item appears exactly once.
- `Talk` opens the conversation genre menu.
- `DressUp` opens the outfit panel.
- `OutfitReaction` opens the outfit reaction panel.
- `Schedule`, `Training`, and `Skill` open their respective panels.
- `StatusDetail`, `StillGallery`, and `MessageLog` open their respective panels.
- `Tea`, `Rest`, `Walk`, and `Gift` still select Heroine3-specific reactions.
- No TestHeroine dialogue or image reference is introduced.
- Closing a panel returns to the expected main menu state.

## Completion Criteria

The issue is considered fixed when a newly created heroine can prepare, edit, validate, export, and import the complete main menu without copying Unity assets manually, and the resulting menu behavior matches `TestHeroine` while retaining heroine-specific reactions.
