using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SpeakBeaver.Windows;

public class ConfigWindow : Window, IDisposable
{
    public ConfigWindow(Plugin plugin) : base(
        "Speak Beaver设置",
        ImGuiWindowFlags.NoCollapse) { }

    public void Dispose() { }

    public override void Draw()
    {
        var selectedDevice = Speech2Text.SelectedDevice;
        // 选择音频设备
        if (ImGui.Combo("选择音频设备", ref selectedDevice, Speech2Text.GetInputDevices().Values.ToArray(),
                        Speech2Text.GetInputDevices().Count))
        {
            Speech2Text.SelectedDevice = selectedDevice;
        }

        // 设置开始时的提示语
        var startText = Plugin.Configuration.StartMessage;
        if (ImGui.InputText("输入开始", ref startText, 200))
        {
            Plugin.Configuration.StartMessage = startText;
            Plugin.Configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "当可以开始说话时，向聊天框发送这个命令");
        }

        ImGui.SameLine();
        // 测试按钮
        if (ImGui.Button("测试##开始"))
        {
            Plugin.SendChatMessage(Plugin.Configuration.StartMessage, true);
        }

        // 设置结束时的提示语
        var endText = Plugin.Configuration.EndMessage;
        if (ImGui.InputText("输入结束", ref endText, 200))
        {
            Plugin.Configuration.EndMessage = endText;
            Plugin.Configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "当语音转写结束时，向聊天框发送这个命令");
        }

        ImGui.SameLine();
        // 测试按钮
        if (ImGui.Button("测试##结束"))
        {
            Plugin.SendChatMessage(Plugin.Configuration.EndMessage, true);
        }
        // 设置停止词
        var closeWord = Plugin.Configuration.CloseWord;
        if (ImGui.InputText("输入停止词", ref closeWord, 200))
        {
            Plugin.Configuration.CloseWord = closeWord;
            Plugin.Configuration.Save();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("当语音转写结果包括这个词时，停止语音转写\n建议在讯飞控制台的热词里添加这个词以提高识别准确度\n当设置为空时，关闭这个功能");
        }
        if (ImGui.CollapsingHeader("频道设置"))
        {
            // 设置一个button用来添加频道
            if (ImGui.Button("添加频道"))
            {
                // 添加一个频道
                Plugin.Configuration.ChannelDictionary.Add("新频道", "/xx ");
                Plugin.Configuration.Save();

            }
            // 设定一个三列的表格，用于存放频道设置
            ImGui.Columns(4, "频道设置", false);
            ImGui.SetColumnWidth(0, 40);
            ImGui.SetColumnWidth(1, 210);
            ImGui.SetColumnWidth(2, 110);
            ImGui.Text("启用");
            ImGui.NextColumn();
            ImGui.Text("频道名称");
            ImGui.NextColumn();
            ImGui.Text("前缀");
            ImGui.NextColumn();
            ImGui.Text("删除");
            ImGui.NextColumn();
            // 遍历频道字典
            foreach (var (name, cmd) in Plugin.Configuration.ChannelDictionary)
            {
                // 一个Checkbox用于判断当前选中的频道
                bool currentChannel = name == Plugin.Configuration.Channel;
                if (ImGui.Checkbox($"##{name}", ref currentChannel))
                {
                    // 如果选中的频道改变，就把改变后的频道存入配置文件
                    Plugin.Configuration.Channel = name;
                    Plugin.Configuration.Save();
                    Plugin.UpdateChannelBar();
                }

                ImGui.NextColumn();
                // 一个文本框用于存放频道名称
                var channelName = name;
                if (ImGui.InputText($"名称##{name}", ref channelName, 200,ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    // 如果文本框内容改变，就把改变后的内容存入字典
                    Plugin.Configuration.ChannelDictionary.Remove(name);
                    Plugin.Configuration.ChannelDictionary.Add(channelName, cmd);
                    Plugin.Configuration.Save();
                }

                ImGui.NextColumn();
                // 一个文本框用于存放频道命令
                var channelCmd = cmd;
                if (ImGui.InputText($"前缀##{name}", ref channelCmd, 20))
                {
                    // 如果文本框内容改变，就把改变后的内容存入字典
                    Plugin.Configuration.ChannelDictionary[channelName] = channelCmd;
                    Plugin.Configuration.Save();
                }

                ImGui.NextColumn();
                // 一个按钮用于删除当前频道
                if (ImGui.Button($"删除##{name}"))
                {
                    // 如果点击了删除按钮，就把当前频道从字典中移除
                    Plugin.Configuration.ChannelDictionary.Remove(name);
                    Plugin.Configuration.Save();
                    Plugin.UpdateChannelBar();
                }
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        if (ImGui.CollapsingHeader("替换词"))
        {
            // 设置一个button用于添加
            if (ImGui.Button("添加##替换词"))
            {
                Plugin.Configuration.ReplaceDict.Add("原词", "替换词");
            }
            // 建立一个两列的列表，让用户编辑和添加替换词
            ImGui.Columns(3, "替换词", false);
            ImGui.SetColumnWidth(0, 210);
            ImGui.SetColumnWidth(1, 210);
            ImGui.Text("原字符串");
            ImGui.NextColumn();
            ImGui.Text("替换字符串");
            ImGui.NextColumn();
            ImGui.Text("删除");
            ImGui.NextColumn();

            // 遍历替换词字典
            foreach (var (oriStr,newStr) in Plugin.Configuration.ReplaceDict)
            {
                var oriStrInput = oriStr;
                if (ImGui.InputText($"原字符串##{oriStr}", ref oriStrInput, 200, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    // 如果原字符串改变，就把改变后的内容存入字典
                    Plugin.Configuration.ReplaceDict.Remove(oriStr);
                    Plugin.Configuration.ReplaceDict.Add(oriStrInput, newStr);
                    Plugin.Configuration.Save();
                }
                ImGui.NextColumn();
                var newStrInput = newStr;
                if (ImGui.InputText($"替换字符串##{oriStr}", ref newStrInput, 200))
                {
                    // 如果替换字符串改变，就把改变后的内容存入字典
                    Plugin.Configuration.ReplaceDict[oriStrInput] = newStrInput;
                    Plugin.Configuration.Save();
                }
                ImGui.NextColumn();
                // 一个按钮用于删除当前替换词
                if (ImGui.Button($"删除##{oriStr}"))
                {
                    // 如果点击了删除按钮，就把当前替换词从字典中移除
                    Plugin.Configuration.ReplaceDict.Remove(oriStr);
                    Plugin.Configuration.Save();
                }
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
        }

        // 折叠框
        if (ImGui.CollapsingHeader("讯飞Api设置"))
        {
            // 输入讯飞API相关
            var hostUrl = Plugin.Configuration.HostUrl;
            if (ImGui.InputText("url", ref hostUrl, 200))
            {
                Plugin.Configuration.HostUrl = hostUrl;
                Plugin.Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("默认为中英文常用语识别，一般不需要改");
            }

            var appID = Plugin.Configuration.AppID;
            if (ImGui.InputText("AppID", ref appID, 200))
            {
                Plugin.Configuration.AppID = appID;
                Plugin.Configuration.Save();
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(
                    "还没申请讯飞Api？前往插件主页查看白嫖指南！");
            }
            var apiSecret = Plugin.Configuration.ApiSecret;
            if (ImGui.InputText("ApiSecret", ref apiSecret, 200))
            {
                Plugin.Configuration.ApiSecret = apiSecret;
                Plugin.Configuration.Save();
            }

            var apiKey = Plugin.Configuration.ApiKey;
            if (ImGui.InputText("ApiKey", ref apiKey, 200))
            {
                Plugin.Configuration.ApiKey = apiKey;
                Plugin.Configuration.Save();
            }

            ImGui.Separator();
            // 选择是否加标点
            var autoPunctuation = Plugin.Configuration.AutoPunctuation;
            if (ImGui.Checkbox("自动加标点", ref autoPunctuation))
            {
                Plugin.Configuration.AutoPunctuation = autoPunctuation;
                Plugin.Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(
                    "这个功能似乎有问题，可以研究一下讯飞的API");
            }

            ImGui.SameLine();
            // 设置自动中断时长
            var autoDisconnect = Plugin.Configuration.AutoDisconnect;
            if (ImGui.Checkbox("自动停止转写", ref autoDisconnect))
            {
                Plugin.Configuration.AutoDisconnect = autoDisconnect;
                Plugin.Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(
                    "受讯飞API限制，即使关闭此项，每次语音转写的时长最大也会为60秒\n也就是说，当开启了一次语音转写，到了60秒，不管是否还在说话\n识别都将强行结束，需要重新开启");
            }

            var autoDisconnectTime = Plugin.Configuration.AutoDisconnectTime;
            if (ImGui.InputInt("自动停止转写时长(s)", ref autoDisconnectTime, 5000))
            {
                Plugin.Configuration.AutoDisconnectTime = autoDisconnectTime;
                Plugin.Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("当开启转写，在一定时间后停止转写，因为上述原因，设置值大于60时无效");
            }

            var noSpeakTime = Plugin.Configuration.NoSpeakTime;
            if (ImGui.InputInt("说话间隔(ms)", ref noSpeakTime, 500))
            {
                Plugin.Configuration.NoSpeakTime = noSpeakTime;
                Plugin.Configuration.Save();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("当说完一句话，在此项设置的时长内，\n如果没有继续说话，转写将会停止");
            }

            ImGui.Separator();
            // 设置语音转写的候选句个数（1-5）
            var maxAlternatives = Plugin.Configuration.MaxAlternatives;
            if (ImGui.InputInt("候选句个数", ref maxAlternatives, 1))
            {
                // 设置范围1-5
                if (maxAlternatives > 5) maxAlternatives = 5;
                if (maxAlternatives < 1) maxAlternatives = 1;
                Plugin.Configuration.MaxAlternatives = maxAlternatives;
                Plugin.Configuration.Save();
            }
        }
    }
}
