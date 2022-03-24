using System;
using UnityEngine;
using KSPPluginFramework;

namespace TransferWindowPlanner
{
    public class AngleRenderEject2: MonoBehaviourExtended
    {
        public Boolean isDrawing { get; private set; }

        /// <summary>
        /// Is the angle drawn and visible on screen
        /// </summary>
        public Boolean isVisible { get { return isAngleVisible; } }

        public Boolean isAngleVisible { get; private set; }

        /// <summary>
        /// Is the angle in the process of becoming visible
        /// </summary>
        public Boolean isBecomingVisible
        {
            get { return _isBecomingVisible; }
        }
        private Boolean _isBecomingVisible = false;
        private Boolean _isBecomingVisible_LinesDone = false;
        private Boolean _isBecomingVisible_ArcDone = false;

        private Boolean _isHiding = false;


        /// <summary>
        /// Is the angle in the process of being hidden
        /// </summary>
        public Boolean IsBecomingInvisible { get; private set; }
        private DateTime StartDrawing;

        /// <summary>
        /// The Body we are measuring from
        /// </summary>
        public CelestialBody bodyOrigin { get; set; }

        /// <summary>
        /// The direction of the asymptote, in the body-centered intertial frame. This is the direction of the relative
        /// velocity after escaping the departure planet's SoI.
        /// </summary>
        public Vector3d AsymptoteDirection { get; set; }
        /// <summary>
        /// The direction of the periapsis of the transfer orbit.
        /// </summary>
        public Vector3d PeriapsisDirection { get; set; }

        private GameObject objLineStart;
        private GameObject objLineEnd;
        private GameObject objLineArc;

        private LineRenderer lineStart = null;
        private LineRenderer lineEnd = null;
        internal LineRenderer lineArc = null;


        internal PlanetariumCamera cam;

        internal Int32 ArcPoints = 72;
        internal Int32 StartWidth = 10;
        internal Int32 EndWidth = 10;

        private GUIStyle styleLabelEnd;
        private GUIStyle styleLabelTarget;

        internal override void Start()
        {
            base.Start();

            if (!TransferWindowPlanner.lstScenesForAngles.Contains(HighLogic.LoadedScene))
            {
                this.enabled = false;
                return;
            }

            LogFormatted("Initializing EjectAngle Render");
            objLineStart = new GameObject("LineStart");
            objLineEnd = new GameObject("LineEnd");
            objLineArc = new GameObject("LineArc");

            //Get the orbit lines material so things look similar
            Material orbitLines = ((MapView)GameObject.FindObjectOfType(typeof(MapView))).orbitLinesMaterial;

            //Material dottedLines = ((MapView)GameObject.FindObjectOfType(typeof(MapView))).dottedLineMaterial;    //Commented because usage removed

            //init all the lines
            lineStart = InitLine(objLineStart, Color.blue, 2, 10, orbitLines);

            lineEnd = InitLine(objLineEnd, Color.red, 2, 10, orbitLines);
            lineArc = InitLine(objLineArc, Color.green, ArcPoints, 10, orbitLines);

            styleLabelEnd = new GUIStyle
            {
                normal = {textColor = Color.white},
                alignment = TextAnchor.MiddleCenter
            };
            styleLabelTarget = new GUIStyle
            {
                normal = {textColor = Color.white},
                alignment = TextAnchor.MiddleCenter
            };

            //get the map camera - well need this for distance/width calcs
            cam = (PlanetariumCamera)GameObject.FindObjectOfType(typeof(PlanetariumCamera));
        }

        /// <summary>
        /// Initialise a LineRenderer with some basic values
        /// </summary>
        /// <param name="objToAttach">GameObject that renderer is attached to - one linerenderer per object</param>
        /// <param name="lineColor">Draw this color</param>
        /// <param name="VertexCount">How many vertices make up the line</param>
        /// <param name="InitialWidth">line width</param>
        /// <param name="linesMaterial">Line material</param>
        /// <returns></returns>
        private LineRenderer InitLine(GameObject objToAttach, Color lineColor, Int32 VertexCount, Int32 InitialWidth, Material linesMaterial)
        {
            objToAttach.layer = 9;
            LineRenderer lineReturn = objToAttach.AddComponent<LineRenderer>();

            lineReturn.material = linesMaterial;
            //lineReturn.SetColors(lineColor, lineColor);
            lineReturn.startColor = lineColor;
            lineReturn.endColor = lineColor;
            lineReturn.transform.parent = null;
            lineReturn.useWorldSpace = true;
            //lineReturn.SetWidth(InitialWidth, InitialWidth);
            lineReturn.startWidth = InitialWidth;
            lineReturn.endWidth = InitialWidth;
            //lineReturn.SetVertexCount(VertexCount);
            lineReturn.positionCount = VertexCount;
            lineReturn.enabled = false;

            return lineReturn;
        }



        internal override void OnDestroy()
        {
            base.OnDestroy();

            _isBecomingVisible = false;
            _isBecomingVisible_LinesDone = false;
            _isBecomingVisible_ArcDone = false;
            _isHiding = false;
            isDrawing = false;

            //Bin the objects
            lineStart = null;
            lineEnd = null;
            lineArc = null;

            objLineStart.DestroyGameObject();
            objLineEnd.DestroyGameObject();
            objLineArc.DestroyGameObject();
        }

        public void DrawAngle(CelestialBody bodyOrigin, Vector3d asymptote, Vector3d periapsis)
        {
            this.bodyOrigin = bodyOrigin;
            this.AsymptoteDirection = asymptote.normalized;
            this.PeriapsisDirection = periapsis.normalized;

            isDrawing = true;
            StartDrawing = DateTime.Now;
            _isBecomingVisible = true;
            _isBecomingVisible_LinesDone = false;
            _isBecomingVisible_ArcDone = false;
            _isHiding = false;
        }

        public void HideAngle()
        {
            StartDrawing = DateTime.Now;
            _isHiding = true;
            //isDrawing = false;
        }

        //keeps angles in the range 0 to 360
        internal static double ClampDegrees360(double angle)
        {
            angle = angle % 360.0;
            if (angle < 0) return angle + 360.0;
            else return angle;
        }

        //keeps angles in the range -180 to 180
        internal static double ClampDegrees180(double angle)
        {
            angle = ClampDegrees360(angle);
            if (angle > 180) angle -= 360;
            return angle;
        }

        private static Vector3d VectToUnityFrame(Vector3d v)
        {
            v.Swizzle(); // Swap y and z axes
            v = Planetarium.Rotation * v;
            return v;
        }

        internal override void OnPreCull()
        {
            base.OnPreCull();

            //not sure if this is right - but its working
            if (!TransferWindowPlanner.lstScenesForAngles.Contains(HighLogic.LoadedScene))
            {
                return;
            }

            if (MapView.MapIsEnabled && isDrawing)
            {
                double lineLength = bodyOrigin.Radius * 5;
                Vector3d vectAsymptote = VectToUnityFrame(this.AsymptoteDirection.normalized);
                Vector3d vectPeriapsis = VectToUnityFrame(this.PeriapsisDirection.normalized);

                //Are we Showing, Hiding or Static State
                if (_isHiding)
                {
                    Single pctDone = (Single)(DateTime.Now - StartDrawing).TotalSeconds / 0.25f;
                    if (pctDone >= 1)
                    {
                        _isHiding = false;
                        isDrawing = false;
                    }
                    pctDone = Mathf.Clamp01(pctDone);
                    DrawLine(lineStart, Vector3d.zero, vectAsymptote * Mathf.Lerp((Single)lineLength, 0, pctDone));
                    DrawLine(lineEnd, Vector3d.zero, vectPeriapsis * Mathf.Lerp((Single)lineLength, 0, pctDone));
                    DrawArc2(lineArc, vectAsymptote, vectPeriapsis, Mathf.Lerp((Single) bodyOrigin.Radius * 3, 0, pctDone));
                }
                else if (isBecomingVisible)
                {
                    if (!_isBecomingVisible_LinesDone)
                    {
                        Single pctDone = (Single)(DateTime.Now - StartDrawing).TotalSeconds / 0.5f;
                        if (pctDone >= 1)
                        {
                            _isBecomingVisible_LinesDone = true;
                            StartDrawing = DateTime.Now;
                        }
                        pctDone = Mathf.Clamp01(pctDone);

                        Vector3d vectAsymptoteWorking =
                            vectAsymptote * Mathf.Lerp(0, (Single) lineLength, pctDone);
                        DrawLine(lineStart, Vector3d.zero, vectAsymptoteWorking);
                    }
                    else if (!_isBecomingVisible_ArcDone)
                    {
                        Single pctDone = (Single)(DateTime.Now - StartDrawing).TotalSeconds / 0.5f;
                        if (pctDone >= 1)
                        {
                            _isBecomingVisible_ArcDone = true;
                            _isBecomingVisible = false;
                        }
                        pctDone = Mathf.Clamp01(pctDone);

                        Vector3d vectPeriapsisWorking = Vector3.Slerp(vectAsymptote, vectPeriapsis, pctDone);

                        //draw the origin and end lines
                        DrawLine(lineStart, Vector3d.zero, vectAsymptote * lineLength);
                        DrawLine(lineEnd, Vector3d.zero, vectPeriapsisWorking * lineLength);
                        DrawArc2(lineArc, vectAsymptote, vectPeriapsisWorking, bodyOrigin.Radius * 3);
                    }
                }
                else
                {
                    DrawLine(lineStart, Vector3d.zero, vectAsymptote * lineLength);
                    DrawLine(lineEnd, Vector3d.zero, vectPeriapsis * lineLength);
                    DrawArc2(lineArc, vectAsymptote, vectPeriapsis, bodyOrigin.Radius * 3);
                }
            }
            else
            {
                lineStart.enabled = false;
                lineEnd.enabled = false;
                lineArc.enabled = false;
            }
        }

        internal override void OnGUIEvery()
        {
            if (MapView.MapIsEnabled && isDrawing && !_isBecomingVisible && !_isHiding)
            {
                Vector3d center = bodyOrigin.transform.position;
                double length = 5 * bodyOrigin.Radius;
                Vector3 asymptote = PlanetariumCamera.Camera.WorldToScreenPoint(
                    ScaledSpace.LocalToScaledSpace(
                        center + length * VectToUnityFrame(AsymptoteDirection.normalized)));
                Vector3 periapsis = PlanetariumCamera.Camera.WorldToScreenPoint(
                    ScaledSpace.LocalToScaledSpace(
                        center + length * VectToUnityFrame(PeriapsisDirection.normalized)));
                double angle = Vector3d.Angle(AsymptoteDirection, PeriapsisDirection);

                GUI.Label(new Rect(
                    periapsis.x - 50,
                    Screen.height - periapsis.y - 15,
                    100, 30),
                    $"Burn position", styleLabelEnd);

                GUI.Label(new Rect(
                    asymptote.x - 50,
                    Screen.height - asymptote.y - 15,
                    100, 30),
                    "Escape direction", styleLabelTarget);
            }
        }

        private void DrawArc2(LineRenderer line, Vector3d vectStart, Vector3d vectEnd, Double radius)
        {
            Vector3d center = bodyOrigin.transform.position;
            for (int i = 0; i < ArcPoints; i++)
            {
                float t = (float) i / (ArcPoints - 1);
                Vector3d vectArc = Vector3.Slerp(vectStart, vectEnd, t);
                line.SetPosition(i, ScaledSpace.LocalToScaledSpace(center + vectArc * radius));
            }

            line.startWidth = line.endWidth = 10f / 1000f * cam.Distance;
            line.enabled = true;
        }

        private void DrawLine(LineRenderer line, Vector3d pointStart, Vector3d pointEnd)
        {
            Vector3d center = bodyOrigin.transform.position;
            line.SetPosition(0, ScaledSpace.LocalToScaledSpace(center + pointStart));
            line.SetPosition(1, ScaledSpace.LocalToScaledSpace(center + pointEnd));
            line.startWidth = line.endWidth = 10f / 1000f * cam.Distance;
            line.enabled = true;
        }
    }
}
