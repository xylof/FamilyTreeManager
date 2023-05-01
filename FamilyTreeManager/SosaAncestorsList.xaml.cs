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
        Person _probant;
        private (int sosaNumber, Person person) _clickedPersonWithSosaNumber;
        private (int sosaNumber, Person person) _lastClickedPersonWithSosaNumber;
        private LastOptionClicked _lastOptionClicked;

        private enum LastOptionClicked
        {
            None, Option1, Option2, Option3
        };

        internal SosaAncestorsList(Person probant)
        {
            InitializeComponent();

            _probant = probant;
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
                        FontWeight = FontWeights.DemiBold,
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

        private void textBlock_RightClick(object sender, RoutedEventArgs e)
        {
            ContextMenu contextMenu = FindResource("cmTextBlock") as ContextMenu;
            contextMenu.PlacementTarget = sender as TextBlock;
            contextMenu.IsOpen = true;

            _lastClickedPersonWithSosaNumber = _clickedPersonWithSosaNumber;
            _clickedPersonWithSosaNumber = ((int sosaNumber, Person person))(sender as TextBlock).DataContext;
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            if (_lastOptionClicked == LastOptionClicked.Option1 && _lastClickedPersonWithSosaNumber == _clickedPersonWithSosaNumber)
                return;

            List<(int sosaNumber, Person person)> distinguishedPeople = DistinguishPeopleIndicatedByCondition(tuple => tuple.person == _clickedPersonWithSosaNumber.person);
            Dictionary<double, int> generationNumbersOccurences = new Dictionary<double, int>();

            foreach ((int sosaNumber, Person person) tuple in distinguishedPeople)
            {
                double generationNumber = Math.Floor(Math.Log2(tuple.sosaNumber)) + 1;

                if (!generationNumbersOccurences.ContainsKey(generationNumber))
                    generationNumbersOccurences[generationNumber] = 1;
                else
                    generationNumbersOccurences[generationNumber]++;
            }

            infoStackPanel.Children.RemoveRange(1, infoStackPanel.Children.Count - 1);
            infoTextBlock.Text = $"Liczba wystąpień osoby {_clickedPersonWithSosaNumber.person} na liście wynosi {distinguishedPeople.Count}, w tym:";

            foreach (KeyValuePair<double, int> keyValuePair in generationNumbersOccurences)
            {
                TextBlock textBlock = new TextBlock()
                {
                    Text = $"{keyValuePair.Value} raz{(keyValuePair.Value > 1 ? "y" : "")} w pokoleniu nr {keyValuePair.Key}",
                    TextWrapping = TextWrapping.Wrap,
                    Width = 500,
                    FontSize = 20
                };

                infoStackPanel.Children.Add(textBlock);
            }

            _lastOptionClicked = LastOptionClicked.Option1;
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            if (_lastOptionClicked == LastOptionClicked.Option2 && _lastClickedPersonWithSosaNumber == _clickedPersonWithSosaNumber)
                return;

            int sosaNumber = _clickedPersonWithSosaNumber.sosaNumber;
            List<int> sosaNumbers = new List<int> { sosaNumber };

            while (sosaNumber != 1)
            {
                sosaNumber /= 2;
                sosaNumbers.Add(sosaNumber);
            }

            DistinguishPeopleIndicatedByCondition(tuple => sosaNumbers.Contains(tuple.sosaNumber));

            infoStackPanel.Children.RemoveRange(1, infoStackPanel.Children.Count - 1);
            infoTextBlock.Text = $"Linia łącząca osobę {_clickedPersonWithSosaNumber.person} z probantem {_probant}";

            _lastOptionClicked = LastOptionClicked.Option2;
        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
        {
            if (_lastOptionClicked == LastOptionClicked.Option3 && _lastClickedPersonWithSosaNumber == _clickedPersonWithSosaNumber)
                return;

            List<(int sosaNumber, Person person)> distinguishedPeople = DistinguishPeopleIndicatedByCondition(tuple => tuple.person == _clickedPersonWithSosaNumber.person);

            List<int> sosaNumbers = new List<int>();
            sosaNumbers.AddRange(distinguishedPeople.Select(tuple => tuple.sosaNumber));

            foreach (int sosaNumber in new List<int>(sosaNumbers))
            {
                int sosaNumberCopy = sosaNumber;

                while (sosaNumberCopy != 1)
                {
                    sosaNumberCopy /= 2;
                    sosaNumbers.Add(sosaNumberCopy);
                }
            }

            sosaNumbers = sosaNumbers.Distinct().ToList();

            DistinguishPeopleIndicatedByCondition(tuple => sosaNumbers.Contains(tuple.sosaNumber));

            infoStackPanel.Children.RemoveRange(1, infoStackPanel.Children.Count - 1);
            infoTextBlock.Text = $"Wszystkie linie łączące osobę {_clickedPersonWithSosaNumber.person} z probantem {_probant}";

            _lastOptionClicked = LastOptionClicked.Option3;
        }

        private List<(int sosaNumber, Person person)> DistinguishPeopleIndicatedByCondition(Func<(int sosaNumber, Person person), bool> predicate)
        {
            List<(int sosaNumber, Person person)> distinguishedPeople = new List<(int sosaNumber, Person person)>();

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

                        distinguishedPeople.Add(currentPersonWithSosaNumber);
                    }
                    else
                    {
                        textBlock.FontWeight = FontWeights.Normal;
                        textBlock.FontStyle = FontStyles.Normal;
                    }
                }
            }

            return distinguishedPeople;
        }

        private void grandparentsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ColorAncestorsFromEachGrandparent();
            grandparentsDescriptionsStackPanel.Visibility = Visibility.Visible;

            paterGrandfatherLabel.Content = $"Przodkowie dziadka ojczystego: {_probant.GetGrandparents[0]}";
            paterGrandmotherLabel.Content = $"Przodkowie babci ojczystej: {_probant.GetGrandparents[1]}";
            materGrandfatherLabel.Content = $"Przodkowie dziadka macierzystego: {_probant.GetGrandparents[2]}";
            materGrandmotherLabel.Content = $"Przodkowie babci macierzystej: {_probant.GetGrandparents[3]}";
        }

        private void grandparentsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MakeAllPeopleBlack();
            grandparentsDescriptionsStackPanel.Visibility = Visibility.Hidden;
        }

        private void ColorAncestorsFromEachGrandparent()
        {
            int generationNumber = 0;

            double firstGrandfatherMinSosaNumber = 0;
            double firstGrandfatherMaxSosaNumber = 0;

            double firstGrandmotherMinSosaNumber = 0;
            double firstGrandmotherMaxSosaNumber = 0;

            double secondGrandfatherMinSosaNumber = 0;
            double secondGrandfatherMaxSosaNumber = 0;

            double secondGrandmotherMinSosaNumber = 0;
            double secondGrandmotherMaxSosaNumber = 0;

            foreach (var stackPanelElement in stackPanel.Children)
            {
                TextBlock textBlock = stackPanelElement as TextBlock;

                if (textBlock is TextBlock)
                {
                    if (textBlock.DataContext == null)
                    {
                        string[] temp = textBlock.Text.Split();
                        generationNumber = int.Parse(temp.Last());

                        firstGrandfatherMinSosaNumber = 4 * Math.Pow(2, generationNumber - 3);
                        firstGrandfatherMaxSosaNumber = (4 + 1) * Math.Pow(2, generationNumber - 3) - 1;

                        firstGrandmotherMinSosaNumber = 5 * Math.Pow(2, generationNumber - 3);
                        firstGrandmotherMaxSosaNumber = (5 + 1) * Math.Pow(2, generationNumber - 3) - 1;

                        secondGrandfatherMinSosaNumber = 6 * Math.Pow(2, generationNumber - 3);
                        secondGrandfatherMaxSosaNumber = (6 + 1) * Math.Pow(2, generationNumber - 3) - 1;

                        secondGrandmotherMinSosaNumber = 7 * Math.Pow(2, generationNumber - 3);
                        secondGrandmotherMaxSosaNumber = (7 + 1) * Math.Pow(2, generationNumber - 3) - 1;
                    }
                    else if (generationNumber >= 3)
                    {
                        (int sosaNumber, Person person) currentPersonWithSosaNumber = ((int sosaNumber, Person person))textBlock.DataContext;

                        if (firstGrandfatherMinSosaNumber <= currentPersonWithSosaNumber.sosaNumber && currentPersonWithSosaNumber.sosaNumber <= firstGrandfatherMaxSosaNumber)
                            textBlock.Foreground = Brushes.Blue;
                        else if (firstGrandmotherMinSosaNumber <= currentPersonWithSosaNumber.sosaNumber && currentPersonWithSosaNumber.sosaNumber <= firstGrandmotherMaxSosaNumber)
                            textBlock.Foreground = Brushes.Red;
                        else if (secondGrandfatherMinSosaNumber <= currentPersonWithSosaNumber.sosaNumber && currentPersonWithSosaNumber.sosaNumber <= secondGrandfatherMaxSosaNumber)
                            textBlock.Foreground = Brushes.Green;
                        else if (secondGrandmotherMinSosaNumber <= currentPersonWithSosaNumber.sosaNumber && currentPersonWithSosaNumber.sosaNumber <= secondGrandmotherMaxSosaNumber)
                            textBlock.Foreground = Brushes.Brown;
                    }
                }
            }
        }

        private void MakeAllPeopleBlack()
        {
            foreach (var stackPanelElement in stackPanel.Children)
            {
                TextBlock textBlock = stackPanelElement as TextBlock;

                if (textBlock is TextBlock)
                    textBlock.Foreground = Brushes.Black;
            }
        }
    }
}
