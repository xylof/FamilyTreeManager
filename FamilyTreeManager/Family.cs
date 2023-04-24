using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTreeManager
{
    class Family
    {
        public enum RelationTypeEnum { Partner, Enga, Marr }

        public string ID { get; set; }
        public Person Father { get; set; }
        public Person Mother { get; set; }
        public List<Person> Children { get; set; }
        public RelationTypeEnum RelationType { get; set; }
    }
}
