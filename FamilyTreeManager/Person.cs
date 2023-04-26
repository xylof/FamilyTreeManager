using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTreeManager
{
    class Person
    {
        public enum SexEnum { M, F };

        public string ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string MarriedSurname { get; set; }
        public SexEnum Sex { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public bool IsBirthDateEstimated { get; set; }
        public DateTime DeathDate { get; set; }
        public string DeathPlace { get; set; }
        public bool IsDeathDateEstimated { get; set; }
        public bool IsDead { get; set; }
        public string FamsID { get; set; }
        public string FamcID { get; set; }
        public Dictionary<string, double> Nationalities { get; set; }

        public Person()
        {
            Nationalities = new Dictionary<string, double>();
        }

        public void FillNationalities(string[] nationsAndNumbers)
        {
            for (int i = 0; i < nationsAndNumbers.Length; i += 2)
                Nationalities.Add(nationsAndNumbers[i], double.Parse(nationsAndNumbers[i + 1]));
        }

        public override string ToString()
        {
            if (MarriedSurname != null)
                return $"{Name} {MarriedSurname} z d. {Surname}";
            return $"{Name} {Surname}";
        }
    }
}
