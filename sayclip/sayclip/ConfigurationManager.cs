using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace sayclip
{
    public sealed class ConfigurationManager : INotifyPropertyChanged
    {
        private static readonly Lazy<ConfigurationManager> instance = new Lazy<ConfigurationManager>(() => new ConfigurationManager());
        public static ConfigurationManager getInstance
        {
            get
            {
                return instance.Value;
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool translating
        {
            get
            {
                return Properties.Settings.Default.translate;
            }
            set
            {
                Properties.Settings.Default.translate = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public bool copyResultToClipboard
        {
            get
            {
                return Properties.Settings.Default.copyresult;
            }
            set
            {
                Properties.Settings.Default.copyresult = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public bool allowCopyRepeatedText
        {
            get
            {
                return Properties.Settings.Default.allowRepeat;
            }
            set
            {
                Properties.Settings.Default.allowRepeat = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool isSayclipActive
        {
            get
            {
                return Properties.Settings.Default.active;
            }
            set
            {
                Properties.Settings.Default.active = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public double clipboardPollingSpeed
        {
            get
            {
                return Properties.Settings.Default.interval;
            }
            set
            {
                Properties.Settings.Default.interval = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public bool useCache
        {
            get
            {
                return (Properties.Settings.Default.cache);
            }
            set
            {
                Properties.Settings.Default.cache = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public string cacheFileLocation
        {
            get
            {
                return (Properties.Settings.Default.cachePath);
            }
            set
            {
                Properties.Settings.Default.cachePath = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }
        public bool useGlobalCache
        {
            get
            {
                return (Properties.Settings.Default.globalCache);
            }
            set
            {
                Properties.Settings.Default.globalCache = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
