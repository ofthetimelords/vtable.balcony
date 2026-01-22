namespace Vtable.Balcony.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddImplementationsOf<TService>(this IServiceCollection services)
        {
            var serviceType = typeof(TService);
            var assembly = serviceType.Assembly;

            var implementations = assembly
                .GetTypes()
                .Where(t => serviceType.IsAssignableFrom(t)
                            && !t.IsInterface
                            && !t.IsAbstract);

            foreach (var impl in implementations)
            {
                services.AddSingleton(serviceType, impl);
            }

            return services;
        }

        public static IServiceCollection ScanGenericImplementations(this IServiceCollection services, Type type)
        {
            return services.Scan(s =>s.FromAssembliesOf(type)
                            .AddClasses(c => c.AssignableTo(type))
                            .AsImplementedInterfaces()
                            .WithSingletonLifetime());
        }
    }
}
