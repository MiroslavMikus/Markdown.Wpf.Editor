using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        public DemoViewModel()
        {
            Text = "Demo text";
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
