public interface IServiceLocator
{
    static IServiceLocator Instance { get; set; }

    static IServiceLocator()
    {
        Instance = new DefaultServiceLocator();
    }

    T GetService<T>() where T : IService;

    bool TryRegisterService<TContract, TImplementation>(TImplementation service)
        where TContract : class, IService
        where TImplementation : class, TContract;
    bool TryUnregisterService<TContract, TImplementation>(TImplementation service)
        where TContract : class, IService
        where TImplementation : class, TContract;

    bool TryRegisterService<T>(T service) where T : class, IService
        => TryRegisterService<T, T>(service);
    bool TryUnregisterService<T>(T service) where T : class, IService
        => TryUnregisterService<T, T>(service);
}
