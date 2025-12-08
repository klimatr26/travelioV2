using System;
using System.Collections.Generic;
using System.Text;

namespace TravelioREST;

internal static class Global
{
    [ThreadStatic]
    private static HttpClient? client;

    public static HttpClient ThreadLocalCachedHttpClient => client ??= new();

    public readonly static HttpClient CachedHttpClient = new();
}
