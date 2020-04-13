using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DePeuter.Shared.Wpf.Windows;

namespace DePeuter.Shared.Wpf.Controls.Infrastructure
{
    public abstract class WindowViewModelBase : ControlWindowViewModelBase
    {
        private string _screenTitle;
        public virtual string ScreenTitle
        {
            get
            {
                if (_screenTitle != null)
                {
                    return _screenTitle;
                }

                var type = GetType();

                if(type.HasCustomAttribute<DescriptionAttribute>())
                {
                    return type.GetCustomAttribute<DescriptionAttribute>().Description;
                }

                return "Unknown ScreenTitle";
            }
            protected set
            {
                _screenTitle = value;
                OnPropertyChanged();
            }
        }
        public virtual ImageSource ScreenIcon
        {
            get { return null; }
        }

        protected WindowViewModelBase()
            : base(null)
        {
        }
        protected WindowViewModelBase(Window window)
            : this()
        {
            _window = InitializeWindow(window);
        }
        protected WindowViewModelBase(Size initialSize)
            : this()
        {
            _window = InitializeWindow(new Window() { Width = initialSize.Width, Height = initialSize.Height });
            InitializeContentUserControlView();
        }
        protected WindowViewModelBase(double width, double height)
            : this()
        {
            _window = InitializeWindow(new Window() { Width = width, Height = height });
            InitializeContentUserControlView();
        }

        public void RefreshTitle()
        {
            OnPropertyChanged("ScreenTitle");
        }

        private Window _window;
        public Window Window
        {
            get
            {
                if(_window == null)
                {
                    _window = InitializeWindow(new Window());
                }
                return _window;
            }
        }
        protected virtual bool MaximizeIsVisible { get { return true; } }
        protected virtual bool MinimizeIsVisible { get { return true; } }

        public void Show()
        {
            Show(false);
        }
        public bool? ShowDialog()
        {
            return Show(true);
        }
        private void InitializeContentUserControlView()
        {
            var uc = GetView(this);
            //uc.Loaded += userControl_Loaded;
            //uc.Padding = new Thickness(5);

            Window.DataContext = uc.DataContext;
            Window.Content = uc;
        }
        private bool? Show(bool showDialog)
        {
            //_window = new Window();
            //_window.SetBinding(Window.TitleProperty, new Binding("ScreenTitle") { Mode = BindingMode.OneWay });
            //_window.SetBinding(Window.IconProperty, new Binding("ScreenIcon") { Mode = BindingMode.OneWay });

            if (Window.Content == null)
            {
                InitializeContentUserControlView();
            }
            //if(_initialSize != null)
            //{
            //    _window.Width = _initialSize.Value.Width;
            //    _window.Height = _initialSize.Value.Height;
            //}
            //_window.Closing += Closing;
            //_window.Closed += Closed;

            if(showDialog)
            {
                return Window.ShowDialog();
            }

            Window.Show();
            return null;
        }

        private Window InitializeWindow(Window window)
        {
            window.SetBinding(Window.TitleProperty, new Binding("ScreenTitle") { Mode = BindingMode.OneWay });
            window.SetBinding(Window.IconProperty, new Binding("ScreenIcon") { Mode = BindingMode.OneWay });

            var maximizeIsVisible = MaximizeIsVisible;
            var minimizeIsVisible = MinimizeIsVisible;

            if (!maximizeIsVisible || !minimizeIsVisible)
            {
                window.SourceInitialized += (s, e) =>
                {
                    HideMinimizeAndMaximizeButtons(window, !minimizeIsVisible, !maximizeIsVisible);
                };
            }

            //if(_initialSize != null)
            //{
            //    window.Width = _initialSize.Value.Width;
            //    window.Height = _initialSize.Value.Height;
            //}

            window.Closing += Closing;
            window.Closed += Closed;

            window.Loaded += window_Loaded;

            //var uc = GetView(this);
            ////uc.Loaded += userControl_Loaded;
            ////uc.Padding = new Thickness(5);

            //window.DataContext = uc.DataContext;
            //window.Content = uc;
            IntializingWindow(window);

            return window;
        }
        
        // from winuser.h
        private const int GWL_STYLE = -16, WS_MAXIMIZEBOX = 0x10000, WS_MINIMIZEBOX = 0x20000;
        [DllImport("user32.dll")]
        extern private static int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")]
        extern private static int SetWindowLong(IntPtr hwnd, int index, int value);
        private static void HideMinimizeAndMaximizeButtons(Window window, bool hideMinimize, bool hideMaximize)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            if (hideMinimize)
            {
                currentStyle = (currentStyle & ~WS_MINIMIZEBOX);
                SetWindowLong(hwnd, GWL_STYLE, currentStyle);    
            }
            if (hideMaximize)
            {
                currentStyle = (currentStyle & ~WS_MAXIMIZEBOX);
                SetWindowLong(hwnd, GWL_STYLE, currentStyle);    
            }
            
        }

        protected virtual void IntializingWindow(Window window)
        {
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            var window = (Window)sender;
            var vm = (ViewModelBase)window.DataContext;

            try
            {
                vm.RaiseLoaded(sender, e);
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }

            window.Loaded -= window_Loaded;
        }

        //void userControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var uc = (UserControl)sender;
        //    var vm = (ViewModelBase)uc.DataContext;

        //    vm.RaiseLoaded(sender, e);

        //    uc.Loaded -= userControl_Loaded;
        //}

        private static readonly object Lock = new object();
        private static readonly Dictionary<Type, Type> ViewModelTypeMapping = new Dictionary<Type, Type>();
        private static UserControl GetView(ViewModelBase viewModel)
        {
            var viewModelType = viewModel.GetType();

            Type viewType;
            lock(Lock)
            {
                if(!ViewModelTypeMapping.ContainsKey(viewModelType))
                {
                    var viewTypeName = viewModelType.FullName.Replace("ViewModel", "View");
                    viewType = Type.GetType(viewTypeName);
                    if(viewType == null)
                    {
                        var viewTypes = viewModelType.Assembly.GetLoadedTypes().Where(x => typeof(UserControl).IsAssignableFrom(x)).ToArray();
                        foreach(var type in viewTypes)
                        {
                            if(type.FullName == viewTypeName)
                            {
                                viewType = type;
                                break;
                            }
                        }
                    }

                    if(viewType == null)
                    {
                        throw new Exception("Required type '" + viewModel.GetType().FullName.Replace("ViewModel", "View") + "' was not found");
                    }

                    ViewModelTypeMapping.Add(viewModelType, viewType);
                }

                viewType = ViewModelTypeMapping[viewModelType];
            }

            var uc = (UserControl)Activator.CreateInstance(viewType);
            uc.DataContext = viewModel;
            return uc;
        }

        public ICommand CloseCommand { get { return NewCommand(CloseCommand_Execute); } }
        protected virtual void CloseCommand_Execute(object param)
        {
            Close();
        }
        public ICommand SaveCommand { get { return NewCommand(SaveCommand_Execute); } }
        private void SaveCommand_Execute(object param)
        {
            Save(false);
        }
        public ICommand SaveAndCloseCommand { get { return NewCommand(SaveAndCloseCommand_Execute); } }
        private void SaveAndCloseCommand_Execute(object param)
        {
            Save(true);
        }

        protected virtual void Saving(CancelEventArgs e)
        {
        }
        protected virtual void Saved()
        {
        }
        private void Save(bool closeAfter)
        {
            LoadAsync(() =>
            {
                var e = new CancelEventArgs();
                Saving(e);
                return e.Cancel;
            }, cancelled =>
            {
                if(cancelled)
                {
                    return;
                }

                RefreshTitle();

                Saved();

                if(closeAfter)
                {
                    Close();
                }
            }, "Saving");
        }

        protected virtual void Closing(object sender, CancelEventArgs e)
        {
        }
        protected virtual void Closed(object sender, EventArgs e)
        {
        }
        public void Close()
        {
            _window.Close();
        }
    }
}
