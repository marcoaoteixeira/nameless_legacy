using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using Nameless.WinForms.AutoComplete.Win32;

namespace Nameless.WinForms.AutoComplete {

    /// <summary>
    /// http://www.codeproject.com/Articles/16942/AutoComplete-TextBox
    /// </summary>
    public class AutoCompleteTextBox : TextBox {

        #region Private Fields

        private ListBox _listBox;
        private WinHook _winHook;
        /// <summary>Required designer variable.</summary>
        private IContainer _components;

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the popup <see cref="Form" />.
        /// </summary>
        protected Form PopupForm { get; set; }

        #endregion

        #region Public Properties

        /// <summary>Gets or sets the entry mode.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public EntryMode Mode { get; set; }

        /// <summary>Gets or sets the collection of auto complete entries.</summary>
        [Editor(typeof(AutoCompleteEntryCollection.AutoCompleteEntryCollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public AutoCompleteEntryCollection Items { get; set; }

        /// <summary>Gets or sets the auto complete trigger collection.</summary>
        [Editor(typeof(AutoCompleteTriggerCollection.AutoCompleteTriggerCollectionEditor), typeof(UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public AutoCompleteTriggerCollection Triggers { get; set; }

        /// <summary>Gets or sets the popup width.</summary>
        [Browsable(true)]
        [Description("The width of the popup (-1 will auto-size the popup to the width of the textbox).")]
        public int PopupWidth {
            get => PopupForm.Width;
            set => PopupForm.Width = value == -1 ? Width : value;
        }

        /// <summary>Gets or sets the popup border style.</summary>
        public BorderStyle PopupBorderStyle {
            get => _listBox.BorderStyle;
            set => _listBox.BorderStyle = value;
        }

        /// <summary>Gets or sets the popup offset.</summary>
        [Description("The popup defaults to the lower left edge of the textbox.")]
        public Point PopupOffset { get; set; }

        /// <summary>Gets or sets the popup selection back color.</summary>
        public Color PopupSelectionBackColor { get; set; }

        /// <summary>Gets or sets the popup selection fore color.</summary>
        public Color PopupSelectionForeColor { get; set; }

        #endregion

        #region Public Override Properties

        /// <summary>Gets or sets the text.</summary>
        [Browsable(true)]
        public override string Text {
            get => base.Text;
            set {
                TriggersEnabled = false;
                base.Text = value;
                TriggersEnabled = true;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>Gets or sets if the triggers are enabled.</summary>
        protected bool TriggersEnabled { get; set; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AutoCompleteTextBox" />.
        /// </summary>
        public AutoCompleteTextBox() {
            InitializeComponent();
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AutoCompleteTextBox" />
        /// </summary>
        /// <param name="container">The auto complete text box container.</param>
        public AutoCompleteTextBox(IContainer container) {
            container.Add(this);
            InitializeComponent();
            Initialize();
        }

        #endregion

        #region Protected Virtual Methods

        /// <summary>Called on the default command key press.</summary>
        /// <param name="msg">The message.</param>
        /// <param name="keyData">The key data.</param>
        /// <returns></returns>
        protected virtual bool DefaultCmdKey(ref Message msg, Keys keyData) {
            bool flag = base.ProcessCmdKey(ref msg, keyData);
            if (TriggersEnabled) {
                switch (Triggers.OnCommandKey(keyData)) {
                    case TriggerState.Show:
                        ShowList();
                        break;
                    case TriggerState.ShowAndConsume:
                        flag = true;
                        ShowList();
                        break;
                    case TriggerState.Hide:
                        HideList();
                        break;
                    case TriggerState.HideAndConsume:
                        flag = true;
                        HideList();
                        break;
                    case TriggerState.Select:
                        if (PopupForm.Visible) {
                            SelectCurrentItem();
                            break;
                        }
                        break;
                    case TriggerState.SelectAndConsume:
                        if (PopupForm.Visible) {
                            flag = true;
                            SelectCurrentItem();
                            break;
                        }
                        break;
                }
            }
            return flag;
        }

        /// <summary>Selects the current item.</summary>
        protected virtual void SelectCurrentItem() {
            if (_listBox.SelectedIndex == -1) { return; }
            Focus();
            Text = _listBox.SelectedItem.ToString();
            if (Text.Length > 0) { SelectionStart = Text.Length; }
            HideList();
        }

        /// <summary>Shows the auto complete list.</summary>
        protected virtual void ShowList() {
            if (!PopupForm.Visible) {
                _listBox.SelectedIndex = -1;
                UpdateList();
                var screen = PointToScreen(new Point(0, 0));
                screen.X += PopupOffset.X;
                screen.Y += Height + PopupOffset.Y;
                PopupForm.Location = screen;
                if (_listBox.Items.Count <= 0) { return; }
                PopupForm.Show();
                if (_winHook == null) {
                    _winHook = new WinHook(this);
                    _winHook.AssignHandle(FindForm().Handle);
                }
                Focus();
            } else { UpdateList(); }
        }

        /// <summary>Hides the auto complete list.</summary>
        protected virtual void HideList() {
            Mode = EntryMode.Text;
            if (_winHook != null) { _winHook.ReleaseHandle(); }
            _winHook = null;
            PopupForm.Hide();
        }

        /// <summary>Updates the auto complete list.</summary>
        protected virtual void UpdateList() {
            object selectedItem = _listBox.SelectedItem;
            _listBox.Items.Clear();
            _listBox.Items.AddRange(FilterList(Items).ToObjectArray());
            if (selectedItem != null && _listBox.Items.Contains(selectedItem)) {
                EntryMode mode = Mode;
                Mode = EntryMode.List;
                _listBox.SelectedItem = selectedItem;
                Mode = mode;
            }
            if (_listBox.Items.Count != 0) {
                var num = _listBox.Items.Count;
                if (num > 8) { num = 8; }
                PopupForm.Height = num * _listBox.ItemHeight + 2;
                switch (BorderStyle) {
                    case BorderStyle.FixedSingle:
                        PopupForm.Height += 2;
                        break;
                    case BorderStyle.Fixed3D:
                        PopupForm.Height += 4;
                        break;
                }
                PopupForm.Width = PopupWidth;
                if (_listBox.Items.Count <= 0 || _listBox.SelectedIndex != -1) { return; }
                EntryMode mode = Mode;
                Mode = EntryMode.List;
                _listBox.SelectedIndex = 0;
                Mode = mode;
            } else { HideList(); }
        }

        /// <summary>Filters the auto complete entry collection.</summary>
        /// <param name="collection">The entry collection.</param>
        /// <returns>An instance of <see cref="AutoCompleteEntryCollection" /> filtered by the auto complete matches.</returns>
        protected virtual AutoCompleteEntryCollection FilterList(AutoCompleteEntryCollection collection) {
            var completeEntryCollection = new AutoCompleteEntryCollection();
            completeEntryCollection.AddRange(collection.OfType<IAutoCompleteEntry>()
                .Where(item => item.Matches
                    .Any(match => match
                        .ToUpper()
                        .StartsWith(Text.ToUpper())))
                .ToArray());
            return completeEntryCollection;
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc />
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            switch (keyData) {
                case Keys.Up:
                    Mode = EntryMode.List;
                    if (_listBox.SelectedIndex > 0) { --_listBox.SelectedIndex; }
                    return true;
                case Keys.Down:
                    Mode = EntryMode.List;
                    if (_listBox.SelectedIndex < _listBox.Items.Count - 1) { ++_listBox.SelectedIndex; }
                    return true;
                default:
                    return DefaultCmdKey(ref msg, keyData);
            }
        }

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e) {
            base.OnTextChanged(e);
            if (!TriggersEnabled) { return; }
            switch (Triggers.OnTextChanged(Text)) {
                case TriggerState.Show:
                    ShowList();
                    break;
                case TriggerState.Hide:
                    HideList();
                    break;
                default:
                    UpdateList();
                    break;
            }
        }

        /// <inheritdoc />
        protected override void OnLostFocus(EventArgs e) {
            base.OnLostFocus(e);
            if (Focused || PopupForm.Focused || _listBox.Focused) { return; }
            HideList();
        }

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && _components != null) { _components.Dispose(); }
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void Initialize() {
            Mode = EntryMode.Text;
            Items = new AutoCompleteEntryCollection();
            Triggers = new AutoCompleteTriggerCollection();
            PopupOffset = new Point(12, 0);
            PopupSelectionBackColor = SystemColors.Highlight;
            PopupSelectionForeColor = SystemColors.HighlightText;
            TriggersEnabled = true;
            PopupForm = new Form {
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                FormBorderStyle = FormBorderStyle.None,
                TopMost = true
            };
            PopupForm.Deactivate += new EventHandler(Popup_Deactivate);
            _listBox = new ListBox {
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.None
            };
            _listBox.SelectedIndexChanged += new EventHandler(List_SelectedIndexChanged);
            _listBox.MouseDown += new MouseEventHandler(List_MouseDown);
            _listBox.ItemHeight = 14;
            _listBox.DrawMode = DrawMode.OwnerDrawFixed;
            _listBox.DrawItem += new DrawItemEventHandler(List_DrawItem);
            _listBox.Dock = DockStyle.Fill;
            PopupForm.Controls.Add(_listBox);
            Triggers.Add(new TextLengthTrigger());
            Triggers.Add(new ShortCutTrigger(Keys.Return, TriggerState.SelectAndConsume));
            Triggers.Add(new ShortCutTrigger(Keys.Tab, TriggerState.Select));
            Triggers.Add(new ShortCutTrigger(Keys.Space | Keys.Control, TriggerState.ShowAndConsume));
            Triggers.Add(new ShortCutTrigger(Keys.Escape, TriggerState.HideAndConsume));
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e) {
            if (Mode == EntryMode.List) { return; }
            SelectCurrentItem();
        }

        private void List_MouseDown(object sender, MouseEventArgs e) {
            for (int index = 0; index < _listBox.Items.Count; ++index) {
                if (_listBox.GetItemRectangle(index).Contains(e.X, e.Y)) {
                    _listBox.SelectedIndex = index;
                    SelectCurrentItem();
                }
            }
            HideList();
        }

        private void List_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.State == DrawItemState.Selected) {
                e.Graphics.FillRectangle(new SolidBrush(PopupSelectionBackColor), e.Bounds);
                e.Graphics.DrawString(_listBox.Items[e.Index].ToString(), e.Font, new SolidBrush(PopupSelectionForeColor), e.Bounds, StringFormat.GenericDefault);
            } else {
                e.DrawBackground();
                e.Graphics.DrawString(_listBox.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, StringFormat.GenericDefault);
            }
        }

        private void Popup_Deactivate(object sender, EventArgs e) {
            if (Focused || PopupForm.Focused || _listBox.Focused) { return; }
            HideList();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => _components = new Container();

        #endregion

        #region Public Inner Enums

        /// <summary>Defines the entry modes.</summary>
        [Flags]
        public enum EntryMode {
            /// <summary>Text</summary>
            Text = 0,
            /// <summary>List</summary>
            List = 1,
        }

        #endregion

        #region Private Inner Class

        /// <summary>This is the class we will use to hook mouse events.</summary>
        private class WinHook : NativeWindow {

            #region Private Fields

            private AutoCompleteTextBox _autoCompleteTextBox;

            #endregion

            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of <see cref="WinHook" />
            /// </summary>
            /// <param name="autoCompleteTextBox">The <see cref="AutoCompleteTextBox" /> the hook is running for.</param>
            public WinHook(AutoCompleteTextBox autoCompleteTextBox) => _autoCompleteTextBox = autoCompleteTextBox;

            #endregion

            /// <summary>
            /// Look for any kind of mouse activity that is not in the
            /// text box itself, and hide the popup if it is visible.
            /// </summary>
            /// <param name="message"></param>
            protected override void WndProc(ref Message message) {
                Point point;
                Rectangle rectangle;
                switch (message.Msg) {
                    case Messages.WM_MOVE:
                    case Messages.WM_SIZE:
                        _autoCompleteTextBox.HideList();
                        break;
                    case Messages.WM_NCLBUTTONDOWN:
                    case Messages.WM_NCRBUTTONDOWN:
                    case Messages.WM_NCMBUTTONDOWN:
                    case Messages.WM_LBUTTONDOWN:
                    case Messages.WM_LBUTTONDBLCLK:
                    case Messages.WM_RBUTTONDOWN:
                    case Messages.WM_RBUTTONDBLCLK:
                    case Messages.WM_MBUTTONDOWN:
                    case Messages.WM_MBUTTONDBLCLK:
                        point = _autoCompleteTextBox.FindForm().PointToScreen(new Point((int)message.LParam));
                        rectangle = new Rectangle(_autoCompleteTextBox.PointToScreen(new Point(0, 0)), _autoCompleteTextBox.Size);
                        if (!rectangle.Contains(point)) {
                            _autoCompleteTextBox.HideList();
                            break;
                        }
                        break;
                    case Messages.WM_PARENTNOTIFY:
                        switch ((int)message.WParam) {
                            case Messages.WM_NCLBUTTONDOWN:
                            case Messages.WM_NCRBUTTONDOWN:
                            case Messages.WM_NCMBUTTONDOWN:
                            case Messages.WM_LBUTTONDOWN:
                            case Messages.WM_LBUTTONDBLCLK:
                            case Messages.WM_RBUTTONDOWN:
                            case Messages.WM_RBUTTONDBLCLK:
                            case Messages.WM_MBUTTONDOWN:
                            case Messages.WM_MBUTTONDBLCLK:
                                point = _autoCompleteTextBox.FindForm().PointToScreen(new Point((int)message.LParam));
                                rectangle = new Rectangle(_autoCompleteTextBox.PointToScreen(new Point(0, 0)), _autoCompleteTextBox.Size);
                                if (!rectangle.Contains(point)) {
                                    _autoCompleteTextBox.HideList();
                                    break;
                                }
                                break;
                        }
                        break;
                }
                base.WndProc(ref message);
            }
        }

        #endregion
    }
}