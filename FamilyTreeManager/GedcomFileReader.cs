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
        private enum Months
        {
            JAN, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, NOV, DEC
        };

        public List<Person> people = new List<Person>();

        public void ReadGedcomFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string line;
                List<string> onePersonLines = null;
                List<string> oneFamilyLines = null;

                do
                {
                    line = sr.ReadLine();
                } while (!line.StartsWith("0 @"));

                while (!line.StartsWith("0 @F"))
                {
                    if (line.StartsWith("0 @I"))
                    {
                        ExtractPersonInfo(onePersonLines);
                        onePersonLines = new List<string>();
                    }

                    onePersonLines.Add(line);
                    line = sr.ReadLine();
                }

                ExtractPersonInfo(onePersonLines);

                while (!sr.EndOfStream)
                {
                    if (line.StartsWith("0 @F"))
                    {
                        ExtractFamilyInfo(oneFamilyLines);
                        oneFamilyLines = new List<string>();
                    }

                    oneFamilyLines.Add(line);
                    line = sr.ReadLine();
                }

                ExtractFamilyInfo(oneFamilyLines);
            }
        }

        private void ExtractPersonInfo(List<string> onePersonLines)
        {
            if (onePersonLines == null)
                return;

            Person person = new Person();
            string data;
            bool isThisBirthRecord = false;

            foreach (string currentLine in onePersonLines)
            {
                string[] temp;

                if (currentLine.StartsWith("0 @I"))
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
                else if (currentLine.StartsWith("1 BIRT"))
                    isThisBirthRecord = true;
                else if (currentLine.StartsWith("1 DEAT"))
                {
                    isThisBirthRecord = false;
                    person.IsDead = true;
                }
                else if (currentLine.StartsWith("2 DATE"))
                {
                    temp = currentLine.Split();
                    int day = 0, month = 0, year = 0;

                    if (temp.Length == 5)
                    {
                        day = int.Parse(temp[2]);
                        month = (int)Enum.Parse(typeof(Months), temp[3]) + 1;
                        year = int.Parse(temp[4]);
                    }
                    else if (temp.Length == 4)
                    {
                        month = (int)Enum.Parse(typeof(Months), temp[2]) + 1;
                        year = int.Parse(temp[3]);
                    }
                    else if (temp.Length == 3)
                    {
                        year = int.Parse(temp[2]);
                    }

                    if (isThisBirthRecord)
                        person.BirthDate = new DateTime(year, month, day);
                    else
                        person.DeathDate = new DateTime(year, month, day);
                }
                else if (currentLine.StartsWith("2 PLAC"))
                {
                    data = currentLine.Remove(0, 7); // Usuwa pierwsze 7 znaków z łańcucha string

                    if (isThisBirthRecord)
                        person.BirthPlace = data;
                    else
                        person.DeathPlace = data;
                }
                else if (currentLine.StartsWith("1 EVEN NAT"))
                {
                    data = currentLine.Remove(0, 11);
                    string[] delimiters = { " ", ", " };
                    temp = data.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                    person.FillNationalities(temp);
                }
                else if (currentLine.StartsWith("1 FAMS"))
                {
                    Regex regex = new Regex("@.*?@");
                    Match match = regex.Match(currentLine);

                    person.FamsID = match.Value;
                }
                else if (currentLine.StartsWith("1 FAMC"))
                {
                    Regex regex = new Regex("@.*?@");
                    Match match = regex.Match(currentLine);

                    person.FamcID = match.Value;
                }
            }

            people.Add(person);
        }

        private void ExtractFamilyInfo(List<string> oneFamilyLines)
        {

        }
    }
}
