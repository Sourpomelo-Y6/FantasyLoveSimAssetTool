# FantasyLoveSimAssetTool

`FantasyLoveSimAssetTool` は、Unity プロジェクト `FantasyLoveSim` 向けのヒロイン素材を管理するための WPF アプリです。

Stable Diffusion などで生成した立ち絵、イベントスチル、行動スチル、エンディングスチルと、それぞれのプロンプト記録をキャラクター単位で整理し、Unity に取り込みやすいフォルダ構成へ export することを目的にしています。

詳細仕様は [Docs/CharacterAssetGenerationToolSpec.md](Docs/CharacterAssetGenerationToolSpec.md) を参照してください。

## 現在の状態

現在は、素材管理 MVP の主要機能を実装済みです。

- .NET 5 WPF アプリ
- MVVM ベースの単一画面構成
- キャラクター基本情報の JSON 保存
- 画像登録と用途別フォルダへのコピー
- 登録済み画像の登録解除。画像ファイルと prompt JSON は削除しない
- 登録済み画像のプレビュー
- 登録済み画像の Status フィルタ
- 画像登録時と Export 時の解像度、縦横比、透過 PNG 検査
- 採用、保留、没ステータス管理
- prompt JSON の保存
- キャラクター容姿プロンプトとスチル用テンプレートの合成
- 全スチル共通の追加 positive prompt
- スチル作業タブを通常のスチル生成、登録、採用作業の主導線として表示
- スチル一覧(確認用)タブで仕様上必要なスチルを表形式で確認
- `PromptTemplates/templates.json` による prompt テンプレート管理
- Prompt タブでのテンプレート用途選択
- `ComfySettings/comfyui.json` による ComfyUI 設定読み込み
- Prompt タブでの ComfyUI workflow template 読み込み、JSON 検証、保存
- ComfyUI workflow template への prompt 差し込み preview
- スチル作業タブ内での positive / negative prompt 確認と ComfyUI workflow preview 作成、表示
- スチル作業タブからローカル ComfyUI `/prompt` への workflow 送信と `prompt_id` 表示
- `prompt_id` からの ComfyUI 生成履歴の手動確認、自動確認、出力画像ファイル情報表示
- ComfyUI 生成結果待機中のボタン制御とアプリ側キャンセル
- ComfyUI `/interrupt` による生成停止要求
- ComfyUI `/view` からの生成画像取得、一時保存、スチル作業タブでのプレビュー
- ComfyUI 生成画像のスチル採用登録
- ComfyUI 生成条件の prompt JSON への記録
- Unity 向け Export
- Export 件数と警告の表示

現状では、Stable Diffusion などの画像生成自体はアプリ内では完結せず、外部生成した画像を登録、整理、出力するツールとして動作します。
この外部ファイル登録フローは今後も残し、将来追加するローカル ComfyUI 連携は、同じ登録処理へ生成結果を渡す任意機能として扱います。
ComfyUI 連携は現時点では workflow 送信、`prompt_id` 取得、生成履歴の自動確認、生成停止要求、生成画像の一時保存、プレビュー、スチル採用登録までです。待機キャンセルはアプリ側の自動確認停止であり、Comfy 停止は ComfyUI 本体へ `/interrupt` を送信します。

## 想定する用途

このツールでは、次の情報をキャラクター単位で管理します。

- ヒロイン基本情報
- 性格、口調、一人称、二人称
- 衣装、表情、画像用途
- Stable Diffusion の positive prompt / negative prompt
- model、VAE、LoRA、sampler、steps、CFG scale、seed などの生成設定
- 採用画像、保留画像、没画像
- Unity 取り込み用の export 結果

Unity 側では、生成された `Images` 配下を次のような構成で取り込む想定です。

```text
Assets/Images/Heroines/<HeroineId>/Sprites/
Assets/Images/Heroines/<HeroineId>/Event/
Assets/Images/Heroines/<HeroineId>/Actions/
Assets/Images/Heroines/<HeroineId>/Ending/
```

## プロジェクト構成

```text
FantasyLoveSimAssetTool.sln
FantasyLoveSimAssetTool/
  App.xaml
  Common/
    ObservableObject.cs
    RelayCommand.cs
  Models/
    HeroineProfile.cs
    HeroineAsset.cs
    PromptRecord.cs
    PromptTemplate.cs
    ExportReport.cs
    ImageInspectionResult.cs
    ComfySettings.cs
    ComfyOutputImage.cs
    StillDefinition.cs
    StillWorkItem.cs
  ViewModels/
    MainWindowModel.cs
  Services/
    CharacterProjectService.cs
    PromptRecordService.cs
    PromptTemplateService.cs
    StillDefinitionService.cs
    ImageInspectionService.cs
    ComfySettingsService.cs
    ComfyWorkflowService.cs
    ComfyClientService.cs
    ExportService.cs
  Views/
    MainWindow.xaml
PromptTemplates/
  templates.json
ComfySettings/
  comfyui.json
  workflow-template.json
Docs/
  CharacterAssetGenerationToolSpec.md
  Handoff.md
```

`PromptTemplates/` と `ComfySettings/` の JSON は、ビルド時に実行出力フォルダへコピーされます。
Visual Studio から起動した場合も、アプリは出力先の JSON を読み込みます。

## 開発環境

- Windows
- Visual Studio 2022 以降
- .NET SDK
- WPF

プロジェクトのターゲットフレームワークは `net5.0-windows` です。

## ビルド

Visual Studio で `FantasyLoveSimAssetTool.sln` を開いてビルドします。

コマンドラインで確認する場合は、リポジトリルートで次を実行します。

```powershell
dotnet build FantasyLoveSimAssetTool.sln
```

WPF は Windows Desktop SDK が必要です。WSL や Linux 上の .NET SDK ではビルドできない場合があります。

現在のターゲットフレームワーク `net5.0-windows` はサポート終了済みですが、`net8.0-windows` への移行がうまくいかなかったため、当面は `net5.0-windows` のまま維持します。ターゲットフレームワーク移行は別タスクとして扱います。

## 実装済みの主な機能

- キャラクター作成
- `Characters/<HeroineId>/profile.json` の保存、読み込み
- `Images/Sprites`, `Event`, `Actions`, `Ending`, `Prompts` の作成
- 元画像の登録と用途別フォルダへのコピー
- 登録済み画像の一覧表示とプレビュー
- 登録済み画像の `Accepted`, `Pending`, `Rejected` 切り替え
- prompt 記録の保存、読み込み
- スチル用途別デフォルトテンプレートの適用
- `Export/<HeroineId>/` への Unity 向け出力
- Export report による件数、警告表示

## 画面

- 左ペイン: キャラクター一覧、新規作成、再読み込み
- 基本情報タブ: プロフィール、口調、容姿プロンプトなどの編集
- 画像タブ: 画像登録、登録解除、ステータス編集、プレビュー
- Prompt タブ: prompt 記録編集、テンプレート適用
- Export タブ: Export 実行、件数、警告確認

## 保存データの方針

アプリ内部のデータは JSON を基本にします。

例:

```text
Characters/
  TestHeroine/
    profile.json
    Images/
      Sprites/
      Event/
      Actions/
      Ending/
    Prompts/
      GameStartIntro_01.prompt.json
```

採用画像と prompt 記録は、同じベース名で対応させます。

```text
GameStartIntro_01.png
GameStartIntro_01.prompt.json
```

## Export 方針

Unity 向けの export 結果は、次の構成を基本にします。

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

`Images` 配下は Unity の `Assets/Images/Heroines/<HeroineId>/` へコピーし、`Data` 配下は ScriptableObject 作成時の参照資料として使います。

## 今後の拡張候補

- 余白や表情差分などの高度な画像検査
- 会話、イベント、行動反応、エンディング本文の作成
- Unity Editor Import 用の会話データ JSON export
- Unity Editor 側で JSON から ScriptableObject `.asset` を生成する補助
- Python スクリプト連携による高度な画像検査
- ComfyUI 生成中のより詳細な進捗表示
