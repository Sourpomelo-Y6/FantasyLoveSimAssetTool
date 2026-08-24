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

            var isOutfitGroup = string.Equals(TargetGroup, "Outfit", StringComparison.OrdinalIgnoreCase);
            var targets = viewModel.ShortTextTargets
                .Where(target => IsTargetInGroup(target, isOutfitGroup))
                .ToList();

            TargetComboBox.ItemsSource = targets;

            if (IsVisible && !targets.Contains(viewModel.SelectedShortTextTarget))
            {
                viewModel.SelectedShortTextTarget = targets.FirstOrDefault();
            }
        }

        private static bool IsTargetInGroup(ShortTextGenerationTarget target, bool isOutfitGroup)
        {
            var isOutfitTarget = string.Equals(target.RequiredContext, "OutfitMessage", StringComparison.Ordinal)
                || string.Equals(target.RequiredContext, "OutfitReaction", StringComparison.Ordinal);
            return isOutfitGroup ? isOutfitTarget : !isOutfitTarget;
        }
    }
}
