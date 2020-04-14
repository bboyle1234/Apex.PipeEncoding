using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Apex.PipeCompressors.DependencyInjection {

    internal sealed class ServiceProviderPipeWriterFactory : IPipeWriterFactory {

        readonly IServiceProvider Services;
        readonly ConcurrentDictionary<Type, object> Writers = new ConcurrentDictionary<Type, object>();

        public ServiceProviderPipeWriterFactory(IServiceProvider services) {
            Services = services;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeWriter<T> GetPipeWriter<T>()
            => Writers.GetOrAdd(typeof(T), t => Services.GetRequiredService<IPipeWriter<T>>()) as IPipeWriter<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeWriter GetPipeWriter(Type messageType)
            => Writers.GetOrAdd(messageType, t => Services.GetRequiredService(typeof(IPipeWriter<>).MakeGenericType(t))) as IPipeWriter;
    }
}
