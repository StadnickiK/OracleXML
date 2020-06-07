using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SklepGenerator.DataTypes
{
    class Number : DataType
    {
        public Number() { }
        public Number(string name, int value)
        {
            Name = name;
            Value = value;
        }
        public new string Name { get; set; }
        public new int Value { get; set; }
    }
}
