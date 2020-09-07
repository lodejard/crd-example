using k8s;
using k8s.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WheresLou.Examples.CustomResources
{
    public class CustomResourceDefinitionGenerator
    {
        private readonly JsonSchemaGeneratorSettings _jsonSchemaGeneratorSettings;
        private readonly JsonSerializerSettings _deserializationSettings;

        public CustomResourceDefinitionGenerator(IKubernetes client)
        {
            _jsonSchemaGeneratorSettings = new JsonSchemaGeneratorSettings()
            {
                SchemaType = SchemaType.OpenApi3,
                TypeMappers =
                {
                    new ObjectTypeMapper(typeof(V1ObjectMeta), new JsonSchema{Type = JsonObjectType.Object})
                }
            };

            _deserializationSettings = client.DeserializationSettings;
        }

        public V1CustomResourceDefinition GenerateCustomResourceDefinition<TResource>(string scope)
        {
            var entity = typeof(TResource).GetTypeInfo().GetCustomAttribute<KubernetesEntityAttribute>();

            var group = entity.Group;
            var version = entity.ApiVersion;
            var kind = entity.Kind;
            var plural = entity.PluralName;
            var name = $"{plural}.{group}";

            var schema = GenerateJsonSchema<TResource>();

            return new V1CustomResourceDefinition(
                metadata: new V1ObjectMeta(
                    name: name),
                spec: new V1CustomResourceDefinitionSpec(
                    group: group,
                    names: new V1CustomResourceDefinitionNames(
                        kind: kind,
                        plural: plural),
                    scope: scope,
                    versions: new[]
                    {
                        new V1CustomResourceDefinitionVersion(
                            name: version,
                            served: true,
                            storage:true,
                            schema: new V1CustomResourceValidation(
                                openAPIV3Schema: schema))
                    }));
        }

        private V1JSONSchemaProps GenerateJsonSchema<TResource>()
        {
            // start with JsonSchema
            var schema = JsonSchema.FromType<TResource>(_jsonSchemaGeneratorSettings);

            // convert to JToken to make alterations
            var rootToken = JObject.Parse(schema.ToJson());            
            rootToken = RewriteObject(rootToken);
            rootToken.Remove("$schema");
            rootToken.Remove("definitions");

            // convert to k8s.Models.V1JSONSchemaProps to return
            return JsonSerializer
                .Create(_deserializationSettings)
                .Deserialize<V1JSONSchemaProps>(new JTokenReader(rootToken));
        }

        private JObject RewriteObject(JObject sourceObject)
        {
            var targetObject = new JObject();

            var queue = new Queue<JObject>();
            queue.Enqueue(sourceObject);
            while (queue.Count != 0)
            {
                sourceObject = queue.Dequeue();
                foreach (var property in sourceObject.Properties())
                {
                    if (property.Name == "$ref")
                    {
                        // resolve the target of the "$ref"
                        var reference = sourceObject;
                        foreach (var part in property.Value.Value<string>().Split("/"))
                        {
                            if (part == "#")
                            {
                                reference = (JObject)reference.Root;
                            }
                            else
                            {
                                reference = (JObject)reference[part];
                            }
                        }
                        // the referenced object should be merged into the current target
                        queue.Enqueue(reference);
                        // and $ref property is not added
                        continue;
                    }
                    if (property.Name == "additionalProperties" && property.Value.Type == JTokenType.Boolean && property.Value.Value<bool>() == false)
                    {
                        // don't add this property when it has a default value
                        continue;
                    }
                    if (property.Name == "oneOf" && property.Value.Type == JTokenType.Array && property.Value.Children().Count() == 1)
                    {
                        // a single oneOf array item should be merged into current object
                        queue.Enqueue(RewriteObject(property.Value.Children().Cast<JObject>().Single()));
                        // and don't add the oneOf property
                        continue;
                    }
                    // all other properties are added after the value is rewritten recursively
                    targetObject.Add(property.Name, RewriteToken(property.Value));
                }
            }

            return targetObject;
        }

        private JToken RewriteToken(JToken sourceToken)
        {
            if (sourceToken is JObject sourceObject)
            {
                return RewriteObject(sourceObject);
            }
            else if (sourceToken is JArray sourceArray)
            {
                return new JArray(sourceArray.Select(RewriteToken));
            }
            else
            {
                return sourceToken;
            }
        }
    }
}
