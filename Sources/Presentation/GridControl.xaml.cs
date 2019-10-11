using Minesweeper.Logic;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Minesweeper.Presentation
{
    // Serialize minefield and cell state grid
    internal sealed partial class GridControl : UserControl, CellControl.IGrid, INotifyPropertyChanged
    {

        public GridControl() => InitializeComponent();

        private Minefield m_minefield;
        private int m_usedFlags;
        private int m_uncoveredCells;
        private CellControl[,] m_cells;

        public event PropertyChangedEventHandler PropertyChanged;

        public Minefield Minefield
        {
            get => m_minefield;
            set
            {
                if (m_minefield != value)
                {
                    m_minefield = value;
                    Populate();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Minefield)));
                    m_usedFlags = 0;
                    m_uncoveredCells = 0;
                    Playing = true;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsedFlags)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UncoveredCells)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playing)));
                }
            }
        }

        public int? UsedFlags => Minefield != null ? m_usedFlags : (int?) null;
        public int? UncoveredCells => Minefield != null ? m_uncoveredCells : (int?) null;
        public int? CoveredCells => Minefield != null ? (Minefield.CellCount - CoveredCells - UsedFlags) : null;
        public bool Playing { get; private set; }

        bool CellControl.IGrid.CanFlag => UsedFlags < Minefield.BombCount;

        private void Uncover(CellControl _c)
        {
            if (_c.State != CellControl.EState.UNCOVERED)
            {
                m_uncoveredCells++;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UncoveredCells)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));
                if (CoveredCells <= 0 || Minefield[_c.Index].IsBomb)
                {
                    Stop();
                }
            }
        }

        private void Populate()
        {
            m_grid.ColumnDefinitions.Clear();
            m_grid.RowDefinitions.Clear();
            m_grid.Children.Clear();
            m_cells = new CellControl[Minefield.Width, Minefield.Height];
            for (int x = 0; x < Minefield.Width; x++)
            {
                m_grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = GridLength.Auto
                });
            }
            for (int y = 0; y < Minefield.Height; y++)
            {
                m_grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = GridLength.Auto
                });
            }
            for (int x = 0; x < Minefield.Width; x++)
            {
                for (int y = 0; y < Minefield.Height; y++)
                {
                    CellControl cell = new CellControl(Minefield[x, y], (x, y), this);
                    cell.OnFlagChanged += (_c, _f) =>
                    {
                        m_usedFlags += _f ? 1 : -1;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsedFlags)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));
                        if (CoveredCells <= 0)
                        {
                            Stop();
                        }
                    };
                    cell.OnUncovered += Uncover;
                    cell.OnUncoverNeighbors += (_c) =>
                    {
                        foreach ((int nx, int ny) in Minefield.Neighborhood(_c.Index.x, _c.Index.y))
                        {
                            CellControl nc = m_cells[nx, ny];
                            if (nc.State == CellControl.EState.COVERED)
                            {
                                Uncover(m_cells[nx, ny]);
                            }
                        }
                        if (CoveredCells <= 0)
                        {
                            Stop();
                        }
                        _c.Update();
                    };
                    m_grid.Children.Add(cell);
                    Grid.SetColumn(cell, x);
                    Grid.SetRow(cell, y);
                }
            }
        }

        public void Stop()
        {
            if (Playing)
            {
                Playing = false;
                m_uncoveredCells = Minefield.CellCount;
                m_usedFlags = 0;
                foreach (CellControl c in m_cells)
                {
                    c.State = CellControl.EState.UNCOVERED;
                }
                foreach (CellControl c in m_cells)
                {
                    c.Update();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsedFlags)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UncoveredCells)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playing)));
            }
        }

        public void Restart()
        {
            if (Minefield == null)
            {
                throw new InvalidOperationException();
            }
            Minefield = new Minefield(Minefield.Width, Minefield.Height, Minefield.BombCount);
        }

        bool CellControl.IGrid.HasCoveredNeighbors((int, int) _index) => false;

        public bool TryLuck()
        {
            int r = new Random().Next(CoveredCells.Value);
            for (int o = 0; o < Minefield.CellCount; o++)
            {
                CellControl c = (CellControl) m_grid.Children[(r + o) % Minefield.CellCount];
                if (c.State == CellControl.EState.COVERED)
                {
                    Uncover(c);
                    if (m_minefield[c.Index].IsBomb)
                    {
                        return true;
                    }
                    break;
                }
            }
            return false;
        }
    }
}
