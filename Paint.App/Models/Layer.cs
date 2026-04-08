using Paint.Core;

using System.ComponentModel;


namespace Paint.App.Models
{
    public class Layer : INotifyPropertyChanged
    {
        private string _name;
        private bool _isVisible = true;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

       
        public List<IShape> Shapes { get; set; } = new List<IShape>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
