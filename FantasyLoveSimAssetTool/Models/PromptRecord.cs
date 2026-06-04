namespace FantasyLoveSimAssetTool.Models
{
    public class PromptRecord
    {
        public string PositivePrompt { get; set; }

        public string NegativePrompt { get; set; }

        public string Model { get; set; }

        public string Vae { get; set; }

        public string Lora { get; set; }

        public string Sampler { get; set; }

        public int Steps { get; set; }

        public double CfgScale { get; set; }

        public long Seed { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public string ControlNetMemo { get; set; }

        public string UpscaleMemo { get; set; }

        public string InpaintMemo { get; set; }

        public string AdoptionReason { get; set; }

        public string RevisionMemo { get; set; }

        public PromptRecord()
        {
            PositivePrompt = string.Empty;
            NegativePrompt = string.Empty;
            Model = string.Empty;
            Vae = string.Empty;
            Lora = string.Empty;
            Sampler = string.Empty;
            ControlNetMemo = string.Empty;
            UpscaleMemo = string.Empty;
            InpaintMemo = string.Empty;
            AdoptionReason = string.Empty;
            RevisionMemo = string.Empty;
        }
    }
}
