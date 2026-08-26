using FantasyLoveSimAssetTool.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace FantasyLoveSimAssetTool.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainWindowModel model && !model.CanCloseWindow()) e.Cancel = true;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            if (DataContext is MainWindowModel model)
            {
                model.CheckLegacyWorkspaceMigration();
            }
        }

        private void OpenLocalAiSettings_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowModel model)
            {
                new LocalAiSettingsWindow(model.WorkspacePath) { Owner = this }.ShowDialog();
            }
        }

        private void ImageSourcePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void ImageSourcePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            if (DataContext is MainWindowModel model)
            {
                model.SetImageSourceFromDroppedFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }

        private void HeadPartImagePath_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            if (DataContext is MainWindowModel model)
            {
                model.SetHeadPartImageFromDroppedFiles((string[])e.Data.GetData(DataFormats.FileDrop));
            }
        }
    }
}
