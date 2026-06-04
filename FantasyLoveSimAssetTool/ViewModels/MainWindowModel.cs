using FantasyLoveSimAssetTool.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyLoveSimAssetTool.ViewModels
{
    class MainWindowModel : ObservableObject
    {
        private string text;

        public string Text
        {
            get { return this.text; }
            set
            {
                if (this.text == value) { return; }
                this.text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public MainWindowModel()
        {
            Text = "Hello,world!";
        }
    }
}
