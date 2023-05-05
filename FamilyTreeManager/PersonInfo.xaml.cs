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

namespace FamilyTreeManager
{
    /// <summary>
    /// Interaction logic for PersonInfo.xaml
    /// </summary>
    public partial class PersonInfo : Window
    {
        internal PersonInfo(Person person)
        {
            InitializeComponent();

            personLabel.Content = person.ShowLongerPersonDescription();

            ShowPieChartWithNationalities(person);
        }

        public IEnumerable<ISeries> Series { get; set; }

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
                    Name = keyValuePair.Key,
                };

                series.Add(pieSeries);
            }

            nationalitiesPieChart.Series = series;
        }
    }
}
