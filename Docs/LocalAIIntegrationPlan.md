# LocalAIEventPrototype 統合計画

## 1. 目的

`LocalAIEventPrototype` で検証したローカルAI連携を `FantasyLoveSimAssetTool` に取り込み、ヒロインごとの会話、イベント、戦闘、訓練、衣装反応などの文章制作を支援する。

AssetToolをゲームデータの正本として維持し、AIは既存データを直接管理する主体ではなく、編集可能な文章案を生成する補助機能として扱う。

UIデザインは、現在のAssetToolよりも生成工程、状態、再実行箇所が分かりやすい `LocalAIEventPrototype` を基準とする。ただし、Prototypeの画面をそのまま追加するのではなく、AssetToolの既存タブとデータ構造へ統合する。

## 2. 確認した現状

### LocalAIEventPrototype

Prototypeには次の機能がある。

- llama.cppのOpenAI互換APIへの接続
- `/v1/models` によるモデル一覧取得と接続確認
- JSON Schemaを指定した構造化テキスト生成
- コードフェンスや余分な文章を含む応答の解析
- Character / Emotion / Action / Stateからのプロンプト合成
- ComfyUI画像生成
- Visionモデルによる画像説明
- バッチ生成、キャンセル、失敗項目の再試行
- 生成時プロンプトと生レスポンスの確認

再利用の中心は `Services/LlamaCppClient.cs` の通信処理と構造化応答処理である。画面、イベント専用モデル、`Characters.json` はAssetToolの構造と重複するため、そのまま移植しない。

### FantasyLoveSimAssetTool

AssetToolの `HeroineProfile` には、AI生成に必要な次の情報がすでにある。

- 名前、性格、話し方
- 一人称、二人称
- 好きなもの、嫌いなもの
- 朝、夜、ゲーム開始時などの固定台詞
- 行動反応方針、エンディング方針
- 衣装別メッセージ
- 通常会話、ゲームイベント、行動反応、予定イベント、エンディング
- 戦闘結果、戦闘パネル、単独帰還時のメッセージ
- 訓練中の台詞

したがって、キャラクター設定の正本はPrototypeの `Characters.json` ではなく、AssetToolのヒロイン別 `profile.json` とする。

## 3. 基本設計

文章生成は次の4層からプロンプトを構築する。

```text
共通の執筆指示
  + ヒロイン設定
  + 文言種類別テンプレート
  + 現在の場面・ゲーム条件
```

### 共通の執筆指示

- 恋愛シミュレーションゲーム向けの自然な日本語にする
- 設定にない事実を勝手に確定しない
- 一人称、二人称、口調を守る
- 指定された長さと出力形式を守る
- 制作メモやプロンプトをゲーム本文へ混ぜない
- 指定されたJSONだけを返す

ワークスペース共通設定として保存し、ヒロインデータとは分離する。

### ヒロイン設定

`HeroineProfile` から名前、性格、話し方、一人称、二人称、好み、各種方針を自動的に収集する。

口調の安定化のため、将来次の項目を追加する。

- `SpeechExamples`: 代表的な台詞3～5件
- `ForbiddenExpressions`: 禁止する語尾や表現
- `BackstorySummary`: AIが参照してよい背景情報
- `PlayerRelationshipPolicy`: 好感度帯ごとの距離感
- `WritingNotes`: 全文言へ適用する演技上の注意

既存の朝、夜、開始時台詞は口調例として利用できるが、固定文言と参考例を区別してプロンプトへ渡す。

### 文言種類別テンプレート

共通指示とキャラクター設定だけでは、短いUI文言と複数ページのイベント本文を安定して書き分けられない。そのため、用途ごとに次を定義する。

- 生成目的
- 文字数または行数
- 必須の意味
- 禁止事項
- 出力JSON Schema
- 利用できる表情、話者などの候補
- 既存文との重複を避けるか

対象テンプレートの例：

- `morning_greeting`
- `outfit_changed_message`
- `training_dialogue`
- `battle_panel_message`
- `conversation`
- `game_event`
- `action_reaction`
- `scheduled_event`
- `ending`

### 現在の場面・ゲーム条件

生成時に選択中のデータから、必要な情報だけを渡す。

- 会話種別、カテゴリ
- 好感度範囲
- 場所、行動、時間、季節、天候
- 衣装、戦闘結果、訓練状態
- イベントタイトル、制作メモ
- 前後の台詞
- 使用可能な表情ID

ID、発火条件、priority、参照Asset IDなど、ゲーム構造に関わる値はAIに自由決定させない。画面で指定した値を維持し、AIは主にタイトルと本文を生成する。

## 4. サービス構成

```text
AssetToolの各編集画面
        ↓
LocalTextGenerationService
        ↓
PromptComposer
  ├ 共通指示
  ├ HeroineProfile
  ├ タスク別テンプレート
  └ 選択中の条件
        ↓
ILocalLlmClient / LlamaCppClient
        ↓
構造化結果と検証結果をプレビュー
        ↓
利用者が採用
        ↓
既存の保存・Export処理
```

通信クライアントはGemmaやQwenなどの固有名に依存させず、OpenAI互換APIを提供する任意のローカルモデルを選択できる設計にする。

AI応答を受け取った時点では既存データを変更しない。生成結果のプレビューから「採用」を実行したときだけ編集モデルへ反映し、保存は既存の保存コマンドで行う。

## 5. UI方針

### Prototypeから採用する要素

- 暗色ベースのパネルとアクセントカラー
- 大きなセクション見出しと工程番号
- 左右2カラムによる入力条件と結果の分離
- 接続中、生成中、成功、失敗のステータスバッジ
- 主要操作を大きく見せるボタン
- 詳細な接続設定や生レスポンスを `Expander` へ収納
- 入力、生成結果、生レスポンスを画面内で確認できる構成
- 失敗しても完了済みの結果を保持し、該当工程だけ再実行できる操作
- 画面下部の常時表示ステータス

### AssetTool向けの適用方法

Prototypeの単一イベント生成画面を丸ごと追加せず、AssetToolの各タブに共通の「AI文章支援パネル」を設ける。

```text
選択中データと生成条件
        │
        ├ AI生成
        ├ 別案を生成
        └ 空欄のみ一括生成

生成候補
        ├ 候補1
        ├ 候補2
        └ 候補3

[採用] [再生成] [破棄]
```

通常利用では接続設定や生レスポンスを隠し、必要なときだけ展開する。大量の既存項目を扱うAssetToolでは、Prototypeの視認性を保ちながら画面の縦方向への肥大化を避ける。

共通スタイルは `ResourceDictionary` に分離し、既存タブへ段階的に適用する。最初から全画面を一括変更せず、LocalAI設定画面とAI文章支援パネルでデザインを確立してから既存画面の整理へ広げる。

## 6. AI生成対象の導入順

### 第1段階: 短い固定文言

- 朝と夜の挨拶
- 初期台詞、次の行動を促す文言
- 衣装ロック、衣装変更、衣装反応
- 戦闘パネル結果メッセージ
- 単独帰還反応
- 訓練中の短い台詞

1件生成、別案3件、空欄のみ一括生成を提供する。

### 第2段階: 会話とイベント本文

`ConversationEntry` と `ConversationLine` に対応した構造化JSONを生成する。

```json
{
  "title": "...",
  "lines": [
    {
      "speaker": "Heroine",
      "text": "...",
      "expression": "Smile"
    }
  ]
}
```

対象は通常会話、GameEvents、ActionReactions、ScheduledEvents、BattleResultEvents、Endingsとする。既存行の置換、続き生成、別案生成を区別する。

### 第3段階: バッチ生成と品質検査

- 好感度帯ごとの通常会話
- 各ジャンルのフォールバック会話
- 訓練状態ごとの候補
- 戦闘結果、衣装、季節、天候の差分
- 空欄のみ、選択項目のみ、失敗項目のみの実行
- キャンセルと個別再試行

生成後に一人称、二人称、禁止表現、長さ、重複、話者、表情IDを検査する。

### 第4段階: 画像・Vision連携

- AssetToolの登録画像またはComfyUI生成画像を入力
- VisionモデルでScene Descriptionを生成
- Scene Descriptionとイベント条件から本文を生成
- 画像、文章、使用プロンプトの対応を記録

まずテキスト生成を安定させ、その後に追加する。

## 7. 実装フェーズ

### Phase 1: LLM基盤

- `ILocalLlmClient` と `LlamaCppClient` を追加
- URL検証、モデル一覧、接続テスト
- タイムアウト、キャンセル、空応答、HTTPエラー処理
- JSON Schemaと応答解析
- AssetToolは現在.NET 5のため、Prototypeのコードを.NET 5互換へ調整する

### Phase 2: 設定とプロンプト合成

- `LocalAISettings` を追加
- 共通指示とタスク別テンプレートを外部ファイル化
- `HeroineProfile` からキャラクターコンテキストを生成
- 送信プロンプトのプレビュー
- 接続設定とゲームデータを分離

保存構成案：

```text
LocalAISettings/
├ connection.json
├ base-instruction.txt
└ text-tasks.json
```

秘密情報を扱うようになった場合はGit管理対象へ平文保存しない。

### Phase 3: 短文生成UI

- Prototype準拠の共通スタイルを作成
- LocalAI接続・モデル設定画面を追加
- 選択フィールド用のAI文章支援パネルを追加
- 候補生成、採用、再生成、破棄、元に戻すを実装
- 戦闘と訓練の空欄一括生成を追加

### Phase 4: 会話・イベント生成

- `ConversationEntry` を生成入力へ変換
- 複数行の構造化出力を解析
- 登録済みの話者、表情だけを許可
- 生成結果へ既存の検証処理を実行

### Phase 5: バッチと品質管理

- 進捗、キャンセル、個別失敗、再試行
- 類似・重複文チェック
- キャラクター口調と禁止表現の検査
- 生応答とエラーを確認できるログ

### Phase 6: 画像・Vision統合

- PrototypeのVision処理を汎用化
- AssetToolのComfyUI生成・登録画像と接続
- Scene Descriptionを編集可能な中間結果として保持

## 8. 最初のMVP完了条件

1. AssetToolからllama.cppへ接続確認できる
2. 共通指示を編集・保存できる
3. 選択中の `HeroineProfile` からキャラクター設定を構築できる
4. 短い文言を候補として3件生成できる
5. 採用前は既存データを変更しない
6. 採用後も既存の保存操作を行うまでファイルを書き換えない
7. 戦闘・訓練の空欄のみ一括生成できる
8. 通信失敗、解析失敗、キャンセルで既存データを失わない
9. 接続、解析、プロンプト合成、採用処理のテストがある
10. LocalAI設定画面と支援パネルがPrototype準拠の視覚設計になっている

## 9. 設計上の決定

- キャラクター設定の正本はAssetToolの `HeroineProfile` とする
- Prototypeの独立したイベントモデルと `Characters.json` は統合後の正本にしない
- AIは文章案を生成し、ID、条件、priority、Asset参照はAssetToolが管理する
- AI応答を直接保存せず、必ずプレビューと採用操作を通す
- 共通指示だけでなく、文言種類別テンプレートと現在条件を使用する
- UIはLocalAIEventPrototypeをデザイン基準とする
- UI全体の刷新とLocalAI機能追加を同時に行わず、共通スタイルとAI支援画面から段階的に適用する
- テキスト生成を先に完成させ、Visionと画像連携は後続フェーズにする

