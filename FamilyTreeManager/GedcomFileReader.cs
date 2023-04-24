using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FamilyTreeManager
{
    class GedcomFileReader
    {
        public void ReadGedcomFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                List<string> onePersonLines = null;

                do
                {
                    line = sr.ReadLine();
                } while (!line.StartsWith("0 @"));

                while (!sr.EndOfStream)
                {
                    if (line.StartsWith("0 @"))
                    {
                        ExtractPersonInfo(onePersonLines);
                        onePersonLines = new List<string>();
                    }

                    onePersonLines.Add(line);
                    line = sr.ReadLine();
                }
            }
        }

        private Person ExtractPersonInfo(List<string> onePersonLines)
        {
            if (onePersonLines == null)
                return null;

            Person person = new Person();
            string data;

            foreach (string currentLine in onePersonLines)
            {
                string[] temp;
                string previosuLine;

                if (currentLine.StartsWith("0 @"))
                {
                    Regex regex = new Regex("@.*?@");
                    Match match = regex.Match(currentLine);

                    person.ID = match.Value;
                }
                else if (currentLine.StartsWith("2 GIVN"))
                {
                    temp = currentLine.Split();
                    data = temp.Last();
                    person.Name = data;
                }
                else if (currentLine.StartsWith("2 SURN"))
                {
                    temp = currentLine.Split();
                    data = temp.Last();
                    person.Surname = data;
                }
                else if (currentLine.StartsWith("2 _MARNM"))
                {
                    temp = currentLine.Split();
                    data = temp.Last();
                    person.MarriedSurname = data;
                }
                else if (currentLine.StartsWith("1 SEX"))
                {
                    char sex = currentLine.Last();
                    person.Sex = sex == 'M' ? Person.SexEnum.M : Person.SexEnum.F;
                }

                previosuLine = currentLine;
            }

            return null;
        }
    }
}
