using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FantasyLoveSimAssetTool.Models;
using FantasyLoveSimAssetTool.ViewModels;

namespace FantasyLoveSimAssetTool.Views
{
    public partial class LocalAiShortTextPanel : UserControl
    {
        public static readonly DependencyProperty TargetGroupProperty = DependencyProperty.Register(
            nameof(TargetGroup),
            typeof(string),
            typeof(LocalAiShortTextPanel),
            new PropertyMetadata("Common", OnTargetGroupChanged));

        public LocalAiShortTextPanel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
            IsVisibleChanged += OnIsVisibleChanged;
        }

        public string TargetGroup
        {
            get => (string)GetValue(TargetGroupProperty);
            set => SetValue(TargetGroupProperty, value);
        }

        private static void OnTargetGroupChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((LocalAiShortTextPanel)dependencyObject).RefreshTargets();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RefreshTargets();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RefreshTargets();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                RefreshTargets();
            }
        }

        private void RefreshTargets()
        {
            if (TargetComboBox == null || DataContext is not MainWindowModel viewModel)
            {
                return;
            }

            var targets = viewModel.ShortTextTargets
                .Where(target => IsTargetInGroup(target, TargetGroup))
                .ToList();

            TargetComboBox.ItemsSource = targets;

            if (IsVisible && !targets.Contains(viewModel.SelectedShortTextTarget))
            {
                viewModel.SelectedShortTextTarget = targets.FirstOrDefault();
            }
        }

        private static bool IsTargetInGroup(ShortTextGenerationTarget target, string targetGroup)
        {
            if (string.Equals(targetGroup, "Outfit", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "OutfitMessage" || target.RequiredContext == "OutfitReaction";
            if (string.Equals(targetGroup, "BattleSkill", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "BattleSkill";
            if (string.Equals(targetGroup, "TrainingSkill", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "TrainingSkill";
            if (string.Equals(targetGroup, "SkillTreeNode", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "SkillTreeNode";
            if (string.Equals(targetGroup, "ConversationLine", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "ConversationLine";
            if (string.Equals(targetGroup, "BattleResultEvent", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "BattleResultEvent";
            if (string.Equals(targetGroup, "BattlePanelMessage", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "BattlePanelMessage";
            if (string.Equals(targetGroup, "SoloReturnReaction", StringComparison.OrdinalIgnoreCase))
                return target.RequiredContext == "SoloReturnReaction";
            return target.RequiredContext == "None";
        }
    }
}
