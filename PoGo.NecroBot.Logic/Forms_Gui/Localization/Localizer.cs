#region using directives

using PoGo.NecroBot.Logic.Forms_Gui.Common;

#endregion

namespace PoGo.NecroBot.Logic.Forms_Gui.Localization
{
    public interface ILocalizer
    {
        string GetFormat(TranslationString key);
        string GetFormat(TranslationString key, params object[] data);
    }

    public class Localizer : ILocalizer
    {
        public string GetFormat(TranslationString key)
        {
            return "";
        }

        public string GetFormat(TranslationString key, params object[] data)
        {
            return "";
        }
    }
}