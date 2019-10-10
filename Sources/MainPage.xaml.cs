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
        /*
        private void GridContainer_SizeChanged(object _sender, SizeChangedEventArgs _e)
        {
            double gridAspect = m_Grid.RowDefinitions.Count / (double) m_Grid.ColumnDefinitions.Count;
            double containerAspect = m_GridContainer.Height / m_GridContainer.Width;
            if (gridAspect < containerAspect)
            {
                m_Grid.Height = m_GridContainer.Height;
                m_Grid.Width = m_Grid.Height / gridAspect;
            }
            else
            {
                m_Grid.Width = m_GridContainer.Width;
                m_Grid.Height = m_Grid.Width * gridAspect;
            }
        }*/
    }
}
