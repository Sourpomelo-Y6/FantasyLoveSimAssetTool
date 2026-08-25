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
    public sealed class ConversationDraftGenerationService
    {
        private readonly ILocalLlmClient llmClient;

        public ConversationDraftGenerationService(ILocalLlmClient llmClient)
        {
            this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        public async Task<ConversationDraftGenerationResult> GenerateAsync(
            LocalAiSettings settings,
            string baseInstruction,
            ConversationDraftGenerationContext context,
            CancellationToken cancellationToken = default)
        {
            string prompt = BuildPrompt(context);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, baseInstruction, prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);
            try
            {
                return new ConversationDraftGenerationResult
                {
                    Lines = ParseLines(response.Content, context.ExpressionIds),
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId
                };
            }
            catch (InvalidOperationException ex)
            {
                return new ConversationDraftGenerationResult
                {
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId,
                    ParseError = ex.Message
                };
            }
        }

        public static string BuildPrompt(ConversationDraftGenerationContext context)
        {
            if (context == null || string.IsNullOrWhiteSpace(context.AdditionalPrompt))
                throw new InvalidOperationException("状況テンプレートとキャラクター固有プロンプトを選択してください。");
            var builder = new StringBuilder();
            builder.AppendLine(context.AdditionalPrompt.Trim());
            builder.AppendLine("【現在の会話情報】");
            Append(builder, "会話種別", context.ConversationKind, 40);
            Append(builder, "会話ID", context.ConversationEntryId, 100);
            Append(builder, "カテゴリ", context.ConversationCategory, 100);
            Append(builder, "主要条件", context.ConditionSummary, 300);
            builder.AppendLine("会話の下書きを1案だけ、1～3行で作成してください。");
            builder.AppendLine("speakerはHeroine、Player、Systemのいずれかにしてください。");
            builder.AppendLine("ID、条件、好感度、画像、選択肢は生成・変更しないでください。");
            List<string> expressions = NormalizeExpressions(context.ExpressionIds);
            if (expressions.Count > 0)
                builder.AppendLine("Heroine行のexpressionIdは次のIDまたは空文字にしてください: " + string.Join(",", expressions));
            builder.AppendLine("{\"lines\":[{\"speaker\":\"Heroine\",\"text\":\"台詞\",\"expressionId\":\"表情ID\"}]}");
            return builder.ToString().Trim();
        }

        public static IReadOnlyList<ConversationDraftLine> ParseLines(
            string content,
            IReadOnlyCollection<string> allowedExpressionIds)
        {
            string value = (content ?? string.Empty).Trim();
            if (value.Length == 0) throw new InvalidOperationException("生成結果が空です。");
            try
            {
                int first = value.IndexOf('{');
                int last = value.LastIndexOf('}');
                if (first < 0 || last <= first) throw new JsonException();
                using JsonDocument document = JsonDocument.Parse(value.Substring(first, last - first + 1));
                if (!document.RootElement.TryGetProperty("lines", out JsonElement lines) ||
                    lines.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException("生成結果にlines配列がありません。");
                HashSet<string> expressions = new HashSet<string>(
                    NormalizeExpressions(allowedExpressionIds), StringComparer.Ordinal);
                List<ConversationDraftLine> result = lines.EnumerateArray()
                    .Take(3)
                    .Select(item => ParseLine(item, expressions))
                    .Where(line => line != null && !string.IsNullOrWhiteSpace(line.Text))
                    .ToList();
                if (result.Count == 0) throw new InvalidOperationException("利用可能な会話行がありません。");
                return result;
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("生成結果を会話下書きJSONとして解析できません。");
            }
        }

        private static ConversationDraftLine ParseLine(JsonElement item, HashSet<string> expressions)
        {
            if (item.ValueKind != JsonValueKind.Object) return null;
            string speaker = GetString(item, "speaker");
            if (speaker != "Heroine" && speaker != "Player" && speaker != "System") speaker = "Heroine";
            string expression = GetString(item, "expressionId");
            if (speaker != "Heroine" || !expressions.Contains(expression)) expression = string.Empty;
            return new ConversationDraftLine
            {
                Speaker = speaker,
                Text = GetString(item, "text").Trim(),
                ExpressionId = expression
            };
        }

        private static string GetString(JsonElement item, string propertyName) =>
            item.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;

        private static List<string> NormalizeExpressions(IReadOnlyCollection<string> values) =>
            (values ?? Array.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim()).Distinct(StringComparer.Ordinal).Take(20).ToList();

        private static void Append(StringBuilder builder, string label, string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string text = value.Trim().Replace("\r", " ").Replace("\n", " ");
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            builder.AppendLine($"{label}: {text}");
        }
    }
}
