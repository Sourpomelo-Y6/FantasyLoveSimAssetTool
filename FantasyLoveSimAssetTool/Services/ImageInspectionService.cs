using FantasyLoveSimAssetTool.Models;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FantasyLoveSimAssetTool.Services
{
    public class ImageInspectionService
    {
        private const int MinimumDimension = 512;
        private const double SpriteMinimumHeightWidthRatio = 1.2;
        private const double ExtremeAspectRatio = 3.0;

        public ImageInspectionResult Inspect(string imagePath, AssetUsage usage)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new ArgumentException("Image path is required.", nameof(imagePath));
            }

            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Image file was not found.", imagePath);
            }

            BitmapDecoder decoder = BitmapDecoder.Create(
                new Uri(imagePath, UriKind.Absolute),
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            BitmapSource frame = decoder.Frames[0];
            ImageInspectionResult result = new ImageInspectionResult
            {
                FilePath = imagePath,
                FileFormat = Path.GetExtension(imagePath).TrimStart('.').ToUpperInvariant(),
                PixelWidth = frame.PixelWidth,
                PixelHeight = frame.PixelHeight,
                HasTransparentPixels = HasTransparentPixels(frame)
            };

            AddWarnings(result, usage);
            return result;
        }

        public string BuildSummary(ImageInspectionResult result)
        {
            if (result == null)
            {
                return string.Empty;
            }

            string transparency = result.HasTransparentPixels ? "透過あり" : "透過なし";
            return $"{result.PixelWidth}x{result.PixelHeight} / {result.FileFormat} / {transparency}";
        }

        private static void AddWarnings(ImageInspectionResult result, AssetUsage usage)
        {
            if (result.PixelWidth < MinimumDimension || result.PixelHeight < MinimumDimension)
            {
                result.Warnings.Add($"画像サイズが小さい可能性があります: {result.PixelWidth}x{result.PixelHeight}");
            }

            double widthHeightRatio = result.PixelHeight == 0
                ? 0
                : (double)result.PixelWidth / result.PixelHeight;
            if (widthHeightRatio > ExtremeAspectRatio || widthHeightRatio < 1 / ExtremeAspectRatio)
            {
                result.Warnings.Add($"縦横比が極端です: {result.PixelWidth}x{result.PixelHeight}");
            }

            if (usage == AssetUsage.Sprites || usage == AssetUsage.Battle)
            {
                if (!string.Equals(result.FileFormat, "PNG", StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add(usage == AssetUsage.Battle
                        ? "戦闘画像は透過 PNG が望ましいです。"
                        : "立ち絵は透過 PNG が望ましいです。");
                }

                if (!result.HasTransparentPixels)
                {
                    result.Warnings.Add(usage == AssetUsage.Battle
                        ? "戦闘画像に透過ピクセルが見つかりません。"
                        : "立ち絵に透過ピクセルが見つかりません。");
                }

                if (usage == AssetUsage.Sprites)
                {
                    double heightWidthRatio = result.PixelWidth == 0
                        ? 0
                        : (double)result.PixelHeight / result.PixelWidth;
                    if (heightWidthRatio < SpriteMinimumHeightWidthRatio)
                    {
                        result.Warnings.Add("立ち絵としては縦長の画像が望ましいです。");
                    }
                }
            }
        }

        private static bool HasTransparentPixels(BitmapSource source)
        {
            BitmapSource readableSource = source;
            if (source.Format != PixelFormats.Bgra32)
            {
                readableSource = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            }

            int stride = readableSource.PixelWidth * 4;
            byte[] pixels = new byte[stride * readableSource.PixelHeight];
            readableSource.CopyPixels(pixels, stride, 0);

            for (int index = 3; index < pixels.Length; index += 4)
            {
                if (pixels[index] < 255)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
