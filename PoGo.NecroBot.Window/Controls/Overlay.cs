using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PoGo.NecroBot.Window.Controls
{
    [TemplatePart(Name = "PART_OverlayAdorner", Type = typeof(AdornerDecorator))]
    public class Overlay : ContentControl
    {
        public static readonly DependencyProperty OverlayContentProperty =
            DependencyProperty.Register("OverlayContent", typeof(UIElement), typeof(Overlay),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOverlayContentChanged)));

        public static readonly DependencyProperty IsOverlayContentVisibleProperty =
            DependencyProperty.Register("IsOverlayContentVisible", typeof(bool), typeof(Overlay),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsOverlayContentVisibleChanged)));

        private UIElementAdorner m_adorner;

        static Overlay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Overlay), new FrameworkPropertyMetadata(typeof(Overlay)));
        }

        [Category("Overlay")]
        public UIElement OverlayContent
        {
            get { return (UIElement)GetValue(OverlayContentProperty); }
            set { SetValue(OverlayContentProperty, value); }
        }

        [Category("Overlay")]
        public bool IsOverlayContentVisible
        {
            get { return (bool)GetValue(IsOverlayContentVisibleProperty); }
            set { SetValue(IsOverlayContentVisibleProperty, value); }
        }

        private static void OnOverlayContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#pragma warning disable IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
            Overlay overlay = d as Overlay;
            if (overlay != null)
            {
                if (overlay.IsOverlayContentVisible)
                {
                    overlay.RemoveOverlayContent();
                    overlay.AddOverlayContent();
                }
            }
#pragma warning restore IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
        }

        private static void OnIsOverlayContentVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#pragma warning disable IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
            Overlay overlay = d as Overlay;
            if (overlay != null)
            {
                if ((bool)e.NewValue)
                {
                    overlay.AddOverlayContent();
                }
                else
                {
                    overlay.RemoveOverlayContent();
                }
            }
#pragma warning restore IDE0019 // Use pattern matching - Build.Bat Error Happens if We Do
        }

        private void AddOverlayContent()
        {
            if (OverlayContent != null)
            {
                m_adorner = new UIElementAdorner(this, OverlayContent);
                m_adorner.Add();

                AdornerLayer parentAdorner = AdornerLayer.GetAdornerLayer(this);
                parentAdorner.Add(m_adorner);
            }
        }

        private void RemoveOverlayContent()
        {
            if (m_adorner != null)
            {
                AdornerLayer parentAdorner = AdornerLayer.GetAdornerLayer(this);
                parentAdorner.Remove(m_adorner);

                m_adorner.Remove();
                m_adorner = null;
            }
        }

        #region Class UIElementAdorner

        private class UIElementAdorner : Adorner
        {
            private List<UIElement> m_logicalChildren;
            private UIElement m_element;

            public UIElementAdorner(UIElement adornedElement, UIElement element)
                : base(adornedElement)
            {
                m_element = element;
            }

            public void Add()
            {
                AddLogicalChild(m_element);
                AddVisualChild(m_element);
            }

            public void Remove()
            {
                RemoveLogicalChild(m_element);
                RemoveVisualChild(m_element);
            }

            protected override Size MeasureOverride(Size constraint)
            {
                m_element.Measure(constraint);
                return m_element.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                Point location = new Point(0, 0);
                Rect rect = new Rect(location, finalSize);
                m_element.Arrange(rect);
                return finalSize;
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            protected override Visual GetVisualChild(int index)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException("index");

                return m_element;
            }

            protected override IEnumerator LogicalChildren
            {
                get
                {
                    if (m_logicalChildren == null)
                    {
                        m_logicalChildren = new List<UIElement>
                        {
                            m_element
                        };
                    }

                    return m_logicalChildren.GetEnumerator();
                }
            }
        }

        #endregion
    }
}
