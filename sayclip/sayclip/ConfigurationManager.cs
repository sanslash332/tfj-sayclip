using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
    public sealed class ConfigurationManager
    {
        private static readonly Lazy<ConfigurationManager> instance = new Lazy<ConfigurationManager>(() => new ConfigurationManager());
        public static ConfigurationManager getInstance
        {
            get
            {
                return instance.Value;
            }
        }
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
            }
        }



    }
}
