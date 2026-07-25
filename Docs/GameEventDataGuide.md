# Game Event Data Guide

このドキュメントは、`FantasyLoveSimAssetTool` の `会話データ` タブで作る `GameEvents` のカテゴリ、条件、イベントスチル参照の運用をまとめる。

画像本体は後から準備してもよい。
先にイベント本文、発火条件、参照する予定の `imageAssetIds` を決めておくと、Unity 側のイベント発火処理と素材作成の対応が崩れにくい。

## 基本方針

- WPF 側では `GameEvents` を `profile.json` に保存する。
- Export 時は `Export/<HeroineId>/Data/game_events_export.json` に出力する。
- Unity 側では `game_events_export.json` を読み、`GameEventData` または同等の ScriptableObject に変換する。
- イベント発火は `category` と `conditions` を見て Unity 側で判定する。
- イベントスチルは `imageAssetIds` に WPF 側の `HeroineAsset.AssetId` を入れて紐付ける。

## WPF 側の入力手順

1. `会話データ` タブを開く。
2. `データ種別` を `GameEvents` にする。
3. `追加` を押す。
4. カテゴリ候補から `Intro`、`DayStart` などを選ぶ。
5. `イベント雛形` を押す。
6. 自動入力された `Id`、`タイトル`、条件、台詞行をイベント内容に合わせて修正する。
7. `イベント完了時変化` に全ページ表示後の好感度変化を入力する。変化なしは `0` とする。
8. 必要なら `画像AssetId` に `GameStartIntro_01` などを入れる。
9. `会話データ保存` を押す。

`イベント雛形` は、選択中のイベント行に対してカテゴリ、ID、タイトル、条件、台詞の初期行を設定する。
既に台詞本文が入っている場合、本文は置き換えない。

## カテゴリ一覧

| category | 用途 | 主な条件 |
| --- | --- | --- |
| `Intro` | 初回導入、出会い、チュートリアル相当 | `once`, `requiredFlagIds`, `timeOfDay` |
| `DayStart` | 一日の開始時のイベント | `timeOfDay`, `weather`, `season` |
| `Location` | 場所に入ったとき、場所での発生イベント | `locationId`, `minAffection`, `maxAffection` |
| `Date` | デート、同行、特定行動後のイベント | `actionId`, `locationId`, `minAffection`, `maxAffection` |
| `Quest` | フラグ、アイテム、進行度条件つきイベント | `requiredFlagIds`, `requiredItemId`, `once` |
| `Weather` | 天候に応じたイベント | `weather`, `locationId`, `timeOfDay` |
| `Season` | 季節に応じたイベント | `season`, `locationId`, `timeOfDay` |
| `Scheduled` | 日付、進行度、シナリオ進行など予定イベント | `requiredFlagIds`, `timeOfDay` |

カテゴリ名は Unity 側でも同じ文字列で扱う。
変更する場合は、WPF 側の候補、Export 検証、Unity 側の発火処理を同時に更新する。

## 条件フィールド

| field | 意味 | 空の場合 |
| --- | --- | --- |
| `locationId` | 場所条件。例: `Forest`, `Lake`, `Cave`, `Room`, `Town` | 場所条件なし |
| `minAffection` | 必要な最小好感度 | 既定値 0 |
| `maxAffection` | 許可する最大好感度 | 既定値 9999 |
| `weather` | 天候条件。例: `Sunny`, `Rainy`, `Cloudy`, `Snow` | 天候条件なし |
| `season` | 季節条件。例: `Spring`, `Summer`, `Autumn`, `Winter` | 季節条件なし |
| `timeOfDay` | 時間帯条件。例: `Morning`, `Day`, `Evening`, `Night` | 時間帯条件なし |
| `actionId` | 行動条件。例: `Tea`, `Rest`, `Walk`, `Gift`, `Talk` | 行動条件なし |
| `costumeId` | 服装条件。例: `Default`, `Summer`, `Raincoat`, `Dress` | 服装条件なし |
| `requiredItemId` | 必要アイテム ID | アイテム条件なし |
| `once` | 1回だけ発火するイベントか | `false` |
| `requiredFlagIds` | 必要フラグ ID 一覧 | フラグ条件なし |

`requiredFlagIds` は改行、カンマ、セミコロン区切りで複数指定できる。

## イベント完了時の好感度

`イベント完了時変化` は `game_events_export.json.items[].affectionChange` として出力する。
Unityではイベントの全ページ表示完了時に反映し、`once=true` のイベントは表示済み記録と
同時に一度だけ確定する。開始演出や自動発生イベントで報酬が不要なら `0` を明示する。
入力可能範囲は `-9999〜9999`。制作状況のイベント詳細には符号付きの値を表示し、
範囲外は制作状況とExport前検証の両方でエラーにする。

## once とフラグ運用

`once=true` の `GameEvents` は、`requiredFlagIds` を必ず指定する。

理由は、Unity 側で「既に見たイベント」と「まだ発火条件を満たしていないイベント」を区別するため。
WPF 側では、`GameEvents` で `once=true` かつ `requiredFlagIds` が空の場合に警告する。

推奨例:

| category | requiredFlagIds 例 |
| --- | --- |
| `Intro` | `IntroNotSeen` |
| `Quest` | `QuestAvailable` |
| `Scheduled` | `Chapter01Started` |

Unity 側では、イベント発火後に対応する完了フラグを立てる。
現時点では WPF JSON に「完了後に立てるフラグ」は持たせていないため、Unity 側のイベント管理で扱う。

## イベントスチル参照

イベントスチルを使う場合は、`imageAssetIds` に画像の `AssetId` を入れる。

例:

```text
GameStartIntro_01
```

複数画像を使う場合は、改行、カンマ、セミコロンで区切る。

```text
GameStartIntro_01
GameStartIntro_02
```

Export 検証では、`imageAssetIds` が `Accepted` 画像を参照していない場合に警告する。
画像準備前でも先に予定 ID を入れてよいが、最終 export 前には該当画像を登録して `Accepted` にする。

## ID 命名

`イベント雛形` は、カテゴリから次の形式で ID を作る。

```text
Event_<Category>_<Number>
```

例:

```text
Event_Intro_01
Event_DayStart_01
Event_Location_02
```

手入力する場合も、Unity 側の更新キーとして使うため、同じ `kind` 内で一意にする。
WPF 側では ID 重複を警告する。

## Export JSON 例

```json
{
  "schemaVersion": 1,
  "heroineId": "TestHeroine",
  "kind": "GameEvents",
  "items": [
    {
      "id": "Event_Intro_01",
      "title": "導入イベント",
      "category": "Intro",
      "conditions": {
        "locationId": "Room",
        "minAffection": 0,
        "maxAffection": 100,
        "weather": "",
        "season": "",
        "timeOfDay": "Morning",
        "actionId": "",
        "costumeId": "",
        "requiredItemId": "",
        "once": true,
        "requiredFlagIds": [
          "IntroNotSeen"
        ]
      },
      "lines": [
        {
          "speaker": "主人公",
          "text": "導入イベントを開始する。",
          "expression": "Neutral"
        },
        {
          "speaker": "TestHeroine",
          "text": "ここにヒロインの反応を入力する。",
          "expression": "Smile"
        }
      ],
      "imageAssetIds": [
        "GameStartIntro_01"
      ],
      "priority": 100,
      "memo": ""
    }
  ]
}
```

## Unity 側の発火判定案

Unity 側では、候補イベントを次の順で絞り込む。

1. `category` が現在の発火タイミングに合う。
2. `locationId`、`weather`、`season`、`timeOfDay`、`costumeId` が現在状態に合う。
3. `minAffection` / `maxAffection` の範囲内。
4. `actionId`、`requiredItemId`、`requiredFlagIds` を満たす。
5. `once=true` のイベントが既読済みなら除外する。
6. 残った候補から `priority` が高いものを優先する。

同じ優先度のイベントが複数ある場合、Unity 側でランダムにするか、安定ソートで先頭を選ぶかを決める。
最初は安定ソートで先頭を選び、必要になったらランダムや重み付けを追加する。

## WPF 側の警告

WPF 側は会話データ一覧の `警告` 列と Export report で、主に次を警告する。

- `Id`、`Title`、`Category` が空。
- 同じ種別内で `Id` が重複している。
- `priority` が 0 未満。
- `minAffection` が `maxAffection` より大きい。
- `GameEvents` で `once=true` なのに `requiredFlagIds` が空。
- 台詞行がない、または話者、本文が空。
- 表情、場所、行動、天候、季節、時間帯、服装が候補外。
- `imageAssetIds` が `Accepted` 画像を参照していない。

画像準備を後回しにしている間は、画像関連警告は残っていてもよい。
本文と条件が固まったら、画像登録と `Accepted` 化で警告を消す。
