using System;

namespace PoGo.NecroBot.Logic.Model.Settings
{
    public class NecroBotConfigAttribute : Attribute
    {
        public string SheetName { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool HiddenOnGui { get; set; }
    }
}