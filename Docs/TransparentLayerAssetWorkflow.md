# Transparent Layer Asset Workflow

このドキュメントは、表情差分、衣装差分を Unity 側で重ねて表示するための透過 PNG レイヤー素材を用意する手順をまとめる。

関連する export 契約は `Docs/UnityImportPlan.md` の `sprite_layers_export.json`、全体ロードマップは `Docs/ExpressionCostumeVariantRoadmap.md` を参照する。

## 基本方針

透過版の素材は、完成済み立ち絵を大量に作るのではなく、同じキャンバスサイズ、同じキャラクター位置で次のレイヤーを作る。

```text
BaseBody    体、髪、基本シルエット
Costume     衣装差分
Expression  表情差分、顔パーツ差分
Accessory   小物、装飾
```

Unity 側では、これらを `drawOrder` 順に重ねる。
そのため、すべての透過 PNG は BaseBody と同じ解像度、同じ原点、同じキャラクター位置で作る必要がある。

## 最初に用意する最小セット

最初は次の5枚を用意する。

```text
Heroine_BaseBody.png
Costume_Default.png
Costume_Summer.png
Expression_Neutral.png
Expression_Smile.png
```

最低限 Unity で安定表示するには、次が必要になる。

- `BaseBody` が1枚ある
- `Costume` の `Default` が1枚ある
- `Expression` の `Neutral` が1枚ある
- すべて同じキャンバスサイズである
- すべて透過 PNG である
- 画像内の位置がずれていない

## 推奨キャンバス

当面は 1024 x 1024 の正方形を基準にする。
ComfyUI workflow でも 1024 x 1024 を使っているため、生成と検査を合わせやすい。

将来、縦長立ち絵に寄せる場合でも、重要なのは全レイヤーでサイズを統一することである。
途中で 1024 x 1536 などに変える場合は、BaseBody、Costume、Expression、Accessory をすべて同じサイズで作り直す。

## 作り方 A: 生成画像から手作業で切り出す

もっとも現実的な初期運用は、まず完成立ち絵を作り、それを画像編集ソフトでレイヤー化する方法である。

手順:

1. Base になる完成立ち絵を作る。
2. 画像編集ソフトでキャンバスサイズを固定する。
3. 背景を透明にする。
4. 体、髪、肌、輪郭など常に表示する部分を `Heroine_BaseBody.png` として残す。
5. 通常服だけを残した透過 PNG を `Costume_Default.png` として保存する。
6. 夏服など別衣装だけを残した透過 PNG を `Costume_Summer.png` として保存する。
7. 通常表情だけを残した透過 PNG を `Expression_Neutral.png` として保存する。
8. 笑顔だけを残した透過 PNG を `Expression_Smile.png` として保存する。
9. すべての PNG を重ねて、完成立ち絵として破綻しないか確認する。

注意:

- レイヤーを書き出すときに、透明部分をトリミングしない。
- PNG の実寸が変わると Unity 側で位置がずれる。
- 顔だけ、服だけの画像でも、キャンバス全体は BaseBody と同じサイズにする。
- Expression は顔パーツだけでもよいが、BaseBody 側に同じ顔が残っていると二重表示になる。

## 作り方 B: ComfyUI で透過 PNG を直接作る

ComfyUI で直接レイヤー素材を生成する場合は、プロンプトで「単体レイヤー」「透明背景」「BaseBody と同じ位置」を強く指定する。

BaseBody の例:

```text
full body base character, same pose, centered, transparent background, no clothes layer detail, neutral face area, clean alpha, fixed canvas, aligned to reference
```

Costume の例:

```text
default outfit layer only, transparent background, no body, no face, same pose, fixed canvas, aligned to base body, clean alpha
```

Expression の例:

```text
smiling face expression layer only, transparent background, face parts only, no hair, no body, fixed canvas, aligned to base body, clean alpha
```

ただし、画像生成だけで完全に位置が合う透過レイヤーを作るのは難しい。
実運用では、生成後に画像編集ソフトで位置、余白、不要部分、アルファを調整する前提にする。

## 作り方 C: 完成画像から透過差分だけを作る

完成済みの `Default_Neutral`、`Default_Smile` などがある場合は、差分を切り出して表情レイヤーを作ることもできる。

手順:

1. 同じポーズ、同じ解像度の完成画像を複数用意する。
2. Neutral 画像を基準にする。
3. Smile 画像から口、目、眉など変化した部分だけを残す。
4. 残りを透明にして `Expression_Smile.png` として保存する。
5. Neutral 表情と差し替えて見たときに破綻しないか確認する。

この方法は、元画像同士の位置が完全に一致している場合に有効である。
位置が少しでも違う場合は、顔の輪郭や髪の境界でずれが目立つ。

## WPF ツールへの登録

透過 PNG ができたら、WPF ツールでは通常の画像と同じく登録する。

1. キャラクターを選ぶ。
2. 差分定義タブで `Definitions/layer_assets.json` のレイヤー定義を確認する。
3. 足りないレイヤーがあれば、差分定義タブで追加する。
4. スチル作業タブで対象レイヤーを選ぶ。
5. 透過 PNG を画像登録欄に登録する。
6. AssetId がレイヤー定義の `assetId` と一致することを確認する。
7. Status を `Accepted` にする。
8. レイヤープレビュータブで BaseBody、Costume、Expression を重ねて確認する。
9. Export を実行する。

Export では、`Definitions/layer_assets.json` に定義され、かつ `Accepted` 済みの素材が `Data/sprite_layers_export.json` に出力される。
未採用のレイヤー素材は warning に出る。

## layer_assets.json の例

```json
{
  "schemaVersion": 1,
  "layers": [
    {
      "assetId": "Heroine_BaseBody",
      "layerKind": "BaseBody",
      "displayName": "レイヤー: ベース体",
      "fileName": "Heroine_BaseBody.png",
      "drawOrder": 0,
      "prompt": "full body base character, neutral face area, transparent background"
    },
    {
      "assetId": "Costume_Default",
      "layerKind": "Costume",
      "costumeId": "Default",
      "displayName": "レイヤー: 通常服",
      "fileName": "Costume_Default.png",
      "drawOrder": 10,
      "prompt": "default outfit layer only, transparent background, aligned to base body"
    },
    {
      "assetId": "Expression_Smile",
      "layerKind": "Expression",
      "expressionId": "Smile",
      "displayName": "レイヤー: 表情 笑顔",
      "fileName": "Expression_Smile.png",
      "drawOrder": 20,
      "prompt": "smiling face expression layer only, transparent background, aligned to base body"
    }
  ]
}
```

## 命名規則

推奨する `assetId` と fileName:

```text
Heroine_BaseBody          -> Heroine_BaseBody.png
Costume_Default           -> Costume_Default.png
Costume_<CostumeId>       -> Costume_<CostumeId>.png
Expression_Neutral        -> Expression_Neutral.png
Expression_<ExpressionId> -> Expression_<ExpressionId>.png
Accessory_<Name>          -> Accessory_<Name>.png
```

`assetId` は WPF と Unity の主キーになる。
表示名やファイル名ではなく、`assetId` を基準に管理する。

## 検査項目

WPF の Export warning でも一部確認するが、作業時には次を手動確認する。

- 透過 PNG である
- 背景が透明である
- BaseBody と解像度が一致している
- BaseBody と縦横比が一致している
- キャンバスがトリミングされていない
- 表情レイヤーに不要な髪、肌、服が残っていない
- 衣装レイヤーに不要な顔、背景が残っていない
- `Default` 衣装がある
- `Neutral` 表情がある
- レイヤープレビューで位置ずれがない
- Export 後に `sprite_layers_export.json` に出ている

## Unity 側での確認

Unity 側では Import 後に次を確認する。

- 画像が `Assets/Images/Heroines/<HeroineId>/Sprites/` にコピーされている
- Texture Type が `Sprite` になっている
- `HeroineLayeredSpriteData.asset` に `Sprite` 参照が入っている
- BaseBody、Costume、Expression が `drawOrder` 順に表示される
- 表情指定がない場合は `Neutral` に fallback する
- 衣装指定がない場合は `Default` に fallback する

## 判断基準

透過レイヤー方式は、表情や衣装の組み合わせが増えるほど有利になる。
一方で、画像の位置合わせと Unity 側の表示制御が必要になる。

少数の固定立ち絵だけで足りる段階では、完成済み PNG をそのまま使う方が速い。
会話中に表情だけを頻繁に切り替える、衣装と表情を独立して管理したい、季節服やイベント服を増やしたい場合は、透過レイヤー方式を優先する。
