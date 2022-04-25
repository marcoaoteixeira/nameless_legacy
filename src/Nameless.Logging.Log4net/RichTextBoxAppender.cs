using System;
using System.Drawing;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace Nameless.Logging.Log4net {

    /// <summary>Appends logging events to a RichTextBox</summary>
    /// <remarks>
    /// <para>
    /// RichTextBoxAppender appends log events to a specified RichTextBox control.
    /// It also allows the color, font and style of a specific type of message to be set.
    /// </para>
    /// <para>
    /// When configuring the rich text box appender, mapping should be
    /// specified to map a logging level to a text style. For example:
    /// </para>
    /// <code lang="XML" escaped="true">
    ///  <mapping>
    ///    <level value="DEBUG" />
    ///    <textColorName value="DarkGreen" />
    ///  </mapping>
    ///  <mapping>
    ///    <level value="INFO" />
    ///    <textColorName value="ControlText" />
    ///  </mapping>
    ///  <mapping>
    ///    <level value="WARN" />
    ///    <textColorName value="Blue" />
    ///  </mapping>
    ///  <mapping>
    ///    <level value="ERROR" />
    ///    <textColorName value="Red" />
    ///    <bold value="true" />
    ///    <pointSize value="10" />
    ///  </mapping>
    ///  <mapping>
    ///    <level value="FATAL" />
    ///    <textColorName value="Black" />
    ///    <backColorName value="Red" />
    ///    <bold value="true" />
    ///    <pointSize value="12" />
    ///    <fontFamilyName value="Lucida Console" />
    ///  </mapping>
    /// </code>
    /// <para>
    /// The Level is the standard log4net logging level. TextColorName and BackColorName should match
    /// a value of the System.Drawing.KnownColor enumeration. Bold and/or Italic may be specified, using
    /// <code>true</code> or <code>false</code>. FontFamilyName should match a font available on the client,
    /// but if it's not found, the control's font will be used.
    /// </para>
    /// <para>
    /// The RichTextBox property has to be set in code. The most straightforward way to accomplish
    /// this is in the Load event of the Form containing the control.
    /// <code lang="C#">
    /// private void MainForm_Load(object sender, EventArgs e)
    /// {
    ///    log4net.Appender.RichTextBoxAppender.SetRichTextBox(logRichTextBox, "MainFormRichTextAppender");
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    /// <author>Stephanie Giovannini</author>
    public class RichTextBoxAppender : AppenderSkeleton {

        #region Private Read-Only Fields

        private readonly LevelMapping _levelMapping = new LevelMapping();

        #endregion

        #region Private Delegates

        /// <summary>Delegate used to invoke UpdateControl</summary>
        /// <param name="loggingEvent">The event to log</param>
        /// <remarks>This delegate is used when UpdateControl must be
        /// called from a thread other than the thread that created the
        /// RichTextBox control.</remarks>
        private delegate void UpdateControlHandler(LoggingEvent loggingEvent);

        #endregion

        #region Private Fields

        private RichTextBox _source;
        private Form _form;
        private int _maxTextLength = 65536;

        #endregion

        #region Protected Override Properties

        /// <summary>This appender requires a layout to be set.</summary>
        /// <value><c>true</c></value>
        /// <remarks>
        /// <para>
        /// This appender requires a layout to be set.
        /// </para>
        /// </remarks>
        protected override bool RequiresLayout => true;

        #endregion

        #region Public Properties

        /// <summary>Reference to RichTextBox that displays logging events</summary>
        /// <remarks>
        /// <para>
        /// This property is a reference to the RichTextBox control
        /// that will display logging events.
        /// </para>
        /// <para>If RichTextBox is null, no logging events will be displayed.</para>
        /// <para>RichTextBox will be set to null when the control's containing
        /// Form is closed.</para>
        /// </remarks>
        public RichTextBox RichTextBoxInstance {
            set {
                if (value == _source) { return; }

                if (_form != null) {
                    _form.FormClosed -= new FormClosedEventHandler(containerForm_FormClosed);
                    _form = null;
                }

                if (value != null) {
                    value.ReadOnly = true;
                    value.HideSelection = false;
                    _form = value.FindForm();
                    _form.FormClosed += new FormClosedEventHandler(containerForm_FormClosed);
                }
                _source = value;
            }
            get => _source;
        }

        /// <summary>
        /// Maximum number of characters in control before it is cleared
        /// </summary>
        public int MaxBufferLength {
            get => _maxTextLength;
            set {
                if (value <= 0) { return; }
                _maxTextLength = value;
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>Assign a RichTextBox to a RichTextBoxAppender</summary>
        /// <param name="richTextBox">Reference to RichTextBox control that will display logging events</param>
        /// <param name="appenderName">Name of RichTextBoxAppender (case-sensitive)</param>
        /// <returns>True if a RichTextBoxAppender named <code>appenderName</code> was found</returns>
        /// <remarks>
        /// <para>This method sets the RichTextBox property of the RichTextBoxAppender
        /// in the default repository with <code>Name == appenderName</code>.</para>
        /// </remarks>
        /// <example>
        /// <code lang="C#">
        /// private void MainForm_Load(object sender, EventArgs e) {
        ///     log4net.Appender.RichTextBoxAppender.SetRichTextBox(logRichTextBox, "MainFormRichTextAppender");
        /// }
        /// </code>
        /// </example>
        public static bool SetRichTextBox(RichTextBox richTextBox, string appenderName) {
            if (string.IsNullOrWhiteSpace(appenderName)) { return false; }

            foreach (var appender in LogManager.GetRepository().GetAppenders()) {
                if (appender.Name == appenderName) {
                    if (appender is RichTextBoxAppender richTextBoxAppender) {
                        richTextBoxAppender.RichTextBoxInstance = richTextBox;
                        return true;
                    }
                    break;
                }
            }
            return false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add a mapping of level to text style - done by the config file
        /// </summary>
        /// <param name="mapping">The mapping to add</param>
        /// <remarks>
        /// <para>
        /// Add a <see cref="LevelTextStyle" /> mapping to this appender.
        /// Each mapping defines the text style for a level.
        /// </para>
        /// </remarks>
        public void AddMapping(LevelTextStyle mapping) => _levelMapping.Add(mapping);

        #endregion

        #region Public Override Methods

        /// <summary>Initialize the options for this appender</summary>
        /// <remarks>
        /// <para>
        /// Initialize the level to text style mappings set on this appender.
        /// </para>
        /// </remarks>
        public override void ActivateOptions() {
            base.ActivateOptions();

            _levelMapping.ActivateOptions();
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        /// This method is called by the <seealso cref="DoAppend(LoggingEvent)" /> method.
        /// </summary>
        /// <param name="loggingEvent">The event to log.</param>
        /// <remarks>
        /// <para>
        /// Writes the event to the RichTextBox control, if set.
        /// </para>
        /// <para>
        /// The format of the output will depend on the appender's layout.
        /// </para>
        /// <para>
        /// This method can be called from any thread.
        /// </para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent) {
            if (_source == null) { return; }

            if (_source.InvokeRequired) { _source.BeginInvoke(new UpdateControlHandler(UpdateControl), (object)loggingEvent); }
            else { UpdateControl(loggingEvent); }
        }

        /// <summary>Remove references to container form</summary>
        protected override void OnClose() {
            base.OnClose();

            if (_form == null) { return; }

            _form.FormClosed -= new FormClosedEventHandler(containerForm_FormClosed);
            _form = null;
        }

        #endregion

        #region Private Methods

        /// <summary>Add logging event to configured control</summary>
        /// <param name="loggingEvent">The event to log</param>
        private void UpdateControl(LoggingEvent loggingEvent) {
            if (_source.TextLength > _maxTextLength) {
                _source.Clear();
                _source.AppendText(string.Format("(earlier messages cleared because log length exceeded maximum of {0})\n\n", _maxTextLength));
            }

            if (_levelMapping.Lookup(loggingEvent.Level) is LevelTextStyle levelTextStyle) {
                _source.SelectionBackColor = levelTextStyle.BackColor;
                _source.SelectionColor = levelTextStyle.TextColor;

                if (levelTextStyle.Font != null) { _source.SelectionFont = levelTextStyle.Font; }
                else if (levelTextStyle.PointSize > 0.0 && _source.Font.SizeInPoints != levelTextStyle.PointSize) {
                    _source.SelectionFont = new Font(
                        familyName: _source.Font.FontFamily.Name,
                        emSize: levelTextStyle.PointSize > 0.0 ? levelTextStyle.PointSize : _source.Font.SizeInPoints,
                        style: levelTextStyle.FontStyle
                    );
                }
                else if (_source.Font.Style != levelTextStyle.FontStyle) {
                    _source.SelectionFont = new Font(
                        prototype: _source.Font,
                        newStyle: levelTextStyle.FontStyle
                    );
                }
            }

            _source.AppendText(RenderLoggingEvent(loggingEvent));
        }

        /// <summary>
        /// Remove reference to RichTextBox when container form is closed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void containerForm_FormClosed(object sender, FormClosedEventArgs e) => RichTextBoxInstance = null;

        #endregion

        #region Public Inner Classes

        /// <summary>
        /// A class to act as a mapping between the level that a logging call is made at and
        /// the text style in which it should be displayed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defines the mapping between a level and the text style in which it should be displayed..
        /// </para>
        /// </remarks>
        public class LevelTextStyle : LevelMappingEntry {

            #region Public Properties

            /// <summary>Name of a KnownColor used for text</summary>
            public string TextColorName { get; set; } = "ControlText";

            /// <summary>Name of a KnownColor used as text background</summary>
            public string BackColorName { get; set; } = "ControlLight";

            /// <summary>Name of a font family</summary>
            public string FontFamilyName { get; set; }

            /// <summary>Display level in bold style</summary>
            public bool Bold { get; set; }

            /// <summary>Display level in italic style</summary>
            public bool Italic { get; set; }

            /// <summary>Font size of level, 0 to use default</summary>
            public float PointSize { get; set; }

            #endregion

            #region Internal Properties

            internal Color TextColor { get; private set; }

            internal Color BackColor { get; private set; }

            internal FontStyle FontStyle { get; private set; }

            internal Font Font { get; private set; }

            #endregion

            #region Public Override Methods

            /// <summary>Initialize the options for the object</summary>
            /// <remarks>Parse the properties</remarks>
            public override void ActivateOptions() {
                base.ActivateOptions();

                TextColor = Color.FromName(TextColorName);
                BackColor = Color.FromName(BackColorName);
                FontStyle = Bold ? FontStyle.Bold : FontStyle;
                FontStyle = Italic ? FontStyle.Italic : FontStyle;

                if (string.IsNullOrWhiteSpace(FontFamilyName)) { return; }

                var emSize = PointSize > 0.0 ? PointSize : 8.25f;
                try { Font = new Font(FontFamilyName, emSize, FontStyle); }
                catch (Exception) { Font = null; }
            }

            #endregion
        }

        #endregion
    }
}
