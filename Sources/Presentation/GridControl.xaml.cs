using Minesweeper.Logic;
using System;
using System.ComponentModel;
using System.Linq;
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
                    Playing = value != null;
                    IsEnabled = Playing;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsedFlags)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UncoveredCells)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Playing)));
                }
            }
        }

        public int? UsedFlags => Minefield != null ? m_usedFlags : (int?) null;
        public int? UncoveredCells => Minefield != null ? m_uncoveredCells : (int?) null;
        public int? CoveredCells => Minefield != null ? (Minefield.CellCount - UncoveredCells - UsedFlags) : null;
        public bool Playing { get; private set; }

        bool CellControl.IGrid.CanFlag => UsedFlags < Minefield.BombCount;

        UIElement CellControl.IGrid.FocusSearchRoot => m_grid;

        private void Uncover(CellControl _c)
        {
            if (_c.State != CellControl.EState.UNCOVERED)
            {
                if (_c.State == CellControl.EState.FLAGGED)
                {
                    _c.State = CellControl.EState.UNCOVERED;
                    FlagChanged(_c, false);
                }
                _c.State = CellControl.EState.UNCOVERED;
                Uncovered(_c);
            }
        }

        private void FlagChanged(CellControl _c, bool _flagged)
        {
            bool couldFlag = ((CellControl.IGrid) this).CanFlag;
            m_usedFlags += _flagged ? 1 : -1;
            if (couldFlag != ((CellControl.IGrid) this).CanFlag)
            {
                foreach (CellControl c in m_cells)
                {
                    c.Update();
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsedFlags)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));
            if (CoveredCells <= 0)
            {
                Stop();
            }
        }

        private void Uncovered(CellControl _c)
        {
            m_uncoveredCells++;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UncoveredCells)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CoveredCells)));

            foreach (CellControl nc in Minefield
                .Expand(_c.Index, _t => m_cells[_t.x, _t.y].State != CellControl.EState.UNCOVERED)
                .Select(_t => m_cells[_t.x, _t.y]))
            {
                Uncover(nc);
            }

            if (CoveredCells <= 0 || Minefield[_c.Index].IsBomb)
            {
                Stop();
            }

        }

        private void Populate()
        {
            m_grid.ColumnDefinitions.Clear();
            m_grid.RowDefinitions.Clear();
            m_grid.Children.Clear();
            if (Minefield != null)
            {
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
                        cell.OnFlagChanged += FlagChanged;
                        cell.OnUncovered += Uncovered;
                        cell.OnUncoverNeighbors += (_c) =>
                        {
                            foreach ((int nx, int ny) in Minefield.Neighborhood(_c.Index))
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
                        m_cells[x, y] = cell;
                        m_grid.Children.Add(cell);
                        Grid.SetColumn(cell, x);
                        Grid.SetRow(cell, y);
                    }
                }
            }
            else
            {
                m_cells = null;
            }
        }

        public void Stop()
        {
            if (Playing)
            {
                IsEnabled = false;
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

        Control CellControl.IGrid.GetFocusNeighbor((int x, int y) _index) =>
            Minefield.Neighborhood(_index)
            .Select(_i => m_cells[_i.x, _i.y])
            .FirstOrDefault(_c => _c.State != CellControl.EState.UNCOVERED);
    }
}
