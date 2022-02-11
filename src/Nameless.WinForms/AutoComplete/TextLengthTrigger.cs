using System;

namespace Nameless.WinForms.AutoComplete {

    /// <summary>
    /// Text length trigger implementation of <see cref="AutoCompleteTrigger" />.
    /// </summary>
    public class TextLengthTrigger : AutoCompleteTrigger {

        #region Public Properties

        /// <summary>Gets or sets the text length.</summary>
        public int TextLength { get; set; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TextLengthTrigger" />.
        /// </summary>
        /// <param name="textLength">The text length. Default is 2.</param>
        public TextLengthTrigger(int textLength = 2) => TextLength = textLength;

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public override TriggerState OnTextChanged(string text) {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }

            if (text.Length >= TextLength) { return TriggerState.Show; }

            return text.Length < TextLength ? TriggerState.Hide : TriggerState.None;
        }

        #endregion
    }
}
