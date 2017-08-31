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

namespace HDTools2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		public Page Source
		{
			set
			{
				Dispatcher.Invoke(() => src.Navigate(value));
			}
		}
		//public Frame src;
        public MainWindow()
        {
            InitializeComponent();
			Source = new UserInputPage(this);
        }
    }
}
