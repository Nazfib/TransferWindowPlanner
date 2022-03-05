using KSP;
using KSP.Localization;
using KSP.UI.Screens.Mapview;
using UnityEngine;

namespace TransferWindowPlanner
{
    public class ParkingOrbitRenderer : OrbitTargetRenderer
    {
        private TWPWindow _window;

        public static ParkingOrbitRenderer Setup(TWPWindow window, bool activedraw = true)
        {
            var cb = FlightGlobals.GetHomeBody();
            var orbit = new Orbit(0, 0, 5 * cb.Radius, 0, 0, 0, 0, cb);
            var obj = OrbitTargetRenderer.Setup<ParkingOrbitRenderer>("ParkingOrbit", 0, orbit, activedraw);
            obj._window = window;
            return obj;
        }

        protected override void UpdateLocals()
        {
            var transferSelected = _window.TransferSelected;
            if (transferSelected != null && _window.blnDisplayParkingOrbit)
            {
                snapshot.ReferenceBodyIndex = transferSelected.Origin.flightGlobalsIndex;
                snapshot.semiMajorAxis = transferSelected.ParkingSemiMajorAxis;
                snapshot.inclination = transferSelected.EjectionInclination * LambertSolver.Rad2Deg;
                snapshot.LAN = transferSelected.EjectionLongitudeOfAscendingNode * LambertSolver.Rad2Deg;
                activeDraw = true;
            }
            else
            {
                activeDraw = false;
            }
            targetVessel = FlightGlobals.ActiveVessel;
            base.UpdateLocals();
        }

        protected override void ascNode_OnUpdateCaption(MapNode n, MapNode.CaptionData data)
        {
            if (!activeDraw) return;
            data.Header = Localizer.Format("#autoLOC_277932", // <<1>>Ascending Node: <<2>>°<<3>>
                startColor, relativeInclination.ToString("0.0"), "</color>");
        }

        protected override void descNode_OnUpdateCaption(MapNode n, MapNode.CaptionData data)
        {
            if (!activeDraw) return;
            data.Header = Localizer.Format("#autoLOC_277943", // <<1>>Descending Node: <<2>>°<<3>>
                startColor, (-relativeInclination).ToString("0.0"), "</color>");
        }
    }
}
