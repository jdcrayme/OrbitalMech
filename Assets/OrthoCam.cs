using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Planets;
using UnityEngine;

namespace Assets
{
    public class OrthoCam : MonoBehaviour {

        public static OrthoCam Instance;

        #region Fields

        public int MinSize = 1;         // The minimum and maximum zoom factor exponents.
        public int MaxSize = 10;        // I.E. 1 gives a orthographic width of 10, 2 gives 100 3 gives 1000 etc.

        public IBody Focus;             // The thing the camera is looking at

        private Camera _cam;            // The camera componant
        private float _size;            // The ortho size
        private float _scale;           // The ortho scale
        private float _sliderValue = 1; // The zoom slider value

        private int _targetBodyIndex = 0;
        private readonly List<IBody> _targetableBodies = new List<IBody>();

        #endregion 

        #region Getters and Setters

        public float Scale { get { return 1.0f / _size; } }
        public float Size { get { return _cam.orthographicSize * _size; } }

        #endregion

        #region Constructor
        public OrthoCam()
        {
            Instance = this;
        }
        #endregion


        #region Public Methods

        /// <summary>
        /// Initializes the camera
        /// </summary>
        public void Start ()
        {
            _cam = GetComponent<Camera>();
            AdjustScale(1);

        }

        /// <summary>
        /// Draws the scale control 
        /// </summary>
        public void OnGUI()
        {
            var newSliderVal = GUI.VerticalSlider(new Rect(25, 25, 100, 300), _sliderValue, MaxSize, MinSize);

            if (Math.Abs(newSliderVal - _sliderValue) > 0.0001)
                AdjustScale(newSliderVal);

            if (Input.GetKeyDown(KeyCode.N))
            {
                _targetBodyIndex++;
                if (_targetBodyIndex >= _targetableBodies.Count)
                    _targetBodyIndex = 0;

                Focus = _targetableBodies[_targetBodyIndex];

                Debug.Log("Tracking "+((MonoBehaviour)Focus).gameObject.name);
            }
        }

        /// <summary>
        /// Converts a bodies world position to a screen position based on what the camera is doing
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vector3 WorldToScreen(Vector3 pos)
        {
            var offset = Vector3.zero;
            if (Focus != null)
                offset = Focus.Position;

            return (pos - offset)*_scale;
        }

        public void AddTargetableBody(IBody body)
        {
            _targetableBodies.Add(body);
        }

        public void RemoveTargetableBody(IBody body)
        {
            _targetableBodies.Remove(body);
        }


        /// <summary>
        /// Update is called once per frame 
        /// </summary>
        public void Update () {

        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Adjusts the scale of the camera based on the GUI slider position
        /// </summary>
        /// <param name="sliderPosition"></param>
        private void AdjustScale(float sliderPosition)
        {
            _sliderValue = sliderPosition;
            _size = Mathf.Pow(10, _sliderValue);
            _scale = 1.0f/_size;
        }

        #endregion
    }
}
