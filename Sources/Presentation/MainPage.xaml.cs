using Minesweeper.Logic;
using Minesweeper.Presentation;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Minesweeper
{
    internal sealed partial class MainPage : Page
    {

        public MainPage() => InitializeComponent();

        #region Serialization

        private void Page_Loaded(object _sender, RoutedEventArgs _e) => _ = LoadGame();

        private const string c_saveFile = "MainPage.GridControl.SaveGame.DataContract";

        private async Task<bool> WriteSaveGame(GridControl.SaveGame _save)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await storageFolder.CreateFileAsync(c_saveFile, CreationCollisionOption.ReplaceExisting);
                using (Stream stream = await file.OpenStreamForWriteAsync())
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(GridControl.SaveGame));
                    serializer.WriteObject(stream, _save);
                    return true;
                }
            }
            catch (Exception _e)
            {
                return false;
            }
        }

        private async Task<GridControl.SaveGame> ReadSaveGame()
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                if (new FileInfo(Path.Combine(storageFolder.Path, c_saveFile)).Exists)
                {
                    StorageFile file = await storageFolder.GetFileAsync(c_saveFile);
                    using (Stream stream = await file.OpenStreamForWriteAsync())
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(GridControl.SaveGame));
                        return ((GridControl.SaveGame) serializer.ReadObject(stream));
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception _e)
            {
                return null;
            }
        }

        private async Task RunOnUIThread(Action _func)
        {
            if (!Window.Current.Visible || Window.Current.Dispatcher.HasThreadAccess)
            {
                _func();
            }
            else
            {
                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    _func();
                });
            }
        }

        public async Task<bool> SaveGame()
        {
            GridControl.SaveGame save = null;
            await RunOnUIThread(() =>
            {
                save = GridControl.SaveGame.Save(m_grid);
            });
            return await WriteSaveGame(save);
        }

        private async Task<bool> LoadGame()
        {
            await RunOnUIThread(() =>
            {
                m_commandBar.IsEnabled = false;
            });
            GridControl.SaveGame save = await ReadSaveGame();
            await RunOnUIThread(() =>
            {
                save?.Restore(m_grid);
                m_commandBar.IsEnabled = true;
            });
            return save != null;
        }

        #endregion

        #region XAML Helpers

        private bool IsNotNull(object _obj) => _obj != null;

        private double AnastasioOpacity(int? _covered, int _total) => (1.0 - (_covered ?? _total) / (double) _total) * 0.25;
        private Visibility IsNotNullVis(object _obj) => BoolToVisibility(IsNotNull(_obj));

        private Visibility BoolToVisibility(bool _b) => _b ? Visibility.Visible : Visibility.Collapsed;

        private string GridSizeString(int _w, int _h) => $"{_w}x{_h}";

        private string DenominatorString(int _n) => $"/{_n}";

        #endregion

        #region Input

        private async void NewGameButton_Click(object _sender, RoutedEventArgs _e)
        {
            NewGameDialog dialog = new NewGameDialog()
            {
                WidthSetting = m_grid.Minefield?.Width ?? 8,
                HeightSetting = m_grid.Minefield?.Height ?? 6,
                BombsCountSetting = m_grid.Minefield?.BombCount ?? 10
            };
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

        #endregion

    }
}
