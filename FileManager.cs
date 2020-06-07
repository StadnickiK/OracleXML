using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SklepGenerator
{
    public class FileManager
    {
        string text = "";

        public void AddLine(string s)
        {
            text += s + "\n";
        }

        public void SaveToFile(string name)
        {
            string today = DateTime.Now.ToString("yy'.'mm'.'dd'-'HH'.'mm'.'ss");
            string s = name + " " + today + ".txt";
            using (StreamWriter streamWriter = new StreamWriter(s))
            {
                streamWriter.WriteLine(text);
            }
            Console.WriteLine("File " + s+" saved");
            text = "";
        }

        public void SaveAsXMLFile(string name)
        {
            string today = DateTime.Now.ToString("yy'.'mm'.'dd'-'HH'.'mm'.'ss");
            string s = name + " " + today + ".xml";
            using (StreamWriter streamWriter = new StreamWriter(s))
            {
                streamWriter.WriteLine(text);
            }
            Console.WriteLine("File " + s + " saved");
            text = "";
        }
    }
}
