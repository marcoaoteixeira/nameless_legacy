namespace Nameless.WinForms.AutoComplete {

    /// <summary>Represents a auto complete entry.</summary>
    public interface IAutoCompleteEntry {

        #region Properties

        /// <summary>
        /// Gets the matches <see cref="string" />.
        /// </summary>
        string[] Matches { get; }

        #endregion
    }
}
