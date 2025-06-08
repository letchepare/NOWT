﻿using FontAwesome6.Fonts;
using NOWT.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NOWT.Views;

public partial class Home : UserControl
{
    public static ImageAwesome ValorantStatus;
    public static ImageAwesome AccountStatus;
    public static ImageAwesome MatchStatus;

    public Home()
    {
        InitializeComponent();
        DataContextChanged += DataContextChangedHandler;

        ValorantStatus = ValorantStatusView;
        AccountStatus = AccountStatusView;
        MatchStatus = MatchStatusView;
    }

    private void DataContextChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not HomeViewModel viewModel)
            return;
        viewModel.GoMatchEvent += () =>
        {
            Dispatcher.Invoke(() =>
            {
                if (GoMatch.Command.CanExecute(null))
                    GoMatch.Command.Execute(null);
            });
        };
    }
}
