// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// Views/ChallengeModeView.xaml.cs

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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MemCard2025DesktopViewer.Views
{
    /// <summary>
    /// Interaction logic for ChallengeModeView.xaml
    /// </summary>
    public partial class ChallengeModeView : UserControl
    {
        public ChallengeModeView()
        {
            InitializeComponent();

            // Set DataContext to ViewModel instance
            DataContext = new ChallengeModeViewModel();

            // Assign MediaElement to ViewModel property
            Loaded += ChallengeModeView_Loaded;


        }

        private void ChallengeModeView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChallengeModeViewModel viewModel)
            {
                if (viewModel.MediaPlayerElement == null) // Avoid reassigning on multiple loads
                {
                    viewModel.MediaPlayerElement = audioPlayer;
                    // MessageBox.Show("Loaded ---- viewModel.MediaPlayerElement = audioPlayer");
                }
            }
            else
            {
                // Optional: Log or handle unexpected DataContext
                MessageBox.Show("DataContext is not an instance of ChallengeModeViewModel.");
                // Console.WriteLine("DataContext is not an instance of DisplayModeViewModel.");
            }
        }

        private void AudioPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            // MessageBox.Show("Media opened successfully!", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AudioPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Media failed to load: {e.ErrorException.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
