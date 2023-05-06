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
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.IO;
using System.Text.RegularExpressions;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace FamilyTreeManager
{
    /// <summary>
    /// Interaction logic for PersonInfo.xaml
    /// </summary>
    public partial class PersonInfo : Window
    {
        private Person _probant;
        private string _imagesDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString() + "\\Images";

        internal PersonInfo(Person probant)
        {
            InitializeComponent();

            _probant = probant;
            personLabel.Content = probant.ShowLongerPersonDescription();

            PopulateTreeViews();

            DisplayPortraitImageOrPlaceholder();

            DisplayMostDistantAncestorInMaleLine();
            DisplayMostDistantAncestorInFemaleLine();

            ShowPieChartWithNationalities(probant);
        }

        //public IEnumerable<ISeries> Series { get; set; }

        private void DisplayPortraitImageOrPlaceholder()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("D:/Marek/Marek/Pliki Graficzne/Gry/The Sims 3/Simowie");
            FileInfo[] files = directoryInfo.GetFiles($"{_probant.Name}*");
            BitmapImage bitmapImage;
            int index = -1;

            if (files.Length == 1)
                index = 0;
            else if (files.Length > 1)
            {
                Regex regex = new Regex(@"\b" + _probant.Name + @"\b");
                index = Array.FindIndex(files, file => regex.Match(file.Name.Split()[0]).Success);
            }

            if (index != -1)
                bitmapImage = new BitmapImage(new Uri(files[index].FullName, UriKind.Absolute));
            else
            {
                string sexImage = _probant.IsMale ? _imagesDirectory + "\\Man.png" : _imagesDirectory + "\\Woman.png";
                bitmapImage = new BitmapImage(new Uri(sexImage, UriKind.Absolute));
            }

            portraitImage.Source = bitmapImage;
        }

        private void PopulateTreeViews()
        {
            int grandchildrenCount = 0;

            foreach (Person grandparent in _probant.GetGrandparents)
                if (grandparent != null)
                    CreateTreeViewItem(grandparent, grandparentsTreeView, grandparent.IsMale ? "Dziadek" : "Babcia");

            foreach (Person parent in _probant.GetParents)
                if (parent != null)
                    CreateTreeViewItem(parent, parentsTreeView, parent.IsMale ? "Ojciec" : "Matka");

            foreach (Person sibling in _probant.GetSiblings)
                CreateTreeViewItem(sibling, siblingsTreeView, sibling.IsMale ? "Brat" : "Siostra");

            foreach (Person partner in _probant.Partners)
                CreateTreeViewItem(partner, partnersTreeView, partner.IsMale ? "Mąż/Partner" : "Żona/Partnerka");

            foreach (Person child in _probant.Children)
            {
                TreeViewItem treeViewItem = CreateTreeViewItem(child, descendantsTreeView, child.IsMale ? "Syn" : "Córka");
                treeViewItem.IsExpanded = true;

                foreach (Person grandchild in child.Children)
                {
                    TreeViewItem subTreeViewItem = CreateTreeViewItem(grandchild, treeViewItem, grandchild.IsMale ? "Wnuk" : "Wnuczka");
                    grandchildrenCount++;
                }
            }

            descendantsTreeView.Header = $"Dzieci i wnuki [{_probant.Children.Count}/{grandchildrenCount}]";
        }

        private TreeViewItem CreateTreeViewItem(Person person, TreeViewItem precedentTreeViewItem, string kinshipName)
        {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = person.ToString();
            treeViewItem.Cursor = Cursors.Hand;
            treeViewItem.DataContext = person;
            //treeViewItem.MouseDoubleClick += Person_MouseDoubleClick;
            precedentTreeViewItem.Items.Add(treeViewItem);
            ChangeTreeViewFontWeight(person, treeViewItem);

            //SetToolTip(person, treeViewItem, kinshipName);

            return treeViewItem;
        }

        private void ChangeTreeViewFontWeight(Person person, TreeViewItem treeViewItem)
        {
            if (person.Children.Count == 0)
                treeViewItem.FontWeight = FontWeights.Normal;
        }

        private void ShowPieChartWithNationalities(Person person)
        {
            if (person.Nationalities.Count == 0)
                return;

            List<ISeries> series = new List<ISeries>();

            foreach (KeyValuePair<string, double> keyValuePair in person.Nationalities)
            {
                PieSeries<double> pieSeries = new PieSeries<double>
                {
                    Values = new double[] { keyValuePair.Value },
                    Name = $"{keyValuePair.Key} {(keyValuePair.Value >= 1 ? keyValuePair.Value : keyValuePair.Value.ToString("N2"))}%",
                    DataLabelsSize = 19,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{keyValuePair.Key}{Environment.NewLine} {point.PrimaryValue:N2}%"
                };

                series.Add(pieSeries);
            }

            nationalitiesPieChart.Series = series;

            nationalitiesPieChart.Title = new LabelVisual
            {
                Text = "Narodowości",
                TextSize = 28,
                Paint = new SolidColorPaint(SKColors.Black)
            };
        }

        private void DisplayMostDistantAncestorInMaleLine()
        {
            Person ancestor = _probant.GetMostDistantAncestorInMaleLine();

            if (ancestor != _probant)
                mostDistantMaleAncestor.Content = ancestor.ShowLongerPersonDescription();
            else
                mostDistantMaleAncestorTitle.Visibility = Visibility.Hidden;
        }

        private void DisplayMostDistantAncestorInFemaleLine()
        {
            Person ancestor = _probant.GetMostDistantAncestorInFemaleLine();

            if (ancestor != _probant)
                mostDistantFemaleAncestor.Content = ancestor.ShowLongerPersonDescription();
            else
                mostDistantFemaleAncestorTitle.Visibility = Visibility.Hidden;
        }

        private void ancestorsButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            List<Person> ancestors = _probant.GetAllAncestors();
            ancestorsLabel.Content = ancestors.Count;
        }

        private void descendantsButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            List<Person> descendants = _probant.GetAllDescendants();
            descendantsLabel.Content = descendants.Count;
        }
    }
}