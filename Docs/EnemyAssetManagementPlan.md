# Enemy Asset Management Plan

このドキュメントは、ヒロインとは独立した敵キャラクター素材を `FantasyLoveSimAssetTool` で管理するための設計メモである。

現状の `AssetUsage.Battle` は、ヒロインの戦闘立ち絵や攻撃、被ダメージ差分を `Characters/<HeroineId>/` 配下で扱うための用途として残す。
敵キャラクターはヒロインに紐づかない共通素材として扱うため、ヒロインの `profile.json` や `assets_export.json` には混ぜない。

## 基本方針

- 敵キャラクターは `Enemies/<EnemyId>/` 配下で管理する。
- 敵画像はヒロイン export とは別の enemy export として出す。
- Unity 側の取り込み先は `Assets/Images/Enemies/<EnemyId>/...` と `Assets/Resources/Enemies/...` を基本にする。
- 最初は戦闘 UI に表示する静止画だけを対象にする。
- 敵の行動、ステータス、ドロップ、AI は画像管理が安定してから別タスクにする。

## 保存フォルダ案

Tool 側の作業データ:

```text
Enemies/
  <EnemyId>/
    enemy.json
    Images/
      Battle/
    Prompts/
      <AssetId>.prompt.json
```

`enemy.json` は最初に次を持てばよい。

```json
{
  "schemaVersion": 1,
  "enemyId": "ForestSlime",
  "displayName": "森スライム",
  "enemyType": "Slime",
  "memo": "",
  "assets": []
}
```

## Export フォルダ案

敵キャラクター export はヒロイン export と分ける。

```text
Export/
  Enemies/
    <EnemyId>/
      Images/
        Battle/
      Data/
        enemy_profile_export.json
        enemy_assets_export.json
      Prompts/
        <AssetId>.prompt.json
```

Unity 側の取り込み先:

```text
Assets/Images/Enemies/<EnemyId>/Battle/
Assets/Resources/Enemies/<EnemyId>.asset
```

将来、敵ごとに複数の ScriptableObject を持つ場合は次へ広げる。

```text
Assets/Resources/Enemies/<EnemyId>/
  EnemyProfileData.asset
  EnemyAssetCatalog.asset
```

## 画像用途と命名

敵画像の `assetId` は、ヒロイン側と衝突しないように `Enemy_<EnemyId>_<Pose>` を基本にする。

| 用途 | assetId 例 | 備考 |
| --- | --- | --- |
| 通常 | `Enemy_ForestSlime_Idle` | 最初に必要 |
| 攻撃 | `Enemy_ForestSlime_Attack` | 必要になってから |
| 被ダメージ | `Enemy_ForestSlime_Damage` | 必要になってから |
| 撃破 | `Enemy_ForestSlime_Defeat` | 必要になってから |

ファイル名は `assetId` と一致させる。

```text
Images/Battle/Enemy_ForestSlime_Idle.png
Images/Battle/Enemy_ForestSlime_Attack.png
Images/Battle/Enemy_ForestSlime_Damage.png
Images/Battle/Enemy_ForestSlime_Defeat.png
```

## enemy_assets_export.json 案

```json
{
  "schemaVersion": 1,
  "enemyId": "ForestSlime",
  "unityImageRoot": "Assets/Images/Enemies/ForestSlime",
  "assets": [
    {
      "assetId": "Enemy_ForestSlime_Idle",
      "usage": "Battle",
      "status": "Accepted",
      "fileName": "Enemy_ForestSlime_Idle.png",
      "memo": "戦闘画面の通常画像",
      "exportImagePath": "Images/Battle/Enemy_ForestSlime_Idle.png",
      "exportPromptPath": "Prompts/Enemy_ForestSlime_Idle.prompt.json",
      "unityImagePath": "Assets/Images/Enemies/ForestSlime/Battle/Enemy_ForestSlime_Idle.png"
    }
  ]
}
```

## 新しいタブ案

WPF Tool には「敵キャラ素材」タブを追加する。

最初の最小機能:

- 敵キャラ一覧
- `EnemyId`
- 表示名
- 敵タイプ
- メモ
- 敵画像一覧
- 画像登録、登録解除、上書き登録
- `Accepted` / `Pending` / `Rejected`
- prompt 記録
- 敵キャラ export

ヒロインの「スチル作業」タブとは分ける。
敵画像はヒロインの基本 prompt やスチル定義とは前提が違うため、同じ画面に混ぜない。

## ヒロイン Battle との境界

`Characters/<HeroineId>/Images/Battle/` に置くもの:

- `Battle_Heroine_Idle`
- `Battle_Heroine_Attack`
- `Battle_Heroine_Damage`
- `Battle_Heroine_Victory`
- `Battle_Heroine_Defeat`
- ヒロイン固有の戦闘演出用画像

`Enemies/<EnemyId>/Images/Battle/` に置くもの:

- `Enemy_<EnemyId>_Idle`
- `Enemy_<EnemyId>_Attack`
- `Enemy_<EnemyId>_Damage`
- `Enemy_<EnemyId>_Defeat`
- ヒロインに依存しない敵共通画像

一時的にヒロイン export 内へ敵画像を入れる運用は避ける。
BattlePanel のデバッグで敵画像が必要な場合も、敵キャラ素材タブから enemy export を出し、Unity 側で `Assets/Images/Enemies/...` として取り込む。

## 実装順

1. `EnemyProfile` と `EnemyAsset` のモデルを追加する。実装済み。
2. `EnemyProjectService` を追加し、`Enemies/<EnemyId>/enemy.json` を保存、読み込みする。実装済み。
3. 「敵キャラ素材」タブを追加する。
4. 敵画像登録、登録解除、上書き登録を実装する。
5. `enemy_profile_export.json` と `enemy_assets_export.json` を出力する。
6. Unity 側 importer の enemy export 対応を進める。

現在の `EnemyProjectService` は、敵プロファイルの作成、保存、読み込み、一覧読み込み、`Images/Battle/` と `Prompts/` の作成、戦闘画像登録、登録解除までを持つ。
画像ファイル本体は、ヒロイン側の登録解除と同じく登録解除では削除しない。

## 後回しにするもの

- 敵ステータス編集
- 敵行動パターン編集
- ドロップ報酬編集
- 敵のアニメーション素材
- 戦闘背景やエフェクト素材
- ヒロイン別に敵画像を差し替える条件分岐
