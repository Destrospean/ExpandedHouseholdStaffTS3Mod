using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Objects.Electronics;
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

        static Main()
        {
            CommonUtils.AddEnumValue<CommodityKind>("BeHousekeeper", Housekeeper.BeServiceCommodityKind);
            World.sOnStartupAppEventHandler += OnStartupApp;
            World.sOnWorldLoadFinishedEventHandler += OnWorldLoadFinished;
            World.OnObjectPlacedInLotEventHandler += OnObjectPlacedInLot;
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

        static void OnStartupApp(object sender, EventArgs args)
        {
            Exception exception;
            CommonUtils.TryGetException(() => CommonUtils.LoadMotive("BeHousekeeperMotive"), out exception);
        }

        static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            foreach (Phone phone in Sims3.Gameplay.Queries.GetObjects<Phone>())
            {
                phone.AddInteractions();
            }
            CommonUtils.ShowDebugMessageDialog(Housekeeper.BeServiceCommodityKind.ToString());
            Exception exception;
            CommonUtils.TryGetException(() =>
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
                            CommonUtils.UpdateMotiveTunings(enumerator.Current.CreatedSim, Housekeeper.BeServiceCommodityKind);
                        }
                    }
                }, out exception);
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
