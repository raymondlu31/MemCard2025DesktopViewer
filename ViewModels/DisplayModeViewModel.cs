// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// ViewModels/DisplayModeViewModel.cs

using System.IO;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MemCard2025DesktopViewer.Commands;
using MemCard2025DesktopViewer.Models;
using MemCard2025DesktopViewer.Services;
using System.Windows.Controls; // Required for MediaElement
using MemCard2025DesktopViewer.Utilities;
using MemCard2025DesktopViewer.Views;

namespace MemCard2025DesktopViewer.ViewModels
{
    public class DisplayModeViewModel : ViewModelBase
    {
        private string _currentImagePath;
        private string _currentAudioPath;
        private string _currentSubtitlePath;
        private string _currentSubtitleContent;
        private int _currentIndex;
        private bool _isInitialCardDisplayed;
        private bool _isAutoCycleEnabled;

        private MediaElement _mediaPlayerElement;

        private readonly CardManager _cardManager;

        private Timer _autoCycleTimer;
        private const int DefaultIntervalSeconds = 2; // Fallback if config value is missing

        private Flex2ndDisplayWindow _flex2ndDisplayWindow;

        public ObservableCollection<Card> CardSequence { get; set; }

        public string CurrentImagePath
        {
            get => _currentImagePath;
            set => SetProperty(ref _currentImagePath, value);
        }

        public string CurrentAudioPath
        {
            get => _currentAudioPath;
            set => SetProperty(ref _currentAudioPath, value);
        }

        public string CurrentSubtitlePath
        {
            get => _currentSubtitlePath;
            set => SetProperty(ref _currentSubtitlePath, value);
        }

        public string CurrentSubtitleContent
        {
            get => _currentSubtitleContent;
            set => SetProperty(ref _currentSubtitleContent, value);
        }

        public DisplayModeViewModel()
        {
            _cardManager = new CardManager();
            _isInitialCardDisplayed = true;


            // Comment out init card display

        // Initialize with the display init card
        var initCard = Card.DisplayModeInitCard();
        DisplayInitialCard(initCard);
        // DisplayModeInitDebug();



        // Load from runtime files
        _cardManager.UpdateCardList();

        CardSequence = new ObservableCollection<Card>(_cardManager.CurrentCardList);
        _currentIndex = -1;

        NextCommand = new RelayCommand(GoToNextCard, CanGoNext);
        PreviousCommand = new RelayCommand(GoToPreviousCard, CanGoPrevious);
        AutoCycleCommand = new RelayCommand(StartAutoCycle);
        Flex2ndDisplayCommand = new RelayCommand(OpenFlex2ndDisplayWindow, CanOpenFlex2ndDisplayWindow);
        ExitAutoCycleCommand = new RelayCommand(StopAutoCycle);
        GoHomeCommand = new RelayCommand(GoHome);
        // PlayAudioCommand = new RelayCommand<MediaElement>(PlayAudio);
        PlayAudioCommand = new RelayCommand(param => CardMediaHelper.PlayAudio((MediaElement)param, CurrentAudioPath));

        // UpdateCurrentCard();
        // DisplayModeInitDebug();
    }

    private void DisplayInitialCard(Card initCard)
    {
        var messageBuilder2 = new StringBuilder();
        // Update paths for the init card
        CurrentImagePath = _cardManager.GetCard_ImagePath(initCard);
        // messagebox to show the image path
        // MessageBox.Show(CurrentImagePath, "Image Path", MessageBoxButton.OK, MessageBoxImage.Information);
        CurrentAudioPath = _cardManager.GetCard_AudioPath(initCard);
        CurrentSubtitlePath = _cardManager.GetCard_SubtitlePath(initCard);

        // Use CardMediaHelper for display
        CurrentImagePath = CardMediaHelper.DisplayImage(CurrentImagePath);
        CurrentSubtitleContent = CardMediaHelper.DisplaySubtitle(CurrentSubtitlePath);

        // Play audio if media element is available
        if (MediaPlayerElement != null)
        {
            CardMediaHelper.PlayAudio(MediaPlayerElement, CurrentAudioPath);
        }
        messageBuilder2.AppendLine($"Card Unic Name: {initCard.Card_Unic_Name}");
    }

    /// <summary>
    /// Displays a pop-up window with the CardSequence and the first card's details.
    /// </summary>
    private void DisplayModeInitDebug()
    {
        if (CardSequence == null || !CardSequence.Any())
        {
            MessageBox.Show("CardSequence is empty. Please check your card data.",
                "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var firstCard = CardSequence.First();
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine("Card Sequence:");
        foreach (var card in CardSequence)
        {
            messageBuilder.AppendLine(card.Card_Unic_Name);
        }

        messageBuilder.AppendLine("\nFirst Card Details:");
        messageBuilder.AppendLine($"Card Unic Name: {firstCard.Card_Unic_Name}");
        messageBuilder.AppendLine($"Image Path: {firstCard.Image_Path}");
        messageBuilder.AppendLine($"Audio Path: {firstCard.Audio_Path}");

        MessageBox.Show(messageBuilder.ToString(),
            "Display Mode Initialization", MessageBoxButton.OK, MessageBoxImage.Information);
    }





    private void UpdateCurrentCard()
    {
        if (_currentIndex >= 0 && _currentIndex < CardSequence.Count)
        {
            _isInitialCardDisplayed = false;
            var currentCard = CardSequence[_currentIndex];

            // Update image, audio, and subtitle paths using CardManager
            CurrentImagePath = _cardManager.GetCard_ImagePath(currentCard);
            CurrentAudioPath = _cardManager.GetCard_AudioPath(currentCard);
            CurrentSubtitlePath = _cardManager.GetCard_SubtitlePath(currentCard);

            // Use CardMediaHelper for image and subtitle display
            CurrentImagePath = CardMediaHelper.DisplayImage(CurrentImagePath);
            CurrentSubtitleContent = CardMediaHelper.DisplaySubtitle(CurrentSubtitlePath);

            // Automatically play audio after next card
            if (MediaPlayerElement != null)
            {
                CardMediaHelper.PlayAudio(MediaPlayerElement, CurrentAudioPath);
            }

        }
    }

    public MediaElement MediaPlayerElement
    {
        get => _mediaPlayerElement;
        set => SetProperty(ref _mediaPlayerElement, value);
    }


    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand AutoCycleCommand { get; }
    public ICommand Flex2ndDisplayCommand { get; }
    public ICommand ExitAutoCycleCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand PlayAudioCommand { get; }


    private void GoToNextCard(object parameter)
    {
        if (_isInitialCardDisplayed)
        {
            _currentIndex++;
            UpdateCurrentCard();
        }
        else if (_currentIndex < CardSequence.Count - 1)
        {
            _currentIndex++;
            UpdateCurrentCard();
        }
        else if (_isAutoCycleEnabled) // Reset to first card if auto-cycling
        {
            _currentIndex = 0;
            UpdateCurrentCard();
        }
    }

    private void GoToPreviousCard(object parameter)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateCurrentCard();
        }
    }

    private bool CanGoNext(object parameter) =>
    _isInitialCardDisplayed || _currentIndex < CardSequence.Count - 1 || _isAutoCycleEnabled;

    private bool CanGoPrevious(object parameter) => _currentIndex > 0;

    private void OpenFlex2ndDisplayWindow(object parameter)
    {
        if (_flex2ndDisplayWindow == null || !_flex2ndDisplayWindow.IsVisible)
        {
            if (string.IsNullOrEmpty(CurrentImagePath) || string.IsNullOrEmpty(CurrentSubtitleContent))
            {
                MessageBox.Show("No card is currently displayed. Please start Auto Cycle mode or select a card.",
                    "Flex 2nd Display Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _flex2ndDisplayWindow = new Flex2ndDisplayWindow(this);
            _flex2ndDisplayWindow.Show();
        }
        else
        {
            _flex2ndDisplayWindow.Focus();
        }
    }

    // when _isAutoCycleEnabled = true, can open the 2nd display window
    private bool CanOpenFlex2ndDisplayWindow(object parameter) => _isAutoCycleEnabled;


    private void StartAutoCycle(object parameter = null)
    {
        if (_autoCycleTimer == null)
        {
            _isAutoCycleEnabled = true;
            // Read the config file
            var configPath = Constants.Paths.MEMCARD_CONFIG;
            var configData = ConfigHelper.ReadConfigFile(configPath);

            // Get the interval from the config or use default
            int intervalSeconds = DefaultIntervalSeconds;
            if (configData.TryGetValue("DisplayMode_AutoCycle_interval", out var intervalValue) &&
                int.TryParse(intervalValue, out var parsedInterval) &&
                parsedInterval > 0)
            {
                intervalSeconds = parsedInterval;
            }

            // Initialize and start the timer
            _autoCycleTimer = new Timer(intervalSeconds * 1000); // Convert to milliseconds

            // _autoCycleTimer = new Timer(2000); // 2 seconds
            _autoCycleTimer.Elapsed += AutoCycleStep;
            _autoCycleTimer.Start();
            // MessageBox.Show($"Auto cycle interval: {intervalSeconds} seconds", "DisplayMode_AutoCycle_interval");

            // MessageBox.Show("Auto cycle started. Cards will cycle continuously.");
        }
    }

    private void AutoCycleStep(object sender, ElapsedEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            GoToNextCard(null);
        });
    }

    private void StopAutoCycle(object parameter = null)
    {
        if (_autoCycleTimer != null)
        {
            _isAutoCycleEnabled = false;
            _autoCycleTimer.Stop();
            _autoCycleTimer.Dispose();
            _autoCycleTimer = null;
            // MessageBox.Show("Auto cycle stopped.");
        }
    }

    private void GoHome(object parameter = null)
    {
        // Logic to navigate back to the home view
        Application.Current.MainWindow.DataContext = new MainViewModel();
    }
}
}
