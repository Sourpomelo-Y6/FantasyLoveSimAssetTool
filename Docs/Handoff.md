# FantasyLoveSimAssetTool 引継ぎ

## 概要

`FantasyLoveSimAssetTool` は、Unity プロジェクト `FantasyLoveSim` 向けのヒロイン画像素材とキャラクター設定を管理する WPF アプリです。

目的は、Stable Diffusion などで生成した画像をキャラクター単位で整理し、採用画像と prompt 記録を追跡しながら、Unity に取り込みやすいフォルダ構成へ export することです。

仕様の詳細は `Docs/CharacterAssetGenerationToolSpec.md`、リポジトリ概要は `ReadMe.md` を参照してください。

## 現在の状態

現在は、素材管理 MVP の主要機能を実装済みです。

- ソリューション: `FantasyLoveSimAssetTool.sln`
- アプリ本体: `FantasyLoveSimAssetTool/`
- ターゲットフレームワーク: `net5.0-windows`
- UI: `Views/MainWindow.xaml`
- ViewModel: `ViewModels/MainWindowModel.cs`
- モデル: `Models/HeroineProfile.cs`, `Models/HeroineAsset.cs`, `Models/PromptRecord.cs`, `Models/PromptTemplate.cs`, `Models/ExportReport.cs`
- サービス: `Services/CharacterProjectService.cs`, `Services/PromptRecordService.cs`, `Services/PromptTemplateService.cs`, `Services/ExportService.cs`
- 共通基盤: `Common/ObservableObject.cs`, `Common/RelayCommand.cs`

実装済みの機能は次の通りです。

- キャラクター基本情報の保存、読み込み
- キャラクター一覧表示
- 画像用途別フォルダ作成
- 画像登録と用途別フォルダへのコピー
- 登録済み画像のプレビュー
- 登録済み画像の `Accepted`, `Pending`, `Rejected` 変更と保存
- prompt 記録保存、読み込み
- キャラクター容姿プロンプトとスチル用テンプレートの合成
- Unity 向け export
- `heroine_profile_note.md` と下書き Markdown の出力
- Export report による件数、警告表示

## 開発時の注意

WPF は Windows Desktop SDK が必要です。

WSL や Linux 上の .NET SDK では、`Microsoft.NET.Sdk.WindowsDesktop` が見つからずビルドできない場合があります。確認は Windows 上の Visual Studio または Windows の .NET SDK で行ってください。

現在のターゲットフレームワークは `net5.0-windows` のまま維持します。

`net5.0-windows` はサポート終了済みですが、このプロジェクトでは `net8.0-windows` への移行がうまくいかなかったため、当面は変更しない方針です。実装を進める際も、ターゲットフレームワーク移行は別タスクとして扱ってください。

## 実装済みの最小範囲

仕様書の「最初に作る最小機能」は概ね実装済みです。

1. キャラクター基本情報を JSON で保存する
2. 画像用途別フォルダを作成する
3. 採用画像と prompt 記録を同じ ID で保存する
4. Unity 向け export フォルダを作る
5. `heroine_profile_note.md` を出力する

画像生成自体は外部ツールで行い、本アプリは登録、整理、追跡、出力を担当します。

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
  PromptTemplate.cs
  ExportReport.cs
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

### PromptTemplate

- `TemplateId`
- `DisplayName`
- `Usage`
- `TemplateText`

### ExportReport

- `ExportPath`
- `AcceptedAssetCount`
- `ExportedImageCount`
- `ExportedPromptCount`
- `SkippedImageCount`
- `Warnings`

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
- export 件数と警告の report 生成

## 現在の画面構成

- 左ペイン: キャラクター一覧、新規作成、再読み込み
- 基本情報タブ: プロフィール、容姿プロンプト、反応方針など
- 画像タブ: 画像登録、ステータス編集、プレビュー
- Prompt タブ: prompt 記録編集、テンプレート適用
- Export タブ: export 実行、件数、警告表示

## ここまでの実装順序

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
14. Export report を表示する
15. 登録済み画像をプレビューする
16. 登録済み画像の Status を一覧から変更、保存する

## 次に進める候補

- 画像サイズ、縦横比、透過の検査
- 画像登録のドラッグ&ドロップ対応
- Export 結果フォルダを開く操作
- prompt テンプレートの JSON 化
- Accepted 画像だけを一覧上で絞り込む機能
- 画像ファイルの差し替え、削除

## 検証観点

- `HeroineId` から期待するフォルダが作られる
- `profile.json` が保存、再読み込みできる
- 画像と prompt JSON が同じ `AssetId` で対応する
- キャラクター容姿プロンプトとスチル用テンプレートから positive prompt を生成できる
- `Accepted` の画像だけが export される
- Export report に件数と警告が出る
- 登録済み画像を選ぶとプレビューが表示される
- 登録済み画像の Status を変更して `profile.json` に保存できる
- Export 結果が `Docs/CharacterAssetGenerationToolSpec.md` の構成と一致する
- `heroine_profile_note.md` に Unity 側で必要な参照情報が入る

## 未決事項

- 作業データの保存先をアプリ直下に固定するか、ユーザーが選べるようにするか
- JSON の細かいスキーマ
- 画像登録時にコピーするか、参照パスだけ保持するか
- スチル用デフォルトプロンプトテンプレートをコード内固定にするか、JSON 設定として編集可能にするか
- prompt テンプレートのプレースホルダー名をどう定義するか
- `net5.0-windows` 維持を前提に、将来ターゲットフレームワーク移行を再検証するタイミング
- 画像検査をどの段階で入れるか
- Export 結果フォルダをアプリから開くか
- 画像削除時に元ファイルも削除するか、profile から除外するだけにするか

## 次の担当者へのメモ

まずは見た目よりも、保存形式と export 結果を固めるのが重要です。

このツールの価値は、画像生成を自動化することより、採用済み素材、生成条件、Unity 取り込み先を失わずに管理することにあります。最初の実装では外部生成した画像を登録する前提で進め、Stable Diffusion 連携や Python 画像検査は後から追加する方が安全です。

今後は、画像検査、テンプレート管理、Export 後の導線改善を進めるとよいです。現状の MVP は外部生成した画像を登録し、採用状態と prompt 記録を管理し、Unity 向けに出力する用途には使える状態です。
