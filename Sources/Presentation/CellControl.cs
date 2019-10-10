using Minesweeper.Logic;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Minesweeper.Presentation
{
    internal class CellControl : Button
    {

        public (int x, int y) Index { get; }

        public CellControl(int _x, int _y)
        {
            Content = new FontIcon();
            UpdateState();
            IsRightTapEnabled = true;
            Index = (_x, _y);
        }

        public enum EState
        {
            COVERED, UNCOVERED, FLAGGED
        }

        private EState m_state = EState.COVERED;

        public EState State
        {
            get => m_state;
            set
            {
                if (m_state != value)
                {
                    EState previous = m_state;
                    m_state = value;
                    UpdateState();
                    OnStateChanged?.Invoke(this, previous);
                }
            }
        }

        private Minefield.ICell m_data = null;

        public Minefield.ICell Data
        {
            get => m_data;
            set
            {
                if (m_data != value)
                {
                    m_data = value;
                    UpdateState();
                }
            }
        }

        public delegate void OnStateChangedHandler(CellControl _cell, EState _previous);

        public event OnStateChangedHandler OnStateChanged;

        public Func<bool> Flaggable { get; set; } = null;

        public void UpdateState()
        {
            IsEnabled = State != EState.UNCOVERED;
            char icon = '0';
            switch (State)
            {
                case EState.UNCOVERED:
                {
                    if (Data.IsBomb)
                    {
                        icon = 'b';
                    }
                    else
                    {
                        icon = Data.Neighbors.ToString()[0];
                    }
                }
                break;
                case EState.FLAGGED:
                {
                    icon = 'f';
                }
                break;
            }
            (Content as FontIcon).Glyph = icon.ToString();
        }

        protected override void OnTapped(TappedRoutedEventArgs _e)
        {
            base.OnTapped(_e);
            switch (State)
            {
                case EState.COVERED when Data != null:
                State = EState.UNCOVERED;
                break;
                case EState.FLAGGED:
                State = EState.COVERED;
                break;
            }
        }

        protected override void OnRightTapped(RightTappedRoutedEventArgs _e)
        {
            base.OnRightTapped(_e);
            switch (State)
            {
                case EState.COVERED when Flaggable?.Invoke() == true:
                State = EState.FLAGGED;
                break;
                case EState.FLAGGED:
                State = EState.COVERED;
                break;
            }
        }

    }

}
