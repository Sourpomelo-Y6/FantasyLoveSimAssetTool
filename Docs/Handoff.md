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
- モデル: `Models/HeroineProfile.cs`, `Models/HeroineAsset.cs`, `Models/PromptRecord.cs`, `Models/PromptTemplate.cs`, `Models/ExportReport.cs`, `Models/ImageInspectionResult.cs`, `Models/ComfySettings.cs`, `Models/ComfyOutputImage.cs`, `Models/StillDefinition.cs`, `Models/StillWorkItem.cs`
- サービス: `Services/CharacterProjectService.cs`, `Services/PromptRecordService.cs`, `Services/PromptTemplateService.cs`, `Services/StillDefinitionService.cs`, `Services/ImageInspectionService.cs`, `Services/ComfySettingsService.cs`, `Services/ComfyWorkflowService.cs`, `Services/ComfyClientService.cs`, `Services/ExportService.cs`
- 共通基盤: `Common/ObservableObject.cs`, `Common/RelayCommand.cs`

実装済みの機能は次の通りです。

- キャラクター基本情報の保存、読み込み
- キャラクター一覧表示
- 画像用途別フォルダ作成
- 画像登録と用途別フォルダへのコピー
- 外部ツールで生成済みの画像ファイルを登録する運用
- 画像登録欄へのドラッグ&ドロップ入力
- 既存 `AssetId` への画像上書き登録と確認ダイアログ
- 登録済み画像のプレビュー
- 登録済み画像の Status フィルタ
- 画像登録時の解像度、縦横比、透過 PNG 検査
- 登録済み画像の `Accepted`, `Pending`, `Rejected` 変更と保存
- prompt 記録保存、読み込み
- キャラクター容姿プロンプトとスチル用テンプレートの合成
- `PromptTemplates/templates.json` からの prompt テンプレート読み込み
- Prompt タブでのテンプレート用途選択
- `ComfySettings/comfyui.json` からの ComfyUI 設定読み込みと Prompt タブでの表示
- `ComfySettings/workflow-template.json` への positive / negative prompt 差し込み preview
- スチル作業タブからローカル ComfyUI `/prompt` への workflow 送信と `prompt_id` 表示
- `prompt_id` からの ComfyUI 生成履歴の手動確認、自動確認、出力画像ファイル情報表示
- ComfyUI 生成結果待機中のボタン制御とアプリ側キャンセル
- ComfyUI `/interrupt` による生成停止要求
- ComfyUI `/view` からの生成画像取得、一時保存、スチル作業タブでのプレビュー
- ComfyUI 生成画像のスチル採用登録
- ComfyUI 生成条件の `PromptRecord` への保存
- 仕様書にある常時必要スチルの固定リスト表示
- スチル作業タブでの用途フィルタ、状態表示、画像プレビュー
- スチル固有 prompt、合成 positive prompt、現在の negative prompt のプレビュー
- スチル作業タブ内での positive / negative prompt を使った ComfyUI workflow preview 作成と表示
- キャラクター基本 prompt 変更時のスチル合成 prompt 即時更新と古い Comfy preview のクリア
- スチルから `PromptRecord.PositivePrompt` への反映
- スチルから画像登録欄への `AssetId`、用途、状態の反映
- スチルごとの画像登録状況、prompt 保存状況、AssetStatus 表示
- スチルの `Status` と `SpecificPrompt` をキャラクターごとの `StillWorkItems` として `profile.json` に保存
- スチル作業タブからのスチル保存
- Unity 向け export
- `heroine_profile_note.md` と下書き Markdown の出力
- Export report による件数、警告表示
- Export 時の Accepted 画像検査と警告表示
- Export タブで Accepted 画像だけを対象一覧に表示
- Export 結果フォルダをアプリから開く操作

## 開発時の注意

WPF は Windows Desktop SDK が必要です。

WSL や Linux 上の .NET SDK では、`Microsoft.NET.Sdk.WindowsDesktop` が見つからずビルドできない場合があります。確認は Windows 上の Visual Studio または Windows の .NET SDK で行ってください。

現在のターゲットフレームワークは `net5.0-windows` のまま維持します。

`net5.0-windows` はサポート終了済みですが、このプロジェクトでは `net8.0-windows` への移行がうまくいかなかったため、当面は変更しない方針です。実装を進める際も、ターゲットフレームワーク移行は別タスクとして扱ってください。

Codex のローカル作業用メタデータである `.agents/` と `.codex/` は Git で無視しています。

## 実装済みの最小範囲

仕様書の「最初に作る最小機能」は概ね実装済みです。

1. キャラクター基本情報を JSON で保存する
2. 画像用途別フォルダを作成する
3. 採用画像と prompt 記録を同じ ID で保存する
4. Unity 向け export フォルダを作る
5. `heroine_profile_note.md` を出力する

現状では、画像生成自体は外部ツールで行い、本アプリは登録、整理、追跡、出力を担当します。
この外部ファイル登録フローは、今後ローカル ComfyUI 連携を追加する場合も残します。

## 推奨ディレクトリ構成

アプリ内の作業データは、次の構成を基本にします。

```text
Characters/
  <HeroineId>/
    profile.json
    StillWorkItems は profile.json 内に保存
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
  StillDefinition.cs
  StillWorkItem.cs
  AssetUsage.cs
  AssetStatus.cs
  StillStatus.cs
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
- `Assets`
- `StillWorkItems`

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
- `ComfyPromptId`
- `ComfyOutputFileName`
- `ComfyOutputSubfolder`
- `ComfyOutputType`
- `ComfyEndpointUrl`
- `ComfyWorkflowTemplatePath`
- `ComfyWorkflowJson`

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

### StillDefinition

仕様書にある固定スチル定義です。アプリ起動時に `StillDefinitionService` から生成され、キャラクターごとの保存値があれば上書きされます。

- `AssetId`
- `DisplayName`
- `Usage`
- `FileName`
- `SpecificPrompt`
- `Status`

### StillWorkItem

キャラクターごとのスチル作業状態です。`HeroineProfile.StillWorkItems` として `profile.json` に保存します。

- `AssetId`
- `Status`
- `SpecificPrompt`

### StillStatus

- `NotGenerated`
- `Generating`
- `Accepted`
- `NeedsFix`
- `NotNeeded`

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
- 画像登録と既存 `AssetId` の上書き

### PromptRecordService

- prompt JSON の読み書き
- 画像ファイル名と prompt ファイル名の対応付け

### PromptTemplateService

- `PromptTemplates/templates.json` からスチル用途別 prompt テンプレートを読み込む
- `templates.json` はビルド時に実行出力フォルダへコピーされる
- JSON がない、空、不正な場合はコード内のデフォルトテンプレートへ fallback する
- キャラクター容姿プロンプトとテンプレートの合成
- 合成結果を `PromptRecord.PositivePrompt` に反映

### ComfySettingsService

- `ComfySettings/comfyui.json` から ComfyUI 接続設定を読み込む
- `comfyui.json` はビルド時に実行出力フォルダへコピーされる
- JSON がない、空、不正な場合は `http://127.0.0.1:8188` などのデフォルト設定へ fallback する

### ComfyWorkflowService

- `ComfySettings/workflow-template.json` を読み込む
- `workflow-template.json` はビルド時に実行出力フォルダへコピーされる
- `PromptRecord.PositivePrompt` と `PromptRecord.NegativePrompt` を workflow template の placeholder に差し込む
- workflow preview 用の整形 JSON と、ComfyUI 送信用 JSON を作る
- `nodes` / `links` を持つ ComfyUI 画面用 workflow は、既知ノードを `/prompt` 用 API prompt 形式へ変換する
- 現時点の変換対象は `CheckpointLoaderSimple`、`CLIPTextEncode`、`EmptyLatentImage`、`KSamplerAdvanced`、`VAEDecode`、`SaveImage`、`PrimitiveInt`
- `PrimitiveInt` から `noise_seed` などの seed 入力へ負の値が渡る場合は、ComfyUI API の validation に合わせて非負のランダム seed に変換する
- `SaveImage.filename_prefix` の `%date:<format>%` は、ComfyUI API 側では展開されない場合があるため、送信前にツール側で現在日付へ展開する

### ComfyClientService

- `ComfySettings.EndpointUrl` の `/prompt` に workflow JSON を送る
- 成功時に ComfyUI の `prompt_id` を返す
- `ComfySettings.EndpointUrl` の `/history/{prompt_id}` から生成履歴を取得する
- 生成履歴の `outputs.*.images[]` から `filename`、`subfolder`、`type` を抽出する
- `ComfySettings.EndpointUrl` の `/view` から生成画像 bytes を取得する
- ComfyUI 未起動、URL 不正、workflow 不正、`prompt_id` 欠落は例外として ViewModel 側でステータス表示する
- 取得した ComfyUI 生成画像は、既存の画像登録処理と同じ上書き確認ルールで採用登録する
- 採用時に positive / negative prompt、`prompt_id`、Comfy 出力ファイル情報、endpoint、workflow template path、送信用 workflow JSON、seed、model、sampler、steps、CFG scale、画像サイズを `PromptRecord` に保存する

### StillDefinitionService

- 仕様書にある常時必要スチルの固定リスト管理
- `AssetId`、用途、出力ファイル名、スチル固有 prompt の初期値提供

### ExportService

- 採用画像の export
- prompt 記録の export
- `heroine_profile_note.md` 生成
- 下書き Markdown の生成
- export 件数と警告の report 生成

## 現在の画面構成

- 左ペイン: キャラクター一覧、新規作成、再読み込み
- 基本情報タブ: プロフィール、容姿プロンプト、反応方針など
- 画像タブ: 画像登録、ドラッグ&ドロップ入力、上書き確認、ステータス編集、プレビュー
- スチル作業タブ: 用途フィルタ付きの作業リスト、状況表示、画像プレビュー、合成 prompt、Prompt 反映、画像登録欄への反映、スチル保存
- スチル一覧タブ: 仕様上必要なスチルの表形式一覧、状態と追加 prompt の編集
- Prompt タブ: prompt 記録編集、テンプレート適用
- Export タブ: export 実行、件数、警告表示、出力先を開く

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
17. 仕様書にある常時必要スチルを固定リスト化する
18. スチル一覧タブを追加する
19. スチル作業タブを追加する
20. スチル固有 prompt とキャラクター容姿 prompt の合成プレビューを追加する
21. スチルから Prompt タブへ positive prompt を反映する
22. スチルごとの画像登録状況、prompt 保存状況、AssetStatus、画像プレビューを表示する
23. スチル作業タブから画像登録欄へ `AssetId`、用途、状態を反映する
24. スチル作業タブに用途フィルタを追加する
25. スチルの `Status` と `SpecificPrompt` を `StillWorkItems` として `profile.json` に保存する
26. Export 結果フォルダを開く操作を追加する
27. 画像登録欄へのドラッグ&ドロップ入力を追加する
28. 既存 `AssetId` の画像上書き登録と確認ダイアログを追加する
29. スチル作業タブから ComfyUI workflow preview を作成する
30. スチル作業タブから ComfyUI `/prompt` へ送信し、`prompt_id` を表示する
31. `/history/{prompt_id}` から生成画像ファイル情報を取得する
32. `/view` から生成画像を取得し、`Temp/ComfyResults/` に一時保存してプレビューする
33. ComfyUI 生成画像を既存画像登録処理で採用登録する
34. ComfyUI 生成条件を `PromptRecord` に保存する
35. キャラクター基本 prompt 変更時にスチル合成 prompt を即時更新し、古い Comfy preview をクリアする
36. `Comfy 送信` 後に `/history/{prompt_id}` を自動確認し、生成結果待機中は二重送信を防ぐ
37. `待機キャンセル` でアプリ側の ComfyUI 生成結果自動確認を停止する
38. `Comfy 停止` で ComfyUI 本体へ `/interrupt` を送信し、アプリ側の自動確認も停止する

## 次に進める候補

- ComfyUI 生成中のより詳細な進捗表示
- ComfyUI workflow JSON のテンプレート編集画面
- スチル一覧タブを開発確認用に残すか、スチル作業タブへ統合するか判断する
- 会話データ作成機能の設計と Unity Editor Import 用 JSON export
- 画像ファイルの差し替え、削除

## ローカル ComfyUI 連携案

外部生成済み画像ファイルを登録する現行機能は、基本フロー兼フォールバックとして維持します。
その上で、任意機能としてローカルで起動している ComfyUI に prompt を送り、生成画像を取得して登録できる導線を追加します。

想定する流れは次の通りです。

1. スチル作業タブで対象スチルを選ぶ
2. キャラクター容姿 prompt とスチル固有 prompt から positive prompt を合成する
3. `PromptRecord.PositivePrompt` と negative prompt を ComfyUI 用 workflow JSON に差し込む
4. ローカル ComfyUI の HTTP API に workflow を送信し、`prompt_id` を受け取る
5. `/history/{prompt_id}` から生成結果の `filename`、`subfolder`、`type` を取得する
6. 生成完了後、出力画像を取得して `Temp/ComfyResults/` に一時保存し、プレビューする
7. 採用する画像を既存の画像登録処理に渡し、`HeroineAsset` として保存する

ComfyUI の既定接続先は、ローカル実行を前提に `http://127.0.0.1:8188` とします。
ただし、URL、workflow JSON、seed、画像サイズ、出力ノード名は環境差が出やすいため、将来は設定として編集可能にする必要があります。

生成画像の保存先は、外部ファイル登録時と同じく次を基本にします。

```text
Characters/<HeroineId>/Images/<Usage>/<AssetId>.png
```

ComfyUI から取得した画像も、登録時には既存 `AssetId` の上書き確認を通します。
これにより、外部ファイル登録、ドラッグ&ドロップ登録、ComfyUI 生成結果登録のすべてが同じ `HeroineAsset` 更新ルールを使えます。

ComfyUI が起動していない、生成に失敗した、workflow JSON が不正などの場合でも、外部ファイル登録は継続して使えるようにします。
ComfyUI 連携は画像生成の補助であり、素材管理 MVP の必須条件にはしません。

## Unity .asset と会話データ作成案

将来、会話データ、イベントデータ、行動反応、エンディング本文もこのツールで作成できるようにします。
ただし、WPF ツールから Unity の `.asset` を直接生成する方式は優先しません。

Unity の `.asset` は ScriptableObject の保存ファイルで、Force Text 設定なら YAML として外部から読めます。
しかし、外部ツールで直接書く場合は `.meta` の GUID、ScriptableObject の型情報、fileID、Assembly 名、Unity バージョン差分を正しく扱う必要があります。
壊れやすいため、WPF ツール側では中間データを JSON または Markdown として出力し、Unity Editor 側で ScriptableObject に変換する方針にします。

想定する流れは次の通りです。

1. WPF ツールでヒロインごとの会話、イベント、行動反応、エンディング本文を編集する
2. `conversations_export.json`、`game_events_export.json`、`action_reactions_export.json`、`endings_export.json` などを出力する
3. Unity プロジェクト側の Editor 拡張で JSON を読み込む
4. Unity Editor 内で `ConversationData`、`GameEventData`、`ActionReactionData`、`EndingData` の `.asset` を生成、更新する

この方式なら、`.asset` の GUID や型情報は Unity Editor が管理できます。
WPF ツールは、Unity に渡す内容の作成、整理、export に集中します。

## 検証観点

- `HeroineId` から期待するフォルダが作られる
- `profile.json` が保存、再読み込みできる
- 画像と prompt JSON が同じ `AssetId` で対応する
- キャラクター容姿プロンプトとスチル用テンプレートから positive prompt を生成できる
- スチル作業タブで用途フィルタが効く
- スチル作業タブで画像登録状況、prompt 保存状況、AssetStatus が表示される
- スチル作業タブで登録済み画像プレビューが表示される
- スチル作業タブから Prompt タブへ positive prompt を反映できる
- スチル作業タブから画像登録欄へ `AssetId`、用途、状態を反映できる
- スチル作業タブで変更した `Status` と `SpecificPrompt` が保存、再読み込み後も復元される
- `Accepted` の画像だけが export される
- Export report に件数と警告が出る
- Export タブから出力先フォルダを開ける
- 登録済み画像を選ぶとプレビューが表示される
- 画像タブで `All`, `Accepted`, `Pending`, `Rejected` の Status フィルタが効く
- 登録済み画像の Status を変更して `profile.json` に保存できる
- 画像ファイルを元画像欄へドラッグ&ドロップできる
- 既存 `AssetId` の画像登録時に上書き確認が表示される
- 上書き承認時に既存画像と `HeroineAsset` が更新される
- 画像登録後に解像度、形式、透過の検査結果が表示される
- Export 時に Accepted 画像の検査警告が `ExportReport.Warnings` に追加される
- Export タブの対象一覧には Accepted 画像だけが表示される
- Export 結果が `Docs/CharacterAssetGenerationToolSpec.md` の構成と一致する
- `heroine_profile_note.md` に Unity 側で必要な参照情報が入る

## 未決事項

- 作業データの保存先をアプリ直下に固定するか、ユーザーが選べるようにするか
- JSON の細かいスキーマ
- `StillWorkItems` を `profile.json` に保存する現方式で十分か、将来は専用 JSON に分離するか
- スチル状態 `StillStatus` と画像状態 `HeroineAsset.Status` をどの程度連動させるか
- prompt テンプレートのプレースホルダー名をどう定義するか
- ComfyUI workflow JSON のテンプレートを UI から編集できるようにするか、JSON ファイル編集のままにするか
- ComfyUI 生成中の詳細 progress を UI に出すか
- 会話データ JSON のスキーマを Unity 側の `ConversationData` などとどう対応させるか
- Unity Editor Import 拡張を別リポジトリで作るか、Unity プロジェクト側に直接置くか
- `.asset` の直接生成を将来も避けるか、限定条件付きで対応するか
- `net5.0-windows` 維持を前提に、将来ターゲットフレームワーク移行を再検証するタイミング
- 画像削除時に元ファイルも削除するか、profile から除外するだけにするか

## 次の担当者へのメモ

まずは見た目よりも、保存形式と export 結果を固めるのが重要です。

このツールの価値は、画像生成を自動化することより、採用済み素材、生成条件、Unity 取り込み先を失わずに管理することにあります。最初の実装では外部生成した画像を登録する前提で進め、ローカル ComfyUI 連携や Python 画像検査は後から追加する方が安全です。

次に優先するなら、ComfyUI 生成中の詳細 progress 表示、または workflow JSON のテンプレート編集画面を小さく実装するとよいです。

その後に、画像検査、テンプレート編集画面、Export 後の導線改善を進めるとよいです。現状の MVP は外部生成した画像または ComfyUI 生成画像を登録し、採用状態と prompt 記録を管理し、Unity 向けに出力する用途には使える状態です。
