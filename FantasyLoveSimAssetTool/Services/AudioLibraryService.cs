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

    public sealed class AudioRegistrationPlan
    {
        public string Category { get; set; } = string.Empty;
        public string LogicalId { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string DestinationPath { get; set; } = string.Empty;
        public List<string> ExistingPaths { get; } = new List<string>();
        public bool HasConflicts => ExistingPaths.Any(path =>
            !PathsEqual(path, SourcePath) || !PathsEqual(path, DestinationPath));

        private static bool PathsEqual(string left, string right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) return false;
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
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
            Dictionary<string, string> voiceReferenceDetails =
                CollectVoiceReferenceDetails(profiles);
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
                voiceReferenceDetails.TryGetValue(file.Key, out string referenceDetails);
                item.ReferenceDetails = referenceDetails ?? string.Empty;
                result.Items.Add(item);
                registeredKeys.Add(file.Key);
            }

            foreach (KeyValuePair<string, int> reference in voiceReferences.OrderBy(pair => pair.Key))
            {
                if (registeredKeys.Contains(reference.Key)) continue;
                AudioLibraryItem item = CreateMissingVoiceItem(
                    projectRoot,
                    reference.Key,
                    reference.Value,
                    voiceReferenceDetails.TryGetValue(reference.Key, out string details)
                        ? details
                        : string.Empty);
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

        public AudioRegistrationPlan CreateRegistrationPlan(
            string unityProjectPath,
            AudioLibraryItem item,
            string sourcePath)
        {
            ValidateUnityProjectPath(unityProjectPath);
            if (item == null ||
                (!string.Equals(item.Category, "BGM", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(item.Category, "SE", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("ファイル登録はBGMまたはSEの行で利用してください。");
            }
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                throw new FileNotFoundException("登録する音声ファイルが見つかりません。", sourcePath);
            }

            string extension = Path.GetExtension(sourcePath);
            if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "対応する音声形式は .wav / .mp3 / .ogg / .aif / .aiff です。");
            }

            string logicalId = ValidateLogicalId(item.LogicalId);
            string folder = string.Equals(item.Category, "BGM", StringComparison.OrdinalIgnoreCase)
                ? "Bgm"
                : "SE";
            string audioRoot = Path.GetFullPath(Path.Combine(
                unityProjectPath,
                "Assets",
                "Resources",
                "Audio"));
            string destinationBase = Path.GetFullPath(Path.Combine(
                audioRoot,
                folder,
                logicalId.Replace('/', Path.DirectorySeparatorChar)));
            EnsurePathIsUnder(destinationBase, audioRoot);

            AudioRegistrationPlan plan = new AudioRegistrationPlan
            {
                Category = item.Category.ToUpperInvariant(),
                LogicalId = logicalId,
                SourcePath = Path.GetFullPath(sourcePath),
                DestinationPath = destinationBase + extension.ToLowerInvariant()
            };

            string destinationDirectory = Path.GetDirectoryName(destinationBase);
            string destinationName = Path.GetFileName(destinationBase);
            if (Directory.Exists(destinationDirectory))
            {
                foreach (string existingPath in Directory.EnumerateFiles(
                    destinationDirectory,
                    destinationName + ".*",
                    SearchOption.TopDirectoryOnly))
                {
                    if (SupportedExtensions.Contains(
                        Path.GetExtension(existingPath),
                        StringComparer.OrdinalIgnoreCase))
                    {
                        plan.ExistingPaths.Add(Path.GetFullPath(existingPath));
                    }
                }
            }

            return plan;
        }

        public AudioRegistrationPlan CreateVoiceRegistrationPlan(
            string unityProjectPath,
            string heroineId,
            string usage,
            string voiceId,
            string sourcePath)
        {
            ValidateUnityProjectPath(unityProjectPath);
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                throw new FileNotFoundException("登録する音声ファイルが見つかりません。", sourcePath);
            }
            string extension = Path.GetExtension(sourcePath);
            if (!SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "対応する音声形式は .wav / .mp3 / .ogg / .aif / .aiff です。");
            }

            string normalizedHeroineId = ValidateLogicalId(heroineId);
            string normalizedUsage = ValidateLogicalId(usage);
            string normalizedVoiceId = ValidateLogicalId(voiceId);
            string relativeVoiceId = normalizedVoiceId.StartsWith(
                normalizedUsage + "/",
                StringComparison.OrdinalIgnoreCase)
                ? normalizedVoiceId
                : normalizedUsage + "/" + normalizedVoiceId;
            string audioRoot = Path.GetFullPath(Path.Combine(
                unityProjectPath,
                "Assets",
                "Resources",
                "Audio"));
            string destinationBase = Path.GetFullPath(Path.Combine(
                audioRoot,
                "Voice",
                normalizedHeroineId.Replace('/', Path.DirectorySeparatorChar),
                relativeVoiceId.Replace('/', Path.DirectorySeparatorChar)));
            EnsurePathIsUnder(destinationBase, audioRoot);

            AudioRegistrationPlan plan = new AudioRegistrationPlan
            {
                Category = "VOICE",
                LogicalId = normalizedHeroineId + "/" + relativeVoiceId,
                SourcePath = Path.GetFullPath(sourcePath),
                DestinationPath = destinationBase + extension.ToLowerInvariant()
            };
            AddExistingAudioPaths(plan, destinationBase);
            return plan;
        }

        public string RegisterAudio(AudioRegistrationPlan plan, bool replaceExisting)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (plan.HasConflicts && !replaceExisting)
            {
                throw new InvalidOperationException(
                    "同じIDの音声ファイルが存在します。置き換えを確認してから登録してください。");
            }

            string sourcePath = Path.GetFullPath(plan.SourcePath);
            string destinationPath = Path.GetFullPath(plan.DestinationPath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));

            // コピー成功前に既存ファイルを失わないよう、先に登録先を確保する。
            if (!PathsEqual(sourcePath, destinationPath))
            {
                File.Copy(sourcePath, destinationPath, replaceExisting);
            }

            if (replaceExisting)
            {
                foreach (string existingPath in plan.ExistingPaths)
                {
                    string fullExistingPath = Path.GetFullPath(existingPath);
                    if (PathsEqual(fullExistingPath, sourcePath) ||
                        PathsEqual(fullExistingPath, destinationPath))
                    {
                        continue;
                    }
                    File.Delete(fullExistingPath);
                    string metaPath = fullExistingPath + ".meta";
                    if (File.Exists(metaPath)) File.Delete(metaPath);
                }
            }
            return destinationPath;
        }

        public string GetRegistrationDirectory(
            string unityProjectPath,
            AudioLibraryItem item)
        {
            ValidateUnityProjectPath(unityProjectPath);
            if (item == null) throw new ArgumentNullException(nameof(item));
            string folder = string.Equals(item.Category, "BGM", StringComparison.OrdinalIgnoreCase)
                ? "Bgm"
                : string.Equals(item.Category, "SE", StringComparison.OrdinalIgnoreCase)
                    ? "SE"
                    : throw new InvalidOperationException("保存先表示はBGMまたはSEで利用してください。");
            string logicalId = ValidateLogicalId(item.LogicalId);
            return Path.GetDirectoryName(Path.Combine(
                Path.GetFullPath(unityProjectPath),
                "Assets",
                "Resources",
                "Audio",
                folder,
                logicalId.Replace('/', Path.DirectorySeparatorChar)));
        }

        public string GetVoiceRegistrationDirectory(
            string unityProjectPath,
            string heroineId,
            string usage)
        {
            ValidateUnityProjectPath(unityProjectPath);
            string normalizedHeroineId = ValidateLogicalId(heroineId);
            string normalizedUsage = ValidateLogicalId(usage);
            return Path.Combine(
                Path.GetFullPath(unityProjectPath),
                "Assets",
                "Resources",
                "Audio",
                "Voice",
                normalizedHeroineId.Replace('/', Path.DirectorySeparatorChar),
                normalizedUsage.Replace('/', Path.DirectorySeparatorChar));
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

        public static Dictionary<string, string> CollectVoiceReferenceDetails(
            IEnumerable<HeroineProfile> profiles)
        {
            Dictionary<string, HashSet<string>> details =
                new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (HeroineProfile profile in profiles ?? Enumerable.Empty<HeroineProfile>())
            {
                if (profile == null || string.IsNullOrWhiteSpace(profile.HeroineId)) continue;
                string heroineId = profile.HeroineId.Trim();
                foreach (TrainingDialogueEntry entry in
                    profile.TrainingDialogues?.Items ?? Enumerable.Empty<TrainingDialogueEntry>())
                {
                    if (entry?.Messages == null) continue;
                    foreach (TrainingDialogueMessage message in entry.Messages)
                    {
                        AddVoiceReferenceDetail(
                            details,
                            heroineId,
                            message?.VoiceId,
                            $"訓練: {entry.TrainingId}/{entry.VisualState}");
                    }
                }
                foreach (BattleResultEventEntry item in
                    profile.BattleMessages?.ResultEvents ??
                    Enumerable.Empty<BattleResultEventEntry>())
                {
                    AddVoiceReferenceDetail(
                        details,
                        heroineId,
                        item?.VoiceId,
                        $"戦闘後イベント: {item?.EventId}");
                }
                foreach (BattlePanelResultMessageEntry item in
                    profile.BattleMessages?.PanelMessages ??
                    Enumerable.Empty<BattlePanelResultMessageEntry>())
                {
                    AddVoiceReferenceDetail(
                        details,
                        heroineId,
                        item?.VoiceId,
                        $"戦闘パネル: {item?.MessageId}");
                }
            }
            return details.ToDictionary(
                pair => pair.Key,
                pair => string.Join(Environment.NewLine, pair.Value.OrderBy(value => value)),
                StringComparer.OrdinalIgnoreCase);
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
            int referenceCount,
            string referenceDetails)
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
                ReferenceDetails = referenceDetails ?? string.Empty,
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
            string key = BuildVoiceReferenceKey(heroineId, voiceId);
            references.TryGetValue(key, out int count);
            references[key] = count + 1;
        }

        private static void AddVoiceReferenceDetail(
            Dictionary<string, HashSet<string>> details,
            string heroineId,
            string voiceId,
            string description)
        {
            if (string.IsNullOrWhiteSpace(voiceId)) return;
            string key = BuildVoiceReferenceKey(heroineId, voiceId);
            if (!details.TryGetValue(key, out HashSet<string> descriptions))
            {
                descriptions = new HashSet<string>(StringComparer.Ordinal);
                details.Add(key, descriptions);
            }
            descriptions.Add(description);
        }

        private static string BuildVoiceReferenceKey(string heroineId, string voiceId)
        {
            string normalized = Normalize(voiceId.Trim().Trim('/'));
            return normalized.StartsWith("Audio/Voice/", StringComparison.OrdinalIgnoreCase)
                ? normalized.Substring("Audio/".Length)
                : "Voice/" + heroineId + "/" + normalized;
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

        private static string ValidateLogicalId(string logicalId)
        {
            string normalized = Normalize(logicalId);
            if (string.IsNullOrWhiteSpace(normalized) ||
                Path.IsPathRooted(logicalId) ||
                normalized.Split('/').Any(part =>
                    string.IsNullOrWhiteSpace(part) ||
                    part == "." ||
                    part == ".." ||
                    part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                throw new InvalidOperationException("音声IDに使用できないパスが含まれています。");
            }
            return normalized;
        }

        private static void EnsurePathIsUnder(string path, string root)
        {
            string rootWithSeparator = root.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("音声の保存先がUnityプロジェクト外です。");
            }
        }

        private static void AddExistingAudioPaths(
            AudioRegistrationPlan plan,
            string destinationBase)
        {
            string destinationDirectory = Path.GetDirectoryName(destinationBase);
            string destinationName = Path.GetFileName(destinationBase);
            if (!Directory.Exists(destinationDirectory)) return;
            foreach (string existingPath in Directory.EnumerateFiles(
                destinationDirectory,
                destinationName + ".*",
                SearchOption.TopDirectoryOnly))
            {
                if (SupportedExtensions.Contains(
                    Path.GetExtension(existingPath),
                    StringComparer.OrdinalIgnoreCase))
                {
                    plan.ExistingPaths.Add(Path.GetFullPath(existingPath));
                }
            }
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }

        private sealed class AudioLibrarySettings
        {
            public string UnityProjectPath { get; set; } = string.Empty;
        }
    }
}
