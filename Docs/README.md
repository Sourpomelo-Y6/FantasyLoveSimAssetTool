# ドキュメント索引

AssetToolの操作、素材制作、Unityとのデータ契約はこのリポジトリを正本とする。
Unity Runtime、Scene、Prefab、ゲーム内での利用方法は `FantasyLoveSim/Docs` を参照する。

## 最初に読む文書

- [`../ReadMe.md`](../ReadMe.md): ツール概要、起動、実装済み機能
- [`Handoff.md`](Handoff.md): 現在の実装状況と次の候補
- [`CharacterAssetGenerationToolSpec.md`](CharacterAssetGenerationToolSpec.md): 素材管理・生成・Exportの全体仕様
- [`ToolUsabilityReorganizationPlan.md`](ToolUsabilityReorganizationPlan.md): 画面構成と操作改善

## データ制作・管理

- [`ConversationClassificationRules.md`](ConversationClassificationRules.md): 通常会話の分類、ID、priority
- [`GameEventDataGuide.md`](GameEventDataGuide.md): イベント条件と発火データ
- [`HeroineMainMenuActionPlan.md`](HeroineMainMenuActionPlan.md): ヒロイン別メニュー設定
- [`ExpressionCostumeVariantRoadmap.md`](ExpressionCostumeVariantRoadmap.md): 表情・衣装差分
- [`TransparentLayerAssetWorkflow.md`](TransparentLayerAssetWorkflow.md): 透過レイヤー素材
- [`PlayerAssetManagementPlan.md`](PlayerAssetManagementPlan.md): プレイヤー素材
- [`EnemyAssetManagementPlan.md`](EnemyAssetManagementPlan.md): 敵素材

## Unity連携

`Extra/` をExport／Importと双方向同期の正本とする。主な入口は次の文書。

- [`Extra/UnityImportPlan.md`](Extra/UnityImportPlan.md): JSON契約とUnity取り込み方針
- [`Extra/UnityEditorImportImplementationPlan.md`](Extra/UnityEditorImportImplementationPlan.md): Unity Editor側の実装
- [`Extra/UnityToWpfSyncPlan.md`](Extra/UnityToWpfSyncPlan.md): UnityからAssetToolへの同期
- [`Extra/CurrentFeatureSyncPlan.md`](Extra/CurrentFeatureSyncPlan.md): 機能別の同期状況
- [`Extra/ConversationDataPlan.md`](Extra/ConversationDataPlan.md): 会話・イベント・反応・エンディング
- [`Extra/TrainingImagePlan.md`](Extra/TrainingImagePlan.md): 訓練画像

## 更新ルール

- AssetToolの操作、素材定義、JSON契約はこのリポジトリだけで更新する
- Unity固有のRuntime挙動やScene設定はUnityリポジトリだけで更新する
- 同じ全文を両方のリポジトリへコピーしない
- 契約変更時は、AssetToolのExport、Unity Import、双方向同期、検証項目を同時に確認する
