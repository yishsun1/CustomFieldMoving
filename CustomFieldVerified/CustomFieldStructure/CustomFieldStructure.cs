using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFieldVerified
{
    internal class CustomFieldStructure : ICustomFieldManager
    {
        public int DefaultID { get; set; } = Constants.NotExistId;
        public List<CustomFieldSetting> Settings { get; set; } = new List<CustomFieldSetting>();
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
}
