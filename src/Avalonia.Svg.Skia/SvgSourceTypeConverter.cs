﻿using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Platform;

namespace Avalonia.Svg.Skia;

/// <summary>
/// Represents a <see cref="SvgSource"/> type converter.
/// </summary>
public class SvgSourceTypeConverter : TypeConverter
{
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string);
    }

    /// <inheritdoc/>
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        var path = (string)value;
        var baseUri = context?.GetContextBaseUri();
        return SvgSource.Load<SvgSource>(path, baseUri);
    }
}
