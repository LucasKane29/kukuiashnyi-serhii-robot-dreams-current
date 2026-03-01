using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LightProbeGenerate : MonoBehaviour
{
    [Header("Terrain")]
    [SerializeField] private Terrain terrain;

    [Header("Grid Settings")]
    [SerializeField] private float spacingX = 5f;
    [SerializeField] private float spacingZ = 5f;

    [Header("Height Layers")]
    [SerializeField] private int   heightLayers  = 3;
    [SerializeField] private float layerStep     = 2f;
    [SerializeField] private float groundOffset  = 0.5f;

    [Header("Validation")]
    // Мінімальна відстань між двома probe-ами — ближчі відкидаються як дублікати
    [SerializeField] private float minProbeDistance = 0.1f;

    [Header("Zone")]
    [SerializeField] private bool    useCustomZone = false;
    [SerializeField] private Vector3 zoneCenter    = Vector3.zero;
    [SerializeField] private Vector3 zoneSize      = new Vector3(50f, 20f, 50f);

    [Header("Components")]
    [SerializeField] private LightProbeGroup lightProbeGroup;

    public void GenerateProbes()
    {
        if (terrain == null)
        {
            Debug.LogError("LightProbeGenerate: Terrain не призначено!");
            return;
        }
        if (lightProbeGroup == null)
        {
            Debug.LogError("LightProbeGenerate: LightProbeGroup не призначено!");
            return;
        }

        // --- Фікс #2: валідація параметрів перед генерацією ---
        float safeSpacingX = Mathf.Max(0.01f, spacingX);
        float safeSpacingZ = Mathf.Max(0.01f, spacingZ);
        int   safeLayers   = Mathf.Max(1, heightLayers);
        // Якщо layerStep = 0 при кількох шарах — всі probe-и співпадуть по Y
        float safeLayerStep = safeLayers > 1 ? Mathf.Max(0.01f, layerStep) : 0f;

        Vector3 terrainSize   = terrain.terrainData.size;
        Vector3 terrainOrigin = terrain.transform.position;

        float startX, endX, startZ, endZ;

        if (useCustomZone)
        {
            float halfX = zoneSize.x * 0.5f;
            float halfZ = zoneSize.z * 0.5f;

            startX = Mathf.Max(terrainOrigin.x,                 zoneCenter.x - halfX);
            endX   = Mathf.Min(terrainOrigin.x + terrainSize.x, zoneCenter.x + halfX);
            startZ = Mathf.Max(terrainOrigin.z,                 zoneCenter.z - halfZ);
            endZ   = Mathf.Min(terrainOrigin.z + terrainSize.z, zoneCenter.z + halfZ);

            if (startX >= endX || startZ >= endZ)
            {
                Debug.LogWarning("LightProbeGenerate: Зона не перетинається з terrain!");
                return;
            }
        }
        else
        {
            startX = terrainOrigin.x;
            endX   = terrainOrigin.x + terrainSize.x;
            startZ = terrainOrigin.z;
            endZ   = terrainOrigin.z + terrainSize.z;
        }

        var positions = new List<Vector3>();

        // --- Фікс #1: цілочисельна індексація замість float-накопичення ---
        // float x += spacing накопичує похибку → можливі дублікати на межах
        int stepsX = Mathf.FloorToInt((endX - startX) / safeSpacingX);
        int stepsZ = Mathf.FloorToInt((endZ - startZ) / safeSpacingZ);

        // --- Фікс #3: дедублікація через квантований HashSet ---
        // Два probe-и вважаються однаковими якщо ближчі за minProbeDistance
        float safeMinDist = Mathf.Max(0.001f, minProbeDistance);
        var   seen        = new HashSet<Vector3Int>();

        // Y-межі зони: probe-и поза ними відкидаються незалежно від terrain
        float zoneYMin = useCustomZone ? zoneCenter.y - zoneSize.y * 0.5f : float.NegativeInfinity;
        float zoneYMax = useCustomZone ? zoneCenter.y + zoneSize.y * 0.5f : float.PositiveInfinity;

        for (int ix = 0; ix <= stepsX; ix++)
        {
            float x = startX + ix * safeSpacingX;

            for (int iz = 0; iz <= stepsZ; iz++)
            {
                float z = startZ + iz * safeSpacingZ;

                // При зоні — шари йдуть від дна зони вгору незалежно від рельєфу.
                // Без зони — від поверхні terrain вгору (класична поведінка).
                float baseY = useCustomZone
                    ? zoneYMin + groundOffset
                    : terrainOrigin.y + terrain.SampleHeight(new Vector3(x, 0f, z)) + groundOffset;

                for (int layer = 0; layer < safeLayers; layer++)
                {
                    float worldY = baseY + layer * safeLayerStep;

                    if (worldY > zoneYMax)
                        break;

                    Vector3 localPos = lightProbeGroup.transform.InverseTransformPoint(
                        new Vector3(x, worldY, z));

                    Vector3Int key = Quantize(localPos, safeMinDist);
                    if (seen.Add(key))
                        positions.Add(localPos);
                }
            }
        }

        // --- Фінальна перевірка: мінімум 4 некопланарних probe-и ---
        if (positions.Count < 4)
        {
            Debug.LogError($"LightProbeGenerate: Згенеровано лише {positions.Count} probe-ів. " +
                           "Потрібно щонайменше 4 некопланарних точки для валідних cells. " +
                           "Зменш spacing або збільш зону.");
            return;
        }

        lightProbeGroup.probePositions = positions.ToArray();

#if UNITY_EDITOR
        EditorUtility.SetDirty(lightProbeGroup);
#endif

        Debug.Log($"Light Probes згенеровано: {positions.Count} точок" +
                  (useCustomZone ? $" (зона {zoneSize.x:F1}×{zoneSize.z:F1})" : " (вся карта)"));
    }

    // Перетворює позицію у дискретний ключ з розміром клітинки cellSize
    private static Vector3Int Quantize(Vector3 pos, float cellSize)
    {
        return new Vector3Int(
            Mathf.RoundToInt(pos.x / cellSize),
            Mathf.RoundToInt(pos.y / cellSize),
            Mathf.RoundToInt(pos.z / cellSize));
    }

    // Простий fallback-gizmo (видно навіть без вибраного Editor)
    private void OnDrawGizmosSelected()
    {
        if (!useCustomZone) return;

        Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.12f);
        Gizmos.DrawCube(zoneCenter, zoneSize);

        Gizmos.color = new Color(0.2f, 0.9f, 0.2f, 0.7f);
        Gizmos.DrawWireCube(zoneCenter, zoneSize);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LightProbeGenerate))]
    public class LightProbeGenerateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(10);

            if (GUILayout.Button("Generate Light Probes"))
                ((LightProbeGenerate)target).GenerateProbes();
        }

        private void OnSceneGUI()
        {
            var gen = (LightProbeGenerate)target;
            if (!gen.useCustomZone) return;

            serializedObject.Update();

            SerializedProperty centerProp = serializedObject.FindProperty("zoneCenter");
            SerializedProperty sizeProp   = serializedObject.FindProperty("zoneSize");

            Vector3 center = centerProp.vector3Value;
            Vector3 size   = sizeProp.vector3Value;

            float halfX   = size.x * 0.5f;
            float halfY   = size.y * 0.5f;
            float halfZ   = size.z * 0.5f;
            float dotSize = HandleUtility.GetHandleSize(center) * 0.08f;

            // Малюємо грані зони
            DrawZoneFaces(center, halfX, halfY, halfZ);

            EditorGUI.BeginChangeCheck();

            // Переміщення центру
            Handles.color = Color.green;
            Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);

            // Грані по XZ (зелені)
            Handles.color = new Color(0.2f, 1f, 0.2f);
            Vector3 newRight = Handles.Slider(
                center + new Vector3( halfX, 0f, 0f), Vector3.right,
                dotSize, Handles.DotHandleCap, 0f);
            Vector3 newLeft = Handles.Slider(
                center + new Vector3(-halfX, 0f, 0f), Vector3.left,
                dotSize, Handles.DotHandleCap, 0f);
            Vector3 newFront = Handles.Slider(
                center + new Vector3(0f, 0f,  halfZ), Vector3.forward,
                dotSize, Handles.DotHandleCap, 0f);
            Vector3 newBack = Handles.Slider(
                center + new Vector3(0f, 0f, -halfZ), Vector3.back,
                dotSize, Handles.DotHandleCap, 0f);

            // Верхня та нижня грані по Y (блакитні)
            Handles.color = new Color(0.3f, 0.8f, 1f);
            Vector3 newTop = Handles.Slider(
                center + new Vector3(0f,  halfY, 0f), Vector3.up,
                dotSize, Handles.DotHandleCap, 0f);
            Vector3 newBottom = Handles.Slider(
                center + new Vector3(0f, -halfY, 0f), Vector3.down,
                dotSize, Handles.DotHandleCap, 0f);

            if (EditorGUI.EndChangeCheck())
            {
                // Зміщуємо всі краї разом з центром
                Vector3 delta = newCenter - center;
                newRight  += delta;
                newLeft   += delta;
                newFront  += delta;
                newBack   += delta;
                newTop    += delta;
                newBottom += delta;

                // Новий центр = середина між протилежними гранями
                float cx = (newRight.x + newLeft.x)  * 0.5f;
                float cy = (newTop.y   + newBottom.y) * 0.5f;
                float cz = (newFront.z + newBack.z)   * 0.5f;

                // Новий розмір = відстань між протилежними гранями (мін. 1 м)
                float sx = Mathf.Max(1f, Mathf.Abs(newRight.x - newLeft.x));
                float sy = Mathf.Max(1f, Mathf.Abs(newTop.y   - newBottom.y));
                float sz = Mathf.Max(1f, Mathf.Abs(newFront.z - newBack.z));

                centerProp.vector3Value = new Vector3(cx, cy, cz);
                sizeProp.vector3Value   = new Vector3(sx, sy, sz);

                serializedObject.ApplyModifiedProperties();
            }

            // Підпис розміру зони
            Handles.color = Color.white;
            Handles.Label(
                center + new Vector3(halfX + 1f, halfY, 0f),
                $"{size.x:F1} × {size.z:F1} × {size.y:F1} м");
        }

        // Малює 6 граней боксу з напівпрозорою заливкою та контуром
        private static void DrawZoneFaces(Vector3 c, float hx, float hy, float hz)
        {
            Vector3 FTR = c + new Vector3( hx,  hy,  hz);
            Vector3 FTL = c + new Vector3(-hx,  hy,  hz);
            Vector3 FBR = c + new Vector3( hx, -hy,  hz);
            Vector3 FBL = c + new Vector3(-hx, -hy,  hz);
            Vector3 BTR = c + new Vector3( hx,  hy, -hz);
            Vector3 BTL = c + new Vector3(-hx,  hy, -hz);
            Vector3 BBR = c + new Vector3( hx, -hy, -hz);
            Vector3 BBL = c + new Vector3(-hx, -hy, -hz);

            Color fill    = new Color(0.2f, 0.9f, 0.2f, 0.06f);
            Color outline = new Color(0.2f, 0.9f, 0.2f, 0.8f);

            Handles.DrawSolidRectangleWithOutline(new[] { FTL, FTR, BTR, BTL }, fill, outline); // Top
            Handles.DrawSolidRectangleWithOutline(new[] { FBL, FBR, BBR, BBL }, fill, outline); // Bottom
            Handles.DrawSolidRectangleWithOutline(new[] { FBL, FBR, FTR, FTL }, fill, outline); // Front
            Handles.DrawSolidRectangleWithOutline(new[] { BBR, BBL, BTL, BTR }, fill, outline); // Back
            Handles.DrawSolidRectangleWithOutline(new[] { FBR, BBR, BTR, FTR }, fill, outline); // Right
            Handles.DrawSolidRectangleWithOutline(new[] { BBL, FBL, FTL, BTL }, fill, outline); // Left
        }
    }
#endif
}
