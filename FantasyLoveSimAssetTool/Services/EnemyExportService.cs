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
    public class EnemyExportService
    {
        private readonly EnemyProjectService enemyProjectService;
        private readonly ImageInspectionService imageInspectionService;

        public string ExportDirectory
        {
            get { return Path.Combine(enemyProjectService.WorkspaceRoot, "Export", "Enemies"); }
        }

        public EnemyExportService(EnemyProjectService enemyProjectService)
            : this(enemyProjectService, new ImageInspectionService())
        {
        }

        public EnemyExportService(EnemyProjectService enemyProjectService, ImageInspectionService imageInspectionService)
        {
            this.enemyProjectService = enemyProjectService ?? throw new ArgumentNullException(nameof(enemyProjectService));
            this.imageInspectionService = imageInspectionService ?? throw new ArgumentNullException(nameof(imageInspectionService));
        }

        public EnemyExportReport ExportEnemy(EnemyProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (string.IsNullOrWhiteSpace(profile.EnemyId))
            {
                throw new ArgumentException("EnemyId is required.", nameof(profile));
            }

            string enemyExportDirectory = Path.Combine(ExportDirectory, profile.EnemyId);
            EnsureExportDirectories(enemyExportDirectory);

            IReadOnlyList<EnemyAsset> acceptedAssets = (profile.Assets ?? new ObservableCollection<EnemyAsset>())
                .Where(asset => asset.Status == AssetStatus.Accepted)
                .ToList();

            EnemyExportReport report = new EnemyExportReport
            {
                ExportPath = enemyExportDirectory,
                AcceptedAssetCount = acceptedAssets.Count
            };

            foreach (EnemyAsset asset in acceptedAssets)
            {
                if (ExportImage(profile, asset, enemyExportDirectory, report))
                {
                    report.ExportedImageCount++;
                }
                else
                {
                    report.SkippedImageCount++;
                }

                if (ExportPrompt(profile, asset, enemyExportDirectory, report))
                {
                    report.ExportedPromptCount++;
                }
            }

            WriteDataFiles(profile, acceptedAssets, enemyExportDirectory);
            return report;
        }

        private static void EnsureExportDirectories(string enemyExportDirectory)
        {
            Directory.CreateDirectory(enemyExportDirectory);
            Directory.CreateDirectory(Path.Combine(enemyExportDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(enemyExportDirectory, "Images", AssetUsage.Battle.ToString()));
            Directory.CreateDirectory(Path.Combine(enemyExportDirectory, "Data"));
            Directory.CreateDirectory(Path.Combine(enemyExportDirectory, "Prompts"));
        }

        private bool ExportImage(EnemyProfile profile, EnemyAsset asset, string enemyExportDirectory, EnemyExportReport report)
        {
            if (string.IsNullOrWhiteSpace(asset.StoredPath))
            {
                report.Warnings.Add($"{asset.AssetId}: StoredPath が空のため画像を export できません。");
                return false;
            }

            string sourcePath = Path.Combine(enemyProjectService.GetEnemyDirectory(profile.EnemyId), asset.StoredPath);
            if (!File.Exists(sourcePath))
            {
                report.Warnings.Add($"{asset.AssetId}: 画像ファイルが見つかりません: {sourcePath}");
                return false;
            }

            AddImageInspectionWarnings(asset, sourcePath, report);

            string fileName = GetExportFileName(asset, sourcePath);
            string destinationDirectory = Path.Combine(enemyExportDirectory, "Images", AssetUsage.Battle.ToString());
            Directory.CreateDirectory(destinationDirectory);
            File.Copy(sourcePath, Path.Combine(destinationDirectory, fileName), true);
            return true;
        }

        private void AddImageInspectionWarnings(EnemyAsset asset, string sourcePath, EnemyExportReport report)
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

        private bool ExportPrompt(EnemyProfile profile, EnemyAsset asset, string enemyExportDirectory, EnemyExportReport report)
        {
            if (string.IsNullOrWhiteSpace(asset.PromptRecordPath))
            {
                report.Warnings.Add($"{asset.AssetId}: PromptRecordPath が空のため prompt JSON を export できません。");
                return false;
            }

            string sourcePath = Path.Combine(enemyProjectService.GetEnemyDirectory(profile.EnemyId), asset.PromptRecordPath);
            if (!File.Exists(sourcePath))
            {
                report.Warnings.Add($"{asset.AssetId}: prompt JSON が見つかりません: {sourcePath}");
                return false;
            }

            string destinationPath = Path.Combine(enemyExportDirectory, "Prompts", Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destinationPath, true);
            return true;
        }

        private static void WriteDataFiles(EnemyProfile profile, IReadOnlyList<EnemyAsset> acceptedAssets, string enemyExportDirectory)
        {
            string dataDirectory = Path.Combine(enemyExportDirectory, "Data");
            File.WriteAllText(Path.Combine(dataDirectory, "enemy_profile_export.json"), BuildProfileExportJson(profile));
            File.WriteAllText(Path.Combine(dataDirectory, "enemy_assets_export.json"), BuildAssetsExportJson(profile, acceptedAssets));
        }

        private static string BuildProfileExportJson(EnemyProfile profile)
        {
            object exportModel = new
            {
                schemaVersion = 1,
                enemyId = profile.EnemyId,
                displayName = profile.DisplayName,
                enemyType = profile.EnemyType,
                memo = profile.Memo
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string BuildAssetsExportJson(EnemyProfile profile, IReadOnlyList<EnemyAsset> acceptedAssets)
        {
            object exportModel = new
            {
                schemaVersion = 1,
                enemyId = profile.EnemyId,
                unityImageRoot = $"Assets/Images/Enemies/{profile.EnemyId}",
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
                        "Enemies",
                        profile.EnemyId,
                        AssetUsage.Battle.ToString(),
                        GetExportFileName(asset, string.Empty))
                }).ToList()
            };

            return JsonSerializer.Serialize(exportModel, CreateJsonOptions());
        }

        private static string GetExportFileName(EnemyAsset asset, string fallbackPath)
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
