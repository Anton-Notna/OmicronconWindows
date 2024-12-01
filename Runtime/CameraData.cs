using System;
using UnityEngine;

namespace OmicronWindows
{
    public struct CameraData
    {
        public float NearClipPlane;
        public float FarClipPlane;
        public float OrthographicSize;

        public CameraData(Camera camera)
        {
            NearClipPlane = camera.nearClipPlane;
            FarClipPlane = camera.farClipPlane;
            OrthographicSize = camera.orthographicSize;
        }

        public static bool operator !=(CameraData d1, CameraData d2) => (d1 == d2) == false;

        public static bool operator ==(CameraData d1, CameraData d2) => d1.NearClipPlane == d2.NearClipPlane && d1.FarClipPlane == d2.FarClipPlane && d1.OrthographicSize == d2.OrthographicSize;

        public override bool Equals(object obj) => obj is CameraData data && this == data;

        public override int GetHashCode() => HashCode.Combine(NearClipPlane, FarClipPlane, OrthographicSize);
    }
}