using FantasyLoveSimAssetTool.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FantasyLoveSimAssetTool.Services
{
    public class CharacterProjectService
    {
        private const string ProfileFileName = "profile.json";
        private readonly JsonSerializerOptions jsonOptions;

        public string WorkspaceRoot { get; }

        public string CharactersDirectory
        {
            get { return Path.Combine(WorkspaceRoot, "Characters"); }
        }

        public CharacterProjectService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        public CharacterProjectService(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
            {
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));
            }

            WorkspaceRoot = workspaceRoot;
            jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public HeroineProfile CreateCharacter(string heroineId, string displayName)
        {
            ValidateHeroineId(heroineId);

            HeroineProfile profile = new HeroineProfile
            {
                HeroineId = heroineId.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? heroineId.Trim() : displayName.Trim()
            };

            EnsureCharacterDirectories(profile.HeroineId);
            SaveProfile(profile);
            return profile;
        }

        public void SaveProfile(HeroineProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            ValidateHeroineId(profile.HeroineId);
            EnsureCharacterDirectories(profile.HeroineId);

            string json = JsonSerializer.Serialize(profile, jsonOptions);
            File.WriteAllText(GetProfilePath(profile.HeroineId), json);
        }

        public HeroineProfile LoadProfile(string heroineId)
        {
            ValidateHeroineId(heroineId);

            string path = GetProfilePath(heroineId);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Profile file was not found.", path);
            }

            string json = File.ReadAllText(path);
            HeroineProfile profile = JsonSerializer.Deserialize<HeroineProfile>(json, jsonOptions);
            if (profile == null)
            {
                throw new InvalidOperationException("Profile file could not be deserialized.");
            }

            return profile;
        }

        public IReadOnlyList<HeroineProfile> LoadProfiles()
        {
            if (!Directory.Exists(CharactersDirectory))
            {
                return new List<HeroineProfile>();
            }

            return Directory.GetDirectories(CharactersDirectory)
                .Select(Path.GetFileName)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => LoadProfile(id))
                .OrderBy(profile => profile.HeroineId)
                .ToList();
        }

        public void EnsureCharacterDirectories(string heroineId)
        {
            ValidateHeroineId(heroineId);

            Directory.CreateDirectory(GetCharacterDirectory(heroineId));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Sprites"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Event"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Actions"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Images", "Ending"));
            Directory.CreateDirectory(Path.Combine(GetCharacterDirectory(heroineId), "Prompts"));
        }

        public string GetCharacterDirectory(string heroineId)
        {
            ValidateHeroineId(heroineId);
            return Path.Combine(CharactersDirectory, heroineId.Trim());
        }

        public string GetProfilePath(string heroineId)
        {
            return Path.Combine(GetCharacterDirectory(heroineId), ProfileFileName);
        }

        private static void ValidateHeroineId(string heroineId)
        {
            if (string.IsNullOrWhiteSpace(heroineId))
            {
                throw new ArgumentException("HeroineId is required.", nameof(heroineId));
            }

            if (heroineId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("HeroineId contains invalid file name characters.", nameof(heroineId));
            }
        }
    }
}
