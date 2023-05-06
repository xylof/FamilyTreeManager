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
        //public string FamsID { get; set; }
        //public string FamcID { get; set; }
        public Dictionary<string, double> Nationalities { get; set; }
        public Person Father { get; set; }
        public Person Mother { get; set; }
        public List<Person> Partners { get; set; }
        public List<Person> Children { get; set; }

        public Person()
        {
            Nationalities = new Dictionary<string, double>();
            Partners = new List<Person>();
            Children = new List<Person>();
        }

        public void FillNationalities(string[] nationsAndNumbers)
        {
            for (int i = 0; i < nationsAndNumbers.Length; i += 2)
                Nationalities.Add(nationsAndNumbers[i], double.Parse(nationsAndNumbers[i + 1]));
        }

        public void AddPartner(Person partner)
        {
            Partners.Add(partner);
        }

        public void AddChild(Person child)
        {
            Children.Add(child);
        }

        public double Age
        {
            get
            {
                // Porównanie konkretnej daty do DateTime.MinValue ma podobne znaczenie, jak porównywanie zmiennej do wartości null
                if (IsDead && BirthDate != DateTime.MinValue && DeathDate != DateTime.MinValue) // Przypadek, gdy osoba już NIE żyje i znamy jej daty urodzenia oraz śmierci
                {
                    //double percentOfYear = 0;

                    //if (BirthDate.Month != 0 && DeathDate.Month != 0)
                    //    percentOfYear = (DeathDate.Month - BirthDate.Month) / 12.0;

                    //double age = DeathDate.Year - BirthDate.Year + percentOfYear;
                    //return Math.Round(age, 2);

                   return GetAge(() => BirthDate.Month != 0 && DeathDate.Month != 0, DeathDate.Month, DeathDate.Year);
                }
                if (!IsDead && BirthDate != DateTime.MinValue) // Przypadek gdy osoba jeszcze żyje i znamy jej datę urodzenia
                {
                    //double percentOfYear = 0;

                    //if (BirthDate.Month != 0)
                    //    percentOfYear = (DateTime.Now.Month - BirthDate.Month) / 12.0;

                    //double age = DateTime.Now.Year - BirthDate.Year + percentOfYear;
                    //return Math.Round(age, 2);

                    return GetAge(() => BirthDate.Month != 0, DateTime.Now.Month, DateTime.Now.Year);
                }
                return -1;
            }
        }

        private double GetAge(Func<bool> predicate, int month, int year)
        {
            double percentOfYear = 0;

            if (predicate())
                percentOfYear = (month - BirthDate.Month) / 12.0;

            double age = year - BirthDate.Year + percentOfYear;
            return Math.Round(age, 2);
        }

        public string GetBirthDate
        {
            get 
            {
                if (BirthDate == DateTime.MinValue)
                    return "";
                return IsBirthDateEstimated ? $"Ok. {BirthDate}" : BirthDate.ToString();
            }
        }

        public string GetDeathDate
        {
            get
            {
                if (DeathDate == DateTime.MinValue)
                    return "";
                return IsDeathDateEstimated ? $"Ok. {DeathDate}" : DeathDate.ToString();
            }
        }

        public bool IsMale
        {
            get { return Sex == SexEnum.M; }
        }

        public List<Person> GetSiblings
        {
            get
            {
                List<Person> siblings = new List<Person>();

                if (HasParents)
                {
                    siblings = Father.Children.Concat(Mother.Children).Distinct().ToList();
                    siblings.Remove(this);
                }
                return siblings;
            }
        }

        public Person[] GetParents
        {
            get { return new Person[] { Father, Mother }; }
        }

        public bool HasParents
        {
            get { return Father != null && Mother != null; }
        }

        public Person[] GetGrandparents
        {
            get
            {
                Person[] grandparents = new Person[4];

                if (Father != null)
                {
                    grandparents[0] = Father.Father;
                    grandparents[1] = Father.Mother;
                }
                if (Mother != null)
                {
                    grandparents[2] = Mother.Father;
                    grandparents[3] = Mother.Mother;
                }

                return grandparents;
            }
        }

        public List<Person> GetAllAncestors()
        {
            Queue<Person> queue = new Queue<Person>();
            List<Person> ancestors = new List<Person>();

            queue.Enqueue(this);

            while (queue.Count != 0)
            {
                Person person = queue.Dequeue();

                foreach (Person parent in person.GetParents)
                    if (parent != null && !ancestors.Contains(parent))
                    {
                        queue.Enqueue(parent);
                        ancestors.Add(parent);
                    }
            }
            return ancestors;
        }

        public List<Person> GetAllDescendants()
        {
            Queue<Person> queue = new Queue<Person>();
            List<Person> descendants = new List<Person>();

            queue.Enqueue(this);

            while (queue.Count != 0)
            {
                Person person = queue.Dequeue();

                foreach (Person child in person.Children)
                    if (!descendants.Contains(child))
                    {
                        queue.Enqueue(child);
                        descendants.Add(child);
                    }
            }
            return descendants;
        }

        public Person GetMostDistantAncestorInMaleLine()
        {
            if (Father != null)
                return Father.GetMostDistantAncestorInMaleLine();
            return this;
        }

        public Person GetMostDistantAncestorInFemaleLine()
        {
            if (Mother != null)
                return Mother.GetMostDistantAncestorInFemaleLine();
            return this;
        }

        public bool IsThisEmptyPerson
        {
            get { return Name == "?" && BirthDate == DateTime.MinValue && DeathDate == DateTime.MinValue; }
        }

        public override string ToString()
        {
            if (MarriedSurname != null)
            {
                if (Surname != null)
                    return $"{Name} {MarriedSurname} z d. {Surname}";
                else
                    return $"{Name} {MarriedSurname}";
            }
            return $"{Name} {Surname}";
        }

        public string ShowLongerPersonDescription()
        {
            if (GetBirthDate != "" && GetDeathDate != "")
                return $"{ToString()}, {GetBirthDate} - {GetDeathDate}";
            if (GetBirthDate != "" && GetDeathDate == "")
                return $"{ToString()}, ur. {GetBirthDate}";
            if (GetBirthDate == "" && GetDeathDate != "")
                return $"{ToString()}, zm. {GetDeathDate}";
            return ToString();
        }
    }
}
