// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// Views/Flex2ndDisplayWindow.xaml

using MemCard2025DesktopViewer.ViewModels;
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

namespace MemCard2025DesktopViewer.Views
{
    /// <summary>
    /// Interaction logic for Flex2ndDisplayWindow.xaml
    /// </summary>
    public partial class Flex2ndDisplayWindow : Window
    {
        public Flex2ndDisplayWindow(DisplayModeViewModel viewModel)
        {
            InitializeComponent();
            // Bind to the provided DisplayModeViewModel
            DataContext = viewModel;
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
