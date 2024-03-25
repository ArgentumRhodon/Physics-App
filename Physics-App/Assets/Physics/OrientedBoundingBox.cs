using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor.EditorTools;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RigidBody))]
public class OrientedBoundingBox : MonoBehaviour
{
    public RigidBody RigidBody { get; private set; }

    [SerializeField]
    public Bounds bounds;

    private void Awake()
    {
        RigidBody = GetComponent<RigidBody>();
    }

    public Vector3[] GetVertices()
    {
        Matrix4x4 transformMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

        Vector4 e1 = bounds.extents.x * new Vector4(1, 0, 0, 0);
        Vector4 e2 = bounds.extents.y * new Vector4(0, 1, 0, 0);
        Vector4 e3 = bounds.extents.z * new Vector4(0, 0, 1, 0);
        Vector4 c = new Vector4(bounds.center.x, bounds.center.y, bounds.center.z, 1);

        Vector3[] vertices = new Vector3[8];

        // All global corners of the box collider
        vertices[0] = (Vector3)(transformMatrix * (c + e1 + e2 + e3));
        vertices[1] = (Vector3)(transformMatrix * (c + e1 + e2 - e3));
        vertices[2] = (Vector3)(transformMatrix * (c + e1 - e2 + e3));
        vertices[3] = (Vector3)(transformMatrix * (c + e1 - e2 - e3));
        vertices[4] = (Vector3)(transformMatrix * (c - e1 + e2 + e3));
        vertices[5] = (Vector3)(transformMatrix * (c - e1 + e2 - e3));
        vertices[6] = (Vector3)(transformMatrix * (c - e1 - e2 + e3));
        vertices[7] = (Vector3)(transformMatrix * (c - e1 - e2 - e3));

        return vertices;
    }

    public Vector3[] GetAxes()
    {
        return new Vector3[]
        {
            transform.right,
            transform.up,
            transform.forward
        };
    }
}

#if UNITY_EDITOR
[EditorTool("OBB Edit", typeof(OrientedBoundingBox)), CanEditMultipleObjects]
public class OrientedBoundingBoxEditorTool : EditorTool
{
    public override GUIContent toolbarIcon => EditorGUIUtility.IconContent("d_EditCollider");

    private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
    public override void OnToolGUI(EditorWindow window)
    {
        if (!(window is SceneView)) return;

        OrientedBoundingBox boundingBox = (OrientedBoundingBox)target;

        // copy the target object's data to the handle
        m_BoundsHandle.center = boundingBox.bounds.center;
        m_BoundsHandle.size = boundingBox.bounds.size;

        // draw the handle
        EditorGUI.BeginChangeCheck();
        Handles.color = Color.yellow;

        // Draw the controls with respect to the collider's game object
        Matrix4x4 matrix = Matrix4x4.TRS(boundingBox.transform.position, boundingBox.transform.rotation, boundingBox.transform.localScale);
        using (new Handles.DrawingScope(matrix))
        {
            m_BoundsHandle.DrawHandle();
        }

        if (EditorGUI.EndChangeCheck())
        {
            // record the target object before setting new values so changes can be undone/redone
            Undo.RecordObject(boundingBox, "Change Bounds");

            // copy the handle's updated data back to the target object
            boundingBox.bounds.size = m_BoundsHandle.size;
            boundingBox.bounds.center = m_BoundsHandle.center;
        }
    }
}
#endif