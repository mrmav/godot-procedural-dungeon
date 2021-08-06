using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class Cape : SpringSystem
{
    [Export]
    public NodePath NodeToFollow;
    
    [Export]
    public List<NodePath> SpringsToFollow;

    private MeshInstance2D _meshInstance;
    private ArrayMesh _mesh;
    private MeshDataTool _mdt;

    private Node2D _node2Follow;
    private Vector2[] _offset;
    private SpringPoint[] _springs2Follow;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        if(NodeToFollow != null)
        {
            _node2Follow = GetNode<Node2D>(NodeToFollow);
        }

        _springs2Follow = new SpringPoint[SpringsToFollow.Count];
        _offset = new Vector2[SpringsToFollow.Count];
        for(int i = 0; i < SpringsToFollow.Count; i++)
        {
            _springs2Follow[i] = GetNode<SpringPoint>(SpringsToFollow[i]);
            _offset[i] = _springs2Follow[i].GlobalPosition - _node2Follow.GlobalPosition;
            GD.Print(_offset[i]);
        }

        _mdt = new MeshDataTool();
        _meshInstance = GetNode<MeshInstance2D>("Viewport/CapeMeshInstance");
        _mesh = new ArrayMesh();

        Array arr = new Array();
        arr.Resize((int)Mesh.ArrayType.Max);

        Vector3[] vertices = new Vector3[_springs.Count];
        Color[] colors = new Color[_springs.Count];

        Vector2[] vertices2D = new Vector2[_springs.Count];


        for(int i = 0; i < _springs.Count; i++)
        {
            Vector2 s = _springs[i].Position;
            vertices[i] = new Vector3(s.x, s.y, 0f);
            vertices2D[i] = new Vector2(s.x, s.y);

            colors[i] = Colors.White;
        }


        int[] engineIndices = Geometry.TriangulateDelaunay2d(vertices2D);
        GD.Print(engineIndices);
        GD.Print(engineIndices.Length);

        int[] indices = new int[48];

        // create indices
        int currentIndice = 0;
        int currentSubDivison = 0;
        int XSubs = 2;
        int ZSubs = 4;
        int nVerticesWidth = 3;
        for (int z = 0; z < ZSubs; z++)
        {
            for (int x = 0; x < XSubs; x++)
            {
                /* calculate positions in the array
                * 
                *  1---2
                *  | / |
                *  4---3
                *  
                */

                int vert1 = currentSubDivison + z;
                int vert2 = vert1 + 1;
                int vert3 = vert2 + nVerticesWidth;
                int vert4 = vert3 - 1;

                // first tri
                indices[currentIndice++] = (short)vert1;
                indices[currentIndice++] = (short)vert2;
                indices[currentIndice++] = (short)vert4;

                // secon tri
                indices[currentIndice++] = (short)vert4;
                indices[currentIndice++] = (short)vert2;
                indices[currentIndice++] = (short)vert3;

                currentSubDivison++;

            }

        }

        arr[(int)Mesh.ArrayType.Vertex] = vertices;
        arr[(int)Mesh.ArrayType.Color] = colors;
        //arr[(int)Mesh.ArrayType.Index]  = indices;
        arr[(int)Mesh.ArrayType.Index]  = engineIndices;
        
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arr);
        _meshInstance.Mesh = _mesh;

    }

    public void UpdateMesh()
    {
        _mdt.CreateFromSurface(_mesh, 0);

        int count = _mdt.GetVertexCount();
        Vector2 transform = TransformTo(_springs[0].GlobalPosition, new Vector2(0, 0));        
        for(int i = 0; i < count; i++)
        {   
            Vector2 newPosition = _springs[i].GlobalPosition + transform;
            _mdt.SetVertex(i, new Vector3(newPosition.x, newPosition.y, 0f));
        }

        _mesh.SurfaceRemove(0);
        _mdt.CommitToSurface(_mesh);

    }

    public Vector2 TransformTo(Vector2 p1, Vector2 p2)
    {
        return p2 - p1;
    }

    public override void _Process(float delta)
    {
        if(_node2Follow != null)
        {
            //Position = _node2Follow.Position;

            for(int i = 0; i < _springs2Follow.Length; i++)
            {
                _springs2Follow[i].GlobalPosition = _node2Follow.GlobalPosition + _offset[i];
            }

            for(int i = 0; i < _springs.Count; i++)
            {
                _springs[i].SetBasePosition(_springs[0].Position + _springs[i].GetOriginalPosition());
            }

        }

        //_meshInstance.GlobalPosition = new Vector2(64, 64);

        UpdateMesh();

        base._Process(delta);
    }
    
    // see: https://stackoverflow.com/questions/6989100/sort-points-in-clockwise-order
    private int ComparePoints(Vector2 a, Vector2 b, Vector2 _center)
    {

        if (a.x - _center.x >= 0 && b.x - _center.x < 0)
            return -1;
        if (a.x - _center.x < 0 && b.x - _center.x >= 0)
            return 1;
        if (a.x - _center.x == 0 && b.x - _center.x == 0) {
            if (a.y - _center.y >= 0 || b.y - _center.y >= 0)
                return Bool2Int(a.y > b.y);
            return Bool2Int(b.y > a.y);
        }

        // compute the cross product of vectors (center -> a) x (center -> b)
        float det = (a.x - _center.x) * (b.y - _center.y) - (b.x - _center.x) * (a.y - _center.y);
        if (det < 0)
            return -1;
        if (det > 0)
            return 1;

        // points a and b are on the same line from the center
        // check which point is closer to the center
        float d1 = (a.x - _center.x) * (a.x - _center.x) + (a.y - _center.y) * (a.y - _center.y);
        float d2 = (b.x - _center.x) * (b.x - _center.x) + (b.y - _center.y) * (b.y - _center.y);
        return Bool2Int(d1 > d2);

    }

    private Vector2 CalculateCenterPoint(List<Vector2> pts)
    {
        Vector2 c = Vector2.Zero;

        for(int i = 0; i < pts.Count; i++)
        {
            c += pts[i];
        }

        return c / pts.Count;

    }

    private int Bool2Int(bool b)
    {
        if(b)
            return 1;
        
        return -1;
    }
}
