using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    /// <summary>
    /// ビルド出力とは独立した作業フォルダーと、その移行を管理します。
    /// </summary>
    public sealed class WorkspacePathService
    {
        private static readonly string[] DataDirectories =
        {
            "Characters", "Enemies", "Player", "Definitions", "PromptTemplates", "ComfySettings"
        };

        private readonly string settingsPath;
        private readonly string defaultWorkspaceRoot;

        public WorkspacePathService()
            : this(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FantasyLoveSimAssetTool", "workspace-settings.json"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "FantasyLoveSimAssetToolWorkspace"))
        {
        }

        public WorkspacePathService(string settingsPath, string defaultWorkspaceRoot)
        {
            this.settingsPath = settingsPath ?? throw new ArgumentNullException(nameof(settingsPath));
            this.defaultWorkspaceRoot = defaultWorkspaceRoot ?? throw new ArgumentNullException(nameof(defaultWorkspaceRoot));
        }

        public string ResolveWorkspaceRoot()
        {
            string configured = LoadConfiguredWorkspaceRoot();
            string result = string.IsNullOrWhiteSpace(configured) ? defaultWorkspaceRoot : configured;
            result = Path.GetFullPath(result);
            Directory.CreateDirectory(result);
            return result;
        }

        public void SaveWorkspaceRoot(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
            {
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));
            }

            string normalized = Path.GetFullPath(workspaceRoot);
            Directory.CreateDirectory(normalized);
            string settingsDirectory = Path.GetDirectoryName(settingsPath);
            if (!string.IsNullOrWhiteSpace(settingsDirectory)) Directory.CreateDirectory(settingsDirectory);
            string json = JsonSerializer.Serialize(new WorkspaceSettings { WorkspaceRoot = normalized },
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
        }

        public string FindLegacyWorkspace(string destinationRoot)
        {
            IEnumerable<string> candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory }
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return candidates.FirstOrDefault(path =>
                !PathsEqual(path, destinationRoot) &&
                Directory.Exists(Path.Combine(path, "Characters")) &&
                Directory.EnumerateFiles(Path.Combine(path, "Characters"), "profile.json", SearchOption.AllDirectories).Any());
        }

        public WorkspaceMigrationResult Migrate(string sourceRoot, string destinationRoot)
        {
            sourceRoot = Path.GetFullPath(sourceRoot);
            destinationRoot = Path.GetFullPath(destinationRoot);
            if (PathsEqual(sourceRoot, destinationRoot))
            {
                throw new InvalidOperationException("Source and destination workspaces are the same.");
            }

            Directory.CreateDirectory(destinationRoot);
            string backupRoot = Path.Combine(destinationRoot, "Backups",
                "WorkspaceMigration_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff"));
            int copiedFiles = 0;
            int backedUpFiles = 0;

            foreach (string directoryName in DataDirectories)
            {
                string source = Path.Combine(sourceRoot, directoryName);
                if (!Directory.Exists(source)) continue;

                foreach (string sourceFile in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    string relative = sourceFile.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string destinationFile = Path.Combine(destinationRoot, relative);
                    if (File.Exists(destinationFile))
                    {
                        string backupFile = Path.Combine(backupRoot, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
                        File.Copy(destinationFile, backupFile, true);
                        backedUpFiles++;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    File.Copy(sourceFile, destinationFile, true);
                    copiedFiles++;
                }
            }

            return new WorkspaceMigrationResult(copiedFiles, backedUpFiles,
                backedUpFiles > 0 ? backupRoot : string.Empty);
        }

        public void SeedBundledDefaults(string bundledRoot, string destinationRoot)
        {
            foreach (string directoryName in new[] { "Definitions", "PromptTemplates", "ComfySettings" })
            {
                string source = Path.Combine(bundledRoot, directoryName);
                if (!Directory.Exists(source)) continue;
                foreach (string sourceFile in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
                {
                    string relative = sourceFile.Substring(source.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string destinationFile = Path.Combine(destinationRoot, directoryName, relative);
                    if (File.Exists(destinationFile)) continue;
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    File.Copy(sourceFile, destinationFile);
                }
            }

            string bundledCharacters = Path.Combine(bundledRoot, "Characters");
            if (!Directory.Exists(bundledCharacters)) return;
            foreach (string sourceFile in Directory.EnumerateFiles(
                bundledCharacters, "conversation-ai-prompt.json", SearchOption.AllDirectories))
            {
                string relative = sourceFile.Substring(bundledCharacters.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destinationFile = Path.Combine(destinationRoot, "Characters", relative);
                if (File.Exists(destinationFile)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                File.Copy(sourceFile, destinationFile);
            }
        }

        public static bool IsBuildOutputPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            string normalized = Path.GetFullPath(path).Replace('\\', '/');
            return normalized.IndexOf("/bin/Debug/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                normalized.IndexOf("/bin/Release/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string LoadConfiguredWorkspaceRoot()
        {
            if (!File.Exists(settingsPath)) return string.Empty;
            try
            {
                WorkspaceSettings settings = JsonSerializer.Deserialize<WorkspaceSettings>(File.ReadAllText(settingsPath));
                return settings?.WorkspaceRoot ?? string.Empty;
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }

        private static bool PathsEqual(string left, string right) =>
            string.Equals(Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);

        private sealed class WorkspaceSettings
        {
            public int SchemaVersion { get; set; } = 1;
            public string WorkspaceRoot { get; set; }
        }
    }

    public sealed class WorkspaceMigrationResult
    {
        public WorkspaceMigrationResult(int copiedFiles, int backedUpFiles, string backupPath)
        {
            CopiedFiles = copiedFiles;
            BackedUpFiles = backedUpFiles;
            BackupPath = backupPath;
        }

        public int CopiedFiles { get; }
        public int BackedUpFiles { get; }
        public string BackupPath { get; }
    }
}
