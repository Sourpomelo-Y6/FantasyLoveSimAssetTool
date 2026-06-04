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
3. Stable Diffusion で画像を生成し、用途別に採用画像を選ぶ
4. 採用画像を Unity 向けのフォルダ構成とファイル名へ整形して export する
5. Unity 側で `Assets/Images/Heroines/<HeroineId>/...` と `Assets/Resources/Heroines/<HeroineId>/...` に取り込む

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

## スチル一覧タブ

基本的に、この仕様書の「出力ファイル命名」にあるスチル一覧は常に描く対象として扱う。

ツールには新しいタブとして「スチル一覧」を用意し、立ち絵、イベントスチル、行動スチル、エンディングスチルを一覧表示する。各行は、生成したいスチル 1 枚分の作業単位とする。

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

スチル一覧タブでは、行を選択して「Prompt に反映」すると、合成後の positive prompt を選択中スチルの `PromptRecord.PositivePrompt` に反映できるようにする。

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
- 生成結果を登録する
- 採用・保留・没を管理する
- 登録済み画像をプレビューする
- 登録後に採用・保留・没を切り替えられる
- 採用画像のファイル名を Unity 用に決める

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

- Unity の ScriptableObject を YAML として自動生成する
- Unity Editor 拡張で export 結果を取り込む
- JSON から `ConversationData` や `GameEventData` を生成する
- 画像の解像度、縦横比、透過、余白を自動チェックする
- 立ち絵の背景透過や表情差分の整合性をチェックする
- 複数ヒロイン間でプロンプトテンプレートを共有する
- スチル用途別のデフォルトプロンプトテンプレートを編集、追加、共有する
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
