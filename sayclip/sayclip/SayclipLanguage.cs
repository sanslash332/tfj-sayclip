using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sayclip
{
    public class SayclipLanguage
    {
        private string _langCode;
        public string langCode
        {
            get
            {
                return(this._langCode);
            }
        }
        private string _displayName;
        public String displayName
        {
            get
            {
                return(this._displayName);
            }
        }
        private bool _isForSource;
        public bool isForSource
        {
            get
            {
                return(this._isForSource);
            }
        }
        private bool _isForTarget;
        public bool isForTarget
        {
            get
            {
                return(this._isForTarget);
            }
        }

        /// <summary>
        /// a class that represent a language for the sayclip application.
        /// </summary>
        /// <param name="code"> Internal code of the language for the plugin.</param>
        /// <param name="name">The display name of the lang to show in the UI </param>
        /// <param name="source">If the language can be shown in the sources menu </param>
        /// <param name="target">if the language can be shown in the targets menu</param>
        public SayclipLanguage(string code, string name, bool source=true, bool target=true)
        {
            this._langCode = code;
            this._displayName = name;
            this._isForSource = source;
            this._isForTarget = target;

        }
    }
}
