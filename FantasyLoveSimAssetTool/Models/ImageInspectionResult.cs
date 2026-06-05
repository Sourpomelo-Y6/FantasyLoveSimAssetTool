using System.Collections.ObjectModel;

namespace FantasyLoveSimAssetTool.Models
{
    public class ImageInspectionResult
    {
        public string FilePath { get; set; }

        public string FileFormat { get; set; }

        public int PixelWidth { get; set; }

        public int PixelHeight { get; set; }

        public bool HasTransparentPixels { get; set; }

        public ObservableCollection<string> Warnings { get; set; }

        public ImageInspectionResult()
        {
            FilePath = string.Empty;
            FileFormat = string.Empty;
            Warnings = new ObservableCollection<string>();
        }
    }
}
