using System;
using System.Numerics;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using Lumina.Excel.GeneratedSheets;
using Microsoft.VisualBasic.Logging;

namespace SpeakBeaver.Windows;

public class MainWindow : Window, IDisposable
{
    private TextureWrap beaverImage;
    private Plugin plugin;

    public MainWindow(Plugin plugin, TextureWrap beaverImage) : base(
        "Speak Beaver", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
        ImGui.Text($"大河狸会替代你说话，快说“谢谢大河狸”！");

        if (ImGui.Button("设置"))
        {
            plugin.DrawConfigUI();
        }

        ImGui.SameLine();
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

        if (ImGui.Button("输入"))
        {
            plugin.StartSTT();
        }

        ImGui.Spacing();

        ImGui.Text("Have a beaver:");
        ImGui.Indent(55);
        ImGui.Image(beaverImage.ImGuiHandle, new Vector2(beaverImage.Width, beaverImage.Height));
        ImGui.Unindent(55);
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
            Plugin.SendChatMessage("/s nihao",true);
        }
    }
#endif
}

