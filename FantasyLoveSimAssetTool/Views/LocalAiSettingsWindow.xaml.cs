using FantasyLoveSimAssetTool.ViewModels;
using System;
using System.Windows;

namespace FantasyLoveSimAssetTool.Views
{
    public partial class LocalAiSettingsWindow : Window
    {
        private readonly LocalAiSettingsViewModel viewModel;

        public LocalAiSettingsWindow(string workspaceRoot)
        {
            InitializeComponent();
            viewModel = new LocalAiSettingsViewModel(workspaceRoot);
            DataContext = viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
