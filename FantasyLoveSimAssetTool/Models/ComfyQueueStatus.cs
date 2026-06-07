namespace FantasyLoveSimAssetTool.Models
{
    public class ComfyQueueStatus
    {
        public int RunningCount { get; set; }

        public int PendingCount { get; set; }

        public bool IsTargetRunning { get; set; }

        public int TargetPendingIndex { get; set; }

        public bool ContainsTarget
        {
            get { return IsTargetRunning || TargetPendingIndex > 0; }
        }

        public ComfyQueueStatus()
        {
            TargetPendingIndex = 0;
        }
    }
}
