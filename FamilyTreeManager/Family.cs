﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTreeManager
{
    class Family
    {
        public enum RelationTypeEnum { Partner, Engagement, Marriage }

        public string ID { get; set; }
        public Person Husband { get; set; }
        public Person Wife { get; set; }
        public List<Person> Children { get; set; }
        public RelationTypeEnum RelationType { get; set; }
        public DateTime WeddingDate { get; set; }
        public string WeddingPlace { get; set; }
        public bool IsWeddingDateEstimated { get; set; }

        public Family()
        {
            Children = new List<Person>();
        }       

        public void AddChild(Person child)
        {
            Children.Add(child);
        }

        public void CompleteFamilyConnections()
        {
            if (Husband != null && Wife != null)
            {
                Husband.AddPartner(Wife);
                Wife.AddPartner(Husband);
            }

            foreach (Person child in Children)
            {
                if (Husband != null)
                {
                    child.Father = Husband;
                    Husband.AddChild(child);
                }

                if (Wife != null)
                {
                    child.Mother = Wife;
                    Wife.AddChild(child);
                }
            }
        }

        public override string ToString()
        {
            return $"{Husband} + {Wife}";
        }
    }
}
