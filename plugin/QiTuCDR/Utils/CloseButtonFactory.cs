using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QiTuCDR.Utils
{
    internal static class CloseButtonFactory
    {
        private static Style _closeBtnStyle;

        private static Style CloseBtnStyle
        {
            get
            {
                if (_closeBtnStyle == null)
                {
                    _closeBtnStyle = BuildCloseBtnStyle();
                }
                return _closeBtnStyle;
            }
        }

        private static Style BuildCloseBtnStyle()
        {
            var style = new Style(typeof(Button));

            style.Setters.Add(new Setter(Button.WidthProperty, 24d));
            style.Setters.Add(new Setter(Button.HeightProperty, 24d));
            style.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(0)));
            style.Setters.Add(new Setter(Button.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Button.CursorProperty, System.Windows.Input.Cursors.Hand));
            style.Setters.Add(new Setter(Button.MarginProperty, new Thickness(2, 0, 2, 0)));
            style.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33))));

            var template = new ControlTemplate(typeof(Button));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            borderFactory.SetBinding(Border.WidthProperty, new Binding("Width") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.HeightProperty, new Binding("Height") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));

            template.VisualTree = borderFactory;
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            var hoverTrigger = new Trigger
            {
                Property = Button.IsMouseOverProperty,
                Value = true,
            };
            hoverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0xe8, 0x11, 0x23))));
            hoverTrigger.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            style.Triggers.Add(hoverTrigger);

            return style;
        }

        public static Button Create(Action closeAction)
        {
            var closeIcon = new Path
            {
                Width = 8,
                Height = 8,
                Stretch = Stretch.Uniform,
                StrokeThickness = 1,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Fill = Brushes.Transparent,
                Data = new GeometryGroup
                {
                    Children = new GeometryCollection
                    {
                        new PathGeometry(new PathFigureCollection
                        {
                            new PathFigure(new Point(8, 8),
                                new[] { new LineSegment(new Point(40, 40), true) }, false)
                        }),
                        new PathGeometry(new PathFigureCollection
                        {
                            new PathFigure(new Point(8, 40),
                                new[] { new LineSegment(new Point(40, 8), true) }, false)
                        }),
                    }
                }
            };

            var strokeBinding = new Binding("Foreground")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1)
            };
            closeIcon.SetBinding(Shape.StrokeProperty, strokeBinding);

            var button = new Button
            {
                Width = 24,
                Height = 24,
                Margin = new Thickness(0, 0, 12, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                Style = CloseBtnStyle,
                Content = closeIcon,
            };

            button.Click += (s, e) => closeAction();
            return button;
        }
    }
}
