﻿// Copyright (c) 2010 Ravi Bhavnani
// License: Code Project Open License
// http://www.codeproject.com/info/cpol10.aspx

using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RavSoft.GoogleTranslator
{
    /// <summary>
    /// A sample application to demonstrate the <see cref="TranslatorOld"/> class.
    /// </summary>
    public partial class GoogleTranslatorFrm : Form
    {
        #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="GoogleTranslatorFrm"/> class.
            /// </summary>
            public GoogleTranslatorFrm()
            {
                InitializeComponent();
            }

        #endregion

        #region Form event handlers

            /// <summary>
            /// Handles the Load event of the GoogleTranslatorFrm control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void GoogleTranslatorFrm_Load
                (object sender,
                 EventArgs e)
            {
                this._comboFrom.Items.AddRange (Translator.Languages.ToArray());
                this._comboTo.Items.AddRange (Translator.Languages.ToArray());
                this._comboFrom.SelectedItem = "English";
                this._comboTo.SelectedItem = "French";
            }

        #endregion

        #region Button handlers

            /// <summary>
            /// Handles the LinkClicked event of the _lnkSourceEnglish control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
            private void _lnkSourceEnglish_LinkClicked
                (object sender,
                 LinkLabelLinkClickedEventArgs e)
            {
                this._comboFrom.SelectedItem = "English";
            }

            /// <summary>
            /// Handles the LinkClicked event of the _lnkTargetEnglish control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
            private void _lnkTargetEnglish_LinkClicked
                (object sender,
                 LinkLabelLinkClickedEventArgs e)
            {
                this._comboTo.SelectedItem = "English";
            }

            /// <summary>
            /// Handles the Click event of the _btnTranslate control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void _btnTranslate_Click
                (object sender,
                 EventArgs e)
            {
                // Initialize the translator
                Translator t = new Translator();

                this._editTarget.Text = string.Empty;
                this._editTarget.Update();
                this._translationSpeakUrl = null;

                // Translate the text
                try {
                    this.Cursor = Cursors.WaitCursor;
                    this._lblStatus.Text = "Translating...";
                    this._lblStatus.Update();
                    this._editTarget.Text = t.Translate (this._editSourceText.Text.Trim(), (string) this._comboFrom.SelectedItem, (string) this._comboTo.SelectedItem);
                    if (t.Error == null) {
                        this._editTarget.Update();
                        this._translationSpeakUrl = t.TranslationSpeechUrl;
                    }
                    else {
                        MessageBox.Show (t.Error.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    
                }
                catch (Exception ex) {
                    MessageBox.Show (ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                finally {
                    this._lblStatus.Text = string.Format ("Translated in {0} mSec", (int) t.TranslationTime.TotalMilliseconds);
                    this.Cursor = Cursors.Default;
                }
            }

            /// <summary>
            /// Handles the Click event of the _btnSpeak control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            private void _btnSpeak_Click
                (object sender,
                 EventArgs e)
            {
                if (!string.IsNullOrEmpty (this._translationSpeakUrl)) {
                    this._webBrowserCtrl.Navigate (this._translationSpeakUrl);
                }
            }

            /// <summary>
            /// Handles the LinkClicked event of the _lnkReverse control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Windows.Forms.LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
            private void _lnkReverse_LinkClicked
                (object sender,
                 LinkLabelLinkClickedEventArgs e)
            {
                // Swap translation mode
                string from = (string) this._comboFrom.SelectedItem;
                string to = (string) this._comboTo.SelectedItem;
                this._comboFrom.SelectedItem = to;
                this._comboTo.SelectedItem = from;

                // Reset text
                this._editSourceText.Text = this._editTarget.Text;
                this._editTarget.Text = string.Empty;
                this.Update();
                this._translationSpeakUrl = string.Empty;
            }

        #endregion

        #region Fields

            /// <summary>
            /// The URL used to speak the translation.
            /// </summary>
            private string _translationSpeakUrl;

        #endregion
    }
}
