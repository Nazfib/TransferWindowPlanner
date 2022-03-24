using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KSP;
using UnityEngine;

using KSPPluginFramework;

namespace TransferWindowPlanner
{
    public struct TransferDeltaVInfo
    {
        public TransferDeltaVInfo(double eject, double insert)
        {
            Eject = eject;
            Insert = insert;
        }

        public double Eject { get; set; }
        public double Insert { get; set; }
        public double Total { get { return this.Eject + this.Insert; } }
    }

    public class TransferDetails
    {
        public TransferDetails(CelestialBody origin, CelestialBody destination, Double ut, Double dt)
            : this()
        {
            this.Origin = origin;
            this.Destination = destination;
            this.DepartureTime = ut;
            this.TravelTime = dt;
        }
        public TransferDetails() { }

        /// <summary>
        /// Travelling from
        /// </summary>
        public CelestialBody Origin { set; get; }
        /// <summary>
        /// Travelling To
        /// </summary>
        public CelestialBody Destination { get; set; }
        /// <summary>
        /// UT that we are departing at - seconds since Epoch
        /// </summary>
        public Double DepartureTime { get; set; }
        /// <summary>
        /// Seconds of travel time
        /// </summary>
        public Double TravelTime { get; set; }

        /// <summary>
        /// Velocity of the Celestial Origin at Entry Point from Lambert Orbit
        /// </summary>
        public Vector3d OriginVelocity { get; set; }
        /// <summary>
        /// Velocity at Entry point of the Orbit from the Lambert Solver
        /// </summary>
        public Vector3d TransferInitalVelocity { get; set; }
        /// <summary>
        /// Velocity at Exit point of the Orbit from the Lambert Solver
        /// </summary>
        public Vector3d TransferFinalVelocity { get; set; }
        /// <summary>
        /// Velocity of the Celestial Destination at Exit Point from Lambert Orbit
        /// </summary>
        public Vector3d DestinationVelocity { get; set; }

        /// <summary>
        /// velocity in m/s of the vessel in its original orbit before Ejection
        /// </summary>
        public Double OriginVesselOrbitalSpeed { get; set; }
        /// <summary>
        /// velocity in m/s of the vessel in its destination orbit After Injection
        /// </summary>
        public Double DestinationVesselOrbitalSpeed { get; set; }

        /// <summary>
        /// Magnitude of the Ejection Burn
        /// </summary>
        public double DVEjection { get; set; }
        /// <summary>
        /// Magnitude of the Injection Burn
        /// </summary>
        public double DVInjection { get; set; }
        /// <summary>
        /// Magnitude of all burns
        /// </summary>
        public double DVTotal { get { return DVEjection + DVInjection; } }


        public Double PhaseAngle { get { return PhaseAngleCalc(Origin.orbit, Destination.orbit, DepartureTime); } }
        /// <summary>
        /// How far around the Transfer Orbit will we travel in radians
        /// </summary>
        public Double TransferAngle { get; set; }

        /// <summary>
        /// Radius of the parking orbit around the departure body
        /// </summary>
        public Double ParkingSemiMajorAxis { get; set; }

        /// <summary>
        /// Inclination of the escape orbit (and thus the parking orbit)
        /// </summary>
        public Double EjectionInclination { get; set; }

        /// <summary>
        /// Longitude of the ascending node of the escape orbit (and thus the parking orbit)
        /// </summary>
        public Double EjectionLongitudeOfAscendingNode { get; set; }

        /// <summary>
        /// Minimum reachable inclination of the insertion orbit at the target
        /// </summary>
        public Double InsertionInclination { get; set; }

        /// <summary>
        /// Velocity Vector for Ejection - Basically Diff between Transfer Orbit and Planet Orbit velocities
        /// </summary>
        public Vector3d EjectionVector { get { return TransferInitalVelocity - OriginVelocity; } }

        /// <summary>
        /// Angle between periapsis and the asymptote of the escape orbit
        /// </summary>
        public Double EjectionAngle { get; set; }

        /// <summary>
        /// Unit vector in the direction of the periapsis of the departure orbit
        /// </summary>
        public Vector3d PeriDirection { get; set; }

        private Double PhaseAngleCalc(Orbit o1, Orbit o2, Double UT)
        {
            Vector3d n = o1.GetOrbitNormal();

            Vector3d p1 = o1.getRelativePositionAtUT(UT);
            Vector3d p2 = o2.getRelativePositionAtUT(UT);
            double phaseAngle = Vector3d.Angle(p1, p2);
            if (Vector3d.Angle(Quaternion.AngleAxis(90, Vector3d.forward) * p1, p2) > 90)
            {
                phaseAngle = 360 - phaseAngle;
            }

            if (o2.semiMajorAxis < o1.semiMajorAxis)
            {
                phaseAngle = phaseAngle - 360;
            }

            return LambertSolver.Deg2Rad * phaseAngle;
            //return LambertSolver.Deg2Rad * ((phaseAngle + 360) % 360);
        }



        internal string TransferDetailsText
        {
            get
            {

                String Message = ""; //String.Format("{0} (@{2:0}km) -> {1} (@{3:0}km)", this.OriginName, TransferSpecs.DestinationName, TransferSpecs.InitialOrbitAltitude / 1000, TransferSpecs.FinalOrbitAltitude / 1000);
                //Message = Message.AppendLine("Depart at:      {0}", KSPTime.PrintDate(new KSPTime(this.DepartureTime), KSPTime.PrintTimeFormat.DateTimeString));
                Message = Message.AppendLine("Depart at:      {0}", new KSPDateTime(this.DepartureTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat));
                Message = Message.AppendLine("       UT:      {0:0}", this.DepartureTime);
                //Message = Message.AppendLine("   Travel:      {0}", new KSPTime(this.TravelTime).IntervalStringLongTrimYears());
                Message = Message.AppendLine("   Travel:      {0}", new KSPTimeSpan(this.TravelTime).ToStringStandard(TimeSpanStringFormatsEnum.IntervalLongTrimYears));
                Message = Message.AppendLine("       UT:      {0:0}", this.TravelTime);
                //Message = Message.AppendLine("Arrive at:      {0}", KSPTime.PrintDate(new KSPTime(this.DepartureTime + this.TravelTime), KSPTime.PrintTimeFormat.DateTimeString));
                Message = Message.AppendLine("Arrive at:      {0}", new KSPDateTime(this.DepartureTime + this.TravelTime).ToStringStandard(DateStringFormatsEnum.DateTimeFormat));
                Message = Message.AppendLine("       UT:      {0:0}", this.DepartureTime + this.TravelTime);
                Message = Message.AppendLine("Phase Angle:    {0:0.00}°", this.PhaseAngle * LambertSolver.Rad2Deg);
                //Message = Message.AppendLine("Ejection Angle: {0:0.00}°", this.EjectionAngle * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Ejection Angle: {0:0.00}°", this.EjectionAngle);
                Message = Message.AppendLine("Ejection Inc.:  {0:0.00}°", this.EjectionInclination * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Ejection LAN:   {0:0.00}°", this.EjectionLongitudeOfAscendingNode * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Ejection Δv:    {0:0} m/s", this.DVEjection);
                Message = Message.AppendLine("Insertion Inc.: {0:0.00}°", this.InsertionInclination * LambertSolver.Rad2Deg);
                Message = Message.AppendLine("Insertion Δv:   {0:0} m/s", this.DVInjection);
                Message = Message.AppendLine("Total Δv:       {0:0} m/s", this.DVTotal);
                return Message;
            }
        }

    }
}
