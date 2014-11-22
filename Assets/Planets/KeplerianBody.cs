using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Planets
{
    public class KeplerianBody : MonoBehaviour, IBody
    {
        const double G = 6.6725985e-11;

        public static float TimeScale = 100;
        public static float SpaceScale = 1000;

        /// <summary>
        /// Mass 
        /// </summary>
        [Tooltip("Mass in Kg")]
        public float Mass;

        /// <summary>
        /// Eccentricity (e)
        /// </summary>
        [Tooltip("Eccentricity (e)")]
        public double Eccentricity;

        /// <summary>
        /// Semimajor Axis (a) in km
        /// </summary>
        [Tooltip("Semimajor Axis (a) in km")]
        public double SemimajorAxis;

        /// <summary>
        /// Inclination (i) in degrees
        /// </summary>
        [Tooltip("Inclination (i) in degrees")]
        public double Inclination;

        /// <summary>
        /// Longitude of the ascending node (Ω) in degrees
        /// </summary>
        [Tooltip("Longitude of the ascending node (Ω) in degrees")]
        public double LongitudeOfAscendingNode;

        /// <summary>
        /// Argument of periapsis (w) in degrees
        /// </summary>
        [Tooltip("Argument of periapsis (w) in degrees")]
        public double ArgumentOfPeriapsis;

        /// <summary>
        /// Mean anomaly (M0) in degrees
        /// </summary>
        [Tooltip("Mean anomaly (M0) in degrees")]
        public double MeanAnomaly;

        /// <summary>
        /// Cretesian position in km
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Cretesian velocity in km/s
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Vector orthogonal to the orbital plane with body moving according to the right-hand rule
        /// </summary>
        public Vector3 Normal { get; private set; }

        /// <summary>
        /// Vector orthogonal to the orbital plane with body moving oposite the right-hand rule
        /// </summary>
        public Vector3 AntiNormal { get { return -Normal; } }

        /// <summary>
        /// Vector in the direction of the orbit
        /// </summary>
        public Vector3 Prograde { get; private set; }

        /// <summary>
        /// Vector opposite the direction of the orbit
        /// </summary>
        public Vector3 RetroGrade { get { return -Prograde; } }

        /// <summary>
        /// Vector directly away from the gravity well
        /// </summary>
        public Vector3 RadialOut { get; private set; }

        /// <summary>
        /// Vector directly into the gravity well
        /// </summary>
        public Vector3 RadialIn { get { return -RadialOut; } }

        /// <summary>
        /// Returns the body around which this body is orbiting
        /// </summary>
        public KeplerianBody Parent { get { return _primary; } }

        /// <summary>
        /// The change in Mean Anomaly per second
        /// </summary>
        private double _meanAnomalyDelta;

        /// <summary>
        /// The object this body is orbiting around
        /// </summary>
        private KeplerianBody _primary;

        private static readonly List<KeplerianBody> Bodies= new List<KeplerianBody>();

        /// <summary>
        /// Increment the Mean Anomaly by a specified number of seconds
        /// </summary>
        /// <param name="dt">the incremented time in seconds</param>
        public void IncrementMeanAnomaly(float dt)
        {
            MeanAnomaly += _meanAnomalyDelta * dt;

            if (MeanAnomaly > 360)
                MeanAnomaly -= 360;

            if (MeanAnomaly < 0)
                MeanAnomaly += 360;
        }

        private void CalculateAditionalValues()
        {
            var mu = G * (_primary.Mass + Mass);

            _meanAnomalyDelta = Math.Sqrt(mu / (SemimajorAxis * SemimajorAxis * SemimajorAxis));
        }

        /// <summary>
        /// Update this state vector to match the passed orbital elements
        /// </summary>
        public void UpdateStateVectors(ref Vector3 pos, ref Vector3 vel)
        {
            var a = SemimajorAxis*1000; //Convert to meters
            var ec = Eccentricity;
            var i = Inclination*Mathf.Deg2Rad;
            var w0 = ArgumentOfPeriapsis*Mathf.Deg2Rad;
            var o0 = LongitudeOfAscendingNode*Mathf.Deg2Rad;
            var m0 = MeanAnomaly*Mathf.Deg2Rad;

            var totalMass = _primary.Mass + Mass;

            var eca = m0 + ec/2;
            var diff = 10000.0;
            const double eps = 0.000001;
            double e1;

            while (diff > eps)
            {
                e1 = eca - (eca - ec*Math.Sin(eca) - m0)/(1 - ec*Math.Cos(eca));
                diff = Math.Abs(e1 - eca);
                eca = e1;
            }

            var ceca = Math.Cos(eca);
            var seca = Math.Sin(eca);
            e1 = a*Math.Sqrt(Math.Abs(1 - ec*ec));
            var xw = a*(ceca - ec);
            var yw = e1*seca;

            var edot = Math.Sqrt((G*totalMass)/a)/(a*(1 - ec*ceca));
            var xdw = -a*edot*seca;
            var ydw = e1*edot*ceca;

            var cw = Math.Cos(w0);
            var sw = Math.Sin(w0);
            var co = Math.Cos(o0);
            var so = Math.Sin(o0);
            var ci = Math.Cos(i);
            var si = Math.Sin(i);
            var swci = sw*ci;
            var cwci = cw*ci;
            var pX = cw*co - so*swci;
            var pY = cw*so + co*swci;
            var pZ = sw*si;
            var qx = -sw*co - so*cwci;
            var qy = -sw*so + co*cwci;
            var qz = cw*si;

            //Assign Values
            pos.x = (float) (xw*pX + yw*qx);
            pos.y = (float)(xw * pY + yw * qy);
            pos.z = (float)(xw * pZ + yw * qz);
            pos /= 1000; //Convert back to km

            vel.x = (float) (xdw*pX + ydw*qx);
            vel.y = (float)(xdw * pY + ydw * qy);
            vel.z = (float)(xdw * pZ + ydw * qz);
            vel /= 1000; //Convert back to km/s
        }

        private void UpdateSecondaryVectors()
        {
            RadialOut = Position.normalized;
            Normal = Vector3.Cross(Position, Velocity).normalized;
            Prograde = Vector3.Cross(Normal, Position).normalized;
        }

        public Vector3 GetPositionAtMeanAnomoly(double d)
        {
            var pos = new Vector3();
            var vel = new Vector3();

            var anm = MeanAnomaly;

            MeanAnomaly = d;
            
            UpdateStateVectors(ref pos, ref vel);

            MeanAnomaly = anm;
            return pos;
        }


        // Use this for initialization
        public void Start ()
        {
            if(transform.parent==null)
                return;

            _primary = transform.parent.GetComponent<KeplerianBody>();

            transform.parent = null;

            //If there is no primary then this is the refrence for the system and stays put
            if (_primary == null)
                return;

            CalculateAditionalValues();

            Bodies.Add(this);
            OrthoCam.Instance.AddTargetableBody(this);
        }
	
        // Update is called once per frame
        public void Update () {

            //If there is no primary then this is the refrence for the system and stays put
            if (_primary == null)
            {
                transform.position = OrthoCam.Instance.WorldToScreen(Vector3.zero);
                return;
            }
                

            IncrementMeanAnomaly(Time.deltaTime*TimeScale);

            var pos = Position;
            var vel = Velocity;

            UpdateStateVectors(ref pos, ref vel);

            Position = pos;
            Velocity = vel;

            UpdateSecondaryVectors();

            transform.position = OrthoCam.Instance.WorldToScreen(Position-_primary.Position);
        }

        /// <summary>
        /// Clears tracking lists when this body is destroyed
        /// </summary>
        public void OnDestroy()
        {
            Bodies.Remove(this);
            OrthoCam.Instance.RemoveTargetableBody(this);
        }

        /// <summary>
        /// Gets the summed gravitational force vector at a specific point
        /// </summary>
        /// <param name="pos">the specific point</param>
        /// <returns></returns>
        public static Vector3 GetGravitationalAccelerationAtPosition(Vector3 pos)
        {
            var retVal = new Vector3();

            foreach (var body in Bodies)
            {
                var r = body.Position - pos;

                retVal += r.normalized * (float) G * body.Mass / r.sqrMagnitude;
            }

            return retVal;
        }

    }
}
