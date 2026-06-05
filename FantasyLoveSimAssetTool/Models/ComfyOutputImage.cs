namespace FantasyLoveSimAssetTool.Models
{
    public class ComfyOutputImage
    {
        public string FileName { get; set; }

        public string Subfolder { get; set; }

        public string Type { get; set; }

        public string DisplayPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Subfolder))
                {
                    return FileName ?? string.Empty;
                }

                return $"{Subfolder}/{FileName}";
            }
        }

        public ComfyOutputImage()
        {
            FileName = string.Empty;
            Subfolder = string.Empty;
            Type = string.Empty;
        }
    }
}
