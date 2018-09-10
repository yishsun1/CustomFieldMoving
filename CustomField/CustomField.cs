using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFieldManager
{
    internal class CustomField : ICustomField
    {
        public int Combination { get; set; } = Constants.NotExistId;
        public int TextLimit { get; set; } = Constants.NotExistId;
        public int RotationFirst { get; set; } = Constants.NotExistId;
        public int RotationSecond { get; set; } = Constants.NotExistId;
    }
}
