using Sims3.Gameplay;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.UI;
using System.Collections.Generic;
using System.Xml;

namespace Destrospean.Utils.ExpandedHouseholdStaff
{
    public class FileUtils
    {
        public class SaveSetting
        {
            public string Data;

            public SaveSetting(string name, string data)
            {
                Data = data;
            }
        }

        public static readonly string Header = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

        public static bool ExportToFile(string text)
        {
            string name = null;
            bool found = true;
            while (found)
            {
                name = StringInputDialog.Show("Title", "Prompt", "Save Settings");
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }
                name = "Destrospean.ExpandedHouseholdStaff." + name;
                BinModel.Singleton.PopulateExportBin();
                found = false;
                foreach (ExportBinContents contents in BinModel.Singleton.ExportBinContents)
                {
                    if (contents.HouseholdName == null || !contents.HouseholdName.Contains("Destrospean.ExpandedHouseholdStaff"))
                    {
                        continue;
                    }
                    if (contents.HouseholdName == name)
                    {
                        SimpleMessageDialog.Show("Title", "Exists");
                        found = true;
                        break;
                    }
                }
            }
            Household dummyHousehold = Household.Create();
            dummyHousehold.SetName(name);
            dummyHousehold.BioText = text;
            BinModel.Singleton.AddToExportBin(dummyHousehold);
            dummyHousehold.Destroy();
            SimpleMessageDialog.Show("Title", "Success");
            return true;
        }

        public static XmlElement ExtractFromFile()
        {
            BinModel.Singleton.PopulateExportBin();
            List<SaveSetting> settings = new List<SaveSetting>();
            foreach (ExportBinContents contents in BinModel.Singleton.ExportBinContents)
            {
                if (contents.HouseholdName == null)
                {
                    continue;
                }
                if (!contents.HouseholdName.Contains("Destrospean.ExpandedHouseholdStaff."))
                {
                    continue;
                }
                settings.Add(new SaveSetting(contents.HouseholdName, contents.HouseholdBio));
            }
            if (settings.Count == 0)
            {
                SimpleMessageDialog.Show("Title", "Error");
                return null;
            }
            SaveSetting selection = null;
            if (selection == null)
            {
                return null;
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(selection.Data);
            return xmlDocument.DocumentElement;
        }

        public static XmlElement ExtractFromTuning(string name)
        {
            XmlDocument xmlDocument = Simulator.LoadXML(name);
            if (xmlDocument == null)
            {
                return null; 
            }
            return xmlDocument.DocumentElement;
        }

        public static bool ImportFromFile()
        {
            return ImportSettings(ExtractFromFile());
        }

        public static bool ImportFromTuning(string name)
        {
            return ImportSettings(ExtractFromTuning(name));
        }

        public static bool ImportSettings(XmlElement element)
        {
            if (element == null)
            {
                return false;
            }
            SimpleMessageDialog.Show("Title", "Success");
            return true;
        }
    }
}

