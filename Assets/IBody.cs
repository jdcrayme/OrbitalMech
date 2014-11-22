using System;
using UnityEngine;

namespace Assets
{
    public interface IBody
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }
    }
}
