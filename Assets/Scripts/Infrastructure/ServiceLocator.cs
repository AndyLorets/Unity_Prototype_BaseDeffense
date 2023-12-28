using System;
using System.Collections.Generic;

public static class ServiceLocator 
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void RegisterService<T>(T service)
    {
        services[typeof(T)] = service;
    }

    public static T GetService<T>()
    {
        object service;
        services.TryGetValue(typeof(T), out service);
        return (T)service;
    }
}