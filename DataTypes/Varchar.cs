using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SklepGenerator.DataTypes
{
    class Varchar : DataType
    {
        public Varchar() { }
        public Varchar(string name, string value)
        {
            Name = name;
            value = Value;
        }
    }
}
