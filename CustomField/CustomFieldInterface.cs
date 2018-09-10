using System.Collections.Generic;

namespace CustomFieldManager
{
    /// <summary>
    /// Represent a manager to CustomField Structure
    /// </summary>
    public interface ICustomFieldManager
    {

        int DefaultID { get; set; }
        /// <summary>
        /// Get a READONLY setting List
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICustomFieldSetting> PeekSettings();
        /// <summary>
        /// Construct an empty setting at the end of list, returning its reference
        /// </summary>
        /// <returns></returns>
        ICustomFieldSetting AddSetting();
        /// <summary>
        /// Remove the setting at the end of list
        /// </summary>
        void RemoveSetting();
        /// <summary>
        /// Import a XML string into a manipulatable CustomField manager
        /// </summary>
        /// <param name="context"></param>
        void Import(string context);
        /// <summary>
        /// Export a XML string from a CustomField manager 
        /// </summary>
        /// <returns></returns>
        string Export();
    }

    /// <summary>
    /// Represent a setting info of CustomField.
    /// </summary>
    public interface ICustomFieldSetting
    {
        string ID { get; set; }
        string Name { get; set; }
        int CycleTime { get; set; }
        //int DefaultCharacters { get; set; }
        /// <summary>
        /// Get a READONLY field List
        /// </summary>
        /// <returns></returns>
        IEnumerable<ICustomField> PeekFields();
        /// <summary>
        /// Construct an empty field at the end of list, returning its reference
        /// </summary>
        /// <returns></returns>
        ICustomField AddField();
        /// <summary>
        /// Remove the field at the end of list
        /// </summary>
        void RemoveField();
    }
    /// <summary>
    /// Represent a CustomField info
    /// </summary>
    public interface ICustomField
    {
        int Combination { get; set; }
        int TextLimit { get; set; }
        int RotationFirst { get; set; }
        int RotationSecond { get; set; }
    }
    /// <summary>
    /// Constants of Tags and keywords in Custom Field Module
    /// </summary>
    public static class Constants
    {
        public const int NotExistId = -1;
        public const string TagTitleList = "TitleList";
        public const string TagTitle = "Title";
        public const string TagResult = "Result";

        public const string AttrDefaultID = "DefaultID";
        public const string AttrID = "ID";
        public const string AttrName = "Name";
        public const string AttrCycleTime = "CycleTime";
        public const string AttrFields = "Value";
        public const string AttrTextLimit = "TextLimit";
        
        public const string ModifierDefaultID = "_";
        public const char SeparatorField = ',';
        public const char SeparatorAttr = ';';

        public static bool NotExist(int i) => NotExistId == i;
        public static bool NotExist(string s) => null == s;
    }
}
