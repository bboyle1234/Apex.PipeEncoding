using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Apex.PipeCompressors.DependencyInjection {

    internal sealed class ServiceProviderPipeReaderFactory : IPipeReaderFactory {

        readonly IServiceProvider Services;
        readonly ConcurrentDictionary<Type, object> Readers = new ConcurrentDictionary<Type, object>();

        public ServiceProviderPipeReaderFactory(IServiceProvider services) {
            Services = services;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeReader<T> GetPipeReader<T>()
            => Readers.GetOrAdd(typeof(T), t => Services.GetRequiredService<IPipeReader<T>>()) as IPipeReader<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeReader GetPipeReader(Type messageType)
            => Readers.GetOrAdd(messageType, t => Services.GetRequiredService(typeof(IPipeReader<>).MakeGenericType(t))) as IPipeReader;
    }
}
