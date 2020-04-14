using Apex.PipeCompressors;
using Apex.PipeCompressors.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection {

    public static class ServiceCollectionExtensions {

        /// <summary>
        /// Searches the given <paramref name="assembly"/> for all the <see cref="IPipeWriter{T}"/>
        /// implementations and adds them as singletons to the <paramref name="services"/> collection.
        /// </summary>
        public static IServiceCollection AddPipeWriters(this IServiceCollection services, Assembly assembly) {
            foreach (var writer in assembly.GetPipeWriters())
                services.AddSingleton(writer.ServiceType, writer.ImplementationType);
            return services;
        }

        /// <summary>
        /// Searches the given <paramref name="assembly"/> for all the <see cref="IPipeReader{T}"/>
        /// implementations and adds them as singletons to the <paramref name="services"/> collection.
        /// </summary>
        public static IServiceCollection AddPipeReaders(this IServiceCollection services, Assembly assembly) {
            foreach (var reader in assembly.GetPipeReaders())
                services.AddSingleton(reader.ServiceType, reader.ImplementationType);
            return services;
        }

        /// <summary>
        /// Adds a <see cref="IPipeWriterFactory"/> that seaches the <see cref="IServiceProvider"/> for 
        /// all requested <see cref="IPipeWriter{T}"/> implementations.
        /// Once this has been done, use can use it like this:
        /// <code>
        /// var data = new MyObject();
        /// var objectWriter = Services.GetRequiredService&lt;IPipeWriterFactory&gt;().GetPipeWriter&lt;MyObject&gt;();
        /// objectWriter.Write(pipeWriter, data);
        /// </code>
        /// </summary>
        public static IServiceCollection UseServiceProviderPipeWriterFactory(this IServiceCollection services)
            => services.AddSingleton<IPipeWriterFactory, ServiceProviderPipeWriterFactory>();

        /// <summary>
        /// Adds a <see cref="IPipeReaderFactory"/> that seaches the <see cref="IServiceProvider"/> for 
        /// all requested <see cref="IPipeReader{T}{T}"/> implementations.
        /// </summary>
        public static IServiceCollection UseServiceProviderPipeReaderFactory(this IServiceCollection services)
            => services.AddSingleton<IPipeReaderFactory, ServiceProviderPipeReaderFactory>();

        #region Helpers

        /// <summary>
        /// Searches the given <paramref name="assembly"/> to extract metadata about all the <see cref="IPipeWriter{T}"/> implementations found within.
        /// </summary>
        static IEnumerable<ServiceTypeData> GetPipeWriters(this Assembly assembly)
            => assembly.GetTypes().SelectMany(t => t.GetPipeWriters());

        /// <summary>
        /// Searches the given <paramref name="assembly"/> to extract metadata about all the <see cref="IPipeReader{T}"/> implementations found within.
        /// </summary>
        static IEnumerable<ServiceTypeData> GetPipeReaders(this Assembly assembly)
            => assembly.GetTypes().SelectMany(t => t.GetPipeReaders());


        static IEnumerable<ServiceTypeData> GetPipeWriters(this Type type) {
            if (!type.IsClass) yield break;
            if (type.IsAbstract) yield break;
            foreach (var compressorInterface in type.GetInterfaces().Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPipeWriter<>))) {
                yield return new ServiceTypeData {
                    ServiceType = compressorInterface,
                    ImplementationType = type,
                    CompressedObjectType = compressorInterface.GetGenericArguments()[0],
                };
            }
        }

        static IEnumerable<ServiceTypeData> GetPipeReaders(this Type type) {
            if (!type.IsClass) yield break;
            if (type.IsAbstract) yield break;
            foreach (var decompressorInterface in type.GetInterfaces().Where(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IPipeReader<>))) {
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
            /// Would be either <see cref="ICompressor{T}"/> or <see cref="IDecompressor{T}"/>.
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
