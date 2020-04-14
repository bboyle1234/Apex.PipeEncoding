using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Apex.PipeEncoding.DependencyInjection {

    internal sealed class ServiceProviderPipeEncoderFactory : IPipeEncoderFactory {

        readonly IServiceProvider Services;
        readonly ConcurrentDictionary<Type, object> Writers = new ConcurrentDictionary<Type, object>();

        public ServiceProviderPipeEncoderFactory(IServiceProvider services) {
            Services = services;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeEncoder<T> GetPipeWriter<T>()
            => Writers.GetOrAdd(typeof(T), t => Services.GetRequiredService<IPipeEncoder<T>>()) as IPipeEncoder<T>;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPipeEncoder GetPipeWriter(Type messageType)
            => Writers.GetOrAdd(messageType, t => Services.GetRequiredService(typeof(IPipeEncoder<>).MakeGenericType(t))) as IPipeEncoder;
    }
}
