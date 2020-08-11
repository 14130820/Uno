using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Uno.Game
{
    public class LerpMovement : MonoBehaviour
    {
        private Transform thisTransform;
        private Vector3 startPosition;
        private Vector3 targetPosition;

        private Quaternion startRotation;
        private Quaternion targetRotation;

        private float currentLerpTime = 0;
        private float speed;

        private Vector3BezierCurve bezierCurve;
        private LerpCurve lerpCurve;

        private bool updatePhysics = true;
        private bool atPosition = true;
        
        private void Awake()
        {
            thisTransform = this.transform;
        }
        
        private void Update()
        {
            if (!atPosition)
            {
                currentLerpTime += Time.deltaTime;
                if (currentLerpTime > speed)
                {
                    currentLerpTime = speed;
                    atPosition = true;
                }

                var percentage = lerpCurve.ApplyCurve(currentLerpTime / speed);
                thisTransform.localPosition = bezierCurve.ApplyCurve(startPosition, targetPosition, percentage);

                thisTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, percentage);

                if (updatePhysics) GameManager.Instance.UpdatePhysics();
            }
        }
        
        public void MoveTo(Vector3 position, Quaternion rotation, Vector3BezierCurve bezierCurve, LerpCurve lerpCurve, float speed = 0.5f, bool updatePhysics = true)
        {
            this.updatePhysics = updatePhysics;
            this.bezierCurve = bezierCurve;
            this.lerpCurve = lerpCurve;
            this.speed = speed;

            targetPosition = position;
            startPosition = thisTransform.localPosition;

            targetRotation = rotation;
            startRotation = thisTransform.localRotation;

            atPosition = false;
            currentLerpTime = 0;
        }

        public class LerpCurve
        {
            public static readonly LerpCurve Default = new LerpCurve();

            public enum Curve : byte
            {
                None,
                SmoothStart,
                SmoothEnd,
                Both
            }

            private int curveStrength;
            private Curve curveType;

            public LerpCurve()
            {
                curveType = Curve.None;
            }
            public LerpCurve(int curveStrength, Curve curveType)
            {
                this.curveStrength = curveStrength;
                this.curveType = curveType;
            }

            public float ApplyCurve(float time)
            {
                switch (curveType)
                {
                    case Curve.SmoothStart:
                        return Mathf.Pow(1f - Mathf.Cos(time * Mathf.PI * 0.5f), curveStrength);
                    case Curve.SmoothEnd:
                        return Mathf.Pow(Mathf.Sin(time * Mathf.PI * 0.5f), curveStrength);
                    case Curve.Both:
                        var t = Mathf.Pow(Mathf.Sin(time * Mathf.PI * 0.5f), curveStrength);
                        return t * t * (3f - 2f * t);
                }
                return time;
            }
        }

        /// <summary>
        /// Used for lerping an object between positions with a position offset.
        /// </summary>
        public class Vector3BezierCurve
        {
            public static readonly Vector3BezierCurve Default = new Vector3BezierCurve();

            private enum Curve
            {
                None,
                Bezier2,
                Bezier3
            }

            private readonly Curve curveType;
            private readonly Vector3 firstValue;
            private readonly Vector3 secondValue;

            public Vector3BezierCurve()
            {
                curveType = Curve.None;
            }
            public Vector3BezierCurve(Vector3 firstValue)
            {
                curveType = Curve.Bezier2;
                this.firstValue = firstValue;
            }
            public Vector3BezierCurve(Vector3 firstValue, Vector3 secondValue)
            {
                curveType = Curve.Bezier3;
                this.firstValue = firstValue;
                this.secondValue = secondValue;
            }

            public Vector3 ApplyCurve(Vector3 start, Vector3 end, float t)
            {
                switch (curveType)
                {
                    case Curve.None:
                        return Vector3.Lerp(start, end, t);
                    case Curve.Bezier2:
                        return CubeBezier2(start, firstValue, end, t);
                    case Curve.Bezier3:
                        return CubeBezier3(start, firstValue, secondValue, end, t);

                }

                return default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Vector3 CubeBezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
            {
                float r = 1f - t;
                float f0 = r * r;
                float f1 = r * t * 2;
                float f2 = t * t * t;
                return f0 * p0 + f1 * p1 + f2 * p2;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Vector3 CubeBezier3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
            {
                float r = 1f - t;
                float f0 = r * r * r;
                float f1 = r * r * t * 3;
                float f2 = r * t * t * 3;
                float f3 = t * t * t;
                return f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
            }
        }
    }
}
