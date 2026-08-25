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
    }
}
