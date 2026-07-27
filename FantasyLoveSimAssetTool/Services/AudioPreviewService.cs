using System;
using System.IO;
using System.Windows.Media;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class AudioPreviewService
    {
        private readonly MediaPlayer player = new MediaPlayer();

        public void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("再生する音声ファイルが見つかりません。", filePath);
            }

            player.Stop();
            player.Open(new Uri(Path.GetFullPath(filePath), UriKind.Absolute));
            player.Play();
        }

        public void Stop()
        {
            player.Stop();
            player.Close();
        }
    }
}
