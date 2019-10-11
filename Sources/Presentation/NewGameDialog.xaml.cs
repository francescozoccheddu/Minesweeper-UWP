using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Minesweeper.Presentation
{
    public sealed partial class NewGameDialog : ContentDialog
    {
        public NewGameDialog()
        {
            InitializeComponent();
        }

        private double MaxBombs(double _w, double _h) => ((int) _w) * ((int) _h) - 1;

        public int WidthSetting
        {
            get => (int) m_widthSlider.Value;
            set => m_widthSlider.Value = value;
        }
        public int HeightSetting
        {
            get => (int) m_heightSlider.Value;
            set => m_heightSlider.Value = value;
        }
        public int BombsCountSetting
        {
            get => (int) m_bombsSlider.Value;
            set => m_bombsSlider.Value = value;
        }

        private Visibility WillBeSlowVis(double _w, double _h, int _th) => _w * _h > _th ? Visibility.Visible : Visibility.Collapsed;

    }
}
