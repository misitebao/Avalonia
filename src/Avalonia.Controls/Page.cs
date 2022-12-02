using Avalonia.Controls.Metadata;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using static Avalonia.Controls.Platform.IInsetsManager;

namespace Avalonia.Controls
{
    [TemplatePart("PART_SafeAreaBorder", typeof(Border))]
    public class Page : ContentControl
    {
        public static readonly StyledProperty<SystemBarTheme?> SystemBarThemeProperty =
            AvaloniaProperty.Register<Page, SystemBarTheme?>(nameof(SystemBarTheme));

        public static readonly StyledProperty<bool> UseSafeAreaProperty = 
            AvaloniaProperty.Register<Page, bool>(nameof(UseSafeArea));

        public static readonly StyledProperty<string?> TitleProperty = 
            AvaloniaProperty.Register<Page, string?>(nameof(Title));

        public static readonly StyledProperty<WindowState> WindowStateProperty =
            AvaloniaProperty.Register<Window, WindowState>(nameof(WindowState));

        private IInsetsManager? _insetsManager;
        private Border? _safeAreaBorder;
        private bool _isTitleSet;
        private IPageHost? _host;

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool UseSafeArea
        {
            get => GetValue(UseSafeAreaProperty);
            set => SetValue(UseSafeAreaProperty, value);
        }

        public WindowState WindowState
        {
            get { return GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        public SystemBarTheme? SystemBarTheme
        {
            get => GetValue(SystemBarThemeProperty);
            set => SetValue(SystemBarThemeProperty, value);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            if(_insetsManager != null)
            {
                _insetsManager.SafeAreaChanged -= InsetsManager_SafeAreaChanged;
            }

            _insetsManager = null;

            var host = this.FindAncestorOfType<IPageHost>();

            if (host != null)
            {
                _insetsManager = host.InsetsManager;

                if (_insetsManager != null)
                {
                    _insetsManager.SafeAreaChanged += InsetsManager_SafeAreaChanged;

                    _insetsManager.DisplayEdgeToEdge = WindowState == WindowState.Maximized;
                    _insetsManager.IsSystemBarVisible = WindowState != WindowState.FullScreen;
                    _insetsManager.SystemBarTheme = SystemBarTheme;

                    if (UseSafeArea && _safeAreaBorder != null)
                    {
                        _safeAreaBorder.Padding = _insetsManager.GetSafeAreaPadding();
                    }
                }
            }

            if(host is Window window)
            {
                _isTitleSet = window.IsSet(Window.TitleProperty);

                if(!_isTitleSet && IsSet(TitleProperty))
                {
                    window.Title = Title;
                }
            }

            _host = host;
        }

        private void InsetsManager_SafeAreaChanged(object? sender, SafeAreaChangedArgs e)
        {
            if (e != null && UseSafeArea && _safeAreaBorder != null)
            {
                _safeAreaBorder.Padding = e.SafeAreaPadding;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _safeAreaBorder = e.NameScope.Get<Border>("PART_SafeAreaBorder");

            if(UseSafeArea && _safeAreaBorder != null && _insetsManager != null)
            {
                _safeAreaBorder.Padding = _insetsManager.GetSafeAreaPadding();
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if(change.Property == UseSafeAreaProperty && _safeAreaBorder != null && _insetsManager != null)
            {
                _safeAreaBorder.Padding = UseSafeArea ? _insetsManager.GetSafeAreaPadding() : default;
            }
            else if(change.Property == WindowStateProperty)
            {
                if(_host is Window window)
                {
                    window.WindowState = WindowState;
                }
                else if(_insetsManager != null)
                {
                    _insetsManager.DisplayEdgeToEdge = WindowState == WindowState.Maximized;
                    _insetsManager.IsSystemBarVisible = WindowState != WindowState.FullScreen;
                }
            }
            else if(change.Property == TitleProperty && _host is Window window && !_isTitleSet)
            {
                window.Title = Title;
            }
            else if(change.Property == SystemBarThemeProperty && _insetsManager != null)
            {
                _insetsManager.SystemBarTheme = SystemBarTheme;
            }
        }
    }
}
