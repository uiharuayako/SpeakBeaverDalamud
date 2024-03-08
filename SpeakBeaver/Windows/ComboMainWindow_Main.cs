using System;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using Microsoft.VisualBasic.Logging;

namespace SpeakBeaver.Windows;

public partial class ComboMainWindow : Window, IDisposable
{
    private IDalamudTextureWrap beaverImage;
    private Plugin plugin;

    public ComboMainWindow(Plugin plugin, IDalamudTextureWrap beaverImage) : base(
        "Speak Beaver")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.beaverImage = beaverImage;
        this.plugin = plugin;
    }

    public void Dispose()
    {
        Speech2Text.Dispose();
        beaverImage.Dispose();
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("Speak Beaver Windows"))
        {
            if (ImGui.BeginTabItem("语音输入"))
            {
                DrawMain();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("语音控制"))
            {
                DrawVoice(plugin.voiceControl);
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("设置"))
            {
                DrawConfig();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        ImGui.End();
    }

    public void DrawMain()
    {
        ImGui.Image(beaverImage.ImGuiHandle, new Vector2(32, 32));
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DPSRed, $"大河狸会替代你说话，快说");
        ImGui.SameLine();
        if (ImGui.Button("“谢谢大河狸”！"))
        {
            Svc.Chat.Print(new XivChatEntry()
            {
                Message = new SeString(new TextPayload("嗷嗷，嗷，嗷嗷嗷！"),
                                       new IconPayload(BitmapFontIcon.AutoTranslateBegin),
                                       new TextPayload("谢谢大河狸！"), new IconPayload(BitmapFontIcon.AutoTranslateEnd)
                ),
                Name =
                    $"大河狸（曾经是{Svc.ClientState.LocalPlayer.Name}@{Svc.ClientState.LocalPlayer.HomeWorld.GameData.Name}）",
                Type = XivChatType.NPCDialogue
            });
        }

        if (ImGui.Button("开始语音输入"))
        {
            plugin.StartSTTWithArgs();
        }

        ImGui.SameLine();
        // 快捷复制qolbar设置
        if (ImGui.Button("快捷复制qolbar设置"))
        {
            ImGui.SetClipboardText(Plugin.QolbarSetting);
        }

        ImGui.Spacing();
        // 列出各项命令及其功能
        if (ImGui.CollapsingHeader("命令列表"))
        {
            // 两列的列表
            int i = 0;
            ImGui.Columns(2);
            foreach (var (cmd, help) in Plugin.CommandHelp)
            {
                i++;
                Vector4 color = i % 2 == 0 ? ImGuiColors.DPSRed : ImGuiColors.TankBlue;
                ImGui.TextColored(color, $"命令：{cmd} ");
                ImGui.NextColumn();
                ImGui.TextColored(color, help);
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        // 关于本插件
        if (ImGui.CollapsingHeader("关于"))
        {
            // 设置一个button用来打开仓库网址
            if (ImGui.Button("插件主页"))
            {
                Dalamud.Utility.Util.OpenLink("https://github.com/uiharuayako/SpeakBeaverDalamud");
            }

            ImGui.SameLine();
            if (ImGui.Button("Uiharu的插件仓库"))
            {
                Dalamud.Utility.Util.OpenLink("https://github.com/uiharuayako/DalamudPlugins");
            }

            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudViolet, "第一次使用？点击插件主页查看使(bai)用(piao)指南！");
        }
#if DEBUG
            if (ImGui.Button("测试用按钮"))
        {
            // Plugin.ChatGui.PrintChat(new XivChatEntry()
            // {
            //     Message = new SeString(
            //         new IconPayload(BitmapFontIcon.AutoTranslateBegin),
            //         new TextPayload("测试")
            //     ),
            //     Name = "测试",
            //     Type = XivChatType.Say
            // });
            Plugin.SendChatMessage("/s nihao", true);
        }
#endif
    }
    internal static void Spacing(byte count = 1)
    {
        string s = string.Empty;
        for (int i = 0; i < count; i++)
        {
            s += "    ";
        }
        ImGui.Text(s);
        ImGui.SameLine();
    }
}
