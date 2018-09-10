using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFieldManager
{
    internal class CustomFieldSetting : ICustomFieldSetting
    {
        public string ID { get; set; } = null;
        public string Name { get; set; } = null;
        public int CycleTime { get; set; } = Constants.NotExistId;
        public List<CustomField> Fields { get; set; } = new List<CustomField>();

        public ICustomField AddField()
        {
            var newField = new CustomField();
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
}
