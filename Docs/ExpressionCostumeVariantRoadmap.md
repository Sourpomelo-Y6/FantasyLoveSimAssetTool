# Expression and Costume Variant Roadmap

このドキュメントは、表情差分、衣装差分の元データを作成、追加、管理できるようにするためのロードマップをまとめる。

現状は `StillDefinitionService` に固定のスチル定義を直接書いている。
この方式は少数の固定スチルには単純で扱いやすいが、表情、衣装、季節服、イベント用差分が増えると、コード変更なしで追加できない、会話側の `expression` 候補と画像側の `AssetId` の対応が崩れやすい、Unity 側でどの画像をどの表情として使うかが曖昧になる、という問題が出る。

## 目標

- 表情差分と衣装差分の元データを、コードではなくデータとして追加できるようにする。
- 表情 ID、衣装 ID、表示名、生成 prompt、出力ファイル名、Unity 側で使う ID を一貫して管理する。
- 表情差分の画像作成と、会話データの `expression` 値を対応させる。
- 衣装差分の画像作成と、季節、イベント、状態条件を対応させる。
- 既存のスチル作業タブ、ComfyUI 生成、画像採用、Unity export の流れは維持する。

## 基本方針

表情、衣装、スチル定義を分けて管理する。

- 表情マスタ: `Neutral`, `Smile`, `Sad`, `Angry` などの表情 ID と生成 prompt を持つ。
- 衣装マスタ: `Default`, `Spring`, `Summer`, `Dress` などの衣装 ID と生成 prompt を持つ。
- スチル生成ルール: どの表情、衣装の組み合わせを実際に作るかを定義する。
- キャラクターごとの作業状態: 生成済み、採用済み、要修正、追加 prompt などは従来通りキャラクターごとに保存する。

最初から全組み合わせを生成対象にしない。
表情と衣装を掛け合わせると数が急増するため、必要な組み合わせだけを明示的に生成対象にする。

## 透過レイヤー方式を採用する場合

表情差分と衣装差分を、完成済み立ち絵 PNG としてではなく、透過 PNG のレイヤー素材として用意する場合は設計が変わる。
この場合、WPF ツールは「完成画像を管理するツール」だけではなく、「Unity 側で重ねて表示するレイヤー素材を管理するツール」になる。

基本レイヤーは次を想定する。

```text
BaseBody        体、髪、基本シルエット
Costume         衣装レイヤー
Expression      表情レイヤー、または顔パーツ差分
Accessory       任意の小物、装飾
ComposedPreview WPF 側または Unity 側で確認する合成プレビュー
```

この方式では、`Heroine_Default_Smile.png` のような完成済み立ち絵を大量に作るのではなく、次のような素材を作る。

```text
Heroine_BaseBody.png
Costume_Default.png
Costume_Summer.png
Expression_Neutral.png
Expression_Smile.png
Expression_Angry.png
```

Unity 側では、現在の衣装 ID と表情 ID をもとに、複数の Sprite を同じ座標、同じ Sorting 設定で重ねる。
会話データの `expression` は `Expression` レイヤーの選択に使い、衣装状態や季節、イベント状態は `Costume` レイヤーの選択に使う。

### メリット

- 表情数と衣装数の組み合わせ爆発を避けられる。
- `Expression_Smile` を1枚作れば、複数衣装に再利用できる。
- Unity 側で会話中に表情だけ差し替えやすい。
- 衣装変更と表情変更を別々の状態として扱える。

### デメリット

- 画像生成側で、完全に位置が合った透過 PNG を作る必要がある。
- 表情、衣装、身体の境界がずれると破綻しやすい。
- Unity 側にレイヤー合成用の表示ロジックが必要になる。
- Export データも、単なる `assets_export.json` だけでは不足し、レイヤー種別や描画順を渡す必要がある。

このため、透過レイヤー方式を採用する場合は、Phase 6 の Unity export 拡張を早めに行う。
完成済み立ち絵方式よりも、WPF 側と Unity 側のデータ契約を先に固める必要がある。

## 想定データ

最終的には、次のような JSON を `Definitions/` などに置く。

```text
Definitions/
  expressions.json
  costumes.json
  sprite_variants.json
```

### expressions.json

```json
{
  "schemaVersion": 1,
  "expressions": [
    {
      "expressionId": "Neutral",
      "displayName": "通常",
      "prompt": "neutral expression",
      "unityExpressionId": "Neutral"
    },
    {
      "expressionId": "Smile",
      "displayName": "笑顔",
      "prompt": "gentle smile",
      "unityExpressionId": "Smile"
    }
  ]
}
```

### costumes.json

```json
{
  "schemaVersion": 1,
  "costumes": [
    {
      "costumeId": "Default",
      "displayName": "通常服",
      "prompt": "default outfit",
      "unityCostumeId": "Default"
    },
    {
      "costumeId": "Summer",
      "displayName": "夏服",
      "prompt": "summer outfit",
      "unityCostumeId": "Summer"
    }
  ]
}
```

### sprite_variants.json

```json
{
  "schemaVersion": 1,
  "variants": [
    {
      "assetId": "Heroine_Default_Neutral",
      "displayName": "立ち絵: 通常服 通常",
      "costumeId": "Default",
      "expressionId": "Neutral",
      "fileName": "Heroine_Default_Neutral.png",
      "enabledByDefault": true
    },
    {
      "assetId": "Heroine_Default_Smile",
      "displayName": "立ち絵: 通常服 笑顔",
      "costumeId": "Default",
      "expressionId": "Smile",
      "fileName": "Heroine_Default_Smile.png",
      "enabledByDefault": true
    }
  ]
}
```

`StillDefinitionService` は、このデータから `StillDefinition` を生成する。
`SpecificPrompt` は、キャラクター容姿 prompt、共通 positive prompt、衣装 prompt、表情 prompt、スチル固有 prompt を合成する。

### layer_assets.json

透過レイヤー方式を採用する場合は、`sprite_variants.json` に加えて、または代わりに、レイヤー素材を定義する。

```json
{
  "schemaVersion": 1,
  "layers": [
    {
      "assetId": "Heroine_BaseBody",
      "layerKind": "BaseBody",
      "displayName": "ベース体",
      "fileName": "Heroine_BaseBody.png",
      "drawOrder": 0,
      "prompt": "full body base character, neutral face area, transparent background"
    },
    {
      "assetId": "Costume_Summer",
      "layerKind": "Costume",
      "costumeId": "Summer",
      "displayName": "夏服",
      "fileName": "Costume_Summer.png",
      "drawOrder": 10,
      "prompt": "summer outfit layer only, transparent background, aligned to base body"
    },
    {
      "assetId": "Expression_Smile",
      "layerKind": "Expression",
      "expressionId": "Smile",
      "displayName": "笑顔",
      "fileName": "Expression_Smile.png",
      "drawOrder": 20,
      "prompt": "smiling face expression layer only, transparent background, aligned to base body"
    }
  ]
}
```

この場合の `StillDefinition` は、完成画像ではなく「レイヤー素材の作成対象」として扱う。
`AssetUsage` は当面 `Sprites` のままでよいが、将来は `SpriteLayer` のような用途を追加するか検討する。

## 段階的ロードマップ

### Phase 1: ドキュメントと命名規則を固定する

- 表情 ID、衣装 ID、AssetId、fileName の命名規則を決める。
- 会話データの `expression` と表情マスタの `expressionId` を対応させる。
- Unity 側で画像参照を `AssetId` で解決する方針を維持する。

推奨命名:

```text
Heroine_<CostumeId>_<ExpressionId>
Heroine_Default_Neutral.png
Heroine_Default_Smile.png
Heroine_Summer_Smile.png
```

この段階では、まだ実装を大きく変えず、追加する値をドキュメント化するだけでよい。

### Phase 2: 表情マスタを作る

- `ConversationValueCatalog.Expressions` の固定配列を、将来的に `expressions.json` から読める形へ移行する。
- まずは既存値をそのまま JSON に移す。
- 会話データタブの表情候補、Export 警告、スチル生成候補が同じ表情マスタを参照するようにする。

この段階で、会話で使う `expression` と、立ち絵画像として作る表情差分の元データがつながる。

### Phase 3: 衣装マスタを作る

- `Default`, `Spring`, `Summer`, `Autumn`, `Winter`, `Dress`, `NightDress`, `Raincoat` などを衣装マスタへ移す。
- 衣装ごとに表示名、prompt、Unity 側 ID を持たせる。
- 既存の `Heroine_Spring`, `Heroine_Summer` などは、衣装マスタから生成されるスチルとして扱う。

この段階では、衣装と表情の全組み合わせはまだ作らない。
既存の衣装立ち絵をデータ化することを優先する。

### Phase 4: スプライト差分生成ルールを作る

- `sprite_variants.json` を追加する。
- 表情と衣装のどの組み合わせを作るかを明示する。
- `enabledByDefault` を持たせ、最初から必要なものだけを作業対象にする。
- スチル作業タブでは、この差分定義から生成された `StillDefinition` を表示する。

例:

- 通常服: `Neutral`, `Smile`, `Sad`, `Angry`, `Shy`, `Surprised`
- 夏服: `Neutral`, `Smile`
- ドレス: `Neutral`, `Smile`

この方式なら、全組み合わせを強制せず、必要な差分だけを増やせる。

### Phase 5: GUIで元データを編集できるようにする

専用タブ、または設定画面として「差分定義」編集を追加する。

最初に必要な機能:

- 表情マスタ一覧
- 衣装マスタ一覧
- スプライト差分一覧
- 追加、削除、複製
- `AssetId` と `fileName` の自動生成
- prompt preview
- 重複 ID、空 prompt、未使用表情、未使用衣装の警告

この段階から、コードを触らずに表情、衣装、差分画像の作成対象を増やせる。

### Phase 6: Unity export を拡張する

`assets_export.json` に加えて、必要なら差分メタデータを出力する。

候補:

```text
Data/
  sprite_variants_export.json
```

内容:

- `assetId`
- `costumeId`
- `expressionId`
- `unityImagePath`
- `usage`

Unity 側では、`expressionId` と `costumeId` から表示する立ち絵を選べるようにする。
会話データの `expression` は `expressionId` として扱い、必要なら現在の衣装条件と組み合わせて表示画像を決める。

透過レイヤー方式を採用する場合は、`sprite_layers_export.json` を優先する。

```text
Data/
  sprite_layers_export.json
```

内容:

- `assetId`
- `layerKind`
- `costumeId`
- `expressionId`
- `unityImagePath`
- `drawOrder`
- `anchor`
- `canvasWidth`
- `canvasHeight`

Unity 側では、`layerKind` と `drawOrder` に従って SpriteRenderer、UI Image、または独自の立ち絵表示コンポーネントにレイヤーを割り当てる。
全レイヤーは同じキャンバスサイズ、同じ原点、同じ基準位置で作ることを必須にする。
`sprite_layers_export.json` の必須項目、Unity 側 ScriptableObject 案、Import 手順、fallback ルールは `Docs/UnityImportPlan.md` を正とする。

### Phase 6b: Unity 側レイヤー表示を追加する

透過レイヤー方式では、Unity 側に次の仕組みが必要になる。

- `HeroineLayeredSpriteData`
  - `heroineId`
  - `baseBody`
  - `costumeLayers`
  - `expressionLayers`
  - `accessoryLayers`
- `HeroineLayeredSpriteView`
  - 現在の `costumeId` と `expressionId` を受け取る
  - 必要な Sprite を `drawOrder` 順に表示する
  - 存在しない組み合わせは fallback する
- fallback ルール
  - 表情が存在しない場合は `Neutral`
  - 衣装が存在しない場合は `Default`
  - ベース体がない場合は表示エラーとして扱う

会話データの `expression` は、完成立ち絵の `AssetId` ではなく `expressionId` として扱う。
衣装は会話データに直接持たせるのではなく、ゲーム状態、季節、イベント、プレイヤー操作などから現在衣装を決める。

### Phase 7: 高度な差分運用

必要になった段階で進める。

- 表情差分だけを同じ衣装、同じ構図で作るための img2img / reference workflow
- 透過 PNG、キャンバスサイズ、立ち位置の一致検査
- 表情差分同士の顔位置検査
- 衣装差分ごとの季節、イベント、好感度条件
- Unity 側での fallback ルール
  - 指定衣装、指定表情がない場合は `Default + Neutral`
  - 指定衣装の表情がない場合は同衣装の `Neutral`

## 実装順の推奨

最初にやるべきは、Phase 2 と Phase 3 の「外部 JSON 化」ではなく、Phase 1 の命名規則固定と、Phase 4 の生成ルール案の確定。

理由は、表情と衣装は組み合わせ爆発しやすく、先に GUI や自動生成を作ると不要な差分を大量に管理する設計になりやすいため。
まず「どの組み合わせを作るか」を明示するルールを決める。

実装としては次の順が現実的。

1. `Docs/ExpressionCostumeVariantRoadmap.md` で命名規則とデータ構造を固定する。
2. `Definitions/expressions.json` と `Definitions/costumes.json` を追加する。
3. `StillDefinitionService` が固定定義に加えて JSON 定義を読めるようにする。
4. `ConversationValueCatalog.Expressions` を `expressions.json` 由来に寄せる。
5. 完成済み立ち絵方式なら `Definitions/sprite_variants.json` を追加する。
6. 透過レイヤー方式なら `Definitions/layer_assets.json` を追加する。
7. スチル作業タブで、差分定義またはレイヤー定義由来の `StillDefinition` を扱えることを確認する。
8. GUI上で表情、衣装、差分定義、レイヤー定義を編集できるようにする。
9. Unity export に `sprite_variants_export.json` または `sprite_layers_export.json` を追加する。
10. Unity 側に完成立ち絵参照方式、またはレイヤー合成方式の表示コンポーネントを実装する。

透過レイヤー方式を選ぶなら、実装順は次を推奨する。

1. `expressions.json` と `costumes.json` を追加する。
2. `layer_assets.json` を追加し、BaseBody、Costume、Expression を定義する。
3. WPF export に `sprite_layers_export.json` を追加する。
4. Unity 側に `HeroineLayeredSpriteData` と `HeroineLayeredSpriteView` を追加する。
5. WPF 側でレイヤー素材の透過 PNG 検査、サイズ一致検査を追加する。
6. GUI でレイヤー定義を追加、複製、削除できるようにする。

## 現時点で手動追加する場合

大改造前に、今の構造で表情差分を追加する場合は次を編集する。

1. `StillDefinitionService` に `Create(...)` を追加する。
2. 会話データで使う表情名なら `ConversationValueCatalog.Expressions` に追加する。
3. 必要なら `Docs/ConversationDataPlan.md` と `Docs/UnityImportPlan.md` の表情値一覧を更新する。

例:

```csharp
Create("Heroine_Default_Angry", "立ち絵: 通常服 怒り", AssetUsage.Sprites, "Heroine_Default_Angry.png", "standing character sprite, full body, default outfit, angry expression, transparent background")
```

この手動方式は短期的には有効だが、差分が増えるほど保守が難しくなる。
そのため、長期的には外部 JSON 化と GUI 編集へ移行する。

透過レイヤー方式を手動で試す場合は、完成済み立ち絵の `AssetId` ではなく、まず次のようなレイヤー素材を通常の `Sprites` として登録する。

```text
Heroine_BaseBody
Costume_Default
Expression_Neutral
Expression_Smile
```

ただし、現状の Unity export はこれらを「単独の画像」として渡すだけで、レイヤー合成情報は渡さない。
Unity 側で重ねて使うには、追加で `sprite_layers_export.json` 相当のデータ契約が必要になる。
