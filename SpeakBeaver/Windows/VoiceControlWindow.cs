using System;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using Microsoft.VisualBasic.Logging;

namespace SpeakBeaver.Windows;

public class VoiceControlWindow : Window, IDisposable
{
    private VoiceControlManager voiceControl;
    public VoiceControlWindow(Plugin plugin) : base(
        "Speak Beaver语音控制", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        voiceControl = plugin.voiceControl;
    }

    public void Dispose()
    {

    }

    public override void Draw()
    {
        // 语音控制界面，让用户自定义命令
        // 添加命令按钮
        if (ImGui.Button("添加命令"))
        {
            // 添加命令
            Plugin.Configuration.VoiceControl.Add("语音关键词", "命令");
        }
        // 设置一个列表，有三列，第一列是关键词，第二列是命令，第三列是一个用于删除当前关键词的按钮
        ImGui.Columns(3, "语音控制列表", false);
        ImGui.SetColumnWidth(0, 200);
        ImGui.SetColumnWidth(1, 200);
        ImGui.SetColumnWidth(2, 50);
        ImGui.Text("语音关键词");
        ImGui.NextColumn();
        ImGui.Text("命令");
        ImGui.NextColumn();
        ImGui.Text("删除");
        ImGui.NextColumn();
        foreach (var (keyWord, command) in Plugin.Configuration.VoiceControl)
        {
            var keyWordCopy = keyWord;
            if (ImGui.InputText($"##语音关键词{keyWordCopy}", ref keyWordCopy, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                // 修改关键词
                Plugin.Configuration.VoiceControl.Remove(keyWord);
                Plugin.Configuration.VoiceControl.Add(keyWordCopy, command);
                Plugin.Configuration.Save();
            }
            ImGui.NextColumn();
            var commandCopy = command;
            if (ImGui.InputText($"##命令{keyWordCopy}", ref commandCopy, 200))
            {
                // 修改命令
                Plugin.Configuration.VoiceControl.Remove(keyWord);
                Plugin.Configuration.VoiceControl.Add(keyWord, commandCopy);
                Plugin.Configuration.Save();
            }
            ImGui.NextColumn();
            if (ImGui.Button($"删除##{keyWordCopy}"))
            {
                // 删除关键词
                Plugin.Configuration.VoiceControl.Remove(keyWord);
                Plugin.Configuration.Save();
            }
            ImGui.NextColumn();
        }
        ImGui.Columns(1);
        // 更新识别引擎语法
        if (ImGui.Button("更新识别关键词"))
        {
            // 更新识别引擎语法
            voiceControl.UpdateGrammar();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("更新识别引擎语法，每五秒只能更新一次");
        }
    }
}
