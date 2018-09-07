using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace CustomFieldSimple
{
    public class DirtyCustomFieldIO : ICustomFieldIO
    {

        #region - Members -

        private CustomFieldStructure _structure = null;

        #endregion

        public void Import(string context)
        {
            #region - Functions -

            IEnumerable<XElement> SearchByName(XElement root, string tag)
            {
                var elements = root.DescendantsAndSelf(tag);
                if (elements is null)
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

            if (String.IsNullOrWhiteSpace(context))
                return;
            var tree = XElement.Parse(context);
            if (tree is null){ return; }
            var tList = SearchByName(tree, Constants.TagTitleList).First();
            var buildingStucture = new CustomFieldStructure();
            LambdaRunIfFound(tList, Constants.AttrDefaultID, (s) =>
            {
                string prefix = Constants.ModifierDefaultID;
                if (s.StartsWith(prefix) && Int32.TryParse(s.Substring(prefix.Length), out int val))
                {
                    buildingStucture.DefaultID = val;
                }
            });

            foreach (var titleElement in SearchByName(tList, Constants.TagTitle))
            {
                var setting = new CustomFieldSetting();
                LambdaRunIfFound(titleElement, Constants.AttrID, (s) => { setting.ID = s; });
                LambdaRunIfFound(titleElement, Constants.AttrName, (s) => { setting.Name = s; });
                LambdaRunIfFound(titleElement, Constants.AttrCycleTime, (s) => { setting.CycleTime = Int32.Parse(s); });

                List<int> limitList = null;
                LambdaRunIfFound(titleElement, Constants.AttrTextLimit, (limitString) =>
                {
                    IEnumerable<int> limitValues;
                    try
                    {
                        limitValues = limitString.Split(Constants.SeparatorField).Select(Int32.Parse);
                    }
                    catch (FormatException){ return;}
                    setting.DefaultCharacters = limitValues.First();
                    limitList = limitValues.Skip(1).ToList();
                });
                LambdaRunIfFound(titleElement, Constants.AttrFields, (fieldString) =>
                {
                    IEnumerable<IEnumerable<int>> fieldValues;
                    try
                    {
                        fieldValues = fieldString.Split(Constants.SeparatorField)
                            .Select(s => s.Split(Constants.SeparatorAttr).Select(Int32.Parse));
                    }
                    catch (FormatException) { return;
                    }
                    setting.CommodityId = fieldValues.First().First();
                    var buildingFields = new List<RestrictField>();
                    foreach (var values in fieldValues.Skip(1))
                    {
                        var field = new RestrictField();
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
                            e.Field.Characters = e.Limit;
                        }
                    }
                    foreach (var elt in buildingFields)
                    {
                        setting.Fields.Add(elt);
                    }
                });

                buildingStucture.Settings.Add(setting);
            }
            _structure = buildingStucture;
        }

        public string Export() => null;
        public ICustomFieldStructure GetStructure()
        {
            if (_structure is null){ _structure = new CustomFieldStructure(); }
            return _structure;
        }
    }

    class CustomFieldStructure : ICustomFieldStructure
    {
        public int DefaultID { get; set; } = Constants.NotExistId;
        public List<CustomFieldSetting> Settings { get; } = new List<CustomFieldSetting>();
        public ICustomFieldSetting AddSetting()
        {
            var newSetting = new CustomFieldSetting();
            Settings.Add(newSetting);
            return newSetting;
        }
        public IEnumerable<ICustomFieldSetting> PeekSettings()
        {
            return Settings.AsReadOnly();
        }
        public void RemoveSetting()
        {
            if (!Settings.Any())
            {
                throw new InvalidOperationException("Settings is Empty.");
            }
            Settings.RemoveAt(Settings.Count() - 1);
        }
    }
    class CustomFieldSetting : ICustomFieldSetting
    {
        public string ID { get; set; } = null;
        public string Name { get; set; } = null;
        public int CycleTime { get; set; } = Constants.NotExistId;
        public List<RestrictField> Fields { get; } = new List<RestrictField>();
        public int CommodityId { get; set; } = Constants.NotExistId;
        public int DefaultCharacters { get; set; } = Constants.NotExistId;

        public ICustomField AddField()
        {
            var newField = new RestrictField();
            Fields.Add(newField);
            return newField;
        }
        public IEnumerable<ICustomField> PeekFields()
        {
            return Fields.AsReadOnly();
        }
        public void RemoveField()
        {
            if (!Fields.Any())
            {
                throw new InvalidOperationException("Fields is Empty.");
            }
            Fields.RemoveAt(Fields.Count() - 1);
        }
    }
    class RestrictField : ICustomField
    {
        public int Combination { get; set;} = Constants.NotExistId;
        public int Characters { get; set; } = Constants.NotExistId;
        public int RotationFirst { get; set; } = Constants.NotExistId;
        public int RotationSecond { get; set; } = Constants.NotExistId;
    }
}
