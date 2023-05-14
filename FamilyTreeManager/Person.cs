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
        public Person MostDistantAncestorInMaleLine { get; set; }
        public Person MostDistantAncestorInFemaleLine { get; set; }
        public int RelativesDensityFactor { get; private set; }

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

        // TODO Trzeba raczej sprawdzać, czy probant ma przynajmniej jednego rodzica w drzewie, a jeśli tak, to jakiego
        public bool HasParents
        {
            get { return Father != null && Mother != null; }
        }

        /// <summary>
        /// Dziadkowie
        /// </summary>
        public Person[] GetGrandparents
        {
            get
            {
                Person[] parents = GetParents;
                Person[] grandparents = new Person[4];

                for (int i = 0; i < parents.Length; i++)
                {
                    if (parents[i] != null)
                    {
                        grandparents[i * 2] = parents[i].Father;
                        grandparents[i * 2 + 1] = parents[i].Mother;
                    }
                }

                return grandparents;
            }
        }

        /// <summary>
        /// Pradziadkowie
        /// </summary>
        public Person[] GetGreatGrandparents
        {           
            get
            {
                Person[] grandparents = GetGrandparents;
                Person[] greatGrandparents = new Person[8];

                for (int i = 0; i < grandparents.Length; i++)
                {
                    if (grandparents[i] != null)
                    {
                        greatGrandparents[i * 2] = grandparents[i].Father;
                        greatGrandparents[i * 2 + 1] = grandparents[i].Mother;
                    }
                }

                return greatGrandparents.Distinct().ToArray();
            }
        }

        /// <summary>
        /// Wnuki
        /// </summary>
        public List<Person> GetGrandchildren
        {
            get
            {
                List<Person> grandchildren = new List<Person>();

                foreach (Person child in Children)
                    grandchildren.AddRange(child.Children);

                return grandchildren.Distinct().ToList();
            }
        }

        /// <summary>
        /// Prawnuki
        /// </summary>
        public List<Person> GetGreatGrandchildren
        {
            get
            {
                List<Person> greatGrandchildren = new List<Person>();

                foreach (Person grandchild in GetGrandchildren)
                    greatGrandchildren.AddRange(grandchild.Children);

                return greatGrandchildren.Distinct().ToList();
            }
        }

        /// <summary>
        /// Siostrzeńcy/bratankowie i siostrzenice/bratanice
        /// </summary>
        public List<Person> GetNephewsAndNieces
        {
            get
            {
                List<Person> nephewsAndNieces = new List<Person>();

                foreach (Person sibling in GetSiblings)
                    nephewsAndNieces.AddRange(sibling.Children);

                return nephewsAndNieces.Distinct().ToList();
            }
        }

        /// <summary>
        /// Wnuki wujeczne i cioteczne (dzieci bratanków/siostrzeńców)
        /// </summary>
        public List<Person> GetGreatNephewsAndNieces
        {
            get
            {
                List<Person> greatNephewsAndNieces = new List<Person>();

                foreach (Person nephewOrNiece in GetNephewsAndNieces)
                    greatNephewsAndNieces.AddRange(nephewOrNiece.Children);

                return greatNephewsAndNieces.Distinct().ToList();
            }
        }

        /// <summary>
        /// Ciocie i wujkowie (rodzeństwo rodziców)
        /// </summary>
        public List<Person> GetAuntsAndUncles
        {
            get
            {
                List<Person> auntsAndUncles = new List<Person>();

                foreach (Person parent in GetParents)
                    if (parent != null)
                        auntsAndUncles.AddRange(parent.GetSiblings);

                return auntsAndUncles;
            }
        }

        /// <summary>
        /// Pierwsi kuzyni (dzieci cioć i wujków)
        /// </summary>
        public List<Person> GetFirstCousins
        {
            get
            {
                List<Person> firstCousins = new List<Person>();

                foreach (Person auntOrUncle in GetAuntsAndUncles)
                    firstCousins.AddRange(auntOrUncle.Children);

                return firstCousins.Distinct().ToList();
            }
        }

        /// <summary>
        /// Dzieci pierwszych kuzynów
        /// </summary>
        public List<Person> GetFirstCousinsChildren
        {
            get
            {
                List<Person> firstCousinsChildren = new List<Person>();

                foreach (Person firtstCousin in GetFirstCousins)
                    firstCousinsChildren.AddRange(firtstCousin.Children);

                return firstCousinsChildren.Distinct().ToList();
            }
        }

        /// <summary>
        /// Dzadkowie wujeczni i cioteczni (rodzeństwo dziadków)
        /// </summary>
        public List<Person> GetGreatAuntsAndUncles
        {
            get
            {
                List<Person> greatAuntsAndUncles = new List<Person>();

                foreach (Person grandparent in GetGrandparents)
                    if (grandparent != null)
                        greatAuntsAndUncles.AddRange(grandparent.GetSiblings);

                return greatAuntsAndUncles.Distinct().ToList();
            }
        }

        /// <summary>
        /// Pierwsi kuzyni rodziców (dzieci wujecznych i ciotecznych dziadków)
        /// </summary>
        public List<Person> GetParentsFirstCousins
        {
            get
            {
                List<Person> parentsFirstCousins = new List<Person>();

                foreach (Person parent in GetParents)
                    if (parent != null)
                        parentsFirstCousins.AddRange(parent.GetFirstCousins);

                return parentsFirstCousins.Distinct().ToList();
            }
        }

        /// <summary>
        /// Drudzy kuzyni (dzieci pierwszych kuzynów rodziców)
        /// </summary>
        public List<Person> GetSecondCousins
        {
            get
            {
                List<Person> secondCousins = new List<Person>();

                foreach (Person parentFirstCousin in GetParentsFirstCousins)
                    secondCousins.AddRange(parentFirstCousin.Children);

                return secondCousins.Distinct().ToList();
            }
        }

        public void CalculateRelativesDensityFactor()
        {
            int a = Array.FindAll(GetParents, per => per != null).Length * 100;
            int b = Array.FindAll(GetGrandparents, per => per != null).Length * 80;
            int c = Array.FindAll(GetGreatGrandparents, per => per != null).Length * 60;

            int d = Children.Count * 100;
            int e = GetGrandchildren.Count * 80;
            int f = GetGreatGrandchildren.Count * 60;

            int g = Partners.Count * 80;

            int h = GetSiblings.Count * 100;
            int i = GetNephewsAndNieces.Count * 80;
            int j = GetGreatNephewsAndNieces.Count * 60;

            int k = GetAuntsAndUncles.Count * 80;
            int l = GetFirstCousins.Count * 60;
            int m = GetFirstCousinsChildren.Count * 40;

            int n = GetGreatAuntsAndUncles.Count * 60;
            int o = GetParentsFirstCousins.Count * 40;
            int p = GetSecondCousins.Count * 20;

            RelativesDensityFactor = a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p;
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

        public void SetMostDistantAncestorsInMaleAndFemaleLines()
        {
            Person maleAncestor = GetMostDistantAncestorInMaleLine();
            Person femaleAncestor = GetMostDistantAncestorInFemaleLine();

            MostDistantAncestorInMaleLine = maleAncestor != this ? maleAncestor : null;
            MostDistantAncestorInFemaleLine = femaleAncestor != this ? femaleAncestor : null;
        }

        private Person GetMostDistantAncestorInMaleLine()
        {
            if (Father != null)
                return Father.GetMostDistantAncestorInMaleLine();
            return this;
        }

        private Person GetMostDistantAncestorInFemaleLine()
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
