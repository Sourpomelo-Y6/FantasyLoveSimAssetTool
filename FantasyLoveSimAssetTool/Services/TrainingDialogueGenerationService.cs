using FantasyLoveSimAssetTool.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class TrainingDialogueGenerationService
    {
        private readonly ILocalLlmClient llmClient;

        public TrainingDialogueGenerationService(ILocalLlmClient llmClient)
        {
            this.llmClient = llmClient ?? throw new ArgumentNullException(nameof(llmClient));
        }

        public async Task<TrainingDialogueGenerationResult> GenerateAsync(
            HeroineProfile profile, LocalAiSettings settings, string baseInstruction,
            TrainingDialogueGenerationContext context, CancellationToken cancellationToken = default)
        {
            string prompt = BuildPrompt(profile, context);
            LocalLlmTestResult response = await llmClient.GenerateAsync(
                settings.ServerUrl, settings.ModelId, baseInstruction, prompt,
                settings.Temperature, settings.MaxTokens, settings.TimeoutSeconds, cancellationToken);
            try
            {
                return new TrainingDialogueGenerationResult
                {
                    Candidates = ShortTextGenerationService.ParseCandidates(response.Content).Take(3).ToList(),
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId
                };
            }
            catch (InvalidOperationException ex)
            {
                return new TrainingDialogueGenerationResult
                {
                    Prompt = prompt,
                    RawResponse = response.Content,
                    ModelId = response.ModelId,
                    ParseError = ex.Message
                };
            }
        }

        public static string BuildPrompt(HeroineProfile profile, TrainingDialogueGenerationContext context)
        {
            if (profile == null) throw new InvalidOperationException("ヒロインを選択してください。");
            if (context == null || string.IsNullOrWhiteSpace(context.TrainingId) ||
                string.IsNullOrWhiteSpace(context.VisualState))
                throw new InvalidOperationException("訓練画像と表示状態を選択してください。");
            var builder = new StringBuilder();
            builder.AppendLine("訓練中の状況に対応するヒロイン本人の短い台詞を、異なる内容で最大3件作成してください。");
            Append(builder, "名前", profile.DisplayName, 40);
            Append(builder, "性格", profile.Personality, 200);
            Append(builder, "口調", profile.SpeakingStyle, 200);
            Append(builder, "一人称", profile.FirstPerson, 40);
            Append(builder, "二人称", profile.SecondPerson, 40);
            Append(builder, "訓練ID", context.TrainingId, 100);
            Append(builder, "訓練名", context.TrainingDisplayName, 100);
            Append(builder, "カテゴリー", context.TrainingCategory, 100);
            Append(builder, "表示状態", FormatVisualState(context.VisualState), 100);
            builder.AppendLine($"画面内の人物: 主人公={(context.PlayerVisible ? "表示" : "非表示")}、ヒロイン={(context.HeroineVisible ? "表示" : "非表示")}");
            if (!string.IsNullOrWhiteSpace(context.AdditionalInstruction))
            {
                builder.AppendLine("【ユーザーの追加指定・入力案】");
                AppendBlock(builder, context.AdditionalInstruction, 1000);
                builder.AppendLine("キーワードは台詞へ反映し、文章は意味を保ってキャラクターらしい自然な台詞へ添削してください。");
            }
            foreach (string message in (context.ExistingMessages ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)).Take(10))
                Append(builder, "既存候補（重複禁止）", message, 160);
            builder.AppendLine("各候補は5～100文字とし、ヒロイン以外の台詞、話者名、Voice ID、説明を含めないでください。");
            builder.AppendLine("表示状態とLP消費の主体を取り違えないでください。");
            builder.AppendLine("{\"candidates\":[{\"text\":\"ヒロインの台詞\"}]}");
            return builder.ToString().Trim();
        }

        public static string FormatVisualState(string state)
        {
            switch (state)
            {
                case "SelectedBeforeFirstStep": return "訓練開始前";
                case "SelectedAfterFirstStep": return "訓練実行後・継続中";
                case "PlayerLpConsumed": return "主人公がLPを消費して疲労";
                case "HeroineLpConsumed": return "ヒロインがLPを消費して疲労";
                case "SimultaneousLpConsumed": return "主人公とヒロインが同時にLPを消費して疲労";
                default: return state ?? string.Empty;
            }
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
