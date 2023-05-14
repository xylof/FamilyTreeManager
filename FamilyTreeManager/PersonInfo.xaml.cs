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
        private bool _wasGrandchildClicked;
        private List<Person> _displayedFamilyNodes;
        private int _currentFamilyNodeIndex;
        private string _imagesDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString()).ToString() + "\\Images";

        internal PersonInfo(Person probant, List<Person> displayedFamilyNodes)
        {
            InitializeComponent();
            _probant = probant;
            _displayedFamilyNodes = displayedFamilyNodes;
            _currentFamilyNodeIndex = 0;
            SetWindowControls();
        }

        private void SetWindowControls()
        {
            personLabel.Content = _probant.ShowLongerPersonDescription();

            PopulateTreeViews();

            DisplayPortraitImageOrPlaceholder();

            DisplayMostDistantAncestorInMaleLine();
            DisplayMostDistantAncestorInFemaleLine();

            ShowPieChartWithNationalities();
            ShowPieChartWithDescendantsSex();

            ChoosePieChartToBeVisible();

            SetRadioButtonsVisibility();
        }

        private void DisplayPortraitImageOrPlaceholder()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("D:/Marek/Marek/Pliki Graficzne/Gry/The Sims 3/Simowie");
            FileInfo[] files = directoryInfo.GetFiles($"{_probant.Name}*"); // Filtruję pliki, których nazwy zaczynają się od imienia probanta
            FileInfo[] filesWithProbantName;
            BitmapImage bitmapImage;
            int index = -1;

            if (files.Length == 1)
                index = 0;
            else if (files.Length > 1)
            {
                Regex regex = new Regex(@"\b" + _probant.Name + @"\b");
                filesWithProbantName = Array.FindAll(files, file => regex.Match(file.Name.Split()[0]).Success);

                if (filesWithProbantName.Length == 1)
                {
                    index = 0;
                    files = filesWithProbantName;
                }
                else
                    index = Array.FindIndex(files, file => file.Name.Remove(file.Name.Length - 4, 4) == _probant.Surname); // TODO do poprawienia
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
            treeViewItem.Foreground = person.IsMale ? Brushes.Blue : new SolidColorBrush(Color.FromRgb(163, 0, 112));
            treeViewItem.MouseDoubleClick += Person_MouseDoubleClick;
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

        private void ShowPieChartWithNationalities()
        {
            if (_probant.Nationalities.Count == 0)
                return;

            nationalitiesPieChart = new LiveChartsCore.SkiaSharpView.WPF.PieChart()
            {
                Width = 900,
                Height = 600,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
                Visibility = Visibility.Hidden
            };

            pieChartsGrid.Children.Add(nationalitiesPieChart);

            List<ISeries> series = new List<ISeries>();

            foreach (KeyValuePair<string, double> keyValuePair in _probant.Nationalities)
            {
                PieSeries<double> pieSeries = new PieSeries<double>
                {
                    Values = new double[] { keyValuePair.Value },
                    Name = $"{keyValuePair.Key} {$"{keyValuePair.Value:0.##}"}%",
                    DataLabelsSize = 17,
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{keyValuePair.Key} {$"{point.PrimaryValue:0.##}"}%"
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

        private void ShowPieChartWithDescendantsSex()
        {
            if (_probant.Children.Count == 0)
                return;

            descendantsSexPieChart = new LiveChartsCore.SkiaSharpView.WPF.PieChart()
            {
                Width = 900,
                Height = 600,
                LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
                Visibility = Visibility.Visible
            };

            pieChartsGrid.Children.Add(descendantsSexPieChart);

            List<Person> descendants = _probant.GetAllDescendants();
            List<ISeries> series = new List<ISeries>();

            PieSeries<double> pieSeries = new PieSeries<double>
            {
                Values = new double[] { descendants.FindAll(per => per.IsMale).Count },
                Name = "Mężczyźni",
                Fill = new SolidColorPaint(SKColors.LightSkyBlue),
                DataLabelsSize = 19,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black)
            };

            series.Add(pieSeries);

            pieSeries = new PieSeries<double>
            {
                Values = new double[] { descendants.FindAll(per => !per.IsMale).Count },
                Name = "Kobiety",
                Fill = new SolidColorPaint(SKColors.HotPink),
                DataLabelsSize = 19,
                DataLabelsPaint = new SolidColorPaint(SKColors.Black)
            };

            series.Add(pieSeries);

            descendantsSexPieChart.Series = series;

            descendantsSexPieChart.Title = new LabelVisual
            {
                Text = "Płeć potomków",
                TextSize = 28,
                Paint = new SolidColorPaint(SKColors.Black)
            };
        }

        private void DisplayMostDistantAncestorInMaleLine()
        {
            Person ancestor = _probant.MostDistantAncestorInMaleLine;

            if (ancestor != null)
            {
                mostDistantMaleAncestorTitle.Visibility = Visibility.Visible;
                mostDistantMaleAncestor.Content = ancestor.ShowLongerPersonDescription();
            }
            else
                mostDistantMaleAncestorTitle.Visibility = Visibility.Hidden;
        }

        private void DisplayMostDistantAncestorInFemaleLine()
        {
            Person ancestor = _probant.MostDistantAncestorInFemaleLine;

            if (ancestor != null)
            {
                mostDistantFemaleAncestorTitle.Visibility = Visibility.Visible;
                mostDistantFemaleAncestor.Content = ancestor.ShowLongerPersonDescription();
            }
            else
                mostDistantFemaleAncestorTitle.Visibility = Visibility.Hidden;
        }

        private void SetRadioButtonsVisibility()
        {
            if (_probant.Nationalities.Count == 0)
                nationalitiesRadioButton.Visibility = Visibility.Hidden;
            else
                nationalitiesRadioButton.Visibility = Visibility.Visible;

            if (_probant.Children.Count == 0)
                descendantsRadioButton.Visibility = Visibility.Hidden;
            else
                descendantsRadioButton.Visibility = Visibility.Visible;
        }

        private void ResetWindow()
        {
            grandparentsTreeView.Items.Clear();
            parentsTreeView.Items.Clear();
            siblingsTreeView.Items.Clear();
            partnersTreeView.Items.Clear();
            descendantsTreeView.Items.Clear();

            ancestorsButton.IsEnabled = true;
            descendantsButton.IsEnabled = true;
            ancestorsLabel.Content = null;
            descendantsLabel.Content = null;

            mostDistantMaleAncestorTitle.Visibility = Visibility.Hidden;
            mostDistantMaleAncestor.Content = null;
            mostDistantFemaleAncestorTitle.Visibility = Visibility.Hidden;
            mostDistantFemaleAncestor.Content = null;

            nationalitiesPieChart.Series = null;
            nationalitiesPieChart.Title = null;
            descendantsSexPieChart.Series = null;
            descendantsSexPieChart.Title = null;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFamilyNodeIndex--;
            _probant = _displayedFamilyNodes[_currentFamilyNodeIndex];
            ResetWindow();
            SetWindowControls();

            forwardButton.IsEnabled = true;
            forwardButton.Opacity = 1;

            if (_currentFamilyNodeIndex == 0)
            {
                backButton.IsEnabled = false;
                backButton.Opacity = 0.6;
            }
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            _currentFamilyNodeIndex++;
            _probant = _displayedFamilyNodes[_currentFamilyNodeIndex];
            ResetWindow();
            SetWindowControls();

            if (_currentFamilyNodeIndex == _displayedFamilyNodes.Count - 1)
            {
                forwardButton.IsEnabled = false;
                forwardButton.Opacity = 0.6;
            }

            if (_displayedFamilyNodes.Count > 1)
            {
                backButton.IsEnabled = true;
                backButton.Opacity = 1;
            }
        }

        private void Polygon_MouseMove(object sender, MouseEventArgs e)
        {
            Polygon polygon = (Polygon)sender;
            polygon.Fill = Brushes.Aqua;
        }

        private void Polygon_MouseLeave(object sender, MouseEventArgs e)
        {
            Polygon polygon = (Polygon)sender;
            polygon.Fill = Brushes.Brown;
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

        private void descendantsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MakeDescendantsSexPieChartVisible();
        }

        private void nationalitiesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MakeNationalitiesPieChartVisible();
        }

        private void MakeDescendantsSexPieChartVisible()
        {
            descendantsSexPieChart.Visibility = Visibility.Visible;
            nationalitiesPieChart.Visibility = Visibility.Hidden;
        }

        private void MakeNationalitiesPieChartVisible()
        {
            descendantsSexPieChart.Visibility = Visibility.Hidden;
            nationalitiesPieChart.Visibility = Visibility.Visible;
        }

        private void ChoosePieChartToBeVisible()
        {
            if ((bool)descendantsRadioButton.IsChecked)
                MakeDescendantsSexPieChartVisible();
            else
                MakeNationalitiesPieChartVisible();
        }

        private void Person_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // Dzięki tej linijce event wykonuje się tylko raz (z wyjątkiem kliknięcia we wnuka)

            if (_wasGrandchildClicked)
            {
                _wasGrandchildClicked = false;
                return;
            }

            if (sender is TreeViewItem)
            {
                TreeViewItem treeViewItem = (TreeViewItem)sender;
                _probant = (Person)treeViewItem.DataContext;

                if (((TreeViewItem)treeViewItem.Parent).DataContext is Person)
                    _wasGrandchildClicked = true;
            }
            else if (sender is TextBlock && e.ClickCount == 2)
                _probant = (Person)((TextBlock)sender).DataContext;
            else
                return;

            if (_displayedFamilyNodes.Count > _currentFamilyNodeIndex + 1)
            {
                _displayedFamilyNodes.RemoveRange(_currentFamilyNodeIndex + 1, _displayedFamilyNodes.Count - _currentFamilyNodeIndex - 1);
                forwardButton.IsEnabled = false;
                forwardButton.Opacity = 0.6;
            }

            _displayedFamilyNodes.Add(_probant);
            _currentFamilyNodeIndex++;

            backButton.IsEnabled = true;
            backButton.Opacity = 1;

            ResetWindow();
            SetWindowControls();
        }
    }
}