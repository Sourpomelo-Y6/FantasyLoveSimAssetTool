using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FantasyLoveSimAssetTool.Services
{
    public class PlayerExportService
    {
        private readonly PlayerProjectService playerProjectService;
        private readonly ImageInspectionService imageInspectionService;

        public string ExportDirectory
        {
            get { return Path.Combine(playerProjectService.WorkspaceRoot, "Export", "Player"); }
        }

        public PlayerExportService(PlayerProjectService playerProjectService)
            : this(playerProjectService, new ImageInspectionService())
        {
        }

        public PlayerExportService(PlayerProjectService playerProjectService, ImageInspectionService imageInspectionService)
        {
            this.playerProjectService = playerProjectService ?? throw new ArgumentNullException(nameof(playerProjectService));
            this.imageInspectionService = imageInspectionService ?? throw new ArgumentNullException(nameof(imageInspectionService));
        }

        public PlayerExportReport ExportPlayer(PlayerProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            EnsureExportDirectories();

            IReadOnlyList<PlayerAsset> acceptedAssets = (profile.Assets ?? new ObservableCollection<PlayerAsset>())
                .Where(asset => asset.Status == AssetStatus.Accepted)
                .ToList();

            PlayerExportReport report = new PlayerExportReport
            {
                ExportPath = ExportDirectory,
                AcceptedAssetCount = acceptedAssets.Count
            };

            foreach (PlayerAsset asset in acceptedAssets)
            {
                if (ExportImage(asset, report))
                {
                    report.ExportedImageCount++;
                }
                else
                {
                    report.SkippedImageCount++;
                }

                if (ExportPrompt(asset, report))
                {
                    report.ExportedPromptCount++;
                }
            }

            WriteDataFiles(profile, acceptedAssets);
            return report;
        }

        private void EnsureExportDirectories()
        {
            Directory.CreateDirectory(ExportDirectory);
            Directory.CreateDirectory(Path.Combine(ExportDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(ExportDirectory, "Images", AssetUsage.Battle.ToString()));
            Directory.CreateDirectory(Path.Combine(ExportDirectory, "Data"));
            Directory.CreateDirectory(Path.Combine(ExportDirectory, "Prompts"));
        }

        private bool ExportImage(PlayerAsset asset, PlayerExportReport report)
        {
            if (string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                report.Warnings.Add($"{asset.AssetId}: StoredPath が空のため画像を export できません。");
                return false;
            }

            string sourcePath = Path.Combine(playerProjectService.PlayerDirectory, asset.StoredPath);
            if (!File.Exists(sourcePath))
            {
                report.Warnings.Add($"{asset.AssetId}: 画像ファイルが見つかりません: {sourcePath}");
                return false;
            }

            AddImageInspectionWarnings(asset, sourcePath, report);

            string fileName = GetExportFileName(asset, sourcePath);
            string destinationDirectory = Path.Combine(ExportDirectory, "Images", AssetUsage.Battle.ToString());
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(sourcePath, Path.Combine(destinationDirectory, fileName), true);
            return true;
        }

        private void AddImageInspectionWarnings(PlayerAsset asset, string sourcePath, PlayerExportReport report)
        {
            try
            {
                ImageInspectionResult result = imageInspectionService.Inspect(sourcePath, AssetUsage.Battle);
                foreach (string warning in result.Warnings)
                {
                    report.Warnings.Add($"{asset.AssetId}: {warning}");
                }
            }
            catch (Exception ex)
            {
                report.Warnings.Add($"{asset.AssetId}: 画像検査に失敗しました: {ex.Message}");
            }
        }

        private bool ExportPrompt(PlayerAsset asset, PlayerExportReport report)
        {
            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                report.Warnings.Add($"{asset.AssetId}: PromptRecordPath が空のため prompt JSON を export できません。");
                return false;
            }

            string sourcePath = Path.Combine(playerProjectService.PlayerDirectory, asset.PromptRecordPath);
            if (!File.Exists(sourcePath))
            {
                report.Warnings.Add($"{asset.AssetId}: prompt JSON が見つかりません: {sourcePath}");
                return false;
            }

            string destinationPath = Path.Combine(ExportDirectory, "Prompts", Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destinationPath, true);
            return true;
        }

        private void WriteDataFiles(PlayerProfile profile, IReadOnlyList<PlayerAsset> acceptedAssets)
        {
            string dataDirectory = Path.Combine(ExportDirectory, "Data");
            File.WriteAllText(Path.Combine(dataDirectory, "player_profile_export.json"), BuildProfileExportJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "player_assets_export.json"), BuildAssetsExportJson(profile, acceptedAssets));
        }

        private static string BuildProfileExportJson(PlayerProfile profile)
        {
            object exportModel = new
            {
                schemaVersion = 1,
                playerId = profile.PlayerId,
                displayName = profile.DisplayName,
                memo = profile.Memo
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string BuildAssetsExportJson(PlayerProfile profile, IReadOnlyList<PlayerAsset> acceptedAssets)
        {
            object exportModel = new
            {
                schemaVersion = 1,
                playerId = profile.PlayerId,
                unityImageRoot = "Assets/Images/Player",
                assets = acceptedAssets.Select(asset => new
                {
                    assetId = asset.AssetId,
                    usage = AssetUsage.Battle,
                    status = asset.Status,
                    fileName = GetExportFileName(asset, string.Empty),
                    memo = asset.Memo,
                    exportImagePath = ToExportRelativePath("Images", AssetUsage.Battle.ToString(), GetExportFileName(asset, string.Empty)),
                    exportPromptPath = string.IsNullOrWhiteSpace(asset.PromptRecordPath)
                        ? string.Empty
                        : ToExportRelativePath("Prompts", Path.GetFileName(asset.PromptRecordPath)),
                    unityImagePath = ToExportRelativePath(
                        "Assets",
                        "Images",
                        "Player",
                        AssetUsage.Battle.ToString(),
                        GetExportFileName(asset, string.Empty))
                }).ToList()
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string GetExportFileName(PlayerAsset asset, string fallbackPath)
        {
            if (!string.IsNullOrWhiteSpace(asset.FileName))
            {
                return asset.FileName;
            }

            if (!string.IsNullOrWhiteSpace(fallbackPath))
            {
                return Path.GetFileName(fallbackPath);
            }

            return asset.AssetId + ".png";
        }

        private static string ToExportRelativePath(params string[] parts)
        {
            return string.Join("/", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static JsonSerializerOptions CreateJsonOptions()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
