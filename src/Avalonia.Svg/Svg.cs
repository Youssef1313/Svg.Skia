﻿using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using ShimSkiaSharp;

namespace Avalonia.Svg;

/// <summary>
/// Svg control.
/// </summary>
public class Svg : Control
{
    private readonly Uri _baseUri;
    private SKPicture? _picture;
    private AvaloniaPicture? _avaloniaPicture;

    /// <summary>
    /// Defines the <see cref="Path"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> PathProperty =
        AvaloniaProperty.Register<Svg, string?>(nameof(Path));

    /// <summary>
    /// Defines the <see cref="Source"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<Svg, string?>(nameof(Source));

    /// <summary>
    /// Defines the <see cref="Stretch"/> property.
    /// </summary>
    public static readonly StyledProperty<Stretch> StretchProperty =
        AvaloniaProperty.Register<Svg, Stretch>(nameof(Stretch), Stretch.Uniform);

    /// <summary>
    /// Defines the <see cref="StretchDirection"/> property.
    /// </summary>
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        AvaloniaProperty.Register<Svg, StretchDirection>(
            nameof(StretchDirection),
            StretchDirection.Both);

    /// <summary>
    /// Gets or sets the Svg path.
    /// </summary>
    [Content]
    public string? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    /// <summary>
    /// Gets or sets the Svg source.
    /// </summary>
    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value controlling how the image will be stretched.
    /// </summary>
    public Stretch Stretch
    {
        get { return GetValue(StretchProperty); }
        set { SetValue(StretchProperty, value); }
    }

    /// <summary>
    /// Gets or sets a value controlling in what direction the image will be stretched.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get { return GetValue(StretchDirectionProperty); }
        set { SetValue(StretchDirectionProperty, value); }
    }

    /// <summary>
    /// Gets svg model.
    /// </summary>
    public SKPicture? Model => _picture;

    static Svg()
    {
        AffectsRender<Svg>(PathProperty, SourceProperty, StretchProperty, StretchDirectionProperty);
        AffectsMeasure<Svg>(PathProperty, SourceProperty, StretchProperty, StretchDirectionProperty);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Svg"/> class.
    /// </summary>
    /// <param name="baseUri">The base URL for the XAML context.</param>
    public Svg(Uri baseUri)
    {
        _baseUri = baseUri;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Svg"/> class.
    /// </summary>
    /// <param name="serviceProvider">The XAML service provider.</param>
    public Svg(IServiceProvider serviceProvider)
    {
        _baseUri = serviceProvider.GetContextBaseUri();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (_picture == null)
        {
            return new Size();
        }

        var sourceSize = _picture is { }
            ? new Size(_picture.CullRect.Width, _picture.CullRect.Height)
            : default;

        return Stretch.CalculateSize(availableSize, sourceSize, StretchDirection);

    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_picture == null)
        {
            return new Size();
        }

        var sourceSize = _picture is { }
            ? new Size(_picture.CullRect.Width, _picture.CullRect.Height)
            : default;

        return Stretch.CalculateSize(finalSize, sourceSize);

    }

    public override void Render(DrawingContext context)
    {
        if (_picture is null)
        {
            return;
        }

        var viewPort = new Rect(Bounds.Size);
        var sourceSize = new Size(_picture.CullRect.Width, _picture.CullRect.Height);
        if (sourceSize.Width <= 0 || sourceSize.Height <= 0)
        {
            return;
        }

        var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
        var scaledSize = sourceSize * scale;
        var destRect = viewPort
            .CenterRect(new Rect(scaledSize))
            .Intersect(viewPort);
        var sourceRect = new Rect(sourceSize)
            .CenterRect(new Rect(destRect.Size / scale));

        var bounds = _picture.CullRect;
        var scaleMatrix = Matrix.CreateScale(
            destRect.Width / sourceRect.Width,
            destRect.Height / sourceRect.Height);
        var translateMatrix = Matrix.CreateTranslation(
            -sourceRect.X + destRect.X - bounds.Top,
            -sourceRect.Y + destRect.Y - bounds.Left);

        using (context.PushClip(destRect))
        using (context.PushTransform(translateMatrix * scaleMatrix))
        {
            if (_avaloniaPicture is { })
            {
                _avaloniaPicture.Draw(context);
            }
        }
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PathProperty)
        {
            var path = change.GetNewValue<string?>();
            LoadFromPath(path);
            InvalidateVisual();
        }
        
        if (change.Property == SourceProperty)
        {
            var source = change.GetNewValue<string?>();
            LoadFromSource(source);
            InvalidateVisual();
        }
    }

    private void LoadFromPath(string? path, Dictionary<string, string>? entities = null)
    {
        _picture = default;
        _avaloniaPicture?.Dispose();

        if (path is not null)
        {
            _picture = SvgSource.LoadPicture(path, _baseUri, entities);
            if (_picture is { })
            {
                _avaloniaPicture = AvaloniaPicture.Record(_picture);
            }
        }
    }

    private void LoadFromSource(string? source)
    {
        _picture = default;
        _avaloniaPicture?.Dispose();

        if (source is not null)
        {
            _picture = SvgSource.LoadPictureFromSvg(source);
            if (_picture is { })
            {
                _avaloniaPicture = AvaloniaPicture.Record(_picture);
            }
        }
    }
}
