using k8s;
using k8s.Models;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WheresLou.Examples.CustomResources.Models;

namespace WheresLou.Examples.CustomResources
{
    public static class MovieKubernetesExtensions
    {
        public static async Task<V1alpha1Movie> ReadNamespacedMovieAsync(this IKubernetes operations, string name, string namespaceParameter, CancellationToken cancellationToken = default)
        {
            var result = await operations.GetNamespacedCustomObjectAsync(
                group: V1alpha1Movie.KubeGroup,
                version: V1alpha1Movie.KubeApiVersion,
                namespaceParameter: namespaceParameter,
                plural: V1alpha1Movie.KubePluralName,
                name: name,
                cancellationToken: cancellationToken);
            
            return SafeJsonConvert.DeserializeObject<V1alpha1Movie>(SafeJsonConvert.SerializeObject(result, operations.SerializationSettings), operations.DeserializationSettings);
        }

        public static async Task<V1alpha1Movie> ReplaceNamespacedMovieAsync(this IKubernetes operations, V1alpha1Movie body, string name, string namespaceParameter, CancellationToken cancellationToken = default)
        {
            var result = await operations.ReplaceNamespacedCustomObjectAsync(
                body: body,
                group: V1alpha1Movie.KubeGroup,
                version: V1alpha1Movie.KubeApiVersion,
                namespaceParameter: namespaceParameter,
                plural: V1alpha1Movie.KubePluralName,
                name: name,
                cancellationToken: cancellationToken);

            return SafeJsonConvert.DeserializeObject<V1alpha1Movie>(SafeJsonConvert.SerializeObject(result, operations.SerializationSettings), operations.DeserializationSettings);
        }

        public static async Task<V1alpha1Movie> CreateNamespacedMovieAsync(this IKubernetes operations, V1alpha1Movie body, string namespaceParameter,CancellationToken cancellationToken = default)
        {
            var result = await operations.CreateNamespacedCustomObjectAsync(
                body: body,
                group: V1alpha1Movie.KubeGroup,
                version: V1alpha1Movie.KubeApiVersion,
                namespaceParameter: namespaceParameter,
                plural: V1alpha1Movie.KubePluralName,
                cancellationToken: cancellationToken);

            return SafeJsonConvert.DeserializeObject<V1alpha1Movie>(SafeJsonConvert.SerializeObject(result, operations.SerializationSettings), operations.DeserializationSettings);
        }
    }
}
