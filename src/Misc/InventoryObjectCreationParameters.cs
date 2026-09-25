using Sims3.Gameplay;
using Sims3.Gameplay.Interfaces;
using Sims3.SimIFace;

namespace Destrospean.Misc
{
    [Persistable]
    public class InventoryObjectCreationParameters
    {
        uint mProductVersion = 0u;

        public int Count;

        public string InstanceName;

        public string Preset;

        public ProductVersion ProductVersion
        {
            get
            {
                return (ProductVersion)mProductVersion;
            }
            set
            {
                mProductVersion = (uint)value;
            }
        }

        protected InventoryObjectCreationParameters()
        {
        }

        public InventoryObjectCreationParameters(string instanceName, ProductVersion productVersion, int count, string preset = null)
        {
            Count = count;
            InstanceName = instanceName;
            Preset = preset;
            ProductVersion = productVersion;
        }

        public IGameObject Instantiate()
        {
            return InstantiateMany(1)[0];
        }

        public IGameObject[] InstantiateMany()
        {
            return InstantiateMany(Count);
        }

        public IGameObject[] InstantiateMany(int count)
        {
            if (count < 0)
            {
                return new IGameObject[0];
            }
            IGameObject[] results = new IGameObject[count];
            for (int i = 0; i < count; i++)
            {
                results[i] = GlobalFunctions.CreateObjectOutOfWorld(InstanceName, ProductVersion);
            }
            return results;
        }
    }
}
