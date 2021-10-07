﻿using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Countdown.Views
{
    [Microsoft.UI.Xaml.Markup.ContentProperty(Name = "Children")]
    public sealed partial class GroupBorder : UserControl
    {
        private readonly Compositor compositor;
        private readonly ContainerVisual containerVisual;

        public GroupBorder()
        {
            this.InitializeComponent();

            compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            containerVisual = compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(this, containerVisual);

            // offset the text block from the control edge
            HeadingPresenter.Margin = new Thickness(HeadingMargin, 0, 0, 0);

            // the padding is dependent on the corner radius
            ChildPresenter.Padding = CalculateContentPresenterPadding();

            // reuse the following properties to define the group border
            RegisterPropertyChangedCallback(CornerRadiusProperty, CornerRadiusPropertyChanged);
            RegisterPropertyChangedCallback(BorderThicknessProperty, BorderThicknessPropertyChanged);
            RegisterPropertyChangedCallback(FontSizeProperty, FontPropertyChanged);
            RegisterPropertyChangedCallback(FontFamilyProperty, FontPropertyChanged);
            RegisterPropertyChangedCallback(FontWeightProperty, FontPropertyChanged);
            RegisterPropertyChangedCallback(FontStyleProperty, FontPropertyChanged);
            RegisterPropertyChangedCallback(FontStretchProperty, FontPropertyChanged);

            Loaded += (s, e) =>
            {
                HeadingPresenter.SizeChanged += (s, e) => RedrawBorder();
                SizeChanged += (s, e) => RedrawBorder();

                RedrawBorder(); // first draw
            };
        }

        private void RedrawBorder()
        {
            if (IsLoaded)
            {
                containerVisual.Children.RemoveAll();
                containerVisual.Children.InsertAtBottom(CreateBorderRoundedRect());
            }
        }

        private void CornerRadiusPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            Thickness newPadding = CalculateContentPresenterPadding();

            if (ChildPresenter.Padding != newPadding)
                ChildPresenter.Padding = newPadding;
            else
                RedrawBorder();
        }

        private Thickness CalculateContentPresenterPadding()
        {
            // a non uniform corner radius is unlikely, but possible
            const double inset = 1.0;
            return new Thickness(Math.Max(CornerRadius.TopLeft, CornerRadius.BottomLeft) + inset,
                                        Math.Max(CornerRadius.TopLeft, CornerRadius.TopRight) + inset,
                                        Math.Max(CornerRadius.TopRight, CornerRadius.BottomRight) + inset,
                                        Math.Max(CornerRadius.BottomLeft, CornerRadius.BottomRight) + inset);
        }

        private void BorderThicknessPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            // non uniform border thicknesses aren't supported
            Debug.Assert(BorderThickness == new Thickness(BorderThickness.Left));
            RedrawBorder();
        }

        private void FontPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            HeadingPresenter.FontFamily = FontFamily;
            HeadingPresenter.FontSize = FontSize;
            HeadingPresenter.FontStyle = FontStyle;
            HeadingPresenter.FontWeight = FontWeight;
            HeadingPresenter.FontStretch = FontStretch;
        }

        public static readonly DependencyProperty ChildrenProperty =
            DependencyProperty.Register(nameof(Children),
            typeof(object),
            typeof(GroupBorder),
            new PropertyMetadata(null, (d, e) => ((GroupBorder)d).ChildPresenter.Content = e.NewValue));

        public object Children
        {
            get { return GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        public static readonly DependencyProperty HeadingProperty =
            DependencyProperty.Register(nameof(Heading),
            typeof(object),
            typeof(GroupBorder),
            new PropertyMetadata(null, (d, e) => ((GroupBorder)d).HeadingPresenter.Content = e.NewValue));

        public object Heading
        {
            get { return GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        public static readonly DependencyProperty BorderColourProperty =
              DependencyProperty.Register(nameof(BorderColour),
              typeof(Color),
              typeof(GroupBorder),
              new PropertyMetadata(Colors.LightGray, (d, e) => ((GroupBorder)d).RedrawBorder()));

        public Color BorderColour
        {
            get { return (Color)GetValue(BorderColourProperty); }
            set { SetValue(BorderColourProperty, value); }
        }

        public static readonly DependencyProperty HeadingBaseLineRatioProperty =
            DependencyProperty.Register(nameof(HeadingBaseLineRatio),
            typeof(double),
            typeof(GroupBorder),
            new PropertyMetadata(0.65, (d, e) => ((GroupBorder)d).RedrawBorder()));

        // How far down the heading the border line is drawn.
        // If 0.0, it'll be at the top of the content.
        // If 1.0, it would be drawn at the bottom. 
        public double HeadingBaseLineRatio
        {
            get { return (double)GetValue(HeadingBaseLineRatioProperty); }
            set { SetValue(HeadingBaseLineRatioProperty, value); }
        }

        public static readonly DependencyProperty HeadingMarginProperty =
            DependencyProperty.Register(nameof(HeadingMargin),
            typeof(double),
            typeof(GroupBorder),
            new PropertyMetadata(12.0, HeadingMarginPropertyChanged));

        // The offset from the control edge to the heading presenter. 
        public double HeadingMargin
        {
            get { return (double)GetValue(HeadingMarginProperty); }
            set { SetValue(HeadingMarginProperty, value); }
        }

        private static void HeadingMarginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GroupBorder)d).HeadingPresenter.Margin = new Thickness((double)e.NewValue, 0, 0, 0);
        }

        public static readonly DependencyProperty BorderEndPaddingProperty =
            DependencyProperty.Register(nameof(BorderEndPadding),
            typeof(double),
            typeof(GroupBorder),
            new PropertyMetadata(3.0, (d, e) => ((GroupBorder)d).RedrawBorder()));

        // Padding between the end of the border and the start of the text.
        // This affects the border, changes won't cause a new measure pass.
        public double BorderEndPadding
        {
            get { return (double)GetValue(BorderEndPaddingProperty); }
            set { SetValue(BorderEndPaddingProperty, value); }
        }

        public static readonly DependencyProperty BorderStartPaddingProperty =
            DependencyProperty.Register(nameof(BorderStartPadding),
            typeof(double),
            typeof(GroupBorder),
            new PropertyMetadata(4.0, (d, e) => ((GroupBorder)d).RedrawBorder()));

        // Padding between the start of the border and the end of the text.
        // This affects the border, changes won't cause a new measure pass.
        public double BorderStartPadding
        {
            get { return (double)GetValue(BorderStartPaddingProperty); }
            set { SetValue(BorderStartPaddingProperty, value); }
        }

        private ShapeVisual CreateBorderRoundedRect()
        {
            float borderStrokeThickness = (float)BorderThickness.Left;
            float halfStrokeThickness = borderStrokeThickness * 0.5f;

            float textRHS;
            float textLHS;

            if (FlowDirection == FlowDirection.LeftToRight)
            {
                textLHS = (float)(HeadingPresenter.Margin.Left - BorderEndPadding);
                textRHS = (float)(HeadingPresenter.Margin.Left + HeadingPresenter.ActualWidth + BorderStartPadding);
            }
            else
            {
                textLHS = (float)(ActualSize.X - (HeadingPresenter.Margin.Left + HeadingPresenter.ActualWidth + BorderStartPadding));
                textRHS = (float)(ActualSize.X - HeadingPresenter.Margin.Left + BorderEndPadding);
            }

            float headingCenter = (float)(HeadingPresenter.ActualHeight * Math.Clamp(HeadingBaseLineRatio, 0.0, 1.0));
            using CanvasPathBuilder builder = new CanvasPathBuilder(null);

            // right hand side of text
            builder.BeginFigure(textRHS, headingCenter, CanvasFigureFill.DoesNotAffectFills);

            float radius = (float)CornerRadius.TopRight;
            builder.AddLine(ActualSize.X - (radius + halfStrokeThickness), headingCenter);

            if (radius > 0) // top right corner
            {
                Vector2 arcEnd = new Vector2(ActualSize.X - halfStrokeThickness, headingCenter + radius + halfStrokeThickness);
                builder.AddArc(arcEnd, radius, radius, 0f, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            radius = (float)CornerRadius.BottomRight;
            builder.AddLine(ActualSize.X - halfStrokeThickness, ActualSize.Y - (radius + halfStrokeThickness));

            if (radius > 0) // bottom right corner
            {
                Vector2 arcEnd = new Vector2(ActualSize.X - (radius + halfStrokeThickness), ActualSize.Y - halfStrokeThickness);
                builder.AddArc(arcEnd, radius, radius, 0f, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            radius = (float)CornerRadius.BottomLeft;
            builder.AddLine(radius + halfStrokeThickness, ActualSize.Y - halfStrokeThickness);

            if (radius > 0) // bottom left corner
            {
                Vector2 arcEnd = new Vector2(halfStrokeThickness, ActualSize.Y - (radius + halfStrokeThickness));
                builder.AddArc(arcEnd, radius, radius, 0f, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }


            radius = (float)CornerRadius.TopLeft;
            builder.AddLine(halfStrokeThickness, headingCenter + radius);

            if (radius > 0) // top left corner
            {
                Vector2 arcEnd = new Vector2(radius + halfStrokeThickness, headingCenter);
                builder.AddArc(arcEnd, radius, radius, 0f, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            }

            builder.AddLine(textLHS, headingCenter);
            builder.EndFigure(CanvasFigureLoop.Open);

            // create a composition geometry from the canvas path data
            using CanvasGeometry canvasGeometry = CanvasGeometry.CreatePath(builder);
            CompositionPathGeometry pathGeometry = compositor.CreatePathGeometry();
            pathGeometry.Path = new CompositionPath(canvasGeometry);

            // create a shape from the geometry
            CompositionSpriteShape spriteShape = compositor.CreateSpriteShape(pathGeometry);
            spriteShape.StrokeThickness = borderStrokeThickness;
            spriteShape.StrokeBrush = compositor.CreateColorBrush(BorderColour);

            // create a visual for the shape
            ShapeVisual shapeVisual = compositor.CreateShapeVisual();
            shapeVisual.Size = ActualSize;
            shapeVisual.Shapes.Add(spriteShape);

            return shapeVisual;
        }
    }
}
