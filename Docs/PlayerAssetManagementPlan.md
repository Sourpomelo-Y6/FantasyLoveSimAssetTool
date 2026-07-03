# Player Asset Management Plan

このドキュメントは、ヒロインや敵キャラとは独立したプレイヤー戦闘素材を `FantasyLoveSimAssetTool` で管理するための設計メモである。

プレイヤー画像は共通素材として扱い、ヒロインの `Characters/<HeroineId>/` や敵の `Enemies/<EnemyId>/` には混ぜない。
Unity 側コピー先は `Assets/Images/Player/Battle/` を基本にする。

## 保存フォルダ

Tool 側の作業データ:

```text
Player/
  player.json
  Images/
    Battle/
  Prompts/
    <AssetId>.prompt.json
```

`player.json` は次の情報を持つ。

```json
{
  "schemaVersion": 1,
  "playerId": "Player",
  "displayName": "Player",
  "appearancePrompt": "",
  "battleCommonPositivePrompt": "clean lines, highly detailed, masterpiece, 8k, best quality, very aesthetic, absurdres, newest",
  "negativePrompt": "lowres, bad anatomy, bad face, error, extra digit, fewer digits, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry",
  "memo": "",
  "assets": []
}
```

## Export フォルダ

```text
Export/
  Player/
    Images/
      Battle/
        <AssetId>.png
    Data/
      player_profile_export.json
      player_assets_export.json
    Prompts/
      <AssetId>.prompt.json
```

Unity 側コピー先:

```text
Export/Player/Images/Battle/<fileName>
  -> Assets/Images/Player/Battle/<fileName>
```

## 標準 AssetId

| assetId | 用途 |
| --- | --- |
| `Battle_Player_Idle` | 通常画像 |
| `Battle_Player_Attack` | 攻撃画像 |
| `Battle_Player_Damage` | 被ダメージ画像 |
| `Battle_Player_Victory` | 勝利画像 |
| `Battle_Player_Defeat` | 敗北画像 |

最初に必要なのは `Battle_Player_Idle`。
BattlePanel の最小確認では `Battle_Player_Idle`、`Battle_Heroine_Idle`、`Enemy_ForestSlime_Idle` を揃える。

## 実装済み

- 「プレイヤー素材」タブ
- プレイヤー基本 prompt、共通 positive、negative、個別作画 prompt
- 標準候補追加
- 外部画像登録、登録解除、上書き登録
- ComfyUI 送信、生成画像取得、Comfy 採用
- `Export/Player/` への player export

## 注意

`標準候補追加` で作っただけの候補は画像未登録の `Pending` である。
Unity へ渡すには、画像登録または Comfy 採用を行い、Status を `Accepted` にしてから `Player Export` を実行する。
