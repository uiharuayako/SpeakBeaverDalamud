namespace SpeakBeaver
{
    public class ActionEventInfo
    {
        public string Name { get; set; }
        public int MacroIndex { get; set; }
        public bool IsShared { get; set; }
        public bool IsEnable { get; set; }
        public bool noMacro { get; set; }
        public bool noCmd { get; set; }
        public string macroString { get; set; }
        public string Type { get; set; }
        public ActionEventInfo()
        {
            Name = "";
            MacroIndex = -1;
            IsEnable = true;
            noMacro = false;
            noCmd = false;
            IsShared = false;
            macroString = "";
            Type = "";
        }
        public ActionEventInfo(string type)
        {
            Name = "";
            MacroIndex = -1;
            IsEnable = true;
            noMacro = false;
            noCmd = false;
            IsShared = false;
            macroString = "";
            Type = type;
        }
        public string MacroToString(string targetName)
        {
            // 将target插入到MacroString中
            return macroString.Replace("<tar>",targetName);
        }
        public string[] MacroStringLines()
        {
            return macroString.Split("\n");
        }
    }
}
