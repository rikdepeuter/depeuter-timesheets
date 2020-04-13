using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DePeuter.Shared.Wpf.Commands;
using DePeuter.Timesheets.Constants;
using DePeuter.Timesheets.Controls.Timesheets;
using DePeuter.Timesheets.Database.Contexts;
using DePeuter.Timesheets.Database.DTO;
using DePeuter.Timesheets.Database.Entities;
using DePeuter.Timesheets.Infrastructure.Controls;
using DePeuter.Timesheets.Utils;
using Microsoft.Win32;
using Timer = System.Threading.Timer;

namespace DePeuter.Timesheets.Controls.TimesheetDay
{
    /// <summary>
    /// Interaction logic for SupportDayControlView.xaml
    /// </summary>
    public partial class TimesheetDayView : UserControlBase
    {
        public TimesheetDayViewModel ViewModel { get { return (TimesheetDayViewModel)DataContext; } }

        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public int IntervalMinute { get; set; }
        public GridLength ItemsHeight { get; set; }
        public GridLength ItemsWidth
        {
            get { return cdContent.Width; }
            set { cdContent.Width = value; }
        }
        public bool ScrollbarIsVisible
        {
            get { return svMain.VerticalScrollBarVisibility == ScrollBarVisibility.Auto; }
            set { svMain.VerticalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden; }
        }

        private TotalTimeControl _newTotalTime;

        public bool ConfirmNewTime { get; set; }

        public TimesheetDayView()
        {
            InitializeComponent();

            DataContextChanged += SupportDayControlView_DataContextChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            UpdateInactiveTimes();
        }

        public void Initialize(int startHour, int startMinute, int intervalMinute)
        {
            StartHour = startHour;
            StartMinute = startMinute;
            IntervalMinute = intervalMinute;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;

            if(IntervalMinute == 0)
            {
                return;
            }

            RedrawGrid();

            ScrollToStart();

            var vm = ViewModel;
            if(vm != null)
            {
                vm.RefreshData();
            }
        }

        public void ScrollToStart()
        {
            ScrollTo(StartHour, StartMinute);
        }
        public void ScrollTo(int hour, int minute)
        {
            var partsPerHour = 60.0/IntervalMinute;
            var itemsOffset = hour*partsPerHour;
            if(minute > 0)
            {
                if(minute%IntervalMinute == 0)
                {
                    var parts = minute/IntervalMinute;
                    itemsOffset += parts;
                }
            }
            var topOffset = itemsOffset * ItemsHeight.Value;
            svMain.ScrollToVerticalOffset(topOffset);
        }

        public void UpdateLastTimesheetToNow()
        {
            var endTime = DateTime.Now.RoundByMinute(IntervalMinute);
            var item = ViewModel.Items.LastOrDefault();
            if(item == null)
            {
                return;
            }

            if(item.Type == (int)TimesheetType.Break)
            {
                var item2 = ViewModel.Items.Reverse().FirstOrDefault(x => x.Type == (int)TimesheetType.Normal);
                if(item2 == null)
                {
                    return;
                }

                var breakEndTime = item.EndTime;
                item = EF.Clone(item2);
                item.StartTime = breakEndTime;
            }

            if(item.EndTime != endTime)
            {
                item.EndTime = endTime;

                //var newItem = new Timesheet()
                //{
                //    Username = item2.Username,
                //    Type = item2.Type,
                //    TaskNumber = item2.TaskNumber,
                //    Description = item2.Description,
                //    StartTime = item.EndTime,
                //    EndTime = endTime
                //};
                TimesheetsContext.My.Save(item);
            }
        }

        void SupportDayControlView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = ViewModel;

            vm.RequestUpdateItems += vm_RequestUpdateItems;
            vm.RequestUpdateNewItem += vm_RequestUpdateNewItem;
        }

        void vm_RequestUpdateItems(object sender, EventArgs e)
        {
            if(!_timeIndices.Any())
            {
                return;
            }

            var items = ViewModel.Items;

            var itemControls = gMain.Children.OfType<TimeItemControl>().ToArray();
            foreach(var itemControl in itemControls)
            {
                gMain.Children.Remove(itemControl);
            }

            if(items != null)
            {
                foreach(var item in items)
                {
                    AddSupportDayItemControl(item);
                }

                SetWidthsOfTimeItemControls();
            }

            UpdateCurrentTime();
            UpdateInactiveTimes();
            //UpdateStartTime();
            //UpdateEndTime();
        }

        void vm_RequestUpdateNewItem(object sender, TimePeriodChangingEventArgs e)
        {
            var startEnd = new[] { e.Start, e.End };

            SetNewSupportDayItemControl(startEnd.Min(), startEnd.Max(), false);
        }

        void control_TimePeriodChanged(object sender, TimePeriodChangingEventArgs e)
        {
            var control = (TimeItemControl)sender;

            if(control.IsNew && ConfirmNewTime)
            {
                return;
            }

            RaiseTimePeriodChanged(sender, e);
        }

        void RaiseTimePeriodChanged(object sender, TimePeriodChangingEventArgs e)
        {
            ViewModel.RaiseTimePeriodChanged(sender, e);

            SetWidthsOfTimeItemControls();
        }

        void control_SelectedChanged(object sender, EventArgs e)
        {
            var control = (TimeItemControl)sender;

            if(control.IsSelected)
            {
                var itemControls = gMain.Children.OfType<TimeItemControl>().Where(x => x.IsSelected && !Equals(x, control)).ToArray();
                foreach(var itemControl in itemControls)
                {
                    itemControl.IsSelected = false;
                }

                var maxZIndex = gMain.Children.OfType<TimeItemControl>().Select(Grid.GetZIndex).Max();
                Grid.SetZIndex(control, maxZIndex + 1);
            }
        }

        private int GetRowIndex(DateTime start)
        {
            var minute = start.Minute;
            minute = (int)(Math.Round((double)minute/IntervalMinute) * IntervalMinute);

            var key = string.Format("{0:00}{1:00}", start.Hour, minute);
            if(_timeIndices.ContainsKey(key))
            {
                return _timeIndices[key];
            }
            return 0;
        }
        private int GetRowSpan(DateTime start, DateTime end)
        {
            var startMinutes = start.Hour*60 + start.Minute;
            var endMinutes = end.Hour*60 + end.Minute;
            var differenceMinutes = endMinutes - startMinutes;

            return Math.Abs(differenceMinutes/IntervalMinute);
        }
        private DateTime GetTime(int row)
        {
            var key = _timeIndices.Where(x => x.Value == row).Select(x => x.Key).Single(); //_timeIndices.GetKeyByValue(row);
            var hour = int.Parse(key.Substring(0, 2));
            var minute = int.Parse(key.Substring(2, 2));
            var day = 1;
            while (hour >= 24)
            {
                hour -= 24;
                day += 1;
            }
            return new DateTime(1, 1, day, hour, minute, 0);
        }

        private readonly Dictionary<string, int> _timeIndices = new Dictionary<string, int>();

        #region "Inner controls"
        private class TimeItemControl : UserControl
        {
            private readonly TimesheetDayView _parent;
            public Timesheet Item { get; private set; }

            private static readonly Brush BorderColor = Brushes.LightGray;
            private static readonly Brush SelectedColor = Brushes.Black; //SystemColors.HighlightBrush;

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if(_isSelected == value)
                    {
                        return;
                    }

                    _isSelected = value;

                    _border.BorderBrush = value ? SelectedColor : BorderColor;

                    if(SelectedChanged != null)
                    {
                        SelectedChanged(this, EventArgs.Empty);
                    }
                }
            }

            private bool _isResizingTop;
            public bool IsResizingTop
            {
                get { return _isResizingTop; }
                set
                {
                    if(_isResizingTop == value)
                    {
                        return;
                    }

                    _isResizingTop = value;

                    RefreshResizeInfo();

                    if(!value)
                    {
                        Cursor = Cursors.Arrow;

                        RaiseTimePeriodChanged();
                    }
                    else
                    {
                        IsSelected = true;
                    }
                }
            }

            private bool _isResizingBottom;
            public bool IsResizingBottom
            {
                get { return _isResizingBottom; }
                set
                {
                    if(_isResizingBottom == value)
                    {
                        return;
                    }

                    _isResizingBottom = value;

                    RefreshResizeInfo();

                    if(!value)
                    {
                        Cursor = Cursors.Arrow;

                        RaiseTimePeriodChanged();
                    }
                    else
                    {
                        IsSelected = true;
                    }
                }
            }

            public int OriginalMovingRowIndex { get; private set; }
            public int OriginalMovingMouseRowIndex { get; private set; }
            private bool _isMoving;
            private Brush _originalBackground = Brushes.Transparent;
            public bool IsMoving
            {
                get { return _isMoving; }
                set
                {
                    if(_isMoving == value)
                    {
                        return;
                    }

                    if(value && (IsResizingTop || IsResizingBottom))
                    {
                        return;
                    }

                    Background = value ? SystemColors.HighlightBrush : _originalBackground;

                    _isMoving = value;

                    if(!value)
                    {
                        RaiseTimePeriodChanged();
                    }
                    else
                    {
                        IsSelected = true;

                        OriginalMovingRowIndex = Grid.GetRow(this);
                        OriginalMovingMouseRowIndex = _parent.GetMouseRowIndex();
                    }
                }
            }

            public bool IsNew { get { return Item.IsNew; } }
            public int Id { get { return Item.Id; } }

            public event EventHandler SelectedChanged;
            public event EventHandler<TimePeriodChangingEventArgs> TimePeriodChanging;

            public new Brush Background
            {
                get { return _border.Background; }
                set { _border.Background = value; }
            }

            private readonly Border _border;
            private readonly TotalTimeControl _totalTime;

            public TimeItemControl(TimesheetDayView parent, Timesheet item)
            {
                Item = item;
                _parent = parent;

                base.Background = Brushes.Transparent;

                var contentGrid = new Grid
                {
                    Background = Brushes.Transparent
                };
                var tb = new TextBlock
                {
                    Text = Item.Description,
                    Margin = new Thickness(2, 0, 0, 0),
                    Background = Brushes.Transparent,
                    TextWrapping = TextWrapping.WrapWithOverflow
                };
                if(Item.TaskNumber != null)
                {
                    //var job = Session.Jobs.Single(x => x.Id == Item.JobId.Value);
                    tb.Text = string.Format("[{0}] {1}", Item.TaskNumber, Item.Description);
                    //if(!string.IsNullOrEmpty(job.Project))
                    //{
                    //    tb.Text += string.Format(" ({0})", job.Project);
                    //}
                }
                contentGrid.Children.Add(tb);

                var buttons = new WrapPanel()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                //if(!item.IsNew())
                //{
                //    var vm = _parent.ViewModel.Parent as SupportDetailViewModel;
                //    if(vm == null || Id != vm.Entity.Id)
                //    {
                //        var openButton = new Button
                //        {
                //            Command = parent.ViewModel.OpenItemCommand,
                //            CommandParameter = item.Data,
                //            Content = new Image { Source = Properties.Resources.Support.ToBitmapImage(), Width = 16, Height = 16 },
                //            Focusable = false,
                //            BorderThickness = new Thickness(0),
                //            Background = Brushes.Transparent,
                //            ToolTip = "Open Support",
                //            Style = (Style)FindResource(ToolBar.ButtonStyleKey),
                //            Opacity = 0.3
                //        };
                //        openButton.MouseEnter += (s, e) =>
                //        {
                //            var b = (Button)s;
                //            b.Opacity = 1;
                //        };
                //        openButton.MouseLeave += (s, e) =>
                //        {
                //            var b = (Button)s;
                //            b.Opacity = 0.3;
                //        };
                //        buttons.Children.Add(openButton);
                //    }
                //}

                contentGrid.Children.Add(buttons);

                _totalTime = new TotalTimeControl(parent);
                contentGrid.Children.Add(_totalTime);

                var grid = new Grid
                {
                    Background = Brushes.Transparent
                };

                _border = new Border
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = BorderColor,
                    Child = contentGrid
                };
                grid.Children.Add(_border);

                var resizeTop = new Grid { Background = Brushes.Transparent, Height = 5, VerticalAlignment = VerticalAlignment.Top };
                var resizeBottom = new Grid { Background = Brushes.Transparent, Height = 5, VerticalAlignment = VerticalAlignment.Bottom };
                resizeTop.MouseMove += resize_MouseMove;
                resizeTop.MouseLeave += resize_MouseLeave;
                resizeTop.MouseDown += resizeTop_MouseDown;
                resizeBottom.MouseMove += resize_MouseMove;
                resizeBottom.MouseLeave += resize_MouseLeave;
                resizeBottom.MouseDown += resizeBottom_MouseDown;

                grid.Children.Add(resizeTop);
                grid.Children.Add(resizeBottom);

                var cc = new ContentControl
                {
                    Content = grid,
                    Margin = new Thickness(0, 0, 2, 2)
                };
                cc.MouseDoubleClick += cc_MouseDoubleClick;
                cc.MouseDown += cc_MouseDown;
                cc.MouseUp += cc_MouseUp;

                Content = cc;
                ToolTip = TimesheetSearchResult.GetToolTip(Item.Id, Item.StartTime, Item.EndTime, Item.Type, Item.TaskNumber, Item.Description);
                UpdateBackground();

                ContextMenu = new ContextMenu();
                ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Edit",
                    Icon = new Image { Source = Properties.Resources.edit.ToBitmapImage(), Width = 16, Height = 16 },
                    Command = NewContextMenuCommand("Edit"),
                    CommandParameter = Item
                });
                ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Fill",
                    Icon = new Image { Source = Properties.Resources.expand_16.ToBitmapImage(), Width = 16, Height = 16 },
                    Command = _parent.ViewModel.FillCommand,
                    CommandParameter = Item
                });
                ContextMenu.Items.Add(new Separator());
                ContextMenu.Items.Add(new MenuItem()
                {
                    Header = "Delete",
                    Icon = new Image { Source = Properties.Resources.delete.ToBitmapImage(), Width = 16, Height = 16 },
                    Command = NewContextMenuCommand("Delete"),
                    CommandParameter = Item
                });
            }

            private ICommand NewContextMenuCommand(string commandName)
            {
                return _parent.ViewModel.NewContextMenuCommand(commandName);
            }

            void cc_MouseDown(object sender, MouseButtonEventArgs e)
            {
                if(e.ChangedButton == MouseButton.Left)
                {
                    if(!IsMoving)
                    {
                        IsSelected = !IsSelected;
                    }

                    e.Handled = true;
                }
            }

            void cc_MouseUp(object sender, MouseButtonEventArgs e)
            {
                if(e.ChangedButton == MouseButton.Left)
                {
                    IsMoving = false;
                }
            }

            void cc_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            {
                IsMoving = true;
            }

            void resizeTop_MouseDown(object sender, MouseButtonEventArgs e)
            {
                IsResizingTop = true;
                e.Handled = true;
            }

            void resizeBottom_MouseDown(object sender, MouseButtonEventArgs e)
            {
                IsResizingBottom = true;
                e.Handled = true;
            }

            void resize_MouseLeave(object sender, MouseEventArgs e)
            {
                if(!IsResizingTop)
                {
                    Cursor = Cursors.Arrow;
                }
            }

            void resize_MouseMove(object sender, MouseEventArgs e)
            {
                if(!IsMoving)
                {
                    Cursor = Cursors.SizeNS;
                }
            }

            private DateTime[] GetStartAndEnd()
            {
                var row = Grid.GetRow(this);
                var rowSpan = Grid.GetRowSpan(this);
                var start = _parent.GetTime(row);
                var end = _parent.GetTime(row + rowSpan);

                start = Item.StartTime.Date.AddHours(start.Hour).AddMinutes(start.Minute);
                end = Item.EndTime.Date.AddHours(end.Hour).AddMinutes(end.Minute);

                return new[] { start, end };
            }
            private bool _disableTimePeriodChanged;
            void RaiseTimePeriodChanged()
            {
                if(TimePeriodChanging == null || _disableTimePeriodChanged)
                {
                    return;
                }

                var startAndEnd = GetStartAndEnd();
                var start = startAndEnd.First();
                var end = startAndEnd.Last();

                if(start == Item.StartTime && end == Item.EndTime)
                {
                    return;
                }

                var e = new TimePeriodChangingEventArgs(Item, start, end);
                try
                {
                    TimePeriodChanging(this, e);
                }
                catch(Exception ex)
                {
                    _parent.HandleException(ex);
                    e.Cancel = true;
                }

                if(e.Cancel)
                {
                    ResetRowAndRowSpan();
                }
                else
                {
                    Item.StartTime = start;
                    Item.EndTime = end;
                    //Item = new TimeItem(start, end, Item.Data, Item.Tooltip);
                }
            }

            public void ResetRowAndRowSpan()
            {
                SetSize(_parent.GetRowIndex(Item.StartTime), _parent.GetRowSpan(Item.StartTime, Item.EndTime));
            }

            public void Cancel()
            {
                _disableTimePeriodChanged = true;
                try
                {
                    IsResizingTop = false;
                    IsResizingBottom = false;
                    IsMoving = false;
                }
                finally
                {
                    _disableTimePeriodChanged = false;
                }

                ResetRowAndRowSpan();
            }

            private void SetBackground(Brush brush)
            {
                _originalBackground = brush;
                Background = brush;
            }

            private Color ConvertTimesheetTypeToColor(TimesheetType timesheetType)
            {
                switch(timesheetType)
                {
                    case TimesheetType.Break:
                        return Colors.Beige;
                }

                return Color.FromRgb(238, 243, 249);
            }
            public void UpdateBackground()
            {
                SetBackground(IsNew ? Brushes.WhiteSmoke : new SolidColorBrush(ConvertTimesheetTypeToColor((TimesheetType)Item.Type)));
            }

            public void SetSize(int? row, int? rowSpan)
            {
                if(row != null)
                {
                    Grid.SetRow(this, row.Value);
                }
                if(rowSpan != null)
                {
                    if(rowSpan.Value == 0)
                    {
                        Visibility = Visibility.Collapsed;
                        return;
                    }

                    Grid.SetRowSpan(this, rowSpan.Value);
                }

                RefreshResizeInfo();
            }

            private void RefreshResizeInfo()
            {
                var visible = IsResizingTop || IsResizingBottom;

                if(!visible)
                {
                    _totalTime.Hide();
                    return;
                }

                var startAndEnd = GetStartAndEnd();
                var start = startAndEnd.First();
                var end = startAndEnd.Last();

                var duration = end - start;

                if(IsResizingTop)
                {
                    _totalTime.Show(duration, VerticalAlignment.Top, Background);
                }
                else if(IsResizingBottom)
                {
                    _totalTime.Show(duration, VerticalAlignment.Bottom, Background);
                }
            }
        }

        private class RowIndexControl : UserControl
        {
            public int RowIndex { get; private set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if(_isSelected == value)
                    {
                        return;
                    }
                    _isSelected = value;
                    Background = GetBackgroundColor();
                }
            }

            public RowIndexControl(int rowIndex, Brush gridLineBrush)
            {
                RowIndex = rowIndex;
                IsSelected = false;

                var border = new Border
                {
                    Background = Brushes.Transparent,
                    BorderBrush = gridLineBrush,
                    BorderThickness = new Thickness(1, 0, 0, 0)
                };

                Content = border;
            }

            private Brush GetBackgroundColor()
            {
                if(IsSelected)
                {
                    return SystemColors.HighlightBrush;
                }
                return Brushes.Transparent;
            }
        }

        private class TotalTimeControl : UserControl
        {
            private readonly Grid _grid;
            private readonly TextBlock _tb;
            private readonly TextBlock _tbStart;
            private readonly TextBlock _tbEnd;

            private readonly TimesheetDayView _parent;

            public TotalTimeControl(TimesheetDayView parent)
            {
                Visibility = Visibility.Collapsed;

                _parent = parent;

                _grid = new Grid();
                _tb = new TextBlock
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 2, 0, 2)
                };
                _tbStart = new TextBlock
                {
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(2, 0, 0, 0)
                };
                _tbEnd = new TextBlock
                {
                    TextAlignment = TextAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(2, 0, 0, 0)
                };

                _grid.Children.Add(_tb);
                _grid.Children.Add(_tbStart);
                _grid.Children.Add(_tbEnd);

                Content = _grid;
            }

            public void Show(TimeSpan duration, VerticalAlignment alignment, Brush background)
            {
                Show(null, null, duration, alignment, background);
            }
            public void Show(DateTime start, DateTime end, VerticalAlignment alignment, Brush background)
            {
                Show(start, end, end - start, alignment, background);
            }

            private void Show(DateTime? start, DateTime? end, TimeSpan? duration, VerticalAlignment alignment, Brush background)
            {
                _grid.Background = background;
                _tb.Text = duration != null ? string.Format("{0:00}:{1:00}", duration.Value.Hours, duration.Value.Minutes) : null;
                _tb.VerticalAlignment = alignment;
                _tbStart.Text = start != null ? string.Format("{0:0}:{1:00}", start.Value.Hour, start.Value.Minute) : null;
                _tbEnd.Text = end != null ? string.Format("{0:0}:{1:00}", end.Value.Hour, end.Value.Minute) : null;
                if(duration == null)
                {
                    duration = end - start;
                }
                _tbEnd.Visibility = end != null && duration.Value.TotalMinutes >= 15 ? Visibility.Visible : Visibility.Collapsed;
                Visibility = Visibility.Visible;
            }

            public void Hide()
            {
                Visibility = Visibility.Collapsed;
            }
        }

        private class InactiveTimeControl : UserControl
        {
            private readonly TimesheetDayView _parent;

            public InactiveTimeControl(TimesheetDayView parent, double startMinutes, double endMinutes)
            {
                _parent = parent;

                var totalMinutes = endMinutes - startMinutes;

                Content = new Grid()
                {
                    Background = Brushes.LightGray,
                    Height = ConvertMinutesToHeight(totalMinutes)
                };
                Opacity = 0.5;
                VerticalAlignment = VerticalAlignment.Top;

                Margin = new Thickness(0, ConvertMinutesToHeight(startMinutes), 0, 0);

                //ToolTip = string.Format("{0}", new MinutesDisplayConverter().Convert(totalMinutes, null, null, null));
            }

            private double ConvertMinutesToHeight(double minutes)
            {
                var intervalsPerHour = 60.0/_parent.IntervalMinute;
                var itemsHeightPerHour = _parent.ItemsHeight.Value*intervalsPerHour;
                var itemsHeightPerMinute = itemsHeightPerHour/60;

                return minutes*itemsHeightPerMinute;
            }
        }

        private class StartTimeControl : UserControl
        {
            private readonly Border _border;
            private readonly TextBlock _tb;
            private readonly Timer _timer;

            private readonly TimesheetDayView _parent;

            public StartTimeControl(TimesheetDayView parent)
            {
                _parent = parent;

                _border = new Border()
                {
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    BorderBrush = Brushes.Blue
                };
                //_tb = new TextBlock
                //{
                //    Foreground = Brushes.Blue,
                //    TextAlignment = TextAlignment.Center,
                //    Margin = new Thickness(0, 1, 0, 0)
                //};
                //_border.Child = _tb;

                Content = _border;
                VerticalAlignment = VerticalAlignment.Top;

                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            }

            void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            {
                RefreshTime();
            }

            public void RefreshTime()
            {
                var time = _parent.ViewModel.FindParent<TimesheetsViewModel>().GetDayStartTime(_parent.ViewModel.Date);
                if(time != null)
                {
                    var nowMinutes = time.Value.Hour*60 + time.Value.Minute;
                    var intervalsPerHour = 60.0/_parent.IntervalMinute;
                    var itemsHeightPerHour = _parent.ItemsHeight.Value*intervalsPerHour;
                    var itemsHeightPerMinute = itemsHeightPerHour/60;

                    Margin = new Thickness(0, nowMinutes*itemsHeightPerMinute, 0, 0);
                    //_tb.Text = time.Value.ToString("H:mm");
                }

                Visibility = time != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private class EndTimeControl : UserControl
        {
            private readonly Border _border;
            private readonly TextBlock _tb;
            private readonly Timer _timer;

            private readonly TimesheetDayView _parent;

            public EndTimeControl(TimesheetDayView parent)
            {
                _parent = parent;

                _border = new Border()
                {
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    BorderBrush = Brushes.Blue
                };
                //_tb = new TextBlock
                //{
                //    Foreground = Brushes.Blue,
                //    TextAlignment = TextAlignment.Center,
                //    Margin = new Thickness(0, 1, 0, 0)
                //};
                //_border.Child = _tb;

                Content = _border;
                VerticalAlignment = VerticalAlignment.Top;

                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            }

            void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            {
                RefreshTime();
            }

            public void RefreshTime()
            {
                var time = _parent.ViewModel.FindParent<TimesheetsViewModel>().GetDayEndTime(_parent.ViewModel.Date);
                if(time != null)
                {
                    var nowMinutes = time.Value.Hour*60 + time.Value.Minute;
                    var intervalsPerHour = 60.0/_parent.IntervalMinute;
                    var itemsHeightPerHour = _parent.ItemsHeight.Value*intervalsPerHour;
                    var itemsHeightPerMinute = itemsHeightPerHour/60;

                    Margin = new Thickness(0, nowMinutes*itemsHeightPerMinute, 0, 0);
                    //_tb.Text = time.Value.ToString("H:mm");
                }

                Visibility = time != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        private void RedrawGrid()
        {
            gMain.Children.Clear();
            gMain.RowDefinitions.Clear();

            var hour = 0;
            var minute = 0;

            _timeIndices.Clear();
            var intervalCount = 24*60/IntervalMinute; //_intervalCount; //(int)(1440.0/(double)IntervalMinute);
            for(var i = 0; i < intervalCount; i++)
            {
                _timeIndices.Add(string.Format("{0:00}{1:00}", hour, minute), i);

                minute += IntervalMinute;

                if(minute >= 60)
                {
                    hour++;
                    minute -= 60;
                }
            }

            _timeIndices.Add(string.Format("{0:00}{1:00}", hour, minute), intervalCount);

            hour = 0; //StartHour;
            minute = 0; //StartMinute;

            var isFirst = true;

            var gridLineBrush = new SolidColorBrush(Color.FromRgb(225, 225, 225));
            var intervalMinuteCount = 15/IntervalMinute;

            for(var i = 0; i < intervalCount; i++)
            {
                gMain.RowDefinitions.Add(new RowDefinition() { Height = ItemsHeight });

                if(minute == 0 || isFirst)
                {
                    var tb = new TextBlock { Text = hour + ":", HorizontalAlignment = HorizontalAlignment.Right };
                    gMain.Children.Add(tb);
                    Grid.SetColumn(tb, 0);
                    Grid.SetRow(tb, i);
                    Grid.SetRowSpan(tb, intervalMinuteCount);

                    var border = new Border
                    {
                        BorderBrush = gridLineBrush,
                        BorderThickness = new Thickness(0, 1, 0, 0)
                    };

                    gMain.Children.Add(border);
                    Grid.SetColumn(border, 0);
                    Grid.SetRow(border, i);
                    Grid.SetColumnSpan(border, 5);
                    Grid.SetZIndex(border, 2);
                }

                if(minute%15 == 0)
                {
                    var tb = new TextBlock
                    {
                        Text = minute.ToString("00")
                    };

                    gMain.Children.Add(tb);
                    Grid.SetColumn(tb, 1);
                    Grid.SetRow(tb, i);
                    Grid.SetRowSpan(tb, intervalMinuteCount);
                }

                {
                    var tb = new RowIndexControl(i, gridLineBrush);
                    gMain.Children.Add(tb);
                    Grid.SetColumn(tb, 3);
                    Grid.SetColumnSpan(tb, 2);
                    Grid.SetRow(tb, i);
                    Grid.SetZIndex(tb, 1);
                }

                minute += IntervalMinute;

                if(minute >= 60)
                {
                    hour++;
                    minute -= 60;
                }

                isFirst = false;
            }
        }

        private void UpdateCurrentTime()
        {
            var control = gMain.Children.OfType<CurrentTimeControl>().SingleOrDefault();
            if(control == null)
            {
                control = new CurrentTimeControl(this);
                control.PreviewMouseLeftButtonUp += gMain_MouseUp;

                gMain.Children.Add(control);
                Grid.SetColumnSpan(control, gMain.ColumnDefinitions.Count);
                Grid.SetRowSpan(control, gMain.RowDefinitions.Count);
                Grid.SetZIndex(control, int.MaxValue);
            }

            control.RefreshTime();
        }

        private void UpdateInactiveTimes()
        {
            var controls = gMain.Children.OfType<InactiveTimeControl>().ToArray();

            foreach(var control in controls)
            {
                gMain.Children.Remove(control);
            }

            var sessionSwitches = ViewModel.FindParent<TimesheetsViewModel>().GetSessionSwitches(ViewModel.Date);
            var isInactive = true;
            var startMinutes = 0.0;

            foreach(var sessionSwitch in sessionSwitches.OrderBy(x => x.CreatedAt))
            {
                var endMinutes = (sessionSwitch.CreatedAt - sessionSwitch.CreatedAt.Date).TotalMinutes;

                if(isInactive)
                {
                    if(sessionSwitch.Reason == SessionSwitchReason.SessionLogon.ToString() || sessionSwitch.Reason == SessionSwitchReason.SessionUnlock.ToString())
                    {
                        var control = new InactiveTimeControl(this, startMinutes, endMinutes);
                        gMain.Children.Add(control);
                        Grid.SetColumn(control, 3);
                        Grid.SetColumnSpan(control, 2);
                        Grid.SetRowSpan(control, gMain.RowDefinitions.Count);
                        Grid.SetZIndex(control, 0);

                        isInactive = false;
                        startMinutes = endMinutes;
                    }
                }
                else
                {
                    if(sessionSwitch.Reason == SessionSwitchReason.SessionLogoff.ToString() || sessionSwitch.Reason == SessionSwitchReason.SessionLock.ToString())
                    {
                        isInactive = true;
                        startMinutes = endMinutes;
                    }
                }
            }

            if(isInactive)
            {
                var control = new InactiveTimeControl(this, startMinutes, 24*60);
                gMain.Children.Add(control);
                Grid.SetColumn(control, 3);
                Grid.SetColumnSpan(control, 2);
                Grid.SetRowSpan(control, gMain.RowDefinitions.Count);
                Grid.SetZIndex(control, 0);
            }
        }

        //private void UpdateStartTime()
        //{
        //    var control = gMain.Children.OfType<StartTimeControl>().SingleOrDefault();
        //    if(control == null)
        //    {
        //        control = new StartTimeControl(this);
        //        gMain.Children.Add(control);
        //        Grid.SetColumn(control, 3);
        //        Grid.SetColumnSpan(control, 2);
        //        Grid.SetRowSpan(control, gMain.RowDefinitions.Count);
        //        Grid.SetZIndex(control, int.MaxValue);
        //    }

        //    control.RefreshTime();
        //}

        //private void UpdateEndTime()
        //{
        //    var control = gMain.Children.OfType<EndTimeControl>().SingleOrDefault();
        //    if(control == null)
        //    {
        //        control = new EndTimeControl(this);
        //        gMain.Children.Add(control);

        //        Grid.SetColumn(control, 3);
        //        Grid.SetColumnSpan(control, 2);
        //        Grid.SetRowSpan(control, gMain.RowDefinitions.Count);
        //        Grid.SetZIndex(control, int.MaxValue);
        //    }

        //    control.RefreshTime();
        //}

        private class TimeItemControlInfo
        {
            public TimeItemControl Control { get; private set; }
            public int RowIndex { get; private set; }
            public int RowSpan { get; private set; }

            private readonly double _maxWidth;

            public TimeItemControlInfo(TimeItemControl control, double maxWidth)
            {
                Control = control;
                RowIndex = Grid.GetRow(control);
                RowSpan = Grid.GetRowSpan(control);

                _maxWidth = maxWidth;
            }

            public void SetWidth(int colIndex, int colCount)
            {
                var colWidth = _maxWidth/colCount;

                Control.Width = colWidth;
                Control.Margin = new Thickness(colIndex * colWidth, 0, 0, 0);
            }

            public bool IsInConflict(TimeItemControlInfo otherItem)
            {
                if(RowIndex + RowSpan <= otherItem.RowIndex)
                {
                    return false;
                }

                if(RowIndex >= otherItem.RowIndex + otherItem.RowSpan)
                {
                    return false;
                }

                return true;
            }
        }
        
        private void SetWidthsOfTimeItemControls()
        {
            var maxWidth = ItemsWidth.Value;
            if(double.IsNaN(maxWidth))
            {
                return;
            }

            var controls = gMain.Children.OfType<TimeItemControl>().Select(x => new TimeItemControlInfo(x, maxWidth)).OrderBy(x => x.RowIndex).ThenByDescending(x => x.RowSpan).ToArray();

            var conflicts = new List<List<TimeItemControlInfo>>();

            for(var i = 0; i < controls.Length; i++)
            {
                var item = controls[i];
                item.Control.HorizontalAlignment = HorizontalAlignment.Left;

                var hasConflict = false;

                for(var j = 0; j < i; j++)
                {
                    var item2 = controls[j];

                    if(item.IsInConflict(item2))
                    {
                        var conflict = conflicts.SingleOrDefault(x => x.Contains(item) || x.Contains(item2));
                        if(conflict == null)
                        {
                            conflict = new List<TimeItemControlInfo>();
                            conflicts.Add(conflict);
                            conflict.Add(item);
                        }

                        conflict.Add(item2);
                        hasConflict = true;
                    }
                }

                if(!hasConflict)
                {
                    item.SetWidth(0, 1);
                }
            }

            foreach(var conflict in conflicts)
            {
                for(var i = 0; i < conflict.Count; i++)
                {
                    var item = conflict[i];

                    item.SetWidth(i, conflict.Count);
                }
            }
        }

        private bool ResizeOrMoveControl(TimeItemControl control, int row)
        {
            var rowIndex = Grid.GetRow(control);
            var rowSpan = Grid.GetRowSpan(control);

            if(control.IsResizingTop)
            {
                var rowDifference = rowIndex - row;

                if(rowDifference > 0)
                {
                    control.SetSize(row, rowSpan + rowDifference);
                    return true;
                }
                if(rowDifference < 0)
                {
                    var newRowSpan = rowSpan + rowDifference;

                    if(rowIndex < gMain.RowDefinitions.Count - 1 && newRowSpan > 0)
                    {
                        control.SetSize(row, newRowSpan);
                        return true;
                    }
                }
            }
            else if(control.IsResizingBottom)
            {
                var rowSpanDifference = row - (rowIndex + rowSpan - 1);

                if(rowSpanDifference > 0)
                {
                    var newRowSpan = rowSpan + rowSpanDifference;

                    control.SetSize(null, newRowSpan);
                    return true;
                }
                if(rowSpanDifference < 0)
                {
                    var newRowSpan = rowSpan + rowSpanDifference;
                    if(newRowSpan < 1)
                    {
                        newRowSpan = 1;
                    }

                    control.SetSize(null, newRowSpan);
                    return true;
                }
            }
            else if(control.IsMoving)
            {
                var rowDifference = row - control.OriginalMovingMouseRowIndex;

                var newRowIndex = control.OriginalMovingRowIndex + rowDifference;

                if(newRowIndex >= 0 && newRowIndex + rowSpan - 1 < gMain.RowDefinitions.Count)
                {
                    control.SetSize(newRowIndex, null);
                    return true;
                }
            }

            return false;
        }

        private int GetMouseRowIndex()
        {
            foreach(var tb in gMain.Children.OfType<RowIndexControl>())
            {
                var position = Mouse.GetPosition(tb);

                if(position.Y >= 0 && position.Y < tb.ActualHeight)
                {
                    return tb.RowIndex;
                }
            }
            return -1;
        }

        private void gMain_MouseMove(object sender, MouseEventArgs e)
        {
            var rowIndex = GetMouseRowIndex();
            if(rowIndex != -1)
            {
                var control = gMain.Children.OfType<TimeItemControl>().FirstOrDefault(x => x.IsResizingTop || x.IsResizingBottom || x.IsMoving);
                if(control != null)
                {
                    if(ResizeOrMoveControl(control, rowIndex))
                    {
                        //SetWidthsOfTimeItemControls();
                    }
                }
                else
                {
                    //selecting new item
                    if(e.LeftButton == MouseButtonState.Pressed && _originalNewPeriodRowIndex != -1)
                    {
                        int startIndex = -1, endIndex = -1;

                        foreach(var tb in gMain.Children.OfType<RowIndexControl>())
                        {
                            if(rowIndex < _originalNewPeriodRowIndex)
                            {
                                tb.IsSelected = tb.RowIndex >= rowIndex && tb.RowIndex <= _originalNewPeriodRowIndex;
                            }
                            else if(rowIndex > _originalNewPeriodRowIndex)
                            {
                                tb.IsSelected = tb.RowIndex <= rowIndex && tb.RowIndex >= _originalNewPeriodRowIndex;
                            }
                            else
                            {
                                tb.IsSelected = tb.RowIndex == _originalNewPeriodRowIndex;
                            }

                            if(tb.IsSelected)
                            {
                                if(startIndex == -1)
                                {
                                    startIndex = tb.RowIndex;
                                }
                                if(tb.RowIndex > endIndex)
                                {
                                    endIndex = tb.RowIndex;
                                }
                            }
                        }

                        ShowNewTotalTime(startIndex, endIndex);
                    }
                }
            }
        }

        private void ShowNewTotalTime(int startIndex, int endIndex)
        {
            if(startIndex != -1 && endIndex != -1)
            {
                var start = GetTime(startIndex);
                var end = GetTime(endIndex + 1);

                Grid.SetRow(_newTotalTime, startIndex);
                Grid.SetRowSpan(_newTotalTime, endIndex - startIndex + 1);
                Grid.SetColumn(_newTotalTime, 3);
                Grid.SetZIndex(_newTotalTime, int.MaxValue - 1);

                _newTotalTime.Show(start, end, VerticalAlignment.Center, Brushes.Transparent);
            }
        }

        private void CommitResizingOrMoving()
        {
            var control = gMain.Children.OfType<TimeItemControl>().FirstOrDefault(x => x.IsResizingTop || x.IsResizingBottom || x.IsMoving);
            if(control == null)
            {
                return;
            }

            control.IsResizingTop = false;
            control.IsResizingBottom = false;
            control.IsMoving = false;
        }

        private void CommitNewPeriod()
        {
            if(_newTotalTime != null)
            {
                gMain.Children.Remove(_newTotalTime);
            }

            int startIndex = -1, endIndex = -1;

            foreach(var tb in gMain.Children.OfType<RowIndexControl>())
            {
                if(tb.IsSelected)
                {
                    if(startIndex == -1)
                    {
                        startIndex = tb.RowIndex;
                    }

                    if(tb.RowIndex > endIndex)
                    {
                        endIndex = tb.RowIndex;
                    }
                }
            }

            if(startIndex != -1 && endIndex != -1)
            {
                var start = GetTime(startIndex);
                var end = GetTime(endIndex + 1);

                start = ViewModel.Date.Date.AddHours(start.Hour).AddMinutes(start.Minute);
                end = ViewModel.Date.Date.AddHours(end.Hour).AddMinutes(end.Minute);

                SetNewSupportDayItemControl(start, end, true);

                CancelNewPeriod();
            }
        }

        public void SetNewItemFromStartUntilNow()
        {
            var startTime = ViewModel.Items.Select(x => (DateTime?)x.EndTime).Max();
            if (startTime == null)
            {
                startTime = ViewModel.Parent.GetDayStartTime(ViewModel.Date);
            }

            if (startTime == null)
            {
                return;
            }

            startTime = startTime.Value.RoundDownByMinute(IntervalMinute);
            var endTime = DateTime.Now.RoundByMinute(IntervalMinute);

            if (startTime != endTime)
            {
                SetNewSupportDayItemControl(startTime.Value, endTime, true);    
            }
        }

        private void SetNewSupportDayItemControl(DateTime start, DateTime end, bool raiseTimePeriodChangedEvent)
        {
            var control = gMain.Children.OfType<TimeItemControl>().SingleOrDefault(x => x.IsNew);
            if(control != null)
            {
                gMain.Children.Remove(control);
            }

            var newControl = AddSupportDayItemControl(new Timesheet() { StartTime = start, EndTime = end });
            if(newControl != null)
            {
                if(ConfirmNewTime)
                {
                    var grid = (Grid)((ContentControl)newControl.Content).Content;

                    var bOk = new Button() { Content = "OK", Width = 70, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(1, 0, 0, 1) };
                    bOk.Click += bNewOk_Click;

                    var bCancel = new Button() { Content = "Cancel", Width = 70, VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 1, 1) };
                    bCancel.Click += bNewCancel_Click;

                    grid.Children.Add(bOk);
                    grid.Children.Add(bCancel);
                }
                else
                {
                    if(raiseTimePeriodChangedEvent)
                    {
                        control_TimePeriodChanged(newControl, new TimePeriodChangingEventArgs(newControl.Item, start, end));
                    }
                }
            }
        }

        private TimeItemControl AddSupportDayItemControl(Timesheet item)
        {
            if(!item.HasTime())
            {
                return null;
            }

            var control = new TimeItemControl(this, item);
            control.SelectedChanged += control_SelectedChanged;
            control.TimePeriodChanging += control_TimePeriodChanged;

            gMain.Children.Add(control);

            Grid.SetColumn(control, 3);
            control.ResetRowAndRowSpan();
            Grid.SetZIndex(control, 3);

            return control;
        }

        private TimeItemControl FindNewControl()
        {
            foreach(var control in gMain.Children.OfType<TimeItemControl>())
            {
                if(control.IsNew)
                {
                    return control;
                }
            }

            return null;
        }

        void bNewCancel_Click(object sender, RoutedEventArgs e)
        {
            var newControl = FindNewControl();

            CancelNewPeriod();

            if(newControl != null)
            {
                gMain.Children.Remove(newControl);
            }
        }

        void bNewOk_Click(object sender, RoutedEventArgs e)
        {
            var newControl = FindNewControl();

            if(newControl != null)
            {
                gMain.Children.Remove(newControl);

                RaiseTimePeriodChanged(newControl, new TimePeriodChangingEventArgs(newControl.Item, newControl.Item.StartTime, newControl.Item.EndTime));
            }
        }

        private void CancelNewPeriod()
        {
            _originalNewPeriodRowIndex = -1;

            foreach(var tb in gMain.Children.OfType<RowIndexControl>())
            {
                tb.IsSelected = false;
            }

            if(_newTotalTime != null)
            {
                gMain.Children.Remove(_newTotalTime);
            }
        }

        private void gMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                CommitResizingOrMoving();
                CommitNewPeriod();
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                CancelEdits();
            }
        }

        private void CancelEdits()
        {
            var control = gMain.Children.OfType<TimeItemControl>().FirstOrDefault(x => x.IsResizingTop || x.IsResizingBottom || x.IsMoving);
            if(control != null)
            {
                control.Cancel();
            }

            CancelNewPeriod();
        }

        private void gMain_MouseLeave(object sender, MouseEventArgs e)
        {
            //CommitResizingOrMoving();
            //CommitNewPeriod();
        }

        private void gMain_MouseEnter(object sender, MouseEventArgs e)
        {
            if(e.LeftButton != MouseButtonState.Pressed)
            {
                CommitResizingOrMoving();
                CommitNewPeriod();
            }
        }

        private int _originalNewPeriodRowIndex = -1;

        private void gMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                var vm = ViewModel;

                if(vm.SupportAdd)
                {
                    CancelNewPeriod();

                    _originalNewPeriodRowIndex = GetMouseRowIndex();
                    if(_originalNewPeriodRowIndex == -1)
                    {
                        return;
                    }

                    foreach(var tb in gMain.Children.OfType<RowIndexControl>())
                    {
                        if(tb.RowIndex == _originalNewPeriodRowIndex)
                        {
                            tb.IsSelected = true;
                            break;
                        }
                    }

                    if(_newTotalTime != null)
                    {
                        gMain.Children.Remove(_newTotalTime);
                    }
                    if(_newTotalTime == null)
                    {
                        _newTotalTime = new TotalTimeControl(this);
                    }

                    gMain.Children.Add(_newTotalTime);

                    ShowNewTotalTime(_originalNewPeriodRowIndex, _originalNewPeriodRowIndex);
                }
            }
        }
    }

    public class TimePeriodChangingEventArgs : CancelEventArgs
    {
        public Timesheet Item { get; private set; }
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public TimePeriodChangingEventArgs(Timesheet item, DateTime start, DateTime end)
        {
            Item = item;
            Start = start;
            End = end;
        }
    }

    public class ContextMenuCommandEventArgs : EventArgs
    {
        public string CommandName { get; private set; }
        public Timesheet Timesheet { get; private set; }

        public ContextMenuCommandEventArgs(string commandName, Timesheet timesheet)
        {
            CommandName = commandName;
            Timesheet = timesheet;
        }
    }
}