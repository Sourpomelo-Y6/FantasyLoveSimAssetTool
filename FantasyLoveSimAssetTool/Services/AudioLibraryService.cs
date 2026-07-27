using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class AudioLibraryScanResult
    {
        public List<AudioLibraryItem> Items { get; } = new List<AudioLibraryItem>();
        public int AvailableCount => Items.Count(item => item.IsAvailable);
        public int MissingCount => Items.Count(item => !item.IsAvailable);
        public int BgmCount => Items.Count(item => item.Category == "BGM");
        public int SeCount => Items.Count(item => item.Category == "SE");
        public int VoiceCount => Items.Count(item => item.Category == "VOICE");

        public string CreateSummary()
        {
            return $"音声 {Items.Count} 件 / 導入済み {AvailableCount} / 未配置 {MissingCount} " +
                $"/ BGM {BgmCount} / SE {SeCount} / VOICE {VoiceCount}";
        }
    }

    public sealed class AudioLibraryService
    {
        private static readonly string[] SupportedExtensions =
        {
            ".wav", ".mp3", ".ogg", ".aif", ".aiff"
        };

        private static readonly string[] BgmIds =
        {
            "Title", "Main", "Ending", "Battle", "Training"
        };

        private static readonly string[] SeIds =
        {
            "UI/Confirm", "UI/Cancel", "UI/Next", "UI/Error",
            "Shop/PurchaseSuccess", "Shop/PurchaseFailed",
            "Skill/AcquireSuccess", "Skill/AcquireFailed",
            "Schedule/Set", "Schedule/Cancel",
            "Training/Step", "Training/Complete", "Training/Cancel",
            "Battle/Attack", "Battle/Defend", "Battle/Heal", "Battle/Skill",
            "Battle/Item", "Battle/Victory", "Battle/Defeat", "Battle/Escape",
            "Event/Start"
        };

        public AudioLibraryScanResult Scan(
            string unityProjectPath,
            IEnumerable<HeroineProfile> profiles)
        {
            ValidateUnityProjectPath(unityProjectPath);
            string projectRoot = Path.GetFullPath(unityProjectPath);
            string audioRoot = Path.Combine(projectRoot, "Assets", "Resources", "Audio");
            Dictionary<string, string> files = FindAudioFiles(audioRoot);
            Dictionary<string, int> voiceReferences = CollectVoiceReferences(profiles);
            AudioLibraryScanResult result = new AudioLibraryScanResult();

            AddExpected(result.Items, files, projectRoot, "BGM", "Bgm", BgmIds);
            AddExpected(result.Items, files, projectRoot, "SE", "SE", SeIds);

            HashSet<string> registeredKeys = new HashSet<string>(
                result.Items.Select(BuildKey),
                StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> file in files.OrderBy(pair => pair.Key))
            {
                if (registeredKeys.Contains(file.Key)) continue;
                AudioLibraryItem item = CreateDiscoveredItem(file.Key, file.Value);
                if (item == null) continue;
                voiceReferences.TryGetValue(file.Key, out int referenceCount);
                item.ReferenceCount = referenceCount;
                result.Items.Add(item);
                registeredKeys.Add(file.Key);
            }

            foreach (KeyValuePair<string, int> reference in voiceReferences.OrderBy(pair => pair.Key))
            {
                if (registeredKeys.Contains(reference.Key)) continue;
                AudioLibraryItem item = CreateMissingVoiceItem(
                    projectRoot,
                    reference.Key,
                    reference.Value);
                if (item != null)
                {
                    result.Items.Add(item);
                    registeredKeys.Add(reference.Key);
                }
            }

            return result;
        }

        public static bool IsUnityProjectPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) &&
                Directory.Exists(Path.Combine(path, "Assets")) &&
                File.Exists(Path.Combine(path, "ProjectSettings", "ProjectVersion.txt"));
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FantasyLoveSimAssetTool",
                "audio-library.json");
        }

        public static string LoadUnityProjectPath()
        {
            try
            {
                string path = GetSettingsPath();
                if (!File.Exists(path)) return string.Empty;
                AudioLibrarySettings settings =
                    JsonSerializer.Deserialize<AudioLibrarySettings>(File.ReadAllText(path));
                return settings?.UnityProjectPath ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void SaveUnityProjectPath(string unityProjectPath)
        {
            string path = GetSettingsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(
                path,
                JsonSerializer.Serialize(
                    new AudioLibrarySettings { UnityProjectPath = unityProjectPath ?? string.Empty },
                    new JsonSerializerOptions { WriteIndented = true }));
        }

        public static Dictionary<string, int> CollectVoiceReferences(
            IEnumerable<HeroineProfile> profiles)
        {
            Dictionary<string, int> references =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (HeroineProfile profile in profiles ?? Enumerable.Empty<HeroineProfile>())
            {
                if (profile == null || string.IsNullOrWhiteSpace(profile.HeroineId)) continue;
                string heroineId = profile.HeroineId.Trim();

                if (profile.TrainingDialogues?.Items != null)
                {
                    foreach (TrainingDialogueEntry entry in profile.TrainingDialogues.Items)
                    {
                        if (entry?.Messages == null) continue;
                        foreach (TrainingDialogueMessage message in entry.Messages)
                        {
                            AddVoiceReference(references, heroineId, message?.VoiceId);
                        }
                    }
                }

                if (profile.BattleMessages?.ResultEvents != null)
                {
                    foreach (BattleResultEventEntry item in profile.BattleMessages.ResultEvents)
                    {
                        AddVoiceReference(references, heroineId, item?.VoiceId);
                    }
                }
                if (profile.BattleMessages?.PanelMessages != null)
                {
                    foreach (BattlePanelResultMessageEntry item in profile.BattleMessages.PanelMessages)
                    {
                        AddVoiceReference(references, heroineId, item?.VoiceId);
                    }
                }
            }
            return references;
        }

        private static void ValidateUnityProjectPath(string path)
        {
            if (!IsUnityProjectPath(path))
            {
                throw new InvalidOperationException(
                    "Unityプロジェクトを確認できません。ProjectSettings/ProjectVersion.txtがあるフォルダを選択してください。");
            }
        }

        private static Dictionary<string, string> FindAudioFiles(string audioRoot)
        {
            Dictionary<string, string> result =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(audioRoot)) return result;
            foreach (string file in Directory.EnumerateFiles(audioRoot, "*.*", SearchOption.AllDirectories))
            {
                if (!SupportedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                    continue;
                string relative = Path.GetRelativePath(audioRoot, file);
                string key = Normalize(Path.ChangeExtension(relative, null));
                if (!result.ContainsKey(key)) result.Add(key, file);
            }
            return result;
        }

        private static void AddExpected(
            List<AudioLibraryItem> destination,
            Dictionary<string, string> files,
            string projectRoot,
            string category,
            string folder,
            IEnumerable<string> ids)
        {
            foreach (string id in ids)
            {
                string key = Normalize(folder + "/" + id);
                files.TryGetValue(key, out string filePath);
                destination.Add(new AudioLibraryItem
                {
                    Category = category,
                    LogicalId = id,
                    FilePath = filePath ?? string.Empty,
                    ExpectedPath = Path.Combine(
                        projectRoot,
                        "Assets",
                        "Resources",
                        "Audio",
                        folder,
                        id.Replace('/', Path.DirectorySeparatorChar)) + ".*",
                    ReferenceCount = 1,
                    IsAvailable = !string.IsNullOrEmpty(filePath),
                    IsExpected = true
                });
            }
        }

        private static AudioLibraryItem CreateDiscoveredItem(string key, string filePath)
        {
            string[] parts = Normalize(key).Split('/');
            if (parts.Length < 2) return null;
            string category = parts[0].Equals("Bgm", StringComparison.OrdinalIgnoreCase)
                ? "BGM"
                : parts[0].Equals("SE", StringComparison.OrdinalIgnoreCase)
                    ? "SE"
                    : parts[0].Equals("Voice", StringComparison.OrdinalIgnoreCase)
                        ? "VOICE"
                        : "OTHER";
            string logicalId = string.Join("/", parts.Skip(1));
            string heroineId = category == "VOICE" && parts.Length >= 3 ? parts[1] : string.Empty;
            return new AudioLibraryItem
            {
                Category = category,
                LogicalId = logicalId,
                HeroineId = heroineId,
                FilePath = filePath,
                ExpectedPath = filePath,
                IsAvailable = true,
                IsExpected = false
            };
        }

        private static AudioLibraryItem CreateMissingVoiceItem(
            string projectRoot,
            string key,
            int referenceCount)
        {
            string[] parts = Normalize(key).Split('/');
            if (parts.Length < 3 || !parts[0].Equals("Voice", StringComparison.OrdinalIgnoreCase))
                return null;
            return new AudioLibraryItem
            {
                Category = "VOICE",
                LogicalId = string.Join("/", parts.Skip(1)),
                HeroineId = parts[1],
                ExpectedPath = Path.Combine(
                    projectRoot,
                    "Assets",
                    "Resources",
                    "Audio",
                    key.Replace('/', Path.DirectorySeparatorChar)) + ".*",
                ReferenceCount = referenceCount,
                IsAvailable = false,
                IsExpected = true
            };
        }

        private static void AddVoiceReference(
            Dictionary<string, int> references,
            string heroineId,
            string voiceId)
        {
            if (string.IsNullOrWhiteSpace(voiceId)) return;
            string normalized = Normalize(voiceId.Trim().Trim('/'));
            string key = normalized.StartsWith("Audio/Voice/", StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring("Audio/".Length)
                : "Voice/" + heroineId + "/" + normalized;
            references.TryGetValue(key, out int count);
            references[key] = count + 1;
        }

        private static string BuildKey(AudioLibraryItem item)
        {
            string folder = item.Category == "BGM" ? "Bgm" : item.Category == "VOICE" ? "Voice" : item.Category;
            return Normalize(folder + "/" + item.LogicalId);
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/').Trim('/');
        }

        private sealed class AudioLibrarySettings
        {
            public string UnityProjectPath { get; set; } = string.Empty;
        }
    }
}
