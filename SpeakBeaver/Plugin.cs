using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Threading;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using ECommons.DalamudServices;
using SpeakBeaver.Windows;

namespace SpeakBeaver
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Speak Beaver";
        private const string CommandName = "/speak";
        private const string VoiceCommandName = "/svoice";

        private DalamudPluginInterface PluginInterface { get; init; }
        public static Configuration Configuration { get; private set; }

        public WindowSystem WindowSystem = new("SpeakBeaver");

        // 状态栏
        public static DtrBarEntry StatusEntry { get; private set; } = null!;
        public static DtrBarEntry ChannelBarEntry { get; private set; } = null!;

        // 引入Chat，设置成private类型
        private static Chat chat { get; set; } = null!;
        private ComboMainWindow ComboMainWindow { get; init; }
        // 语音控制
        public VoiceControlManager voiceControl;
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
            // 初始化Chat
            ECommons.ECommonsMain.Init(pluginInterface, this);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "beaver.png");
            Svc.Log.Info(imagePath);
            var beaverImage = PluginInterface.UiBuilder.LoadImage(imagePath);
            
            // 初始化语音控制
            voiceControl = new VoiceControlManager();
            
            ComboMainWindow = new ComboMainWindow(this, beaverImage);
            
            WindowSystem.AddWindow(ComboMainWindow);
            // 初始化状态栏
            StatusEntry = Svc.DtrBar.Get(Name);
            RecStatusConfig.Status = RecStatusConfig.RecStatus.Idle;
            RecStatusConfig.UpdateStatus();
            ChannelBarEntry= Svc.DtrBar.Get(Name+" Channel");
            UpdateChannelBar();
            chat = new Chat();

            Svc.Commands.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开Speak Beaver主界面"
            });
            Svc.Commands.AddHandler(VoiceCommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开语音控制界面"
            });
            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfig;
            Svc.Framework.Update += OnFrameworkUpdate;
        }

        private void OnFrameworkUpdate(object _)
        {
            if (Speech2Text.IsRecording && !Speech2Text.MsgToSend.Equals(""))
            {
                RecStatusConfig.SendMsg(Speech2Text.MsgToSend);
                Speech2Text.MsgToSend = "";
            }

            if (!VoiceControlManager.Command.Equals(""))
            {
                chat.SendMessage(VoiceControlManager.Command);
                VoiceControlManager.Command = "";
            }
            RecStatusConfig.UpdateStatus();
            UpdateChannelBar();
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            // 这句不写又会怎么样呢？
            Svc.Framework.Update -= OnFrameworkUpdate;
            ComboMainWindow.Dispose();
            StatusEntry.Dispose();
            ChannelBarEntry.Dispose();
            Svc.Commands.RemoveHandler(CommandName);
            Svc.Commands.RemoveHandler(VoiceCommandName);
            ECommons.ECommonsMain.Dispose();
            voiceControl.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            // 解析命令
            // 打开主界面
            if (command == CommandName && args.Equals(""))
            {
                ComboMainWindow.Toggle();
                return;
            }
            // 更改频道，格式/speak change <频道>
            if (command == CommandName && args.StartsWith("change"))
            {
                string[] argsList = args.Split(' ');
                if (argsList.Length == 2)
                {
                    Configuration.Channel = argsList[1];
                    Configuration.Save();
                    return;
                }
            }
            // 开始一次使用默认设置的语音识别，格式/speak start
            if (command == CommandName && args.Equals("start"))
            {
                StartSTTWithArgs();
                return;
            }
            // 开始一次使用自定义限制时长的语音识别，格式/speak limit <时长> （单位，秒）
            if (command == CommandName && args.StartsWith("limit"))
            {
                string[] argsList = args.Split(' ');
                if (argsList.Length == 2)
                {
                    if (int.TryParse(argsList[1], out int limit))
                    {
                        StartSTTWithArgs(limit);
                        return;
                    }
                }
            }
            // 开始一次无限制时长的语音识别，格式/speak unlimited
            if (command == CommandName && args.Equals("unlimited"))
            {
                StartSTTWithArgs(Speech2Text.AutoStopType.NoStop);
                return;
            }
            // 停止语音识别，格式/speak stop
            if (command == CommandName && args.Equals("stop"))
            {
                Speech2Text.Stop();
                return;
            }
            // 语音识别开启
            if (command == VoiceCommandName && args.Equals("start"))
            {
                voiceControl.Start();
                Configuration.EnableVoiceControl = true;
                Configuration.Save();
                return;
            }
            // 语音识别关闭
            if (command == VoiceCommandName && args.Equals("stop"))
            {
                voiceControl.Stop();
                Configuration.EnableVoiceControl = false;
                Configuration.Save();
                return;
            }

        }
        // 将各个命令及其帮助写成字典
        public static Dictionary<string, string> CommandHelp = new Dictionary<string, string>()
        {
            {"/speak","打开主界面"},
            {"/speak start","开始一次使用设置中参数的的语音识别"},
            {"/speak limit <时长>","开始一次使用自定义限制时长的语音识别，\n需将<时长>改为数字，单位秒\n例如：/speak limit 20"},
            {"/speak unlimited","开始一次无限制时长的语音识别"},
            {"/speak stop","停止语音识别"},
            {"/speak change <频道>","更改频道，将频道改为频道名\n例如：/speak change 小队"},
            {"/svoice","打开语音控制界面"},
            {"/svoice start","开启语音控制"},
            {"/svoice stop","关闭语音控制"}
        };

        public static string QolbarSetting =
            "H4sIAAAAAAAACqtWUimpLEhVslJKVdJRSjJSsqqGiwB5Okp5QEZwQWpitlNqYllqEVCk2EfJKhqhqhimyiM/F2RGMpCpXwzSAeRk+gPVGugZ6ABxLJAbpGQF4iklBxcAJSx0TGJrdbAY9XRD/8sZ81EMU0jOSMxLT1WAS5Fr9J6Gp8u7UY0uLkksKqHAyMY5z9YuQjcyv4B8E1+s2/d87zo0/+fnpWWmE2cmUDgpHCisowQSQqgtRtiqVAY02kjPWM9Iz0CpFgD9o8HeBwIAAA==";
        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        private void DrawConfig()
        {
            ComboMainWindow.Toggle();
        }

        // 发送消息，接受的第二个参数决定了发送消息的方式
        public static void SendChatMessage(string message, bool toServer)
        {
            if (toServer)
            {
                chat.SendMessage(message);
            }
            else
            {
                Svc.Chat.Print(message);
            }
        }

        // 开始录音，并且进行限时设置
        // 这段逻辑是真的混乱啊
        // 这个重载包括了两种可能，如果传入time，就用time，如果没传入time，就默认
        public void StartSTTWithArgs(int time,Speech2Text.AutoStopType type=Speech2Text.AutoStopType.PassTime)
        {
            if (!Speech2Text.IsRecording)
            {
                Speech2Text.AutoStopStatus = type;
                Speech2Text.AutoStopTime = time;
                Speech2Text.StartSTT();
            }
        }
        // 这个重载包括了两种可能，如果传入type，就用type，如果没传入type，就默认
        public void StartSTTWithArgs(Speech2Text.AutoStopType type = Speech2Text.AutoStopType.SetTime)
        {
            if (!Speech2Text.IsRecording)
            {
                Speech2Text.AutoStopStatus = type;
                Speech2Text.StartSTT();
            }
        }

        // 河狸！说话！
        public static void SayBeaver(string message)
        {
            Svc.Chat.Print(new XivChatEntry
            {
                Message = message,
                Name = "大河狸",
                Type = XivChatType.NPCDialogue
            });
        }

        // 根据当前状态，设置状态栏的值
        public static void SetStatusEntry(RecStatusConfig.RecStatus status)
        {
            // 判断当前状态
            switch (status)
            {
                case RecStatusConfig.RecStatus.Idle:
                    // 空闲状态
                    StatusEntry.Text = new SeString(new IconPayload(BitmapFontIcon.AutoTranslateBegin),
                                                    new TextPayload("空闲"),
                                                    new IconPayload(BitmapFontIcon.AutoTranslateEnd));
                    break;
                case RecStatusConfig.RecStatus.Recording:
                    // 录音状态
                    StatusEntry.Text = new SeString(new IconPayload(BitmapFontIcon.AutoTranslateBegin),
                                                    new IconPayload(BitmapFontIcon.Recording), new TextPayload("录音中"),
                                                    new IconPayload(BitmapFontIcon.AutoTranslateEnd));
                    break;
                case RecStatusConfig.RecStatus.Connecting:
                    // 识别状态
                    StatusEntry.Text = new SeString(new IconPayload(BitmapFontIcon.AutoTranslateBegin),
                                                    new TextPayload("连接中..."),
                                                    new IconPayload(BitmapFontIcon.AutoTranslateEnd));
                    break;
            }
        }

        public static void UpdateChannelBar()
        {
            ChannelBarEntry.Text = $"→{Configuration.Channel}";
        }
    }
}
