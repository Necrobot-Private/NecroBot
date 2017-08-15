using Microsoft.Windows.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PoGo.NecroBot.Window.Controls
{
    //Credit - https://www.codeproject.com/Articles/41755/Filtering-the-WPF-DataGrid-automatically-via-the-h

    public class FilteringDataGrid   : Microsoft.Windows.Controls.DataGrid
    {
        /// <summary>
        /// This dictionary will have a list of all applied filters
        /// </summary>
        private Dictionary<string, string> columnFilters;
        /// <summary>
        /// Cache with properties for better performance
        /// </summary>
        private Dictionary<string, PropertyInfo> propertyCache;
        /// <summary>
        /// Case sensitive filtering
        /// </summary>
        public static DependencyProperty IsFilteringCaseSensitiveProperty =
             DependencyProperty.Register("IsFilteringCaseSensitive",
             typeof(bool), typeof(FilteringDataGrid), new PropertyMetadata(true));
        /// <summary>
        /// Case sensitive filtering
        /// </summary>
        public bool IsFilteringCaseSensitive
        {
            get { return (bool)(GetValue(IsFilteringCaseSensitiveProperty)); }
            set { SetValue(IsFilteringCaseSensitiveProperty, value); }
        }
        /// <summary>
        /// Register for all text changed events
        /// </summary>
        public FilteringDataGrid()
        {
            // Initialize lists
            columnFilters = new Dictionary<string, string>();
            propertyCache = new Dictionary<string, PropertyInfo>();
            // Add a handler for all text changes
            AddHandler(TextBox.TextChangedEvent,
              new TextChangedEventHandler(OnTextChanged), true);
            // Datacontext changed, so clear the cache
            DataContextChanged += new
              DependencyPropertyChangedEventHandler(
              FilteringDataGrid_DataContextChanged);
        }
        /// <summary>
        /// Clear the property cache if the datacontext changes.
        /// This could indicate that an other type of object is bound.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilteringDataGrid_DataContextChanged(object sender,
                     DependencyPropertyChangedEventArgs e)
        {
            propertyCache.Clear();
        }
        /// <summary>
        /// When a text changes, it might be required to filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the textbox
            TextBox filterTextBox = e.OriginalSource as TextBox;
            // Get the header of the textbox
            DataGridColumnHeader header =
              TryFindParent<DataGridColumnHeader>(filterTextBox);
            if (header != null)
            {
                UpdateFilter(filterTextBox, header);
                ApplyFilters();
            }
        }
        /// <summary>
        /// Update the internal filter
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="header"></param>
        private void UpdateFilter(TextBox textBox, DataGridColumnHeader header)
        {
            // Try to get the property bound to the column.
            // This should be stored as datacontext.
            string columnBinding = header.DataContext != null ?
                                        header.DataContext.ToString() : "";
            if (columnBinding == "Name") columnBinding = "PokemonName";
            // Set the filter 
            if (!String.IsNullOrEmpty(columnBinding))
                columnFilters[columnBinding] = textBox.Text;
        }
        /// <summary>
        /// Apply the filters
        /// </summary>
        /// <param name="border"></param>
        private void ApplyFilters()
        {
            // Get the view
            ICollectionView view = CollectionViewSource.GetDefaultView(ItemsSource);
            if (view != null)
            {
                // Create a filter
                view.Filter = delegate (object item)
                {
                    // Show the current object
                    bool show = true;
                    // Loop filters
                    foreach (KeyValuePair<string, string> filter in columnFilters)
                    {
                        object property = GetPropertyValue(item, filter.Key);
                        if (property != null)
                        {
                            // Check if the current column contains a filter
                            bool containsFilter = false;
                            if (IsFilteringCaseSensitive)
                                containsFilter = property.ToString().Contains(filter.Value);
                            else
                                containsFilter =
                                  property.ToString().ToLower().Contains(filter.Value.ToLower());
                            // Do the necessary things if the filter is not correct
                            if (!containsFilter)
                            {
                                show = false;
                                break;
                            }
                        }
                    }
                    // Return if it's visible or not
                    return show;
                };
            }
        }
        /// <summary>
        /// Get the value of a property
        /// </summary>
        /// <param name="item"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private object GetPropertyValue(object item, string property)
        {
            // No value
            object value = null;
            // Get property  from cache
            PropertyInfo pi = null;
            if (propertyCache.ContainsKey(property))
                pi = propertyCache[property];
            else
            {
                pi = item.GetType().GetProperty(property);
                propertyCache.Add(property, pi);
            }
            // If we have a valid property, get the value
            if (pi != null)
                value = pi.GetValue(item, null);
            // Done
            return value;
        }
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect
        /// child of the queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found,
        /// a null reference is being returned.</returns>
        public static T TryFindParent<T>(DependencyObject child)
          where T : DependencyObject
        {
#pragma warning disable IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
            //get parent item
            DependencyObject parentObject = GetParentObject(child);
            //we've reached the end of the tree
            if (parentObject == null) return null;
            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
#pragma warning restore IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
        }
        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Do note, that for content element,
        /// this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise null.</returns>
        public static DependencyObject GetParentObject(DependencyObject child)
        {
#pragma warning disable IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
            if (child == null) return null;
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;
                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }
            // If it's not a ContentElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
#pragma warning restore IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
        }
    }
}
