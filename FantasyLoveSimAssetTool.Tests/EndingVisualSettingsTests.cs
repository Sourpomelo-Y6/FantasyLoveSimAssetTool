using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class EndingVisualSettingsTests
    {
        [TestMethod]
        public void SaveAndLoadProfile_PreservesEndingVisualSettings()
        {
            string root = Path.Combine(Path.GetTempPath(), "FantasyLoveSimEndingTests", Guid.NewGuid().ToString("N"));
            try
            {
                CharacterProjectService service = new CharacterProjectService(root);
                HeroineProfile profile = service.CreateCharacter("TestHeroine", "Test");
                profile.ConversationEntries.Add(new ConversationEntry
                {
                    Kind = ConversationDataKind.Endings,
                    Id = "GoodEnding",
                    Title = "Good Ending",
                    EndingVisualMode = "StillOnly",
                    KeepEndingStillAcrossPages = true
                });
                service.SaveProfile(profile);

                ConversationEntry loaded = service.LoadProfile("TestHeroine").ConversationEntries[0];

                Assert.AreEqual("StillOnly", loaded.EndingVisualMode);
                Assert.IsTrue(loaded.KeepEndingStillAcrossPages);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
            }
        }
    }
}
