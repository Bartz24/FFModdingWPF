using System;
using System.Collections.Generic;
using System.Linq;
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
        List<T> list = enumerable.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            int j = RandomNum.NextInt(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }

    public static void Shuffle<T>(this List<T> list, Action<T, T> swapFunc)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = RandomNum.NextInt(i, list.Count);
            swapFunc(list[i], list[j]);
            (list[i], list[j]) = (list[j], list[i]);
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
