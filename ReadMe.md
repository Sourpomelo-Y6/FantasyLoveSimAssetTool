# FantasyLoveSimAssetTool

`FantasyLoveSimAssetTool` は、Unity プロジェクト `FantasyLoveSim` 向けのヒロイン素材を管理するための WPF アプリです。

Stable Diffusion などで生成した立ち絵、イベントスチル、行動スチル、エンディングスチルと、それぞれのプロンプト記録をキャラクター単位で整理し、Unity に取り込みやすいフォルダ構成へ export することを目的にしています。

文書全体の入口と、Unityプロジェクトとの正本の分担は [Docs/README.md](Docs/README.md) を参照してください。

詳細仕様は [Docs/CharacterAssetGenerationToolSpec.md](Docs/CharacterAssetGenerationToolSpec.md) を参照してください。
操作画面の整理方針と段階的な再構成案は [Docs/ToolUsabilityReorganizationPlan.md](Docs/ToolUsabilityReorganizationPlan.md) にまとめています。
Unity 取り込み方針は [Docs/Extra/UnityImportPlan.md](Docs/Extra/UnityImportPlan.md)、Unity Editor 側の実装計画は [Docs/Extra/UnityEditorImportImplementationPlan.md](Docs/Extra/UnityEditorImportImplementationPlan.md)、敵キャラ素材の export / Unity import 仕様は [Docs/Extra/EnemyExportUnityImportSpec.md](Docs/Extra/EnemyExportUnityImportSpec.md)、プレイヤー素材の管理方針は [Docs/PlayerAssetManagementPlan.md](Docs/PlayerAssetManagementPlan.md)、会話データ拡張案は [Docs/Extra/ConversationDataPlan.md](Docs/Extra/ConversationDataPlan.md)、通常会話の分類規則は [Docs/ConversationClassificationRules.md](Docs/ConversationClassificationRules.md)、表情・衣装差分ロードマップは [Docs/ExpressionCostumeVariantRoadmap.md](Docs/ExpressionCostumeVariantRoadmap.md)、透過レイヤー素材の作成手順は [Docs/TransparentLayerAssetWorkflow.md](Docs/TransparentLayerAssetWorkflow.md) を参照してください。

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
- スチルごとの negative prompt 追加
- スチル作業タブを通常のスチル生成、登録、採用作業の主導線として表示
- スチル一覧(確認用)タブで仕様上必要なスチルを表形式で確認
- `Definitions/*.json` による表情、衣装、透過レイヤー素材の元データ定義
- `PromptTemplates/templates.json` による prompt テンプレート管理
- Prompt タブでのテンプレート用途選択
- `ComfySettings/comfyui.json` による ComfyUI 設定読み込み
- Prompt タブでの ComfyUI workflow template 読み込み、JSON 検証、保存
- ComfyUI workflow template への prompt 差し込み preview
- スチル作業タブ内での positive / negative prompt 確認と ComfyUI workflow preview 作成、表示
- スチル作業タブからローカル ComfyUI `/prompt` への workflow 送信と `prompt_id` 表示
- `prompt_id` からの ComfyUI 生成履歴の手動確認、自動確認、出力画像ファイル情報表示
- ComfyUI `/queue` による実行中、待機中、対象 prompt のキュー状態表示
- ComfyUI WebSocket による生成中 node と sampler step の詳細進捗表示。失敗時は `/queue` と `/history` の確認に戻る
- ComfyUI 生成結果待機中のボタン制御とアプリ側キャンセル
- ComfyUI `/interrupt` による生成停止要求
- ComfyUI `/view` からの生成画像取得、一時保存、スチル作業タブでのプレビュー
- ComfyUI 生成画像のスチル採用登録
- ComfyUI 生成条件の prompt JSON への記録
- Unity 向け Export
- Unityから取り込んだ訓練カタログの解放条件、一回限定、調子条件、前提訓練の確認
- Export 件数と警告の表示
- 会話、イベント、行動反応、エンディング本文の最小編集と `profile.json` 保存
- 会話データの `conversations_export.json`、`game_events_export.json`、`action_reactions_export.json`、`endings_export.json` 出力
- 会話データ入力時のカテゴリ、条件、表情、画像 AssetId、ID 自動生成補助
- 会話データ一覧の検索、カテゴリ、警告あり、画像あり/なし絞り込み
- Export 時の会話データ件数表示と検証警告
- Unity 側で受け取る会話条件値、表情値に合わせた候補表示と候補外警告
- Unity 側 ScriptableObject 保存先と会話 JSON フィールド対応表の整理
- 選択中会話行のLocalAI候補生成と、状況テンプレート・キャラクター固有指示の合成プレビュー
- 選択中会話項目に対する最大3行のAI下書き生成と、既存行への追加・置き換え
- 状況テンプレートからカテゴリ、仮タイトル、重複しないIDを持つ新規会話項目を準備
- Heroine3の既存会話を抽象化した、日常・冒険・食事・恋愛の共通状況テンプレート16件
- 状況テンプレートを「すべて・日常・冒険・食事・恋愛」で絞り込む種類フィルター
- 会話データ種別に連動するUnity読込ボタンと、一覧付近に整理した会話項目操作
- AI会話下書きの重複・類似、長さ、参照値、禁止表現、内部情報混入を採用前に警告
- 差分定義タブによる表情、衣装、透過レイヤー素材定義 JSON の編集、候補選択、保存前検証
- レイヤープレビュータブによる Accepted 済み透過レイヤー素材の重ね合わせ確認
- 透過レイヤー素材の `sprite_layers_export.json` 出力
- Export 時の透過レイヤー素材検証と warning 表示
- プレイヤー素材タブによる戦闘用プレイヤー画像の登録、ComfyUI 作画、`Export/Player/` 出力

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
Assets/Images/Heroines/<HeroineId>/Battle/
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
    ConversationEntry.cs
    ConversationLine.cs
    ConversationCondition.cs
    ConversationDataKind.cs
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
  conversation-situations.json
Characters/
  Heroine3/
    conversation-ai-prompt.json
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
- `Images/Sprites`, `Event`, `Actions`, `Ending`, `Battle`, `Prompts` の作成
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

### Unity訓練一覧の条件確認

訓練タブの `Unity訓練一覧読込` から、Unityが出力した
`training_catalog_from_unity.json` を選択します。一覧には解放方法、条件バッジ、
前提訓練、参照警告が表示されます。行を選択すると、表示可能な調子、実行可能な調子、
一回限定、前提のAND／OR、前提未達時・完了後の表示規則を確認できます。

旧形式のJSONに含まれない条件項目は、Toolに保存済みの値を消さずに維持します。
前提訓練IDが同じカタログ内に存在しない場合は警告しますが、他の訓練の読込は継続します。
- BGM・SEタブ: 固定用途音声の導入状況、登録、保存先、プレビューを確認
- VOICEタブ: キャラクター別音声の登録、参照数、期待パス、プレビューを確認

### 音声ライブラリの確認

`BGM・SE`または`VOICE`タブで最初に`選択`を押し、対象Unityプロジェクトの
`ProjectSettings/ProjectVersion.txt` を選択します。設定は端末ローカルへ保存され、
両タブで共有されます。キャラクターデータやExport JSONには絶対パスを書き込みません。

`再走査` を押すと、`Assets/Resources/Audio` 以下を調査して次を表示します。

- Title、Main、Ending、Battle、TrainingのBGM
- UI、購入、スキル、予定、訓練、戦闘、イベント用SE
- 選択中のキャラクターで利用できるVOICE
- 導入済み／未配置、実ファイル、期待パス、Toolデータからの参照数

各タブに独立した文字列検索があり、VOICEは選択中キャラクターにも絞り込めます。一覧で
導入済み音声を選ぶと`選択音声を再生`と`停止`で確認できます。Windows環境で
再生できない形式は画面下部のステータス欄へその旨を表示します。プレビューできなくても
登録状態の確認やUnityへの登録には影響しません。

BGMまたはSEの行を選択すると、`BGM・SEを登録` から手元の
`.wav / .mp3 / .ogg / .aif / .aiff` を用途別の正規パスへコピーできます。
BGMは `Assets/Resources/Audio/Bgm/<ID>`、SEは
`Assets/Resources/Audio/SE/<ID>` に配置されます。同じIDの既存ファイルがある場合は
確認ダイアログが表示され、承認した場合だけ別拡張子のファイルとその`.meta`を
置き換えます。`保存先を開く` から対象フォルダも確認できます。

訓練セリフと戦闘メッセージのVoice ID欄は、走査した候補から選べる編集可能な
ドロップダウンです。候補にない将来用IDも直接入力できます。

VOICEは音声タブの`VOICE登録`欄で、選択中キャラクター、用途、拡張子なしVoice IDを
指定して登録できます。保存先は
`Assets/Resources/Audio/Voice/<HeroineId>/<用途>/<VoiceId>.<拡張子>`です。
Voice IDに同じ用途名から始まる値を入力した場合、用途名は重複しません。

登録先は次から選択できます。

- 登録のみ
- 選択中の訓練セリフ
- 選択中の戦闘後イベント
- 選択中の戦闘パネルメッセージ
- 選択中の会話・イベント行

メッセージへ設定する場合は、先に対応するタブで対象行・候補を選択してください。
登録後は`Training/Line01`のようなヒロインIDを含まない相対Voice IDを設定し、
キャラクターデータを保存します。会話・行動反応では用途`Conversation`、ゲームイベント・
予定イベント・エンディングでは用途`Event`が自動提案されます。

会話データタブの台詞行にはVoice ID列があり、通常会話、ゲームイベント、予定イベント、
行動反応、エンディングで編集できます。Unityからの読込とUnity向けExportでも
各行の任意`voiceId`を維持します。Voice IDがない既存JSONも従来どおり読み込めます。

VOICE一覧の状態は次の意味です。

- `○ 使用中`: 音声ファイルがあり、Tool内の訓練・戦闘データから参照されている
- `△ 未使用`: 音声ファイルはあるが、Toolデータ内の参照数が0
- `× 未配置`: Voice IDは参照されているが、対応する音声ファイルがない

`未使用のみ`で参照数0のVOICEへ絞り込めます。参照元列と状態のToolTipには、確認できた
訓練枠、戦闘後イベント、戦闘パネルメッセージ、会話・各イベントを表示します。
`選択VOICEの保存先`から
対象フォルダを開けます。Unity側から直接参照されている音声はToolだけでは判定できないため、
`△ 未使用`でも自動削除はせず、削除前にUnity側を確認してください。

制作状況タブには選択中キャラクターの`VOICE`カテゴリも表示されます。

- `○`: 参照中のVoice IDがすべて配置済み
- `△`: 未使用VOICEがある（Exportを妨げない警告）
- `×`: 参照中のVOICEが未配置、またはVoice IDのパスが不正
- `―`: Unityプロジェクト未選択、またはVoice IDが一件もない

詳細には参照数、実ファイルまたは期待パスを表示します。詳細行をクリックするとVOICEタブへ
移動して対象行を選択します。Unityプロジェクト未選択時はエラーにせず、選択方法を案内します。
未配置・不正な参照はExport準備にも反映しますが、未使用VOICEだけではExportを妨げません。

登録したテスト用音声とUnityが生成する`.meta`は現在の運用ではGitへコミットしません。

## 保存データの方針

アプリ内部のデータは JSON を基本にします。

### 作業フォルダー

作業データは、Visual Studio の `bin/Debug` や `bin/Release` ではなく、既定で次へ保存します。

```text
ドキュメント/FantasyLoveSimAssetToolWorkspace/
```

画面上部の `保存先を変更` から任意のフォルダーへ変更できます。変更時は現在の作業データを
新しい場所へコピーし、同名ファイルがある場合は `Backups/WorkspaceMigration_*` に退避します。
新しい保存先はAssetToolを再起動した後に有効になります。

以前のバージョンが `bin/Debug/.../Characters` にデータを保存していた場合は、実際にAssetToolを
起動してウィンドウが表示された後に検出し、移行確認を表示します。Visual Studioのプロジェクトや
XAMLデザイナーを再読み込みしただけでは表示しません。移行元は削除しないため、移行後の内容を
確認するまでは手動で削除しないでください。`Export` と `Temp` は生成し直せるため、ワークスペース
移行の対象外です。

`profile.json` を上書き保存するたびに、同じキャラクターフォルダーの `Backups` へ直前の内容を
最大5世代保存します。保存途中の破損を避けるため、一時ファイルへ書き込んでから置き換えます。

保存先の選択内容はユーザーごとのローカル設定へ保存され、Git管理対象にはしません。
キャラクターの作業データや生成画像も従来どおりToolリポジトリへコミットしない運用です。

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
      Battle/
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

訓練タブではUnityから取り込んだ訓練を選び、出現回数、調子、前提訓練、表示規則、
解放NodeIdを編集できます。`条件を検証して保存`後にExportすると、
`Data/training_catalog_export.json`へ出力されます。Unity側の
`FantasyLoveSim > Import Heroine Export`で取り込むと、既存`TrainingData`の可否条件と
対象ヒロインの解放ノードを更新します。HP消費や報酬など、条件編集欄にない値は変更しません。

```text
Export/
  TestHeroine/
    Images/
      Sprites/
      Event/
      Actions/
      Ending/
      Battle/
    Data/
      heroine_profile_note.md
      heroine_profile_export.json
      assets_export.json
      sprite_layers_export.json
      conversations_export.json
      game_events_export.json
      action_reactions_export.json
      endings_export.json
      conversations_draft.md
      game_events_draft.md
      action_reactions_draft.md
      endings_draft.md
    Prompts/
      GameStartIntro_01.prompt.json
```

`Images` 配下は Unity の `Assets/Images/Heroines/<HeroineId>/` へコピーします。
`Data/heroine_profile_export.json` と `Data/assets_export.json` は Unity Editor Import 拡張が読む入口にします。
WPF 側では ScriptableObject `.asset` を直接生成せず、Unity Editor 側で JSON から `HeroineProfileData` などを生成、更新する方針です。
`Prompts` 配下の個別 prompt JSON は、生成条件の参照資料として `assets_export.json` の `exportPromptPath` から辿れるようにします。
Unity 側での取り込み手順と、WPF ツールと Unity プロジェクトを別リポジトリで運用する方針は `Docs/Extra/UnityImportPlan.md` にまとめています。
`sprite_layers_export.json` の項目定義、Unity 側 `HeroineLayeredSpriteData` 案、Import 手順、fallback ルールも同ドキュメントにまとめています。

## 今後の拡張候補

- 余白や表情差分などの高度な画像検査
- 会話、イベント、行動反応、エンディング本文の作成
- Unity Editor Import 用の会話データ JSON export
- Unity Editor 側で JSON から ScriptableObject `.asset` を生成する補助
- Python スクリプト連携による高度な画像検査
- スチル一覧(確認用)タブの内容をスチル作業タブへ完全統合するか判断する
