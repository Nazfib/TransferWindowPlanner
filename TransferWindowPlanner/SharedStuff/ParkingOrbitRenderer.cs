using KSP;
using UnityEngine;

namespace TransferWindowPlanner
{
    public class ParkingOrbitRenderer : OrbitTargetRenderer
    {
        public static ParkingOrbitRenderer Setup(CelestialBody cb, double alt, double inc, double lan, bool activedraw = true)
        {
            Orbit orbit = new Orbit(inc, 0, cb.Radius + alt, lan, 0, 0, 0, cb);
            return OrbitTargetRenderer.Setup<ParkingOrbitRenderer>("ParkingOrbit", 0, orbit, activedraw);
        }

        protected override void UpdateLocals()
        {
            this.targetVessel = FlightGlobals.ActiveVessel;
            base.UpdateLocals();
        }
    }
}
