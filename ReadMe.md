# FantasyLoveSimAssetTool

`FantasyLoveSimAssetTool` は、Unity プロジェクト `FantasyLoveSim` 向けのヒロイン素材を管理するための WPF アプリです。

Stable Diffusion などで生成した立ち絵、イベントスチル、行動スチル、エンディングスチルと、それぞれのプロンプト記録をキャラクター単位で整理し、Unity に取り込みやすいフォルダ構成へ export することを目的にしています。

詳細仕様は [Docs/CharacterAssetGenerationToolSpec.md](Docs/CharacterAssetGenerationToolSpec.md) を参照してください。

## 現在の状態

現在は WPF アプリのひな形段階です。

- .NET 5 WPF アプリ
- MVVM の最小構成
- `MainWindow` と `MainWindowModel`
- `ObservableObject`
- `RelayCommand`

まだキャラクター管理、画像管理、プロンプト記録、export 機能は実装されていません。

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
  ViewModels/
    MainWindowModel.cs
  Views/
    MainWindow.xaml
Docs/
  CharacterAssetGenerationToolSpec.md
```

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

## 最初に実装する機能

最初の実装範囲は、仕様書の「最初に作る最小機能」に合わせます。

1. キャラクター基本情報を JSON で保存する
2. 画像用途別フォルダを作成する
3. 採用画像と prompt 記録を同じ ID で保存する
4. Unity 向け export フォルダを作る
5. `heroine_profile_note.md` を出力する

この段階では Stable Diffusion の画像生成自体はアプリ内で行わず、外部で生成した画像を登録・整理するツールとして作ります。

## 予定している画面

- キャラクター一覧
- キャラクター詳細
- 画像用途別リスト
- プロンプト記録編集
- 採用画像管理
- Unity Export

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

- 画像プレビュー
- 画像の採用、保留、没ステータス管理
- ドラッグ&ドロップによる画像登録
- prompt テンプレート管理
- 画像サイズ、縦横比、透過、余白のチェック
- Unity の ScriptableObject 生成補助
- Python スクリプト連携による画像検査
