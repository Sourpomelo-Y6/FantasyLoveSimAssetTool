using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationPromptService
    {
        private readonly string workspaceRoot;
        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ConversationPromptService(string workspaceRoot)
        {
            this.workspaceRoot = workspaceRoot ?? throw new ArgumentNullException(nameof(workspaceRoot));
        }

        public IReadOnlyList<ConversationSituationPrompt> LoadSituations()
        {
            string path = Path.Combine(workspaceRoot, "PromptTemplates", "conversation-situations.json");
            if (!File.Exists(path)) return Array.Empty<ConversationSituationPrompt>();
            try
            {
                return (JsonSerializer.Deserialize<List<ConversationSituationPrompt>>(
                        File.ReadAllText(path), jsonOptions) ?? new List<ConversationSituationPrompt>())
                    .Where(IsValidSituation)
                    .ToList();
            }
            catch (JsonException)
            {
                return Array.Empty<ConversationSituationPrompt>();
            }
        }

        public ConversationCharacterPrompt LoadCharacterPrompt(string heroineId)
        {
            if (string.IsNullOrWhiteSpace(heroineId) ||
                heroineId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                heroineId.Contains("..", StringComparison.Ordinal)) return null;
            string path = Path.Combine(workspaceRoot, "Characters", heroineId, "conversation-ai-prompt.json");
            if (!File.Exists(path)) return null;
            try
            {
                ConversationCharacterPrompt prompt = JsonSerializer.Deserialize<ConversationCharacterPrompt>(
                    File.ReadAllText(path), jsonOptions);
                return IsValidCharacter(prompt) &&
                    string.Equals(prompt.HeroineId, heroineId, StringComparison.Ordinal)
                    ? prompt
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static ConversationCharacterPrompt BuildCharacterPrompt(HeroineProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.HeroineId)) return null;
            string displayName = string.IsNullOrWhiteSpace(profile.DisplayName)
                ? profile.HeroineId.Trim() : profile.DisplayName.Trim();
            var summaryParts = new[] { profile.Personality, profile.ActionReactionPolicy, profile.EndingPolicy }
                .Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => Limit(value, 160)).Take(3).ToList();
            var prompt = new ConversationCharacterPrompt
            {
                HeroineId = profile.HeroineId.Trim(),
                DisplayName = displayName,
                Summary = summaryParts.Count > 0
                    ? string.Join(" ", summaryParts)
                    : $"{displayName}のプロフィール設定に従い、設定にない性格や過去を追加せずに話す。",
                FirstPerson = profile.FirstPerson?.Trim() ?? string.Empty,
                SecondPerson = profile.SecondPerson?.Trim() ?? string.Empty
            };
            if (prompt.Summary.Length > 400) prompt.Summary = prompt.Summary.Substring(0, 400);
            AddIfPresent(prompt.VoiceRules, profile.SpeakingStyle);
            AddIfPresent(prompt.RelationshipRules, profile.ActionReactionPolicy);
            AddIfPresent(prompt.RelationshipRules, profile.EndingPolicy);
            AddIfPresent(prompt.EmotionRules, profile.Likes, "好きなものとして設定されている: ");
            AddIfPresent(prompt.EmotionRules, profile.Dislikes, "苦手なものとして設定されている: ");
            prompt.AvoidExpressions.Add("プロフィールにない過去、家族、能力、好みを断定しない");
            prompt.AvoidExpressions.Add("会話ID、場所ID、衣装IDなどの内部設定値を台詞に含めない");
            foreach (string line in new[]
            {
                profile.InitialDialogueMessage, profile.MorningGreeting, profile.GoodNightGreeting
            }.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => Limit(value, 160)).Distinct().Take(3))
                prompt.ReferenceLines.Add(line);
            return prompt;
        }

        public void SaveCharacterPrompt(ConversationCharacterPrompt prompt)
        {
            if (!IsValidCharacter(prompt) || prompt.HeroineId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                prompt.HeroineId.Contains("..", StringComparison.Ordinal))
                throw new InvalidOperationException("保存するキャラクター固有プロンプトが不正です。");
            string directory = Path.Combine(workspaceRoot, "Characters", prompt.HeroineId.Trim());
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "conversation-ai-prompt.json");
            string temporaryPath = path + ".tmp";
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(prompt, options));
            if (File.Exists(path)) File.Replace(temporaryPath, path, path + ".bak");
            else File.Move(temporaryPath, path);
        }

        public static string BuildCharacterPromptPreview(ConversationCharacterPrompt character)
        {
            if (!IsValidCharacter(character)) return string.Empty;
            var builder = new StringBuilder();
            builder.AppendLine($"名前: {character.DisplayName}");
            builder.AppendLine($"概要: {character.Summary}");
            builder.AppendLine($"一人称: {character.FirstPerson} / 二人称: {character.SecondPerson}");
            AppendList(builder, "口調", character.VoiceRules, 8);
            AppendList(builder, "関係性", character.RelationshipRules, 8);
            AppendList(builder, "感情表現", character.EmotionRules, 8);
            AppendList(builder, "禁止表現", character.AvoidExpressions, 8);
            AppendList(builder, "参考台詞", character.ReferenceLines, 3);
            return builder.ToString().Trim();
        }

        public static string BuildAdditionalPrompt(
            ConversationSituationPrompt situation,
            ConversationCharacterPrompt character)
        {
            if (!IsValidSituation(situation)) throw new InvalidOperationException("状況プロンプトが不正です。");
            if (!IsValidCharacter(character)) throw new InvalidOperationException("キャラクター固有プロンプトが不正です。");
            var builder = new StringBuilder();
            builder.AppendLine("【状況指示】");
            builder.AppendLine(situation.Instruction.Trim());
            AppendList(builder, "必須", situation.RequiredElements, 8);
            AppendList(builder, "避ける", situation.AvoidElements, 8);
            builder.AppendLine("【キャラクター固有指示】");
            builder.AppendLine($"名前: {character.DisplayName}");
            builder.AppendLine($"概要: {character.Summary.Trim()}");
            builder.AppendLine($"一人称: {character.FirstPerson} / 二人称: {character.SecondPerson}");
            AppendList(builder, "口調", character.VoiceRules, 8);
            AppendList(builder, "関係性", character.RelationshipRules, 8);
            AppendList(builder, "感情表現", character.EmotionRules, 8);
            AppendList(builder, "禁止表現", character.AvoidExpressions, 8);
            AppendList(builder, "参考", character.ReferenceLines, 3);
            builder.AppendLine("参考台詞をそのまま再出力せず、状況に合う新しい会話にしてください。");
            return builder.ToString().Trim();
        }

        private static bool IsValidSituation(ConversationSituationPrompt value) =>
            value != null && !string.IsNullOrWhiteSpace(value.SituationId) &&
            !string.IsNullOrWhiteSpace(value.DisplayName) && !string.IsNullOrWhiteSpace(value.Instruction);

        private static bool IsValidCharacter(ConversationCharacterPrompt value) =>
            value != null && !string.IsNullOrWhiteSpace(value.HeroineId) &&
            !string.IsNullOrWhiteSpace(value.DisplayName) && !string.IsNullOrWhiteSpace(value.Summary);

        private static void AppendList(StringBuilder builder, string label, IEnumerable<string> values, int limit)
        {
            List<string> items = (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)).Take(limit).ToList();
            if (items.Count > 0) builder.AppendLine(label + ": " + string.Join(" / ", items));
        }

        private static void AddIfPresent(List<string> values, string value, string prefix = "")
        {
            if (!string.IsNullOrWhiteSpace(value)) values.Add(prefix + Limit(value, 240));
        }

        private static string Limit(string value, int maximumLength)
        {
            string normalized = (value ?? string.Empty).Trim().Replace("\r", " ").Replace("\n", " ");
            return normalized.Length <= maximumLength ? normalized : normalized.Substring(0, maximumLength);
        }
    }
}
