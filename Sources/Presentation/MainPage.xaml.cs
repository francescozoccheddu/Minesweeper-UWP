using Minesweeper.Logic;
using Minesweeper.Presentation;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Minesweeper
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        private struct Cell : Minefield.ICell
        {
            public bool IsBomb => false;

            public int Neighbors => 6;
        }

        private struct CGrid : CellControl2.IGrid
        {
            public bool CanFlag => true;

            public bool HasCoveredNeighbors((int, int) _index) => false;
        }

        public MainPage()
        {
            InitializeComponent();
            Diocane.Children.Add(new CellControl2(new Cell(), (0, 0), new CGrid()));
        }

        private Minefield m_minefield = null;

        private int m_usedFlags = 0;

        private bool m_playing = false;

        private int m_uncovered = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private Minefield m_Minefield
        {
            get => m_minefield;
            set
            {
                if (m_minefield != value)
                {
                    m_minefield = value;
                    InitializeGrid(value);
                    m_UsedFlags = 0;
                    m_Uncovered = 0;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_Minefield)));
                    m_Playing = true;
                    m_luckyIcon.Glyph = "l";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_Covered)));
                }
            }
        }

        private int m_UsedFlags
        {
            get => m_usedFlags;
            set
            {
                if (m_usedFlags != value)
                {
                    m_usedFlags = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_UsedFlags)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_Covered)));
                }
            }
        }

        private int m_Uncovered
        {
            get => m_uncovered;
            set
            {
                if (m_uncovered != value)
                {
                    m_uncovered = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_Uncovered)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_Covered)));
                }
            }
        }

        private bool m_Playing
        {
            get => m_playing;
            set
            {
                if (m_playing != value)
                {
                    m_playing = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(m_Playing)));
                }
            }
        }

        private int m_Covered => (m_Minefield?.CellCount - m_Uncovered - m_UsedFlags) ?? 0;

        private void Lose()
        {
            m_Playing = false;
            foreach (CellControl c in m_Grid.Children)
            {
                c.State = CellControl.EState.UNCOVERED;
            }
        }

        private bool IsNotNull(object _obj) => _obj != null;

        private Visibility IsNotNullVis(object _obj) => BoolToVisibility(IsNotNull(_obj));

        private Visibility BoolToVisibility(bool _b) => _b ? Visibility.Visible : Visibility.Collapsed;

        private bool IsPositive(int _n) => _n > 0;

        private string GridSizeString(int _w, int _h) => $"{_w}x{_h}";

        private string DenominatorString(int _n) => $"/{_n}";

        private bool IsFlaggable() => m_UsedFlags < m_Minefield?.BombCount;

        private void InitializeGrid(Minefield _minefield)
        {
            m_Grid.ColumnDefinitions.Clear();
            m_Grid.RowDefinitions.Clear();
            m_Grid.Children.Clear();
            GridLength star = new GridLength(40, GridUnitType.Pixel);
            for (int x = 0; x < _minefield.Width; x++)
            {
                m_Grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = star
                });
            }
            for (int y = 0; y < _minefield.Height; y++)
            {
                m_Grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = star
                });
            }
            for (int x = 0; x < _minefield.Width; x++)
            {
                for (int y = 0; y < _minefield.Height; y++)
                {
                    CellControl cell = new CellControl(x, y)
                    {
                        Data = _minefield[x, y],
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Flaggable = IsFlaggable
                    };
                    cell.OnStateChanged += (_c, _ps) =>
                    {
                        switch (_c.State)
                        {
                            case CellControl.EState.COVERED when _ps == CellControl.EState.FLAGGED:
                            case CellControl.EState.UNCOVERED when _ps == CellControl.EState.FLAGGED:
                            m_UsedFlags--;
                            break;
                            case CellControl.EState.FLAGGED:
                            m_UsedFlags++;
                            break;
                            case CellControl.EState.UNCOVERED:
                            if (_c.Data.IsBomb)
                            {
                                Lose();
                            }
                            else
                            {
                                m_Uncovered++;
                            }
                            break;
                        }
                    };
                    m_Grid.Children.Add(cell);
                    Grid.SetColumn(cell, x);
                    Grid.SetRow(cell, y);
                }
            }
        }

        private async void NewGameButton_Click(object _sender, RoutedEventArgs _e)
        {
            NewGameDialog dialog = new NewGameDialog();
            ContentDialogResult result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                m_Minefield = new Minefield(dialog.WidthSetting, dialog.HeightSetting, dialog.BombsCountSetting);
            }
        }

        private void ResetButton_Click(object _sender, RoutedEventArgs _e) => m_Minefield = new Minefield(m_Minefield.Width, m_Minefield.Height, m_Minefield.BombCount);

        private void LuckyButton_Click(object _sender, RoutedEventArgs _e)
        {
            int r = new Random().Next(m_Covered);
            {
                for (int o = 0; o < m_Minefield.CellCount; o++)
                {
                    CellControl cell = (CellControl) m_Grid.Children[(r + o) % m_Minefield.CellCount];
                    if (cell.State == CellControl.EState.COVERED)
                    {
                        cell.State = CellControl.EState.UNCOVERED;
                        if (cell.Data.IsBomb)
                        {
                            m_luckyIcon.Glyph = "u";
                        }
                        break;
                    }
                }
            }
        }
    }
}
