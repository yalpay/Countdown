﻿using Countdown.ViewModels;

namespace Countdown.Views;

internal sealed partial class Clock : UserControl
{
    private static CompositionClock? sCompositionClock;
    private static readonly AudioHelper sAudioHelper = new AudioHelper();

    public Clock()
    {
        this.InitializeComponent();

        Loaded += (s, e) =>
        {
            Clock xamlClock = (Clock)s;

            if (sCompositionClock is null)
            {
                sCompositionClock = new CompositionClock(xamlClock);
                State = StopwatchState.AtStart;
            }
            else
                sCompositionClock!.XamlClock = xamlClock;

            ElementCompositionPreview.SetElementChildVisual(xamlClock, sCompositionClock.Visual);
        };

        Unloaded += (s, e) =>
        {
            // remove the visual, there is no reference counting of visuals
            ElementCompositionPreview.SetElementChildVisual((Clock)s, null);
        };
    }

    public StopwatchState State
    {
        get { return (StopwatchState)GetValue(StateProperty); }
        set { SetValue(StateProperty, value); }
    }

    public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State),
            typeof(StopwatchState),
            typeof(Clock),
            new PropertyMetadata(StopwatchState.Undefined, StatePropertyChanged));

    private static void StatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (sCompositionClock is null || !ReferenceEquals(d, sCompositionClock.XamlClock))
            return;
        
        StopwatchState oldState = (StopwatchState)e.OldValue;
        StopwatchState newState = (StopwatchState)e.NewValue;

        if (oldState == StopwatchState.Undefined) // a new page has been loaded
            return;

        if (oldState == newState) // bindings have been re-evaluated
            return;

        switch (newState)
        {
            case StopwatchState.Running:
                {
                    sCompositionClock.Animations.StartForwardAnimations();
                    sAudioHelper.Start();
                    break;
                }
            
            case StopwatchState.Stopped: // the user halted the countdown 
                {
                    sCompositionClock.Animations.StopAnimations();
                    sAudioHelper.Stop();
                    break;
                }
                
            case StopwatchState.Rewinding:
                {
                    sCompositionClock.Animations.StartRewindAnimations();
                    break;
                }

            case StopwatchState.Completed: // let the audio play out, it's not synchronized with the animation
            case StopwatchState.AtStart:
            case StopwatchState.Initializing: break;

            default: throw new Exception($"invalid state: {newState}");
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return CompositionClock.ContainerSize.ToSize();
    }

    public Color FaceColour
    {
        get { return (Color)GetValue(FaceColourProperty); }
        set { SetValue(FaceColourProperty, value); }
    }

    public static readonly DependencyProperty FaceColourProperty =
        DependencyProperty.Register(nameof(FaceColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.Face, (Color)e.NewValue)));

    public Color TickTrailColour
    {
        get { return (Color)GetValue(TickTrailColourProperty); }
        set { SetValue(TickTrailColourProperty, value); }
    }

    public static readonly DependencyProperty TickTrailColourProperty =
        DependencyProperty.Register(nameof(TickTrailColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.TickTrail, (Color)e.NewValue)));

    public Color InnerFrameColour
    {
        get { return (Color)GetValue(InnerFrameColourProperty); }
        set { SetValue(InnerFrameColourProperty, value); }
    }

    public static readonly DependencyProperty InnerFrameColourProperty =
        DependencyProperty.Register(nameof(InnerFrameColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.InnerFrame, (Color)e.NewValue)));

    public Color OuterFrameColour
    {
        get { return (Color)GetValue(OuterFrameColourProperty); }
        set { SetValue(OuterFrameColourProperty, value); }
    }

    public static readonly DependencyProperty OuterFrameColourProperty =
        DependencyProperty.Register(nameof(OuterFrameColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.OuterFrame, (Color)e.NewValue)));

    public Color FrameTickColour
    {
        get { return (Color)GetValue(FrameTickColourProperty); }
        set { SetValue(FrameTickColourProperty, value); }
    }

    public static readonly DependencyProperty FrameTickColourProperty =
        DependencyProperty.Register(nameof(FrameTickColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.FrameTick, (Color)e.NewValue)));

    public Color TickMarksColour
    {
        get { return (Color)GetValue(TickMarksColourProperty); }
        set { SetValue(TickMarksColourProperty, value); }
    }

    public static readonly DependencyProperty TickMarksColourProperty =
        DependencyProperty.Register(nameof(TickMarksColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.TickMarks, (Color)e.NewValue)));

    public Color HandStrokeColour
    {
        get { return (Color)GetValue(HandStrokeColourProperty); }
        set { SetValue(HandStrokeColourProperty, value); }
    }

    public static readonly DependencyProperty HandStrokeColourProperty =
        DependencyProperty.Register(nameof(HandStrokeColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.HandStroke, (Color)e.NewValue)));

    public Color HandFillColour
    {
        get { return (Color)GetValue(HandFillColourProperty); }
        set { SetValue(HandFillColourProperty, value); }
    }

    public static readonly DependencyProperty HandFillColourProperty =
        DependencyProperty.Register(nameof(HandFillColour),
            typeof(Color),
            typeof(Clock),
            new PropertyMetadata(Colors.Transparent, (d, e) => UpdateBrush(BrushId.HandFill, (Color)e.NewValue)));

    private static void UpdateBrush(BrushId brushIndex, Color newColour)
    {
        if (sCompositionClock is null)
            return;

        CompositionColorBrush brush = sCompositionClock.Brushes[brushIndex];

        if (brush is not null && (brush.Color != newColour))
            brush.Color = newColour;
    }

    public bool IsDropShadowVisible
    {
        get { return (bool)GetValue(IsDropShadowVisibleProperty); }
        set { SetValue(IsDropShadowVisibleProperty, value); }
    }

    public static readonly DependencyProperty IsDropShadowVisibleProperty =
        DependencyProperty.Register(nameof(IsDropShadowVisible),
            typeof(bool),
            typeof(Clock),
            new PropertyMetadata(true, (d, e) => UpdateDropShadowVisibility((bool)e.NewValue)));

    private static void UpdateDropShadowVisibility(bool isVisible)
    {
        if (sCompositionClock is null)
            return;

        if (sCompositionClock.IsDropShadowVisible != isVisible)
            sCompositionClock.IsDropShadowVisible = isVisible;
    }


    private sealed class CompositionClock
    {
        public ContainerVisual Visual { get; }
        public static Vector2 ContainerSize { get; } = new Vector2(200);
        public AnimationList Animations { get; }
        public BrushList Brushes { get; }
        public Clock XamlClock { get; set; }
        private SpriteVisual DropShadowVisual { get; }

        public CompositionClock(Clock xamlClock)
        {
            Compositor compositor = App.MainWindow!.Compositor;
            Visual = compositor.CreateContainerVisual();
            Animations = new AnimationList(compositor);
            Brushes = new BrushList(compositor, xamlClock);
            XamlClock = xamlClock;

            // allow room for the drop shadow
            float clockSize = ContainerSize.X * 0.95f;
            Vector2 center = new Vector2(ContainerSize.X * 0.5f);

            DropShadowVisual = CreateFace(compositor, center, clockSize);
            DropShadowVisual.IsVisible = xamlClock.IsDropShadowVisible;

            CreateTickTrail(compositor, center, clockSize);
            CreateFaceTickMarks(compositor, center, clockSize);
            CreateHand(compositor, center, clockSize);
        }

        private const float cOuterFrameStrokePercentage = 0.01f;
        private const float cInnerFrameStrokePercentage = 0.02f;

        private const float cTickMarksStrokePercentage = 0.01f;
        private const float cTickMarksOuterRadiusPercentage = 0.83f;
        private const float cTickMarksInnerRadiusPercentage = 0.74f;

        private const float cHandStrokePercentage = 0.01f;
        private const float cHandTipRadiusPercentage = 0.90f;
        private const float cHandSectorAngle = 25.0f;
        private const float cHandArcRadiusPercentage = 0.055f;

        private const float cTickTrailOuterRadiusPercent = 0.92f;
        private const float cTickTrailInnerRadiusPercent = 0.38f;

        private void CreateTickTrail(Compositor compositor, Vector2 center, float clockSize)
        {
            float radius = clockSize * 0.5f;
            float outerRadius = radius * cTickTrailOuterRadiusPercent;
            float innerRadius = radius * cTickTrailInnerRadiusPercent;

            const float startAngle = 179.5f;
            const float endAngle = 174.5f;

            Vector2 topLeft = VectorToCartesian(outerRadius, startAngle, center);
            Vector2 topRight = VectorToCartesian(outerRadius, endAngle, center);
            Vector2 bottomLeft = VectorToCartesian(innerRadius, startAngle, center);
            Vector2 bottomRight = VectorToCartesian(innerRadius, endAngle, center);

            using CanvasPathBuilder builder = new CanvasPathBuilder(null);

            builder.BeginFigure(topLeft);
            builder.AddArc(topRight, outerRadius, outerRadius, 0f, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
            builder.AddLine(bottomRight);
            builder.AddLine(bottomLeft);
            builder.EndFigure(CanvasFigureLoop.Closed);

            // create a composition geometry from the canvas path data
            using CanvasGeometry canvasGeometry = CanvasGeometry.CreatePath(builder);
            CompositionPathGeometry pathGeometry = compositor.CreatePathGeometry();
            pathGeometry.Path = new CompositionPath(canvasGeometry);

            // create every trail element visual, then change its opacity as required
            for (int segment = 0; segment < 30; segment++)
            {
                // create a shape from the geometry
                CompositionSpriteShape tickSegment = compositor.CreateSpriteShape(pathGeometry);
                tickSegment.FillBrush = Brushes[BrushId.TickTrail];
                tickSegment.CenterPoint = center;
                tickSegment.RotationAngleInDegrees = segment * 6f;  // one second is 6 degrees

                // create a visual for the shape
                ShapeVisual shapeVisual = compositor.CreateShapeVisual();
                shapeVisual.Size = ContainerSize;
                shapeVisual.Shapes.Add(tickSegment);
                shapeVisual.Opacity = 0.0f;

                Animations.AddTickTrailSegment(shapeVisual, segment);

                Visual.Children.InsertAtTop(shapeVisual);
            }
        }

        private SpriteVisual CreateFace(Compositor compositor, Vector2 center, float clockSize)
        {
            CompositionSpriteShape CreateCircle(float radius, float stroke, Vector2 offset, BrushId fillBrushId, BrushId strokeBrushId)
            {
                CompositionEllipseGeometry circleGeometry = compositor.CreateEllipseGeometry();
                circleGeometry.Radius = new Vector2(radius);

                CompositionSpriteShape circleShape = compositor.CreateSpriteShape(circleGeometry);
                circleShape.Offset = offset;
                circleShape.FillBrush = Brushes[fillBrushId];

                if (stroke > 0.0f)
                {
                    circleShape.StrokeThickness = stroke;
                    circleShape.StrokeBrush = Brushes[strokeBrushId];
                }

                return circleShape;
            }

            CompositionContainerShape shapeContainer = compositor.CreateContainerShape();

            // create a visual for the shape container
            ShapeVisual shapeVisual = compositor.CreateShapeVisual();
            shapeVisual.Size = ContainerSize;
            shapeVisual.Shapes.Add(shapeContainer);

            // outer frame
            float radius = clockSize * 0.5f;
            shapeContainer.Shapes.Add(CreateCircle(radius, 0f, center, BrushId.OuterFrame, BrushId.OuterFrame));

            // create the drop shadow now from the simplest shape
            SpriteVisual shadowVisual = CreateDropShadow(compositor, shapeVisual);

            // inner frame
            float outerFrameStroke = clockSize * cOuterFrameStrokePercentage;
            float innerFrameStroke = clockSize * cInnerFrameStrokePercentage;
            radius -= outerFrameStroke + (innerFrameStroke * 0.5f);

            shapeContainer.Shapes.Add(CreateCircle(radius, innerFrameStroke, center, BrushId.Transparent, BrushId.InnerFrame));

            // tick marks around inner frame
            float tickRadius = innerFrameStroke / 5.0f;      // TODO constants, and color static

            for (float degrees = 0; degrees < 360.0; degrees += 30.0f)
                shapeContainer.Shapes.Add(CreateCircle(tickRadius, 0f, VectorToCartesian(radius, degrees, center), BrushId.FrameTick, BrushId.Transparent));

            // clock face fill
            radius -= innerFrameStroke * 0.5f;
            shapeContainer.Shapes.Add(CreateCircle(radius, 0f, center, BrushId.Face, BrushId.Transparent));

            // insert into the tree 
            Visual.Children.InsertAtBottom(shapeVisual);
            Visual.Children.InsertAtBottom(shadowVisual);

            return shadowVisual;
        }

        private static SpriteVisual CreateDropShadow(Compositor compositor, ShapeVisual sourceVisual)
        {
            // create a surface brush to use as a mask for the drop shadow
            CompositionVisualSurface surface = compositor.CreateVisualSurface();
            surface.SourceSize = ContainerSize;
            surface.SourceVisual = sourceVisual;

            // create the drop shadow
            DropShadow shadow = compositor.CreateDropShadow();
            shadow.Mask = compositor.CreateSurfaceBrush(surface);
            shadow.Offset = new Vector3(new Vector2(1.5f), 0f);
            shadow.Color = Colors.DimGray;

            // create a visual for the shadow
            SpriteVisual shadowVisual = compositor.CreateSpriteVisual();
            shadowVisual.Size = ContainerSize;
            shadowVisual.Shadow = shadow;

            return shadowVisual;
        }

        private void CreateFaceTickMarks(Compositor compositor, Vector2 center, float clockSize)
        {
            CompositionSpriteShape CreateLine(Vector2 start, Vector2 end, float thickness, BrushId brushId, CompositionStrokeCap endCap)
            {
                CompositionLineGeometry lineGeometry = compositor.CreateLineGeometry();
                lineGeometry.Start = start;
                lineGeometry.End = end;

                CompositionSpriteShape lineShape = compositor.CreateSpriteShape(lineGeometry);
                lineShape.StrokeThickness = thickness;
                lineShape.StrokeBrush = Brushes[brushId];
                lineShape.StrokeEndCap = endCap;
                lineShape.StrokeStartCap = endCap;

                return lineShape;
            }

            CompositionContainerShape shapeContainer = compositor.CreateContainerShape();

            // add the 5 second tick marks
            float stroke = clockSize * cTickMarksStrokePercentage;
            float startLength = clockSize * 0.5f * cTickMarksInnerRadiusPercentage;
            float endLength = clockSize * 0.5f * cTickMarksOuterRadiusPercentage;
            CompositionStrokeCap endCap = CompositionStrokeCap.Round;

            for (int degrees = 30; degrees < 360; degrees += 30)
            {
                if (degrees % 90 > 0)
                {
                    Vector2 inner = VectorToCartesian(startLength, degrees, center);
                    Vector2 outer = VectorToCartesian(endLength, degrees, center);
                    shapeContainer.Shapes.Add(CreateLine(inner, outer, stroke, BrushId.TickMarks, endCap));
                }
            }

            // now the cross hairs
            float radius = (clockSize * 0.5f) - (clockSize * (cOuterFrameStrokePercentage + cInnerFrameStrokePercentage));

            // horizontal cross hair
            Vector2 start = new Vector2(center.X - radius, center.Y);
            Vector2 end = new Vector2(center.X + radius, center.Y);
            shapeContainer.Shapes.Add(CreateLine(start, end, stroke, BrushId.TickMarks, CompositionStrokeCap.Flat));

            // vertical cross hair
            start = new Vector2(center.X, center.Y - radius);
            end = new Vector2(center.X, center.Y + radius);
            shapeContainer.Shapes.Add(CreateLine(start, end, stroke, BrushId.TickMarks, CompositionStrokeCap.Flat));

            // create a visual for the shapes
            ShapeVisual shapeVisual = compositor.CreateShapeVisual();
            shapeVisual.Size = ContainerSize;
            shapeVisual.Shapes.Add(shapeContainer);

            // insert into tree
            Visual.Children.InsertAtTop(shapeVisual);
        }

        private void CreateHand(Compositor compositor, Vector2 center, float clockSize)
        {
            using CanvasPathBuilder builder = new CanvasPathBuilder(null);

            Vector2 tip = VectorToCartesian(clockSize * 0.5f * cHandTipRadiusPercentage, 0.0f, center);
            builder.BeginFigure(tip);

            float radius = clockSize * cHandArcRadiusPercentage;

            Vector2 arcStartPoint = VectorToCartesian(radius, -cHandSectorAngle, center);
            builder.AddLine(arcStartPoint);

            Vector2 arcEndPoint = VectorToCartesian(radius, cHandSectorAngle, center);
            builder.AddArc(arcEndPoint, radius, radius, 0f, CanvasSweepDirection.Clockwise, CanvasArcSize.Large);
            builder.EndFigure(CanvasFigureLoop.Closed);

            float handStroke = clockSize * cHandStrokePercentage;

            // create a composition geometry from the canvas path data
            using CanvasGeometry canvasGeometry = CanvasGeometry.CreatePath(builder);
            CompositionPathGeometry pathGeometry = compositor.CreatePathGeometry();
            pathGeometry.Path = new CompositionPath(canvasGeometry);

            // create a shape from the geometry
            CompositionSpriteShape secondHand = compositor.CreateSpriteShape(pathGeometry);
            secondHand.FillBrush = Brushes[BrushId.HandFill];
            secondHand.StrokeThickness = handStroke;
            secondHand.StrokeLineJoin = CompositionStrokeLineJoin.Round;
            secondHand.StrokeBrush = Brushes[BrushId.HandStroke];

            // create a visual for the shape
            ShapeVisual shapeVisual = compositor.CreateShapeVisual();
            shapeVisual.Size = ContainerSize;
            shapeVisual.Shapes.Add(secondHand);
            shapeVisual.CenterPoint = new Vector3(center, 0f);
            shapeVisual.RotationAngleInDegrees = 180.0f;

            Animations.AddHand(shapeVisual);

            // add to visual tree
            Visual.Children.InsertAtTop(shapeVisual);
        }

        internal bool IsDropShadowVisible
        {
            set => DropShadowVisual.IsVisible = value;
            get => DropShadowVisual.IsVisible;
        }
    }

    private sealed class AnimationList
    {
        private const float cOneDegreeTime = 1.0f / 180.0f;
        private readonly (Visual visual, KeyFrameAnimation animation)[] list = new (Visual visual, KeyFrameAnimation animation)[31];

        private readonly Compositor compositor;
        private readonly LinearEasingFunction linearEasingFunction;
        private CompositionScopedBatch? batch;

        public AnimationList(Compositor compositor)
        {
            this.compositor = compositor;
            linearEasingFunction = compositor.CreateLinearEasingFunction();
        }

        public void AddHand(Visual visual)
        {
            ScalarKeyFrameAnimation animation = compositor.CreateScalarKeyFrameAnimation();

            animation.InsertKeyFrame(0.0f, 180.0f);
            animation.InsertKeyFrame(1.0f, 360.0f, linearEasingFunction);
            animation.Target = nameof(visual.RotationAngleInDegrees);

            list[0].visual = visual;
            list[0].animation = animation;
        }

        public void AddTickTrailSegment(Visual visual, int index)
        {
            // switch segment zero on at 5.5 degrees, the next at 11.5, then 17.5 etc.
            float onTime = (6.0f * ++index * cOneDegreeTime) - (cOneDegreeTime / 2.0f);

            ScalarKeyFrameAnimation animation = compositor.CreateScalarKeyFrameAnimation();

            animation.InsertKeyFrame(0.0f, 0.0f);
            animation.InsertKeyFrame(onTime - 0.001f, 0.0f);
            animation.InsertKeyFrame(onTime, 1.0f, linearEasingFunction);
            animation.InsertKeyFrame(1.0f, 1.0f);
            animation.Target = nameof(visual.Opacity);

            list[index].visual = visual;
            list[index].animation = animation;
        }

        public void StartForwardAnimations()
        {
            if (batch is not null)
                batch.Completed -= Batch_Completed;

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Batch_Completed;

            for (int index = 0; index < list.Length; index++)
            {
                list[index].animation.Direction = AnimationDirection.Normal;
                list[index].animation.Duration = TimeSpan.FromSeconds(30.0);

                list[index].visual.StartAnimation(list[index].animation.Target, list[index].animation);
            }

            batch.End();
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            try
            {
                if (sCompositionClock is null)
                    return;

                if (sCompositionClock.XamlClock.State == StopwatchState.Running)
                    sCompositionClock.XamlClock.State = StopwatchState.Completed;

                else if (sCompositionClock.XamlClock.State == StopwatchState.Rewinding)
                    sCompositionClock.XamlClock.State = StopwatchState.AtStart;
            }
            catch (Exception ex)
            {
                // if the app is shutting down, try to fail gracefully
                Debug.WriteLine(ex.ToString());
            }
        }

        public void StopAnimations()
        {
            for (int index = 0; index < list.Length; index++)
                list[index].visual.StopAnimation(list[index].animation.Target);
        }

        public void StartRewindAnimations()
        {
            if (batch is not null)
                batch.Completed -= Batch_Completed;

            batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += Batch_Completed;

            float startPoint = 1.0f - (cOneDegreeTime * (list[0].visual.RotationAngleInDegrees - 180.0f));

            for (int index = 0; index < list.Length; index++)
            {
                list[index].animation.Direction = AnimationDirection.Reverse;
                list[index].animation.Duration = TimeSpan.FromSeconds(2);

                list[index].visual.StartAnimation(list[index].animation.Target, list[index].animation);

                AnimationController? ac = list[index].visual.TryGetAnimationController(list[index].animation.Target);

                if (ac is not null)
                    ac.Progress = startPoint;
            }

            batch.End();
        }
    }

    private enum BrushId { Transparent, Face, TickTrail, InnerFrame, OuterFrame, FrameTick, TickMarks, HandStroke, HandFill }

    private sealed class BrushList
    {
        private readonly CompositionColorBrush[] list = new CompositionColorBrush[9];

        public BrushList(Compositor compositor, Clock xamlClock)
        {
            this[BrushId.Transparent] = compositor.CreateColorBrush(Colors.Transparent);
            this[BrushId.Face] = compositor.CreateColorBrush(xamlClock.FaceColour);
            this[BrushId.TickTrail] = compositor.CreateColorBrush(xamlClock.TickTrailColour);

            this[BrushId.InnerFrame] = compositor.CreateColorBrush(xamlClock.InnerFrameColour);
            this[BrushId.OuterFrame] = compositor.CreateColorBrush(xamlClock.OuterFrameColour);
            this[BrushId.FrameTick] = compositor.CreateColorBrush(xamlClock.FrameTickColour);

            this[BrushId.TickMarks] = compositor.CreateColorBrush(xamlClock.TickMarksColour);
            this[BrushId.HandStroke] = compositor.CreateColorBrush(xamlClock.HandStrokeColour);
            this[BrushId.HandFill] = compositor.CreateColorBrush(xamlClock.HandFillColour);
        }

        public CompositionColorBrush this[BrushId i]
        {
            get => list[(int)i];
            private set => list[(int)i] = value;
        }
    }

    private static Vector2 VectorToCartesian(float length, float angle, Vector2 offset)
    {
        (float sin, float cos) = MathF.SinCos(angle * (MathF.PI / 180.0f));

        return new Vector2(MathF.FusedMultiplyAdd(length, sin, offset.X),
                            MathF.FusedMultiplyAdd(length, cos, offset.Y));
    }
}
