using Sims3.Gameplay.Utilities;
using Sims3.UI.Controller;
using Sims3.UI;
using Destrospean.Enums;
using Destrospean.Utils;
using Destrospean.Utils.ExpandedHouseholdStaff;

namespace Destrospean.UI.Columns.ExpandedHouseholdStaff
{
    public class ActiveTopicActionGroupingColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<ServiceUtils.ActiveTopicAction>
    {
        public ActiveTopicActionGroupingColumn(string localizationPath) : base(localizationPath + "/Headers/Grouping:Text", localizationPath + "/Headers/Grouping:Tooltip", 220)
        {
        }

        public override ObjectPicker.ColumnInfo GetValue(ServiceUtils.ActiveTopicAction action)
        {
            return new ObjectPicker.TextColumn(action.Grouping == LongTermRelationshipTypes.BestFriendsForever ? Localization.LocalizeString(0xC86B0C71108DA632) : Localization.LocalizeString(typeof(Dialogs.ComboSelectionDialog).GetLocalizationKey().Replace("ComboSelectionDialog", ((action.Grouping & (LongTermRelationshipTypes.All | LongTermRelationshipTypes.Default | LongTermRelationshipTypes.Ex | LongTermRelationshipTypes.ExSpouse)) == 0 ? "Gameplay/Excel/Socializing/LTR:" : "ActiveTopicActionActivenessDialog/Options:") + action.Grouping)));
        }
    }
}
