using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Backend.Tests._Support;

public class MockRequestCookieCollection : IRequestCookieCollection
{
    private readonly Dictionary<string, string> _cookies = new();
    
    public void Add(string key, string value)
    {
        _cookies.Add(key, value);
    }
    
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _cookies.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ContainsKey(string key)
    {
        return _cookies.ContainsKey(key);
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return _cookies.TryGetValue(key, out value);
    }

    public int Count => _cookies.Count;

    public ICollection<string> Keys => _cookies.Keys;

    public string this[string key] => _cookies[key];
}