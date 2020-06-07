using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SklepGenerator.DataTypes;

namespace SklepGenerator.OracleDataManager
{
    class DataGenerator
    {
        Random rand = new Random();
        public DataGenerator() { }

        public int GetRandNumber(int max = 100)
        {
            return rand.Next(max);
        }

        public int GetRandNumber(int min, int max)
        {
            return rand.Next(min, max);
        }

        string GetRandString(int length = 8)
        {
            string s = "";
            for(int i = 0; i<length;i++)
            {
                char c = Convert.ToChar(rand.Next(48, 122));
                s += c.ToString();
            }
            return s;
        }

        string GetRandDate()
        {
            int year = 1997;
            int month = GetRandNumber(1, 12);
            int day = GetRandNumber(1, 31);

            DateTime date = new DateTime(year,month,day);
            return date.ToString("yy/MM/dd");
        }

        public string GenerateRandData(string value, DateTime date)
        {
            string data = null;
            switch (value)
            {
            case "NUMBER":
                data =  GetRandNumber(9).ToString();
                break;
            case "VARCHAR2":
                data = "'"+GetRandString()+"'";
                break;
            case "DATE":    // RR/mm/dd
                data = "'"+date.AddTicks(864000000000).ToString("yy/MM/dd")+"'";
                break;
            }
            return data;
        }
    }
}
