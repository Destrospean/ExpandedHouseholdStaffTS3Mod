using Sims3.UI;

namespace zoeoeAndDestrospean.UI.Columns
{
    public class TextColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<string>
    {
        public TextColumn(string localizationPath) : base(localizationPath + "/Header:Text", localizationPath + "/Header:Tooltip", 440)
        {
        }

        public override ObjectPicker.ColumnInfo GetValue(string text)
        {
            return new ObjectPicker.TextColumn(text ?? "");
        }
    }
}
