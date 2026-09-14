using Sims3.SimIFace.CAS;
using Sims3.UI;
using System.Collections.Generic;

namespace Destrospean.UI.Columns
{
    public class CASAgeGenderFlagEnabledColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<CASAgeGenderFlags>
    {
        readonly string mLocalizationPath;

        readonly List<CASAgeGenderFlags> mFlagsEnabled;

        public CASAgeGenderFlagEnabledColumn(string localizationPath, CASAgeGenderFlags[] flagsEnabled) : base(localizationPath + "/Headers/Enabled:Text", localizationPath + "/Headers/Enabled:Tooltip", 40)
        {
            mLocalizationPath = localizationPath;
            mFlagsEnabled = new List<CASAgeGenderFlags>(flagsEnabled);
        }

        public override ObjectPicker.ColumnInfo GetValue(CASAgeGenderFlags flag)
        {
            return new ObjectPicker.TextColumn(Responder.Instance.LocalizationModel.LocalizeString(mLocalizationPath + "/Options/Enabled:" + mFlagsEnabled.Contains(flag)));
        }
    }
}
