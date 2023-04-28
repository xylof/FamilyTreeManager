using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal SosaAncestorsList(Person probant)
        {
            InitializeComponent();
            List<(int number, Person person)> ancestors = GetAncestorsInBFSOrder(probant);
            ShowAncestors(ancestors);
        }

        private List<(int number, Person person)> GetAncestorsInBFSOrder(Person probant)
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

        private void ShowAncestors(List<(int number, Person person)> ancestors)
        {
            int currentGenerationNumber = 0;

            foreach ((int number, Person person) tuple in ancestors)
            {
                double parentGeneration = Math.Floor(Math.Log2(tuple.number)) + 1;

                if (parentGeneration > currentGenerationNumber)
                {
                    currentGenerationNumber++;

                    TextBlock generationTextBlock = new TextBlock()
                    {
                        Text = $"Pokolenie {currentGenerationNumber}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 16,
                    };

                    stackPanel.Children.Add(new Label());
                    stackPanel.Children.Add(generationTextBlock);
                }

                TextBlock personTextBlock = new TextBlock()
                {
                    Text = $"{tuple.number}. {tuple.person.ShowLongerPersonDescription()}",
                    FontSize = 16,
                    DataContext = tuple.person
                };

                stackPanel.Children.Add(personTextBlock);
            }
        }

        private void lolo_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in stackPanel.Children)
            {
                var t = item as TextBlock;

                if(t is TextBlock)
                {
                    if (t.Text.Contains("some text"))
                        t.FontWeight = FontWeights.Bold;
                }

            }
        }
    }
}
