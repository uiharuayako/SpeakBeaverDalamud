using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace SpeakBeaver
{
    // 这个类用于管理插件运行的状态
    // 插件运行时有如下状态：
    // 1.未开始录音
    // 2.用户已点击录音按钮，但并未建立ws连接
    // 3.已建立ws连接，正在录音
    // 当状态变更时，程序需要做出如下反应
    // 1.当状态变化为1-2,2-3,3-1时，更新状态栏信息
    // 2.当状态变化为1-2时，让大河狸说话
    // 3.当状态变化为2-3时，发送StartMessage和flytext，告诉用户可以说话了
    // 4.当状态变化为3-1时，发送EndMessage和flytext，告诉用户说话结束
    public static class RecStatusConfig
    {
        public enum RecStatus
        {
            Idle = 0,
            Connecting = 1,
            Recording = 2
        }
        // 将输入的字符串按照字典替换成新的字符串
        public static string Replace(string input)
        {
            var output = input;
            foreach (var (key, value) in Plugin.Configuration.ReplaceDict)
            {
                output = output.Replace(key, value);
            }
            return output;
        }


        // 按照设置发送信息
        public static void SendMsg(string msg)
        {
            // 检测停止词
            if (!Plugin.Configuration.CloseWord.Equals(""))
            {
                if (msg.Contains(Plugin.Configuration.CloseWord))
                {
                    Speech2Text.Stop();
                    return;
                }
            }
            // 执行字符串替换
            msg = Replace(msg);
            if (Plugin.Configuration.ChannelDictionary.ContainsKey(Plugin.Configuration.Channel))
            {
                var sendMsg = Plugin.Configuration.ChannelDictionary[Plugin.Configuration.Channel] + msg;
#if DEBUG
                PluginLog.Log("SendMsg: " + sendMsg);
#endif
                Plugin.SendChatMessage(sendMsg, true);
            }
            else
            {
                Plugin.SendChatMessage("/e " + msg, true);
            }
        }

        // 直接发送信息到给定频道
        public static void SendMsg(string msg, string channel)
        {
            if (Plugin.Configuration.ChannelDictionary.ContainsKey(channel))
            {
                Plugin.SendChatMessage(Plugin.Configuration.ChannelDictionary[channel] + msg, true);
            }
            else
            {
                Plugin.SendChatMessage("/e " + msg, true);
            }
        }

        // 当开始录音
        public static void OnStart()
        {
            // 发送StartMessage
            Plugin.SendChatMessage(Plugin.Configuration.StartMessage, true);
            // 发送toast
            Plugin.ToastGui.ShowNormal("开始录音");
            // 更新状态栏
            Plugin.SetStatusEntry(RecStatus.Recording);
        }

        // 当结束录音
        public static void OnEnd()
        {
            // 发送EndMessage
            Plugin.SendChatMessage(Plugin.Configuration.EndMessage, true);
            // 发送toast
            Plugin.ToastGui.ShowNormal("结束录音");
            // 更新状态栏
            Plugin.SetStatusEntry(RecStatus.Idle);
        }

        // 当开始连接，但并未录音
        public static void OnConnecting()
        {
            // 发送toast
            Plugin.ToastGui.ShowNormal("正在连接...请等待提示后开始说话");
            // 更新状态栏
            Plugin.SetStatusEntry(RecStatus.Connecting);
        }
    }
}
