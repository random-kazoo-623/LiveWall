using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Permissions;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonExtensionData = Newtonsoft.Json.JsonExtensionDataAttribute;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using JsonConverter = Newtonsoft.Json.JsonConverterAttribute;
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
            [JsonProperty("objects")]
            [JsonConverter(typeof(SceneObjectConverter))]
            public List<SceneObjects> Objects { get; set; }
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
            [JsonProperty("ambientcolor")]
            public string AmbientColor { get; set; }
            [JsonProperty("bloom")]
            public bool Bloom { get; set; }
            [JsonProperty("bloomhdrfeather")]
            public double BloomHdrFeather { get; set; }
            [JsonProperty("bloomhdriterations")]
            public int BloomHdrIterations { get; set; }
            [JsonProperty("bloomhdrscatter")]
            public double BloomHdrScatter { get; set; }
            [JsonProperty("bloomhdrstrength")]
            public double BloomHdrStrength { get; set; }
            [JsonProperty("bloomhdrthreshold")]
            public double BloomHdrThreshold { get; set; }
            [JsonProperty("bloomstrength")]
            public double BloomStrength { get; set; }
            [JsonProperty("bloomthreshold")]
            public double BloomThreshold { get; set; }
            [JsonProperty("bloomtint")]
            public string? BloomTint { get; set; }
            [JsonProperty("camerafade")]
            public bool CameraFade { get; set; }
            [JsonProperty("cameraparallax")]
            public bool CameraParallax { get; set; }
            [JsonProperty("cameraparallaxamount")]
            public double CameraParallaxAmount { get; set; }
            [JsonProperty("cameraparallaxdelay")]
            public double CameraParallaxDelay { get; set; }
            [JsonProperty("cameraparallaxmouseinfluence")]
            public double CameraParallaxMouseInfluence { get; set; }
            [JsonProperty("camerapreview")]
            public bool CameraPreview { get; set; }
            [JsonProperty("camerashake")]
            public bool CameraShake { get; set; }
            [JsonProperty("camerashakeamplitude")]
            public double CameraShakeAmplitude { get; set; }
            [JsonProperty("camerashakeroughness")]
            public double CameraShakeRoughness { get; set; }
            [JsonProperty("camerashakespeed")]
            public double CameraShakeSpeed { get; set; }
            [JsonProperty("clearcolor")]
            public string ClearColor { get; set; }
            [JsonProperty("clearenabled")]
            public bool ClearEnabled { get; set; }
            [JsonProperty("farz")]
            public double Farz { get; set; }
            [JsonProperty("fov")]
            public double Fov { get; set; }
            [JsonProperty("gravitydirection")]
            public string? GravityDirection { get; set; }
            [JsonProperty("gravitystrength")]
            public double? GravityStrength { get; set; }
            [JsonProperty("hdr")]
            public bool Hdr { get; set; }
            [JsonProperty("lightconfig")]
            public LightConfig? LightConfig { get; set; }
            [JsonProperty("nearz")]
            public double Nearz { get; set; }
            [JsonProperty("orthogonalprojection")]
            public OrthogonalProjection OrthogonalProjection { get; set; }
            [JsonProperty("perspectiveoverridefov")]
            public double? PerspectiveOverrideFov { get; set; }
            [JsonProperty("skylightcolor")]
            public string SkyLightColor { get; set; }
            [JsonProperty("spritesheetrefreshsync")]
            public bool? SpritesheetRefreshSync { get; set; }
            [JsonProperty("winddirection")]
            public string? WindDirection { get; set; }
            [JsonProperty("windenabled")]
            public bool? WindEnabled { get; set; }
            [JsonProperty("windstrength")]
            public double? WindStrength { get; set; }
            [JsonProperty("zoom")]
            public double Zoom { get; set; }

            //fallback
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

            [JsonProperty("objecteffect")]
            public ObjectEffect? ObjectEffect { get; set; }
            [JsonProperty("objectparticle")]
            public ObjectParticle? ObjectParticle { get; set; }
            [JsonProperty("objectimagemenu")]
            public ObjectImageMenu? ObjectImageMenu { get; set; }
        }

        //converter of 3 types
        public class SceneObjectConverter : Newtonsoft.Json.JsonConverter<SceneObjects>
        {
            public override SceneObjects ReadJson(JsonReader reader, Type objectType, SceneObjects existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                JObject obj = JObject.Load(reader);
                var SceneObject = new SceneObjects();

                if (obj.ContainsKey("particle")) // is a particle obj
                {
                    SceneObject.ObjectParticle = obj.ToObject<ObjectParticle>(serializer);
                }
                if (obj.ContainsKey("effects")) // is an effect obj
                {
                    SceneObject.ObjectEffect = obj.ToObject<ObjectEffect>(serializer);
                }
                if (obj.ContainsKey("text")) // is a menu text
                {
                    SceneObject.ObjectImageMenu = obj.ToObject<ObjectImageMenu>(serializer);
                }

                return SceneObject;
            }

            public override void WriteJson(JsonWriter writer, SceneObjects value, JsonSerializer serializer)
            {
                if (value.ObjectParticle != null)
                {
                    serializer.Serialize(writer, value.ObjectParticle);
                }
                else if (value.ObjectEffect != null)
                {
                    serializer.Serialize(writer, value.ObjectParticle);
                }
                else if (value.ObjectImageMenu != null)
                {
                    serializer.Serialize(writer, value.ObjectImageMenu);
                }
            }
        }
        
        #region Object Effect
        public class ObjectEffect
        {
            [JsonProperty("alightment")]
            public string? Alightment { get; set; }
            [JsonProperty("alpha")]
            public double? Alpha { get; set; }
            [JsonProperty("angles")]
            public string? Angles { get; set; }
            [JsonProperty("brightness")]
            public double? Brightness { get; set; }
            [JsonProperty("color")]
            public string? Color { get; set; }
            [JsonProperty("colorblendmode")]
            public int? ColorBlendMode { get; set; }
            [JsonProperty("copybackground")]
            public bool? CopyBackground { get; set; }
            [JsonProperty("castshadow")]
            public bool? CastShadow { get; set; }
            [JsonProperty("id")]
            public int? Id { get; set; }
            [JsonProperty("image")]
            public string? Image { get; set; }
            [JsonProperty("name")]
            public string? Name { get; set; }
            [JsonProperty("origin")]
            public string? Origin { get; set; }
            [JsonProperty("scale")]
            public string? Scale { get; set; }
            [JsonProperty("size")]
            public string? Size { get; set; }
            [JsonProperty("parent")]
            public int? Parent {  get; set; }

            [JsonProperty("effects")]
            public List<EffectProperty> Effects { get; set; }

            //fallback
            [JsonExtensionData]
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }
        }

        public class EffectProperty
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
        }

        public class ShaderPasses
        {
            [JsonProperty("combos")]
            public ShaderCombos? Combos { get; set; }
            [JsonProperty("constantshadervalues")]
            public ShaderConstantsValues ConstantShaderValues { get; set; }

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("textures")]
            public List<string>? Textures { get; set; }
        }

        public class ShaderConstantsValues
        {
            // read whatever in here and compiles  it
            [JsonExtensionData]
            public IDictionary<string, JToken> RawValues { get; set; } = new Dictionary<string, JToken>();

            [JsonProperty("Bar Color")]
            public ShaderBarColor? BarColor { get; set; }

            [JsonProperty("Bar Count")]
            public ShaderBarGeneric? BarCount { get; set; }

            [JsonProperty("Border Width")]
            public ShaderBarGeneric? BorderWidth { get; set; }


            [JsonIgnore]
            public Dictionary<string, object> AsDictionary
            {
                get
                {
                    var dict = new Dictionary<string, object>();
                    foreach(var kvp in RawValues)
                    {
                        try
                        {
                            dict[kvp.Key] = kvp.Value.Type switch
                            {
                                JTokenType.Integer => kvp.Value.ToObject<int>(),
                                JTokenType.String => kvp.Value.ToObject<string>(),
                                JTokenType.Float => kvp.Value.ToObject<float>(),
                                _ => kvp.Value
                            };
                        }
                        catch
                        {
                            dict[kvp.Key] = kvp.Value.ToString();
                        }
                    }
                    return dict;
                }
            }
        }
        #region Const Shader Nested Values
        /// <summary>
        /// for object shaders: Bar Color
        /// </summary>
        public class ShaderBarColor
        {
            [JsonProperty("user")]
            public string User {  get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }
        }

        /// <summary>
        /// for object shaders: Bar Count, Border Width
        /// </summary>
        public class ShaderBarGeneric
        {
            [JsonProperty("user")]
            public string User { set; get; }
            [JsonProperty("value")]
            public double Value { get; set; }
        }

        #endregion Const Shader Nested Values

        public class ShaderCombos
        {
            //to be used in Passes
            [JsonProperty("QUALITY")]
            public int? Quality { get; set; }
            [JsonProperty("ANAMORPHIC")]
            public int? Anamorphic { get; set; }
            [JsonProperty("VERTICAL")]
            public int? Vertical { get; set; }
            [JsonProperty("HORIZONTAL")]
            public int? Horizontal { get; set; }
            [JsonProperty("BLENDMODE")]
            public int? BlendMode { get; set; }
            [JsonProperty("PRECISE")]
            public int? Precise { get; set; }
            [JsonProperty("HOLLOW")]
            public int? Hollow { get; set; }
            [JsonProperty("SHAPE")]
            public int? Shape { get; set; }
            [JsonProperty("VMAPPING")]
            public int? Vmapping {  get; set; }

            [JsonExtensionData]
            //store unknown keys
            private List<Dictionary<string, JsonElement>> ExtraFields { get; set; }

        }

        #endregion Object Effect


        #region Object Particle

        public class ObjectParticle
        {
            [JsonProperty("angles")] //this could be named origin, angles, alpha, zoom,  etc so i have to turn it into generic type, also present in other objects as well...
            public Particle ParticleObject { get; set; }
            public int Id { get; set; }
            public InstanceOverride InstanceOverride { get; set; }
            public bool LockTransforms { get; set; }
            public string Name { get; set; }
            public string ParallaxDepth { get; set; }
            public string Particle { get; set; }
            public string Scale { get; set; }
            public VisibleMixed Visible { get; set; }
        }


        public class InstanceOverride
        {
            public double Alpha { get; set; }
            public int Id { get; set; }
            public double Rate { get; set; }
        }
        #region Particle
        public class Particle
        {
            public string Value { get; set; }
            public ParticleAnimation Animation { get; set; }

            public bool HasAnimation => Animation != null;
        }


        public class ParticleAnimation
        {
            [JsonPropertyName("animation")]
            public ParticleAnimationInfo Frames { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }
        public class ParticleAnimationInfo
        {
            public ParticleAnimationOptions Options { get; set; }
            public bool Relative {  get; set; }

            [JsonExtensionData]
            public Dictionary<string, JToken> RawChannels { get; set; }

            [JsonIgnore]
            public Dictionary<string, List<ParticleAnimationFrame>> Channels
            {
                get
                {
                    if (_channels != null) return _channels;

                    _channels = new();
                    if (RawChannels == null) return _channels;

                    foreach (var (key, token) in RawChannels)
                    {
                        if (key.StartsWith("c"))
                        {
                            try
                            {
                                var frames = token.ToObject<List<ParticleAnimationFrame>>();
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
            private Dictionary<string, List<ParticleAnimationFrame>> _channels;
        }

        public class ParticleAnimationOptions
        {
            public int Fps { get; set; }
            public int Length { get; set; }
            public string Mode { get; set; }
            public bool WrapLoop { get; set; }

        }
        public class ParticleAnimationFrame
        {
            [JsonProperty("back")]
            public ParticleAnimationFrameValue Back { get; set; }

            [JsonProperty("frame")]
            public int Frame { get; set; }

            [JsonProperty("front")]
            public ParticleAnimationFrameValue Front { get; set; }

            [JsonProperty("lockangle")]
            public bool LockAngle { get; set; }
            [JsonProperty("locklength")]
            public bool LockLength { get; set; }
            [JsonProperty("value")]
            public double Value { get; set; }
        }

        public class ParticleAnimationFrameValue
        {
            [JsonProperty("enabled")]
            public bool Enabled { get; set; }
            [JsonProperty("x")]
            public int X { get; set; }
            [JsonProperty("y")]
            public int Y { get; set; }
        }


        //converter for particle objects
        public class OriginConverter : Newtonsoft.Json.JsonConverter<Particle>
        {
            public override Particle ReadJson(JsonReader reader, Type Objectype, Particle ExistingValue, bool HasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    //if the 1st field is filled in
                    return new Particle { Value = (string)reader.Value };
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    var obj = JObject.Load(reader);
                    var data = obj.ToObject<ParticleAnimation>(serializer);
                    return new Particle { Animation = data, Value = data.Value };
                }

                return null;
            }

            public override void WriteJson(JsonWriter writer, Particle value, JsonSerializer serializer)
            {
                if (value.Animation == null)
                {
                    writer.WriteValue(value.Value);
                }
                else
                {
                    serializer.Serialize(writer, new { animation = value.Animation, value = value.Value });
                }
            }


        }
        #endregion Particle Origin

        #endregion Object Particle


        #region Object Text Image Menu
        public class ObjectImageMenu
        {
            public double Alpha { get; set; }
            public string Anchor { get; set; }
            public string Angles { get; set; }
            public double BackgroundBrightness { get; set; }
            public string BackgroundColor { get; set; }
            public bool BlockAlign { get; set; }
            public double Brightness { get; set; }
            public string Color { get; set; }
            public int ColorBlendMode { get; set; }
            public bool CopyBackground { get; set; }
            public string Font { get; set; }
            public string HorizontalAlign { get; set; }
            public int Id { get; set; }
            public bool LedSource { get; set; }
            public bool LimitRows { get; set; }
            public bool LimitUseEllipsis { get; set; }
            public bool LimitWidth { get; set; }
            public bool LockTransform { get; set; }
            public int MaxRows { get; set; }
            public double MaxWidth { get; set; }
            public string Name { get; set; }
            public bool OpaqueBackgrounnd { get; set; }
            public string Origin { get; set; }
            public int Padding { get; set; }
            public string ParallaxDepth { get; set; }
            public bool Perspective { get; set; }
            public double PointSize { get; set; }
            public string Scale { get; set; }
            public string Size { get; set; }
            public bool Solid { get; set; }

            public ObjectImageMenuText Text { get; set; }
            public string VerticalAlign { get; set; }

            public ObjectImageMenuTextVisible Visible { get; set; }
        }

        public class ObjectImageMenuText
        {
            public string Script { get; set; }
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

        public class ObjectImageMenuTextVisible
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


        #endregion Mixed Values
    }
}