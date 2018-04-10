using System;
using System.Collections.Generic;
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


namespace Markdig.Wpf.Editor
{
    public class MarkdownEditor : Control
    {
        private const string PartUpdateButton = "PART_UpdateButton";
        private Button _updateButton;

        private const string PartTextBox = "PART_TextBox";
        private TextBox _textBox;

        private readonly int RefreshInterval;
        private DispatcherTimer RefreshTimer;
        private DispatcherTimer ProgressTimer;

        public MarkdownEditor()
        {
            RefreshTimer = CreateRenderMdTimer();
            ProgressTimer = CreateProgressUpdateTimer();
            RefreshInterval = 5;

#if DEBUG
            Text = "test";
#endif 
        }

        static MarkdownEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownEditor), new FrameworkPropertyMetadata(typeof(MarkdownEditor)));
        }

        public override void OnApplyTemplate()
        {
            _updateButton = GetTemplateElement<Button>(PartUpdateButton);
            _updateButton.Click += UpdateButton_Click;

            _textBox = GetTemplateElement<TextBox>(PartTextBox);
            _textBox.TextChanged += TextBox_TextChanged;

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                Text = textBox.Text;
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
            MdDocument = GenerateDocument(Text);
        }

        #region propdp
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);

                if (!AutoUpdate) return;

                StopTimers();
                RefreshTimer.Interval = TimeSpan.FromSeconds(RefreshInterval);
                RefreshTimer.Start();
                ProgressTimer.Start();
            }
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MarkdownEditor), CreateBinding(default(string)));

        public bool AutoUpdate
        {
            get { return (bool)GetValue(AutoUpdateProperty); }
            set { SetValue(AutoUpdateProperty, value); }
        }
        public static readonly DependencyProperty AutoUpdateProperty =
            DependencyProperty.Register("AutoUpdate", typeof(bool), typeof(MarkdownEditor), new PropertyMetadata(true));

        public double AutoUpdateInterval
        {
            get { return (double)GetValue(AutoUpdateIntervalProperty); }
            set { SetValue(AutoUpdateIntervalProperty, value); }
        }
        public static readonly DependencyProperty AutoUpdateIntervalProperty =
            DependencyProperty.Register("AutoUpdateInterval", typeof(double), typeof(MarkdownEditor), new PropertyMetadata(default(double)));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(MarkdownEditor), new PropertyMetadata(default(double)));

        public FlowDocument MdDocument
        {
            get { return (FlowDocument)GetValue(MdDocumentProperty); }
            set { SetValue(MdDocumentProperty, value); }
        }
        public static readonly DependencyProperty MdDocumentProperty =
            DependencyProperty.Register("MdDocument", typeof(FlowDocument), typeof(MarkdownEditor), new PropertyMetadata(default(FlowDocument)));

        public ICommand Update
        {
            get { return (ICommand)GetValue(UpdateProperty); }
            set { SetValue(UpdateProperty, value); }
        }
        public static readonly DependencyProperty UpdateProperty =
            DependencyProperty.Register("Update", typeof(ICommand), typeof(MarkdownEditor), new PropertyMetadata(default(ICommand)));

        public Brush EditorBackground
        {
            get { return (Brush)GetValue(EditorBackgroundProperty); }
            set { SetValue(EditorBackgroundProperty, value); }
        }
        public static readonly DependencyProperty EditorBackgroundProperty =
            DependencyProperty.Register("EditorBackground", typeof(Brush), typeof(MarkdownEditor), new PropertyMetadata(default(Brush)));

        #endregion

        /// <summary>
        /// This Timer update ProgressBar
        /// </summary>
        /// <returns></returns>
        private DispatcherTimer CreateProgressUpdateTimer()
        {
            return new DispatcherTimer(TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Normal,
                (a, b) =>
                {
                    Progress += 100.0 / (RefreshInterval * 10);
                }, Dispatcher.CurrentDispatcher);
        }

        /// <summary>
        /// This Timer will generate new Markdown document
        /// </summary>
        /// <returns></returns>
        private DispatcherTimer CreateRenderMdTimer()
        {
            return new DispatcherTimer(TimeSpan.FromMilliseconds(1000),
                DispatcherPriority.Normal,
                (a, b) =>
                {
                    MdDocument = GenerateDocument(Text);
                    StopTimers();
                }, Dispatcher.CurrentDispatcher);
        }

        private FlowDocument GenerateDocument(string a_document)
        {
            if (string.IsNullOrEmpty(a_document)) return null;

            var xaml = Markdown.ToXaml(a_document, BuildPipeline());

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xaml)))
            {
                var reader = new XamlXmlReader(stream, new EditorXamlSchemaContext());

                var document = XamlReader.Load(reader) as FlowDocument;

                return document;
            }
        }

        private void StopTimers()
        {
            RefreshTimer?.Stop();
            ProgressTimer?.Stop();
            Progress = 0;
        }
        private static MarkdownPipeline BuildPipeline()
        {
            return new MarkdownPipelineBuilder()
                .UseSupportedExtensions()
                .Build();
        }

        private T GetTemplateElement<T>(string xName) where T : UIElement => GetTemplateChild(xName) as T;

        protected static FrameworkPropertyMetadata CreateBinding(object defaultValue)
            => new FrameworkPropertyMetadata(defaultValue)
            {
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                BindsTwoWayByDefault = true
            };
    }
}
