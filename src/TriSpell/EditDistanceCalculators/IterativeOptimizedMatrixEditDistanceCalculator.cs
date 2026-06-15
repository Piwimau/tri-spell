using System;

namespace TriSpell.EditDistanceCalculators;

/// <summary>
/// Represents an even more advanced, iterative implementation of an
/// <see cref="IEditDistanceCalculator"/>.
/// </summary>
/// <remarks>
/// This implementation builds upon the fundamental algorithm of the
/// <see cref="IterativeFullMatrixEditDistanceCalculator"/>. However, it makes
/// use of an important observation: For calculating the edit distance at a
/// certain position in the matrix, it is sufficient to consider only the
/// current and previous row of edit distances. By reducing the full matrix down
/// to only two rows, the memory footprint and runtime can be improved.
/// </remarks>
internal sealed class IterativeOptimizedMatrixEditDistanceCalculator
    : IEditDistanceCalculator {

    /// <summary>
    /// Maximum limit for allocating the distances rows on the stack.
    /// </summary>
    private const int MaxStackAllocLimit = 128;

    /// <inheritdoc/>
    public int EditDistance(
        ReadOnlySpan<char> source,
        ReadOnlySpan<char> target
    ) {
        int size = target.Length + 1;
        Span<int> prevDists = (size <= MaxStackAllocLimit)
            ? stackalloc int[size]
            : new int[size];
        Span<int> curDists = (size <= MaxStackAllocLimit)
            ? stackalloc int[size]
            : new int[size];
        // The empty source prefix can only be transformed into target by
        // inserting all characters.
        for (int j = 0; j < prevDists.Length; j++) {
            prevDists[j] = j;
        }
        for (int i = 0; i < source.Length; i++) {
            curDists[0] = i + 1;
            for (int j = 0; j < target.Length; j++) {
                int insert = curDists[j] + 1;
                int delete = prevDists[j + 1] + 1;
                int substitute = prevDists[j]
                    + ((source[i] == target[j]) ? 0 : 1);
                curDists[j + 1] = Math.Min(
                    Math.Min(insert, delete),
                    substitute
                );
            }
            Span<int> temp = prevDists;
            prevDists = curDists;
            curDists = temp;
        }
        return prevDists[target.Length];
    }

}