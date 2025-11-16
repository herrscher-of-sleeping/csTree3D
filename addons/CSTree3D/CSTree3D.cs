namespace CSTree3D;

using System.Collections.Generic;
using Godot;

[Tool]
public partial class CSTree3D : Node3D
{
    MeshInstance3D trunkInstance = null;
    MeshInstance3D twigInstance = null;
    readonly Proctree tree = new();
    Material trunkMaterial = null;
    Material twigMaterial = null;
    private bool isLoaded = false;

    private bool enableTwig = true;

    #region Export variables

    Dictionary<StringName, Variant> defaults;

    public override bool _PropertyCanRevert(StringName property)
    {
        Variant outVal;
        if (defaults.TryGetValue(property, out outVal))
        {
            return true;
        }
        return false;
    }

    public override Variant _PropertyGetRevert(StringName property)
    {
        Variant outVal;
        if (defaults.TryGetValue(property, out outVal))
        {
            return outVal;
        }
        return base._PropertyGetRevert(property);
    }

    /*
    [ExportToolButton("Debug info")]
    public Callable DebugInfoButton => Callable.From(DebugInfo);

    public void DebugInfo()
    {
        GD.Print($"Child count: {GetChildCount()}");
    }
    */

    [Export(PropertyHint.Range, "0,100000,1")]
    public int seed
    {
        get
        {
            return tree.mProperties.mSeed;
        }
        set
        {
            tree.mProperties.mSeed = value;
            UpdateAllMeshes();
        }
    }

    [ExportGroup("Trunk")]
    [Export(PropertyHint.Range, "2,10,2")]
    public int segments
    {
        get
        {
            return tree.mProperties.mSegments;
        }
        set
        {
            tree.mProperties.mSegments = value;
            tree.Generate();
            UpdateMeshTrunk();
        }
    }

    [Export(PropertyHint.Range, "1,13,1")]
    public int branches_count
    {
        get
        {
            return tree.mProperties.mLevels;
        }
        set
        {
            tree.mProperties.mLevels = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0,100,1")]
    public int height
    {
        get
        {
            return tree.mProperties.mTreeSteps;
        }
        set
        {
            tree.mProperties.mTreeSteps = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0.01,10,0.001")]
    public float branch_length
    {
        get
        {
            return tree.mProperties.mInitialBranchLength;
        }
        set
        {
            tree.mProperties.mInitialBranchLength = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0.0,2,0.001")]
    public float branch_length_falloff
    {
        get
        {
            return tree.mProperties.mLengthFalloffFactor;
        }
        set
        {
            tree.mProperties.mLengthFalloffFactor = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0.1,20,0.01")]
    public float branch_factor
    {
        get
        {
            return tree.mProperties.mBranchFactor;
        }
        set
        {
            tree.mProperties.mBranchFactor = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0,20,0.01")]
    public float branch_clump_max
    {
        get
        {
            return tree.mProperties.mClumpMax;
        }
        set
        {
            tree.mProperties.mClumpMax = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0,20,0.01")]
    public float branch_clump_min
    {
        get
        {
            return tree.mProperties.mClumpMin;
        }
        set
        {
            tree.mProperties.mClumpMin = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "-5,5,0.001")]
    public float drop_amount
    {
        get
        {
            return tree.mProperties.mDropAmount;
        }
        set
        {
            tree.mProperties.mDropAmount = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "-5,5,0.001")]
    public float grow_amount
    {
        get
        {
            return tree.mProperties.mGrowAmount;
        }
        set
        {
            tree.mProperties.mGrowAmount = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "-5,5,0.01")]
    public float sweep_amount
    {
        get
        {
            return tree.mProperties.mSweepAmount;
        }
        set
        {
            tree.mProperties.mSweepAmount = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0.01,0.6,0.01")]
    public float max_radius
    {
        get
        {
            return tree.mProperties.mMaxRadius;
        }
        set
        {
            tree.mProperties.mMaxRadius = value;
            tree.Generate();
            UpdateMeshTrunk();
        }
    }

    [Export(PropertyHint.Range, "0.1,1,0.01")]
    public float radius_falloff_rate
    {
        get
        {
            return tree.mProperties.mRadiusFalloffRate;
        }
        set
        {
            tree.mProperties.mRadiusFalloffRate = value;
            tree.Generate();
            UpdateMeshTrunk();
        }
    }

    [Export(PropertyHint.Range, "0,50,0.001")]
    public float climb_rate
    {
        get
        {
            return tree.mProperties.mClimbRate;
        }
        set
        {
            tree.mProperties.mClimbRate = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "-1,1,0.001")]
    public float kink
    {
        get
        {
            return tree.mProperties.mTrunkKink;
        }
        set
        {
            tree.mProperties.mTrunkKink = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "-5,5,0.01")]
    public float twist
    {
        get
        {
            return tree.mProperties.mTwistRate;
        }
        set
        {
            tree.mProperties.mTwistRate = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0,100,0.001")]
    public float length
    {
        get
        {
            return tree.mProperties.mTrunkLength;
        }
        set
        {
            tree.mProperties.mTrunkLength = value;
            UpdateAllMeshes();
        }
    }

    [Export(PropertyHint.Range, "0.001,50,0.001")]
    public float uv_multiplier
    {
        get
        {
            return tree.mProperties.mVMultiplier;
        }
        set
        {
            tree.mProperties.mVMultiplier = value;
            tree.Generate();
            UpdateMeshTrunk();
        }
    }

    [ExportGroup("Twig")]
    [Export]
    public bool twig_enable
    {
        get
        {
            return enableTwig;
        }
        set
        {
            enableTwig = value;
            if (value)
            {
                if (!isLoaded)
                {
                    return;
                }
                twigInstance = new MeshInstance3D();
                AddChild(twigInstance, false, InternalMode.Front);
                twigInstance.Owner = this;
                twigInstance.Name = "twig_mesh";
                tree.Generate();
                UpdateMeshTwig();
            }
            else
            {
                var _twig_inst = GetNodeOrNull("twig_mesh");
                if (_twig_inst != null)
                {
                    RemoveChild(_twig_inst);
                }
                twigInstance = null;
            }
        }
    }

    [Export(PropertyHint.Range, "0,5,0.001")]
    public float twig_scale
    {
        get
        {
            return tree.mProperties.mTwigScale;
        }
        set
        {
            tree.mProperties.mTwigScale = value;
            tree.Generate();
            UpdateMeshTwig();
        }
    }

    [ExportGroup("Materials")]
    [Export(PropertyHint.ResourceType, "StandardMaterial3D,ORMMaterial3D,ShaderMaterial")]
    public Material material_trunk
    {
        get
        {
            return trunkMaterial;
        }
        set
        {
            trunkMaterial = value;
            if (trunkInstance != null)
            {
                trunkInstance.SetSurfaceOverrideMaterial(0, value);
            }
        }
    }

    [Export(PropertyHint.ResourceType, "StandardMaterial3D,ORMMaterial3D,ShaderMaterial")]
    public Material material_twig
    {
        get
        {
            return twigMaterial;
        }
        set
        {
            twigMaterial = value;
            if (twigInstance != null)
            {
                twigInstance.SetSurfaceOverrideMaterial(0, value);
            }
        }
    }

    #endregion


    public CSTree3D()
    {
        defaults = new Dictionary<StringName, Variant>{
            { "seed", tree.mProperties.mSeed },
            { "segments", tree.mProperties.mSegments },
            { "branches_count", tree.mProperties.mLevels },
            { "height", tree.mProperties.mTreeSteps },
            { "branch_length", tree.mProperties.mInitialBranchLength },
            { "branch_length_falloff", tree.mProperties.mLengthFalloffFactor },
            { "branch_factor", tree.mProperties.mBranchFactor },
            { "branch_clump_max", tree.mProperties.mClumpMax },
            { "branch_clump_min", tree.mProperties.mClumpMin },
            { "drop_amount", tree.mProperties.mDropAmount },
            { "grow_amount", tree.mProperties.mGrowAmount },
            { "sweep_amount", tree.mProperties.mSweepAmount },
            { "max_radius", tree.mProperties.mMaxRadius },
            { "radius_fallof_rate", tree.mProperties.mRadiusFalloffRate },
            { "climb_rate", tree.mProperties.mClimbRate },
            { "kink", tree.mProperties.mTrunkKink },
            { "twist", tree.mProperties.mTwistRate },
            { "length", tree.mProperties.mTrunkLength },
            { "uv_multiplier", tree.mProperties.mVMultiplier },
            { "twig_enable", enableTwig },
            { "twig_scale", tree.mProperties.mTwigScale },
        };
        trunkInstance = new MeshInstance3D();
        twigInstance = new MeshInstance3D();
        if (twigMaterial != null)
        {
            twigInstance.SetSurfaceOverrideMaterial(0, twigMaterial);
        }
    }

    public override void _EnterTree()
    {
        if (trunkInstance == null)
        {
            trunkInstance = new MeshInstance3D();
        }

        if (twigInstance == null && enableTwig)
        {
            twigInstance = new MeshInstance3D();
        }

        isLoaded = true;
        base._EnterTree();
        int childCount = GetChildCount(true);
        if (childCount == 2)
        {
            return;
        }
        if (childCount != 0)
        {
            GD.PrintErr($"Child count must be 0, but the actual value is {childCount}, this is a bug");
        }
        AddChild(trunkInstance, false, InternalMode.Front);
        trunkInstance.Name = "mesh";
        trunkInstance.Owner = this;
        if (twigInstance != null)
        {
            AddChild(twigInstance, false, InternalMode.Front);
            twigInstance.Owner = this;
            twigInstance.Name = "twig_mesh";
        }
        UpdateAllMeshes();
    }

    public override void _ExitTree()
    {
        RemoveChild(trunkInstance);
        trunkInstance = null;
        if (twigInstance != null)
        {
            RemoveChild(twigInstance);
            twigInstance = null;
        }
    }

    void UpdateMeshTrunk()
    {
        SurfaceTool st = new SurfaceTool();
        st.Clear();
        st.Begin(Mesh.PrimitiveType.Triangles);
        for (int i = 0; i < tree.mVertCount; i++)
        {
            st.SetUV(new Vector2(tree.mUV[i].U, tree.mUV[i].V));
            st.SetNormal(-tree.mNormal[i]);
            st.AddVertex(tree.mVert[i]);
        }

        for (int i = 0; i < tree.mFaceCount; i++)
        {
            st.AddIndex(tree.mFace[i].X);
            st.AddIndex(tree.mFace[i].Z);
            st.AddIndex(tree.mFace[i].Y);
        }
        st.OptimizeIndicesForCache();
        trunkInstance.SetMesh(st.Commit());
        st.Clear();
        trunkInstance.SetSurfaceOverrideMaterial(0, trunkMaterial);
    }

    void UpdateMeshTwig()
    {
        if (twigInstance != null)
        {
            var st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);
            for (int i = 0; i < tree.mTwigVertCount; i++)
            {
                st.SetUV(new Vector2(tree.mTwigUV[i].U, tree.mTwigUV[i].V));
                st.SetNormal(-tree.mTwigNormal[i]);
                st.AddVertex(tree.mTwigVert[i]);
            }

            for (int i = 0; i < tree.mTwigFaceCount; i++)
            {
                st.AddIndex(tree.mTwigFace[i].X);
                st.AddIndex(tree.mTwigFace[i].Y);
                st.AddIndex(tree.mTwigFace[i].Z);
            }
            st.OptimizeIndicesForCache();
            twigInstance.SetMesh(st.Commit());
            st.Clear();
            twigInstance.SetSurfaceOverrideMaterial(0, twigMaterial);
        }
        else
        {
            return;
        }
    }

    void UpdateAllMeshes()
    {
        tree.Generate();
        UpdateMeshTrunk();
        if (enableTwig)
        {
            UpdateMeshTwig();
        }
    }
}
