﻿namespace Countdown.ViewModels;

internal sealed class SettingsViewModel : PropertyChangedBase
{
    public SettingsViewModel()
    {
    }

    public ElementTheme SelectedTheme
    {
        get => Settings.Data.CurrentTheme;

        set
        {
            Settings.Data.CurrentTheme = value;
            RaisePropertyChanged();
        }
    }

    public bool IsLightTheme
    {
        get { return SelectedTheme == ElementTheme.Light; }
        set { if (value) SelectedTheme = ElementTheme.Light; }
    }

    public bool IsDarkTheme
    {
        get { return SelectedTheme == ElementTheme.Dark; }
        set { if (value) SelectedTheme = ElementTheme.Dark; }
    }

    public bool IsSystemTheme
    {
        get { return SelectedTheme == ElementTheme.Default; }
        set { if (value) SelectedTheme = ElementTheme.Default; }
    }
}
