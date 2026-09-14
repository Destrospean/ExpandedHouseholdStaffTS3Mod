using Sims3.Gameplay.Interfaces.Destrospean.ExpandedHouseholdStaff;
using Sims3.UI;

namespace Destrospean.UI.Columns.ExpandedHouseholdStaff
{
    public class ServiceProfileColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<IServiceProfile>
    {
        public ServiceProfileColumn(string localizationPath) : base(localizationPath + "/Header:Text", localizationPath + "/Header:Tooltip", 440)
        {
        }

        public override ObjectPicker.ColumnInfo GetValue(IServiceProfile profile)
        {
            return new ObjectPicker.TextColumn(profile?.Title ?? "");
        }
    }
}
