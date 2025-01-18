// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// ViewModels/MainViewModel.cs

using MemCard2025DesktopViewer.Commands;
using MemCard2025DesktopViewer.Views;
using MemCard2025DesktopViewer.ViewModels;
using MemCard2025DesktopViewer.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Threading;
using System.Windows.Media;
using MemCard2025DesktopViewer.Utilities;

namespace MemCard2025DesktopViewer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        private readonly CardManager _cardManager;
        private DispatcherTimer _bgmTimer;
        private List<string> _bgmFiles;
        private int _currentBgmIndex;
        private MediaPlayer _mediaPlayer;
        private double _selectedBgmVolume;

        public MainViewModel()
        {
            _cardManager = new CardManager();
            _mediaPlayer = new MediaPlayer
            {
                Volume = 0.1 // Default volume
            };

            SelectedBgmVolume = 0.1; // Default to 10% volume

            // Load BGM List
            LoadBgmList();


            // Default to Home Mode
            CurrentView = new HomeView();
            SetDisplayModeCommand = new RelayCommand(SetDisplayMode);
            SetChallengeModeCommand = new RelayCommand(SetChallengeMode);
            ExitCommand = new RelayCommand(ExitApplication);
            ShuffleCardsCommand = new RelayCommand(ShuffleCards);
            LoadDefaultSequenceCommand = new RelayCommand(LoadDefaultSequence);

            // Initialize BGM Timer
            _bgmTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _bgmTimer.Tick += OnBgmTimerTick;
        }

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public bool IsBackgroundMusicEnabled
        {
            get => _bgmTimer.IsEnabled;
            set
            {
                if (value)
                {
                    StartBackgroundMusic();
                }
                else
                {
                    StopBackgroundMusic();
                }
                OnPropertyChanged();
            }
        }
        public double SelectedBgmVolume
        {
            get => _selectedBgmVolume;
            set
            {
                if (_selectedBgmVolume != value)
                {
                    _selectedBgmVolume = value;
                    _mediaPlayer.Volume = _selectedBgmVolume; // Update MediaPlayer volume
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SetDisplayModeCommand { get; }
        public ICommand SetChallengeModeCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShuffleCardsCommand { get; }
        public ICommand LoadDefaultSequenceCommand { get; }

        private void SetDisplayMode(object parameter = null)
        {
            CurrentView = new DisplayModeView();
        }

        private void SetChallengeMode(object parameter = null)
        {
            CurrentView = new ChallengeModeView();
        }

        private void ExitApplication(object parameter = null)
        {
            Application.Current.Shutdown();
        }

        private void ShuffleCards(object parameter = null)
        {
            _cardManager.ShuffleCards();
            // MessageBox.Show("Cards shuffled successfully!");
        }

        private void LoadDefaultSequence(object parameter = null)
        {
            _cardManager.LoadDefaultSequence();
            // MessageBox.Show("Default card sequence loaded!");
        }

        // BGM logic
        private void LoadBgmList()
        {
            _bgmFiles = new List<string>();

            // Read config file to get BGM list path
            string configPath = Constants.Paths.MEMCARD_CONFIG;
            if (!File.Exists(configPath))
            {
                return;
            }

            string bgmListPath = null;
            var configLines = File.ReadAllLines(configPath);
            foreach (var line in configLines)
            {
                if (line.StartsWith("BGM_list="))
                {
                    bgmListPath = line.Substring("BGM_list=".Length).Trim();
                    break;
                }
            }

            if (string.IsNullOrEmpty(bgmListPath))
            {
                return;
            }

            // Read BGM list file
            if (File.Exists(bgmListPath))
            {
                _bgmFiles.AddRange(File.ReadAllLines(bgmListPath));
            }
            _currentBgmIndex = 0;
        }

        private void StartBackgroundMusic()
        {
            if (_bgmFiles.Count == 0)
            {
                MessageBox.Show("No background music files found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PlayCurrentBgm();
            _bgmTimer.Start();
        }

        private void StopBackgroundMusic()
        {
            _bgmTimer.Stop();
            _mediaPlayer.Stop();
        }

        private void PlayCurrentBgm()
        {
            if (_currentBgmIndex >= _bgmFiles.Count)
            {
                _currentBgmIndex = 0;
            }

            string currentFile = _bgmFiles[_currentBgmIndex];

            if (File.Exists(currentFile))
            {
                _mediaPlayer.Open(new Uri(currentFile, UriKind.RelativeOrAbsolute));
                _mediaPlayer.Volume = SelectedBgmVolume; // Apply the current volume setting
                _mediaPlayer.Play();
            }
            else
            {
                MessageBox.Show($"File not found: {currentFile}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _currentBgmIndex++;
                PlayCurrentBgm();
            }
        }

        private void OnBgmTimerTick(object sender, EventArgs e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan &&
                _mediaPlayer.Position >= _mediaPlayer.NaturalDuration.TimeSpan)
            {
                _currentBgmIndex++;
                PlayCurrentBgm();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
