using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Threading;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
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

        // 引入Service
        [PluginService]
        [RequiredVersion("1.0")]
        public static DtrBar DtrBar { get; private set; } = null!;

        // ChatGui
        public static ChatGui ChatGui { get; private set; } = null!;

        // 引入Chat，设置成private类型
        private static Chat chat { get; set; } = null!;
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] DtrBar dtrBar,
            [RequiredVersion("1.0")] ChatGui chatGui)
        {
            PluginInterface = pluginInterface;
            CommandManager = commandManager;
            DtrBar = dtrBar;
            ChatGui = chatGui;
            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "beaver.png");
            var beaverImage = PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, beaverImage);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            // 初始化Chat
            ECommons.ECommonsMain.Init(pluginInterface, this);
            chat = new Chat();

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();

            ConfigWindow.Dispose();
            MainWindow.Dispose();

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
                Speech2Text.StartSTT();
            }
        }
        // 河狸！说话！
        public static XivChatEntry SayBeaver(string message)
        {
            return new XivChatEntry
            {
                Message = message,
                Name = "大河狸",
                Type = XivChatType.NPCDialogue
            };
        }
    }
}
