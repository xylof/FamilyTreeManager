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
    /// Interaction logic for MaleAndFemaleLines.xaml
    /// </summary>
    public partial class MaleAndFemaleLines : Window
    {
        private List<Person> _people;

        internal MaleAndFemaleLines(List<Person> people)
        {
            InitializeComponent();
            _people = people;

            FindAllLinesOfGivenSex(maleLinesTreeView, person => person.IsMale && person.Father == null, child => child.IsMale);
            FindAllLinesOfGivenSex(femaleLinesTreeView, person => !person.IsMale && person.Mother == null, child => !child.IsMale);

            DisplayLinesWithGivenPeopleAmount(minMaleLinesTextBox, maxMaleLinesTextBox, maleLinesTreeView);
            DisplayLinesWithGivenPeopleAmount(minFemaleLinesTextBox, maxFemaleLinesTextBox, femaleLinesTreeView);
        }

        private void FindAllLinesOfGivenSex(TreeView rootTreeView, Predicate<Person> progenitorPredicate, Predicate<Person> childSexPredicate)
        {
            List<Person> progenitors = _people.FindAll(progenitorPredicate);

            foreach (Person progenitor in progenitors)
                GetAllDescendantsOfGivenSex(progenitor, rootTreeView, childSexPredicate);
        }

        private void GetAllDescendantsOfGivenSex(Person progenitor, TreeView rootTreeView, Predicate<Person> childSexPredicate)
        {
            Queue<(Person person, TreeViewItem treeViewItem, int generation)> queue = new Queue<(Person person, TreeViewItem treeViewItem, int generation)>();
            int peopleCounter = 1;

            TreeViewItem progenitorTreeViewItem = new TreeViewItem()
            {
                Header = $"1) {progenitor.ShowLongerPersonDescription()}",
                FontSize = 18,
                Margin = new Thickness(35, -20, 0, 0),
                FontWeight = FontWeights.Bold,
                Focusable = false,
                IsExpanded = true
            };

            CheckBox checkBox = new CheckBox()
            {
                Margin = new Thickness(0, 20, 0, 0),
                DataContext = progenitorTreeViewItem
            };
            checkBox.Checked += CheckBox_Checked;
            checkBox.Unchecked += CheckBox_Unchecked;

            rootTreeView.Items.Add(checkBox);
            rootTreeView.Items.Add(progenitorTreeViewItem);

            queue.Enqueue((progenitor, progenitorTreeViewItem, 1));

            while (queue.Count != 0)
            {
                var person_TreeViewItem_Generation = queue.Dequeue();

                Person parent = person_TreeViewItem_Generation.person;
                TreeViewItem parentTreeViewItem = person_TreeViewItem_Generation.treeViewItem;
                int parentGeneration = person_TreeViewItem_Generation.generation;

                foreach (Person child in parent.Children.FindAll(childSexPredicate))
                {
                    TreeViewItem childTreeViewItem = new TreeViewItem()
                    {
                        Header = $"{parentGeneration + 1}) {child.ShowLongerPersonDescription()}",
                        FontWeight = FontWeights.DemiBold,
                        IsExpanded = true,
                        DataContext = child
                    };

                    parentTreeViewItem.Items.Add(childTreeViewItem);

                    queue.Enqueue((child, childTreeViewItem, parentGeneration + 1));

                    peopleCounter++;
                }
            }

            progenitorTreeViewItem.DataContext = (progenitor, peopleCounter);
        }

        // W poniższej metodzie duża część kodu jest związana z tym, że gdy dana linia (ród) jest ustawiana jako niewidoczna, to powiązany z nią CheckBox jest przenoszony z jednego miejsca w drugie.
        // Nie wystarczy mu po prostu ustawić właściwości Visibility jako Collapsed, bo wtedy pozostaje po nim sztucznie rozbuchana pusta przestrzeń, a im więcej takich przestrzeni obok siebie,
        // tym gorzej to wygląda na ekranie. Dlatego trzeba było zastosować trik polegający na tym, aby, stosując analogię, zdejmować CheckBoxa ze stołu i chować go do kufra pod stołem, gdy linia ma
        // być niewidoczna oraz wyjmowac go z kufra i z powrotem stawiać na stole, gdy linia ma być widoczna
        private void DisplayLinesWithGivenPeopleAmount(TextBox minTextBox, TextBox maxTextBox, TreeView sexTreeView)
        {
            int minPeopleAmount = int.Parse(minTextBox.Text);
            int maxPeopleAmount = int.Parse(maxTextBox.Text);

            object[] sexTreeViewItemsCopy = new object[sexTreeView.Items.Count]; // Ponieważ niektóre obiekty należące do kontrolki sexTreeView będą z niej usuwane lub do niej dodawane...
            sexTreeView.Items.CopyTo(sexTreeViewItemsCopy, 0); //...trzeba zrobić ich kopię do tablicy, bo inaczej pętla foreach nie mogłaby po nich iterować i wyrzuciłaby błąd

            foreach (var item in sexTreeViewItemsCopy) // W tablicy znajdują się elementy dwóch typów: albo CheckBox albo TreeViewItem
                if (item is CheckBox) // Jeśli element jest CheckBoxem, to wiadomo, że mamy do czynienia z linią (rodem), która jest widoczna na ekranie
                {
                    CheckBox checkBox = item as CheckBox;
                    TreeViewItem progenitorTreeViewItem = checkBox.DataContext as TreeViewItem; // Wyciągamy TreeViewItema powiązanego z aktualnym CheckBoxem

                    var progenitorAndPeopleCounter = ((Person progenitor, int peopleCounter))progenitorTreeViewItem.DataContext;
                    int linePeopleAmount = progenitorAndPeopleCounter.peopleCounter; // Wyciągamy informację o tym, ile osób zawiera aktualna linia (ród)

                    if (minPeopleAmount > linePeopleAmount || linePeopleAmount > maxPeopleAmount) // Jeśli liczba osób w linii NIE mieści się w zakresie wyznaczonym przez wartości min. i maks...
                    {
                        progenitorTreeViewItem.Visibility = Visibility.Collapsed; //...to zwijamy całą linię, żeby nie była widoczna na ekranie...

                        sexTreeView.Items.Remove(item); //...usuwamy poprzedzającego naszą linię CheckBoxa ze zbioru dzieci kontrolki sexTreeView...
                        progenitorTreeViewItem.Items.Add(item); //...i "sztucznie" umieszczamy go na liście dzieci naszego protoplasty
                    }
                }
                else if (item is TreeViewItem) // Jeśli element jest typu TreeViewItem, to znaczy, że MOŻEMY mieć do czynienia z linią (rodem), która NIE jest widoczna na ekranie
                {
                    TreeViewItem progenitorTreeViewItem = item as TreeViewItem;

                    if (progenitorTreeViewItem.Visibility == Visibility.Visible) // Jeśli aktualna linia jest widoczna na ekranie, to wychodzimy z tego miejsca i pętla przechodzi do kolejnego elementu
                        continue;

                    var progenitorAndPeopleCounter = ((Person progenitor, int peopleCounter))progenitorTreeViewItem.DataContext;
                    int linePeopleAmount = progenitorAndPeopleCounter.peopleCounter; // Wyciągamy informację o tym, ile osób zawiera aktualna linia (ród)

                    if (minPeopleAmount <= linePeopleAmount && linePeopleAmount <= maxPeopleAmount) // Jeśli liczba osób w linii mieści się w zakresie wyznaczonym przez wartości min. i maks...
                    {
                        progenitorTreeViewItem.Visibility = Visibility.Visible; //...to czynimy linię widoczną na ekranie...

                        int progenitorTreeViewItemIndex = sexTreeView.Items.IndexOf(progenitorTreeViewItem); //...sprawdzamy, pod jakim indeksem znajduje się ta linia na liście dzieci kotrolki sexTreeView...
                        object progenitorTreeViewItemLastChild = progenitorTreeViewItem.Items[progenitorTreeViewItem.Items.Count - 1]; // ...wyciągamy ostatnie z dzieci naszego protoplasty w nadziei, że będzie to "sztucznie" dodany tam CheckBox (patrz wyżej)

                        if (progenitorTreeViewItemLastChild is CheckBox) // Sprawdzamy, czy wyciągnięty przez nas obiekt to rzeczywiście CheckBox
                        {
                            progenitorTreeViewItem.Items.Remove(progenitorTreeViewItemLastChild); // Jeśli tak, to usuwamy go z listy dzieci naszego protoplasty...
                            sexTreeView.Items.Insert(progenitorTreeViewItemIndex, progenitorTreeViewItemLastChild); //...i wstawiamy go do listy dzieci kotrolki sexTreeView pod takim indeksem, by poprzedzał aktualną linię
                        }
                    }
                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DisplayLinesWithGivenPeopleAmount(minMaleLinesTextBox, maxMaleLinesTextBox, maleLinesTreeView);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DisplayLinesWithGivenPeopleAmount(minFemaleLinesTextBox, maxFemaleLinesTextBox, femaleLinesTreeView);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            checkBox.Background = Brushes.Turquoise;
            TreeViewItem progenitorTreeViewItem = checkBox.DataContext as TreeViewItem;

            var progenitorAndPeopleCounter = ((Person progenitor, int peopleCounter))progenitorTreeViewItem.DataContext;
            Person progenitor = progenitorAndPeopleCounter.progenitor;

            IterateThroughAllItemsInSuitableSexTreeView(progenitor, FontWeights.Heavy);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            checkBox.Background = Brushes.White;
            TreeViewItem progenitorTreeViewItem = checkBox.DataContext as TreeViewItem;

            var progenitorAndPeopleCounter = ((Person progenitor, int peopleCounter))progenitorTreeViewItem.DataContext;
            Person progenitor = progenitorAndPeopleCounter.progenitor;

            IterateThroughAllItemsInSuitableSexTreeView(progenitor, FontWeights.DemiBold);
        }

        private void IterateThroughAllItemsInSuitableSexTreeView(Person progenitor, FontWeight fontWeight)
        {
            TreeView sexTreeView;

            if (progenitor.IsMale)
                sexTreeView = femaleLinesTreeView;
            else
                sexTreeView = maleLinesTreeView;

            foreach (var item in sexTreeView.Items)
                if (item is TreeViewItem)
                {
                    TreeViewItem progenitorTreeViewItem = item as TreeViewItem;
                    ChangeLookOfSuitableTreeViewItems(progenitorTreeViewItem, progenitor, fontWeight);
                }
        }

        private void ChangeLookOfSuitableTreeViewItems(TreeViewItem treeViewItem, Person progenitor, FontWeight fontWeight)
        {
            foreach (var item in treeViewItem.Items)
                if (item is TreeViewItem)
                {
                    TreeViewItem childTreeViewItem = item as TreeViewItem;
                    Person person = childTreeViewItem.DataContext as Person;

                    if (person.MostDistantAncestorInMaleLine == progenitor || person.MostDistantAncestorInFemaleLine == progenitor)
                        childTreeViewItem.FontWeight = fontWeight;

                    ChangeLookOfSuitableTreeViewItems(childTreeViewItem, progenitor, fontWeight);
                }
        }
    }
}