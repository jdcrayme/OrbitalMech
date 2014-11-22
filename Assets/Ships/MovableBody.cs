using System;
using Assets.Planets;
using UnityEngine;

namespace Assets.Ships
{
    public class MovableBody : MonoBehaviour, IBody
    {
    
        public const int PathSize = 10000;
        private readonly StateVector[] _path = new StateVector[PathSize];

        public Vector3 Position { get { return State.Position/1000; } }
        public Vector3 Velocity { get { return State.Velocity/1000; } }

        public Vector3 GetPositionPath(int i)
        {
            if (i <= PathSize && _path[i] != null)
                return _path[i].Position/1000;

            return Vector3.zero;
        }

        public StateVector State;

        [Serializable]
        public class StateVector
        {
            public Vector3 Position = Vector3.forward;
            public Vector3 Velocity = new Vector3();
        }

        // Use this for initialization
        public void Start ()
        {
            OrthoCam.Instance.AddTargetableBody(this);
        }
	
        // Update is called once per frame
        public void Update()
        {
            var lastState = State;

            _path[0] = State;

            for (var i = 1; i < PathSize; i++)
            {
                _path[i] = lastState = CalculateNewPosition(lastState, 3600);
            }

            State = CalculateNewPosition(State, Time.deltaTime*1000);
            
            transform.position = OrthoCam.Instance.WorldToScreen(Position);
        }

        /// <summary>
        /// Clears tracking lists when this body is destroyed
        /// </summary>
        public void OnDestroy()
        {
            OrthoCam.Instance.RemoveTargetableBody(this);
        }


        private static StateVector CalculateNewPosition(StateVector state, float delta)
        {
            var newState = new StateVector();

            var a1 = KeplerianBody.GetGravitationalAccelerationAtPosition(state.Position);
            
            newState.Position = state.Position + state.Velocity*delta + a1*delta*delta;

            var a2 = KeplerianBody.GetGravitationalAccelerationAtPosition(newState.Position);

            newState.Velocity = state.Velocity + (a1 + a2)*delta/2;

            return newState;
        }
    }
}
