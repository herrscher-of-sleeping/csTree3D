using Godot;
using System;

public struct UV
{
    public float U;
    public float V;

    public UV(float u, float v)
    {
        U = u;
        V = v;
    }
}

public class Proctree
{
    public Properties mProperties = new Properties();
    public int mVertCount;
    public int mTwigVertCount;
    public int mFaceCount;
    public int mTwigFaceCount;

    public Vector3[] mVert;
    public Vector3[] mNormal;
    public UV[] mUV;
    public Vector3[] mTwigVert;
    public Vector3[] mTwigNormal;
    public UV[] mTwigUV;
    public Vector3I[] mFace;
    public Vector3I[] mTwigFace;

    Branch mRoot;

    private void init()
    {
        mRoot = null;
        mVert = null;
        mNormal = null;
        mUV = null;
        mTwigVert = null;
        mTwigNormal = null;
        mTwigUV = null;
        mFace = null;
        mTwigFace = null;

        mVertCount = 0;
        mTwigVertCount = 0;
        mFaceCount = 0;
        mTwigFaceCount = 0;
    }

    private void AllocVertBuffers()
    {
        mVert = new Vector3[mVertCount];
        mNormal = new Vector3[mVertCount];
        mUV = new UV[mVertCount];
        mTwigVert = new Vector3[mTwigVertCount];
        mTwigNormal = new Vector3[mTwigVertCount];
        mTwigUV = new UV[mTwigVertCount];
        mTwigFace = new Vector3I[mTwigFaceCount];

        // Reset back to zero, we'll use these as counters

        mVertCount = 0;
        mTwigVertCount = 0;
        mTwigFaceCount = 0;
    }

    private void AllocFaceBuffers()
    {
        mFace = new Vector3I[mFaceCount];

        // Reset back to zero, we'll use these as counters

        mFaceCount = 0;
    }

    private void CalcVertSizes(Branch aBranch)
    {
        int segments = mProperties.mSegments;
        if (aBranch == null)
            aBranch = mRoot;

        if (aBranch.mParent == null)
        {
            mVertCount += segments;
        }

        if (aBranch.mChild0 != null)
        {
            mVertCount +=
                1 +
                (segments / 2) - 1 +
                1 +
                (segments / 2) - 1 +
                (segments / 2) - 1;

            CalcVertSizes(aBranch.mChild0);
            CalcVertSizes(aBranch.mChild1);
        }
        else
        {
            mVertCount++;
            mTwigVertCount += 8;
            mTwigFaceCount += 4;
        }
    }

    private void CalcFaceSizes(Branch aBranch)
    {
        int segments = mProperties.mSegments;
        if (aBranch == null)
            aBranch = mRoot;

        if (aBranch.mParent == null)
        {
            mFaceCount += segments * 2;
        }

        if (aBranch.mChild0.mRing0 != null)
        {
            mFaceCount += segments * 4;

            CalcFaceSizes(aBranch.mChild0);
            CalcFaceSizes(aBranch.mChild1);
        }
        else
        {
            mFaceCount += segments * 2;
        }
    }

    private void CalcNormals()
    {
        int[] normalCount = new int[mVertCount];
        for (int i = 0; i < mVertCount; i++)
        {
            normalCount[i] = 0;
            mNormal[i] = new Vector3(0, 0, 0);
        }

        for (int i = 0; i < mFaceCount; i++)
        {
            normalCount[mFace[i].X]++;
            normalCount[mFace[i].Y]++;
            normalCount[mFace[i].Z]++;

            Vector3 norm = (mVert[mFace[i].Y] - mVert[mFace[i].Z]).Cross(mVert[mFace[i].Y] - mVert[mFace[i].X]).Normalized();

            mNormal[mFace[i].X] += norm;
            mNormal[mFace[i].Y] += norm;
            mNormal[mFace[i].Z] += norm;
        }

        for (int i = 0; i < mVertCount; i++)
        {
            float d = 1.0f / normalCount[i];
            mNormal[i] *= d;
        }
    }

    private void DoFaces(Branch aBranch)
    {

        if (aBranch == null)
        {
            aBranch = mRoot;
        }
        int segments = mProperties.mSegments;
        int i;
        if (aBranch.mParent == null)
        {
            Vector3 tangent = (aBranch.mChild0.mHead - aBranch.mHead).Cross(aBranch.mChild1.mHead - aBranch.mHead).Normalized();
            Vector3 normal = aBranch.mHead.Normalized();
            Vector3 left = new Vector3(-1, 0, 0);
            float angle = Mathf.Acos(tangent.Dot(left));
            if (left.Cross(tangent).Dot(normal) > 0)
            {
                angle = 2 * Mathf.Pi - angle;
            }
            int segOffset = (int)Mathf.Floor(0.5f + (angle / Mathf.Pi / 2 * segments));
            for (i = 0; i < segments; i++)
            {
                int v1 = aBranch.mRing0[i];
                int v2 = aBranch.mRootRing[(i + segOffset + 1) % segments];
                int v3 = aBranch.mRootRing[(i + segOffset) % segments];
                int v4 = aBranch.mRing0[(i + 1) % segments];

                Vector3I a = new Vector3I(v1, v4, v3);
                mFace[mFaceCount++] = a;
                a = new Vector3I(v4, v2, v3);
                mFace[mFaceCount++] = a;

                mUV[(i + segOffset) % segments] = new UV(i / (float)segments, 0);

                float len = (mVert[aBranch.mRing0[i]] - mVert[aBranch.mRootRing[(i + segOffset) % segments]]).Length() * mProperties.mVMultiplier;
                mUV[aBranch.mRing0[i]] = new UV(i / (float)segments, len);
                mUV[aBranch.mRing2[i]] = new UV(i / (float)segments, len);
            }
        }

        if (aBranch.mChild0.mRing0 != null)
        {
            int segOffset0 = -1, segOffset1 = -1;
            float match0 = 0;
            float match1 = 0;

            Vector3 v1 = (mVert[aBranch.mRing1[0]] - aBranch.mHead).Normalized();
            Vector3 v2 = (mVert[aBranch.mRing2[0]] - aBranch.mHead).Normalized();

            v1 = ScaleInDirection(v1, (aBranch.mChild0.mHead - aBranch.mHead).Normalized(), 0);
            v2 = ScaleInDirection(v2, (aBranch.mChild1.mHead - aBranch.mHead).Normalized(), 0);

            for (i = 0; i < segments; i++)
            {
                Vector3 d = (mVert[aBranch.mChild0.mRing0[i]] - aBranch.mChild0.mHead).Normalized();
                float l = d.Dot(v1);
                if (segOffset0 == -1 || l > match0)
                {
                    match0 = l;
                    segOffset0 = segments - i;
                }
                d = (mVert[aBranch.mChild1.mRing0[i]] - aBranch.mChild1.mHead).Normalized();
                l = d.Dot(v2);
                if (segOffset1 == -1 || l > match1)
                {
                    match1 = l;
                    segOffset1 = segments - i;
                }
            }

            float UVScale = mProperties.mMaxRadius / aBranch.mRadius;

            for (i = 0; i < segments; i++)
            {
                int v1i = aBranch.mChild0.mRing0[i];
                int v2i = aBranch.mRing1[(i + segOffset0 + 1) % segments];
                int v3i = aBranch.mRing1[(i + segOffset0) % segments];
                int v4i = aBranch.mChild0.mRing0[(i + 1) % segments];
                mFace[mFaceCount++] = new Vector3I(v1i, v4i, v3i);
                mFace[mFaceCount++] = new Vector3I(v4i, v2i, v3i);

                v1i = aBranch.mChild1.mRing0[i];
                v2i = aBranch.mRing2[(i + segOffset1 + 1) % segments];
                v3i = aBranch.mRing2[(i + segOffset1) % segments];
                v4i = aBranch.mChild1.mRing0[(i + 1) % segments];

                mFace[mFaceCount++] = new Vector3I(v1i, v2i, v3i);
                mFace[mFaceCount++] = new Vector3I(v1i, v4i, v2i);

                float len1 = (mVert[aBranch.mChild0.mRing0[i]] - mVert[aBranch.mRing1[(i + segOffset0) % segments]]).Length() * UVScale;
                UV uv1 = mUV[aBranch.mRing1[(i + segOffset0 - 1) % segments]];

                mUV[aBranch.mChild0.mRing0[i]] = new UV(uv1.U, uv1.V + len1 * mProperties.mVMultiplier);
                mUV[aBranch.mChild0.mRing2[i]] = new UV(uv1.U, uv1.V + len1 * mProperties.mVMultiplier);

                float len2 = (mVert[aBranch.mChild1.mRing0[i]] - mVert[aBranch.mRing2[(i + segOffset1) % segments]]).Length() * UVScale;
                UV uv2 = mUV[aBranch.mRing2[(i + segOffset1 - 1) % segments]];

                mUV[aBranch.mChild1.mRing0[i]] = new UV(uv2.U, uv2.V + len2 * mProperties.mVMultiplier);
                mUV[aBranch.mChild1.mRing2[i]] = new UV(uv2.U, uv2.V + len2 * mProperties.mVMultiplier);
            }

            DoFaces(aBranch.mChild0);
            DoFaces(aBranch.mChild1);
        }
        else
        {
            for (i = 0; i < segments; i++)
            {
                mFace[mFaceCount++] = new Vector3I(
                    aBranch.mChild0.mEnd,
                    aBranch.mRing1[(i + 1) % segments],
                    aBranch.mRing1[i]
                );

                mFace[mFaceCount++] = new Vector3I(
                    aBranch.mChild1.mEnd,
                    aBranch.mRing2[(i + 1) % segments],
                    aBranch.mRing2[i]
                );

                float len = (mVert[aBranch.mChild0.mEnd] - mVert[aBranch.mRing1[i]]).Length();
                mUV[aBranch.mChild0.mEnd] = new UV(i / (float)segments - 1, len * mProperties.mVMultiplier);
                len = (mVert[aBranch.mChild1.mEnd] - mVert[aBranch.mRing2[i]]).Length();
                mUV[aBranch.mChild1.mEnd] = new UV(i / (float)segments, len * mProperties.mVMultiplier);
            }
        }

    }
    private void CreateTwigs(Branch aBranch)
    {
        if (aBranch == null)
        {
            aBranch = mRoot;
        }

        if (aBranch.mChild0 == null)
        {
            Vector3 tangent = (aBranch.mParent.mChild0.mHead - aBranch.mParent.mHead).Cross(aBranch.mParent.mChild1.mHead - aBranch.mParent.mHead).Normalized();
            Vector3 binormal = (aBranch.mHead - aBranch.mParent.mHead).Normalized();
            //Vector3 normal = cross(tangent, binormal); //never used

            int vert1 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * mProperties.mTwigScale + binormal * (mProperties.mTwigScale * 2 - aBranch.mLength);
            int vert2 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * (-mProperties.mTwigScale) + binormal * (mProperties.mTwigScale * 2 - aBranch.mLength);
            int vert3 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * (-mProperties.mTwigScale) + binormal * (-aBranch.mLength);
            int vert4 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * mProperties.mTwigScale + binormal * (-aBranch.mLength);

            int vert8 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * mProperties.mTwigScale + binormal * (mProperties.mTwigScale * 2 - aBranch.mLength);
            int vert7 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * (-mProperties.mTwigScale) + binormal * (mProperties.mTwigScale * 2 - aBranch.mLength);
            int vert6 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * (-mProperties.mTwigScale) + binormal * (-aBranch.mLength);
            int vert5 = mTwigVertCount;
            mTwigVert[mTwigVertCount++] = aBranch.mHead + tangent * mProperties.mTwigScale + binormal * (-aBranch.mLength);

            mTwigFace[mTwigFaceCount++] = new Vector3I(vert1, vert2, vert3);
            mTwigFace[mTwigFaceCount++] = new Vector3I(vert4, vert1, vert3);
            mTwigFace[mTwigFaceCount++] = new Vector3I(vert6, vert7, vert8);
            mTwigFace[mTwigFaceCount++] = new Vector3I(vert6, vert8, vert5);

            Vector3 normal = (mTwigVert[vert1] - mTwigVert[vert3]).Cross(mTwigVert[vert2] - mTwigVert[vert3]).Normalized();
            Vector3 normal2 = (mTwigVert[vert7] - mTwigVert[vert6]).Cross(mTwigVert[vert8] - mTwigVert[vert6]).Normalized();

            mTwigNormal[vert1] = normal;
            mTwigNormal[vert2] = normal;
            mTwigNormal[vert3] = normal;
            mTwigNormal[vert4] = normal;

            mTwigNormal[vert8] = normal2;
            mTwigNormal[vert7] = normal2;
            mTwigNormal[vert6] = normal2;
            mTwigNormal[vert5] = normal2;

            mTwigUV[vert1] = new UV(0, 0);
            mTwigUV[vert2] = new UV(1, 0);
            mTwigUV[vert3] = new UV(1, 1);
            mTwigUV[vert4] = new UV(0, 1);

            mTwigUV[vert8] = new UV(0, 0);
            mTwigUV[vert7] = new UV(1, 0);
            mTwigUV[vert6] = new UV(1, 1);
            mTwigUV[vert5] = new UV(0, 1);
        }
        else
        {
            CreateTwigs(aBranch.mChild0);
            CreateTwigs(aBranch.mChild1);
        }
    }

    Vector3 VecAxisAngle(Vector3 aVec, Vector3 aAxis, float aAngle)
    {
        //v std::cos(T) + (axis x v) * std::sin(T) + axis*(axis . v)(1-std::cos(T)
        float cosr = Mathf.Cos(aAngle);
        float sinr = Mathf.Sin(aAngle);
        return aVec * cosr + aAxis.Cross(aVec) * sinr + aAxis * aAxis.Dot(aVec) * (1 - cosr);
    }

    Vector3 ScaleInDirection(Vector3 aVector, Vector3 aDirection, float aScale)
    {
        float currentMag = aVector.Dot(aDirection);

        Vector3 change = aDirection * (currentMag * aScale - currentMag);
        return aVector + change;
    }

    private void CreateForks(Branch aBranch, float aRadius)
    {
        if (aBranch == null) aBranch = mRoot;
        if (aRadius == 0) aRadius = mProperties.mMaxRadius;

        aBranch.mRadius = aRadius;

        if (aRadius > aBranch.mLength) aRadius = aBranch.mLength;

        int segments = mProperties.mSegments;

        float segmentAngle = Mathf.Pi * 2 / (float)segments;

        if (aBranch.mParent == null)
        {
            aBranch.mRootRing = new int[segments];
            //create the root of the tree
            //branch.root = [];
            Vector3 axis = new Vector3(0, 1, 0);
            int i;
            for (i = 0; i < segments; i++)
            {
                Vector3 left = new Vector3(-1, 0, 0);
                Vector3 vec = VecAxisAngle(left, axis, -segmentAngle * i);
                aBranch.mRootRing[i] = mVertCount;
                mVert[mVertCount++] = vec * (aRadius / mProperties.mRadiusFalloffRate);
            }
        }

        //cross the branches to get the left
        //add the branches to get the up
        if (aBranch.mChild0 != null)
        {
            Vector3 axis;
            if (aBranch.mParent != null)
            {
                axis = (aBranch.mHead - aBranch.mParent.mHead).Normalized();
            }
            else
            {
                axis = aBranch.mHead.Normalized();
            }

            Vector3 axis1 = (aBranch.mHead - aBranch.mChild0.mHead).Normalized();
            Vector3 axis2 = (aBranch.mHead - aBranch.mChild1.mHead).Normalized();
            Vector3 tangent = axis1.Cross(axis2).Normalized();
            aBranch.mTangent = tangent;

            Vector3 axis3 = tangent.Cross((-axis1 - axis2).Normalized()).Normalized();
            Vector3 dir = new Vector3(axis2.X, 0, axis2.Z);
            Vector3 centerloc = aBranch.mHead - dir * mProperties.mMaxRadius / 2;

            aBranch.mRing0 = new int[segments];
            aBranch.mRing1 = new int[segments];
            aBranch.mRing2 = new int[segments];

            int ring0count = 0;
            int ring1count = 0;
            int ring2count = 0;

            float scale = mProperties.mRadiusFalloffRate;

            if (aBranch.mChild0.mTrunktype != 0 || aBranch.mTrunktype != 0)
            {
                scale = 1.0f / mProperties.mTaperRate;
            }

            //main segment ring
            int linch0 = mVertCount;
            aBranch.mRing0[ring0count++] = linch0;
            aBranch.mRing2[ring2count++] = linch0;
            mVert[mVertCount++] = centerloc + tangent * aRadius * scale;

            int start = mVertCount - 1;
            Vector3 d1 = VecAxisAngle(tangent, axis2, 1.57f);
            Vector3 d2 = tangent.Cross(axis).Normalized();
            float s = 1 / d1.Dot(d2);
            int i;
            for (i = 1; i < segments / 2; i++)
            {
                Vector3 vec = VecAxisAngle(tangent, axis2, segmentAngle * i);
                aBranch.mRing0[ring0count++] = start + i;
                aBranch.mRing2[ring2count++] = start + i;
                vec = ScaleInDirection(vec, d2, s);
                mVert[mVertCount++] = centerloc + vec * aRadius * scale;
            }
            int linch1 = mVertCount;
            aBranch.mRing0[ring0count++] = linch1;
            aBranch.mRing1[ring1count++] = linch1;
            mVert[mVertCount++] = centerloc - tangent * aRadius * scale;
            for (i = segments / 2 + 1; i < segments; i++)
            {
                Vector3 vec = VecAxisAngle(tangent, axis1, segmentAngle * i);
                aBranch.mRing0[ring0count++] = mVertCount;
                aBranch.mRing1[ring1count++] = mVertCount;
                mVert[mVertCount++] = centerloc + vec * aRadius * scale;
            }
            aBranch.mRing1[ring1count++] = linch0;
            aBranch.mRing2[ring2count++] = linch1;
            start = mVertCount - 1;
            for (i = 1; i < segments / 2; i++)
            {
                Vector3 vec = VecAxisAngle(tangent, axis3, segmentAngle * i);
                aBranch.mRing1[ring1count++] = start + i;
                aBranch.mRing2[ring2count++] = start + (segments / 2 - i);
                Vector3 v = vec * aRadius * scale;
                mVert[mVertCount++] = centerloc + v;
            }

            //child radius is related to the brans direction and the length of the branch
            //float length0 = length(sub(aBranch.mHead, aBranch.mChild0.mHead)); // never used
            //float length1 = length(sub(aBranch.mHead, aBranch.mChild1.mHead)); // never used

            float radius0 = 1 * aRadius * mProperties.mRadiusFalloffRate;
            float radius1 = 1 * aRadius * mProperties.mRadiusFalloffRate;
            if (aBranch.mChild0.mTrunktype != 0)
            {
                radius0 = aRadius * mProperties.mTaperRate;
            }
            CreateForks(aBranch.mChild0, radius0);
            CreateForks(aBranch.mChild1, radius1);
        }
        else
        {
            //add points for the ends of braches
            aBranch.mEnd = mVertCount;
            //branch.head=add(branch.head,scaleVec([this.properties.xBias,this.properties.yBias,this.properties.zBias],branch.length*3));
            mVert[mVertCount++] = (aBranch.mHead);
        }
    }

    private void FixUVs()
    {
        // There'll never be more than 50% bad vertices
        int[] badverttable = new int[mVertCount / 2];
        int i;
        int badverts = 0;

        // step 1: find bad verts
        // - If edge's U coordinate delta is over 0.5, texture has wrapped around. 
        // - The vertex that has zero U is the wrong one
        // - Care needs to be taken not to tag bad vertex more than once.

        for (i = 0; i < mFaceCount; i++)
        {
            // x/y edges (vertex 0 and 1)
            if ((Math.Abs(mUV[mFace[i].X].U - mUV[mFace[i].Y].U) > 0.5f) && (mUV[mFace[i].X].U == 0 || mUV[mFace[i].Y].U == 0))
            {
                int found = 0, j;
                for (j = 0; j < badverts; j++)
                {
                    if (badverttable[j] == mFace[i].Y && mUV[mFace[i].Y].U == 0)
                        found = 1;
                    if (badverttable[j] == mFace[i].X && mUV[mFace[i].X].U == 0)
                        found = 1;
                }
                if (found == 0)
                {
                    if (mUV[mFace[i].X].U == 0)
                        badverttable[badverts] = mFace[i].X;
                    if (mUV[mFace[i].Y].U == 0)
                        badverttable[badverts] = mFace[i].Y;
                    badverts++;
                }
            }

            // x/z edges (vertex 0 and 2)
            if ((Mathf.Abs(mUV[mFace[i].X].U - mUV[mFace[i].Z].U) > 0.5f) && (mUV[mFace[i].X].U == 0 || mUV[mFace[i].Z].U == 0))
            {
                int found = 0, j;
                for (j = 0; j < badverts; j++)
                {
                    if (badverttable[j] == mFace[i].Z && mUV[mFace[i].Z].U == 0)
                        found = 1;
                    if (badverttable[j] == mFace[i].X && mUV[mFace[i].X].U == 0)
                        found = 1;
                }
                if (found == 0)
                {
                    if (mUV[mFace[i].X].U == 0)
                        badverttable[badverts] = mFace[i].X;
                    if (mUV[mFace[i].Z].U == 0)
                        badverttable[badverts] = mFace[i].Z;
                    badverts++;
                }
            }

            // y/z edges (vertex 1 and 2)
            if ((Mathf.Abs(mUV[mFace[i].Y].U - mUV[mFace[i].Z].U) > 0.5f) && (mUV[mFace[i].Y].U == 0 || mUV[mFace[i].Z].U == 0))
            {
                int found = 0, j;
                for (j = 0; j < badverts; j++)
                {
                    if (badverttable[j] == mFace[i].Z && mUV[mFace[i].Z].U == 0)
                        found = 1;
                    if (badverttable[j] == mFace[i].Y && mUV[mFace[i].Y].U == 0)
                        found = 1;
                }
                if (found == 0)
                {
                    if (mUV[mFace[i].Y].U == 0)
                        badverttable[badverts] = mFace[i].Y;
                    if (mUV[mFace[i].Z].U == 0)
                        badverttable[badverts] = mFace[i].Z;
                    badverts++;
                }
            }
        }

        // step 2: allocate more space for our new duplicate verts
        Array.Resize(ref mVert, mVertCount + badverts);
        Array.Resize(ref mNormal, mVertCount + badverts);
        Array.Resize(ref mUV, mVertCount + badverts);

        // step 3: populate duplicate verts - otherwise identical except for U=1 instead of 0

        for (i = 0; i < badverts; i++)
        {
            mVert[mVertCount + i] = mVert[badverttable[i]];
            mNormal[mVertCount + i] = mNormal[badverttable[i]];
            mUV[mVertCount + i] = mUV[badverttable[i]];
            mUV[mVertCount + i].U = 1.0f;
        }

        // step 4: fix faces

        for (i = 0; i < mFaceCount; i++)
        {
            // x/y edges (vertex 0 and 1)
            if ((Mathf.Abs(mUV[mFace[i].X].U - mUV[mFace[i].Y].U) > 0.5f) && (mUV[mFace[i].X].U == 0 || mUV[mFace[i].Y].U == 0))
            {
                int found = 0, j;
                for (j = 0; j < badverts; j++)
                {
                    if (badverttable[j] == mFace[i].Y && mUV[mFace[i].Y].U == 0)
                        found = j;
                    if (badverttable[j] == mFace[i].X && mUV[mFace[i].X].U == 0)
                        found = j;
                }
                if (mUV[mFace[i].Y].U == 0)
                    mFace[i].Y = mVertCount + found;
                if (mUV[mFace[i].X].U == 0)
                    mFace[i].X = mVertCount + found;
            }

            // x/z edges (vertex 0 and 2)
            if ((Mathf.Abs(mUV[mFace[i].X].U - mUV[mFace[i].Z].U) > 0.5f) && (mUV[mFace[i].X].U == 0 || mUV[mFace[i].Z].U == 0))
            {
                int found = 0, j;
                for (j = 0; j < badverts; j++)
                {
                    if (badverttable[j] == mFace[i].Z && mUV[mFace[i].Z].U == 0)
                        found = j;
                    if (badverttable[j] == mFace[i].X && mUV[mFace[i].X].U == 0)
                        found = j;
                }
                if (mUV[mFace[i].X].U == 0)
                    mFace[i].X = mVertCount + found;
                if (mUV[mFace[i].Z].U == 0)
                    mFace[i].Z = mVertCount + found;
            }

            // y/z edges (vertex 1 and 2)
            if ((Mathf.Abs(mUV[mFace[i].Y].U - mUV[mFace[i].Z].U) > 0.5f) && (mUV[mFace[i].Y].U == 0 || mUV[mFace[i].Z].U == 0))
            {
                int found = 0, j;
                for (j = 0; j < badverts; j++)
                {
                    if (badverttable[j] == mFace[i].Z && mUV[mFace[i].Z].U == 0)
                        found = j;
                    if (badverttable[j] == mFace[i].Y && mUV[mFace[i].Y].U == 0)
                        found = j;
                }
                if (mUV[mFace[i].Y].U == 0)
                    mFace[i].Y = mVertCount + found;
                if (mUV[mFace[i].Z].U == 0)
                    mFace[i].Z = mVertCount + found;
            }
        }

        // step 5: update vert count
        mVertCount += badverts;
    }

    public Proctree()
    {
        init();
    }

    public void Generate()
    {
        init();
        mProperties.mRseed = mProperties.mSeed;
        var starthead = new Vector3(0, mProperties.mTrunkLength, 0);
        mRoot = new Branch(starthead, null);
        mRoot.mLength = mProperties.mInitialBranchLength;
        mRoot.Split(mProperties.mLevels, mProperties.mTreeSteps, mProperties);

        CalcVertSizes(null);
        AllocVertBuffers();
        CreateForks(null, 0);
        CreateTwigs(null);
        CalcFaceSizes(null);
        AllocFaceBuffers();
        DoFaces(null);
        CalcNormals();
        FixUVs();
        mRoot = null;
    }
};