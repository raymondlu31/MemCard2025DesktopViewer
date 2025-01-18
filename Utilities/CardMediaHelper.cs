// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.


// Utilities/CardMediaHelper.cs

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace MemCard2025DesktopViewer.Utilities
{
    public static class CardMediaHelper
    {
        /// <summary>
        /// Plays the audio file associated with the current card.
        /// </summary>
        /// <param name="mediaElement">The MediaElement control to play the audio.</param>
        /// <param name="audioPath">The path to the audio file.</param>
        public static void PlayAudio(MediaElement mediaElement, string audioPath)
        {
            if (mediaElement == null)
            {
                MessageBox.Show("Media player not initialized.", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(audioPath))
            {
                MessageBox.Show("No audio file available for the current card.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(audioPath))
            {
                MessageBox.Show("Audio file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                mediaElement.Source = new Uri(audioPath, UriKind.Absolute);
                mediaElement.Volume = 1.0; // Ensure volume is up
                mediaElement.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing audio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Displays the corrected image path, adjusted for orientation.
        /// </summary>
        public static string DisplayImage(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return "Image file not found.";
            }

            try
            {
                string adjustedImagePath = AdjustImageOrientation(imagePath);
                return adjustedImagePath;
            }
            catch (Exception ex)
            {
                return $"Error adjusting image orientation: {ex.Message}";
            }
        }

        /// <summary>
        /// Adjusts the orientation of an image based on its EXIF metadata and saves the corrected image.
        /// </summary>
        /// <param name="imagePath">The path to the original image file.</param>
        /// <returns>The path to the orientation-corrected image.</returns>
        public static string AdjustImageOrientation(string imagePath)
        {
            if (!File.Exists(imagePath))
                return imagePath;

            ushort? orientation = null;

            try
            {
                // Create a BitmapDecoder to read the image file
                BitmapDecoder decoder;
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                }

                // Get the first frame of the image
                var frame = decoder.Frames[0];


                // Check for EXIF orientation metadata
                string orientationValue = "1";
                if (frame.Metadata is BitmapMetadata metadata && metadata.ContainsQuery("/app1/ifd/{ushort=274}"))
                {
                    orientation = metadata.GetQuery("/app1/ifd/{ushort=274}") as ushort?;
                    if (orientation.HasValue)
                    {
                        // MessageBox.Show($"orientation.Value: {orientation.Value}", "Debug Info");
                        orientationValue = orientation.Value.ToString();

                    }
                }

                // If no rotation needed, return original path
                if (orientationValue == "1" || orientationValue == "")
                    return imagePath;

                // Create a transformed bitmap
                TransformedBitmap transformedBitmap = null;

                // Apply the appropriate rotation transform
                switch (orientationValue)
                {
                    case "2": // Flip horizontal
                        transformedBitmap = new TransformedBitmap(frame, new ScaleTransform(-1, 1));
                        break;
                    case "3": // 180° rotate
                        transformedBitmap = new TransformedBitmap(frame, new RotateTransform(180));
                        break;
                    case "4": // Flip vertical
                        transformedBitmap = new TransformedBitmap(frame, new ScaleTransform(1, -1));
                        break;
                    case "5": // Flip horizontal and rotate 270°
                        transformedBitmap = new TransformedBitmap(
                            new TransformedBitmap(frame, new ScaleTransform(-1, 1)),
                            new RotateTransform(270));
                        break;
                    case "6": // 90° rotate
                        transformedBitmap = new TransformedBitmap(frame, new RotateTransform(90));
                        break;
                    case "7": // Flip horizontal and rotate 90°
                        transformedBitmap = new TransformedBitmap(
                            new TransformedBitmap(frame, new ScaleTransform(-1, 1)),
                            new RotateTransform(90));
                        break;
                    case "8": // 270° rotate
                        transformedBitmap = new TransformedBitmap(frame, new RotateTransform(270));
                        break;
                }

                if (transformedBitmap != null)
                {
                    // Create path for corrected image
                    string runtime_directory = Constants.Paths.RUNTIME_FOLDER;
                    string filename = Path.GetFileNameWithoutExtension(imagePath);
                    string extension = Path.GetExtension(imagePath);
                    string correctedPath = Path.Combine(runtime_directory, $"{filename}_CardImageCorrected{extension}");

                    // check if the corrected image already exists, if exists, use the existing corrected image, do not create a new one
                    if (File.Exists(correctedPath))
                    {
                        return correctedPath;
                    }
                    else
                    {
                        // Create an encoder for the output file format
                        BitmapEncoder encoder;
                        switch (extension.ToLower())
                        {
                            case ".png":
                                encoder = new PngBitmapEncoder();
                                break;
                            case ".JPG":
                            case ".jpeg":
                                encoder = new JpegBitmapEncoder();
                                break;
                            default:
                                encoder = new BmpBitmapEncoder();
                                break;
                        }

                        // Add the transformed frame and save
                        encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
                        using (var stream = File.Create(correctedPath))
                        {
                            encoder.Save(stream);
                        }
                        return correctedPath;
                    }
 
                }

                return imagePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image orientation: {ex.Message}", "Image Processing Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return imagePath; // Return original path if processing fails
            }
        }


        /// <summary>
        /// Reads and returns the content of the subtitle file for the current card.
        /// </summary>
        /// <param name="subtitlePath">The path to the subtitle file.</param>
        /// <returns>The content of the subtitle file or an error message if the file is not found or cannot be read.</returns>
        public static string DisplaySubtitle(string subtitlePath)
        {
            try
            {
                if (!File.Exists(subtitlePath))
                {
                    return "Subtitle file not found.";
                }

                return File.ReadAllText(subtitlePath);
            }
            catch (Exception ex)
            {
                return $"Error loading subtitle: {ex.Message}";
            }
        }
    }
}
