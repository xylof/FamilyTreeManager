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

        private enum RecordType
        {
            None, Birth, Death, Marriage
        };

        public List<Person> people = new List<Person>();
        public List<Family> families = new List<Family>();

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
            //bool isThisBirthRecord = false;
            RecordType recordType = RecordType.None;

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
                    //isThisBirthRecord = true;
                    recordType = RecordType.Birth;
                else if (currentLine.StartsWith("1 DEAT"))
                {
                    //isThisBirthRecord = false;
                    recordType = RecordType.Death;
                    person.IsDead = true;
                }
                else if (currentLine.StartsWith("2 DATE"))
                {
                    temp = currentLine.Split();
                    ExtractDate(temp, recordType, person);

                    //int day = 0, month = 0, year = 0;

                    //if (temp.Length == 5)
                    //{
                    //    day = int.Parse(temp[2]);
                    //    month = (int)Enum.Parse(typeof(Months), temp[3]) + 1;
                    //    year = int.Parse(temp[4]);
                    //}
                    //else if (temp.Length == 4)
                    //{
                    //    month = (int)Enum.Parse(typeof(Months), temp[2]) + 1;
                    //    year = int.Parse(temp[3]);
                    //}
                    //else if (temp.Length == 3)
                    //{
                    //    year = int.Parse(temp[2]);
                    //}

                    //if (recordType == RecordType.Birth)
                    //    person.BirthDate = new DateTime(year, month, day);
                    //else
                    //    person.DeathDate = new DateTime(year, month, day);
                }
                else if (currentLine.StartsWith("2 PLAC"))
                {
                    data = currentLine.Remove(0, 7); // Usuwa pierwsze 7 znaków z łańcucha string

                    if (recordType == RecordType.Birth)
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
                //else if (currentLine.StartsWith("1 FAMS"))
                //{
                //    Regex regex = new Regex("@.*?@");
                //    Match match = regex.Match(currentLine);

                //    person.FamsID = match.Value;
                //}
                //else if (currentLine.StartsWith("1 FAMC"))
                //{
                //    Regex regex = new Regex("@.*?@");
                //    Match match = regex.Match(currentLine);

                //    person.FamcID = match.Value;
                //}
            }

            people.Add(person);
        }

        private void ExtractFamilyInfo(List<string> oneFamilyLines)
        {
            if (oneFamilyLines == null)
                return;

            Family family = new Family();
            string data;
            RecordType recordType = RecordType.Marriage;

            foreach (string currentLine in oneFamilyLines)
            {
                string[] temp;

                if (currentLine.StartsWith("0 @F"))
                {
                    Regex regex = new Regex("@.*?@");
                    Match match = regex.Match(currentLine);

                    family.ID = match.Value;
                }
                else if (currentLine.StartsWith("1 HUSB"))
                {
                    temp = currentLine.Split();
                    data = temp.Last();
                    family.Husband = people.Find(p => p.ID == data);
                }
                else if (currentLine.StartsWith("1 WIFE"))
                {
                    temp = currentLine.Split();
                    data = temp.Last();
                    family.Wife = people.Find(p => p.ID == data);
                }
                else if (currentLine.StartsWith("1 CHIL"))
                {
                    temp = currentLine.Split();
                    data = temp.Last();
                    family.AddChild(people.Find(p => p.ID == data));
                }
                else if (currentLine.StartsWith("1 MARR"))
                    family.RelationType = Family.RelationTypeEnum.Marriage;
                else if (currentLine.StartsWith("1 ENGA"))
                    family.RelationType = Family.RelationTypeEnum.Engagement;
                else if (currentLine.EndsWith("PARTNERS"))
                    family.RelationType = Family.RelationTypeEnum.Partner;
                else if (currentLine.StartsWith("2 DATE"))
                {
                    temp = currentLine.Split();
                    ExtractDate(temp, recordType, family);

                    //int day = 0, month = 0, year = 0;

                    //if (temp.Length == 5)
                    //{
                    //    day = int.Parse(temp[2]);
                    //    month = (int)Enum.Parse(typeof(Months), temp[3]) + 1;
                    //    year = int.Parse(temp[4]);
                    //}
                    //else if (temp.Length == 4)
                    //{
                    //    month = (int)Enum.Parse(typeof(Months), temp[2]) + 1;
                    //    year = int.Parse(temp[3]);
                    //}
                    //else if (temp.Length == 3)
                    //{
                    //    year = int.Parse(temp[2]);
                    //}

                    //family.WeddingDate = new DateTime(year, month, day);
                }
                else if (currentLine.StartsWith("2 PLAC"))
                    family.WeddingPlace = currentLine.Remove(0, 7); // Usuwa pierwsze 7 znaków z łańcucha string
            }

            family.CompleteFamilyConnections();
            families.Add(family);
        }

        private void ExtractDate(string[] temp, RecordType recordType, Person person)
        {
            ExtractDate(temp, recordType, person, null);
        }

        private void ExtractDate(string[] temp, RecordType recordType, Family family)
        {
            ExtractDate(temp, recordType, null, family);
        }

        private void ExtractDate(string[] temp, RecordType recordType, Person person, Family family)
        {
            int day = 0, month = 0, year = 0;
            const int dayMonthAndYearRecordLength = 5;
            const int monthAndYearRecordLength = 4;
            const int yearRecordLength = 3;
            int change = 0;
            bool isThisDateEstimated = false;

            if (temp[2] == "ABT")
            {
                change = 1;
                isThisDateEstimated = true;
            }

            if (temp.Length == dayMonthAndYearRecordLength + change)
            {
                day = int.Parse(temp[2 + change]);
                month = (int)Enum.Parse(typeof(Months), temp[3 + change]) + 1;
                year = int.Parse(temp[4 + change]);
            }
            else if (temp.Length == monthAndYearRecordLength + change)
            {
                month = (int)Enum.Parse(typeof(Months), temp[2 + change]) + 1;
                year = int.Parse(temp[3 + change]);
            }
            else if (temp.Length == yearRecordLength + change)
            {
                year = int.Parse(temp[2 + change]);
            }

            if (recordType == RecordType.Birth)
            {
                person.BirthDate = new DateTime(year, month, day);
                person.IsBirthDateEstimated = isThisDateEstimated;
            }
            else if (recordType == RecordType.Death)
            {
                person.DeathDate = new DateTime(year, month, day);
                person.IsDeathDateEstimated = isThisDateEstimated;
            }
            else if (recordType == RecordType.Marriage)
            {
                family.WeddingDate = new DateTime(year, month, day);
                family.IsWeddingDateEstimated = isThisDateEstimated;
            }
        }
    }
}
