using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Nameless.WinForms.AutoComplete {

    /// <summary>
    /// A collection of <see cref="AutoCompleteTrigger" />.
    /// </summary>
    public class AutoCompleteTriggerCollection : CollectionBase {

        #region Public Properties

        /// <summary>Gets the trigger associated to the index.</summary>
        /// <param name="index">The index.</param>
        /// <returns>An instance of <see cref="AutoCompleteTrigger" />.</returns>
        public AutoCompleteTrigger this[int index] => InnerList[index] as AutoCompleteTrigger;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a new <see cref="AutoCompleteTrigger" /> to the collection.
        /// </summary>
        /// <param name="item">The <see cref="AutoCompleteTrigger" /> instance.</param>
        public void Add(AutoCompleteTrigger item) => InnerList.Add(item);

        /// <summary>
        /// Removes a <see cref="AutoCompleteTrigger" /> from the collection.
        /// </summary>
        /// <param name="item">The <see cref="AutoCompleteTrigger" /> instance.</param>
        public void Remove(AutoCompleteTrigger item) => InnerList.Remove(item);

        #endregion

        #region Public Virtual Methods

        /// <summary>Triggers when text changed.</summary>
        /// <param name="text">The text.</param>
        /// <returns>An instance of <see cref="TriggerState" />.</returns>
        public virtual TriggerState OnTextChanged(string text) {
            foreach (AutoCompleteTrigger inner in InnerList) {
                TriggerState triggerState = inner.OnTextChanged(text);
                if (triggerState != TriggerState.None) {
                    return triggerState;
                }
            }
            return TriggerState.None;
        }

        /// <summary>Triggers when receives a specific key command.</summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>An instance of <see cref="TriggerState" />.</returns>
        public virtual TriggerState OnCommandKey(Keys keyData) {
            foreach (AutoCompleteTrigger inner in InnerList) {
                TriggerState triggerState = inner.OnCommandKey(keyData);
                if (triggerState != TriggerState.None) {
                    return triggerState;
                }
            }
            return TriggerState.None;
        }

        #endregion

        #region Public Inner Classes

        /// <summary>
        /// Auto complete trigger implementation of <see cref="CollectionEditor" />.
        /// </summary>
        public class AutoCompleteTriggerCollectionEditor : CollectionEditor {

            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of <see cref="T:Nameless.Framework.WinForms.AutoComplete.AutoCompleteTriggerCollection.AutoCompleteTriggerCollectionEditor" />
            /// </summary>
            /// <param name="type">The type of the collection for this editor to edit.</param>
            public AutoCompleteTriggerCollectionEditor(Type type)
              : base(type) {
            }

            #endregion

            #region Public Override Methods

            /// <inheritdoc />
            protected override bool CanSelectMultipleInstances() => false;

            /// <inheritdoc />
            protected override Type[] CreateNewItemTypes() => new[] {
                typeof (ShortCutTrigger),
                typeof (TextLengthTrigger)
            };

            /// <inheritdoc />
            protected override Type CreateCollectionItemType() => typeof(ShortCutTrigger);

            #endregion
        }

        #endregion
    }
}
