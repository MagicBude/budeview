using System.Globalization;

namespace BudeView.Core;

public sealed class NaturalStringComparer : IComparer<string>
{
    public static NaturalStringComparer Instance { get; } = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var xi = 0;
        var yi = 0;

        while (xi < x.Length && yi < y.Length)
        {
            if (char.IsDigit(x[xi]) && char.IsDigit(y[yi]))
            {
                var xStart = xi;
                var yStart = yi;

                while (xi < x.Length && char.IsDigit(x[xi]))
                {
                    xi++;
                }

                while (yi < y.Length && char.IsDigit(y[yi]))
                {
                    yi++;
                }

                var xToken = x.AsSpan(xStart, xi - xStart);
                var yToken = y.AsSpan(yStart, yi - yStart);

                var xTrimmed = TrimLeadingZeroes(xToken);
                var yTrimmed = TrimLeadingZeroes(yToken);

                var lengthComparison = xTrimmed.Length.CompareTo(yTrimmed.Length);
                if (lengthComparison != 0)
                {
                    return lengthComparison;
                }

                var numericComparison = xTrimmed.CompareTo(yTrimmed, StringComparison.Ordinal);
                if (numericComparison != 0)
                {
                    return numericComparison;
                }

                var zeroComparison = xToken.Length.CompareTo(yToken.Length);
                if (zeroComparison != 0)
                {
                    return zeroComparison;
                }

                continue;
            }

            var xc = char.ToUpper(x[xi], CultureInfo.InvariantCulture);
            var yc = char.ToUpper(y[yi], CultureInfo.InvariantCulture);

            if (xc != yc)
            {
                return xc.CompareTo(yc);
            }

            xi++;
            yi++;
        }

        return x.Length.CompareTo(y.Length);
    }

    private static ReadOnlySpan<char> TrimLeadingZeroes(ReadOnlySpan<char> value)
    {
        var index = 0;
        while (index < value.Length - 1 && value[index] == '0')
        {
            index++;
        }

        return value[index..];
    }
}
