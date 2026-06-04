# FantasyLoveSimAssetTool 引継ぎ

## 概要

`FantasyLoveSimAssetTool` は、Unity プロジェクト `FantasyLoveSim` 向けのヒロイン画像素材とキャラクター設定を管理する WPF アプリです。

目的は、Stable Diffusion などで生成した画像をキャラクター単位で整理し、採用画像と prompt 記録を追跡しながら、Unity に取り込みやすいフォルダ構成へ export することです。

仕様の詳細は `Docs/CharacterAssetGenerationToolSpec.md`、リポジトリ概要は `ReadMe.md` を参照してください。

## 現在の状態

現在は WPF アプリのひな形段階です。

- ソリューション: `FantasyLoveSimAssetTool.sln`
- アプリ本体: `FantasyLoveSimAssetTool/`
- ターゲットフレームワーク: `net5.0-windows`
- UI: `Views/MainWindow.xaml`
- ViewModel: `ViewModels/MainWindowModel.cs`
- 共通基盤: `Common/ObservableObject.cs`, `Common/RelayCommand.cs`

実装済みの機能は、`MainWindow` に `MainWindowModel.Text` を表示する最小構成のみです。

次の機能は未実装です。

- キャラクター基本情報の保存
- キャラクター一覧
- 画像用途別フォルダ作成
- 画像登録
- prompt 記録保存
- 採用、保留、没の状態管理
- Unity 向け export
- `heroine_profile_note.md` 出力

## 開発時の注意

WPF は Windows Desktop SDK が必要です。

WSL や Linux 上の .NET SDK では、`Microsoft.NET.Sdk.WindowsDesktop` が見つからずビルドできない場合があります。確認は Windows 上の Visual Studio または Windows の .NET SDK で行ってください。

現在のターゲットフレームワークは `net5.0-windows` のまま維持します。

`net5.0-windows` はサポート終了済みですが、このプロジェクトでは `net8.0-windows` への移行がうまくいかなかったため、当面は変更しない方針です。実装を進める際も、ターゲットフレームワーク移行は別タスクとして扱ってください。

## 推奨する最初の実装範囲

最初は画像生成連携や高度な UI は入れず、素材管理の土台だけを作るのがよいです。

1. キャラクター基本情報を JSON で保存する
2. 画像用途別フォルダを作成する
3. 採用画像と prompt 記録を同じ ID で保存する
4. Unity 向け export フォルダを作る
5. `heroine_profile_note.md` を出力する

この範囲ができると、Stable Diffusion で生成した画像を手動登録し、Unity へ取り込む作業を分離できます。

## 推奨ディレクトリ構成

アプリ内の作業データは、次の構成を基本にします。

```text
Characters/
  <HeroineId>/
    profile.json
    Images/
      Sprites/
      Event/
      Actions/
      Ending/
    Prompts/
      <AssetId>.prompt.json
```

Unity 向け export は次の構成を基本にします。

```text
Export/
  <HeroineId>/
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
      <AssetId>.prompt.json
```

## 推奨モデル

最初に追加する Model は、次の程度で十分です。

```text
Models/
  HeroineProfile.cs
  HeroineAsset.cs
  PromptRecord.cs
  AssetUsage.cs
  AssetStatus.cs
```

### HeroineProfile

- `HeroineId`
- `DisplayName`
- `Age`
- `Height`
- `Personality`
- `SpeakingStyle`
- `FirstPerson`
- `SecondPerson`
- `Likes`
- `Dislikes`
- `AppearancePrompt`
- `ActionReactionPolicy`
- `EndingPolicy`

### HeroineAsset

- `AssetId`
- `Usage`
- `Status`
- `FileName`
- `SourcePath`
- `StoredPath`
- `PromptRecordPath`
- `Memo`

### PromptRecord

- `PositivePrompt`
- `NegativePrompt`
- `Model`
- `Vae`
- `Lora`
- `Sampler`
- `Steps`
- `CfgScale`
- `Seed`
- `ImageWidth`
- `ImageHeight`
- `ControlNetMemo`
- `UpscaleMemo`
- `InpaintMemo`
- `AdoptionReason`
- `RevisionMemo`

### AssetUsage

- `Sprites`
- `Event`
- `Actions`
- `Ending`

### AssetStatus

- `Accepted`
- `Pending`
- `Rejected`

## 推奨サービス

ViewModel にファイル処理を直接書きすぎないよう、次のサービスを切ると保守しやすくなります。

```text
Services/
  CharacterProjectService.cs
  PromptRecordService.cs
  ExportService.cs
```

### CharacterProjectService

- キャラクター作成
- `profile.json` 読み書き
- 画像用途別フォルダ作成
- キャラクター一覧取得

### PromptRecordService

- prompt JSON の読み書き
- 画像ファイル名と prompt ファイル名の対応付け

### PromptTemplateService

- スチル用途別のデフォルトプロンプトテンプレート管理
- キャラクター容姿プロンプトとテンプレートの合成
- 合成結果を `PromptRecord.PositivePrompt` に反映

### ExportService

- 採用画像の export
- prompt 記録の export
- `heroine_profile_note.md` 生成
- 下書き Markdown の生成

## 最初の画面構成案

最初の UI は、1 画面に詰め込みすぎず次の構成にすると実装しやすいです。

- 左ペイン: キャラクター一覧
- 中央: 選択キャラクターの基本情報
- 右ペイン: 画像用途別の採用状況
- 下部または別タブ: Export 実行

初期段階では画像プレビューを簡易表示に留め、まず JSON 保存と export の正しさを優先してください。

## 実装順序

1. `Models` を追加する
2. JSON 保存用サービスを追加する
3. 新規キャラクター作成コマンドを作る
4. キャラクター一覧を表示する
5. 選択キャラクターの `profile.json` を編集、保存できるようにする
6. 画像用途別フォルダを作成する
7. 採用画像登録の最小機能を作る
8. prompt JSON 登録の最小機能を作る
9. キャラクター容姿プロンプトを追加する
10. スチル用途別のデフォルトプロンプトテンプレートを用意する
11. 容姿プロンプトとテンプレートを合成して `PromptRecord.PositivePrompt` に反映する
12. ExportService を作る
13. `heroine_profile_note.md` を出力する

## 検証観点

- `HeroineId` から期待するフォルダが作られる
- `profile.json` が保存、再読み込みできる
- 画像と prompt JSON が同じ `AssetId` で対応する
- キャラクター容姿プロンプトとスチル用テンプレートから positive prompt を生成できる
- `Accepted` の画像だけが export される
- Export 結果が `Docs/CharacterAssetGenerationToolSpec.md` の構成と一致する
- `heroine_profile_note.md` に Unity 側で必要な参照情報が入る

## 未決事項

- 作業データの保存先をアプリ直下に固定するか、ユーザーが選べるようにするか
- JSON の細かいスキーマ
- 画像登録時にコピーするか、参照パスだけ保持するか
- スチル用デフォルトプロンプトテンプレートをコード内固定にするか、JSON 設定として編集可能にするか
- prompt テンプレートのプレースホルダー名をどう定義するか
- `net5.0-windows` 維持を前提に、将来ターゲットフレームワーク移行を再検証するタイミング
- 画像プレビュー、ドラッグ&ドロップ、画像検査をどの段階で入れるか

## 次の担当者へのメモ

まずは見た目よりも、保存形式と export 結果を固めるのが重要です。

このツールの価値は、画像生成を自動化することより、採用済み素材、生成条件、Unity 取り込み先を失わずに管理することにあります。最初の実装では外部生成した画像を登録する前提で進め、Stable Diffusion 連携や Python 画像検査は後から追加する方が安全です。

今後は、キャラクターごとの容姿プロンプトを登録し、各種スチル用のデフォルトプロンプトテンプレートと合成する機能を追加します。これにより、同じキャラクターの外見を保ったまま、立ち絵、イベント、行動、エンディング用の prompt を効率よく作れるようにします。
