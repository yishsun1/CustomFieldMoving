using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CustomFieldVerified
{
    /// <summary>
    /// Represent a customIO implementation with verifying functionality
    /// </summary>
    public class CustomFieldVerifiedIO : ICustomFieldIO
    {
        #region - Members -
        private CustomFieldStructure _structure = null;
        private IDVerifyHandler _idVerifier;
        #endregion

        #region - Functions -

        private void CheckStructure()
        {
            #region - Function -

            int DefaultIfXQField(int localID)
            {
                //Frequently
                if (Constants.NotExist(localID)
                    || !_idVerifier(localID))
                {
                    return Constants.NotExistId;
                }
                return localID;
            }

            #endregion

            if (_idVerifier is null) { return; }

            var checking = _structure;

            checking.DefaultID = DefaultIfXQField(checking.DefaultID);
            foreach (var setting in checking.PeekSettings())
            {
                //String
                //setting.ID = SafeString(setting.ID);
                //setting.Name = SafeString(setting.Name);
                foreach (var field in setting.PeekFields())
                {
                    field.Combination = DefaultIfXQField(field.Combination);
                    field.RotationFirst = DefaultIfXQField(field.RotationFirst);
                    field.RotationSecond = DefaultIfXQField(field.RotationSecond);
                }
            }
        }
        private void Initialize(IDVerifyHandler verifier)
        {
            _idVerifier = verifier;
            _structure = new CustomFieldStructure();
        }
        private string SafeString(string s) { return String.IsNullOrWhiteSpace(s) ? null : s; }
        private int SafeParse(string s) { return Int32.TryParse(s, out int result) ? result : Constants.NotExistId; }

        #region - Export -

        void SetAttrIfExist(XElement node, string key, int value)
        {
            if (Constants.NotExist(value))
            {
                return;
            }
            node.SetAttributeValue(key, value);
        }
        void SetAttrIfExist(XElement node, string key, string value)
        {
            if (Constants.NotExist(value))
            {
                return;
            }
            node.SetAttributeValue(key, value);
        }
        void SetAttrForDefaultID(XElement node, string key, int value)
        {

            if (Constants.NotExist(value))
            {
                return;
            }
            node.SetAttributeValue(key, Constants.ModifierDefaultID + value);
        }
        string FieldToString(CustomField f) //
        {
            var words = new LinkedList<int>();
            bool shouldAssignValue = false;
            int[] fieldValues = { f.RotationSecond, f.RotationFirst, f.Combination }; //Reversed
            foreach (var item in fieldValues)
            {
                if (!Constants.NotExist(item) || shouldAssignValue)
                {
                    words.AddFirst(item);
                    shouldAssignValue = true;
                }
            }
            var result = String.Join(Constants.SeparatorAttr.ToString(), words);
            return result;
        }

        #endregion

        #region - Import -

        IEnumerable<XElement> SearchByName(XElement root, string tag)
        {
            var elements = root.DescendantsAndSelf(tag);
            if (elements is null || !elements.Any())
                yield break;
            foreach (var elt in elements)
            {
                yield return elt;
            }
        }
        void LambdaRunIfFound(XElement node, string key, Action<string> method)
        {
            var found = node.Attribute(key);
            if (null != found)
            {
                method(found.Value);
            }
        }

        #endregion

        #endregion

        public delegate bool IDVerifyHandler(int id);

        public CustomFieldVerifiedIO()
        {
            Initialize(null);
        }
        public CustomFieldVerifiedIO(IDVerifyHandler verifier)
        {
            Initialize(verifier);
        }
        public void Import(string context)
        {
            if (String.IsNullOrWhiteSpace(context)) { return; }
            var tree = XElement.Parse(context);
            if (tree is null) { return; }

            int newDefaultID = Constants.NotExistId;
            var newSettingList = new List<CustomFieldSetting>();

            var titleListNode = SearchByName(tree, Constants.TagTitleList).First();
            LambdaRunIfFound(titleListNode, Constants.AttrDefaultID, (s) =>
            {
                string prefix = Constants.ModifierDefaultID;
                if (s.StartsWith(prefix))
                {
                    newDefaultID = SafeParse(s.Substring(prefix.Length));
                }
            });
            foreach (var titleElement in SearchByName(titleListNode, Constants.TagTitle))
            {
                var setting = new CustomFieldSetting();
                LambdaRunIfFound(titleElement, Constants.AttrID, (s) => { setting.ID = SafeString(s); });
                LambdaRunIfFound(titleElement, Constants.AttrName, (s) => { setting.Name = SafeString(s); });
                LambdaRunIfFound(titleElement, Constants.AttrCycleTime, (s) => { setting.CycleTime = SafeParse(s); });

                List<int> limitList = null;
                LambdaRunIfFound(titleElement, Constants.AttrTextLimit, (limitString) =>
                {
                    IEnumerable<int> limitValues;
                    try
                    {
                        limitValues = limitString.Split(Constants.SeparatorField).Select(SafeParse);
                    }
                    catch (FormatException) { return; }
                    limitList = limitValues.ToList();
                });
                LambdaRunIfFound(titleElement, Constants.AttrFields, (fieldString) =>
                {
                    IEnumerable<IEnumerable<int>> fieldValues;
                    fieldValues = fieldString.Split(Constants.SeparatorField)
                        .Select(s => s.Split(Constants.SeparatorAttr).Select(SafeParse));
                    var buildingFields = new List<CustomField>();
                    foreach (var values in fieldValues)
                    {
                        var field = new CustomField();
                        var assigning = new List<Func<int, int>>()
                        {
                            i => field.Combination = i,
                            i => field.RotationFirst = i,
                            i => field.RotationSecond = i
                        };
                        foreach (var e in values.Zip(assigning, (v, f) => new { Value = v, Assign = f }))
                        {
                            e.Assign(e.Value);
                        }
                        buildingFields.Add(field);
                    }
                    if (null != limitList)
                    {
                        foreach (var e in limitList.Zip(buildingFields, (l, f) => new { Limit = l, Field = f }))
                        {
                            e.Field.TextLimit = e.Limit;
                        }
                    }
                    foreach (var elt in buildingFields)
                    {
                        setting.Fields.Add(elt);
                    }
                });

                newSettingList.Add(setting);
            }

            var buildingStucture = new CustomFieldStructure
            {
                DefaultID = newDefaultID,
                Settings = newSettingList
            };
            _structure = buildingStucture;

            CheckStructure();
        }
        public string Export()
        {
            CheckStructure();

            var source = _structure;
            XElement exporting = new XElement(Constants.TagResult);
            XElement tListNode = new XElement(Constants.TagTitleList);
            exporting.Add(tListNode);
            SetAttrForDefaultID(tListNode, Constants.AttrDefaultID, source.DefaultID);

            foreach (var settingEntry in source.Settings)
            {
                var valueString = String.Join(Constants.SeparatorField.ToString(),
                    settingEntry.Fields.Select(FieldToString)
                    );
                var textLimits = settingEntry.Fields.Select(f => f.TextLimit);
                string charactersString;
                if (textLimits.All(i => Constants.NotExist(i)))
                {
                    charactersString = null;
                }
                else
                {
                    charactersString = String.Join(Constants.SeparatorField.ToString(), textLimits);
                }

                XElement settingNode = new XElement(Constants.TagTitle);
                tListNode.Add(settingNode);
                SetAttrIfExist(settingNode, Constants.AttrID, settingEntry.ID);
                SetAttrIfExist(settingNode, Constants.AttrName, settingEntry.Name);
                SetAttrIfExist(settingNode, Constants.AttrCycleTime, settingEntry.CycleTime);
                SetAttrIfExist(settingNode, Constants.AttrFields, valueString);
                SetAttrIfExist(settingNode, Constants.AttrTextLimit, charactersString);
            }
            using (var writer = new StringWriter())
            {
                exporting.Save(writer);
                return writer.ToString();
            }
        }
        public ICustomFieldManager GetManager()
        {
            return _structure;
        }
    }
}
