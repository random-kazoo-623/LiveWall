using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xamarin.Forms;
using static LiveWall.Scripts.SceneClass;
using JsonConverter = Newtonsoft.Json.JsonConverterAttribute;
using JsonExtensionData = Newtonsoft.Json.JsonExtensionDataAttribute;
using JsonIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
namespace LiveWall.Scripts
{
    internal class SceneClass
    {
        #region Possible Redundant
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
            private IDictionary<string, JToken> ExtraFields { get; set; }
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
            public List<SceneObject> Objects { get; set; } = new List<SceneObject>();
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
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }
        
        public class InstanceOverride
        {
            [JsonProperty ("id")]
            public int Id { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }
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

            //animation based stuff
            [JsonProperty("animation")]
            public ObjectAnimation? Animation { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }
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
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }

        public class ShaderConstantsValues
        {

            [JsonExtensionData]
            public Dictionary<string, JToken> RawChannels { get; set; } // there are similar variables like point0, point1, ...
            //dump everything else into a dict for post processing

            //other nested fields inside shaderconstantsvalues

            [JsonProperty("shader bar color")]
            private ShaderBarColor? ShaderBarColor { get; set; }
            [JsonProperty("shader bar generic")]
            private ShaderBarGeneric? ShaderBarGeneric { get; set; }
            private IDictionary<string, JToken> ExtraFields { get; set; }


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
            private IDictionary<string, JToken> ExtraFields { get; set; }

        }

        #endregion Object Effect

        #region Object Text
        public class ObjectText // script
        {
            [JsonProperty("script")]
            public Script Script { get; set; }

            [JsonProperty("visible")]
            public ObjectTextVisible Visible { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }

        public class Script
        {
            [JsonProperty("text")]
            public string ScriptText { get; set; }
            [JsonProperty("scriptproperties")]
            public ScriptProperties ScriptProperties { get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class ScriptProperties
        {
            [JsonProperty("use24hrformat")]
            public Use24HrFormat? Use24HrFormat { get; set; }

            //dump everything else into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }

        }


        public class Use24HrFormat
        {
            [JsonProperty("user")]
            public string User { get; set; }
            [JsonProperty("value")]
            public bool Value { get; set; }
        }

        public class ObjectTextVisible
        {
            [JsonProperty("user")]
            public ObjectTextVisibleUser User { get; set; }
            [JsonProperty("value")]
            public bool Value { get; set; }
        }

        public class ObjectTextVisibleUser
        {
            [JsonProperty("condition")]
            public string Condition { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
        #endregion Object Text

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
            private IDictionary<string, JToken> ExtraFields { get; set; }
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
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }

        #endregion Object Animation

        public class SceneVersion
        {
            [JsonProperty("version")]
            public int Version { get; set; }
        }

        public class VisibleUser
        {
            [JsonProperty("user")]
            public string User { get; set; }
            [JsonProperty("value")]
            public bool Value { get; set; }
        }

        #region Effect Depenendcy

        public class EffectDependentPropterties
        {
            [JsonProperty ("passes")]
            EffectDependentProptertiesPasses Passes { get; set; }
            //dump everything into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }
        public class EffectDependentProptertiesPasses
        {
            List<EffectMaterial> effectMaterials { get; set; }
        }

        public class EffectMaterial
        {
            //dump everything into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }

        public class EffectDependentProptertiesDependencies // thats a mouthful
        {
            List<string> Dependencies { get; set; }
            //dump everything into a dict for post processing
            [JsonExtensionData]
            private IDictionary<string, JToken> ExtraFields { get; set; }
        }




        #endregion Effect Depenendcy

        #endregion Possible Redundant

        #region Load Scene

        /// <summary>
        /// Scene loader, what did you expect? Start painting?
        /// </summary>
        public class SceneLoader
        {
            // base path gifscene.json / scene.json
            private readonly string BasePath;
            public SceneLoader(string SceneDirectory)
            {
                BasePath = SceneDirectory;
            }

            /// <summary>
            /// Load main scene from basepath
            /// Read all referenced json paths and parse them accordingly
            /// </summary>
            /// <param name="path"></param>
            /// <returns>Probably a nested list dict i guess? </returns>
            public SceneClass.GifScene LoadMainScene()
            {
                string path = BasePath;
                Debug.WriteLine("Loading scene on path {0}", path);
                string json = File.ReadAllText(path);
                
                JObject MainScene = ResolveJsonPaths(path);

                // deserialize
                var scene = JsonConvert.DeserializeObject<SceneClass.GifScene>(json);

                return scene;
            }

            /// <summary>
            /// Use Python, yes python to resovle fields and replace file paths with fetched Json Fields.
            /// Ignored File paths: preview/project.json paths, file paths in dependency (will overflow if included)
            /// </summary>
            /// <param name="path"></param>
            /// <returns>A master JObject containing the whole scene</returns>
            public JObject ResolveJsonPaths(string path)
            {
                //check is python is installed
                string python_path = "";
                //get current dir
                string current_dir = Directory.GetCurrentDirectory();

                //get python exec
                string python_exec_path = other_utilities.find_python_exec();
                if (python_exec_path == "null")
                {
                    // fallback to embeded exe instead
                    Debug.WriteLine("Python is not installed :(");
                    python_path = current_dir + """\Execs\load_scene_helper.exe""";
                }
                else
                {
                    Debug.WriteLine($"Python is installed on {python_exec_path}");
                    python_path = python_exec_path;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = python_path,
                    Arguments = $"{path}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Python error: {error}");
                }

                Debug.WriteLine($"{output}");
                
                var deserialized = JsonConvert.DeserializeObject<dynamic>(output);
                return deserialized;
            }
        }
        #endregion Load Scene
    }


}