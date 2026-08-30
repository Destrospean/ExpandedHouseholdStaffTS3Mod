using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Abstracts.zoeoeAndDestrospean.ServantRolesMod;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Objects.Beds;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
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

        static void AddInteractions(this Bed bed)
        {
            bed.AddInteraction(Housekeeper.SetUnsetServiceBed.Singleton, true);
        }

        static void AddInteractions(this Phone phone)
        {
            phone.AddInteraction(Housekeeper.CallForService.Singleton, true);
        }

        static void AddInteractions(this PhoneCell phoneCell)
        {
            foreach (InteractionObjectPair interaction in phoneCell.Interactions)
            {
                if (interaction.InteractionDefinition.GetType() == Housekeeper.CallForService.Singleton.GetType())
                {
                    return;
                }
            }
            phoneCell.AddInteraction(Housekeeper.CallForService.Singleton);
            phoneCell.AddInventoryInteraction(Housekeeper.CallForService.Singleton);
        }

        static void InitInjection()
        {
            PhoneCell[] objects = Sims3.Gameplay.Queries.GetObjects<PhoneCell>();
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].AddInteractions();
            }
            EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, OnObjectChanged);
            EventTracker.AddListener(EventTypeId.kObjectStateChanged, OnObjectChanged);
        }

        public static ListenerAction OnObjectChanged(Event e)
        {
            PhoneCell phoneCell = e.TargetObject as PhoneCell;
            if (phoneCell != null)
            {
                phoneCell.AddInteractions();
            }
            return ListenerAction.Keep;
        }

        static void OnObjectPlacedInLot(object sender, EventArgs e)
        {
            World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs = e as World.OnObjectPlacedInLotEventArgs;
            if (onObjectPlacedInLotEventArgs != null)
            {
                GameObject gameObject = GameObject.GetObject(onObjectPlacedInLotEventArgs.mObjectId);
                Bed bed = gameObject as Bed;
                if (bed != null)
                {
                    bed.AddInteractions();
                }
                Phone phone = gameObject as Phone;
                if (phone != null)
                {
                    phone.AddInteractions();
                    return;
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

        public static ListenerAction OnSimSelected(Event e)
        {
            if (Household.ActiveHousehold != null)
            {
                InitInjection();
                return ListenerAction.Remove;
            }
            return ListenerAction.Keep;
        }

        static void OnStartupApp(object sender, EventArgs args)
        {
            CommonUtils.TryDisplayScriptError(() => CommonUtils.LoadMotive("BeHousekeeperMotive"));
        }

        static void OnWorldLoadFinished(object sender, EventArgs e)
        {
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
                    if (Household.ActiveHousehold != null)
                    {
                        InitInjection();
                    }
                    else
                    {
                        EventTracker.AddListener(EventTypeId.kEventSimSelected, OnSimSelected);
                    }
                    foreach (Bed bed in Sims3.Gameplay.Queries.GetObjects<Bed>())
                    {
                        bed.AddInteractions();
                    }
                    foreach (Phone phone in Sims3.Gameplay.Queries.GetObjects<Phone>())
                    {
                        phone.AddInteractions();
                    }
                });
        }

        static void OnWorldQuit(object sender, EventArgs e)
        {
            if (Housekeeper.Instance != null)
            {
                Housekeeper.sInstance = null;
            }
        }
    }
}
