using FantasyLoveSimAssetTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace FantasyLoveSimAssetTool.Services
{
    public class StillDefinitionService
    {
        private readonly IReadOnlyList<StillDefinition> defaultDefinitions;

        public StillDefinitionService()
        {
            defaultDefinitions = new List<StillDefinition>
            {
                Create("Heroine_Normal", "立ち絵: 通常", AssetUsage.Sprites, "Heroine_Normal.png", "standing character sprite, full body, neutral expression, transparent background"),
                Create("Heroine_Smile", "立ち絵: 笑顔", AssetUsage.Sprites, "Heroine_Smile.png", "standing character sprite, full body, gentle smile, transparent background"),
                Create("Heroine_Spring", "立ち絵: 春服", AssetUsage.Sprites, "Heroine_Spring.png", "standing character sprite, full body, spring outfit, transparent background"),
                Create("Heroine_Summer", "立ち絵: 夏服", AssetUsage.Sprites, "Heroine_Summer.png", "standing character sprite, full body, summer outfit, transparent background"),
                Create("Heroine_Autumn", "立ち絵: 秋服", AssetUsage.Sprites, "Heroine_Autumn.png", "standing character sprite, full body, autumn outfit, transparent background"),
                Create("Heroine_Winter", "立ち絵: 冬服", AssetUsage.Sprites, "Heroine_Winter.png", "standing character sprite, full body, winter outfit, transparent background"),
                Create("Heroine_Dress", "立ち絵: ドレス", AssetUsage.Sprites, "Heroine_Dress.png", "standing character sprite, full body, elegant dress, transparent background"),
                Create("Heroine_NightDress", "立ち絵: ナイトドレス", AssetUsage.Sprites, "Heroine_NightDress.png", "standing character sprite, full body, night dress, transparent background"),
                Create("Heroine_Raincoat", "立ち絵: レインコート", AssetUsage.Sprites, "Heroine_Raincoat.png", "standing character sprite, full body, raincoat, transparent background"),

                Create("GameStartIntro_01", "イベント: 導入", AssetUsage.Event, "GameStartIntro_01.png", "visual novel event still, first meeting scene, warm light, cinematic composition"),
                Create("DayStart_Routine_01", "イベント: 日常開始", AssetUsage.Event, "DayStart_Routine_01.png", "visual novel event still, calm morning routine, relaxed atmosphere, detailed room"),
                Create("DayStart_Rainy_01", "イベント: 雨の日開始", AssetUsage.Event, "DayStart_Rainy_01.png", "visual novel event still, rainy morning, soft window light, quiet mood"),
                Create("WithForest_01", "イベント: 森", AssetUsage.Event, "WithForest_01.png", "visual novel event still, walking together in a forest, dappled sunlight, romantic mood"),
                Create("WithLake_01", "イベント: 湖", AssetUsage.Event, "WithLake_01.png", "visual novel event still, lakeside scene, clear water, gentle breeze, romantic mood"),
                Create("WithCave_01", "イベント: 洞窟", AssetUsage.Event, "WithCave_01.png", "visual novel event still, cave exploration scene, magical light, adventurous mood"),

                Create("Tea_01", "行動: お茶", AssetUsage.Actions, "Tea_01.png", "visual novel event still, drinking tea together, cozy room, warm lighting"),
                Create("Rest_01", "行動: 休憩", AssetUsage.Actions, "Rest_01.png", "visual novel event still, resting together, relaxed pose, peaceful atmosphere"),
                Create("Walk_01", "行動: 散歩", AssetUsage.Actions, "Walk_01.png", "visual novel event still, walking outdoors together, natural sunlight, peaceful path"),
                Create("Gift_01", "行動: 贈り物", AssetUsage.Actions, "Gift_01.png", "visual novel event still, receiving a gift, surprised happy expression, intimate composition"),

                Create("GoodEnding_01", "エンディング: Good", AssetUsage.Ending, "GoodEnding_01.png", "good ending still, emotional smile, hopeful atmosphere, beautiful lighting"),
                Create("NormalEnding_01", "エンディング: Normal", AssetUsage.Ending, "NormalEnding_01.png", "normal ending still, bittersweet smile, calm atmosphere, soft lighting"),
                Create("BadEnding_01", "エンディング: Bad", AssetUsage.Ending, "BadEnding_01.png", "bad ending still, distant expression, lonely atmosphere, subdued lighting")
            };
        }

        public IReadOnlyList<StillDefinition> GetDefaultDefinitions()
        {
            return defaultDefinitions.Select(Clone).ToList();
        }

        private static StillDefinition Create(string assetId, string displayName, AssetUsage usage, string fileName, string specificPrompt)
        {
            return new StillDefinition
            {
                AssetId = assetId,
                DisplayName = displayName,
                Usage = usage,
                FileName = fileName,
                SpecificPrompt = specificPrompt,
                Status = StillStatus.NotGenerated
            };
        }

        private static StillDefinition Clone(StillDefinition source)
        {
            return new StillDefinition
            {
                AssetId = source.AssetId,
                DisplayName = source.DisplayName,
                Usage = source.Usage,
                FileName = source.FileName,
                SpecificPrompt = source.SpecificPrompt,
                Status = source.Status
            };
        }
    }
}
