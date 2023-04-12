using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Threading;
using Dalamud.Game;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ECommons.Automation;
using SpeakBeaver.Windows;

namespace SpeakBeaver
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Speak Beaver";
        private const string CommandName = "/speak";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public static Configuration Configuration { get; private set; }

        public WindowSystem WindowSystem = new("SpeakBeaver");

        // 状态栏
        public static DtrBarEntry StatusEntry { get; private set; } = null!;

        // 引入Service
        public static DtrBar DtrBar { get; private set; } = null!;

        // ChatGui
        public static ChatGui ChatGui { get; private set; } = null!;

        // toast
        public static ToastGui ToastGui { get; private set; } = null!;

        //Framework
        public static Framework Framework { get; private set; } = null!;

        // 引入Chat，设置成private类型
        private static Chat chat { get; set; } = null!;
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] DtrBar dtrBar,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] ToastGui toastGui,
            [RequiredVersion("1.0")] Framework framework)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            DtrBar = dtrBar;
            ChatGui = chatGui;
            ToastGui = toastGui;
            Framework = framework;
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "beaver.png");
            var beaverImage = PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, beaverImage);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            // 初始化状态栏
            StatusEntry = DtrBar.Get(Name);
            SetStatusEntry(RecStatusConfig.RecStatus.Idle);
            // 初始化Chat
            ECommons.ECommonsMain.Init(pluginInterface, this);
            chat = new Chat();

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "打开Speak Beaver主界面"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            Framework.Update += OnFrameworkUpdate;
        }

        private void OnFrameworkUpdate(object _)
        {
            if (Speech2Text.IsRecording && !Speech2Text.MsgToSend.Equals(""))
            {
                RecStatusConfig.SendMsg(Speech2Text.MsgToSend);
                Speech2Text.MsgToSend="";
            }
        }
        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();
            StatusEntry.Dispose();
            CommandManager.RemoveHandler(CommandName);
            ECommons.ECommonsMain.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
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
                ChatGui.Print(message);
            }
        }

        // 开始录音
        public void StartSTT()
        {
            if (!Speech2Text.IsRecording)
            {
                Speech2Text.AutoStopStatus = Speech2Text.AutoStopType.SetTime;
                Speech2Text.StartSTT();
            }
        }

        // 河狸！说话！
        public static void SayBeaver(string message)
        {
            ChatGui.PrintChat(new XivChatEntry
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
    }
}
