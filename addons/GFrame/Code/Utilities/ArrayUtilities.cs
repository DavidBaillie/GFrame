using System.Collections.Generic;

public static class ArrayUtilities
{
    /// <summary>
    /// Determines if the provided index is valid within the array bounds
    /// </summary>
    /// <typeparam name="T">Type of array</typeparam>
    /// <param name="array">Array to check</param>
    /// <param name="index">Index to check if inside array</param>
    /// <returns>If the index is inside the array</returns>
    public static bool IndexIsInBounds<T>(this T[] array, int index)
    {
        return index > 0 && index < array.Length;
    }

    /// <summary>
    /// Determines if the provided index is valid within the array bounds
    /// </summary>
    /// <typeparam name="T">Type of array</typeparam>
    /// <param name="array">Array to check</param>
    /// <param name="indexOne">Index 1 to check if inside array</param>
    /// <param name="indexTwo">Index 2 to check if inside array</param>
    /// <returns>If the indices are inside the array</returns>
    public static bool IndexIsInBounds<T>(this T[,] array, int indexOne, int indexTwo)
    {
        return indexOne > 0 && indexTwo > 0 && indexOne < array.GetLength(0) && indexTwo < array.GetLength(1);
    }

    /// <summary>
    /// Provided an <paramref name="array"/> and an <paramref name="desiredIndex"/> to target, 
    /// returns the closest index to the target that is a valid index inside the array
    /// </summary>
    /// <typeparam name="T">Type of the array</typeparam>
    /// <param name="array">Array to check against</param>
    /// <param name="desiredIndex">Desired index to have inside the array that might be outside the bounds</param>
    /// <returns>Closest index inside the array to the <paramref name="desiredIndex"/></returns>
    public static int GetClosestIndexInBounds<T>(this T[] array, int desiredIndex)
    {
        if (desiredIndex < 0)
            return 0;

        if (desiredIndex >= array.Length)
            return array.Length - 1;

        return desiredIndex;
    }

    /// <summary>
    /// Provided an <paramref name="array"/> and a set of desired indixes to target, 
    /// returns the closest index to the target that is a valid index inside the array
    /// </summary>
    /// <typeparam name="T">Type of the array</typeparam>
    /// <param name="array">Array to check against</param>
    /// <param name="desiredIndex">Desired index to have inside the array that might be outside the bounds</param>
    /// <returns><see cref="Godot.Vector2I"/> containing the closest index to the desired array position</returns>
    public static Godot.Vector2I GetClosestIndexInBounds<T>(this T[,] array, Godot.Vector2I desiredIndex)
        => GetClosestIndexInBounds(array, desiredIndex.X, desiredIndex.Y);

    /// <summary>
    /// Provided an <paramref name="array"/> and a set of desired indixes to target, 
    /// returns the closest index to the target that is a valid index inside the array
    /// </summary>
    /// <typeparam name="T">Type of the array</typeparam>
    /// <param name="array">Array to check against</param>
    /// <param name="desiredIndexOne">Desired index 1 to have inside the array that might be outside the bounds</param>
    /// <param name="desiredIndexTwo">Desired index 2 to have inside the array that might be outside the bounds</param>
    /// <returns><see cref="Godot.Vector2I"/> containing the closest index to the desired array position</returns>
    public static Godot.Vector2I GetClosestIndexInBounds<T>(this T[,] array, int desiredIndexOne, int desiredIndexTwo)
    {
        var target = new Godot.Vector2I();

        if (desiredIndexOne < 0)
            target.X = 0;
        else if (desiredIndexOne >= array.GetLength(0))
            target.X = array.GetLength(0) - 1;
        else
            target.X = desiredIndexOne;

        if (desiredIndexTwo < 0)
            target.Y = 0;
        else if (desiredIndexTwo >= array.GetLength(1))
            target.Y = array.GetLength(1) - 1;
        else
            target.Y = desiredIndexTwo;

        return target;
    }


    public static IEnumerable<Godot.Vector2I> GetIndicesInRange<T>(this T[,] array, Godot.Vector2I startIndex, Godot.Vector2I range)
    {
        // Preload an array sized to fit all the possible indicies into it (assuming the grid position will most often fit the range)
        var indices = new List<Godot.Vector2I>(
            Godot.Mathf.Abs(range.X - startIndex.X) * Godot.Mathf.Abs(range.Y - startIndex.Y));

        // Iterate all possible array positions
        for (int x = startIndex.X - range.X; x <= startIndex.X + range.X; x++)
        {
            // Optimization to avoid all invalid X positions from the get-go
            if (x < 0 || x >= array.GetLength(0))
                continue;

            for (int y = startIndex.Y - range.Y; y <= startIndex.Y + range.Y; y++)
            {
                // X is always assumed valid at this point in time, check that the Y is now also valid
                if (y < 0 || y >= array.GetLength(1))
                    continue;

                indices.Add(new Godot.Vector2I(x, y));
            }
        }

        return indices;
    }
}
