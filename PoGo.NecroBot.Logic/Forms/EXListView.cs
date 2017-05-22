using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Runtime.InteropServices;

namespace PoGo.NecroBot.Logic.Forms
{
    public class EXListView : ListView
    {
        private ListViewItem.ListViewSubItem _clickedsubitem; //clicked ListViewSubItem
        private ListViewItem _clickeditem; //clicked ListViewItem
        private int _col; //index of doubleclicked ListViewSubItem
        private TextBox txtbx; //the default edit control
        private int _sortcol; //index of clicked ColumnHeader
        private Brush _sortcolbrush; //color of items in sorted column
        private Brush _highlightbrush; //color of highlighted items
        private int _cpadding; //padding of the embedded controls
            
        private const UInt32 LVM_FIRST = 0x1000;
        private const UInt32 LVM_SCROLL = (LVM_FIRST + 20);
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x020A;
        private const int WM_PAINT = 0x000F;
            
        private struct EmbeddedControl
        {
            public Control MyControl;
            public EXControlListViewSubItem MySubItem;
        }
            
        private ArrayList _controls;
            
        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hWnd, UInt32 m, int wParam, int lParam);
        
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PAINT)
            {
                foreach (EmbeddedControl c in _controls)
                {
                    Rectangle r = c.MySubItem.Bounds;
                    if (r.Y > 0 && r.Y < ClientRectangle.Height)
                    {
                        c.MyControl.Visible = true;
                        c.MyControl.Bounds = new Rectangle(r.X + _cpadding, r.Y + _cpadding, r.Width - (2 * _cpadding), r.Height - (2 * _cpadding));
                    }
                    else
                    {
                        c.MyControl.Visible = false;
                    }
                }
            }
            switch (m.Msg)
            {
                case WM_HSCROLL:
                case WM_VSCROLL:
                case WM_MOUSEWHEEL:
                    Focus();
                    break;
            }
            base.WndProc(ref m);
        }
        
        private void ScrollMe(int x, int y)
        {
            SendMessage(Handle, LVM_SCROLL, x, y);
        }
        
        public EXListView()
        {
            _cpadding = 4;
            _controls = new ArrayList();
            _sortcol = -1;
            _sortcolbrush = SystemBrushes.ControlLight;
            _highlightbrush = SystemBrushes.Highlight;
            OwnerDraw = true;
            FullRowSelect = true;
            View = View.Details;
            MouseDown += new MouseEventHandler(This_MouseDown);
            MouseDoubleClick += new MouseEventHandler(This_MouseDoubleClick);
            DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(This_DrawColumnHeader);
            DrawSubItem += new DrawListViewSubItemEventHandler(This_DrawSubItem);
            MouseMove += new MouseEventHandler(This_MouseMove);
            ColumnClick += new ColumnClickEventHandler(This_ColumnClick);
            txtbx = new TextBox()
            {
                Visible = false
            };
            Controls.Add(txtbx);
            txtbx.Leave += new EventHandler(C_Leave);
            txtbx.KeyPress += new KeyPressEventHandler(Txtbx_KeyPress);
        }
        
        public void AddControlToSubItem(Control control, EXControlListViewSubItem subitem)
        {
            Controls.Add(control);
            subitem.MyControl = control;
            EmbeddedControl ec;
            ec.MyControl = control;
            ec.MySubItem = subitem;
            _controls.Add(ec);
        }
        
        public void RemoveControlFromSubItem(EXControlListViewSubItem subitem)
        {
            Control c = subitem.MyControl;
            for (int i = 0; i < _controls.Count; i++) {
                if (((EmbeddedControl)_controls[i]).MySubItem == subitem) {
                    _controls.RemoveAt(i);
                    subitem.MyControl = null;
                    Controls.Remove(c);
                    c.Dispose();
                    return;
                }
            }
        }
        
        public int ControlPadding
        {
            get {return _cpadding;}
            set {_cpadding = value;}
        }
        
        public Brush MySortBrush
        {
            get {return _sortcolbrush;}
            set {_sortcolbrush = value;}
        }
        
        public Brush MyHighlightBrush
        {
            get {return _highlightbrush;}
            set {_highlightbrush = value;}
        }
        
        private void Txtbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Return) {
                _clickedsubitem.Text = txtbx.Text;
                txtbx.Visible = false;
                _clickeditem.Tag = null;
            }
        }
        
        private void C_Leave(object sender, EventArgs e)
        {
            Control c = (Control) sender;
            _clickedsubitem.Text = c.Text;
            c.Visible = false;
            _clickeditem.Tag = null;
        }
        
        private void This_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo lstvinfo = HitTest(e.X, e.Y);
            ListViewItem.ListViewSubItem subitem = lstvinfo.SubItem;
            if (subitem == null) return;
            int subx = subitem.Bounds.Left;
            if (subx < 0)
            {
                ScrollMe(subx, 0);
            }
        }
        
        private void This_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            EXListViewItem lstvItem = GetItemAt(e.X, e.Y) as EXListViewItem;
            if (lstvItem == null) return;
            _clickeditem = lstvItem;
            int x = lstvItem.Bounds.Left;
            int i;
            for (i = 0; i < Columns.Count; i++)
            {
                x = x + Columns[i].Width;
                if (x > e.X)
                {
                    x = x - Columns[i].Width;
                    _clickedsubitem = lstvItem.SubItems[i];
                    _col = i;
                    break;
                }
            }
            if (!(Columns[i] is EXColumnHeader)) return;
            EXColumnHeader col = (EXColumnHeader)Columns[i];
            if (col.GetType() == typeof(EXEditableColumnHeader))
            {
                EXEditableColumnHeader editcol = (EXEditableColumnHeader) col;
                if (editcol.MyControl != null)
                {
                    Control c = editcol.MyControl;
                    if (c.Tag != null)
                    {
                        Controls.Add(c);
                        c.Tag = null;
                        if (c is ComboBox)
                        {
                            ((ComboBox) c).SelectedValueChanged += new EventHandler(Cmbx_SelectedValueChanged);
                        }
                        c.Leave += new EventHandler(C_Leave);
                    }
                    c.Location = new Point(x, GetItemRect(Items.IndexOf(lstvItem)).Y);
                    c.Width = Columns[i].Width;
                    if (c.Width > Width) c.Width = ClientRectangle.Width;
                    c.Text = _clickedsubitem.Text;
                    c.Visible = true;
                    c.BringToFront();
                    c.Focus();
                }
                else
                {
                    txtbx.Location = new Point(x, GetItemRect(Items.IndexOf(lstvItem)).Y);
                    txtbx.Width = Columns[i].Width;
                    if (txtbx.Width > Width) txtbx.Width = ClientRectangle.Width;
                    txtbx.Text = _clickedsubitem.Text;
                    txtbx.Visible = true;
                    txtbx.BringToFront();
                    txtbx.Focus();
                }
            }
            else if (col.GetType() == typeof(EXBoolColumnHeader))
            {
                EXBoolColumnHeader boolcol = (EXBoolColumnHeader) col;
                if (boolcol.Editable)
                {
                    EXBoolListViewSubItem boolsubitem = (EXBoolListViewSubItem) _clickedsubitem;
                    if (boolsubitem.BoolValue == true)
                    {
                        boolsubitem.BoolValue = false;
                    }
                    else
                    {
                        boolsubitem.BoolValue = true;
                    }
                    Invalidate(boolsubitem.Bounds);
                }
            }
        }
        
        private void Cmbx_SelectedValueChanged(object sender, EventArgs e)
        {
            if (((Control) sender).Visible == false || _clickedsubitem == null) return;
            if (sender.GetType() == typeof(EXComboBox))
            {
                EXComboBox excmbx = (EXComboBox) sender;
                object item = excmbx.SelectedItem;
                //Is this an combobox item with one image?
                if (item.GetType() == typeof(EXComboBox.EXImageItem))
                {
                    EXComboBox.EXImageItem imgitem = (EXComboBox.EXImageItem) item;
                    //Is the first column clicked -- in that case it's a ListViewItem
                    if (_col == 0)
                    {
                        if (_clickeditem.GetType() == typeof(EXImageListViewItem))
                        {
                            ((EXImageListViewItem) _clickeditem).MyImage = imgitem.MyImage;
                        }
                        else if (_clickeditem.GetType() == typeof(EXMultipleImagesListViewItem))
                        {
                            EXMultipleImagesListViewItem imglstvitem = (EXMultipleImagesListViewItem) _clickeditem;
			                imglstvitem.MyImages.Clear();
			                imglstvitem.MyImages.AddRange(new object[] {imgitem.MyImage});
                        }
                    //another column than the first one is clicked, so we have a ListViewSubItem
                    }
                    else
                    {
                        if (_clickedsubitem.GetType() == typeof(EXImageListViewSubItem))
                        {
                            EXImageListViewSubItem imgsub = (EXImageListViewSubItem) _clickedsubitem;
                            imgsub.MyImage = imgitem.MyImage;
                        }
                        else if (_clickedsubitem.GetType() == typeof(EXMultipleImagesListViewSubItem))
                        {
                            EXMultipleImagesListViewSubItem imgsub = (EXMultipleImagesListViewSubItem) _clickedsubitem;
                            imgsub.MyImages.Clear();
                            imgsub.MyImages.Add(imgitem.MyImage);
			                imgsub.MyValue = imgitem.MyValue;
                        }
                    }
                    //or is this a combobox item with multiple images?
                }
                else if (item.GetType() == typeof(EXComboBox.EXMultipleImagesItem))
                {
                    EXComboBox.EXMultipleImagesItem imgitem = (EXComboBox.EXMultipleImagesItem) item;
                    if (_col == 0)
                    {
                        if (_clickeditem.GetType() == typeof(EXImageListViewItem))
                        {
                            ((EXImageListViewItem) _clickeditem).MyImage = (Image) imgitem.MyImages[0];
                        }
                        else if (_clickeditem.GetType() == typeof(EXMultipleImagesListViewItem))
                        {
                            EXMultipleImagesListViewItem imglstvitem = (EXMultipleImagesListViewItem) _clickeditem;
			                imglstvitem.MyImages.Clear();
			                imglstvitem.MyImages.AddRange(imgitem.MyImages);
                        }
                    }
                    else
                    {
                        if (_clickedsubitem.GetType() == typeof(EXImageListViewSubItem))
                        {
                            EXImageListViewSubItem imgsub = (EXImageListViewSubItem) _clickedsubitem;
                            if (imgitem.MyImages != null)
                            {
                                imgsub.MyImage = (Image) imgitem.MyImages[0];
                            }
                        }
                        else if (_clickedsubitem.GetType() == typeof(EXMultipleImagesListViewSubItem))
                        {
                            EXMultipleImagesListViewSubItem imgsub = (EXMultipleImagesListViewSubItem) _clickedsubitem;
                            imgsub.MyImages.Clear();
			                imgsub.MyImages.AddRange(imgitem.MyImages);
			                imgsub.MyValue = imgitem.MyValue;
                        }
                    }
                }
            }
            ComboBox c = (ComboBox) sender;
            _clickedsubitem.Text = c.Text;
            c.Visible = false;
            _clickeditem.Tag = null;
        }
        
        private void This_MouseMove(object sender, MouseEventArgs e)
        {
            ListViewItem item = GetItemAt(e.X, e.Y);
            if (item != null && item.Tag == null)
            {
                Invalidate(item.Bounds);
                item.Tag = "t";
            }
        }
        
        private void This_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }
        
        private void This_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();
            if (e.ColumnIndex == _sortcol)
            {
                e.Graphics.FillRectangle(_sortcolbrush, e.Bounds);
            }
            if ((e.ItemState & ListViewItemStates.Selected) != 0)
            {
                e.Graphics.FillRectangle(_highlightbrush, e.Bounds);
            }
            int fonty = e.Bounds.Y + e.Bounds.Height / 2 - e.SubItem.Font.Height / 2;
            int x = e.Bounds.X + 2;
            if (e.ColumnIndex == 0)
            {
                EXListViewItem item = (EXListViewItem) e.Item;
                if (item.GetType() == typeof(EXImageListViewItem))
                {
                    EXImageListViewItem imageitem = (EXImageListViewItem) item;
                    if (imageitem.MyImage != null)
                    {
                        Image img = imageitem.MyImage;
                        int imgy = e.Bounds.Y + e.Bounds.Height / 2 - img.Height / 2;
                        e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                        x += img.Width + 2;
                    }
                }
                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, new SolidBrush(e.SubItem.ForeColor), x, fonty);
                return;
            }
            EXListViewSubItemAB subitem = e.SubItem as EXListViewSubItemAB;
            if (subitem == null)
            {
                e.DrawDefault = true;
            }
            else
            {
                x = subitem.DoDraw(e, x, Columns[e.ColumnIndex] as EXColumnHeader);                
                e.Graphics.DrawString(e.SubItem.Text, e.SubItem.Font, new SolidBrush(e.SubItem.ForeColor), x, fonty);
            }
        }
        
        private void This_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (Items.Count == 0) return;
            for (int i = 0; i < Columns.Count; i++)
            {
                Columns[i].ImageKey = null;
            }
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Tag = null;
            }
            if (e.Column != _sortcol)
            {
                _sortcol = e.Column;
                Sorting = SortOrder.Ascending;
                Columns[e.Column].ImageKey = "up";
            }
            else
            {
                if (Sorting == SortOrder.Ascending)
                {
                    Sorting = SortOrder.Descending;
                    Columns[e.Column].ImageKey = "down";
                }
                else
                {
                    Sorting = SortOrder.Ascending;
                    Columns[e.Column].ImageKey = "up";
                }
            }
            if (_sortcol == 0)
            {
                //ListViewItem
                if (Items[0].GetType() == typeof(EXListViewItem))
                {
                    //sorting on text
                    ListViewItemSorter = new ListViewItemComparerText(e.Column, Sorting);
                }
                else
                {
                    //sorting on value
                    ListViewItemSorter = new ListViewItemComparerValue(e.Column, Sorting);
                }
            }
            else
            {
                //ListViewSubItem
                if (Items[0].SubItems[_sortcol].GetType() == typeof(EXListViewSubItemAB))
                {
                    //sorting on text
                    ListViewItemSorter = new ListViewSubItemComparerText(e.Column, Sorting);
                }
                else
                {
                    //sorting on value
                    ListViewItemSorter = new ListViewSubItemComparerValue(e.Column, Sorting);
                }
            }
        }
        
        class ListViewSubItemComparerText : IComparer
        {
            
            private int _col;
            private SortOrder _order;

            public ListViewSubItemComparerText() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            public ListViewSubItemComparerText(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            public int Compare(object x, object y)
            {
#pragma warning disable IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
                int returnVal = -1;
                
                string xstr = ((ListViewItem) x).SubItems[_col].Text;
                string ystr = ((ListViewItem) y).SubItems[_col].Text;

                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;

                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y))
                {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                }
                else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y))
                {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                }
                else
                {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;
                return returnVal;
#pragma warning restore IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
            }
        }
	
	    class ListViewSubItemComparerValue : IComparer
        {
            
            private int _col;
            private SortOrder _order;

            public ListViewSubItemComparerValue() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            public ListViewSubItemComparerValue(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            public int Compare(object x, object y)
            {
#pragma warning disable IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
                int returnVal = -1;
                
                string xstr = ((EXListViewSubItemAB) ((ListViewItem) x).SubItems[_col]).MyValue;
                string ystr = ((EXListViewSubItemAB) ((ListViewItem) y).SubItems[_col]).MyValue;

                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;

                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y))
                {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                }
                else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y))
                {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                }
                else
                {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;
                return returnVal;
#pragma warning restore IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
            }
        }
	
	    class ListViewItemComparerText : IComparer
        {
            private int _col;
            private SortOrder _order;

            public ListViewItemComparerText() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            public ListViewItemComparerText(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            public int Compare(object x, object y)
            {
#pragma warning disable IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
                int returnVal = -1;
                
                string xstr = ((ListViewItem) x).Text;
                string ystr = ((ListViewItem) y).Text;

                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;

                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y))
                {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                }
                else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y))
                {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                }
                else
                {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;
                return returnVal;
#pragma warning restore IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
            }
        }
	
	    class ListViewItemComparerValue : IComparer
        {
            private int _col;
            private SortOrder _order;

            public ListViewItemComparerValue() {
                _col = 0;
                _order = SortOrder.Ascending;
            }

            public ListViewItemComparerValue(int col, SortOrder order) {
                _col = col;
                _order = order;
            }
            
            public int Compare(object x, object y)
            {
#pragma warning disable IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
                int returnVal = -1;
                
                string xstr = ((EXListViewItem) x).MyValue;
                string ystr = ((EXListViewItem) y).MyValue;

                decimal dec_x;
                decimal dec_y;
                DateTime dat_x;
                DateTime dat_y;

                if (Decimal.TryParse(xstr, out dec_x) && Decimal.TryParse(ystr, out dec_y))
                {
                    returnVal = Decimal.Compare(dec_x, dec_y);
                }
                else if (DateTime.TryParse(xstr, out dat_x) && DateTime.TryParse(ystr, out dat_y))
                {
                    returnVal = DateTime.Compare(dat_x, dat_y);
                }
                else
                {
                    returnVal = String.Compare(xstr, ystr);
                }
                if (_order == SortOrder.Descending) returnVal *= -1;
                return returnVal;
#pragma warning restore IDE0018 // Inline variable declaration - Build.Bat Error Happens if We Do
            }
        }
    }
    
    public class EXColumnHeader : ColumnHeader
    {
        public EXColumnHeader()
        {
            
        }
        
        public EXColumnHeader(string text)
        {
            Text = text;
        }
        
        public EXColumnHeader(string text, int width)
        {
            Text = text;
            Width = width;
        }
        
    }
    
    public class EXEditableColumnHeader : EXColumnHeader {
        
        private Control _control;
        
        public EXEditableColumnHeader()
        {
            
        }
        
        public EXEditableColumnHeader(string text)
        {
            Text = text;
        }
        
        public EXEditableColumnHeader(string text, int width)
        {
            Text = text;
            Width = width;
        }
        
        public EXEditableColumnHeader(string text, Control control)
        {
            Text = text;
            MyControl = control;
        }
        
        public EXEditableColumnHeader(string text, Control control, int width)
        {
            Text = text;
            MyControl = control;
            Width = width;
        }
        
        public Control MyControl
        {
            get {return _control;}
            set
            {
                _control = value;
                _control.Visible = false;
                _control.Tag = "not_init";
            }
        }
    }
    
    public class EXBoolColumnHeader : EXColumnHeader
    {
        
        private Image _trueimage;
        private Image _falseimage;
        private bool _editable;
            
        public EXBoolColumnHeader()
        {
            Init();
        }
        
        public EXBoolColumnHeader(string text)
        {
            Init();
            Text = text;
        }
        
        public EXBoolColumnHeader(string text, int width)
        {
            Init();
            Text = text;
            Width = width;
        }
        
        public EXBoolColumnHeader(string text, Image trueimage, Image falseimage)
        {
            Init();
            Text = text;
            _trueimage = trueimage;
            _falseimage = falseimage;
        }
        
        public EXBoolColumnHeader(string text, Image trueimage, Image falseimage, int width)
        {
            Init();
            Text = text;
            _trueimage = trueimage;
            _falseimage = falseimage;
            Width = width;
        }
        
        private void Init()
        {
            _editable = false;
        }
        
        public Image TrueImage
        {
            get {return _trueimage;}
            set {_trueimage = value;}
        }
        
        public Image FalseImage
        {
            get {return _falseimage;}
            set {_falseimage = value;}
        }
        
        public bool Editable
        {
            get {return _editable;}
            set {_editable = value;}
        }
    }
    
    public abstract class EXListViewSubItemAB : ListViewItem.ListViewSubItem {
        
        private string _value = "";
        
        public EXListViewSubItemAB()
        {
            
        }
        
        public EXListViewSubItemAB(string text)
        {
            Text = text;
        }
        
        public string MyValue
        {
            get {return _value;}
            set {_value = value;}
        }
        
        //return the new x coordinate
        public abstract int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch);
    }
    
    public class EXListViewSubItem : EXListViewSubItemAB
    {
        public EXListViewSubItem()
        {
            
        }
        
        public EXListViewSubItem(string text)
        {
            Text = text;
        }
        
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {
            return x;
        }
    }
    
    public class EXControlListViewSubItem : EXListViewSubItemAB
    {
        private Control _control;
            
        public EXControlListViewSubItem()
        {
            
        }
        
        public Control MyControl
        {
            get {return _control;}
            set {_control = value;}
        }
        
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {
            return x;
        }
    }
    
    public class EXImageListViewSubItem : EXListViewSubItemAB
    {
        private Image _image;
            
        public EXImageListViewSubItem()
        {
            
        }
        
        public EXImageListViewSubItem(string text)
        {
            Text = text;
        }
            
        public EXImageListViewSubItem(Image image)
        {
            _image = image;
        }
	
        public EXImageListViewSubItem(Image image, string value)
        {
            _image = image;
            MyValue = value;
        }
	
        public EXImageListViewSubItem(string text, Image image, string value)
        {
            Text = text;
            _image = image;
            MyValue = value;
        }
        
        public Image MyImage
        {
            get {return _image;}
            set {_image = value;}
        }
        
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {
            if (MyImage != null)
            {
                Image img = MyImage;
                int imgy = e.Bounds.Y + e.Bounds.Height / 2 - img.Height / 2;
                e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                x += img.Width + 2;
            }
            return x;
        }
    }
    
    public class EXMultipleImagesListViewSubItem : EXListViewSubItemAB
    {
        private ArrayList _images;
            
        public EXMultipleImagesListViewSubItem()
        {
            
        }
        
        public EXMultipleImagesListViewSubItem(string text)
        {
            Text = text;
        }
            
        public EXMultipleImagesListViewSubItem(ArrayList images)
        {
            _images = images;
        }
	
        public EXMultipleImagesListViewSubItem(ArrayList images, string value)
        {
            _images = images;
            MyValue = value;
        }
        
        public EXMultipleImagesListViewSubItem(string text, ArrayList images, string value)
        {
            Text = text;
            _images = images;
            MyValue = value;
        }
        
        public ArrayList MyImages
        {
            get {return _images;}
            set {_images = value;}
        }
        
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {    
            if (MyImages != null && MyImages.Count > 0)
            {
                for (int i = 0; i < MyImages.Count; i++)
                {
                    Image img = (Image)MyImages[i];
                    int imgy = e.Bounds.Y + e.Bounds.Height / 2 - img.Height / 2;
                    e.Graphics.DrawImage(img, x, imgy, img.Width, img.Height);
                    x += img.Width + 2;
                }
            }
            return x;
        }
    }
    
    public class EXBoolListViewSubItem : EXListViewSubItemAB
    {
        private bool _value;
            
        public EXBoolListViewSubItem()
        {
            
        }
        
        public EXBoolListViewSubItem(bool val)
        {
            _value = val;
            MyValue = val.ToString();
        }
        
        public bool BoolValue
        {
            get {return _value;}
            set
            {
		        _value = value;
                MyValue = value.ToString();
	        }
        }
        
        public override int DoDraw(DrawListViewSubItemEventArgs e, int x, EXColumnHeader ch)
        {    
            EXBoolColumnHeader boolcol = (EXBoolColumnHeader) ch;
            Image boolimg;
            if (BoolValue == true)
            {
                boolimg = boolcol.TrueImage;
            }
            else
            {
                boolimg = boolcol.FalseImage;
            }
            int imgy = e.Bounds.Y + e.Bounds.Height / 2 - boolimg.Height / 2;
            e.Graphics.DrawImage(boolimg, x, imgy, boolimg.Width, boolimg.Height);
            x += boolimg.Width + 2;
            return x;
        }
    }
    
    public class EXListViewItem : ListViewItem
    {
	    private string _value;
        
        public EXListViewItem()
        {
            
        }
        
        public EXListViewItem(string text)
        {
            Text = text;
        }
	
        public string MyValue
        {
            get {return _value;}
            set {_value = value;}
        }
        
    }
    
    public class EXImageListViewItem : EXListViewItem
    {
        private Image _image;
            
        public EXImageListViewItem()
        {
            
        }
        
        public EXImageListViewItem(string text)
        {
            Text = text;
        }
        
        public EXImageListViewItem(Image image)
        {
            _image = image;
        }
	
        public EXImageListViewItem(string text, Image image)
        {
            _image = image;
            Text = text;
        }
	
	    public EXImageListViewItem(string text, Image image, string value)
        {
            Text = text;
            _image = image;
            MyValue = value;
        }
        
        public Image MyImage
        {
            get {return _image;}
            set {_image = value;}
        }
    }
    
    public class EXMultipleImagesListViewItem : EXListViewItem
    {
        private ArrayList _images;
            
        public EXMultipleImagesListViewItem()
        {
            
        }
        
        public EXMultipleImagesListViewItem(string text)
        {
            Text = text;
        }
            
        public EXMultipleImagesListViewItem(ArrayList images)
        {
            _images = images;
        }
	
	    public EXMultipleImagesListViewItem(string text, ArrayList images)
        {
            Text = text;
            _images = images;
        }
	
	    public EXMultipleImagesListViewItem(string text, ArrayList images, string value)
        {
            Text = text;
            _images = images;
            MyValue = value;
        }
        
        public ArrayList MyImages {
            get {return _images;}
            set {_images = value;}
        }
    }    
}