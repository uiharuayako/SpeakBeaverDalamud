# SpeakBeaver 语音大河狸

这是一个让FF14支持语音输入的程序。和[Python版](https://github.com/uiharuayako/SpeakBeaver)原理相同但更为方便快捷！

## 配置方式
1. （使用本地版的忽略这一步，暂时还没上架）在Dalamud设置-测试版中添加我的插件库``https://raw.githubusercontent.com/uiharuayako/DalamudPlugins/main/pluginmaster.json``，加载插件``Speak Beaver``
2. 进入[讯飞开放平台](https://www.xfyun.cn/)，申请一个开发者账号
3. 进入控制台，点击创建新应用，随便写一点内容。再次进入控制台，进入你创建的应用，在左边的菜单里找到 **语音听写（流式版）** 。这个东西每天都有500次的免费识别次数，真的真的用不完，相信我。如果你实在担心用完或者想和很多亲友分享（5人以上？），设置个支付密码就可以白嫖50000的服务量。
4. 在右边找到“服务接口认证信息”，把APPID、APISecret、APIKey的一串代码复制到插件设置的讯飞Api设置里，注意不要多复制出空格。
5. 服务接口认证信息下面有一个高级功能，把“多候选-句级”开通（暂时还没做这个功能，但是接口里填了这个参数，理论上开不开通功能是一样的）
6. （非必要）在QolBar里添加并修改这个快捷方式，把按键改成你觉得方便点的  
```H4sIAAAAAAAACqtWUimpLEhVslJKVdJRSjJSsqqGiwB5Okp5QEZwQWpitlNqYllqEVCk2EfJKhqhqhimyiM/F2RGMpCpXwzSAeRk+gPVGugZ6ABxLJAbpGQF4iklBxcAJSx0TGJrdbAY9XRD/8sZ81EMU0jOSMxLT1WAS5Fr9J6Gp8u7UY0uLkksKqHAyMY5z9YuQjcyv4B8E1+s2/d87zo0/+fnpWWmE2cmUDgpHCisowQSQqgtRtiqVAY02kjPWM9Iz0CpFgD9o8HeBwIAAA==```
7. （非必要）打开插件主界面（命令``/speak``），查看命令列表，把命令列表中的命令写到宏里绑定到你觉得方便的快捷键上。
8.  在设置中把输入设备切到你正在使用的麦克风，然后使用命令或者点击插件主界面的 开始语音输入 按钮，测试一下语言输入能不能正常工作

## 进阶设置？
在讯飞开放平台-控制台-语音听写流式版中，有一个个性化热词功能，可以添加FF14中的专有名词来让识别更准确，在这里举个栗子：  
```
主坦
副坦
纯奶
盾奶
近战
法系
远敏
```
遗憾的是他不支持英文，像D1 D2这种词还是会识别成第一，第二。在上线的正式版中，将会加入词语替换功能。（现在先摆一会