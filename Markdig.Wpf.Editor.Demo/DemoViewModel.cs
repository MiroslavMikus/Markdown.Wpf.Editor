using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Markdig.Wpf.Editor.Demo
{
    public class DemoViewModel : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        public ICommand OnHyperlink { get; }

        public DemoViewModel()
        {
            Text = @"
*Demo text*

[Open cmd](cmd)

[Open google](https://www.google.com)";

            Markdig.Markdown
            OnHyperlink = new DelegateCommand(link =>
            {
                if (link is string input)
                {
                    MessageBox.Show(input);

                    Process.Start(input);
                }
            });
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
