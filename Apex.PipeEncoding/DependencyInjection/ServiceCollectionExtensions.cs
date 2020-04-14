using Apex.PipeEncoding;
using Apex.PipeEncoding.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection {

    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Searches the given <paramref name="assembly"/> for all the <see cref="IPipeEncoder{T}"/>
        /// implementations and adds them as singletons to the <paramref name="services"/> collection.
        /// </summary>
        public static IServiceCollection AddPipeEncoders(this IServiceCollection services, Assembly assembly) {
            foreach (var writer in assembly.GetPipeEncoders())
                services.AddSingleton(writer.ServiceType, writer.ImplementationType);
            return services;
        }

        /// <summary>
        /// Searches the given <paramref name="assembly"/> for all the <see cref="IPipeDecoder{T}"/>
        /// implementations and adds them as singletons to the <paramref name="services"/> collection.
        /// </summary>
        public static IServiceCollection AddPipeDecoders(this IServiceCollection services, Assembly assembly) {
            foreach (var reader in assembly.GetPipeDecoders())
                services.AddSingleton(reader.ServiceType, reader.ImplementationType);
            return services;
        }

        /// <summary>
        /// Adds a <see cref="IPipeEncoderFactory"/> that seaches the <see cref="IServiceProvider"/> for 
        /// all requested <see cref="IPipeEncoder{T}"/> implementations.
        /// Once this has been done, use can use it like this:
        /// <code>
        /// var data = new MyObject();
        /// var objectWriter = Services.GetRequiredService&lt;IPipeWriterFactory&gt;().GetPipeWriter&lt;MyObject&gt;();
        /// objectWriter.Write(pipeWriter, data);
        /// </code>
        /// </summary>
        public static IServiceCollection UseServiceProviderPipeEncoderFactory(this IServiceCollection services)
            => services.AddSingleton<IPipeEncoderFactory, ServiceProviderPipeEncoderFactory>();

        /// <summary>
        /// Adds a <see cref="IPipeDecoderFactory"/> that seaches the <see cref="IServiceProvider"/> for 
        /// all requested <see cref="IPipeDecoder{T}"/> implementations.
        /// </summary>
        public static IServiceCollection UseServiceProviderPipeDecoderFactory(this IServiceCollection services)
            => services.AddSingleton<IPipeDecoderFactory, ServiceProviderPipeDecoderFactory>();

        #region Helpers

        /// <summary>
        /// Searches the given <paramref name="assembly"/> to extract metadata about all the <see cref="IPipeEncoder{T}"/> implementations found within.
        /// </summary>
        static IEnumerable<ServiceTypeData> GetPipeEncoders(this Assembly assembly)
            => assembly.GetTypes().SelectMany(t => t.GetPipeEncoders());

        /// <summary>
        /// Searches the given <paramref name="assembly"/> to extract metadata about all the <see cref="IPipeDecoder{T}"/> implementations found within.
        /// </summary>
        static IEnumerable<ServiceTypeData> GetPipeDecoders(this Assembly assembly)
            => assembly.GetTypes().SelectMany(t => t.GetPipeDecoders());


        static IEnumerable<ServiceTypeData> GetPipeEncoders(this Type type) {
            if (!type.IsClass) yield break;
            if (type.IsAbstract) yield break;
            foreach (var compressorInterface in type.GetInterfaces().Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPipeEncoder<>))) {
                yield return new ServiceTypeData {
                    ServiceType = compressorInterface,
                    ImplementationType = type,
                    CompressedObjectType = compressorInterface.GetGenericArguments()[0],
                };
            }
        }

        static IEnumerable<ServiceTypeData> GetPipeDecoders(this Type type) {
            if (!type.IsClass) yield break;
            if (type.IsAbstract) yield break;
            foreach (var decompressorInterface in type.GetInterfaces().Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPipeDecoder<>))) {
                yield return new ServiceTypeData {
                    ServiceType = decompressorInterface,
                    ImplementationType = type,
                    CompressedObjectType = decompressorInterface.GetGenericArguments()[0],
                };
            }
        }

        /// <summary>
        /// Used for obtaining information about a compressor or decompressor type.
        /// </summary>
        sealed class ServiceTypeData {

            /// <summary>
            /// The type of the class that implements the compressor or decompressor. You can create instances of this class to perform the actual work.
            /// </summary>
            public Type ImplementationType;

            /// <summary>
            /// Would be either <see cref="IPipeEncoder{T}"/> or <see cref="IPipeDecoder{T}"/>.
            /// </summary>
            public Type ServiceType;

            /// <summary>
            /// The type of the object to be compressed or decompressed.
            /// </summary>
            public Type CompressedObjectType;
        }

        #endregion
    }
}
