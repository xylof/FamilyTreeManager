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
            List<(int sosaNumber, Person person)> ancestors = GetAncestorsInBFSOrder(probant);
            ShowAncestors(ancestors);
        }

        private List<(int sosaNumber, Person person)> GetAncestorsInBFSOrder(Person probant)
        {
            Queue<(int sosaNumber, Person person)> queue = new Queue<(int, Person)>();
            List<(int sosaNumber, Person person)> ancestors = new List<(int, Person)>();

            queue.Enqueue(new(1, probant));
            ancestors.Add(new(1, probant));

            while (queue.Count != 0)
            {
                (int sosaNumber, Person person) tuple = queue.Dequeue();
                Person currentPerson = tuple.person;
                int sosaNumber = tuple.sosaNumber;

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

        private void ShowAncestors(List<(int sosaNumber, Person person)> ancestors)
        {
            int currentGenerationNumber = 0;

            foreach ((int sosaNumber, Person person) tuple in ancestors)
            {
                double parentGenerationNumber = Math.Floor(Math.Log2(tuple.sosaNumber)) + 1;

                if (parentGenerationNumber > currentGenerationNumber)
                {
                    currentGenerationNumber++;

                    TextBlock generationTextBlock = new TextBlock()
                    {
                        Text = $"Pokolenie {currentGenerationNumber}",
                        FontWeight = FontWeights.Bold,
                        FontSize = 16
                    };

                    stackPanel.Children.Add(new Label());
                    stackPanel.Children.Add(generationTextBlock);
                }

                TextBlock personTextBlock = new TextBlock()
                {
                    Text = $"{tuple.sosaNumber}. {tuple.person.ShowLongerPersonDescription()}",
                    FontSize = 16,
                    DataContext = tuple
                };

                personTextBlock.MouseRightButtonDown += textBlock_RightClick;
                stackPanel.Children.Add(personTextBlock);
            }
        }

        private (int sosaNumber, Person person) _clickedPersonWithSosaNumber;

        private void textBlock_RightClick(object sender, RoutedEventArgs e)
        {           
            ContextMenu contextMenu = FindResource("cmTextBlock") as ContextMenu;
            contextMenu.PlacementTarget = sender as TextBlock;
            contextMenu.IsOpen = true;

            _clickedPersonWithSosaNumber = ((int sosaNumber, Person person))(sender as TextBlock).DataContext;
        }

        private int _counter = 0;

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            DistinguishPeopleIndicatedByCondition(tuple => tuple.person == _clickedPersonWithSosaNumber.person);
            infoTextBlock.Text = $"{_clickedPersonWithSosaNumber.person} występuje {_counter} razy";
            _counter = 0;
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            int sosaNumber = _clickedPersonWithSosaNumber.sosaNumber;
            List<int> sosaNumbers = new List<int> { sosaNumber };

            while (sosaNumber != 1)
            {
                sosaNumber /= 2;
                sosaNumbers.Add(sosaNumber);
            }

            DistinguishPeopleIndicatedByCondition(tuple => sosaNumbers.Contains(tuple.sosaNumber));
            infoTextBlock.Text = "";
            _counter = 0;
        }

        private void DistinguishPeopleIndicatedByCondition(Func<(int sosaNumber, Person person), bool> predicate)
        {
            foreach (var stackPanelElement in stackPanel.Children)
            {
                TextBlock textBlock = stackPanelElement as TextBlock;

                if (textBlock is TextBlock && textBlock.DataContext != null)
                {
                    (int sosaNumber, Person person) currentPersonWithSosaNumber = ((int sosaNumber, Person person))textBlock.DataContext;
                    
                    textBlock.Text = textBlock.Text.Trim();

                    if (predicate(currentPersonWithSosaNumber))
                    {
                        textBlock.Text = $"  {textBlock.Text}";
                        textBlock.FontWeight = FontWeights.Bold;
                        textBlock.FontStyle = FontStyles.Italic;
                        textBlock.Foreground = Brushes.DarkRed;

                        kfddl();
                    }
                    else
                    {
                        textBlock.FontWeight = FontWeights.Normal;
                        textBlock.FontStyle = FontStyles.Normal;
                        textBlock.Foreground = Brushes.Black;
                    }
                }
            }
        }

        private void kfddl()
        {
            _counter++;
        }
    }
}
