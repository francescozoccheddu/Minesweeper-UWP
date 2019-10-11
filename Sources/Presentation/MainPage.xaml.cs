using Minesweeper.Logic;
using Minesweeper.Presentation;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Minesweeper
{
    internal sealed partial class MainPage : Page
    {

        public MainPage() => InitializeComponent();

        private bool IsNotNull(object _obj) => _obj != null;

        private Visibility IsNotNullVis(object _obj) => BoolToVisibility(IsNotNull(_obj));

        private Visibility BoolToVisibility(bool _b) => _b ? Visibility.Visible : Visibility.Collapsed;

        private string GridSizeString(int _w, int _h) => $"{_w}x{_h}";

        private string DenominatorString(int _n) => $"/{_n}";

        private async void NewGameButton_Click(object _sender, RoutedEventArgs _e)
        {
            NewGameDialog dialog = new NewGameDialog();
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                m_grid.Minefield = new Minefield(dialog.WidthSetting, dialog.HeightSetting, dialog.BombsCountSetting);
                m_luckyIcon.Glyph = "l";
            }
        }

        private void ResetButton_Click(object _sender, RoutedEventArgs _e)
        {
            m_grid.Restart();
            m_luckyIcon.Glyph = "l";
        }

        private void LuckyButton_Click(object _sender, RoutedEventArgs _e)
        {
            if (m_grid.TryLuck())
            {
                m_luckyIcon.Glyph = "u";
            }
        }

    }
}
