using FantasyLoveSimAssetTool.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace FantasyLoveSimAssetTool.Tests
{
    [TestClass]
    public class BattleResultEventEntryTests
    {
        [TestMethod]
        public void EditingSummaryFields_RaisesPropertyChanged()
        {
            var entry = new BattleResultEventEntry();
            var changed = new List<string>();
            entry.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            entry.EventId = "Battle_Win";
            entry.ResultType = "DuoVictory";
            entry.Message = "勝てましたね。";
            entry.VoiceId = "battle_win_01";

            CollectionAssert.Contains(changed, nameof(BattleResultEventEntry.EventId));
            CollectionAssert.Contains(changed, nameof(BattleResultEventEntry.ResultType));
            CollectionAssert.Contains(changed, nameof(BattleResultEventEntry.Message));
            CollectionAssert.Contains(changed, nameof(BattleResultEventEntry.VoiceId));
        }

        [TestMethod]
        public void EditingOutfitText_UpdatesArrayAndRaisesBothNotifications()
        {
            var entry = new BattleResultEventEntry();
            var changed = new List<string>();
            entry.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            entry.UnlockedOutfitIdsText = "Dress, Armor, Dress";

            CollectionAssert.AreEqual(new[] { "Dress", "Armor" }, entry.UnlockedOutfitIds);
            CollectionAssert.Contains(changed, nameof(BattleResultEventEntry.UnlockedOutfitIds));
            CollectionAssert.Contains(changed, nameof(BattleResultEventEntry.UnlockedOutfitIdsText));
        }

        [TestMethod]
        public void EditingPanelMessageSummaryFields_RaisesPropertyChanged()
        {
            var entry = new BattlePanelResultMessageEntry();
            var changed = new List<string>();
            entry.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

            entry.MessageId = "Panel_Win";
            entry.ResultType = "Victory";
            entry.Message = "勝利です。";
            entry.VoiceId = "panel_win_01";

            CollectionAssert.Contains(changed, nameof(BattlePanelResultMessageEntry.MessageId));
            CollectionAssert.Contains(changed, nameof(BattlePanelResultMessageEntry.ResultType));
            CollectionAssert.Contains(changed, nameof(BattlePanelResultMessageEntry.Message));
            CollectionAssert.Contains(changed, nameof(BattlePanelResultMessageEntry.VoiceId));
        }
    }
}
