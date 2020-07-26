using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Array2DExtensions
{
    public static void Foreach<T>(this T[,] array, Func<int, int, T, bool> action)
    {
        for (int i = 0; i < array.GetLength(0); ++i)
        {
            for (int j = 0; j < array.GetLength(1); ++j)
            {
                if (action(i, j, array[i, j]))
                    return;
            }
        }
    }

    public static void Foreach<T>(this T[,] array, Action<int, int, T> action)
    {
        for (int i = 0; i < array.GetLength(0); ++i)
        {
            for (int j = 0; j < array.GetLength(1); ++j)
                action(i, j, array[i, j]);
        }
    }

    public static T Find<T>(this T[,] array, Predicate<T> predicate)
    {
        for (int i = 0; i < array.GetLength(0); ++i)
        {
            for (int j = 0; j < array.GetLength(1); ++j)
            {
                if (predicate(array[i, j]))
                    return array[i, j];
            }
        }
        return default;
    }
}
