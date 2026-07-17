using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public static class CharacterProductionStatusService
    {
        private static readonly string[] RequiredResultTypes =
            { "SoloVictory", "DuoVictory", "SoloDefeat", "DuoDefeat" };
        private static readonly string[] RequiredPanelTypes = { "Victory", "Defeat" };

        public static CharacterProductionStatusRow Evaluate(HeroineProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return new CharacterProductionStatusRow
            {
                CharacterId = profile.HeroineId ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.HeroineId : profile.DisplayName,
                BasicInformation = EvaluateBasicInformation(profile),
                BattleMessages = EvaluateBattleMessages(profile),
                TrainingImages = EvaluateTrainingImages(profile)
            };
        }

        private static ProductionStatusCell EvaluateBasicInformation(HeroineProfile profile)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>
            {
                ["HeroineId"] = profile.HeroineId,
                ["表示名"] = profile.DisplayName,
                ["性格"] = profile.Personality,
                ["口調"] = profile.SpeakingStyle
            };
            string[] missing = fields.Where(x => string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Key).ToArray();
            return Cell(profile, "基本情報", 0,
                missing.Length == 0 ? ProductionStatusKind.Complete :
                missing.Length == fields.Count ? ProductionStatusKind.Missing : ProductionStatusKind.Partial,
                missing.Length == 0 ? "必須4項目を入力済みです。" :
                $"入力済み {fields.Count - missing.Length}/{fields.Count}。不足: {string.Join(", ", missing)}",
                fields.Select(x => Check(x.Key, !string.IsNullOrWhiteSpace(x.Value),
                    string.IsNullOrWhiteSpace(x.Value) ? "未入力です。" : "入力済みです。")).ToArray());
        }

        private static ProductionStatusCell EvaluateBattleMessages(HeroineProfile profile)
        {
            List<BattleResultEventEntry> events = profile.BattleMessages?.ResultEvents?.Where(x => x != null).ToList()
                ?? new List<BattleResultEventEntry>();
            List<BattlePanelResultMessageEntry> panels = profile.BattleMessages?.PanelMessages?.Where(x => x != null).ToList()
                ?? new List<BattlePanelResultMessageEntry>();
            List<string> missing = RequiredResultTypes.Where(type => !events.Any(x =>
                string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)))
                .Select(type => "結果:" + type).ToList();
            missing.AddRange(RequiredPanelTypes.Where(type => !panels.Any(x =>
                string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)))
                .Select(type => "パネル:" + type));
            int completed = RequiredResultTypes.Length + RequiredPanelTypes.Length - missing.Count;
            ProductionStatusKind kind = missing.Count == 0 ? ProductionStatusKind.Complete :
                completed == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;
            string details = missing.Count == 0
                ? $"必須結果イベント {RequiredResultTypes.Length} 件、パネル文 {RequiredPanelTypes.Length} 件を登録済みです。逃走は任意です。"
                : $"必須項目 {completed}/{RequiredResultTypes.Length + RequiredPanelTypes.Length}。不足: {string.Join(", ", missing)}。逃走は任意です。";
            List<ProductionStatusCheckItem> checks = RequiredResultTypes.Select(type => Check(
                "戦闘結果 " + type,
                events.Any(x => string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)),
                "対応する本文入りイベントが必要です。")).ToList();
            checks.AddRange(RequiredPanelTypes.Select(type => Check(
                "戦闘パネル " + type,
                panels.Any(x => string.Equals(x.ResultType, type, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(x.Message)),
                "対応する本文入りパネル文が必要です。")));
            checks.Add(Check("逃走イベント", true, "任意項目のため完成判定には影響しません。"));
            return Cell(profile, "戦闘メッセージ", 2, kind, details, checks);
        }

        private static ProductionStatusCell EvaluateTrainingImages(HeroineProfile profile)
        {
            string[] trainingIds = profile.TrainingCatalog?.Items?
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.TrainingId))
                .Select(x => x.TrainingId.Trim()).Distinct(StringComparer.Ordinal).ToArray() ?? Array.Empty<string>();
            if (trainingIds.Length == 0)
            {
                return Cell(profile, "訓練画像", 4, ProductionStatusKind.Missing,
                    "登録済み訓練がありません。先にUnity訓練一覧を読み込んでください。",
                    new[] { Check("訓練一覧", false, "Unity訓練一覧を読み込んでください。") });
            }

            Dictionary<string, HeroineAsset> assets = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.AssetId))
                .GroupBy(x => x.AssetId, StringComparer.Ordinal)
                .ToDictionary(x => x.Key, x => x.Last(), StringComparer.Ordinal);
            List<string> incomplete = new List<string>();
            List<ProductionStatusCheckItem> checks = new List<ProductionStatusCheckItem>();
            int completeSlots = 0;
            foreach (string trainingId in trainingIds)
            {
                TrainingImageEntry entry = profile.TrainingImages?.Items?.FirstOrDefault(x =>
                    x != null && string.Equals(x.TrainingId, trainingId, StringComparison.Ordinal));
                string[] ids = entry == null ? Array.Empty<string>() : new[]
                {
                    entry.BeforeFirstStepImageAssetId, entry.AfterFirstStepImageAssetId,
                    entry.PlayerLpConsumedImageAssetId, entry.HeroineLpConsumedImageAssetId,
                    entry.SimultaneousLpConsumedImageAssetId
                };
                int accepted = ids.Count(id => !string.IsNullOrWhiteSpace(id) && assets.TryGetValue(id, out HeroineAsset asset) && asset.Status == AssetStatus.Accepted);
                completeSlots += accepted;
                if (accepted < 5) incomplete.Add($"{trainingId} {accepted}/5");
                string[] stateNames = { "開始前", "進行後", "主人公LP消費", "ヒロインLP消費", "同時LP消費" };
                for (int i = 0; i < stateNames.Length; i++)
                {
                    string assetId = i < ids.Length ? ids[i] : string.Empty;
                    bool acceptedSlot = !string.IsNullOrWhiteSpace(assetId) &&
                        assets.TryGetValue(assetId, out HeroineAsset asset) && asset.Status == AssetStatus.Accepted;
                    checks.Add(Check($"{trainingId} / {stateNames[i]}", acceptedSlot,
                        string.IsNullOrWhiteSpace(assetId) ? "画像AssetIdが未設定です。" :
                        acceptedSlot ? $"{assetId} はAcceptedです。" : $"{assetId} は未採用です。"));
                }
            }

            int totalSlots = trainingIds.Length * 5;
            ProductionStatusKind kind = completeSlots == totalSlots ? ProductionStatusKind.Complete :
                completeSlots == 0 ? ProductionStatusKind.Missing : ProductionStatusKind.Partial;
            string details = kind == ProductionStatusKind.Complete
                ? $"登録済み訓練 {trainingIds.Length} 件の全 {totalSlots} 枠がAcceptedです。"
                : $"Accepted {completeSlots}/{totalSlots} 枠。不足: {string.Join(", ", incomplete)}";
            return Cell(profile, "訓練画像", 4, kind, details, checks);
        }

        private static ProductionStatusCheckItem Check(string name, bool complete, string details) =>
            new ProductionStatusCheckItem { Name = name, IsComplete = complete, Details = details };

        private static ProductionStatusCell Cell(
            HeroineProfile profile,
            string categoryName,
            int tabIndex,
            ProductionStatusKind kind,
            string details,
            IReadOnlyList<ProductionStatusCheckItem> checks) =>
            new ProductionStatusCell
            {
                CategoryName = categoryName,
                CharacterId = profile.HeroineId ?? string.Empty,
                TargetTabIndex = tabIndex,
                Kind = kind,
                Details = details,
                Checks = checks
            };
    }
}
