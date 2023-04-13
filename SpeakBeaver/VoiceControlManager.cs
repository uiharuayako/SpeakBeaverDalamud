using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SpeakBeaver
{
    // 调用.net的语音识别库，完成功能：
    // 当用户说出指定的关键词时，执行指定命令
    public class VoiceControlManager : IDisposable
    {
        // 线程安全的执行命令
        public static volatile string Command = "";
        // 禁止过于频繁的调用

        private bool canCall = true;
        private Timer timer;
        private SpeechRecognitionEngine recEngine;

        public VoiceControlManager()
        {
            InitEngine();
        }

        public void InitEngine()
        {
            // 初始化引擎
            recEngine = new SpeechRecognitionEngine();
            // 从配置文件中读取关键词
            var keywords = Plugin.Configuration.VoiceControl.Keys.ToArray();
            // 创建语法
            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(new Choices(keywords));
            var grammar = new Grammar(grammarBuilder);
            // 将语法添加到语音识别引擎
            recEngine.LoadGrammar(grammar);
            // 设置语音识别引擎的语言
            recEngine.SetInputToDefaultAudioDevice();
            // 设置语音识别引擎的事件
            recEngine.SpeechRecognized += RecEngine_SpeechRecognized;
            // 开始识别
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
        }
        // 更新语法
        public void UpdateGrammar()
        {
            Plugin.SayBeaver("正在为您刷新语音识别关键词");
            if (!canCall) {return;}
            canCall = false;
            timer = new Timer(5000);

            timer.Elapsed += OnTimerElapsed;
            timer.AutoReset = false;
            timer.Enabled = true;
            // 从配置文件中读取关键词
            var keywords = Plugin.Configuration.VoiceControl.Keys.ToArray();
            // 创建语法
            var grammarBuilder = new GrammarBuilder();
            grammarBuilder.Append(new Choices(keywords));
            var grammar = new Grammar(grammarBuilder);
            // 重启Engine
            recEngine.RecognizeAsyncStop();
            // 卸载当前的语法
            recEngine.UnloadAllGrammars();
            // 将语法添加到语音识别引擎
            recEngine.LoadGrammar(grammar);
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            canCall = true;
            timer.Dispose();
        }
        private void RecEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // 当用户说出关键词时，执行指定命令
            var command = Plugin.Configuration.VoiceControl[e.Result.Text];
            // 并不是直接执行，而是将命令写入同步的变量里
            Command = command;
        }

        public void Start()
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            recEngine.RecognizeAsyncStop();
        }
        public void Dispose()
        {
            recEngine.Dispose();
        }
    }
}
