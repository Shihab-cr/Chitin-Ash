using System;
using UnityEngine;

namespace Game.Spawning
{
    [Serializable]
    public class SpawnPointValidator //made as a separate class for"Single Responsibility Principle"
    {
        [SerializeField] private LayerMask entityLayerMask; //Layer mask containing enemies, larvae, and player colliders for overlap checks
        [SerializeField] private LayerMask obstacleLayerMask = ~0; //walls , terrains obstcles etc...
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField, Min(0.1f)] private float clearanceRadius = 0.75f;
        [SerializeField, Min(0.1f)] private float entityHeight = 1.8f;
        [SerializeField, Min(0f)] private float minPlayerDistance = 4f; //to prevent point-blank spawns
        [SerializeField, Range(0f, 180f)] private float forwardConeHalfAngle = 60f;
        [SerializeField, Range(0f, 0.25f)] private float viewportMargin = 0.05f;
        [SerializeField] private bool snapToGround = true;

        public bool TryFindPoint(Vector3 center, float radius, Transform player, Camera cam, int attempts, out Vector3 point)
        {//Searches for a valid spawn coordinate and writes it to out Vector3 point on success

            for (int i = 0; i < attempts; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;
                Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

                if (snapToGround && !TrySnapToGround(candidate, out candidate)) continue;
                if (IsVisibleToCamera(candidate, cam)) continue;
                if (IsTooCloseOrInForwardCone(candidate, player)) continue;
                if (HasOverlap(candidate)) continue;

                point = candidate;
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        private bool TrySnapToGround(Vector3 p, out Vector3 grounded)
        {//Projects p vertically onto the ground below it

            if (Physics.Raycast(p + Vector3.up * 20f, Vector3.down, out RaycastHit hit, 40f, groundMask, QueryTriggerInteraction.Ignore))
            {
                grounded = hit.point + Vector3.up * 0.05f;
                return true;
            }
            grounded = p;
            return false;
        }

        private bool IsVisibleToCamera(Vector3 groundPoint, Camera cam) =>
            //Tests visibility of both the bottom and top of the entity
            IsPointInViewport(groundPoint, cam) ||
            IsPointInViewport(groundPoint + Vector3.up * entityHeight, cam);

        private bool IsPointInViewport(Vector3 p, Camera cam)
        {  //Checks whether a single world point lies inside the camera 

            Vector3 vp = cam.WorldToViewportPoint(p);
            return vp.z > 0f && vp.z <= cam.farClipPlane &&
                   vp.x >= -viewportMargin && vp.x <= 1f + viewportMargin &&
                   vp.y >= -viewportMargin && vp.y <= 1f + viewportMargin;
        }

        private bool IsTooCloseOrInForwardCone(Vector3 p, Transform player)
        {  ///Checks proximity and forward-facing angle
            Vector3 toPoint = Vector3.ProjectOnPlane(p - player.position, Vector3.up);
            if (toPoint.sqrMagnitude < minPlayerDistance * minPlayerDistance) return true;

            Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up); //Flattens the player's facing direction onto the horizontal plane
            if (forward.sqrMagnitude < 0.0001f) return false;

            return Vector3.Dot(forward.normalized, toPoint.normalized) > Mathf.Cos(forwardConeHalfAngle * Mathf.Deg2Rad);
        }

        private bool HasOverlap(Vector3 groundPoint)
        {  //Checks for physical obstructions at the candidate spot
            int mask = entityLayerMask | obstacleLayerMask;
            Vector3 checkCenter = groundPoint + Vector3.up * (clearanceRadius + 0.1f);
            return Physics.CheckSphere(checkCenter, clearanceRadius, mask, QueryTriggerInteraction.Ignore);
        }
    }
}