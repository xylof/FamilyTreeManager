using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            var peopleAndFamilies = gedcomFileReader.ReadGedcomFile(@"C:\Users\Marek\Downloads\m0kd51_64943600221ywr561r4h45_A.ged");

            _people = peopleAndFamilies.people;
            _families = peopleAndFamilies.families;

            CalculateRelativesDensityFactorForAllPeople();
            CreateTable(_people);
        }

        private void CalculateRelativesDensityFactorForAllPeople()
        {
            foreach (Person person in _people)
                person.CalculateRelativesDensityFactor();
        }

        private void CreateTable(List<Person> people)
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

            foreach (Person person in people)
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

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)dataGridContextMenu.PlacementTarget;
            string personID = ((DataRowView)dataGrid.CurrentCell.Item).Row.ItemArray[0].ToString();
            Person person = _people.Find(per => per.ID == personID);

            PersonInfo personInfo = new PersonInfo(person);
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
    }
}
