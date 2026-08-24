using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ShortTextGenerationResult
    {
        public IReadOnlyList<string> Candidates { get; set; }
        public string Prompt { get; set; }
        public string RawResponse { get; set; }
        public string ModelId { get; set; }
    }

    public sealed class ShortTextGenerationService
    {
        private readonly ILocalLlmClient llmClient;

        public ShortTextGenerationService(ILocalLlmClient llmClient)
        {
            this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        public async Task<ShortTextGenerationResult> GenerateAsync(
            HeroineProfile profile,
            ShortTextGenerationTarget target,
            LocalAiSettings settings,
            string baseInstruction,
            CancellationToken cancellationToken = default)
        {
            if (profile == null) throw new InvalidOperationException("ヒロインを選択してください。");
            if (target == null) throw new InvalidOperationException("生成対象を選択してください。");
            string prompt = BuildPrompt(profile, target);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, baseInstruction, prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);

            return new ShortTextGenerationResult
            {
                Candidates = ParseCandidates(response.Content),
                Prompt = prompt,
                RawResponse = response.RawJson,
                ModelId = response.ModelId
            };
        }

        public static string BuildPrompt(HeroineProfile profile, ShortTextGenerationTarget target)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{target.Purpose}を、異なる内容で3件作成してください。");
            builder.AppendLine($"各{target.MinLength}～{target.MaxLength}文字のセリフだけにしてください。");
            Append(builder, "名前", profile.DisplayName, 40);
            Append(builder, "性格", profile.Personality, 200);
            Append(builder, "口調", profile.SpeakingStyle, 200);
            Append(builder, "一人称", profile.FirstPerson, 40);
            Append(builder, "二人称", profile.SecondPerson, 40);
            if (target.IncludeActionPolicy)
                Append(builder, "行動反応方針", profile.ActionReactionPolicy, 200);
            builder.AppendLine("{\"candidates\":[{\"text\":\"候補1\"},{\"text\":\"候補2\"},{\"text\":\"候補3\"}]}");
            return builder.ToString().Trim();
        }

        public static IReadOnlyList<string> ParseCandidates(string content)
        {
            string json = ExtractJson(content);
            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                if (!document.RootElement.TryGetProperty("candidates", out JsonElement candidates) ||
                    candidates.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("生成結果にcandidates配列がありません。");

                List<string> values = candidates.EnumerateArray()
                    .Select(GetCandidateText)
                    .Select(text => (text ?? string.Empty).Trim())
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                if (values.Count != 3)
                    throw new InvalidOperationException($"重複しない候補が3件必要ですが、{values.Count}件でした。");
                return values;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("生成結果を候補JSONとして解析できません。", ex);
            }
        }

        private static string GetCandidateText(JsonElement item)
        {
            if (item.ValueKind == JsonValueKind.String) return item.GetString();
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("text", out JsonElement text) && text.ValueKind == JsonValueKind.String)
                return text.GetString();
            return string.Empty;
        }

        private static string ExtractJson(string content)
        {
            string value = (content ?? string.Empty).Trim();
            int first = value.IndexOf('{');
            int last = value.LastIndexOf('}');
            if (first < 0 || last <= first) throw new InvalidOperationException("生成結果にJSONオブジェクトがありません。");
            return value.Substring(first, last - first + 1);
        }

        private static void Append(StringBuilder builder, string label, string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string text = value.Trim().Replace("\r", " ").Replace("\n", " ");
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            builder.AppendLine($"{label}: {text}");
        }
    }
}
