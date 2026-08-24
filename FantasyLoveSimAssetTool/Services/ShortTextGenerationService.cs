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
        public string ParseError { get; set; }
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
            int candidateCount = 3,
            IReadOnlyCollection<string> excludedCandidates = null,
            CancellationToken cancellationToken = default)
        {
            if (profile == null) throw new InvalidOperationException("ヒロインを選択してください。");
            if (target == null) throw new InvalidOperationException("生成対象を選択してください。");
            string prompt = BuildPrompt(profile, target, candidateCount, excludedCandidates);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, baseInstruction, prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);

            IReadOnlyList<string> candidates;
            string parseError = string.Empty;
            try
            {
                candidates = ParseCandidates(response.Content);
            }
            catch (InvalidOperationException ex)
            {
                candidates = Array.Empty<string>();
                parseError = ex.Message;
            }

            return new ShortTextGenerationResult
            {
                Candidates = candidates,
                Prompt = prompt,
                RawResponse = response.Content,
                ModelId = response.ModelId,
                ParseError = parseError
            };
        }

        public static string BuildPrompt(HeroineProfile profile, ShortTextGenerationTarget target,
            int candidateCount = 3, IReadOnlyCollection<string> excludedCandidates = null)
        {
            if (candidateCount < 1 || candidateCount > 3)
                throw new InvalidOperationException("候補数は1～3件で指定してください。");
            var builder = new StringBuilder();
            builder.AppendLine($"{target.Purpose}を、異なる内容で{candidateCount}件作成してください。");
            builder.AppendLine($"各{target.MinLength}～{target.MaxLength}文字のセリフだけにしてください。");
            Append(builder, "名前", profile.DisplayName, 40);
            Append(builder, "性格", profile.Personality, 200);
            Append(builder, "口調", profile.SpeakingStyle, 200);
            Append(builder, "一人称", profile.FirstPerson, 40);
            Append(builder, "二人称", profile.SecondPerson, 40);
            if (target.IncludeActionPolicy)
                Append(builder, "行動反応方針", profile.ActionReactionPolicy, 200);
            List<string> exclusions = (excludedCandidates ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)).Take(3).ToList();
            if (exclusions.Count > 0)
            {
                builder.AppendLine("次の既存候補と重複させないでください:");
                foreach (string exclusion in exclusions) Append(builder, "除外", exclusion, 100);
            }
            builder.AppendLine("{\"candidates\":[{\"text\":\"候補\"}]}");
            return builder.ToString().Trim();
        }

        public static IReadOnlyList<string> ParseCandidates(string content)
        {
            string value = (content ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException("生成結果が空です。");
            try
            {
                string json = ExtractJson(value);
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

                if (values.Count == 0) throw new InvalidOperationException("生成結果に利用可能な候補がありません。");
                return values;
            }
            catch (JsonException)
            {
                IReadOnlyList<string> plainCandidates = ParsePlainTextCandidates(value);
                if (plainCandidates.Count > 0) return plainCandidates;
                throw new InvalidOperationException("生成結果を候補JSONまたはプレーンテキストとして解析できません。");
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
            if (first < 0 || last <= first) throw new JsonException("JSON object was not found.");
            return value.Substring(first, last - first + 1);
        }

        private static IReadOnlyList<string> ParsePlainTextCandidates(string content)
        {
            return content.Replace("\r", string.Empty)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(CleanPlainTextLine)
                .Where(line => !string.IsNullOrWhiteSpace(line) &&
                    !line.StartsWith("```", StringComparison.Ordinal) &&
                    !line.StartsWith("{", StringComparison.Ordinal) &&
                    !line.StartsWith("}", StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .Take(3)
                .ToList();
        }

        private static string CleanPlainTextLine(string line)
        {
            string value = (line ?? string.Empty).Trim();
            value = System.Text.RegularExpressions.Regex.Replace(value, @"^[-*・\s]+", string.Empty);
            value = System.Text.RegularExpressions.Regex.Replace(value, @"^\d+[\.\)）:\s]+", string.Empty);
            return value.Trim().Trim('"', '「', '」');
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
