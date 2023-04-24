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
        public DateTime DeathDate { get; set; }
        public string DeathPlace { get; set; }
        public string FamsID { get; set; }
        public string FamcID { get; set; }
        public Dictionary<string, double> Nationalities { get; set; }
    }
}
