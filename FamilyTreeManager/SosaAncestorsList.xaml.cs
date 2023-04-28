using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FamilyTreeManager
{
    /// <summary>
    /// Interaction logic for SosaAncestorsList.xaml
    /// </summary>
    public partial class SosaAncestorsList : Window
    {
        internal SosaAncestorsList(Person person)
        {
            InitializeComponent();
            var cc = BFS(person);
        }

        private List<(int number, Person person)> BFS(Person probant)
        {
            Queue<(int number, Person person)> queue = new Queue<(int, Person)>();
            List<(int number, Person person)> ancestors = new List<(int, Person)>();

            queue.Enqueue(new(1, probant));
            ancestors.Add(new(1, probant));

            while (queue.Count != 0)
            {
                (int number, Person person) tuple = queue.Dequeue();
                Person currentPerson = tuple.person;
                int sosaNumber = tuple.number;

                foreach (Person parent in currentPerson.GetParents)
                    if (parent != null)
                    {
                        int parentSosaNumber = sosaNumber * 2; // Przypadek, gdy rodzic jest ojcem

                        if (!parent.IsMale)
                            parentSosaNumber += 1; // Przypadek, gdy rodzic jest matką

                        queue.Enqueue(new(parentSosaNumber, parent));
                        ancestors.Add(new(parentSosaNumber, parent));
                    }
            }
            return ancestors;
        }
    }
}
