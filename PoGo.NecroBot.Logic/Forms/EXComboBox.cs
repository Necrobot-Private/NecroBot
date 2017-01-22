using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace PoGo.NecroBot.Logic.Forms
{

    class EXComboBox : ComboBox {
        
        private Brush _highlightbrush; //color of highlighted items
        
        public EXComboBox() {
            _highlightbrush = SystemBrushes.Highlight;
            this.DrawMode = DrawMode.OwnerDrawFixed;
            this.DrawItem += new DrawItemEventHandler(this_DrawItem);
        }
        
        public Brush MyHighlightBrush {
            get {return _highlightbrush;}
            set {_highlightbrush = value;}
        }
        
        private void this_DrawItem(object sender, DrawItemEventArgs e) {
            if (e.Index == -1) return;
            e.DrawBackground();
            if ((e.State & DrawItemState.Selected) != 0) {
                e.Graphics.FillRectangle(_highlightbrush, e.Bounds);
            }
            EXItem item = (EXItem) this.Items[e.Index];
            Rectangle bounds = e.Bounds;
            int x = bounds.X + 2;
            if (item.GetType() == typeof(EXImageItem)) {
                EXImageItem imgitem = (EXImageItem) item;
                if (imgitem.MyImage != null) {
                    Image img = imgitem.MyImage;
                    int y = bounds.Y + ((int) (bounds.Height / 2)) - ((int) (img.Height / 2)) + 1;
                    e.Graphics.DrawImage(img, x, y, img.Width, img.Height);
                    x += img.Width + 2;
                }
            } else if (item.GetType() == typeof(EXMultipleImagesItem)) {
                EXMultipleImagesItem imgitem = (EXMultipleImagesItem) item; 
                if (imgitem.MyImages != null) {
                    for (int i = 0; i < imgitem.MyImages.Count; i++) {
                        Image img = (Image) imgitem.MyImages[i];
                        int y = bounds.Y + ((int) (bounds.Height / 2)) - ((int) (img.Height / 2)) + 1;
                        e.Graphics.DrawImage(img, x, y, img.Width, img.Height);
                        x += img.Width + 2;
                    }
                }
            }
            int fonty = bounds.Y + ((int) (bounds.Height / 2)) - ((int) (e.Font.Height / 2));
            e.Graphics.DrawString(item.Text, e.Font, new SolidBrush(e.ForeColor), x, fonty);
            e.DrawFocusRectangle();
        }
        
        public class EXItem {
            
            private string _text = "";
            private string _value = "";
            
            public EXItem() {
                
            }
            
            public EXItem(string text) {
                _text = text;
            }
            
            public string Text {
                get {return _text;}
                set {_text = value;}
            }
            
            public string MyValue {
                get {return _value;}
                set {_value = value;}
            }
            
            public override string ToString() {
                return _text;
            }
            
        }
        
        public class EXImageItem : EXItem {
            
            private Image _image;
                
            public EXImageItem() {
                
            }
            
            public EXImageItem(string text) {
                this.Text = text;
            }
            
            public EXImageItem(Image image) {
                _image = image;
            }
            
            public EXImageItem(string text, Image image) {
                this.Text = text;
                _image = image;
            }
            
            public EXImageItem(Image image, string value) {
                _image = image;
                this.MyValue = value;
            }
            
            public EXImageItem(string text, Image image, string value) {
                this.Text = text;
                _image = image;
                this.MyValue = value;
            }
            
            public Image MyImage {
                get {return _image;}
                set {_image = value;}
            }
            
        }
        
        public class EXMultipleImagesItem : EXItem {
            
            private ArrayList _images;
                
            public EXMultipleImagesItem() {
                
            }
            
            public EXMultipleImagesItem(string text) {
                this.Text = text;
            }
            
            public EXMultipleImagesItem(ArrayList images) {
                _images = images;
            }
            
            public EXMultipleImagesItem(string text, ArrayList images) {
                this.Text = text;
                _images = images;
            }
            
            public EXMultipleImagesItem(ArrayList images, string value) {
                _images = images;
                this.MyValue = value;
            }
            
            public EXMultipleImagesItem(string text, ArrayList images, string value) {
                this.Text = text;
                _images = images;
                this.MyValue = value;
            }

            public ArrayList MyImages {
                get {return _images;}
                set {_images = value;}
            }
            
        }
    
    }

}