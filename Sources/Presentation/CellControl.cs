using Minesweeper.Logic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Minesweeper.Presentation
{
    internal class CellControl : Button
    {

        public CellControl()
        {
            UpdateState();
            IsRightTapEnabled = true;

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

        public void UpdateState()
        {
            IsEnabled = State != EState.UNCOVERED;
            switch (State)
            {
                case EState.COVERED:
                {
                    Content = null;
                }
                break;
                case EState.UNCOVERED:
                {
                    if (Data.IsMine)
                    {
                        Content = "M";
                    }
                    else
                    {
                        Content = Data.Neighbors;
                    }
                }
                break;
                case EState.FLAGGED:
                {
                    Content = "F";
                }
                break;
            }
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
                case EState.COVERED:
                State = EState.FLAGGED;
                break;
                case EState.FLAGGED:
                State = EState.COVERED;
                break;
            }
        }

    }

}
