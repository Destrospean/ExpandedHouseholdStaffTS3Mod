using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Interactions;
using Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod.Services;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;

namespace Sims3.Gameplay.zoeoeAndDestrospean.ServantRolesMod
{
    public static class Main
    {
        [Tunable]
        public static bool kInstantiator;

        public static bool HasBeenPreloaded = false;

        static Main()
        {
            CommonUtils.AddEnumValue<CommodityKind>("BeHousekeeper", Housekeeper.ServiceMotive);
            LoadSaveManager.ObjectGroupsPreLoad += OnPreLoad;
            World.OnObjectPlacedInLotEventHandler += OnObjectPlacedInLot;
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnWorldLoadFinishedEventHandler += OnWorldLoadFinished;
            World.sOnWorldQuitEventHandler += OnWorldQuit;
        }

        static void AddInteractions(this GameObject gameObject)
        {
            gameObject.AddInteraction(DismissHousekeeper.Singleton, true);
            gameObject.AddInteraction(RequestHousekeeper.Singleton, true);
        }

        static void OnObjectPlacedInLot(object sender, EventArgs e)
        {
            World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
            if (onObjectPlacedInLotEventArgs != null)
            {
                Phone phone = GameObject.GetObject(onObjectPlacedInLotEventArgs.mObjectId) as Phone;
                if (phone != null)
                {
                    phone.AddInteractions();
                }
            }
        }

        static void OnPreLoad()
        {
            CommonUtils.TryDisplayScriptError(() =>
                {
                    if (!HasBeenPreloaded)
                    {
                        XmlDbData xmlDbData = XmlDbData.ReadData("ServantRolesMod_Housekeeper_ActiveTopic");
                        if (xmlDbData != null)
                        {
                            SocialManager.ParseActiveTopic(xmlDbData);
                        }
                        HasBeenPreloaded = true;
                    }
                });
        }

        static void OnStartupApp(object sender, EventArgs args)
        {
            CommonUtils.TryDisplayScriptError(() => CommonUtils.LoadMotive("BeHousekeeperMotive"));
        }

        static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            foreach (Phone phone in Sims3.Gameplay.Queries.GetObjects<Phone>())
            {
                phone.AddInteractions();
            }
            CommonUtils.TryDisplayScriptError(() =>
                {
                    Housekeeper.Create();
                    if (Housekeeper.Instance == null)
                    {
                        return;
                    }
                    IEnumerator<SimDescription> enumerator = Housekeeper.Instance.Pool.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current != null && enumerator.Current.CreatedSim != null)
                        {
                            CommonUtils.UpdateMotiveTunings(enumerator.Current.CreatedSim, Housekeeper.ServiceMotive);
                        }
                    }
                });
        }

        static void OnWorldQuit(object sender, EventArgs e)
        {
            if (Housekeeper.Instance != null)
            {
                Housekeeper.sHousekeeper = null;
            }
        }
    }
}
