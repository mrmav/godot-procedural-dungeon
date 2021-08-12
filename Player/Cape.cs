using Godot;
using Godot.Collections;
using System.Collections.Generic;

public class Cape : SpringSystem
{
    [Export]
    public NodePath NodeToFollow;
    
    [Export]
    public List<NodePath> SpringsToFollow;

    [Export]
    public float SpringsMaxVelocityOnDash = 600f;

    // this mesh instance is the resulting cape mesh
    private MeshInstance2D _meshInstance;

    // that actual cape mesh
    private ArrayMesh _mesh;

    // mesh tool
    private MeshDataTool _mdt;

    // this array holds the node that
    // the cape will follow
    private Node2D _node2Follow;

    // these are the springs that will follow the node
    // (in this case, all the fixed springs)
    private SpringPoint[] _springs2Follow;

    // the offsets of the springs that will follow the node
    private Vector2[] _offset;

    // a position what will anchor all the spring points
    private Position2D _anchor;
    private Vector2 _anchorOffset;

    // the sprite to show the cape texture
    private Sprite _capeSprite;

    public override void _Ready()
    {
        base._Ready();

        _node2Follow = GetNode<Node2D>(NodeToFollow);

        _anchor = new Position2D();
        _anchor.GlobalPosition = Vector2.Zero;
        AddChild(_anchor);

        _anchorOffset = _anchor.GlobalPosition - _node2Follow.GlobalPosition;

        // init arrays 
        _springs2Follow = new SpringPoint[SpringsToFollow.Count];
        _offset = new Vector2[SpringsToFollow.Count];

        // populate the arrays
        for(int i = 0; i < SpringsToFollow.Count; i++)
        {
            _springs2Follow[i] = GetNode<SpringPoint>(SpringsToFollow[i]);
            _offset[i] = _springs2Follow[i].GlobalPosition - _anchor.GlobalPosition;
        }

        _capeSprite = GetNode<Sprite>("CapeSprite");

        _mdt = new MeshDataTool();
        _meshInstance = GetNode<MeshInstance2D>("Viewport/CapeMeshInstance");  // the meshInstance is inside the viewport so we can use the texture later
        _mesh = new ArrayMesh();

        // this is the 
        Array meshArray = new Array();
        // this resize will make the array able
        // to fit all the vertex atributes that we want
        meshArray.Resize((int)Mesh.ArrayType.Max);

        // array that holds each vertex position
        Vector3[] vertices = new Vector3[_springs.Count];
        // array that holds each vertex color
        Color[] colors = new Color[_springs.Count];
        // array that holds each vertex position in 2D (only used for triangulation function)
        Vector2[] vertices2D = new Vector2[_springs.Count];
        
        // populate arrays
        for(int i = 0; i < _springs.Count; i++)
        {
            Vector2 s = _springs[i].Position;
            vertices2D[i] = new Vector2(s.x, s.y);
            vertices[i] = new Vector3(s.x, s.y, 0f);

            colors[i] = Colors.White;
        }

        // get the triangulated geometry indices
        int[] indices = Geometry.TriangulateDelaunay2d(vertices2D);

        // fill in mesh array with the proper arrays
        meshArray[(int)Mesh.ArrayType.Vertex] = vertices;
        meshArray[(int)Mesh.ArrayType.Color] = colors;
        meshArray[(int)Mesh.ArrayType.Index]  = indices;
        
        // create surface and assign
        _mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshArray);
        _meshInstance.Mesh = _mesh;
        
        DebugDrawOffset = GetTranslation(_anchor.Position, _node2Follow.GlobalPosition); 
        
    }

    /// <summary>
    /// This function updates the mesh with the current cape geometry.
    /// </summary>
    public void UpdateMesh()
    {
        _mdt.CreateFromSurface(_mesh, 0);

        // because we are using a viewport to render the mesh, the mesh position will be relative
        // to the (0, 0) coordinate.
        // here we calculate a vector to transform everything to the (0, 0)
        // based on the anchor position
        Vector2 transform = GetTranslation(_anchor.Position, new Vector2(0, 0));          
        
        int count = _mdt.GetVertexCount();
        for(int i = 0; i < count; i++)
        {   
            // apply translation
            Vector2 newPosition = _springs[i].Position + transform;
            _mdt.SetVertex(i, new Vector3(newPosition.x, newPosition.y, 0f));
        }

        _mesh.SurfaceRemove(0);
        _mdt.CommitToSurface(_mesh);

    }

    public Vector2 GetTranslation(Vector2 p1, Vector2 p2)
    {
        return p2 - p1;
    }

    public override void _Process(float delta)
    {        
        // if we are following, the anchor should follow as well
        _anchor.GlobalPosition = _node2Follow.GlobalPosition + _anchorOffset;

        // make the designated springs follow the node             
        for(int i = 0; i < _springs2Follow.Length; i++)
        {
            _springs2Follow[i].GlobalPosition = _anchor.GlobalPosition + _offset[i];
        }

        // and now, for each spring point, we update the base position.
        // the base position is actually the original offset to its scene center
        // We need to update this position, because we will want to use "LinearRestitution",
        // that will basically make the spring go towards its initial position.
        // we can not use the scene position, because its a simple Node, with no transform.
        // this way, we can have the springs move freely in the game world, and be also a child 
        // of the player. Hope this makes sense for the future me.
        for(int i = 0; i < _springs.Count; i++)
        {
            // also, this only works because the _spring at position 0, is in the (0, 0) of the scene :)
            _springs[i].SetBasePosition(_anchor.GlobalPosition + _springs[i].GetOriginalPosition());
            
        }

        // finally, put the sprite in it's correct location
        _capeSprite.GlobalPosition = _node2Follow.GlobalPosition;

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

    public void SetDashing(bool dash)
    {
        for(int i = 0; i < _springs.Count; i++)
        {
            
            if(dash)
            {
                _springs[i].MaxVelocity = SpringsMaxVelocityOnDash;
            } else
            {
                _springs[i].MaxVelocity = _springs[i].GetOriginalMaxVelocity();

            }
        }

    }
}
