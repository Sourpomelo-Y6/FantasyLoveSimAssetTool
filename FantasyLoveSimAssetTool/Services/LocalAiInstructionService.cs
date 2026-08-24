using System;
using System.IO;

namespace FantasyLoveSimAssetTool.Services
{
    public sealed class LocalAiInstructionService
    {
        public const string DefaultInstruction =
            "恋愛ゲーム用の自然な日本語を書いてください。\n" +
            "キャラクターの口調、一人称、二人称を守ってください。\n" +
            "説明を付けず、指定されたJSONだけを出力してください。";

        private readonly string workspaceRoot;

        public LocalAiInstructionService(string workspaceRoot)
        {
            if (string.IsNullOrWhiteSpace(workspaceRoot))
                throw new ArgumentException("Workspace root is required.", nameof(workspaceRoot));
            this.workspaceRoot = workspaceRoot;
        }

        public string InstructionPath => Path.Combine(workspaceRoot, "LocalAISettings", "base-instruction.txt");

        public string Load()
        {
            if (!File.Exists(InstructionPath)) return DefaultInstruction;
            string text = File.ReadAllText(InstructionPath).Trim();
            return string.IsNullOrWhiteSpace(text) ? DefaultInstruction : text;
        }

        public void Save(string instruction)
        {
            string text = string.IsNullOrWhiteSpace(instruction) ? DefaultInstruction : instruction.Trim();
            Directory.CreateDirectory(Path.GetDirectoryName(InstructionPath));
            File.WriteAllText(InstructionPath, text);
        }
    }
}
