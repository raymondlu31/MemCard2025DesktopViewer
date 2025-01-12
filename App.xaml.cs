﻿// MemCard2025
// MIT License
// Copyright (c) 2025 Raymond Lou Independent Developer
// See LICENSE file for full license information.

// App.xaml.cs

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MemCard2025DesktopViewer.Services;

namespace MemCard2025DesktopViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Instantiate the AppInitializer
            var appInitializer = new AppInitializer();

            // Call InitialLoad to perform startup tasks
            appInitializer.InitialLoad();
        }
    }
}
