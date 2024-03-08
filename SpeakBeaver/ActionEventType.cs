using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
// 事件类型类，仅用于分类事件
namespace SpeakBeaver
{
    public class ActionEventType
    {
        public string TypeName { get; set; }
        public bool EnableType { get; set; }
        public List<ActionEventInfo> Events { get; set; }
        
        public ActionEventType() {
            TypeName = "New Type";
            EnableType = true;
            Events = new List<ActionEventInfo>();
        }
        public ActionEventType(string typeName)
        {
            TypeName= typeName;
            EnableType = true;
            Events = new List<ActionEventInfo>();
        }
        public ActionEventType(string typeName,ActionEventInfo info)
        {
            TypeName = typeName;
            EnableType = true;
            Events = new List<ActionEventInfo>
            {
                info
            };
        }
        public void ExportedType()
        {

        }
        public void RenameType(string newName) {
            TypeName = newName;
            // 对内部的每一个event重命名
            foreach(ActionEventInfo info in Events) {
                info.Type = newName;      
            }
        }
    }
}
