namespace FantasyLoveSimAssetTool.Models
{
    public class ComfyProgressUpdate
    {
        public string EventType { get; set; }

        public string PromptId { get; set; }

        public string NodeId { get; set; }

        public int Value { get; set; }

        public int Max { get; set; }

        public bool IsCompleted { get; set; }

        public string Summary
        {
            get
            {
                if (EventType == "progress" && Max > 0)
                {
                    return $"WebSocket: node {NodeId} / sampler {Value}/{Max}";
                }

                if (EventType == "executing" && IsCompleted)
                {
                    return "WebSocket: 実行完了イベントを受信しました。";
                }

                if (EventType == "executing" && !string.IsNullOrWhiteSpace(NodeId))
                {
                    return $"WebSocket: node {NodeId} を実行中です。";
                }

                if (!string.IsNullOrWhiteSpace(EventType))
                {
                    return $"WebSocket: {EventType} を受信しました。";
                }

                return string.Empty;
            }
        }

        public ComfyProgressUpdate()
        {
            EventType = string.Empty;
            PromptId = string.Empty;
            NodeId = string.Empty;
        }
    }
}
