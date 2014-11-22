using UnityEngine;

namespace Assets.Ships
{
    public class MovableBodyPredictionLine : MonoBehaviour {

        private LineRenderer _trail;
        private MovableBody _body;

        // Use this for initialization
        public void Start()
        {
            _trail = GetComponent<LineRenderer>();
            _body = GetComponentInParent<MovableBody>();
        }

        // Update is called once per frame
        public void Update()
        {
            //Make sure the number of verticies is correct.
            _trail.SetVertexCount(MovableBody.PathSize);
            
            //Plot the points
            for (var i = 0; i < MovableBody.PathSize; i++)
            {
                var position = OrthoCam.Instance.WorldToScreen(_body.GetPositionPath(i));
                _trail.SetPosition(i, position);
            }
        }
    }
}
