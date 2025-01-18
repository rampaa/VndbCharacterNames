namespace VndbCharacterNames;

internal static class ExtensionMethods
{
    public static void AddIfNotExists<TKey, TElement>(this Dictionary<TKey, List<TElement>> dict, TKey key, TElement element) where TKey : notnull
    {
        if (dict.TryGetValue(key, out List<TElement>? list))
        {
            if (!list.Contains(element))
            {
                list.Add(element);
            }
        }
        else
        {
            dict[key] = [element];
        }
    }
}
