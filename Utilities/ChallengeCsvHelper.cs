// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// Utilities/ChallengeCsvHelper.cs
using MemCard2025DesktopViewer.Utilities;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System;

public static class ChallengeCsvHelper
{
    public static (string cardName, string value) ParseChallengeCsvLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return (null, null);

        // Find the last comma that's outside of quotes
        bool insideQuotes = false;
        int lastCommaIndex = -1;

        for (int j = 0; j < line.Length; j++)
        {
            if (line[j] == '"')
                insideQuotes = !insideQuotes;
            else if (line[j] == ',' && !insideQuotes)
                lastCommaIndex = j;
        }

        if (lastCommaIndex == -1)
            return (null, null);

        var cardName = line.Substring(0, lastCommaIndex).Trim().Trim('"');
        var value = line.Substring(lastCommaIndex + 1).Trim();

        return (cardName, value);
    }

    public static string CreateChallengeCsvLine(string cardName, string value)
    {
        // Always quote the card name to handle special characters
        return $"\"{cardName}\",{value}";
    }
}
