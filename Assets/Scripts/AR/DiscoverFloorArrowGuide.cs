using System.Collections.Generic;
using UnityEngine;

namespace AirportAR.AR
{
    /// <summary>
    /// Places three world-locked floor arrows (forward / left / right) using AR Foundation raycasts.
    /// </summary>
    public class DiscoverFloorArrowGuide : MonoBehaviour
    {
        [SerializeField] float arrowDistance = 3f;
        [SerializeField] float arrowHeightOffset = 0.08f;
        [SerializeField] float arrowScale = 1f;
        [SerializeField] int chevronsPerDirection = 3;
        [SerializeField] float chevronSpacing = 0.55f;

        Transform arrowRoot;
        readonly List<GameObject> spawned = new List<GameObject>();

        static readonly Color ArrowColor = new Color(0.55f, 0.78f, 0.15f, 1f);
        static readonly Color LabelColor = Color.white;

        public bool IsPlaced { get; private set; }

        public bool UseArMode =>
            Application.isMobilePlatform &&
            !Application.isEditor &&
            MobileARSessionBootstrap.Instance != null &&
            MobileARSessionBootstrap.Instance.IsTracking;

        public bool TryPlaceGuide(Vector2 screenPoint, out string errorMessage)
        {
            errorMessage = null;
            MobileARSessionBootstrap ar = MobileARSessionBootstrap.Instance;

            if (ar == null || !ar.IsARActive)
            {
                errorMessage = "AR nu este pornit. Încearcă din nou după ce camera AR pornește.";
                return false;
            }

            if (!ar.TryRaycastHorizontalPlane(screenPoint, out Pose hitPose))
            {
                Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                if (!ar.TryRaycastHorizontalPlane(center, out hitPose))
                {
                    errorMessage =
                        "Nu am detectat podeaua. Mișcă telefonul lent stânga-dreapta, apoi atinge din nou ecranul.";
                    return false;
                }
            }

            ClearGuide();
            PlaceDirectionalArrows(hitPose.position, ar.ARCamera);
            IsPlaced = true;
            Debug.Log("[DiscoverFloorArrowGuide] AR floor arrows placed.");
            return true;
        }

        public void ClearGuide()
        {
            foreach (GameObject obj in spawned)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            spawned.Clear();
            IsPlaced = false;

            if (arrowRoot != null)
            {
                Destroy(arrowRoot.gameObject);
                arrowRoot = null;
            }
        }

        void PlaceDirectionalArrows(Vector3 floorPoint, Camera arCamera)
        {
            MobileARSessionBootstrap ar = MobileARSessionBootstrap.Instance;
            Transform parent = ar != null && ar.Origin != null ? ar.Origin.transform : transform;

            arrowRoot = new GameObject("DiscoverArrows").transform;
            arrowRoot.SetParent(parent, false);

            Vector3 forward = arCamera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 left = -right;

            float baseY = floorPoint.y + arrowHeightOffset;

            SpawnDirection(arrowRoot, floorPoint, baseY, forward, "Check-in");
            SpawnDirection(arrowRoot, floorPoint, baseY, left, "Security");
            SpawnDirection(arrowRoot, floorPoint, baseY, right, "Ciambella · Toalete");
        }

        void SpawnDirection(Transform parent, Vector3 origin, float baseY, Vector3 direction, string label)
        {
            var cluster = new GameObject(label);
            cluster.transform.SetParent(parent, false);

            for (int i = 0; i < chevronsPerDirection; i++)
            {
                float offset = arrowDistance + i * chevronSpacing;
                Vector3 pos = origin + direction * offset;
                pos.y = baseY;
                CreateChevron(cluster.transform, pos, direction);
            }

            Vector3 labelPos = origin + direction * (arrowDistance + chevronSpacing);
            labelPos.y = baseY + 0.45f;
            CreateLabel(cluster.transform, labelPos, label);
        }

        void CreateChevron(Transform parent, Vector3 position, Vector3 forward)
        {
            var chevron = new GameObject("Chevron");
            chevron.transform.SetParent(parent, false);
            chevron.transform.position = position;
            chevron.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shaft.transform.SetParent(chevron.transform, false);
            shaft.transform.localScale = new Vector3(0.22f, 0.025f, 0.55f) * arrowScale;
            shaft.transform.localPosition = new Vector3(0f, 0f, 0.18f);
            RemoveCollider(shaft);
            Paint(shaft, ArrowColor);

            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(chevron.transform, false);
            head.transform.localScale = new Vector3(0.42f, 0.025f, 0.42f) * arrowScale;
            head.transform.localPosition = new Vector3(0f, 0f, 0.48f);
            head.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            RemoveCollider(head);
            Paint(head, ArrowColor);

            spawned.Add(chevron);
        }

        void CreateLabel(Transform parent, Vector3 position, string text)
        {
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent, false);
            labelGo.transform.position = position;
            labelGo.AddComponent<FaceCamera>();

            var textMesh = labelGo.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.04f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = LabelColor;
            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            spawned.Add(labelGo);
        }

        static void RemoveCollider(GameObject obj)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }
        }

        static void Paint(GameObject obj, Color color)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var mat = new Material(Shader.Find("Standard"));
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
            {
                mat = new Material(Shader.Find("Unlit/Color"));
            }

            mat.color = color;
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }

            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", color * 0.35f);
            }

            renderer.material = mat;
        }
    }
}
