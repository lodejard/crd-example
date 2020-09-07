using k8s;
using k8s.Models;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WheresLou.Examples.CustomResources.Models;

namespace WheresLou.Examples.CustomResources
{
    class Foo
    {
        public string Bar { get; set; }
    }

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                var client = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());

                await CreateMoviesCustomResourceDefintionAsync(client);
                await CreateMovieAsync(client);

                return 0;
            }
            catch (HttpOperationException ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.Response.Content);

                var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .WithNamingConvention(new YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention())
                    .Build();

                var status = JsonConvert.DeserializeObject<V1Status>(ex.Response.Content);
                Console.WriteLine(serializer.Serialize(status));

                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                return 1;
            }
        }

        private static async Task CreateMoviesCustomResourceDefintionAsync(Kubernetes client)
        {
            // make an instance of the utility object
            var generator = new CustomResourceDefinitionGenerator(client);

            // generate a CRD for V1alpha1Movie
            var crd = generator.GenerateCustomResourceDefinition<V1alpha1Movie>("Namespaced");

            // add a few custom columns so "kubectl get movies --all-namespaces" shows more info
            crd.Spec.Versions[0].AdditionalPrinterColumns = new[] {
                new V1CustomResourceColumnDefinition (
                    jsonPath: ".spec.title",
                    name: "Title",
                    type: "string"),
                new V1CustomResourceColumnDefinition (
                    jsonPath: ".status.playing",
                    name: "Playing",
                    type: "boolean"),
            };

            // save a copy to see what it looks like
            File.WriteAllText(@"..\..\..\crd.yaml", SafeJsonConvert.SerializeObject(crd, client.SerializationSettings));

            try
            {
                // update the CRD in the current cluster
                var existing = await client.ReadCustomResourceDefinitionAsync(crd.Name());
                crd.Metadata.ResourceVersion = existing.ResourceVersion();
                await client.ReplaceCustomResourceDefinitionAsync(crd, crd.Name());
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // create the CRD if it doesn't exist already
                await client.CreateCustomResourceDefinitionAsync(crd);
            }
        }

        private static async Task CreateMovieAsync(Kubernetes client)
        {
            // create an example movie resource
            var movie = new V1alpha1Movie
            {
                ApiVersion = "whereslou.com/v1alpha1",
                Kind = "Movie",
                Metadata = new V1ObjectMeta(
                    name: "cthd", 
                    namespaceProperty: "contoso"),
                Spec = new V1alpha1MovieSpec
                {
                    Title = "Crouching Tiger Hidden Dragon",
                },
                Status = new V1alpha1MovieStatus
                {
                    Playing = false,
                    Reviews = new []
                    {
                        new V1alpha1MovieReview
                        {
                            Name = "alice",
                            Rating = 5,
                            Comments = "What's not to love about this movie?",
                        }
                    }
                }
            };

            // save a copy to see what it looks like
            File.WriteAllText(@"..\..\..\movie.yaml", SafeJsonConvert.SerializeObject(movie, client.SerializationSettings));

            try
            {
                // update the movie in the cluster
                var existing = await client.ReadNamespacedMovieAsync(movie.Name(), movie.Namespace());
                movie.Metadata.ResourceVersion = existing.ResourceVersion();
                await client.ReplaceNamespacedMovieAsync(movie, movie.Name(), movie.Namespace());
            }
            catch (HttpOperationException ex) when (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                // create the movie if it doesn't exist already
                await client.CreateNamespacedMovieAsync(movie, movie.Namespace());
            }
        }
    }
}
