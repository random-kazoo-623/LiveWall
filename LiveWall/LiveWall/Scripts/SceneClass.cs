using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Eventing.Reader;
using System.Security.Permissions;
using System.Text.Json;
using System.Text.Json.Serialization;
using static LiveWall.Scripts.SceneClass;
using JsonConverter = Newtonsoft.Json.JsonConverterAttribute;
using JsonExtensionData = Newtonsoft.Json.JsonExtensionDataAttribute;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
namespace LiveWall.Scripts
{
    internal class SceneClass
    {
        public class GifScene
        {
            [JsonProperty("camera")]
            public CameraData Camera { get; set; }
            [JsonProperty("general")]
            public SceneGeneral General { get; set; }

            //special objects will be resovled at serilization
            [JsonProperty("objects")]
            public List<SceneObjects> Objects { get; set; }

            [JsonProperty("version")]
            public Version Version { get; set; }
        }

        public class CameraData
        {
            [JsonProperty("center")]
            public string Center { get; set; }
            [JsonProperty("eye")]
            public string Eye { get; set; }
            [JsonProperty("up")]
            public string Up { get; set; }
        }

        #region general property
        public class SceneGeneral
        {
            [JsonProperty("lightconfig")]
            public LightConfig? LightConfig { get; set; }
            [JsonProperty("orthogonalprojection")]
            public OrthogonalProjection OrthogonalProjection { get; set; }
            //dump everything into a dict for post processing
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }

        public class LightConfig
        {
            [JsonProperty("tube")]
            public int Tube { get; set; }
        }

        public class OrthogonalProjection
        {
            [JsonProperty("height")]
            public int Height { get; set; }
            [JsonProperty("width")]
            public int Width { get; set; }
        }

        #endregion general property

        #region object property
        //there are multiple version of scene.json file, this is version 1, latest could be version 5
        public class SceneObjects
        {
            //just a list of objects
            [JsonProperty("objects")]
            public List<SceneObject> Objects { get; set; }
        }

        /// <summary>
        /// not to be confused with SceneObjects!
        /// </summary>
        public class SceneObject
        {
            //process known structs
            [JsonProperty ("instanceoverride")]
            public InstanceOverride? InstanceOverride { get; set; }

            [JsonProperty ("effects")]
            public List<ObjectEffects>? ObjectEffects { get; set; }



            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }
        
        public class InstanceOverride
        {
            [JsonProperty ("alpha")]
            public double Alpha { get; set; }
            [JsonProperty ("id")]
            public int Id { get; set; }
            [JsonProperty ("rate")]
            public double Rate { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }


        //effect property
        public class ObjectEffects
        {
            [JsonProperty("file")]
            public string File { get; set; }
            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("passes")]
            public List<ShaderPasses> Passes { get; set; }

            [JsonProperty("visible")]
            public bool? Visible { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }

        public class ShaderPasses
        {
            [JsonProperty("combos")]
            public ShaderCombos? Combos { get; set; }
            [JsonProperty("shaderconstantvalues")]
            public ShaderConstantsValues ConstantShaderValues { get; set; }

            [JsonProperty("id")]
            public int Id { get; set; }
            [JsonProperty("textures")]
            public List<string>? Textures { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }

        public class ShaderConstantsValues
        {

            [JsonExtensionData]
            public Dictionary<string, JToken> RawChannels { get; set; } // there are similar variables like point0, point1, ...
            //dump everything else into a dict for post processing
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }


            [JsonIgnore]
            public Dictionary<string, string> Channels
            {
                get
                {
                    if (_channels != null) return _channels;

                    _channels = new();
                    if (RawChannels == null) return _channels;

                    foreach (var (key, token) in RawChannels)
                    {
                        if (key.StartsWith("point"))
                        {
                            try
                            {
                                var points = token.ToObject<string>();
                                _channels[key] = points;
                            }
                            catch
                            {
                                // ignore malformed entries gracefully
                            }
                        }
                    }
                    return _channels;
                }
            }
            private Dictionary<string, string> _channels;
        }
        #region Const Shader Nested Values
        /// <summary>
        /// for object shaders: Bar Color
        /// </summary>
        public class ShaderBarColor
        {
            public string User { get; set; }
            public string Value { get; set; }
        }

        /// <summary>
        /// for object shaders: Bar Count, Border Width
        /// </summary>
        public class ShaderBarGeneric
        {
            public string User { set; get; }
            public double Value { get; set; }
        }

        #endregion Const Shader Nested Values

        public class ShaderCombos
        {
            //to be used in Passes

            [JsonExtensionData]
            //dump everything into a dict for post processing
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }

        }

        #endregion Object Effect


        #region Object Text
        public class ObjectText // script
        {
            public Script Script { get; set; }

            public ObjectTextVisible Visible { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }

        public class Script
        {
            public string ScriptText { get; set; }
            public ScriptProperties ScriptProperties { get; set; }
            public string Value { get; set; }
        }

        public class ScriptProperties
        {
            public string Delimiter { get; set; }
            public bool ShowSeconds { get; set; }
            public Use24HrFormat Use24HrFormat { get; set; }
        }

        public class Use24HrFormat
        {
            public string User { get; set; }
            public bool Value { get; set; }
        }

        public class ObjectTextVisible
        {
            public ObjectImageMenuTextVisibleUser User { get; set; }
            public bool Value { get; set; }
        }

        public class ObjectImageMenuTextVisibleUser
        {
            public string Condition { get; set; }
            public string Name { get; set; }
        }
        #endregion Object Text Image Menu

        #region Object Animation

        public class ObjectAnimation
        {
            [JsonProperty("animationframes")]
            public AnimationFrames AnimationFrames { get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }

        }

        public class AnimationFrames
        {

            [JsonProperty("options")]
            public AnimationOptions Options { get; set; }
            [JsonProperty("relative")]
            public bool? Relative {  get; set; }

            [JsonExtensionData]
            //get all the raw channels
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
            public Dictionary<string, JToken> RawChannels { get; set; }

            [JsonIgnore]
            public Dictionary<string, List<FrameInfo>> Channels
            {
                get
                {
                    if (_channels != null) return _channels;

                    _channels = new();
                    if (RawChannels == null) return _channels;

                    foreach (var (key, token) in RawChannels)
                    {
                        //look for frames object
                        //append them to a dict and return it
                        if (key.StartsWith("c"))
                        {
                            try
                            {
                                var frames = token.ToObject<List<FrameInfo>>();
                                _channels[key] = frames;
                            }
                            catch
                            {
                                // ignore malformed entries gracefully
                            }
                        }
                    }
                    return _channels;
                }
            }
            private Dictionary<string, List<FrameInfo>> _channels;
        }

        public class FrameInfo
        {
            [JsonProperty("back")]
            public FrameInfoValue Back { get; set; }

            [JsonProperty("frame")]
            public int Frame { get; set; }

            [JsonProperty("front")]
            public FrameInfoValue Front { get; set; }

            [JsonProperty("lockangle")]
            public bool LockAngle { get; set; }
            [JsonProperty("locklength")]
            public bool LockLength { get; set; }
            [JsonProperty("value")]
            public double Value { get; set; }
        }

        public class FrameInfoValue
        {
            [JsonProperty("enabled")]
            public bool Enabled { get; set; }
            [JsonProperty("magic")]
            public bool? Magic { get; set; }
            [JsonProperty("x")]
            public float X { get; set; }
            [JsonProperty("y")]
            public float y { get; set; }

        }

        public class AnimationOptions
        {
            [JsonProperty("fps")]
            public int Fps { get; set; }
            [JsonProperty("length")]
            public int Length { get; set; }
            [JsonProperty("mode")]
            public string Mode { get; set; }
            [JsonProperty("wraploop")]
            public bool WrapLoop { get; set; }
            //fallback
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }

        #endregion Object Animation

        public class SceneVersion
        {
            public int Version { get; set; }
        }

        #endregion object property

        #region Mixed Values

        public class VisibleMixed
        {
            [JsonProperty("isvisible")]
            public bool IsVisible { get; set; }
            [JsonProperty("isuservisible")]
            public VisibleUser UserVisible { get; set; }

            public bool IsUserVisible => UserVisible != null;
        }

        public class VisibleUser
        {
            public string User { get; set; }
            public bool Value { get; set; }
        }

        //converter
        public class VisibleConverter : Newtonsoft.Json.JsonConverter<VisibleMixed>
        {
            public override VisibleMixed ReadJson(JsonReader reader, Type Objectype, VisibleMixed ExistingValue, bool HasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    //if the 1st field is filled in
                    return new VisibleMixed { IsVisible = (bool)reader.Value };
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    var obj = JObject.Load(reader);
                    var data = obj.ToObject<VisibleUser>(serializer);   
                    return new VisibleMixed { UserVisible = data, IsVisible = data.Value};
                }

                return null;
            }

            public override void WriteJson(JsonWriter writer, VisibleMixed value, JsonSerializer serializer)
            {
                if (value.UserVisible == null)
                {
                    writer.WriteValue(value.IsVisible);
                }
                else
                {
                    serializer.Serialize(writer, new { uservisible = value.UserVisible, isvisible = value.IsVisible });
                }
            }


        }


        //mixed value animation
        /// <summary>
        /// Generic type that can handle either a plain value or an animated object.
        /// Works for origin, angles, zoom, alpha, multiply, etc.
        /// </summary>
        public class AnimatableValue<T>
        {
            public T? Value { get; set; }
            public ObjectAnimation? Animation { get; set; }
            public string? Script { get; set; }

            [JsonIgnore]
            public bool HasAnimation => Animation != null;
        }

        //converter
        public class AnimatableValueConverter<T> : Newtonsoft.Json.JsonConverter<AnimatableValue<T>>
        {
            public override AnimatableValue<T> ReadJson(JsonReader reader, Type objectType, AnimatableValue<T>? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);

                if (token.Type == JTokenType.Object)
                {
                    var obj = (JObject)token;
                    var result = new AnimatableValue<T>();

                    if (obj["animation"] != null)
                        result.Animation = obj.ToObject<ObjectAnimation>(serializer);
                    if (obj["script"] != null)
                        result.Script = obj["script"]?.ToString();
                    if (obj["value"] != null)
                        result.Value = obj["value"].ToObject<T>(serializer);

                    return result;
                }
                else
                {
                    //plain data types
                    return new AnimatableValue<T> { Value = token.ToObject<T>(serializer) };
                }
            }

            public override void WriteJson(JsonWriter writer, AnimatableValue<T> value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                if (value.Animation != null)
                {
                    serializer.Serialize(writer, value.Animation);
                }
                else
                {
                    serializer.Serialize(writer, value.Value);
                }
            }
        }


        #endregion Mixed Values
    }
}