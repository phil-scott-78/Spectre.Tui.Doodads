namespace Spectre.Tui.Doodads.Layout;

/// <summary>
/// Distributes available space among flex items according to their sizing strategies.
/// </summary>
internal static class FlexAlgorithm
{
    /// <summary>
    /// Computes the allocated size for each item given the total available space and gap.
    /// </summary>
    /// <param name="totalSpace">The total available space in terminal cells.</param>
    /// <param name="gap">The gap between items in terminal cells.</param>
    /// <param name="items">The flex items to distribute space among.</param>
    /// <param name="minSizes">The minimum size of each item (must match item count).</param>
    /// <returns>An array of allocated sizes, one per item, that sum to the usable space.</returns>
    internal static int[] Distribute(
        int totalSpace,
        int gap,
        ImmutableArray<FlexItem> items,
        IReadOnlyList<int> minSizes)
    {
        if (items.IsEmpty)
        {
            return [];
        }

        if (minSizes.Count != items.Length)
        {
            throw new ArgumentException("minSizes must have the same length as items.", nameof(minSizes));
        }

        var count = items.Length;
        var result = new int[count];

        // Subtract gap space
        var totalGap = gap * (count - 1);
        var available = Math.Max(0, totalSpace - totalGap);
        if (available == 0)
        {
            return result;
        }

        // First pass: apply minimums
        var minTotal = 0;

        for (var i = 0; i < count; i++)
        {
            var min = Math.Max(0, minSizes[i]);
            result[i] = min;
            minTotal += min;
        }

        // If minimums exceed available space, scale them down proportionally.
        if (minTotal >= available)
        {
            if (minTotal > available && minTotal > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var min = Math.Max(0, minSizes[i]);
                    result[i] = (int)((long)min * available / minTotal);
                }

                var allocated = result.Sum();
                var remainder = available - allocated;
                for (var i = count - 1; i >= 0 && remainder > 0; i--)
                {
                    if (minSizes[i] > 0)
                    {
                        result[i]++;
                        remainder--;
                    }
                }
            }

            return result;
        }

        // Second pass: distribute remaining space to ratio-sized items.
        var remaining = available - minTotal;
        var ratioTotal = 0;

        for (var i = 0; i < count; i++)
        {
            if (items[i].Size is FlexSize.RatioSize r)
            {
                ratioTotal += r.Weight;
            }
        }

        if (ratioTotal == 0)
        {
            return result;
        }

        for (var i = 0; i < count; i++)
        {
            if (items[i].Size is FlexSize.RatioSize r)
            {
                result[i] += (int)((long)remaining * r.Weight / ratioTotal);
            }
        }

        var totalAllocated = result.Sum();
        var roundingRemainder = available - totalAllocated;
        for (var i = count - 1; i >= 0 && roundingRemainder > 0; i--)
        {
            if (items[i].Size is FlexSize.RatioSize)
            {
                result[i]++;
                roundingRemainder--;
            }
        }

        return result;
    }
}