using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interactivity;

namespace PoGo.Necrobot.Window.Controls
{
    public class EmptyItemsControlAdornerBehavior : Behavior<ItemsControl>
    {
        #region Overridden Members

        protected override void OnAttached()
        {
            base.OnAttached();

            AdornedElement = this.AssociatedObject;
            this.ItemsControlAdorner = new TemplatedAdorner(AdornedElement, this.DataTemplate, this.Data);

            var collectionViewSource = CollectionViewSource.GetDefaultView(this.AdornedElement.Items);
            if (collectionViewSource != null)
            {
                collectionViewSource.CollectionChanged += ItemsChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            var collectionViewSource = CollectionViewSource.GetDefaultView(this.AdornedElement.ItemsSource);
            if (collectionViewSource != null)
            {
                collectionViewSource.CollectionChanged -= ItemsChanged;
            }
        }

        #endregion Overridden Members

        #region Public Members

        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        public static DependencyProperty DataTemplateProperty =
            DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(EmptyItemsControlAdornerBehavior));

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(EmptyItemsControlAdornerBehavior));

        #endregion Public Members

        #region Private Members

        private ItemsControl AdornedElement { get; set; }
        private TemplatedAdorner ItemsControlAdorner { get; set; }

        private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.ItemsControlAdorner.Visibility =
                this.AdornedElement.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion Private Members
    }

}
