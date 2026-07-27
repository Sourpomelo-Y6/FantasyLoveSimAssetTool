using System;
using System.IO;
using System.Windows.Media;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class AudioPreviewService
    {
        private readonly MediaPlayer player = new MediaPlayer();
        private string currentFilePath = string.Empty;

        public event Action<string> PlaybackFailed;

        public AudioPreviewService()
        {
            player.MediaFailed += OnMediaFailed;
        }

        public void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("再生する音声ファイルが見つかりません。", filePath);
            }

            player.Stop();
            currentFilePath = Path.GetFullPath(filePath);
            player.Open(new Uri(currentFilePath, UriKind.Absolute));
            player.Play();
        }

        public void Stop()
        {
            player.Stop();
            player.Close();
            currentFilePath = string.Empty;
        }

        private void OnMediaFailed(object sender, ExceptionEventArgs e)
        {
            string fileName = string.IsNullOrWhiteSpace(currentFilePath)
                ? "選択した音声"
                : Path.GetFileName(currentFilePath);
            PlaybackFailed?.Invoke(
                $"{fileName} はAssetToolの再生環境で再生できません。" +
                "Unityへの登録と利用には影響しません。");
        }
    }
}
