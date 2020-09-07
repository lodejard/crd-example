using k8s;
using k8s.Models;
using Newtonsoft.Json;
using NJsonSchema.Annotations;
using System.Collections.Generic;

namespace WheresLou.Examples.CustomResources.Models
{
    /// <summary>
    /// Namespaced instance of a movie resource.
    /// </summary>
    [KubernetesEntity(ApiVersion = KubeApiVersion, Group = KubeGroup, Kind = KubeKind, PluralName = KubePluralName)]
    public class V1alpha1Movie : IKubernetesObject<V1ObjectMeta>, ISpec<V1alpha1MovieSpec>, IStatus<V1alpha1MovieStatus>
    {
        public const string KubeApiVersion = "v1alpha1";
        public const string KubeGroup = "whereslou.com";
        public const string KubeKind = "Movie";
        public const string KubePluralName = "movies";

        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("metadata")]
        public V1ObjectMeta Metadata { get; set; }

        /// <summary>
        /// Specification of the movie.
        /// </summary>
        [JsonProperty("spec")]
        public V1alpha1MovieSpec Spec { get; set; }

        /// <summary>
        /// The status of the movie.
        /// </summary>
        [JsonProperty("status")]
        public V1alpha1MovieStatus Status { get; set; }
    }

    [JsonSchemaExtensionData("x-kubernetes-preserve-unknown-fields", true)]
    public class V1alpha1MovieSpec
    {
        /// <summary>
        /// Name of the movie in it's original language.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    [JsonSchemaExtensionData("x-kubernetes-preserve-unknown-fields", true)]
    public class V1alpha1MovieStatus
    {
        /// <summary>
        /// True if the movie is currently playing.
        /// </summary>
        [JsonProperty("playing")]
        public bool? Playing { get; set; }

        /// <summary>
        /// Most recent reviews left by viewers.
        /// </summary>
        [JsonProperty("reviews")]
        [JsonSchemaExtensionData("x-kubernetes-list-type", "map")]
        [JsonSchemaExtensionData("x-kubernetes-list-map-keys", new[] { "name" })]
        public IList<V1alpha1MovieReview> Reviews { get; set; }
    }

    public class V1alpha1MovieReview
    {
        /// <summary>
        /// Username which uniquely identifies the review
        /// </summary>
        [JsonProperty("name")]
        [JsonRequired]
        public string Name { get; set; }

        /// <summary>
        /// Viewer rating from 1 to 5 stars.
        /// </summary>
        [JsonProperty("rating")]
        public double Rating { get; set; }

        /// <summary>
        /// Optional comments left by reviewer.
        /// </summary>
        [JsonProperty("comments")]
        public string Comments { get; set; }
    }
}
