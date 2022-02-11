using System;
using System.Collections;
using System.ComponentModel.Design;

namespace Nameless.WinForms.AutoComplete {

    /// <summary>Represents a collection of auto complete entries.</summary>
    public class AutoCompleteEntryCollection : CollectionBase {

        #region Public Properties

        /// <summary>Gets an entry by its index.</summary>
        /// <param name="index">The entry index.</param>
        /// <returns>An auto complete entry.</returns>
        public IAutoCompleteEntry this[int index] => InnerList[index] as AutoCompleteEntry;

        #endregion

        #region Public Methods

        /// <summary>Adds a new auto complete entry to the collection.</summary>
        /// <param name="entry">An instance of <see cref="IAutoCompleteEntry" />.</param>
        public void Add(IAutoCompleteEntry entry) => InnerList.Add(entry);

        /// <summary>
        /// Adds a collection of auto complete entry to the collection.
        /// </summary>
        /// <param name="collection">A collection of <see cref="IAutoCompleteEntry" />.</param>
        public void AddRange(ICollection collection) => InnerList.AddRange(collection);

        /// <summary>
        /// Retrieves an array of <see cref="object" /> representing all auto complete entries.
        /// </summary>
        /// <returns>An array of <see cref="object" />.</returns>
        public object[] ToObjectArray() => InnerList.ToArray();

        #endregion

        #region Public Inner Class

        /// <summary>
        /// Auto complete entry implementation of <see cref="T:System.ComponentModel.Design.CollectionEditor" />.
        /// </summary>
        public class AutoCompleteEntryCollectionEditor : CollectionEditor {

            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of <see cref="AutoCompleteEntryCollection" />.
            /// </summary>
            /// <param name="type">The type of the collection for this editor to edit.</param>
            public AutoCompleteEntryCollectionEditor(Type type) : base(type) { }

            #endregion

            #region Protected Override Methods

            /// <inheritdoc />
            protected override bool CanSelectMultipleInstances() => false;

            /// <inheritdoc />
            protected override Type CreateCollectionItemType() => typeof(AutoCompleteEntry);

            #endregion
        }

        #endregion
    }
}
