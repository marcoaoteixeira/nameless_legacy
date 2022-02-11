using System.Windows.Forms;

namespace Nameless.WinForms.AutoComplete {

    /// <summary>
    /// Short cut trigger implementation of <see cref="AutoCompleteTrigger" />.
    /// </summary>
    public class ShortCutTrigger : AutoCompleteTrigger {

        #region Public Properties

        /// <summary>Gets or sets the short cut key.</summary>
        public Keys ShortCut { get; set; }

        /// <summary>Gets or sets the trigger state.</summary>
        public TriggerState State { get; set; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShortCutTrigger" />.
        /// </summary>
        /// <param name="shortCut">The short cut key.</param>
        /// <param name="state">The trigger state.</param>
        public ShortCutTrigger(Keys shortCut = Keys.None, TriggerState state = TriggerState.None) {
            ShortCut = shortCut;
            State = state;
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override TriggerState OnCommandKey(Keys keyData) => keyData != ShortCut ? TriggerState.None : State;

        #endregion
    }
}
