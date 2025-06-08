﻿using AutoUpdaterDotNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using NOWT.Helpers;
using NOWT.Properties;
using NOWT.ViewModels;
using RestoreWindowPlace;
using Serilog;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using static NOWT.Helpers.ValApi;

namespace NOWT;

public partial class App : Application
{
    public App()
    {
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;

        WindowPlace = new WindowPlace(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + "\\NOWT\\placement.config"
        );

        if (string.IsNullOrEmpty(Settings.Default.Language))
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InstalledUICulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InstalledUICulture;
            Settings.Default.Language = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
        }
        else
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Language);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
        }

        Settings.Default.Save();
    }

    public WindowPlace WindowPlace { get; }

    private static void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e
    )
    {
        Constants.Log.Error(
            "Unhandled Exception: {Message}, {Stacktrace}",
            e.Exception.Message,
            e.Exception.StackTrace
        );
        e.Handled = true;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        var update_url =
            "https://raw.githubusercontent.com/pwall2222/NOWT/main/NOWT/VersionInfo.xml";

        Constants.LocalAppDataPath =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\NOWT";
        Constants.Log = new LoggerConfiguration().MinimumLevel
            .Debug()
            .WriteTo.Async(
                a =>
                    a.File(
                        Constants.LocalAppDataPath + "\\logs\\log.txt",
                        shared: true,
                        rollingInterval: RollingInterval.Day
                    )
            )
            .CreateLogger();
        Constants.Log.Information("Application Start. Version: {Version}", version);

        CheckAndUpdateJsonAsync().ConfigureAwait(false);

        var conventionViewFactory = new NamingConventionViewFactory();

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddTransient<HomeViewModel>()
                .AddTransient<InfoViewModel>()
                .AddTransient<MatchViewModel>()
                .AddTransient<SettingsViewModel>()
                .AddSingleton<MainViewModel>()
                .AddSingleton<IViewFactory>(conventionViewFactory)
                .BuildServiceProvider()
        );

        AutoUpdater.ShowSkipButton = false;
        AutoUpdater.InstalledVersion = new Version(System.Windows.Forms.Application.ProductVersion);
        AutoUpdater.Start(update_url);

        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        Constants.Log.Information("Application Stop");
        Settings.Default.Save();
        WindowPlace.Save();
    }
}
