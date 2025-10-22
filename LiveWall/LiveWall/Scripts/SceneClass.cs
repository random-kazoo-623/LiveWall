using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LiveWall.Scripts
{
    internal class SceneClass
    {
        public class GifScene
        {
            public CameraData Camera { get; set; }
            public SceneGeneral General { get; set; }

            public List <Dictionary<dynamic, dynamic>> Objects { get; set; }
        }

        public class CameraData
        {
            public string Center { get; set; }
            public string Eye { get; set; }
            public string Up { get; set; }
        }

        #region general property
        public class SceneGeneral
        {
            public string AmbientColor { get; set; }
            public bool Bloom { get; set; }
            public double BloomHdrFeather { get; set; }
            public int BloomHdrIterations { get; set; }
            public double BloomHdrScatter { get; set; }
            public double BloomHdrStrength { get; set; }
            public double BloomHdrThreshold { get; set; }
            public double BloomStrength { get; set; }
            public double BloomThreshold { get; set; }
            public string BloomTint { get; set; }
            public bool CameraFade { get; set; }
            public bool CameraParallax { get; set; }
            public double CameraParallaxAmount { get; set; }
            public double CameraParallaxDelay { get; set; }
            public double CameraParallaxMouseInfluence { get; set; }
            public bool CameraPreview { get; set; }
            public bool CameraShake { get; set; }
            public double CameraShakeAmplitude { get; set; }
            public double CameraShakeRoughness { get; set; }
            public double CameraShakeSpeed { get; set; }
            public string ClearColor { get; set; }
            public bool ClearEnabled { get; set; }
            public double Farz { get; set; }
            public double Fov { get; set; }
            public string GravityDirection { get; set; }
            public double GravityStrength { get; set; }
            public bool Hdr { get; set; }
            public LightConfig LightConfig { get; set; }
            public double Nearz { get; set; }
            public OrthogonalProjection OrthogonalProjection { get; set; }
            public double PerspectiveOverrideFov { get; set; }
            public string SkyLightColor { get; set; }
            public bool SpritesheetRefreshSync { get; set; }
            public string WindDirection { get; set; }
            public bool WindEnabled { get; set; }
            public double WindStrength {  get; set; }
            public double Zoom {  get; set; }


        }

        public class LightConfig
        {
            public int Tube { get; set; }
        }

        public class OrthogonalProjection
        {
            public int Height { get; set; }
            public int Width { get; set; }
        }

        #endregion general property

        #region object property
        public class Objects
        {
            //TODO: IMPLEMENT ALL JSON FIELDS

        }

        #endregion object property
    }


}
