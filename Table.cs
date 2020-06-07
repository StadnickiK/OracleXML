using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SklepGenerator.DataTypes;

namespace SklepGenerator
{
    public class Table
    {
        public string Name { get; set; }
        public List<DataType> Columns { get; set; } = new List<DataType>();
        public string PrimaryKeyName { get; set; }
        public Dictionary<string, string> Relations { get; set; } = new Dictionary<string, string>();
        public List<int> FreeIDs { get; set; } = new List<int>();
    }
}
