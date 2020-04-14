using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Apex.PipeEncoding.DependencyInjection {

    internal sealed class ServiceProviderPipeDecoderFactory : IPipeDecoderFactory {

        readonly IServiceProvider Services;
        readonly ConcurrentDictionary<Type, object> Readers = new ConcurrentDictionary<Type, object>();

        public ServiceProviderPipeDecoderFactory(IServiceProvider services) {
            Services = services;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeDecoder<T> GetPipeReader<T>()
            => Readers.GetOrAdd(typeof(T), t => Services.GetRequiredService<IPipeDecoder<T>>()) as IPipeDecoder<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeDecoder GetPipeReader(Type messageType)
            => Readers.GetOrAdd(messageType, t => Services.GetRequiredService(typeof(IPipeDecoder<>).MakeGenericType(t))) as IPipeDecoder;
    }
}
