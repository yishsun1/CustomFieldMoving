using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFieldSimple
{
    public interface ICustomFieldIO
    {
        void Import(string context);
        string Export();
        ICustomFieldStructure GetStructure();

    }
    public interface IReadonlyStructure
    {
        int DefaultID { get;}
        IEnumerable<ICustomFieldSetting> PeekSettings();
    }
    public interface IReadonlySetting
    {
        string ID { get;}
        string Name { get;}
        int CycleTime { get;}
        int CommodityId { get;}
        int DefaultCharacters { get;}
        IEnumerable<ICustomField> PeekFields();
    }
    public interface IReadonlyField
    {
        int Combination { get;}
        int Characters { get;}
        int RotationFirst { get;}
        int RotationSecond { get;}
    }

    public interface ICustomFieldStructure : IReadonlyStructure
    {
        new int DefaultID { get; set; }
        new IEnumerable<ICustomFieldSetting> PeekSettings();
        ICustomFieldSetting AddSetting();
        void RemoveSetting();
    }
    public interface ICustomFieldSetting : IReadonlySetting
    {
        new string ID { get; set; }
        new string Name { get; set; }
        new int CycleTime { get; set; }
        new int CommodityId { get; set; }
        new int DefaultCharacters { get; set; }
        new IEnumerable<ICustomField> PeekFields();
        ICustomField AddField();
        void RemoveField();
    }
    public interface ICustomField : IReadonlyField
    {
        new int Combination { get; set; }
        new int Characters { get; set; }
        new int RotationFirst { get; set; }
        new int RotationSecond { get; set; }
    }
    
}
