using FantasyLoveSimAssetTool.Common;

namespace FantasyLoveSimAssetTool.Models
{
    public class PromptRecord : ObservableObject
    {
        private string positivePrompt;
        private string negativePrompt;
        private string model;
        private string vae;
        private string lora;
        private string sampler;
        private int steps;
        private double cfgScale;
        private long seed;
        private int imageWidth;
        private int imageHeight;
        private string controlNetMemo;
        private string upscaleMemo;
        private string inpaintMemo;
        private string adoptionReason;
        private string revisionMemo;
        private string comfyPromptId;
        private string comfyOutputFileName;
        private string comfyOutputSubfolder;
        private string comfyOutputType;
        private string comfyEndpointUrl;
        private string comfyWorkflowTemplatePath;
        private string comfyWorkflowJson;
        private string trainingId;
        private string trainingVisualState;
        private bool playerVisible;
        private bool heroineVisible;

        public string PositivePrompt
        {
            get { return positivePrompt; }
            set
            {
                if (positivePrompt == value) { return; }
                positivePrompt = value;
                OnPropertyChanged(nameof(PositivePrompt));
            }
        }

        public string NegativePrompt
        {
            get { return negativePrompt; }
            set
            {
                if (negativePrompt == value) { return; }
                negativePrompt = value;
                OnPropertyChanged(nameof(NegativePrompt));
            }
        }

        public string Model
        {
            get { return model; }
            set
            {
                if (model == value) { return; }
                model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        public string Vae
        {
            get { return vae; }
            set
            {
                if (vae == value) { return; }
                vae = value;
                OnPropertyChanged(nameof(Vae));
            }
        }

        public string Lora
        {
            get { return lora; }
            set
            {
                if (lora == value) { return; }
                lora = value;
                OnPropertyChanged(nameof(Lora));
            }
        }

        public string Sampler
        {
            get { return sampler; }
            set
            {
                if (sampler == value) { return; }
                sampler = value;
                OnPropertyChanged(nameof(Sampler));
            }
        }

        public int Steps
        {
            get { return steps; }
            set
            {
                if (steps == value) { return; }
                steps = value;
                OnPropertyChanged(nameof(Steps));
            }
        }

        public double CfgScale
        {
            get { return cfgScale; }
            set
            {
                if (cfgScale == value) { return; }
                cfgScale = value;
                OnPropertyChanged(nameof(CfgScale));
            }
        }

        public long Seed
        {
            get { return seed; }
            set
            {
                if (seed == value) { return; }
                seed = value;
                OnPropertyChanged(nameof(Seed));
            }
        }

        public int ImageWidth
        {
            get { return imageWidth; }
            set
            {
                if (imageWidth == value) { return; }
                imageWidth = value;
                OnPropertyChanged(nameof(ImageWidth));
            }
        }

        public int ImageHeight
        {
            get { return imageHeight; }
            set
            {
                if (imageHeight == value) { return; }
                imageHeight = value;
                OnPropertyChanged(nameof(ImageHeight));
            }
        }

        public string ControlNetMemo
        {
            get { return controlNetMemo; }
            set
            {
                if (controlNetMemo == value) { return; }
                controlNetMemo = value;
                OnPropertyChanged(nameof(ControlNetMemo));
            }
        }

        public string UpscaleMemo
        {
            get { return upscaleMemo; }
            set
            {
                if (upscaleMemo == value) { return; }
                upscaleMemo = value;
                OnPropertyChanged(nameof(UpscaleMemo));
            }
        }

        public string InpaintMemo
        {
            get { return inpaintMemo; }
            set
            {
                if (inpaintMemo == value) { return; }
                inpaintMemo = value;
                OnPropertyChanged(nameof(InpaintMemo));
            }
        }

        public string AdoptionReason
        {
            get { return adoptionReason; }
            set
            {
                if (adoptionReason == value) { return; }
                adoptionReason = value;
                OnPropertyChanged(nameof(AdoptionReason));
            }
        }

        public string RevisionMemo
        {
            get { return revisionMemo; }
            set
            {
                if (revisionMemo == value) { return; }
                revisionMemo = value;
                OnPropertyChanged(nameof(RevisionMemo));
            }
        }

        public string ComfyPromptId
        {
            get { return comfyPromptId; }
            set
            {
                if (comfyPromptId == value) { return; }
                comfyPromptId = value;
                OnPropertyChanged(nameof(ComfyPromptId));
            }
        }

        public string ComfyOutputFileName
        {
            get { return comfyOutputFileName; }
            set
            {
                if (comfyOutputFileName == value) { return; }
                comfyOutputFileName = value;
                OnPropertyChanged(nameof(ComfyOutputFileName));
            }
        }

        public string ComfyOutputSubfolder
        {
            get { return comfyOutputSubfolder; }
            set
            {
                if (comfyOutputSubfolder == value) { return; }
                comfyOutputSubfolder = value;
                OnPropertyChanged(nameof(ComfyOutputSubfolder));
            }
        }

        public string ComfyOutputType
        {
            get { return comfyOutputType; }
            set
            {
                if (comfyOutputType == value) { return; }
                comfyOutputType = value;
                OnPropertyChanged(nameof(ComfyOutputType));
            }
        }

        public string ComfyEndpointUrl
        {
            get { return comfyEndpointUrl; }
            set
            {
                if (comfyEndpointUrl == value) { return; }
                comfyEndpointUrl = value;
                OnPropertyChanged(nameof(ComfyEndpointUrl));
            }
        }

        public string ComfyWorkflowTemplatePath
        {
            get { return comfyWorkflowTemplatePath; }
            set
            {
                if (comfyWorkflowTemplatePath == value) { return; }
                comfyWorkflowTemplatePath = value;
                OnPropertyChanged(nameof(ComfyWorkflowTemplatePath));
            }
        }

        public string ComfyWorkflowJson
        {
            get { return comfyWorkflowJson; }
            set
            {
                if (comfyWorkflowJson == value) { return; }
                comfyWorkflowJson = value;
                OnPropertyChanged(nameof(ComfyWorkflowJson));
            }
        }

        public string TrainingId
        {
            get { return trainingId; }
            set { if (trainingId != value) { trainingId = value; OnPropertyChanged(nameof(TrainingId)); } }
        }

        public string TrainingVisualState
        {
            get { return trainingVisualState; }
            set { if (trainingVisualState != value) { trainingVisualState = value; OnPropertyChanged(nameof(TrainingVisualState)); } }
        }

        public bool PlayerVisible
        {
            get { return playerVisible; }
            set { if (playerVisible != value) { playerVisible = value; OnPropertyChanged(nameof(PlayerVisible)); } }
        }

        public bool HeroineVisible
        {
            get { return heroineVisible; }
            set { if (heroineVisible != value) { heroineVisible = value; OnPropertyChanged(nameof(HeroineVisible)); } }
        }

        public PromptRecord()
        {
            positivePrompt = string.Empty;
            negativePrompt = string.Empty;
            model = string.Empty;
            vae = string.Empty;
            lora = string.Empty;
            sampler = string.Empty;
            controlNetMemo = string.Empty;
            upscaleMemo = string.Empty;
            inpaintMemo = string.Empty;
            adoptionReason = string.Empty;
            revisionMemo = string.Empty;
            comfyPromptId = string.Empty;
            comfyOutputFileName = string.Empty;
            comfyOutputSubfolder = string.Empty;
            comfyOutputType = string.Empty;
            comfyEndpointUrl = string.Empty;
            comfyWorkflowTemplatePath = string.Empty;
            comfyWorkflowJson = string.Empty;
            trainingId = string.Empty;
            trainingVisualState = string.Empty;
        }
    }
}
