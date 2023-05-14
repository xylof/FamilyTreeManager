using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FamilyTreeManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Person> _people;
        private List<Family> _families;

        public MainWindow()
        {
            InitializeComponent();

            GedcomFileReader gedcomFileReader = new GedcomFileReader();
            var peopleAndFamilies = gedcomFileReader.ReadGedcomFile(@"C:\Users\Marek\Downloads\453e8b_379006ebks58y631435935_A.ged");

            _people = peopleAndFamilies.people;
            _families = peopleAndFamilies.families;

            CalculateRelativesDensityFactorForAllPeople();
            CalculateNationalitiesForAllPeople();
            SetMostDistantAncestorsInMaleAndFemaleLinesForAllPeople();

            CreateTable();
        }

        private void CalculateRelativesDensityFactorForAllPeople()
        {
            foreach (Person person in _people)
                person.CalculateRelativesDensityFactor();
        }

        private void CreateTable()
        {
            DataTable table = new DataTable("Wszystkie osoby");
            DataColumn column;
            DataRow row;

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "ID";
            column.ReadOnly = true;
            column.Unique = true;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Osoba";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Płeć";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "ID ojca";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "ID matki";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Data urodzenia";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Data śmierci";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Osiągnięty wiek";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.Int32");
            column.ColumnName = "Współczynnik zagęszczenia krewnych";
            column.ReadOnly = true;
            column.Unique = false;
            table.Columns.Add(column);

            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(table);

            foreach (Person person in _people)
            {
                row = table.NewRow();
                row["ID"] = person.ID;
                row["Osoba"] = person.ToString();
                row["Płeć"] = person.Sex.ToString();
                row["ID ojca"] = person.HasParents ? person.Father.ID.ToString() : "--";
                row["ID matki"] = person.HasParents ? person.Mother.ID.ToString() : "--";
                row["Data urodzenia"] = person.GetBirthDate != "" ? person.GetBirthDate : "--";
                row["Data śmierci"] = person.GetDeathDate != "" ? person.GetDeathDate : "--";
                row["Osiągnięty wiek"] = person.Age != -1 ? person.Age.ToString() : "--";
                row["Współczynnik zagęszczenia krewnych"] = person.RelativesDensityFactor;
                table.Rows.Add(row);
            }

            dataGrid.ItemsSource = dataSet.Tables["Wszystkie osoby"].DefaultView;
        }

        private void CalculateNationalitiesForAllPeople()
        {
            foreach (Person person in _people)
                CalculateNationalities(person);
        }

        private void SetMostDistantAncestorsInMaleAndFemaleLinesForAllPeople()
        {
            foreach (Person person in _people)
                person.SetMostDistantAncestorsInMaleAndFemaleLines();
        }

        private Dictionary<string, double> CalculateNationalities(Person person)
        {
            if (person == null)
                return new Dictionary<string, double>(); // ewentulanie return null

            if (person.Nationalities.Count == 0)
            {
                Dictionary<string, double> fatherNationalities = CalculateNationalities(person.Father);
                Dictionary<string, double> motherNationalities = CalculateNationalities(person.Mother);

                Dictionary<string, double> dictionaryWithAveragedValues = GetDictionaryWithAveragedValues(fatherNationalities, motherNationalities);
                person.Nationalities = dictionaryWithAveragedValues;

                return dictionaryWithAveragedValues;
            }
            else
                return person.Nationalities;
        }

        private Dictionary<string, double> GetDictionaryWithAveragedValues(Dictionary<string, double> dict1, Dictionary<string, double> dict2)
        {
            Dictionary<string, double> averagedNationalities = dict1.ToDictionary(pair => pair.Key, pair => Math.Round(pair.Value / 2, 2));

            foreach (KeyValuePair<string, double> keyValuePair in dict2)
            {
                if (!averagedNationalities.ContainsKey(keyValuePair.Key))
                    averagedNationalities[keyValuePair.Key] = Math.Round(keyValuePair.Value / 2, 2);
                else
                    averagedNationalities[keyValuePair.Key] += Math.Round(keyValuePair.Value / 2, 2);
            }

            return averagedNationalities.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)dataGridContextMenu.PlacementTarget;
            string personID = ((DataRowView)dataGrid.CurrentCell.Item).Row.ItemArray[0].ToString();
            Person person = _people.Find(per => per.ID == personID);
            List<Person> displayedFamilyNodes = new List<Person> { person };

            PersonInfo personInfo = new PersonInfo(person, displayedFamilyNodes);
            personInfo.Owner = this;
            personInfo.Show();
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)dataGridContextMenu.PlacementTarget;
            string personID = ((DataRowView)dataGrid.CurrentCell.Item).Row.ItemArray[0].ToString();
            Person person = _people.Find(per => per.ID == personID);

            SosaAncestorsList sosaAncestorsList = new SosaAncestorsList(person);
            sosaAncestorsList.Owner = this;
            sosaAncestorsList.Show();
        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)dataGridContextMenu.PlacementTarget;
            string personID = ((DataRowView)dataGrid.CurrentCell.Item).Row.ItemArray[0].ToString();
            Person person = _people.Find(per => per.ID == personID);

            NamesAndSurnames namesAndSurnames = new NamesAndSurnames(person);
            namesAndSurnames.Owner = this;
            namesAndSurnames.Show();
        }

        private void MaleAndFemaleLinesOption_Click(object sender, RoutedEventArgs e)
        {
            MaleAndFemaleLines maleAndFemaleLines = new MaleAndFemaleLines(_people);
            maleAndFemaleLines.Owner = this;
            maleAndFemaleLines.Show();
        }
    }
}
