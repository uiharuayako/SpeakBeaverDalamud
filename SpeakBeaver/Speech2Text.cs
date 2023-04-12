using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using Dalamud.Game.Text;
using WebSocket4Net;

namespace SpeakBeaver
{
    // 语音转文字，静态类，同时只能有一个语音转文字进程运行
    public static class Speech2Text
    {
        public static void Dispose()
        {
            if (webSocket != null)
            {
                webSocket.Close();
                webSocket.Dispose();
            }
        }

        // 线程安全？？
        public static volatile string MsgToSend = "";

        // NAudio Recorder
        private static WaveInEvent recorder;

        // 讯飞API限制用这个format
        private static WaveFormat format = new WaveFormat(16000, 16, 1);

        // 音频帧状态
        public enum VoiceStatus
        {
            FirstFrame = 0,
            ContinueFrame = 1,
            LastFrame = 2
        }

        // 获取当前音频输入设备字典
        public static Dictionary<int, string> GetInputDevices()
        {
            Dictionary<int, string> devices = new Dictionary<int, string>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                devices.Add(i, WaveIn.GetCapabilities(i).ProductName);
            }

            return devices;
        }

        // 记录音频输入设备的number
        public static int SelectedDevice = 0;

        public static volatile VoiceStatus status = VoiceStatus.FirstFrame;

        // 原生WebSocket好麻烦uwu
        private static WebSocket webSocket;

        // 判断当前是否处于录音状态
        public static bool IsRecording = false;

        // 开启一次语音转文字
        public static void StartSTT()
        {
            RecStatusConfig.OnConnecting();
            IsRecording = true;
            // 创建录音设备
            recorder = new WaveInEvent()
            {
                DeviceNumber = SelectedDevice,
                WaveFormat = format
            };
            recorder.DataAvailable += OnDataAvailable;
            recorder.RecordingStopped += OnRecordingStopped;
            // 重置音频帧状态
            status = VoiceStatus.FirstFrame;
            // 创建WebSocket
            // 验证uri的各项参数是否合法
            string uri = GetAuthUrl(Plugin.Configuration.HostUrl, Plugin.Configuration.ApiKey,
                                    Plugin.Configuration.ApiSecret);

            webSocket = new WebSocket(uri);
            // 设置WebSocket事件
            webSocket.Opened += OnOpened;
            webSocket.DataReceived += OnDataReceived;
            webSocket.Closed += OnClosed;
            webSocket.Error += OnError;
            webSocket.MessageReceived += OnMessageReceived;
            // 开启WebSocket
            webSocket.Open();
        }


        private static void OnRecordingStopped(object sender, EventArgs e)
        {
            status = VoiceStatus.LastFrame;
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：录制停止");
        }

        private static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            switch (status)
            {
                case VoiceStatus.FirstFrame:
                {
                    dynamic frame = new JObject();
                    frame.common = new JObject
                    {
                        { "app_id", Plugin.Configuration.AppID }
                    };
                    frame.business = new JObject
                    {
                        { "language", "zh_cn" },
                        { "domain", "iat" },
                        { "accent", "mandarin" },
                        { "nbest", 3 },
                        { "vad_eos", Plugin.Configuration.NoSpeakTime },
                        { "ptt", Plugin.Configuration.AutoPunctuation ? 1 : 0 }
                    };
                    frame.data = new JObject
                    {
                        { "status", (int)VoiceStatus.FirstFrame },
                        { "format", "audio/L16;rate=16000" },
                        { "encoding", "raw" },
                        { "audio", Convert.ToBase64String(e.Buffer) }
                    };
                    webSocket.Send(frame.ToString());
                    status = VoiceStatus.ContinueFrame;
                }
                    break;
                case VoiceStatus.ContinueFrame:
                {
                    dynamic frame = new JObject();
                    frame.data = new JObject
                    {
                        { "status", (int)VoiceStatus.ContinueFrame },
                        { "format", "audio/L16;rate=16000" },
                        { "encoding", "raw" },
                        { "audio", Convert.ToBase64String(e.Buffer) }
                    };
                    webSocket.Send(frame.ToString());
                }
                    break;
                case VoiceStatus.LastFrame:
                {
                    dynamic frame = new JObject();
                    frame.data = new JObject
                    {
                        { "status", (int)VoiceStatus.LastFrame },
                        { "format", "audio/L16;rate=16000" },
                        { "encoding", "raw" },
                        { "audio", Convert.ToBase64String(e.Buffer) }
                    };
                    webSocket.Send(frame.ToString());
                    recorder.StopRecording();
                }
                    break;
                default:
                    break;
            }
        }

        private static void OnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：ws错误");
            status = VoiceStatus.LastFrame;
            Dalamud.Logging.PluginLog.Log(e.Exception.Message);
        }

        private static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：ws收到消息");
            Dalamud.Logging.PluginLog.Log(e.Message);
            dynamic msg = JsonConvert.DeserializeObject(e.Message);
            if (msg.code != 0)
            {
                Dalamud.Logging.PluginLog.Error($"error => {msg.message},sid => {msg.sid}");
                return;
            }

            var ws = msg.data.result.ws;
            if (ws == null)
            {
                return;
            }

            string sb = "";
            foreach (var item in ws)
            {
                sb += item.cw[0].w;
            }
#if DEBUG
            Dalamud.Logging.PluginLog.Log($"识别结果{sb}");
#endif
            MsgToSend = sb;
            if (msg.data.status == 2)
            {
                Dalamud.Logging.PluginLog.Log("识别结束");
                webSocket.Close();
            }
        }

        private static void OnClosed(object sender, EventArgs e)
        {
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：本次ws连接关闭");
            if (IsRecording)
            {
                RecStatusConfig.OnEnd();
            }

            IsRecording = false;
        }

        private static void OnDataReceived(object sender, EventArgs e)
        {
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：收到ws返回信息");
        }

        private static void OnOpened(object sender, EventArgs e)
        {
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：ws连接开启，尝试开启录音");
            recorder.StartRecording();
            RecStatusConfig.OnStart();
            // 设置自动停止
            AutoStop(AutoStopStatus);
        }

        private static String GetAuthUrl(String hostUrl, String apiKey, String apiSecret)
        {
            Uri url = new Uri(hostUrl);
            string date = DateTime.Now.ToString("R");
            string requestLine = $"GET {url.AbsolutePath} HTTP/1.1";
            string signatureOrigin = $"host: {url.Host}\ndate: {date}\n{requestLine}";
            HMAC hmac = HMAC.Create("System.Security.Cryptography.HMACSHA256");
            hmac.Key = Encoding.UTF8.GetBytes(apiSecret);
            var sigBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signatureOrigin));
            string signature = Convert.ToBase64String(sigBytes);
            string authorizationOrigin =
                $@"api_key=""{apiKey}"",algorithm=""hmac-sha256"",headers=""host date request-line"",signature=""{signature}""";
            string authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationOrigin));
            UriBuilder builder = new UriBuilder()
            {
                Scheme = "wss",
                Host = url.Host,
                Path = url.AbsolutePath,
                Query =
                    $"authorization={authorization}&date={HttpUtility.UrlEncode(date).Replace("+", "%20")}&host={url.Host}",
            };
            return builder.ToString();
        }

        // 自动停止转写，停止方式：设定一个Timer，在指定时间后，将音频帧设置为LastFrame
        private static Timer timer;

        // 设置自动停止的方式，有三种：1.按照设置的时间停止 2.按照传入的时间停止 3.不自动停止
        public enum AutoStopType
        {
            // 按照设置的时间停止
            SetTime = 0,

            // 按照传入的时间停止
            PassTime = 1,

            // 不自动停止
            NoStop = 2
        }

        public static AutoStopType AutoStopStatus = AutoStopType.SetTime;
        public static int AutoStopTime = Plugin.Configuration.AutoDisconnectTime;

        private static void AutoStop(AutoStopType type)
        {
            switch (type)
            {
                case AutoStopType.SetTime:
                    // 如果设置了自动停止，且设置了停止时间
                    if (Plugin.Configuration.AutoDisconnectTime > 0 && Plugin.Configuration.AutoDisconnect)
                    {
                        Plugin.SayBeaver($"本次输入最大时间{Plugin.Configuration.AutoDisconnectTime}秒");
                        AutoStop(Plugin.Configuration.AutoDisconnectTime);
                    }

                    break;
                case AutoStopType.PassTime:
                    // 以设定的时间
                    Plugin.SayBeaver($"本次输入最大时间{AutoStopTime}秒");
                    AutoStop(AutoStopTime);
                    break;
                case AutoStopType.NoStop:
                    break;
                default:
                    break;
            }
        }

        // 单位，秒
        private static void AutoStop(int stopTime)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }

            // 初始化一个timer
            timer = new Timer(stopTime * 1000);
            // 只执行一次
            timer.AutoReset = false;
            timer.Elapsed += autoChangeStatus;
            timer.Start();
        }

        private static void autoChangeStatus(object sender, ElapsedEventArgs e)
        {
            if (status == VoiceStatus.LastFrame) return;
            status = VoiceStatus.LastFrame;
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：自动停止转写");
        }

        public static void Stop()
        {
            status = VoiceStatus.LastFrame;
            Dalamud.Logging.PluginLog.Log("SpeakBeaver：手动停止");
        }
    }
}
