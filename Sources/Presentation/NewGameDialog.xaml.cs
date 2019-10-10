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
            this.InitializeComponent();
        }

        private double MaxBombs(double _w, double _h) => ((int) _w) * ((int) _h) - 1;

        public int WidthSetting => (int) m_widthSlider.Value;
        public int HeightSetting => (int) m_heightSlider.Value;
        public int BombsCountSetting => (int) m_bombsSlider.Value;

    }
}
