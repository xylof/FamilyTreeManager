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
    /// Interaction logic for NamesAndSurnames.xaml
    /// </summary>
    public partial class NamesAndSurnames : Window
    {
        internal NamesAndSurnames(Person probant)
        {
            InitializeComponent();

            personLabel.Content = probant.ShowLongerPersonDescription();

            Predicate<Person> surnamesPredicate = ancestor => ancestor.Surname != null;
            Func<Person, string> surnamesFunc = ancestor => ancestor.Surname;
            SortedDictionary<string, int> sortedSurnamesWithOccurences = GetAllAncestorsNames(probant, surnamesPredicate, surnamesFunc);
            DisplayNames(sortedSurnamesWithOccurences, surnamesStackPanel);

            Predicate<Person> namesPredicate = ancestor => ancestor.Name != "?";
            Func<Person, string> namesFunc = ancestor => ancestor.Name;
            SortedDictionary<string, int> sortedNamesWithOccurences = GetAllAncestorsNames(probant, namesPredicate, namesFunc);
            DisplayNames(sortedNamesWithOccurences, namesStackPanel);
        }

        private SortedDictionary<string, int> GetAllAncestorsNames(Person probant, Predicate<Person> predicate, Func<Person, string> func)
        {
            List<Person> ancestors = probant.GetAllAncestors();

            IEnumerable<string> names = ancestors.FindAll(predicate).Select(func);
            Dictionary<string, int> namesWithOccurences = names.GroupBy(name => name).ToDictionary(group => group.Key, group => group.Count());
            SortedDictionary<string, int> sortedNamesWithOccurences = new SortedDictionary<string, int>(namesWithOccurences);

            return sortedNamesWithOccurences;
        }

        private void DisplayNames(SortedDictionary<string, int> sortedNamesWithOccurences, StackPanel parentStackPanel)
        {
            foreach (KeyValuePair<string, int> keyValuePair in sortedNamesWithOccurences)
            {
                string name = keyValuePair.Key;
                int occurences = keyValuePair.Value;

                Label label = new Label()
                {
                    Content = $"{name} ({occurences})",
                    FontSize = 16
                };

                parentStackPanel.Children.Add(label);
            }
        }
    }
}