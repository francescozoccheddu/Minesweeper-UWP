using Minesweeper.Logic;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Minesweeper.Presentation
{
    internal sealed partial class CellControl : UserControl
    {

        public interface IGrid
        {

            bool CanFlag { get; }

            UIElement FocusSearchRoot { get; }

            Control GetFocusNeighbor((int x, int y) _index);

        }

        public CellControl(Minefield.ICell _data, (int x, int y) _index, IGrid _grid)
        {
            if (_data == null)
            {
                throw new ArgumentNullException(nameof(_data));
            }
            if (_grid == null)
            {
                throw new ArgumentNullException(nameof(_grid));
            }
            m_grid = _grid;
            Index = _index;
            m_uncoveredGlyph = _data.IsBomb ? "b" : _data.Neighbors.ToString();
            m_hasNeighbors = !_data.IsBomb && _data.Neighbors > 0;
            // Initialize
            InitializeComponent();
            IsTabStop = true;
            IsTapEnabled = true;
            IsRightTapEnabled = true;
            Update();
        }

        private readonly IGrid m_grid;

        public enum EState
        {
            FLAGGED, UNCOVERED, COVERED
        }

        public delegate void FlagEventHandler(CellControl _sender, bool _flagged);
        public delegate void UncoverEventHandler(CellControl _sender);

        public event FlagEventHandler OnFlagChanged;
        public event UncoverEventHandler OnUncovered;
        public event UncoverEventHandler OnUncoverNeighbors;

        private EState m_state = EState.COVERED;

        private readonly string m_uncoveredGlyph;
        private readonly bool m_hasNeighbors;

        public EState State
        {
            get => m_state;
            set
            {
                if (m_state != value)
                {
                    m_state = value;
                    Update();
                }
            }
        }

        public void Update()
        {
            switch (State)
            {
                case EState.FLAGGED:
                m_CanFlag = true;
                m_CanUncover = true;
                VisualStateManager.GoToState(this, "StateFlagged", true);
                break;
                case EState.UNCOVERED:
                m_CanFlag = false;
                m_CanUncover = m_hasNeighbors;
                VisualStateManager.GoToState(this, "StateUncovered", true);
                break;
                case EState.COVERED:
                m_CanFlag = m_grid.CanFlag;
                m_CanUncover = true;
                VisualStateManager.GoToState(this, "StateCovered", true);
                break;
            }
        }

        public (int x, int y) Index { get; }

        private bool m_canUncover;
        private bool m_CanUncover
        {
            get => m_canUncover;
            set
            {
                if (m_canUncover != value)
                {
                    m_canUncover = value;
                    if (!value && m_PendingUncover)
                    {
                        CancelInput();
                    }
                    UpdateIsEnabled();
                }
            }
        }

        private bool m_canFlag;
        private bool m_CanFlag
        {
            get => m_canFlag;
            set
            {
                if (m_canFlag != value)
                {
                    m_canFlag = value;
                    if (!value && m_PendingFlag)
                    {
                        CancelInput();
                    }
                    UpdateIsEnabled();
                }
            }
        }

        private void Flag()
        {
            switch (State)
            {
                case EState.FLAGGED:
                State = EState.COVERED;
                break;
                case EState.COVERED:
                State = EState.FLAGGED;
                break;
            }
            OnFlagChanged?.Invoke(this, State == EState.FLAGGED);
        }

        private void Uncover()
        {
            switch (State)
            {
                case EState.FLAGGED:
                State = EState.COVERED;
                OnFlagChanged?.Invoke(this, false);
                break;
                case EState.COVERED:
                State = EState.UNCOVERED;
                OnUncovered?.Invoke(this);
                break;
                case EState.UNCOVERED:
                OnUncoverNeighbors?.Invoke(this);
                break;
            }
        }

        #region Input

        public void UpdateIsEnabled()
        {
            bool shouldBeEnabled = m_CanFlag || m_CanUncover;
            if (IsEnabled && !shouldBeEnabled && m_focused)
            {
                if (m_grid.GetFocusNeighbor(Index)?.Focus(FocusState.Programmatic) != true)
                {
                    FindNextElementOptions options = new FindNextElementOptions()
                    {
                        SearchRoot = m_grid.FocusSearchRoot
                    };
                    bool done =
                        FocusManager.TryMoveFocus(FocusNavigationDirection.Right, options)
                        || FocusManager.TryMoveFocus(FocusNavigationDirection.Down, options)
                        || FocusManager.TryMoveFocus(FocusNavigationDirection.Left, options)
                        || FocusManager.TryMoveFocus(FocusNavigationDirection.Up, options);
                }
            }
            IsEnabled = shouldBeEnabled;
        }

        private enum EInputType
        {
            PointerUncover, PointerFlag, KeyUncover, KeyFlag
        }

        private EInputType? m_pendingInput = null;
        private object m_pendingInputInfo = null;
        private Pointer m_capturedPointer = null;
        private bool m_focused;

        private bool m_PendingFlag => m_pendingInput == EInputType.PointerFlag || m_pendingInput == EInputType.KeyFlag;
        private bool m_PendingUncover => m_pendingInput == EInputType.PointerUncover || m_pendingInput == EInputType.KeyUncover;

        private EInputType? GetInputType(PointerRoutedEventArgs _e)
        {
            switch (_e.Pointer.PointerDeviceType)
            {
                case Windows.Devices.Input.PointerDeviceType.Touch:
                case Windows.Devices.Input.PointerDeviceType.Pen:
                return EInputType.PointerUncover;
                case Windows.Devices.Input.PointerDeviceType.Mouse:
                switch (_e.GetCurrentPoint(this).Properties.PointerUpdateKind)
                {
                    case Windows.UI.Input.PointerUpdateKind.LeftButtonPressed:
                    case Windows.UI.Input.PointerUpdateKind.LeftButtonReleased:
                    return EInputType.PointerUncover;
                    case Windows.UI.Input.PointerUpdateKind.RightButtonPressed:
                    case Windows.UI.Input.PointerUpdateKind.RightButtonReleased:
                    return EInputType.PointerFlag;
                }
                break;
            }
            return null;
        }

        private EInputType? GetInputType(KeyRoutedEventArgs _e)
        {
            switch (_e.Key)
            {
                case VirtualKey.Space:
                case VirtualKey.Enter:
                return EInputType.KeyUncover;
                case VirtualKey.F:
                case VirtualKey.Back:
                return EInputType.KeyFlag;
                default:
                return null;
            }
        }

        private bool ProcessInput(EInputType _type, bool _entering, object _info)
        {
            if (m_pendingInput != null)
            {
                if (!_entering && _type == m_pendingInput && m_pendingInputInfo?.Equals(_info) == true)
                {
                    switch (_type)
                    {
                        case EInputType.PointerUncover:
                        case EInputType.KeyUncover:
                        if (m_CanUncover)
                        {
                            Uncover();
                        }

                        break;
                        case EInputType.PointerFlag:
                        case EInputType.KeyFlag:
                        if (m_CanFlag)
                        {
                            Flag();
                        }

                        break;
                    }
                    CancelInput();
                    return true;
                }
            }
            else if (_entering)
            {
                switch (_type)
                {
                    case EInputType.PointerUncover:
                    case EInputType.KeyUncover:
                    if (!m_CanUncover)
                    {
                        return false;
                    }

                    break;
                    case EInputType.PointerFlag:
                    case EInputType.KeyFlag:
                    if (!m_CanFlag)
                    {
                        return false;
                    }

                    break;
                }
                m_pendingInput = _type;
                m_pendingInputInfo = _info;
                VisualStateManager.GoToState(this, "PointerDown", true);
                return true;
            }
            return false;
        }

        private void CancelInput()
        {
            if (m_capturedPointer != null)
            {
                ReleasePointerCapture(m_capturedPointer);
                m_capturedPointer = null;
            }
            m_pendingInput = null;
            VisualStateManager.GoToState(this, "PointerUp", true);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs _e)
        {
            base.OnPointerPressed(_e);
            if (m_capturedPointer == null)
            {
                EInputType? type = GetInputType(_e);
                if (type is EInputType ntype)
                {
                    _e.Handled = true;
                    if (ProcessInput(ntype, true, _e.Pointer.PointerId))
                    {
                        m_capturedPointer = _e.Pointer;
                        CapturePointer(_e.Pointer);
                    }
                }
            }
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs _e)
        {
            base.OnPointerReleased(_e);
            EInputType? type = GetInputType(_e);
            if (type is EInputType ntype)
            {
                _e.Handled = true;
                ProcessInput(ntype, false, _e.Pointer.PointerId);
            }
        }

        protected override void OnPointerCanceled(PointerRoutedEventArgs _e)
        {
            base.OnPointerCanceled(_e);
            EInputType? type = GetInputType(_e);
            if (type is EInputType ntype)
            {
                _e.Handled = true;
                ProcessInput(ntype, false, _e.Pointer.PointerId);
            }
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs _e)
        {
            base.OnPointerEntered(_e);
            if (m_CanFlag || m_CanUncover)
            {
                _e.Handled = true;
                VisualStateManager.GoToState(this, "PointerInside", true);
            }
        }

        protected override void OnPointerExited(PointerRoutedEventArgs _e)
        {
            base.OnPointerExited(_e);
            _e.Handled = true;
            VisualStateManager.GoToState(this, "PointerOutside", true);
        }

        protected override void OnGotFocus(RoutedEventArgs _e)
        {
            base.OnGotFocus(_e);
            m_focused = true;
            if (m_CanFlag || m_CanUncover)
            {
                VisualStateManager.GoToState(this, "Focused", true);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs _e)
        {
            base.OnLostFocus(_e);
            m_focused = false;
            VisualStateManager.GoToState(this, "Unfocused", true);
            if (m_pendingInput == EInputType.KeyFlag || m_pendingInput == EInputType.KeyUncover)
            {
                CancelInput();
            }
        }

        protected override void OnKeyDown(KeyRoutedEventArgs _e)
        {
            base.OnKeyDown(_e);
            EInputType? type = GetInputType(_e);
            if (type is EInputType ntype)
            {
                _e.Handled = true;
                ProcessInput(ntype, true, (_e.DeviceId, _e.Key));
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs _e)
        {
            base.OnKeyUp(_e);
            EInputType? type = GetInputType(_e);
            if (type is EInputType ntype)
            {
                _e.Handled = true;
                ProcessInput(ntype, false, (_e.DeviceId, _e.Key));
            }
        }

        #endregion

    }

}
