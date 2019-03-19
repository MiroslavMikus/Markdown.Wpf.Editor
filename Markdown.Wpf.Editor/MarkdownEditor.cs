using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xaml;
using Markdig;
using Markdig.Wpf;
using XamlReader = System.Windows.Markup.XamlReader;


namespace Markdown.Wpf.Editor
{
    public class MarkdownEditor : Control
    {
        #region Template parts
        private const string PartUpdateButton = "PART_UpdateButton";
        private Button _updateButton;

        private const string PartTextBox = "PART_TextBox";
        private TextBox _textBox;

        private const string PinButton = "PART_PinButton";
        private Button _pinButton;

        private const string AutoUpdateButton = "PART_AutoUpdateButton";
        private Button _autoUpdateButton;

        private const string MarkDownControl = "PART_MarkdownViewer";
        private MarkdownViewer _markDownControl;
        #endregion

        private DispatcherTimer _progressTimer;
        private bool _instantLoad = true;

        public MarkdownEditor()
        {
            GenerateDocument(Text);
        }

        static MarkdownEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownEditor), new FrameworkPropertyMetadata(typeof(MarkdownEditor)));
        }

        public override void OnApplyTemplate()
        {
            _markDownControl = GetTemplateElement<MarkdownViewer>(MarkDownControl);
            _markDownControl.Pipeline = BuildPipeline();
            _markDownControl.CommandBindings.Add(new CommandBinding(Commands.Hyperlink, ExecuteHyperlink));

            _updateButton = GetTemplateElement<Button>(PartUpdateButton);
            _updateButton.Click += UpdateButton_Click;

            _textBox = GetTemplateElement<TextBox>(PartTextBox);
            GenerateDocument(Text);
            _textBox.TextChanged += TextBox_TextChanged;

            _pinButton = GetTemplateElement<Button>(PinButton);
            _pinButton.Click += PinButton_Click;

            _autoUpdateButton = GetTemplateElement<Button>(AutoUpdateButton);
            _autoUpdateButton.Click += AutoUpdateButton_Click;
        }

        #region Events
        private void ExecuteHyperlink(object sender, ExecutedRoutedEventArgs e)
        {
            if (Hyperlink != null)
            {
                Hyperlink.Execute(e.Parameter.ToString());
            }
            else
            {
                Process.Start(e.Parameter.ToString());
            }
        }

        private void AutoUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // switch 'auto-update' state
            AutoUpdate = !AutoUpdate;
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            // switch 'enabled' state
            IsEnabled = !IsEnabled;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // On first load the MD document shold be created instantly
                if (_instantLoad)
                {
                    GenerateDocument(Text);

                    _instantLoad = false;

                    return;
                }

                Text = textBox.Text;

                if (!AutoUpdate) return;

                if (AutoUpdateInterval < 500)
                {
                    GenerateDocument(Text);

                    return;
                }

                StopTimers();
                _progressTimer = CreateProgressTimer();
                _progressTimer.Start();
            }
        }

        /// <summary>
        /// Stops all timers & updates MdDocument
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            StopTimers();
            GenerateDocument(Text);
        }
        #endregion

        #region propdp
        /// <summary>
        /// Raw text. 
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarkdownEditor), CreateBinding(default(string)));

        /// <summary>
        /// Specifies if markdown should be updated automatically in specified <see cref="AutoUpdateInterval"/>. 
        /// Default is true.
        /// </summary>
        public bool AutoUpdate
        {
            get { return (bool)GetValue(AutoUpdateProperty); }
            set { SetValue(AutoUpdateProperty, value); }
        }
        public static readonly DependencyProperty AutoUpdateProperty =
            DependencyProperty.Register("AutoUpdate", typeof(bool), typeof(MarkdownEditor), new PropertyMetadata(true));

        /// <summary>
        /// Interval in ms for updating markdown view. Applies only if <see cref="AutoUpdate"/> is set to true and also the interval is more than 500.
        /// Default is 5000 ms.
        /// </summary>
        public double AutoUpdateInterval
        {
            get { return (double)GetValue(AutoUpdateIntervalProperty); }
            set { SetValue(AutoUpdateIntervalProperty, value); }
        }
        public static readonly DependencyProperty AutoUpdateIntervalProperty =
            DependencyProperty.Register("AutoUpdateInterval", typeof(double), typeof(MarkdownEditor), new PropertyMetadata(5000d));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(MarkdownEditor), new PropertyMetadata(default(double)));

        public ICommand Update
        {
            get { return (ICommand)GetValue(UpdateProperty); }
            set { SetValue(UpdateProperty, value); }
        }
        public static readonly DependencyProperty UpdateProperty =
            DependencyProperty.Register("Update", typeof(ICommand), typeof(MarkdownEditor), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Run action on click on a Hyperlink.
        /// Link will come as string from command-parameter element
        /// </summary>
        public ICommand Hyperlink
        {
            get { return (ICommand)GetValue(HyperlinkProperty); }
            set { SetValue(HyperlinkProperty, value); }
        }
        public static readonly DependencyProperty HyperlinkProperty =
            DependencyProperty.Register("Hyperlink", typeof(ICommand), typeof(MarkdownEditor), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Background property for input field. Markdown view background can be set with background property.
        /// </summary>
        public Brush EditorBackground
        {
            get { return (Brush)GetValue(EditorBackgroundProperty); }
            set { SetValue(EditorBackgroundProperty, value); }
        }
        public static readonly DependencyProperty EditorBackgroundProperty =
            DependencyProperty.Register("EditorBackground", typeof(Brush), typeof(MarkdownEditor), new PropertyMetadata(default(Brush)));

        public new bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }
        public new static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(MarkdownEditor), new PropertyMetadata(true));

        #endregion

        /// <summary>
        /// This Timer update ProgressBar
        /// </summary>
        /// <returns></returns>
        private DispatcherTimer CreateProgressTimer()
        {
            // calculate 0,1 % of update interval
            var updateInterval = AutoUpdateInterval / 1000;

            var updateAt = DateTime.Now.AddMilliseconds(AutoUpdateInterval);

            return new DispatcherTimer(TimeSpan.FromMilliseconds(updateInterval), DispatcherPriority.Normal,
                (a, b) =>
                {
                    var diff = (updateAt - DateTime.Now).TotalMilliseconds;

                    // (updateInterval * 10) = 1 %
                    Progress = 100 - (diff / (updateInterval * 10));

                    if (Progress >= 100)
                    {
                        StopTimers();
                        GenerateDocument(Text);
                    }

                }, Dispatcher.CurrentDispatcher);
        }

        private void StopTimers()
        {
            // stop timer
            _progressTimer?.Stop();

            // reset progress
            Progress = 0;
        }

        private void GenerateDocument(string a_document)
        {
            if (string.IsNullOrEmpty(a_document)) return;

            _markDownControl.Markdown = a_document;
        }

        private static MarkdownPipeline BuildPipeline()
        {
            return new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .Build();
        }

        private T GetTemplateElement<T>(string xName) where T : UIElement => GetTemplateChild(xName) as T;

        private static FrameworkPropertyMetadata CreateBinding(object defaultValue)
            => new FrameworkPropertyMetadata(defaultValue)
            {
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                BindsTwoWayByDefault = true
            };
    }
}
