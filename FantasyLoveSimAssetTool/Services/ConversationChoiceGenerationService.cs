using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class ConversationChoiceGenerationService
    {
        private readonly ILocalLlmClient llmClient;

        public ConversationChoiceGenerationService(ILocalLlmClient llmClient)
        {
            this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        public async Task<ConversationChoiceGenerationResult> GenerateAsync(
            LocalAiSettings settings, string baseInstruction, ConversationChoiceGenerationContext context,
            CancellationToken cancellationToken = default)
        {
            string prompt = BuildPrompt(context);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, BuildSystemInstruction(baseInstruction), prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);
            try
            {
                return new ConversationChoiceGenerationResult
                {
                    Candidates = ShortTextGenerationService.ParseCandidates(response.Content).Take(3).ToList(),
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId
                };
            }
            catch (InvalidOperationException ex)
            {
                return new ConversationChoiceGenerationResult
                {
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId,
                    ParseError = ex.Message
                };
            }
        }

        public static string BuildPrompt(ConversationChoiceGenerationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(context.PreviousLine))
                throw new InvalidOperationException("選択肢の直前にあたる台詞を入力してください。");
            var builder = new StringBuilder();
            builder.AppendLine("【最優先の役割】");
            builder.AppendLine("生成するのはヒロインの台詞ではなく、プレイヤーが選んでヒロインへ伝える発言または行動です。");
            builder.AppendLine("ヒロイン本人の口調、一人称、感情で話させないでください。ヒロインの返答も生成しないでください。");
            builder.AppendLine("プレイヤー視点の自然で短い選択肢にしてください。");
            if (!string.IsNullOrWhiteSpace(context.CharacterPrompt))
            {
                builder.AppendLine("【会話相手であるヒロインの参考設定】");
                builder.AppendLine("以下は会話相手を理解するためだけに使い、生成文の話者や口調へ適用しないでください。");
                AppendBlock(builder, context.CharacterPrompt, 3000);
            }
            builder.AppendLine("【選択肢の生成対象】");
            Append(builder, "会話種別", context.ConversationKind, 40);
            Append(builder, "会話ID", context.ConversationEntryId, 100);
            Append(builder, "カテゴリ", context.ConversationCategory, 100);
            Append(builder, "直前の台詞", context.PreviousLine, 400);
            Append(builder, "方向性", context.Direction, 40);
            if (!string.IsNullOrWhiteSpace(context.AdditionalInstruction))
            {
                builder.AppendLine("【ユーザーの追加指定・入力案】");
                AppendBlock(builder, context.AdditionalInstruction, 1000);
                builder.AppendLine("単語や短いキーワードの場合は候補の内容へ反映してください。文章の場合は意図と意味を保ち、プレイヤーの自然な選択肢文へ添削してください。");
            }
            foreach (string choice in (context.ExistingChoices ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)).Take(10))
                Append(builder, "既存選択肢（重複禁止）", choice, 100);
            builder.AppendLine("プレイヤーが実際に選ぶ短い発言または行動の表示文だけを、異なる内容で最大3件作成してください。");
            builder.AppendLine("各候補の主語と発話者はプレイヤーです。ヒロイン側の返答文、相づち、独白は候補に含めないでください。");
            builder.AppendLine("各候補は2～40文字とし、返答、好感度、遷移先、条件、ID、分岐構造は生成・変更しないでください。");
            builder.AppendLine("{\"candidates\":[{\"text\":\"選択肢\"}]}");
            return builder.ToString().Trim();
        }

        public static string BuildSystemInstruction(string baseInstruction)
        {
            var builder = new StringBuilder((baseInstruction ?? string.Empty).Trim());
            if (builder.Length > 0) builder.AppendLine();
            builder.AppendLine("今回の生成対象はヒロインの台詞ではなく、プレイヤーが選ぶ発言または行動です。");
            builder.AppendLine("キャラクターの口調指定は会話相手の理解だけに使い、出力文には適用しないでください。");
            builder.Append("ヒロインの発言、返答、独白は出力せず、指定されたJSONだけを出力してください。");
            return builder.ToString();
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
            if (string.IsNullOrWhiteSpace(value)) return;
            string text = value.Trim();
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            builder.AppendLine(text);
        }
    }
}
