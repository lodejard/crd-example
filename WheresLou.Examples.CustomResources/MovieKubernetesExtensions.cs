using k8s;
using Microsoft.Rest.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WheresLou.Examples.CustomResources.Models;

namespace WheresLou.Examples.CustomResources
{
    public static class MovieKubernetesExtensions
    {
        public static async Task<V1alpha1Movie> ReadNamespacedMovieAsync(this IKubernetes operations, string name, string namespaceParameter, CancellationToken cancellationToken = default)
        {
            var result = await operations.GetNamespacedCustomObjectWithHttpMessagesAsync(
                group: V1alpha1Movie.KubeGroup,
                version: V1alpha1Movie.KubeApiVersion,
                namespaceParameter: namespaceParameter,
                plural: V1alpha1Movie.KubePluralName,
                name: name,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            
            return SafeJsonConvert.DeserializeObject<V1alpha1Movie>(await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false), operations.DeserializationSettings);
        }

        public static async Task<V1alpha1Movie> ReplaceNamespacedMovieAsync(this IKubernetes operations, V1alpha1Movie body, string name, string namespaceParameter, CancellationToken cancellationToken = default)
        {
            var result = await operations.ReplaceNamespacedCustomObjectWithHttpMessagesAsync(
                body: body,
                group: V1alpha1Movie.KubeGroup,
                version: V1alpha1Movie.KubeApiVersion,
                namespaceParameter: namespaceParameter,
                plural: V1alpha1Movie.KubePluralName,
                name: name,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return SafeJsonConvert.DeserializeObject<V1alpha1Movie>(await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false), operations.DeserializationSettings);
        }

        public static async Task<V1alpha1Movie> CreateNamespacedMovieAsync(this IKubernetes operations, V1alpha1Movie body, string namespaceParameter,CancellationToken cancellationToken = default)
        {
            var result = await operations.CreateNamespacedCustomObjectWithHttpMessagesAsync(
                body: body,
                group: V1alpha1Movie.KubeGroup,
                version: V1alpha1Movie.KubeApiVersion,
                namespaceParameter: namespaceParameter,
                plural: V1alpha1Movie.KubePluralName,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return SafeJsonConvert.DeserializeObject<V1alpha1Movie>(await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false), operations.DeserializationSettings);
        }
    }
}
