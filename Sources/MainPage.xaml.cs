using Minesweeper.Logic;
using Minesweeper.Presentation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Minesweeper
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            InitializeGrid(new Minefield(16, 16, 10));
        }


        private void InitializeGrid(Minefield _minefield)
        {
            m_Grid.ColumnDefinitions.Clear();
            m_Grid.RowDefinitions.Clear();
            m_Grid.Children.Clear();
            GridLength star = new GridLength(1, GridUnitType.Star);
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
                    CellControl cell = new CellControl()
                    {
                        Data = _minefield[x, y],
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    m_Grid.Children.Add(cell);
                    Grid.SetColumn(cell, x);
                    Grid.SetRow(cell, y);
                }
            }
        }

    }
}
