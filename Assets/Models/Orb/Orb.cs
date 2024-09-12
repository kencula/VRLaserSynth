using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class Orb : MonoBehaviour
{
    public Color colour = new Color(0.8f, 0.3f, 0.2f, 1);
    public float size = 1;
    public int detail = 30;
    public float effect = 0.4f;
    public float brightness = 0.5f;
    public float definition = 0.5f;
    public bool vary;
    public int varyOffset;
    private Color varyColour;
    public bool IsGenerated { get { return mesh != null; } }

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    private void Update() {
        if (vary) {
            varyColour = new Color(Mathf.Sin(Time.time / 2 + varyOffset) / 2 + 0.5f,
                Mathf.Sin(Time.time / 5) / 2 + 0.5f,
                Mathf.Sin(Time.time / 10) / 2 + 0.5f
            );
            UpdateShader();
        }
    }

    public void UpdateShader() {
        // Validate
        if (size <= 0) size = 1;
        if (detail <= 0) detail = 1;
        if (effect < 0) effect = 0;
        if (varyOffset < 0) varyOffset = 0;

        // Material color and size
        var renderer = GetComponent<Renderer>();
        if (renderer) {
            var material = renderer.sharedMaterial;
            if (material) {
                if (vary) material.color = varyColour;
                else material.color = colour;
                material.SetFloat("_Effect", effect);
                material.SetFloat("_Size", size);
                material.SetFloat("_Brightness", brightness);
                material.SetFloat("_Definition", definition);
            }
        }
    }

    public void GenerateSphere() {
        mesh = new Mesh();
        mesh.name = "Orb";

        int X = (int)size * detail + 3;
        int Y = (int)size * detail + 3;

        vertices = new Vector3[(X + 1) * Y];
        triangles = new int[X * (Y + 1) * 6];
        var uv = new Vector2[(X + 1) * Y];

        // Vertices
        int i = 0;
        for (int x = 0; x <= X; x++) {
            for (int y = 0; y < Y; y++) {
                vertices[i] = new Vector3(
                    Mathf.Sin(Mathf.PI * x / X) * Mathf.Cos(2 * Mathf.PI * y / Y),
                    Mathf.Cos(Mathf.PI * x / X),
                    Mathf.Sin(Mathf.PI * x / X) * Mathf.Sin(2 * Mathf.PI * y / Y)
                ) * size;
                uv[i++] = new Vector2(x / X, y / Y);
            }
        }

        // Triangles
        int ti = 0;
        for (int x = 0; x < X; x++) {
            for (int y = 0; y < Y - 1; y++, ti += 6) {
                triangles[ti] = x * Y + y;
                triangles[ti + 1] = triangles[ti + 5] = x * Y + y + 1;
                triangles[ti + 2] = triangles[ti + 4] = (x + 1) * Y + y;
                triangles[ti + 3] = (x + 1) * Y + y + 1;
            }

            triangles[ti] = triangles[ti + 4] = (x + 2) * Y - 1;
            triangles[ti + 1] = (x + 1) * Y - 1;
            triangles[ti + 2] = triangles[ti + 5] = x * Y;
            triangles[ti + 3] = (x + 1) * Y;
            ti += 6;
        }

        // Update mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Update
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter)
            meshFilter.sharedMesh = mesh;
    }

    public void GenerateNewMaterial() {
        // Add new material
        var shader = Shader.Find("Custom/Orb");
        var renderer = GetComponent<Renderer>();
        if (renderer && shader) {
            renderer.sharedMaterial = new Material(shader);
        }
    }
}
