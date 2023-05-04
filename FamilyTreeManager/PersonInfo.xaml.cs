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
    /// Interaction logic for PersonInfo.xaml
    /// </summary>
    public partial class PersonInfo : Window
    {
        internal PersonInfo(Person person)
        {
            InitializeComponent();

            personLabel.Content = person.ShowLongerPersonDescription();
        }
    }
}
