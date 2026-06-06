using System.Collections.Generic;
using AirportAR.Map;
using AirportAR.Navigation;
using UnityEngine;

namespace AirportAR.AR
{
    /// <summary>
    /// Spawns 3D arrow markers along a navigation route on the floor (AR or editor fallback).
    /// </summary>
    public class ARArrowSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float arrowHeightOffset = 0.05f;
        [SerializeField] float arrowSpacing = 1.5f;
        [SerializeField] float scale = 0.4f;
        [SerializeField] Color arrowColor = new Color(0.12f, 0.45f, 0.92f);

        [Header("Optional Prefab")]
        [SerializeField] GameObject arrowPrefab;

        readonly List<GameObject> spawnedArrows = new List<GameObject>();
        Transform arrowRoot;
        Transform worldAnchor;

        void Awake()
        {
            arrowRoot = new GameObject("RouteArrows").transform;
            arrowRoot.SetParent(transform, false);
        }

        public void SetWorldAnchor(Transform anchor)
        {
            worldAnchor = anchor;
            if (arrowRoot != null && anchor != null)
            {
                arrowRoot.SetParent(anchor, false);
                arrowRoot.localPosition = Vector3.zero;
                arrowRoot.localRotation = Quaternion.identity;
            }
        }

        public void ClearArrows()
        {
            foreach (GameObject arrow in spawnedArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }

            spawnedArrows.Clear();
        }

        public void SpawnRoute(PathResult route, Vector3 originOffset, float mapScale = 1f)
        {
            ClearArrows();

            if (route == null || !route.HasPath || route.ZoneIds.Count < 2)
            {
                return;
            }

            AirportGraph graph = PathfindingService.Instance?.GetGraph();
            if (graph == null)
            {
                return;
            }

            Transform parent = worldAnchor != null ? arrowRoot : transform;
            if (worldAnchor != null)
            {
                arrowRoot.SetParent(worldAnchor, false);
                arrowRoot.localPosition = Vector3.zero;
                arrowRoot.localRotation = Quaternion.identity;
            }
            else
            {
                arrowRoot.SetParent(transform, false);
            }

            for (int i = 0; i < route.ZoneIds.Count - 1; i++)
            {
                AirportZone fromZone = graph.GetZoneById(route.ZoneIds[i]);
                AirportZone toZone = graph.GetZoneById(route.ZoneIds[i + 1]);
                if (fromZone?.position == null || toZone?.position == null)
                {
                    continue;
                }

                Vector3 start;
                Vector3 end;

                if (worldAnchor != null)
                {
                    Vector3 fromLocal = fromZone.position.ToVector3() * mapScale;
                    Vector3 toLocal = toZone.position.ToVector3() * mapScale;
                    fromLocal.y = arrowHeightOffset;
                    toLocal.y = arrowHeightOffset;
                    start = worldAnchor.TransformPoint(fromLocal);
                    end = worldAnchor.TransformPoint(toLocal);
                }
                else
                {
                    start = originOffset + fromZone.position.ToVector3() * mapScale;
                    end = originOffset + toZone.position.ToVector3() * mapScale;
                    start.y += arrowHeightOffset;
                    end.y += arrowHeightOffset;
                }

                SpawnArrowsAlongSegment(start, end, parent);
            }
        }

        void SpawnArrowsAlongSegment(Vector3 start, Vector3 end, Transform parent)
        {
            Vector3 direction = end - start;
            float length = direction.magnitude;
            if (length < 0.01f)
            {
                return;
            }

            direction.Normalize();
            int count = Mathf.Max(1, Mathf.FloorToInt(length / arrowSpacing));

            for (int i = 1; i <= count; i++)
            {
                float t = i / (float)(count + 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.y += arrowHeightOffset;
                SpawnArrow(pos, direction, parent);
            }
        }

        void SpawnArrow(Vector3 position, Vector3 forward, Transform parent)
        {
            GameObject arrow = arrowPrefab != null
                ? Instantiate(arrowPrefab, parent)
                : CreatePrimitiveArrow(parent);

            arrow.transform.position = position;
            arrow.transform.localScale = Vector3.one * scale;

            if (forward.sqrMagnitude > 0.001f)
            {
                arrow.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }

            spawnedArrows.Add(arrow);
        }

        GameObject CreatePrimitiveArrow(Transform parent)
        {
            var root = new GameObject("Arrow");
            root.transform.SetParent(parent, false);

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaft.transform.SetParent(root.transform, false);
            shaft.transform.localScale = new Vector3(0.15f, 0.03f, 0.5f);
            shaft.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            RemoveCollider(shaft);

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(root.transform, false);
            head.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);
            head.transform.localPosition = new Vector3(0f, 0f, 0.45f);
            RemoveCollider(head);

            ApplyColor(shaft);
            ApplyColor(head);
            return root;
        }

        static void RemoveCollider(GameObject obj)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }
        }

        void ApplyColor(GameObject obj)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = arrowColor;
            }
        }
    }
}
