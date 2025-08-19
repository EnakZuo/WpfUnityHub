using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnityHub.Models
{
    public class UnityProject : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _path = string.Empty;
        private string _unityVersion = string.Empty;
        private DateTime _lastModified;
        private string _selectedEngineVersion = string.Empty;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public string UnityVersion
        {
            get => _unityVersion;
            set
            {
                _unityVersion = value;
                OnPropertyChanged(nameof(UnityVersion));
            }
        }

        public DateTime LastModified
        {
            get => _lastModified;
            set
            {
                _lastModified = value;
                OnPropertyChanged(nameof(LastModified));
            }
        }

        public string SelectedEngineVersion
        {
            get => _selectedEngineVersion;
            set
            {
                _selectedEngineVersion = value;
                OnPropertyChanged(nameof(SelectedEngineVersion));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
