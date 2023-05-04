using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FamilyTreeManager
{
    /// <summary>
    /// Interaction logic for NamesAndSurnames.xaml
    /// </summary>
    public partial class NamesAndSurnames : Window
    {
        private List<Person> _ancestors;

        internal NamesAndSurnames(Person probant)
        {
            InitializeComponent();

            personLabel.Content = probant.ShowLongerPersonDescription();
            _ancestors = probant.GetAllAncestors();

            ExtractSurnames();
            ExtractNames();
        }

        private void ExtractSurnames()
        {
            Predicate<Person> surnamesPredicate = ancestor => ancestor.Surname != null;
            Func<Person, string> surnamesFunc = ancestor => ancestor.Surname;
            SortedDictionary<string, int> sortedSurnamesWithOccurences = GetAllAncestorsNames(surnamesPredicate, surnamesFunc);
            DisplayNames(sortedSurnamesWithOccurences, surnamesStackPanel);
        }

        private void ExtractNames()
        {
            Predicate<Person> namesPredicate = ancestor => ancestor.Name != "?";
            Func<Person, string> namesFunc = ancestor => (bool)multinamesCheckBox.IsChecked ? ancestor.Name : ancestor.Name.Split()[0];
            SortedDictionary<string, int> sortedNamesWithOccurences = GetAllAncestorsNames(namesPredicate, namesFunc);
            DisplayNames(sortedNamesWithOccurences, namesStackPanel);
        }

        private SortedDictionary<string, int> GetAllAncestorsNames(Predicate<Person> predicate, Func<Person, string> func)
        {
            IEnumerable<string> names = _ancestors.FindAll(predicate).Select(func);
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

        private void multinamesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            namesStackPanel.Children.Clear();
            ExtractNames();
        }

        private void multinamesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            namesStackPanel.Children.Clear();
            ExtractNames();
        }
    }
}