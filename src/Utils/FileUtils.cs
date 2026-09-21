using Sims3.Gameplay;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Skills;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Destrospean.Misc;

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

        public static bool TryCreateServiceProfilesXml(out string xml)
        {
            xml = null;
            StringBuilder stringBuilder = new StringBuilder();
            using (Utf8StringWriter stringWriter = new Utf8StringWriter(stringBuilder))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                    {
                        Encoding = Encoding.UTF8
                    }))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("base");
                    foreach (ServiceUtils.ServiceProfile profile in ServiceUtils.ServiceProfiles)
                    {
                        xmlWriter.WriteStartElement("ServiceProfile");
                        xmlWriter.WriteAttributeString("version", profile.VersionString);
                        xmlWriter.WriteAttributeString("name", profile.Name);
                        xmlWriter.WriteAttributeString("workMotive", profile.ServiceMotive.ToString());

                        xmlWriter.WriteStartElement("Title");
                        xmlWriter.WriteAttributeString("localize", false.ToString());
                        xmlWriter.WriteString(profile.Title);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("RequestedMessage");
                        xmlWriter.WriteAttributeString("localize", false.ToString());
                        xmlWriter.WriteString(profile.RequestedMessage);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("CancelledMessage");
                        xmlWriter.WriteAttributeString("localize", false.ToString());
                        xmlWriter.WriteString(profile.CancelledMessage);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("CancelledWhileActiveMessage");
                        xmlWriter.WriteAttributeString("localize", false.ToString());
                        xmlWriter.WriteString(profile.CancelledWhileActiveMessage);
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteElementString("DelayBeforeArriving", profile.DelayBeforeArriving.ToString());

                        xmlWriter.WriteElementString("DelayBeforeLeaving", profile.DelayBeforeLeaving.ToString());

                        xmlWriter.WriteElementString("ExtraWaitTimeAfterSocializing", profile.ExtraWaitTimeAfterSocializing.ToString());

                        xmlWriter.WriteElementString("ValidAges", profile.ValidAges.ToString());

                        xmlWriter.WriteElementString("ValidGenders", profile.ValidGenders.ToString());

                        xmlWriter.WriteElementString("ServiceProfileFlags", profile.GetFlags().ToString());

                        xmlWriter.WriteElementString("TimeToSpendWorking", profile.TimeToSpendWorking.ToString());

                        xmlWriter.WriteElementString("Cost", profile.Cost.ToString());

                        xmlWriter.WriteStartElement("Motives");
                        foreach (CommodityKind motive in profile.Motives)
                        {
                            xmlWriter.WriteStartElement("Motive");
                            xmlWriter.WriteAttributeString("name", motive.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("Traits");
                        foreach (TraitNames trait in profile.Traits)
                        {
                            xmlWriter.WriteStartElement("Trait");
                            xmlWriter.WriteAttributeString("name", trait.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("PotentialTraits");
                        xmlWriter.WriteAttributeString("count", profile.PotentialTraitCount.ToString());
                        foreach (TraitNames trait in profile.PotentialTraits)
                        {
                            xmlWriter.WriteStartElement("Trait");
                            xmlWriter.WriteAttributeString("name", trait.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("HiddenTraits");
                        foreach (TraitNames trait in profile.HiddenTraits)
                        {
                            xmlWriter.WriteStartElement("Trait");
                            xmlWriter.WriteAttributeString("name", trait.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("Skills");
                        foreach (SkillLevelPair skill in profile.Skills)
                        {
                            xmlWriter.WriteStartElement("Skill");
                            xmlWriter.WriteAttributeString("name", skill.SkillName.ToString());
                            xmlWriter.WriteAttributeString("level", skill.SkillLevel.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("Actions");
                        foreach (ServiceUtils.ActiveTopicAction action in profile.Actions)
                        {
                            xmlWriter.WriteStartElement(action.IsActive ? "FPA" : "SPA");
                            xmlWriter.WriteAttributeString("group", action.Grouping.ToString());
                            xmlWriter.WriteAttributeString("name", action.Name);
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("Outputs");
                        foreach (ServiceUtils.CommodityChange output in profile.Outputs)
                        {
                            xmlWriter.WriteStartElement("CommodityChange");
                            xmlWriter.WriteAttributeString("interactionDefinitionType", output.InteractionDefinitionType);
                            xmlWriter.WriteAttributeString("targetType", output.TargetType);
                            xmlWriter.WriteAttributeString("advertised", output.ConstantChange.ToString());
                            xmlWriter.WriteAttributeString("locked", output.Locked.ToString());
                            xmlWriter.WriteAttributeString("actual", output.ActualValue.ToString());
                            xmlWriter.WriteAttributeString("updateType", output.UpdateType.ToString());
                            xmlWriter.WriteAttributeString("timeDependsOnCommodityFilling", output.TimeDependsOnCommodityFilling.ToString());
                            xmlWriter.WriteAttributeString("updateEvenOnFailure", output.UpdateEvenOnFailure.ToString());
                            xmlWriter.WriteAttributeString("updateAboveAndBelowZero", output.UpdateAboveAndBelowZero.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteStartElement("CarInstanceName");
                        xmlWriter.WriteString(profile.CarInstanceName ?? "");
                        xmlWriter.WriteFullEndElement();

                        xmlWriter.WriteElementString("CarProductVersion", profile.CarProductVersion.ToString());

                        xmlWriter.WriteElementString("CheckTime", profile.CheckTime.ToString());

                        xmlWriter.WriteElementString("DriveTime", profile.CheckTime.ToString());

                        xmlWriter.WriteElementString("MaxNumNPCsInPool", profile.MaxNumNPCsInPool.ToString());

                        xmlWriter.WriteElementString("RelationshipLevelForQuit", profile.RelationshipLevelForQuit.ToString());

                        xmlWriter.WriteElementString("TimeWaitBeforePutawayLeftovers", profile.TimeWaitBeforePutawayLeftovers.ToString());

                        xmlWriter.WriteElementString("UseObjectInSameRoomAsSleeperMultiplier", profile.UseObjectInSameRoomAsSleeperMultiplier.ToString());

                        xmlWriter.WriteEndElement(); // Ends writing the "ServiceProfile" element.
                    }
                    foreach (OutfitAssignmentUtils.OutfitAssignment outfitAssignment in OutfitAssignmentUtils.OutfitAssignments)
                    {
                        OutfitAssignmentUtils.AssignedOutfit assignedOutfit;
                        if (outfitAssignment.SimDescription != null || !OutfitAssignmentUtils.AssignedOutfits.TryGetValue(outfitAssignment.SpecialOutfitKey, out assignedOutfit))
                        {
                            continue;
                        }
                        string ageGenderPrefix = outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentUtils.OutfitAssignmentGlobalPrefix.Length, 2);
                        CASAgeGenderFlags age = CASAgeGenderFlags.Adult;
                        switch (ageGenderPrefix[0])
                        {
                            case 'a':
                                age = CASAgeGenderFlags.Adult;
                                break;
                            case 'b':
                                age = CASAgeGenderFlags.Baby;
                                break;
                            case 'c':
                                age = CASAgeGenderFlags.Child;
                                break;
                            case 'e':
                                age = CASAgeGenderFlags.Elder;
                                break;
                            case 'p':
                                age = CASAgeGenderFlags.Toddler;
                                break;
                            case 't':
                                age = CASAgeGenderFlags.Teen;
                                break;
                        }
                        CASAgeGenderFlags gender = CASAgeGenderFlags.Male;
                        switch (ageGenderPrefix[1])
                        {
                            case 'f':
                                gender = CASAgeGenderFlags.Female;
                                break;
                            case 'm':
                                gender = CASAgeGenderFlags.Male;
                                break;
                        }
                        xmlWriter.WriteStartElement("ServiceUniform");
                        xmlWriter.WriteAttributeString("profile", outfitAssignment.ServiceName);
                        xmlWriter.WriteAttributeString("age", age.ToString());
                        xmlWriter.WriteAttributeString("gender", gender.ToString());

                        xmlWriter.WriteStartElement("Parts");
                        foreach (OutfitAssignmentUtils.AssignedOutfit.SavedPart part in assignedOutfit.Parts)
                        {
                            xmlWriter.WriteStartElement("Part");
                            xmlWriter.WriteAttributeString("key", part.Part.Key.ToString().ToUpperInvariant());
                            xmlWriter.WriteRaw(part.Preset?.Substring(part.Preset.IndexOf("\n")) ?? "");
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("PartOverrides");
                        foreach (BodyTypes partType in assignedOutfit.PartOverrides)
                        {
                            xmlWriter.WriteStartElement("PartType");
                            xmlWriter.WriteAttributeString("name", partType.ToString());
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteEndElement(); // Ends writing the "ServiceUniform" element.
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                }
                stringWriter.Flush();
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(stringBuilder.ToString());
            stringBuilder.Remove(0, stringBuilder.Length);
            using (Utf8StringWriter stringWriter = new Utf8StringWriter(stringBuilder))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                    {
                        Encoding = Encoding.UTF8,
                        Indent = true,
                        IndentChars = "  ",
                        OmitXmlDeclaration = false
                    }))
                {
                    xmlDocument.WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                stringWriter.Flush();
            }
            xml = stringBuilder.ToString();
            return true;
        }

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

