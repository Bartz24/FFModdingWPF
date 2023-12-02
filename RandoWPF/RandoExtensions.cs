﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;

namespace Bartz24.RandoWPF;

public static class RandoExtensions
{
    public static double CubeRoot(double x)
    {
        return x < 0 ? -Math.Pow(-x, 1d / 3d) : Math.Pow(x, 1d / 3d);
    }

    public static List<T> Shuffle<T>(this IEnumerable<T> enumerable)
    {
        List<T> newList = new(enumerable);
        int n = newList.Count;
        while (n > 1)
        {
            n--;
            int k = RandomNum.RandInt(0, n - 1);
            (newList[n], newList[k]) = (newList[k], newList[n]);
        }

        return newList;
    }

    public static void Shuffle<T>(this List<T> list, Action<T, T> swapFunc)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RandomNum.RandInt(0, n - 1);
            swapFunc.Invoke(list[n], list[k]);
        }
    }

    public static IList<T> ShuffleWeighted<T>(this IList<T> list, IList<int> weights)
    {
        Dictionary<int, int> map = new();
        for (int i = 0; i < list.Count; i++)
        {
            for (int w = 0; w < weights[i]; w++)
            {
                map.Add(map.Count, i);
            }
        }

        List<int> shuffled = Enumerable.Range(0, map.Count).Shuffle();
        list = Enumerable.Range(0, map.Count).Select(i => list[map[shuffled[i]]]).ToList();
        return list;
    }

    // Finds UI elements by UID when setting a name causes issues
    public static UIElement GetByUid(this DependencyObject rootElement, string uid)
    {
        foreach (UIElement element in LogicalTreeHelper.GetChildren(rootElement).OfType<UIElement>())
        {
            if (element.Uid == uid)
                return element;
            UIElement resultChildren = GetByUid(element, uid);
            if (resultChildren != null)
                return resultChildren;
        }
        return null;
    }
}
