
using UnityEngine;

public static class MouseButton
{
    public const int Left = 0;
    public const int Right = 1;
    public const int Middle = 2;
}

public delegate bool TraversePredicate<in T>(T value);
public delegate bool ConditionPredicate<in T>(T value);
public delegate bool ComparePredicate<in T>(T left, T right);
