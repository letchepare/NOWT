﻿using Microsoft.Toolkit.Mvvm.DependencyInjection;
using NOWT.ViewModels;
using System.Windows;

namespace NOWT;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<MainViewModel>();
        ((App)Application.Current).WindowPlace.Register(this);
    }
}
