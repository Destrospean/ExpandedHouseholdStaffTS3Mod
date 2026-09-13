using Sims3.SimIFace.CAS;
using Sims3.UI;
using System.Collections.Generic;

namespace zoeoeAndDestrospean.UI.Columns
{
    public class PartOverrideEnabledColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<BodyTypes>
    {
        readonly string mLocalizationPath;

        readonly List<BodyTypes> mPartOverrides;

        public PartOverrideEnabledColumn(string localizationPath, BodyTypes[] partOverrides) : base(localizationPath + "/Headers/Enabled:Text", localizationPath + "/Headers/Enabled:Tooltip", 40)
        {
            mLocalizationPath = localizationPath;
            mPartOverrides = new List<BodyTypes>(partOverrides);
        }

        public override ObjectPicker.ColumnInfo GetValue(BodyTypes bodyType)
        {
            return new ObjectPicker.TextColumn(Responder.Instance.LocalizationModel.LocalizeString(mLocalizationPath + "/Options/Enabled:" + mPartOverrides.Contains(bodyType)));
        }
    }
}
