using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Services
{
    public class PromptTemplateService
    {
        public const string CharacterAppearancePromptPlaceholder = "{CharacterAppearancePrompt}";

        private const string TemplateDirectoryName = "PromptTemplates";
        private const string TemplateFileName = "templates.json";

        private readonly IReadOnlyList<PromptTemplate> templates;

        public PromptTemplateService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public PromptTemplateService(string workspaceRoot)
        {
            templates = LoadTemplates(workspaceRoot);
        }

        public IReadOnlyList<PromptTemplate> GetTemplates(AssetUsage usage)
        {
            return templates
                .Where(template => template.Usage == usage)
                .ToList();
        }

        public string BuildPositivePrompt(HeroineProfile profile, PromptTemplate template)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            string appearancePrompt = profile.AppearancePrompt ?? string.Empty;
            return template.TemplateText.Replace(CharacterAppearancePromptPlaceholder, appearancePrompt);
        }

        private static IReadOnlyList<PromptTemplate> LoadTemplates(string workspaceRoot)
        {
            string templatePath = Path.Combine(workspaceRoot, TemplateDirectoryName, TemplateFileName);
            if (!File.Exists(templatePath))
            {
                return CreateDefaultTemplates();
            }

            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new JsonStringEnumConverter());

                List<PromptTemplate> loadedTemplates = JsonSerializer.Deserialize<List<PromptTemplate>>(
                    File.ReadAllText(templatePath),
                    options);

                if (loadedTemplates == null || loadedTemplates.Count == 0)
                {
                    return CreateDefaultTemplates();
                }

                List<PromptTemplate> validTemplates = loadedTemplates
                    .Where(IsValidTemplate)
                    .ToList();
                return validTemplates.Count == 0
                    ? CreateDefaultTemplates()
                    : validTemplates;
            }
            catch
            {
                return CreateDefaultTemplates();
            }
        }

        private static bool IsValidTemplate(PromptTemplate template)
        {
            return template != null
                && !string.IsNullOrWhiteSpace(template.TemplateId)
                && !string.IsNullOrWhiteSpace(template.DisplayName)
                && !string.IsNullOrWhiteSpace(template.TemplateText);
        }

        private static IReadOnlyList<PromptTemplate> CreateDefaultTemplates()
        {
            return new List<PromptTemplate>
            {
                new PromptTemplate
                {
                    TemplateId = "sprites_normal",
                    DisplayName = "立ち絵: 通常",
                    Usage = AssetUsage.Sprites,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", standing character sprite, full body, clean line art, transparent background, soft anime style"
                },
                new PromptTemplate
                {
                    TemplateId = "sprites_smile",
                    DisplayName = "立ち絵: 笑顔",
                    Usage = AssetUsage.Sprites,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", standing character sprite, full body, gentle smile, transparent background, soft anime style"
                },
                new PromptTemplate
                {
                    TemplateId = "event_intro",
                    DisplayName = "イベント: 導入",
                    Usage = AssetUsage.Event,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", romantic event still, first meeting scene, warm light, cinematic composition, detailed background"
                },
                new PromptTemplate
                {
                    TemplateId = "event_daily",
                    DisplayName = "イベント: 日常",
                    Usage = AssetUsage.Event,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", daily life event still, relaxed atmosphere, natural pose, detailed environment, soft anime style"
                },
                new PromptTemplate
                {
                    TemplateId = "actions_tea",
                    DisplayName = "行動: お茶",
                    Usage = AssetUsage.Actions,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", drinking tea with the player, cozy room, gentle atmosphere, warm lighting, visual novel event still"
                },
                new PromptTemplate
                {
                    TemplateId = "actions_walk",
                    DisplayName = "行動: 散歩",
                    Usage = AssetUsage.Actions,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", walking outdoors with the player, peaceful path, natural sunlight, visual novel event still"
                },
                new PromptTemplate
                {
                    TemplateId = "ending_good",
                    DisplayName = "エンディング: Good",
                    Usage = AssetUsage.Ending,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", good ending still, emotional smile, hopeful atmosphere, cinematic composition, beautiful lighting"
                },
                new PromptTemplate
                {
                    TemplateId = "ending_normal",
                    DisplayName = "エンディング: Normal",
                    Usage = AssetUsage.Ending,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", normal ending still, bittersweet smile, calm atmosphere, cinematic composition, soft lighting"
                },
                new PromptTemplate
                {
                    TemplateId = "battle_heroine_idle",
                    DisplayName = "戦闘: ヒロイン通常",
                    Usage = AssetUsage.Battle,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", battle UI character sprite, idle pose, full body, transparent background, clear game asset lighting"
                },
                new PromptTemplate
                {
                    TemplateId = "battle_heroine_attack",
                    DisplayName = "戦闘: ヒロイン攻撃",
                    Usage = AssetUsage.Battle,
                    TemplateText = CharacterAppearancePromptPlaceholder + ", battle UI character sprite, attack pose, full body, transparent background, clear game asset lighting"
                }
            };
        }
    }
}
