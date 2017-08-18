using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace PoGo.NecroBot.Window.Controls
{
    public class EmptyItemsControlAdornerBehavior : Behavior<ItemsControl>
    {
        #region Overridden Members

        protected override void OnAttached()
        {
            base.OnAttached();

            AdornedElement = AssociatedObject;
            ItemsControlAdorner = new TemplatedAdorner(AdornedElement, DataTemplate, Data);

            var collectionViewSource = CollectionViewSource.GetDefaultView(AdornedElement.Items);
            if (collectionViewSource != null)
            {
                collectionViewSource.CollectionChanged += ItemsChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            var collectionViewSource = CollectionViewSource.GetDefaultView(AdornedElement.ItemsSource);
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
            ItemsControlAdorner.Visibility =
                AdornedElement.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion Private Members
    }

}
