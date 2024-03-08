using Dalamud.Logging;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SpeakBeaver.Windows;

public partial class ComboMainWindow
{
    // 统计字符串行数
    static int SubstringCount(string str, string substring)
    {
        if (str.Contains(substring))
        {
            string strReplaced = str.Replace(substring, "");
            return (str.Length - strReplaced.Length) / substring.Length;
        }

        return 0;
    }
    private static JsonSerializerOptions JSOption = new JsonSerializerOptions { WriteIndented = true };
    // private void DrawEvent(Configuration configuration)
    // {
    //     bool EnableActionTrigger = configuration.EnableActionTrigger;
    //     if (ImGui.Checkbox("启用技能事件",
    //         ref EnableActionTrigger))
    //     {
    //         configuration.EnableActionTrigger = EnableActionTrigger;
    //         configuration.Save();
    //     }
    //     ImGui.SameLine();
    //     Spacing();
    //     if (ImGui.Button(LocalizationManager.RightLang.Configwindow_Events_AddEvent))
    //     {
    //         configuration.Events.Add(new ActionEventInfo());
    //         configuration.Save();
    //     }
    //     ImGui.SameLine();
    //     Spacing();
    //     // 新建一个类型，使用默认类型名，没有内置event
    //     if (ImGui.Button(LocalizationManager.RightLang.Configwindow_Events_AddType))
    //     {
    //         configuration.EventTypes.Add(new ActionEventType());
    //         configuration.Save();
    //     }
    //     ImGui.SameLine();
    //     Spacing();
    //     // 导入功能
    //     if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_ImportType}##ImportType"))
    //     {
    //         string clipboard = ImGui.GetClipboardText();
    //         ActionEventType importType = JsonSerializer.Deserialize<ActionEventType>(clipboard);
    //         bool isExistSameType = false;
    //         // 判断，导入的名称是否已存在
    //         foreach (var item in configuration.EventTypes)
    //         {
    //             // 如果存在同名类型，则添加
    //             if (item.TypeName == importType.TypeName)
    //             {
    //                 item.Events.AddRange(importType.Events);
    //                 isExistSameType = true;
    //             }
    //         }
    //         if (!isExistSameType)
    //         {
    //             configuration.EventTypes.Add(importType);
    //         }
    //         configuration.Save();
    //     }
    //     ImGui.Separator();
    //     ImGui.Text("在这个窗口，你可以设定释放特定技能后自动触发的宏或命令。\n可以直接输入多个技能，例如“赤复活，复活，复生”\n使用<tar>指代技能选中的目标，使用<t>指代玩家选中的目标\n例如“/p <me>已复活<tar>”\n可输入多行命令，使用ctrl+enter换行，wait指令无效");
    //
    //     ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 5f));
    //
    //     // 绘制事件ui
    //     if (ImGui.BeginChild("Events List", new Vector2(0f, -1f), true))
    //     {
    //         // 以下是没有类型的事件
    //         for (int i = 0; i < configuration.Events.Count(); i++)
    //         {
    //             string name = configuration.Events[i].Name;
    //             if (ImGui.InputText($"{LocalizationManager.RightLang.Configwindow_Events_ActionName}##ActionName{i}",
    //                 ref name, 50))
    //             {
    //                 configuration.Events[i].Name = name;
    //                 configuration.Save();
    //             }
    //             string eventType = configuration.Events[i].Type;
    //             if (ImGui.InputText($"{LocalizationManager.RightLang.Configwindow_Events_ActionType}##ActionType{i}",
    //                 ref eventType, 50))
    //             {
    //                 configuration.Events[i].Type = eventType;
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_ChangeType}##ChangeType{i}"))
    //             {
    //                 // 加载在这里的事件本来没有Type，当输入文字之后，需要保存Type
    //                 // 确认输入的Type是不是在当前已有的Type里
    //                 bool isTypeExist = false;
    //                 foreach (var thisType in configuration.EventTypes.Where(
    //                 thisType => thisType.TypeName == eventType))
    //                 {
    //                     thisType.Events.Add(configuration.Events[i]);
    //                     isTypeExist = true;
    //                     break;
    //                 }
    //                 if (!isTypeExist)
    //                 {
    //                     // 如果是当前没有的Type，则创建
    //                     configuration.EventTypes.Add(new ActionEventType(eventType, configuration.Events[i]));
    //                 }
    //                 // 删除当前的event
    //                 configuration.Events.RemoveAt(i);
    //                 configuration.Save();
    //             }
    //             int macroindex = configuration.Events[i].MacroIndex;
    //             if (ImGui.DragInt($"{LocalizationManager.RightLang.Configwindow_Events_MacroIndex}##MacroIndex{i}",
    //                 ref macroindex, 1, 0, 99))
    //             {
    //                 configuration.Events[i].MacroIndex = macroindex;
    //                 configuration.Save();
    //             }
    //             // 输入框的高度有点讲究
    //             string macroString = configuration.Events[i].macroString;
    //             if (ImGui.InputTextMultiline($"{LocalizationManager.RightLang.Configwindow_Events_MacroString}##MacroString{i}",
    //                 ref macroString, 3000, new Vector2(ImGui.GetWindowSize().X * 0.7f, ImGui.GetTextLineHeightWithSpacing() + ImGui.GetTextLineHeight() * SubstringCount(macroString, "\n")), ImGuiInputTextFlags.CtrlEnterForNewLine))
    //             {
    //                 configuration.Events[i].macroString = macroString;
    //                 configuration.Save();
    //             }
    //             bool isEnabled = configuration.Events[i].IsEnable;
    //             if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_EnableMacro}##EnableMacro{i}",
    //                 ref isEnabled))
    //             {
    //                 configuration.Events[i].IsEnable = isEnabled;
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             bool isShared = configuration.Events[i].IsShared;
    //             if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_ShareMacro}##ShareMacro{i}",
    //                 ref isShared))
    //             {
    //                 configuration.Events[i].IsShared = isShared;
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             bool noMacro = configuration.Events[i].noMacro;
    //             if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_NoMacro}##NoMacro{i}",
    //                 ref noMacro))
    //             {
    //                 configuration.Events[i].noMacro = noMacro;
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             bool noCmd = configuration.Events[i].noCmd;
    //             if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_NoCmd}##NoCmd{i}",
    //                 ref noCmd))
    //             {
    //                 configuration.Events[i].noCmd = noCmd;
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_RemoveEvent}##RemoveEvent{i}"))
    //             {
    //                 configuration.Events.RemoveAt(i);
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             // 往上移，不能是第一个元素
    //             if (ImGui.Button($"↑##EventUp{i}") && i != 0)
    //             {
    //                 ActionEventInfo temp = configuration.Events[i];
    //                 configuration.Events[i] = configuration.Events[i - 1];
    //                 configuration.Events[i - 1] = temp;
    //                 configuration.Save();
    //             }
    //             ImGui.SameLine();
    //             Spacing();
    //             // 往下移，不能是最后一个元素
    //             if (ImGui.Button($"↓##EventDown{i}") && i != configuration.Events.Count() - 1)
    //             {
    //                 ActionEventInfo temp = configuration.Events[i];
    //                 configuration.Events[i] = configuration.Events[i + 1];
    //                 configuration.Events[i + 1] = temp;
    //                 configuration.Save();
    //             }
    //             ImGui.Separator();
    //         }
    //
    //         ImGui.Separator();
    //         // 加载带有类型的事件
    //         try
    //         {
    //             for (int i = 0; i < configuration.EventTypes.Count(); i++)
    //             {
    //                 // 折叠分隔
    //                 if (ImGui.CollapsingHeader(configuration.EventTypes[i].TypeName))
    //                 {
    //                     // 重命名功能逻辑：重命名之后，和现有类型名称相同，则合并events，和现有名称不同，则新建类型
    //                     string TypeName = configuration.EventTypes[i].TypeName;
    //                     if (ImGui.InputText($"{LocalizationManager.RightLang.Configwindow_Events_RenameType}##Rename{i}",
    //                             ref TypeName, 50, ImGuiInputTextFlags.EnterReturnsTrue))
    //                     {
    //                         // 合并Events
    //                         foreach (var item in configuration.EventTypes.Where(item => item.TypeName == TypeName))
    //                         {
    //                             item.Events.AddRange(configuration.EventTypes[i].Events);
    //                             // 合并完把当前的删了
    //                             configuration.EventTypes.RemoveAt(i);
    //                         }
    //                         configuration.EventTypes[i].TypeName = TypeName;
    //                         configuration.Save();
    //                     }
    //                     ImGui.SameLine();
    //                     Spacing();
    //                     // 确认重命名
    //                     if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_RenameAll}##RenameTypeAndEvents{i}"))
    //                     {
    //                         configuration.EventTypes[i].RenameType(TypeName);
    //                         configuration.Save();
    //                     }
    //                     bool isTypeEnabled = configuration.EventTypes[i].EnableType;
    //                     // 是否启用
    //                     if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_EnableMacro}##EnableType{i}",
    //                         ref isTypeEnabled))
    //                     {
    //                         configuration.EventTypes[i].EnableType = isTypeEnabled;
    //                         configuration.Save();
    //                     }
    //                     ImGui.SameLine();
    //                     Spacing();
    //                     // 添加带有类型的事件
    //                     if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_AddEvent}##AddEventByType{i}"))
    //                     {
    //                         configuration.EventTypes[i].Events.Add(new ActionEventInfo(configuration.EventTypes[i].TypeName));
    //                         configuration.Save();
    //                     }
    //                     ImGui.SameLine();
    //                     Spacing();
    //                     if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_DelType}##RemoveType{i}"))
    //                     {
    //                         configuration.EventTypes.RemoveAt(i);
    //                         configuration.Save();
    //                     }
    //                     ImGui.SameLine();
    //                     Spacing();
    //                     // 导出功能
    //                     if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_ExportType}##ExportType{i}"))
    //                     {
    //                         ImGui.SetClipboardText(Regex.Unescape((JsonSerializer.Serialize(configuration.EventTypes[i], JSOption))));
    //                     }
    //                     ImGui.SameLine();
    //                     Spacing();
    //                     // 往上移，不能是第一个元素
    //                     if (ImGui.Button($"↑##TypeUp{i}") && i != 0)
    //                     {
    //                         ActionEventType temp = configuration.EventTypes[i];
    //                         configuration.EventTypes[i] = configuration.EventTypes[i - 1];
    //                         configuration.EventTypes[i - 1] = temp;
    //                         configuration.Save();
    //                     }
    //                     ImGui.SameLine();
    //                     Spacing();
    //                     // 往下移，不能是最后一个元素
    //                     if (ImGui.Button($"↓##TypeDown{i}") && i != configuration.EventTypes.Count() - 1)
    //                     {
    //                         ActionEventType temp = configuration.EventTypes[i];
    //                         configuration.EventTypes[i] = configuration.EventTypes[i + 1];
    //                         configuration.EventTypes[i + 1] = temp;
    //                         configuration.Save();
    //                     }
    //                     ImGui.Separator();
    //                     // 列出分类的事件
    //
    //                     for (int j = 0; j < configuration.EventTypes[i].Events.Count(); j++)
    //                     {
    //                         // 二重for，此时访问的event是 configuration.EventTypes[i].Events[j]
    //                         string name = configuration.EventTypes[i].Events[j].Name;
    //                         if (ImGui.InputText($"{LocalizationManager.RightLang.Configwindow_Events_ActionName}##ActionName{i}{j}",
    //                             ref name, 50))
    //                         {
    //                             configuration.EventTypes[i].Events[j].Name = name;
    //                             configuration.Save();
    //                         }
    //                         // 当某一event已有type，此时更改他的type，发生如下逻辑
    //                         string eventType = configuration.EventTypes[i].Events[j].Type;
    //                         if (ImGui.InputText($"{LocalizationManager.RightLang.Configwindow_Events_ActionType}##ActionType{i}{j}",
    //                             ref eventType, 50))
    //                         {
    //                             configuration.EventTypes[i].Events[j].Type = eventType;
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_SaveType}##SaveType{i}{j}"))
    //                         {
    //                             // 加载在这里的事件本来有Type，当输入文字之后，需要添加到别的Type里或者新建Type
    //                             // 确认输入的Type是不是在当前已有的Type里
    //                             bool isTypeExist = false;
    //                             foreach (var thisType in configuration.EventTypes.Where(
    //                             thisType => thisType.TypeName == eventType))
    //                             {
    //                                 thisType.Events.Add(configuration.EventTypes[i].Events[j]);
    //                                 // 删除当前的event
    //                                 configuration.EventTypes[i].Events.RemoveAt(j);
    //                                 isTypeExist = true;
    //                                 break;
    //                             }
    //                             if (!isTypeExist)
    //                             {
    //                                 // 如果是当前没有的Type，则创建
    //                                 configuration.EventTypes.Add(new ActionEventType(eventType, configuration.EventTypes[i].Events[j]));
    //                                 // 删除当前的event
    //                                 configuration.EventTypes[i].Events.RemoveAt(j);
    //                             }
    //                             configuration.Save();
    //                         }
    //                         int macroindex = configuration.EventTypes[i].Events[j].MacroIndex;
    //                         if (ImGui.DragInt($"{LocalizationManager.RightLang.Configwindow_Events_MacroIndex}##MacroIndex{i}{j}",
    //                             ref macroindex, 1, 0, 99))
    //                         {
    //                             configuration.EventTypes[i].Events[j].MacroIndex = macroindex;
    //                             configuration.Save();
    //                         }
    //                         string macroString = configuration.EventTypes[i].Events[j].macroString;
    //                         if (ImGui.InputTextMultiline($"{LocalizationManager.RightLang.Configwindow_Events_MacroString}##MacroString{i}{j}",
    //                             ref macroString, 3000, new Vector2(ImGui.GetWindowSize().X * 0.7f, ImGui.GetTextLineHeightWithSpacing() + ImGui.GetTextLineHeight() * SubstringCount(macroString, "\n")), ImGuiInputTextFlags.CtrlEnterForNewLine))
    //                         {
    //                             configuration.EventTypes[i].Events[j].macroString = macroString;
    //                             configuration.Save();
    //                         }
    //                         bool isEnabled = configuration.EventTypes[i].Events[j].IsEnable;
    //                         if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_EnableMacro}##EnableMacro{i}{j}",
    //                             ref isEnabled))
    //                         {
    //                             configuration.EventTypes[i].Events[j].IsEnable = isEnabled;
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         bool isShared = configuration.EventTypes[i].Events[j].IsShared;
    //                         if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_ShareMacro}##ShareMacro{i}{j}",
    //                             ref isShared))
    //                         {
    //                             configuration.EventTypes[i].Events[j].IsShared = isShared;
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         bool noMacro = configuration.EventTypes[i].Events[j].noMacro;
    //                         if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_NoMacro}##NoMacro{i}{j}",
    //                             ref noMacro))
    //                         {
    //                             configuration.EventTypes[i].Events[j].noMacro = noMacro;
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         bool noCmd = configuration.EventTypes[i].Events[j].noCmd;
    //                         if (ImGui.Checkbox($"{LocalizationManager.RightLang.Configwindow_Events_NoCmd}##NoCmd{i}{j}",
    //                             ref noCmd))
    //                         {
    //                             configuration.EventTypes[i].Events[j].noCmd = noCmd;
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         if (ImGui.Button($"{LocalizationManager.RightLang.Configwindow_Events_RemoveEvent}##RemoveEvent{i}{j}"))
    //                         {
    //                             // 删除某一type中的event
    //                             configuration.EventTypes[i].Events.RemoveAt(j);
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         // 往上移，不能是第一个元素
    //                         if (ImGui.Button($"↑##EventInTypeUp{i}{j}") && j != 0)
    //                         {
    //                             ActionEventInfo temp = configuration.EventTypes[i].Events[j];
    //                             configuration.EventTypes[i].Events[j] = configuration.EventTypes[i].Events[j - 1];
    //                             configuration.EventTypes[i].Events[j - 1] = temp;
    //                             configuration.Save();
    //                         }
    //                         ImGui.SameLine();
    //                         Spacing();
    //                         // 往下移，不能是最后一个元素
    //                         if (ImGui.Button($"↓##EventInTypeDown{i}{j}") && j != configuration.EventTypes[i].Events.Count() - 1)
    //                         {
    //                             ActionEventInfo temp = configuration.EventTypes[i].Events[j];
    //                             configuration.EventTypes[i].Events[j] = configuration.EventTypes[i].Events[j + 1];
    //                             configuration.EventTypes[i].Events[j + 1] = temp;
    //                             configuration.Save();
    //                         }
    //                         ImGui.Separator();
    //                     }
    //
    //                 }
    //             }
    //         }
    //         catch (System.ArgumentOutOfRangeException)
    //         { }
    //
    //         ImGui.EndChild();
    //     }
    //     ImGui.PopStyleVar();
    //
    // }
}
