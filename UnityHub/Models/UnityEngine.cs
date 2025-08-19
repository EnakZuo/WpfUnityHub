using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnityHub.Models
{
    public class UnityEngine : INotifyPropertyChanged
    {
        private string _version = string.Empty;
        private string _path = string.Empty;
        private string _platformsLabel = string.Empty;

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged(nameof(Version));
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

        public string PlatformsLabel
        {
            get => _platformsLabel;
            set
            {
                _platformsLabel = value;
                OnPropertyChanged(nameof(PlatformsLabel));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
