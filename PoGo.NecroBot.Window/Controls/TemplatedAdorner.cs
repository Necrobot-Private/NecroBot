using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace PoGo.NecroBot.Window.Controls
{
    public class TemplatedAdorner : Adorner
    {
        private ContentPresenter ContentPresenter { get; set; }

        #region Constructors

        public TemplatedAdorner(
                UIElement adornedElement,
                DataTemplate contentDataTemplate, object content) : base(adornedElement)
        {
            ContentPresenter =
                    new ContentPresenter
                    {
                        ContentTemplate = contentDataTemplate,
                        Content = content,
                        DataContext = content
                    };

            var adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            adornerLayer.Add(this);
        }

        #endregion Constructors

        #region Overridden

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return ContentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ContentPresenter.Arrange(new Rect(new Point(0, 0), finalSize));
            return finalSize;
        }

        #endregion Overridden
    }
}
