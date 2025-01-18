// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// ViewModels/ChallengeModeViewModel.cs

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
using System.Collections.Generic;

namespace MemCard2025DesktopViewer.ViewModels
{
    public class ChallengeModeViewModel : ViewModelBase
    {
        private string _currentImagePath;
        private string _currentAudioPath;
        private string _currentSubtitlePath;
        private string _currentSubtitleContent;
        private int _currentIndex;
        private bool _isInitialCardDisplayed;

        private readonly string _challengeFilePath;


        private MediaElement _mediaPlayerElement;

        private readonly CardManager _cardManager;

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

        public ChallengeModeViewModel()
        {
            _cardManager = new CardManager();
            _isInitialCardDisplayed = true;

            _challengeFilePath = Path.Combine(Constants.Paths.RUNTIME_FOLDER, "CurrentChallenge.tmp");

            // comment out init card display
            
            // Initialize with the display init card
            var initCard = Card.ChallengeModeInitCard();
            ChallengeInitialCard(initCard);
            // ChallengeModeInitDebug();
            


            // Load from runtime files
            _cardManager.UpdateCardList();

            CardSequence = new ObservableCollection<Card>(_cardManager.CurrentCardList);
            _currentIndex = -1;

            // Initialize the challenge progress file
            InitializeChallengeRuntimeFile();

            NextCommand = new RelayCommand(GoToNextCard, CanGoNext);
            PreviousCommand = new RelayCommand(GoToPreviousCard, CanGoPrevious);
            TrueAnswerCommand = new RelayCommand(HandleTrueAnswer, CanAnswer);
            FalseAnswerCommand = new RelayCommand(HandleFalseAnswer, CanAnswer);
            GoHomeCommand = new RelayCommand(GoHome);
            // PlayAudioCommand = new RelayCommand<MediaElement>(PlayAudio);
            PlayAudioCommand = new RelayCommand(param => CardMediaHelper.PlayAudio((MediaElement)param, CurrentAudioPath));

            // UpdateCurrentCard();
            // ChallengeModeInitDebug();
        }

        private void InitializeChallengeRuntimeFile()
        {
            try
            {
                var lines = new List<string>
        {
            "Card_Unic_Name,answer" // Header
        };

                // Add an entry for each card with default answer = true
                foreach (var card in CardSequence)
                {
                    lines.Add(ChallengeCsvHelper.CreateChallengeCsvLine(card.Card_Unic_Name, "true"));
                }

                File.WriteAllLines(_challengeFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing challenge file: {ex.Message}",
                    "File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateChallengeRuntimeFile(string cardUnicName, bool answer)
        {
            try
            {
                var lines = File.ReadAllLines(_challengeFilePath).ToList();

                // Find and update the specific card's answer
                for (int i = 1; i < lines.Count; i++) // Start from 1 to skip header
                {
                    var (currentCardName, _) = ChallengeCsvHelper.ParseChallengeCsvLine(lines[i]);
                    if (currentCardName == cardUnicName)
                    {
                        lines[i] = ChallengeCsvHelper.CreateChallengeCsvLine(cardUnicName, answer.ToString().ToLower());
                        break;
                    }
                }

                File.WriteAllLines(_challengeFilePath, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating challenge file: {ex.Message}",
                    "File Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ChallengeInitialCard(Card initCard)
        {
            var messageBuilder2 = new StringBuilder();
            // Update paths for the init card
            CurrentImagePath = _cardManager.GetCard_ImagePath(initCard);
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
        private void ChallengeModeInitDebug()
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
                "Challenge Mode Initialization", MessageBoxButton.OK, MessageBoxImage.Information);
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

            // Notify commands to recheck CanExecute
            ((RelayCommand)TrueAnswerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)FalseAnswerCommand).RaiseCanExecuteChanged();
        }

        private void ShowChallengeResults()
        {
            try
            {
                var lines = File.ReadAllLines(_challengeFilePath).ToList();
                var totalCards = 0;
                var trueAnswers = 0;

                // Process each data row (skip header)
                for (int i = 1; i < lines.Count; i++)
                {
                    var (cardName, answerStr) = ChallengeCsvHelper.ParseChallengeCsvLine(lines[i]);
                    if (cardName != null && bool.TryParse(answerStr, out var answer))
                    {
                        totalCards++;
                        if (answer)
                            trueAnswers++;
                    }
                }

                var percentage = totalCards > 0 ? (trueAnswers * 100.0 / totalCards) : 0;

                // Build the result message
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("Challenge Results: Good Job!");
                messageBuilder.AppendLine($"You got {trueAnswers} Correct answers out of {totalCards} cards!");
                messageBuilder.AppendLine($"Your Accuracy is {percentage:F2}%.");
                messageBuilder.AppendLine("Good Improvement!");

                // Append detailed results
                messageBuilder.AppendLine("\nDetailed Results:");
                for (int i = 1; i < lines.Count; i++)
                {
                    var (cardName, answer) = ChallengeCsvHelper.ParseChallengeCsvLine(lines[i]);
                    if (cardName != null)
                    {
                        messageBuilder.AppendLine($"{cardName}: {answer}");
                    }
                }

                var result = MessageHelper.ChallengeResultWindow(messageBuilder.ToString(), 18);

                if (result == MessageBoxResult.OK)
                {
                    GoHome();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying challenge results: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public MediaElement MediaPlayerElement
        {
            get => _mediaPlayerElement;
            set => SetProperty(ref _mediaPlayerElement, value);
        }


        public ICommand PreviousCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand TrueAnswerCommand { get; }
        public ICommand FalseAnswerCommand { get; }
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

            // Notify commands to recheck CanExecute
            ((RelayCommand)TrueAnswerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)FalseAnswerCommand).RaiseCanExecuteChanged();

        }

        private void GoToPreviousCard(object parameter)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                UpdateCurrentCard();
            }

            // Notify commands to recheck CanExecute
            ((RelayCommand)TrueAnswerCommand).RaiseCanExecuteChanged();
            ((RelayCommand)FalseAnswerCommand).RaiseCanExecuteChanged();
        }

        private bool CanGoNext(object parameter) =>
        _isInitialCardDisplayed || _currentIndex < CardSequence.Count - 1;

        private bool CanGoPrevious(object parameter) => _currentIndex > 0;

        // CanExecute logic for TrueAnswerCommand and FalseAnswerCommand
        private bool CanAnswer(object parameter) => _currentIndex >= 0;

        private void HandleTrueAnswer(object parameter)
        {
            // Move to next card if available
            if (_currentIndex < CardSequence.Count)
            {
                var currentCard = CardSequence[_currentIndex];
                UpdateChallengeRuntimeFile(currentCard.Card_Unic_Name, true);

                // Check if this is the last card
                if (_currentIndex == CardSequence.Count - 1)
                {
                    ShowChallengeResults();
                }
                else
                {
                    GoToNextCard(null);
                }
            }
        }

        private void HandleFalseAnswer(object parameter)
        {
            // Get current card and update its answer in the file
            if (_currentIndex >= 0 && _currentIndex < CardSequence.Count)
            {
                var currentCard = CardSequence[_currentIndex];
                UpdateChallengeRuntimeFile(currentCard.Card_Unic_Name, false);

                // Move to next card if available
                if (_currentIndex < CardSequence.Count)
                {
                    // Check if this is the last card
                    if (_currentIndex == CardSequence.Count - 1)
                    {
                        ShowChallengeResults();
                    }
                    else
                    {
                        GoToNextCard(null);
                    }
                }
            }
        }

        private void GoHome(object parameter = null)
        {
            // Logic to navigate back to the home view
            Application.Current.MainWindow.DataContext = new MainViewModel();
        }
    }
}
