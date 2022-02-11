using System;

namespace Nameless.WinForms.AutoComplete {

    /// <summary>Represents an auto complete entry.</summary>
    public class AutoCompleteEntry : IAutoCompleteEntry {

        #region Public Properties

        /// <summary>Gets or sets the display for the entry.</summary>
        public string Display { get; set; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AutoCompleteEntry" />.
        /// </summary>
        /// <param name="display">The entry display.</param>
        /// <param name="matches">The entry matches.</param>
        public AutoCompleteEntry(string display, string[] matches) {
            Display = display ?? throw new ArgumentNullException(nameof(display));
            Matches = matches != null && matches.Length > 0 ? matches : new[] { display };
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override string ToString() => this.Display;

        #endregion

        #region IAutoCompleteEntry Members

        /// <inheritdoc />
        public string[] Matches { get; }

        #endregion
    }
}
