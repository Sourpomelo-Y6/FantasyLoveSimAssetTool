# キャラクター素材生成ツール仕様

このドキュメントは、別リポジトリまたは別フォルダで作成する、ヒロインキャラクター用データと Stable Diffusion 画像素材を管理・生成するためのツール仕様です。
Unity プロジェクト本体とは分けて運用し、生成した成果物だけを `FantasyLoveSim` に取り込む前提にします。

## 目的

- ヒロインごとの立ち絵、衣装差分、イベントスチル、行動スチル、エンディングスチルをまとめて生成・管理する
- Stable Diffusion 用のプロンプト、ネガティブプロンプト、Seed、モデル、LoRA、ControlNet 設定を記録する
- 画像だけでなく、ヒロイン設定、口調、会話方針、イベント案、行動反応案も同じキャラクター単位で管理する
- Unity 側の `HeroineProfileData` と `Assets/Images/Heroines/<HeroineId>/...` に取り込みやすい形で出力する

## 想定する運用

1. 別リポジトリにキャラクターごとの素材生成プロジェクトを作る
2. ツール上で `HeroineId`、名前、外見設定、性格、口調、衣装、生成プロンプトを登録する
3. Stable Diffusion や ComfyUI などで画像を生成し、用途別に採用画像を選ぶ
4. 採用画像を Unity 向けのフォルダ構成とファイル名へ整形して export する
5. Unity 側で `Assets/Images/Heroines/<HeroineId>/...` と `Assets/Resources/Heroines/<HeroineId>/...` に取り込む

画像生成は、外部ツールで生成済みの画像ファイルを登録する運用を基本にする。
ローカル ComfyUI 連携を追加する場合も、この外部ファイル登録フローは残し、ComfyUI 生成結果を同じ登録処理に渡せるようにする。

## 管理対象

### キャラクター基本情報

- `heroineId`
- 表示名
- 年齢・身長などの任意プロフィール
- 性格
- 口調
- 一人称・二人称
- 好きなもの、苦手なもの
- 行動反応の方向性
- エンディング方針

### 画像用途

- `Sprites`: 通常立ち絵、衣装差分、表情差分
- `Event`: ゲーム開始、日常イベント、予定イベントなどのイベントスチル
- `Actions`: 行動結果や行動反応用のスチル
- `Ending`: エンディング用スチル

Unity 側の取り込み先は次を基本にする。

```text
Assets/Images/Heroines/<HeroineId>/Sprites/
Assets/Images/Heroines/<HeroineId>/Event/
Assets/Images/Heroines/<HeroineId>/Actions/
Assets/Images/Heroines/<HeroineId>/Ending/
```

### 会話・イベント案

画像生成ツール側では、Unity の ScriptableObject を直接作る必要はない。
ただし、次の下書きをキャラクター単位で持てるようにする。

- ジャンル会話案
- 好感度条件会話案
- 天候・季節・時間帯条件会話案
- ゲーム開始イベント案
- 日開始イベント案
- 行動反応案
- エンディング本文案

最終的には Unity 側で `ConversationData`、`GameEventData`、`ActionReactionData`、`EndingData` に手動または変換ツールで反映する。

### Unity .asset と会話データ作成方針

Unity の `.asset` は ScriptableObject の保存ファイルであり、Asset Serialization が Force Text の場合は YAML として外部から読み書きできる。
ただし、`.asset` を外部ツールから直接生成する場合は、`.meta` の GUID、ScriptableObject の型情報、fileID、Assembly 名、Unity バージョン差分を正しく扱う必要がある。
そのため、このツールから `.asset` を直接出力することは将来拡張としても優先しない。

会話データを作成できるようにする場合は、次の段階的な方式を基本にする。

1. WPF ツール側で会話、イベント、行動反応、エンディング本文を JSON または Markdown として編集、出力する
2. Unity プロジェクト側に Editor 拡張を用意する
3. Unity Editor 内で JSON を読み込む
4. Unity Editor 側で `ConversationData`、`GameEventData`、`ActionReactionData`、`EndingData` の ScriptableObject `.asset` を生成、更新する

この方式なら、`.asset` の GUID や型情報は Unity Editor が管理できる。
WPF ツール側は、Unity に渡す中間データの作成と整形に集中する。

会話データ export の候補ファイルは次の通り。

```text
Data/
  heroine_profile_note.md
  conversations_export.json
  game_events_export.json
  action_reactions_export.json
  endings_export.json
```

当面は既存の下書き Markdown を維持し、後の段階で JSON export と Unity Editor Import を追加する。

## 出力ファイル命名

ファイル名は Unity 側の ID と対応しやすいように、用途と連番を含める。

### 立ち絵

```text
Heroine_Normal.png
Heroine_Smile.png
Heroine_Spring.png
Heroine_Summer.png
Heroine_Autumn.png
Heroine_Winter.png
Heroine_Dress.png
Heroine_NightDress.png
Heroine_Raincoat.png
```

### イベントスチル

```text
GameStartIntro_01.png
DayStart_Routine_01.png
DayStart_Rainy_01.png
WithForest_01.png
WithLake_01.png
WithCave_01.png
```

### 行動スチル

```text
Tea_01.png
Rest_01.png
Walk_01.png
Gift_01.png
```

### エンディングスチル

```text
GoodEnding_01.png
NormalEnding_01.png
BadEnding_01.png
```

## Stable Diffusion 設定の記録

採用画像ごとに次を保存する。

- positive prompt
- negative prompt
- model
- VAE
- LoRA
- sampler
- steps
- CFG scale
- seed
- image size
- ControlNet / reference image の有無
- upscale 設定
- inpaint / img2img の履歴
- 採用理由と修正メモ

画像ファイルとは別に、同名の JSON または YAML を置くと追跡しやすい。

```text
GameStartIntro_01.png
GameStartIntro_01.prompt.json
```

## プロンプトテンプレート

各種スチル用のデフォルトプロンプトテンプレートを用意する。

キャラクターごとに「容姿を表すプロンプト」を登録し、それをスチル用途ごとのテンプレートと合成することで、立ち絵、イベントスチル、行動スチル、エンディングスチル用の生成プロンプトを作れるようにする。

テンプレートは `PromptTemplates/templates.json` で管理する。
JSON が存在しない、空、不正な場合は、アプリ内のデフォルトテンプレートへ fallback する。

```json
[
  {
    "templateId": "sprites_normal",
    "displayName": "立ち絵: 通常",
    "usage": "Sprites",
    "templateText": "{CharacterAppearancePrompt}, standing character sprite, full body, transparent background"
  }
]
```

`usage` は `Sprites`, `Event`, `Actions`, `Ending` のいずれかを指定する。
`templateText` には `{CharacterAppearancePrompt}` を含め、キャラクター容姿プロンプトと合成できるようにする。

### キャラクター容姿プロンプト

キャラクター詳細に、次のような共通の容姿プロンプトを持たせる。

- 髪型、髪色
- 目の色、表情の傾向
- 体型、身長感
- 服装の基本方針
- キャラクター固有の特徴
- 絵柄や品質指定の基本要素

例:

```text
long silver hair, blue eyes, gentle smile, petite girl, fantasy heroine, soft anime style
```

### スチル用デフォルトテンプレート

用途別に、デフォルトのスチル用プロンプトを持たせる。

- `Sprites`: 通常立ち絵、表情差分、衣装差分
- `Event`: ゲーム開始、日常イベント、場所イベント
- `Actions`: お茶、休憩、散歩、贈り物などの行動反応
- `Ending`: Good / Normal / Bad Ending

例:

```text
{CharacterAppearancePrompt}, standing character sprite, full body, transparent background
{CharacterAppearancePrompt}, romantic event still, forest background, warm sunlight
{CharacterAppearancePrompt}, drinking tea with the player, cozy room, gentle atmosphere
{CharacterAppearancePrompt}, good ending still, emotional smile, cinematic composition
```

### 合成機能

ツール上では、次の流れでプロンプトを作れるようにする。

1. キャラクター詳細に容姿プロンプトを登録する
2. 画像用途を選ぶ
3. 用途別のデフォルトテンプレートを選ぶ
4. `{CharacterAppearancePrompt}` などのプレースホルダーをキャラクター固有の容姿プロンプトで置換する
5. 生成された positive prompt を `PromptRecord` に反映する

この機能により、キャラクターの外見の一貫性を保ちながら、各種スチル用のプロンプトを効率よく作れるようにする。

## ローカル ComfyUI 連携

ローカルで起動している ComfyUI に対して、ツール上で組み立てた prompt を送り、生成された画像を取得して登録できる機能を将来追加する。

この機能は、外部ツールで生成済みの画像ファイルを登録する現行フローを置き換えるものではない。
ComfyUI が使えない環境、手動で生成した画像、別ツールで生成した画像も、引き続きファイル選択またはドラッグ&ドロップで登録できるようにする。

### 想定する接続先

既定では、ローカル ComfyUI の HTTP API を使う。

```text
http://127.0.0.1:8188
```

接続先 URL は環境により変わる可能性があるため、将来は設定画面または設定 JSON で変更できるようにする。

ComfyUI 連携設定は `ComfySettings/comfyui.json` で管理する。
workflow template は `ComfySettings/workflow-template.json` に置く。
現時点では設定の読み込み、Prompt タブ上での確認、positive / negative prompt を差し込んだ workflow preview 作成までを実装し、ComfyUI への HTTP 送信、生成進捗取得、画像取得は後続タスクとする。

```json
{
  "endpointUrl": "http://127.0.0.1:8188",
  "workflowTemplatePath": "ComfySettings/workflow-template.json",
  "positivePromptPlaceholder": "{PositivePrompt}",
  "negativePromptPlaceholder": "{NegativePrompt}",
  "outputNodeId": "7",
  "positivePromptNodeId": "2",
  "negativePromptNodeId": "3"
}
```

### ComfyUI に渡す情報

ComfyUI 連携では、次の情報を workflow JSON に差し込む。

- positive prompt
- negative prompt
- seed
- 画像サイズ
- モデル、LoRA、VAE などの workflow 側設定
- 出力先または出力ノード名

positive prompt は、キャラクター容姿プロンプトとスチル固有プロンプトを合成したものを使う。
negative prompt は、`PromptRecord.NegativePrompt` または用途別テンプレートから取得する。

### 生成から登録までの流れ

1. スチル作業画面で対象スチルを選ぶ
2. 合成 positive prompt を確認する
3. 必要に応じて Prompt 記録へ反映する
4. workflow JSON に prompt と生成条件を差し込む
5. ローカル ComfyUI に生成リクエストを送る
6. 生成完了後、出力画像を取得してツール上でプレビューする
7. 採用する画像を選び、既存の画像登録処理に渡す
8. 登録時は既存 `AssetId` の上書き確認を表示する

取得した画像は、外部ファイル登録と同じ保存ルールで `Characters/<HeroineId>/Images/<Usage>/<AssetId>.png` にコピーする。
これにより、外部ファイル登録、ドラッグ&ドロップ登録、ComfyUI 生成結果登録の保存形式を揃える。

### 記録するメタデータ

ComfyUI で生成した画像については、採用時に次を `PromptRecord` へ記録できるようにする。

- 使用した positive prompt
- 使用した negative prompt
- seed
- workflow JSON または workflow 名
- ComfyUI の出力ファイル名
- 採用理由、修正メモ

workflow JSON をそのまま保存するか、workflow 名と差し込み値だけを保存するかは未決とする。

## スチル作業とスチル一覧

基本的に、この仕様書の「出力ファイル命名」にあるスチル一覧は常に描く対象として扱う。

ツールには、作業向けの「スチル作業」と、確認向けの「スチル一覧」を用意する。

「スチル作業」は、制作中に主に使う画面とする。用途フィルタで対象を絞り、選択したスチルの詳細、追加 prompt、合成 prompt、画像登録状況、prompt 保存状況、AssetStatus、登録済み画像プレビューを確認できるようにする。

「スチル一覧」は、仕様上必要なスチルを表形式で確認する画面とする。全項目を横断的に確認したいとき、または開発中のデバッグ用に使う。

### 常に描きたいスチル

初期状態では、次のスチル項目をキャラクターごとに持たせる。

- `Heroine_Normal`
- `Heroine_Smile`
- `Heroine_Spring`
- `Heroine_Summer`
- `Heroine_Autumn`
- `Heroine_Winter`
- `Heroine_Dress`
- `Heroine_NightDress`
- `Heroine_Raincoat`
- `GameStartIntro_01`
- `DayStart_Routine_01`
- `DayStart_Rainy_01`
- `WithForest_01`
- `WithLake_01`
- `WithCave_01`
- `Tea_01`
- `Rest_01`
- `Walk_01`
- `Gift_01`
- `GoodEnding_01`
- `NormalEnding_01`
- `BadEnding_01`

### スチルごとの管理項目

スチル一覧の各行では、次を確認、編集できるようにする。

- `AssetId`
- 用途: `Sprites` / `Event` / `Actions` / `Ending`
- 表示名
- 出力ファイル名
- スチル固有の追加プロンプト
- 合成後の positive prompt
- 対応する採用画像
- prompt 記録の有無
- 状態: 未生成 / 生成中 / 採用済み / 要修正 / 不要

スチル定義のうち、仕様として固定される `AssetId`、用途、表示名、出力ファイル名は `StillDefinitionService` の固定定義から生成する。

キャラクターごとに変わる `SpecificPrompt` と `Status` は、`HeroineProfile.StillWorkItems` として `profile.json` に保存する。これにより、アプリ再起動やキャラクター再読み込み後も、スチルごとの作業状態と追加 prompt を復元できる。

`StillWorkItems` は次の項目を持つ。

- `AssetId`
- `Status`
- `SpecificPrompt`

### スチル作業画面

スチル作業画面では、左側に作成対象リスト、右側に選択スチルの詳細を表示する。

左側の作成対象リストでは、次を表示する。

- 表示名
- `AssetId`
- スチル状態
- 用途フィルタ: `All` / `Sprites` / `Event` / `Actions` / `Ending`

右側の詳細では、次を表示、編集できるようにする。

- `AssetId`
- スチル状態
- 用途
- 出力ファイル名
- スチル固有の追加 prompt
- キャラクター容姿 prompt と追加 prompt を合成した positive prompt
- 画像登録状況: 未登録 / 登録済み / ファイルなし
- prompt 保存状況: 未保存 / 保存済み
- `HeroineAsset.AssetStatus`
- 登録済み画像プレビュー

スチル作業画面には、次の操作を用意する。

- `Prompt に反映`: 選択スチルの合成 positive prompt を `PromptRecord.PositivePrompt` に反映する
- `画像登録欄に反映`: 選択スチルの `AssetId`、用途、初期状態を画像登録欄に反映する
- `Comfy 作成`: 選択スチルの合成 positive prompt から ComfyUI workflow preview を作成する
- `スチル保存`: 選択キャラクターの `profile.json` に `StillWorkItems` を保存する

画像登録は、外部ツールで生成した画像を選んでアプリに登録する操作として扱う。スチル作業画面は、登録するべき `AssetId` と用途を間違えないための導線を提供する。

### プロンプト合成

各スチルには、キャラクターの基本プロンプトに加えて、そのスチル画像を得るための追加プロンプトを持たせる。

生成用 prompt は次のように組み立てる。

```text
{CharacterAppearancePrompt}, {StillSpecificPrompt}
```

例:

```text
long silver hair, blue eyes, gentle smile, petite girl, fantasy heroine, soft anime style,
standing character sprite, full body, transparent background
```

スチル作業画面またはスチル一覧タブでは、行を選択して「Prompt に反映」すると、合成後の positive prompt を選択中スチルの `PromptRecord.PositivePrompt` に反映できるようにする。

### 採用画像との対応

スチル一覧の `AssetId` は、画像登録時の `HeroineAsset.AssetId` と対応させる。

同じ `AssetId` の採用画像がある場合は、そのスチルは「採用済み」として扱える。未登録の場合は「未生成」または「未採用」として表示し、必要なスチルの抜けを一覧で確認できるようにする。

## ツールの画面案

### キャラクター一覧

- 登録済みキャラクターを一覧表示する
- `HeroineId`、表示名、作成状況、採用済み画像数を確認できる
- 新規キャラクター作成、編集、export を実行できる

### キャラクター詳細

- 基本設定
- 口調・会話方針
- 衣装一覧
- 画像用途別リスト
- キャラクター容姿プロンプト
- Stable Diffusion プロンプトテンプレート
- Unity 出力設定

### 画像生成・採用画面

- 用途を選ぶ
- プロンプトテンプレートから生成用プロンプトを作る
- キャラクター容姿プロンプトとスチル用テンプレートを合成する
- ローカル ComfyUI が使える場合は、合成 prompt を送信して生成画像を取得する
- 生成結果を登録する
- 外部ツールで生成済みの画像ファイルも従来通り登録できる
- 元画像欄へ画像ファイルをドラッグ&ドロップして登録元を指定する
- 既存 `AssetId` へ登録する場合は上書き確認を表示する
- 採用・保留・没を管理する
- 登録済み画像をプレビューする
- 登録後に採用・保留・没を切り替えられる
- 採用画像のファイル名を Unity 用に決める

### スチル作業画面

- 用途フィルタで対象スチルを絞り込む
- 選択中スチルの詳細を表示する
- スチル固有 prompt を編集する
- 合成 positive prompt をプレビューする
- 画像登録状況、prompt 保存状況、AssetStatus を表示する
- 登録済み画像をプレビューする
- `Prompt に反映` で prompt 記録へ反映する
- `画像登録欄に反映` で画像登録タブへ登録情報を渡す
- `Comfy 作成` で選択スチルの合成 prompt から workflow preview を作る

### スチル一覧画面

- 仕様上、常に描きたいスチルを一覧表示する
- スチルごとの用途、AssetId、ファイル名、状態を確認できる
- キャラクター容姿プロンプトとスチル固有プロンプトを合成できる
- 合成した positive prompt を prompt 記録へ反映できる
- 採用済み画像の有無を確認できる

### Export 画面

- Unity 向けフォルダ構成で出力する
- 採用画像だけを出力する
- prompt 記録を同梱するか選べる
- `HeroineProfileData` 作成用のメモまたは JSON を出力する
- 出力画像数、出力 prompt 数、スキップ数、警告を表示する
- Export 結果フォルダを開く

## 出力フォルダ例

ツール側の export 結果は次のようにする。

```text
Export/
  TestHeroine/
    Images/
      Sprites/
      Event/
      Actions/
      Ending/
    Data/
      heroine_profile_note.md
      conversations_draft.md
      game_events_draft.md
      action_reactions_draft.md
      endings_draft.md
    Prompts/
      GameStartIntro_01.prompt.json
```

Unity に取り込むときは、`Images` 配下を次へコピーする。

```text
Assets/Images/Heroines/<HeroineId>/
```

`Data` 配下は Unity Editor 上で ScriptableObject を作るときの参照資料として使う。

## 将来の拡張

- Unity Editor 拡張で export 結果を取り込む
- JSON から `ConversationData` や `GameEventData` を生成する
- 会話、イベント、行動反応、エンディング本文の編集画面を追加する
- WPF ツールから Unity 用の会話データ JSON を export する
- 余白、立ち絵の切れ、表情差分の整合性などの高度な画像検査を追加する
- 複数ヒロイン間でプロンプトテンプレートを共有する
- スチル用途別のデフォルトプロンプトテンプレートを編集、追加、共有する
- ローカル ComfyUI へ prompt を送信し、生成画像を取得する
- ComfyUI workflow JSON のテンプレート管理と設定画面を追加する
- Export 結果フォルダを開く
- 登録済み画像の差し替え、削除に対応する

## 最初に作る最小機能

最初は大きな自動化を狙わず、次だけ作ればよい。

1. キャラクター基本情報を JSON または YAML で保存する
2. 画像用途別フォルダを作成する
3. 採用画像と prompt 記録を同じ ID で保存する
4. Unity 向け export フォルダを作る
5. `heroine_profile_note.md` を出力する

この段階で、Stable Diffusion 画像生成と Unity 取り込みの作業を分離しつつ、後から自動化しやすい形にできる。
