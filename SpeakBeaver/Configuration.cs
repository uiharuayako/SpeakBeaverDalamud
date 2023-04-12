using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace SpeakBeaver
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        // 保存讯飞API相关
        public string HostUrl= "http://iat-api.xfyun.cn/v2/iat";
        public string AppID = "你的AppID";
        public string ApiSecret = "你的ApiSecret";
        public string ApiKey = "你的ApiKey";
        // 没有说话多少毫秒后中断连接
        public int NoSpeakTime = 10000;
        // 默认最长说话时间，单位毫秒
        public bool AutoDisconnect = false;
        public int AutoDisconnectTime = 20000;
        // 设置是否自动加标点
        public bool AutoPunctuation = false;
        // 设置候选句个数
        public int MaxAlternatives = 1;
        // 当语音转写开始时，发送的提示语
        public string StartMessage = "/e 开始语音转写<se.3>";
        // 当语音转写结束时，发送的提示语
        public string EndMessage = "/e 语音转写结束<se.4>";
        // 设置替换关系
        public Dictionary<string,string> ReplaceDict = new Dictionary<string, string>()
        {
            {"第一","D1"},
            {"第二","D2"},
            {"第三","D3"},
            {"第四","D4"},
            {"第五","D5"},
        };

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginInterface!.SavePluginConfig(this);
        }
    }
}