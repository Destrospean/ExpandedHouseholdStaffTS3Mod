using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;

namespace zoeoeAndDestrospean.UI.Columns
{
    public class TypeColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<Type>
    {
        public TypeColumn(string localizationPath) : base(localizationPath + "/Header:Text", localizationPath + "/Header:Tooltip", 440)
        {
        }

        public override ObjectPicker.ColumnInfo GetValue(Type type)
        {
            return new ObjectPicker.TextColumn(type?.FullName ?? "");
        }
    }
}
