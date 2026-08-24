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
        public IReadOnlyList<ShortTextGeneratedCandidate> Candidates { get; set; }
        public string Prompt { get; set; }
        public string RawResponse { get; set; }
        public string ModelId { get; set; }
        public string ParseError { get; set; }
    }

    public sealed class ShortTextGeneratedCandidate
    {
        public string Text { get; set; }
        public string ExpressionId { get; set; }
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
            ShortTextGenerationContext context = null,
            IReadOnlyCollection<string> expressionIds = null,
            CancellationToken cancellationToken = default)
        {
            if (profile == null) throw new InvalidOperationException("ヒロインを選択してください。");
            if (target == null) throw new InvalidOperationException("生成対象を選択してください。");
            string prompt = BuildPrompt(profile, target, candidateCount, excludedCandidates, context, expressionIds);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, baseInstruction, prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);

            IReadOnlyList<ShortTextGeneratedCandidate> candidates;
            string parseError = string.Empty;
            try
            {
                candidates = ParseCandidateItems(response.Content, expressionIds);
            }
            catch (InvalidOperationException ex)
            {
                candidates = Array.Empty<ShortTextGeneratedCandidate>();
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
            int candidateCount = 3, IReadOnlyCollection<string> excludedCandidates = null,
            ShortTextGenerationContext context = null, IReadOnlyCollection<string> expressionIds = null)
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
            if (target.RequiredContext == "OutfitMessage")
            {
                if (string.IsNullOrWhiteSpace(context?.OutfitId))
                    throw new InvalidOperationException("衣装メッセージ行を選択し、OutfitIdを入力してください。");
                Append(builder, "衣装ID", context.OutfitId, 80);
            }
            else if (target.RequiredContext == "OutfitReaction")
            {
                if (string.IsNullOrWhiteSpace(context?.ReactionType))
                    throw new InvalidOperationException("衣装反応行を選択し、ReactionTypeを入力してください。");
                Append(builder, "反応種類", context.ReactionType, 80);
            }
            List<string> exclusions = (excludedCandidates ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)).Take(3).ToList();
            if (exclusions.Count > 0)
            {
                builder.AppendLine("次の既存候補と重複させないでください:");
                foreach (string exclusion in exclusions) Append(builder, "除外", exclusion, 100);
            }
            List<string> allowedExpressions = NormalizeExpressionIds(expressionIds);
            if (target.RequiredContext == "OutfitMessage" || target.RequiredContext == "OutfitReaction")
            {
                if (allowedExpressions.Count > 0)
                    builder.AppendLine("各候補の表情は次のIDから1つだけ選んでください: " + string.Join(",", allowedExpressions));
                builder.AppendLine("{\"candidates\":[{\"text\":\"候補\",\"expressionId\":\"表情ID\"}]}");
            }
            else
            {
                builder.AppendLine("{\"candidates\":[{\"text\":\"候補\"}]}");
            }
            return builder.ToString().Trim();
        }

        public static IReadOnlyList<string> ParseCandidates(string content)
        {
            return ParseCandidateItems(content).Select(candidate => candidate.Text).ToList();
        }

        public static IReadOnlyList<ShortTextGeneratedCandidate> ParseCandidateItems(
            string content,
            IReadOnlyCollection<string> allowedExpressionIds = null)
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

                HashSet<string> allowedExpressions = new HashSet<string>(
                    NormalizeExpressionIds(allowedExpressionIds), StringComparer.Ordinal);
                List<ShortTextGeneratedCandidate> values = candidates.EnumerateArray()
                    .Select(item => GetCandidate(item, allowedExpressions))
                    .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Text))
                    .GroupBy(candidate => candidate.Text, StringComparer.Ordinal)
                    .Select(group => group.First())
                    .ToList();

                if (values.Count == 0) throw new InvalidOperationException("生成結果に利用可能な候補がありません。");
                return values;
            }
            catch (JsonException)
            {
                IReadOnlyList<ShortTextGeneratedCandidate> plainCandidates = ParsePlainTextCandidates(value)
                    .Select(text => new ShortTextGeneratedCandidate { Text = text, ExpressionId = string.Empty })
                    .ToList();
                if (plainCandidates.Count > 0) return plainCandidates;
                throw new InvalidOperationException("生成結果を候補JSONまたはプレーンテキストとして解析できません。");
            }
        }

        private static ShortTextGeneratedCandidate GetCandidate(JsonElement item, HashSet<string> allowedExpressionIds)
        {
            string text = (GetCandidateText(item) ?? string.Empty).Trim();
            string expressionId = string.Empty;
            if (item.ValueKind == JsonValueKind.Object &&
                item.TryGetProperty("expressionId", out JsonElement expression) &&
                expression.ValueKind == JsonValueKind.String)
            {
                string proposed = (expression.GetString() ?? string.Empty).Trim();
                if (allowedExpressionIds.Contains(proposed)) expressionId = proposed;
            }
            return new ShortTextGeneratedCandidate { Text = text, ExpressionId = expressionId };
        }

        private static List<string> NormalizeExpressionIds(IReadOnlyCollection<string> expressionIds)
        {
            return (expressionIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.Ordinal)
                .Take(20)
                .ToList();
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
