using UnityEngine;

namespace Assets.Planets
{
    public class KeplerianBodyTrail : MonoBehaviour
    {
        public int Samples = 360;

        private LineRenderer _trail;
        private KeplerianBody _body;

        // Use this for initialization
        public void Start ()
        {
            _trail = GetComponent<LineRenderer>();
            _body = GetComponentInParent<KeplerianBody>();
        }
	
        // Update is called once per frame
        public void Update () {
            //Make sure the number of verticies is correct.
            _trail.SetVertexCount(Samples);

            //Get the correct elements and the starting Anomaly
            var startingMeanAnomoly = _body.MeanAnomaly;

            //If the body we are trailing is orbiting another body, make sure we take that into account
            var parentPos = Vector3.zero;
            if (_body.Parent != null)
                parentPos = _body.Parent.Position;

            //Plot the points
            for (var i = 0; i < Samples; i++)
            {
                var position = OrthoCam.Instance.WorldToScreen(_body.GetPositionAtMeanAnomoly(startingMeanAnomoly + i) - parentPos);
                _trail.SetPosition(i, position);
            }
        }
    }
}
