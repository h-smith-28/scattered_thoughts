using UnityEngine;
using UnityEngine.Rendering;

// Runtime-only visuals: never replace an object's materials or add colliders.
public sealed class s_InteractionFeedback
{
    private readonly Camera camera;
    private readonly Material outlineMaterial;
    private readonly Material previewMaterial;
    private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
    private readonly Mesh previewMesh;
    private Transform focused;
    private MeshFilter[] meshes;

    public s_InteractionFeedback(Camera camera, Material outline, Material preview)
    {
        this.camera = camera;
        // These are editable Shader Graph materials, not generated replacements.
        outlineMaterial = outline ? outline : Resources.Load<Material>("InteractionFeedback/InteractableOutline");
        previewMaterial = preview ? preview : Resources.Load<Material>("InteractionFeedback/StickyNotePlacement");
        previewMesh = new Mesh { name = "Sticky note placement preview", hideFlags = HideFlags.HideAndDontSave };
        previewMesh.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0) };
        previewMesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
        previewMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        previewMesh.RecalculateBounds();
    }

    public void DrawFocus(Transform target)
    {
        if (focused != target)
        {
            focused = target;
            meshes = target ? target.GetComponentsInChildren<MeshFilter>() : null;
        }
        if (!target || !outlineMaterial || meshes == null) return;

        foreach (MeshFilter filter in meshes)
        {
            if (!filter || !filter.sharedMesh) continue;
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            Matrix4x4 matrix = renderer.localToWorldMatrix;
            properties.Clear();
            properties.SetVector("_Center", matrix.inverse.MultiplyPoint3x4(renderer.bounds.center));
            int first = renderer.isPartOfStaticBatch ? renderer.subMeshStartIndex : 0;
            int count = renderer.isPartOfStaticBatch ? renderer.sharedMaterials.Length : filter.sharedMesh.subMeshCount;
            for (int i = first; i < Mathf.Min(first + count, filter.sharedMesh.subMeshCount); i++)
            {
                Graphics.DrawMesh(filter.sharedMesh, matrix, outlineMaterial,
                    renderer.gameObject.layer, camera, i, properties, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
            }
        }
    }

    // The preview and the actual click use exactly the same surface/edge checks.
    public static bool TryGetPlacement(Ray ray, GameObject note, out Vector3 position, out Quaternion rotation)
    {
        position = default;
        rotation = Quaternion.identity;
        if (!note || !Physics.Raycast(ray, out RaycastHit hit, 50f, ~0, QueryTriggerInteraction.Ignore)) return false;
        if (hit.collider.transform.IsChildOf(note.transform)
            || hit.collider.GetComponentInParent<s_PlayerCamera>()) return false;

        BoxCollider box = note.GetComponent<BoxCollider>();
        if (!box) return false;
        Vector3 size = Vector3.Scale(box.size, note.transform.lossyScale);
        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        Vector3 up = Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) > .95f ? Vector3.forward : Vector3.up;
        rotation = Quaternion.LookRotation(hit.normal, up);
        for (int x = -1; x <= 1; x += 2)
        for (int y = -1; y <= 1; y += 2)
        {
            Vector3 corner = hit.point + rotation * new Vector3(x * size.x * .5f, y * size.y * .5f, .03f);
            if (!Physics.Raycast(corner, -hit.normal, out RaycastHit edge, .06f, ~0, QueryTriggerInteraction.Ignore)
                || edge.collider != hit.collider) return false;
        }
        position = hit.point + hit.normal * (size.z * .5f + .002f);
        return true;
    }

    public void DrawPlacement(Ray ray, GameObject note)
    {
        DrawFocus(null);
        if (!previewMaterial || !TryGetPlacement(ray, note, out Vector3 position, out Quaternion rotation)) return;
        BoxCollider box = note.GetComponent<BoxCollider>();
        Vector3 size = Vector3.Scale(box.size, note.transform.lossyScale);
        position += rotation * Vector3.forward * (Mathf.Abs(size.z) * .5f + .003f);
        properties.Clear();
        Graphics.DrawMesh(previewMesh, Matrix4x4.TRS(position, rotation, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), 1)),
            previewMaterial, 2, camera, 0, properties, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
    }

    public void Dispose()
    {
        Object.Destroy(previewMesh);
    }
}
