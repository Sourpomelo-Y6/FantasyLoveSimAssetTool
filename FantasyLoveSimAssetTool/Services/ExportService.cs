using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FantasyLoveSimAssetTool.Services
{
    public class ExportService
    {
        private readonly CharacterProjectService characterProjectService;

        public string ExportDirectory
        {
            get { return Path.Combine(characterProjectService.WorkspaceRoot, "Export"); }
        }

        public ExportService(CharacterProjectService characterProjectService)
        {
            this.characterProjectService = characterProjectService ?? throw new ArgumentNullException(nameof(characterProjectService));
        }

        public string ExportHeroine(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (string.IsNullOrWhiteSpace(profile.HeroineId))
            {
                throw new ArgumentException("HeroineId is required.", nameof(profile));
            }

            string heroineExportDirectory = Path.Combine(ExportDirectory, profile.HeroineId);
            EnsureExportDirectories(heroineExportDirectory);

            IReadOnlyList<HeroineAsset> acceptedAssets = (profile.Assets ?? new System.Collections.ObjectModel.ObservableCollection<HeroineAsset>())
                .Where(asset => asset.Status == AssetStatus.Accepted)
                .ToList();

            foreach (HeroineAsset asset in acceptedAssets)
            {
                ExportImage(profile, asset, heroineExportDirectory);
                ExportPrompt(profile, asset, heroineExportDirectory);
            }

            WriteDataFiles(profile, acceptedAssets, heroineExportDirectory);

            return heroineExportDirectory;
        }

        private void EnsureExportDirectories(string heroineExportDirectory)
        {
            Directory.CreateDirectory(heroineExportDirectory);
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Sprites"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Event"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Actions"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Images", "Ending"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Data"));
            Directory.CreateDirectory(Path.Combine(heroineExportDirectory, "Prompts"));
        }

        private void ExportImage(HeroineProfile profile, HeroineAsset asset, string heroineExportDirectory)
        {
            if (string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                return;
            }

            string sourcePath = Path.Combine(characterProjectService.GetCharacterDirectory(profile.HeroineId), asset.StoredPath);
            if (!File.Exists(sourcePath))
            {
                return;
            }

            string fileName = string.IsNullOrWhiteSpace(asset.FileName) ? Path.GetFileName(sourcePath) : asset.FileName;
            string destinationDirectory = Path.Combine(heroineExportDirectory, "Images", asset.Usage.ToString());
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(sourcePath, Path.Combine(destinationDirectory, fileName), true);
        }

        private void ExportPrompt(HeroineProfile profile, HeroineAsset asset, string heroineExportDirectory)
        {
            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                return;
            }

            string sourcePath = Path.Combine(characterProjectService.GetCharacterDirectory(profile.HeroineId), asset.PromptRecordPath);
            if (!File.Exists(sourcePath))
            {
                return;
            }

            string destinationPath = Path.Combine(heroineExportDirectory, "Prompts", Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destinationPath, true);
        }

        private void WriteDataFiles(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets, string heroineExportDirectory)
        {
            string dataDirectory = Path.Combine(heroineExportDirectory, "Data");
            File.WriteAllText(Path.Combine(dataDirectory, "heroine_profile_note.md"), BuildProfileNote(profile, acceptedAssets));
            WriteDraftFile(Path.Combine(dataDirectory, "conversations_draft.md"), "会話案");
            WriteDraftFile(Path.Combine(dataDirectory, "game_events_draft.md"), "イベント案");
            WriteDraftFile(Path.Combine(dataDirectory, "action_reactions_draft.md"), "行動反応案");
            WriteDraftFile(Path.Combine(dataDirectory, "endings_draft.md"), "エンディング案");
        }

        private static string BuildProfileNote(HeroineProfile profile, IReadOnlyList<HeroineAsset> acceptedAssets)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Heroine Profile Note");
            builder.AppendLine();
            AppendValue(builder, "HeroineId", profile.HeroineId);
            AppendValue(builder, "表示名", profile.DisplayName);
            AppendValue(builder, "年齢", profile.Age);
            AppendValue(builder, "身長", profile.Height);
            builder.AppendLine();
            AppendSection(builder, "性格", profile.Personality);
            AppendSection(builder, "口調", profile.SpeakingStyle);
            AppendValue(builder, "一人称", profile.FirstPerson);
            AppendValue(builder, "二人称", profile.SecondPerson);
            builder.AppendLine();
            AppendSection(builder, "好きなもの", profile.Likes);
            AppendSection(builder, "苦手なもの", profile.Dislikes);
            AppendSection(builder, "容姿プロンプト", profile.AppearancePrompt);
            AppendSection(builder, "行動反応方針", profile.ActionReactionPolicy);
            AppendSection(builder, "エンディング方針", profile.EndingPolicy);

            builder.AppendLine("## Accepted Assets");
            builder.AppendLine();
            if (acceptedAssets.Count == 0)
            {
                builder.AppendLine("- なし");
            }
            else
            {
                foreach (HeroineAsset asset in acceptedAssets)
                {
                    builder.AppendLine($"- `{asset.AssetId}` / `{asset.Usage}` / `{asset.FileName}`");
                }
            }

            return builder.ToString();
        }

        private static void WriteDraftFile(string path, string title)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# " + title);
            builder.AppendLine();
            builder.AppendLine("## Draft");
            builder.AppendLine();
            File.WriteAllText(path, builder.ToString());
        }

        private static void AppendValue(StringBuilder builder, string label, string value)
        {
            builder.AppendLine($"- {label}: {value ?? string.Empty}");
        }

        private static void AppendSection(StringBuilder builder, string title, string value)
        {
            builder.AppendLine("## " + title);
            builder.AppendLine();
            builder.AppendLine(value ?? string.Empty);
            builder.AppendLine();
        }
    }
}
