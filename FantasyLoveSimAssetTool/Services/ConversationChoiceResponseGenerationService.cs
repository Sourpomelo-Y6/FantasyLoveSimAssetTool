using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationChoiceResponseGenerationService
    {
        private readonly ILocalLlmClient llmClient;

        public ConversationChoiceResponseGenerationService(ILocalLlmClient llmClient)
        {
            this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        public async Task<ConversationChoiceResponseGenerationResult> GenerateAsync(
            LocalAiSettings settings, string baseInstruction,
            ConversationChoiceResponseGenerationContext context,
            CancellationToken cancellationToken = default)
        {
            string prompt = BuildPrompt(context);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, baseInstruction, prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);
            try
            {
                return new ConversationChoiceResponseGenerationResult
                {
                    Candidates = ShortTextGenerationService.ParseCandidates(response.Content).Take(3).ToList(),
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId
                };
            }
            catch (InvalidOperationException ex)
            {
                return new ConversationChoiceResponseGenerationResult
                {
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId,
                    ParseError = ex.Message
                };
            }
        }

        public static string BuildPrompt(ConversationChoiceResponseGenerationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(context.ChoiceText))
                throw new InvalidOperationException("プレイヤーの選択肢文言を入力してください。");
            var builder = new StringBuilder();
            builder.AppendLine("【最優先の役割】");
            builder.AppendLine("プレイヤーが選んだ発言または行動に対する、ヒロインの直接の返答を生成してください。");
            builder.AppendLine("プレイヤー側の台詞、選択肢、好感度、条件、ID、分岐構造は生成・変更しないでください。");
            if (!string.IsNullOrWhiteSpace(context.CharacterPrompt))
            {
                builder.AppendLine("【ヒロインのキャラクター設定】");
                AppendBlock(builder, context.CharacterPrompt, 3000);
            }
            builder.AppendLine("【現在の会話情報】");
            Append(builder, "会話種別", context.ConversationKind, 40);
            Append(builder, "会話ID", context.ConversationEntryId, 100);
            Append(builder, "カテゴリ", context.ConversationCategory, 100);
            Append(builder, "直前の台詞", context.PreviousLine, 400);
            Append(builder, "プレイヤーが選んだ選択肢", context.ChoiceText, 200);
            if (!string.IsNullOrWhiteSpace(context.AdditionalInstruction))
            {
                builder.AppendLine("【ユーザーの追加指定・返答案】");
                AppendBlock(builder, context.AdditionalInstruction, 1000);
                builder.AppendLine("単語や短いキーワードの場合は返答の感情や内容へ反映してください。文章の場合は意図と意味を保ち、キャラクター設定に沿った自然なヒロインの返答へ添削してください。");
            }
            foreach (string response in (context.ExistingResponses ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)).Take(10))
                Append(builder, "既存返答（重複禁止）", response, 200);
            builder.AppendLine("選択肢の内容を具体的に受けた自然なヒロインの返答だけを、異なる内容で最大3件作成してください。");
            builder.AppendLine("各候補は5～160文字とし、説明や話者名を付けないでください。");
            builder.AppendLine("{\"candidates\":[{\"text\":\"ヒロインの返答\"}]}");
            return builder.ToString().Trim();
        }

        private static void Append(StringBuilder builder, string label, string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            string text = value.Trim().Replace("\r", " ").Replace("\n", " ");
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            builder.AppendLine($"{label}: {text}");
        }

        private static void AppendBlock(StringBuilder builder, string value, int maxLength)
        {
            string text = value.Trim();
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            builder.AppendLine(text);
        }
    }
}
