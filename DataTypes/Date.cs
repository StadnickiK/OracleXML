using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SklepGenerator.DataTypes
{
    class Date : DataType
    {
        public Date() { }
        public Date(string name, DateTime value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public DateTime Value { get; set; }
    }
}
