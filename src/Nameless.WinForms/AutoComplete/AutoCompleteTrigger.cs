using System.Windows.Forms;

namespace Nameless.WinForms.AutoComplete {

    public abstract class AutoCompleteTrigger {

        #region Public Virtual Methods

        /// <summary>Triggers when text changed.</summary>
        /// <param name="text">The text.</param>
        /// <returns>An instance of <see cref="T:Nameless.Framework.WinForms.AutoComplete.TriggerState" />.</returns>
        public virtual TriggerState OnTextChanged(string text) => TriggerState.None;

        /// <summary>Triggers when receives a specific key command.</summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>An instance of <see cref="T:Nameless.Framework.WinForms.AutoComplete.TriggerState" />.</returns>
        public virtual TriggerState OnCommandKey(Keys keyData) => TriggerState.None;

        #endregion
    }
}
