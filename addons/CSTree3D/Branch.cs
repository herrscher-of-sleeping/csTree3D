using System;
using Godot;

public class Branch
{
    public Branch mChild0;
    public Branch mChild1;
    public Branch mParent;
    public Vector3 mHead;
    public Vector3 mTangent;
    public float mLength;
    public int mTrunktype;
    public int[] mRing0, mRing1, mRing2;
    public int[] mRootRing;
    public float mRadius;
    public int mEnd;


    ~Branch() { }

    public Branch()
    {
        mRootRing = null;
        mRing0 = null;
        mRing1 = null;
        mRing2 = null;
        mChild0 = null;
        mChild1 = null;
        mParent = null;
        mLength = 1;
        mTrunktype = 0;
        mRadius = 0;
        mHead = new Vector3(0, 0, 0);
        mTangent = new Vector3(0, 0, 0);
        mEnd = 0;
    }
    public Branch(Vector3 aHead, Branch aParent)
    {
        mRootRing = null;
        mRing0 = null;
        mRing1 = null;
        mRing2 = null;
        mChild0 = null;
        mChild1 = null;
        mLength = 1;
        mTrunktype = 0;
        mRadius = 0;
        mHead = aHead;
        mTangent = new Vector3(0, 0, 0);
        mParent = aParent;
        mEnd = 0;
    }


    public static Vector3 MirrorBranch(Vector3 aVec, Vector3 aNorm, Properties aProperties)
    {
        Vector3 v = aNorm.Cross(aVec.Cross(aNorm));
        float s = aProperties.mBranchFactor * v.Dot(aVec);
        Vector3 res = new Vector3(
            aVec.X - v.X * s,
            aVec.Y - v.Y * s,
            aVec.Z - v.Z * s
        );
        return res;
    }

    public void Split(int aLevel, int aSteps, Properties aProperties, int aL1 = 1, int aL2 = 1)
    {
        int rLevel = aProperties.mLevels - aLevel;
        Vector3 po;
        if (mParent != null)
        {
            po = mParent.mHead;
        }
        else
        {
            po = new Vector3(0, 0, 0);
            mTrunktype = 1;
        }
        Vector3 so = mHead;
        Vector3 dir = (so - po).Normalized(); // normalize(sub(so, po));

        Vector3 a = new Vector3(dir.Z, dir.X, dir.Y);
        Vector3 normal = dir.Cross(a);
        Vector3 tangent = dir.Cross(normal);
        float r = aProperties.random(rLevel * 10 + aL1 * 5.0f + aL2 + aProperties.mSeed);
        //float r2 = aProperties.random(rLevel * 10 + aL1 * 5.0f + aL2 + 1 + aProperties.seed); // never used

        Vector3 adj = normal * r + tangent * (1 - r);
        if (r > 0.5) adj = -adj;

        float clump = (aProperties.mClumpMax - aProperties.mClumpMin) * r + aProperties.mClumpMin;
        Vector3 newdir = (adj * (1 - clump) + dir * clump).Normalized();


        Vector3 newdir2 = MirrorBranch(newdir, dir, aProperties);
        if (r > 0.5)
        {
            Vector3 tmp = newdir;
            newdir = newdir2;
            newdir2 = tmp;
        }

        if (aSteps > 0)
        {
            float angle = aSteps / (float)aProperties.mTreeSteps * 2 * Mathf.Pi * aProperties.mTwistRate;
            a = new Vector3(Mathf.Sin(angle), r, Mathf.Cos(angle));
            newdir2 = a.Normalized();
        }

        float growAmount = aLevel * aLevel / (float)(aProperties.mLevels * aProperties.mLevels) * aProperties.mGrowAmount;
        float dropAmount = rLevel * aProperties.mDropAmount;
        float sweepAmount = rLevel * aProperties.mSweepAmount;
        a = new Vector3(sweepAmount, dropAmount + growAmount, 0);
        newdir = (newdir + a).Normalized();
        newdir2 = (newdir2 + a).Normalized();

        Vector3 head0 = so + newdir * mLength;
        Vector3 head1 = so + newdir2 * mLength;
        mChild0 = new Branch(head0, this);
        mChild1 = new Branch(head1, this);
        mChild0.mLength = Mathf.Pow(mLength, aProperties.mLengthFalloffPower) * aProperties.mLengthFalloffFactor;
        mChild1.mLength = Mathf.Pow(mLength, aProperties.mLengthFalloffPower) * aProperties.mLengthFalloffFactor;

        if (aLevel > 0)
        {
            if (aSteps > 0)
            {
                a = new Vector3(
                    (r - 0.5f) * 2 * aProperties.mTrunkKink,
                    aProperties.mClimbRate,
                    (r - 0.5f) * 2 * aProperties.mTrunkKink
                );
                mChild0.mHead = mHead + a;
                mChild0.mTrunktype = 1;
                mChild0.mLength = mLength * aProperties.mTaperRate;
                mChild0.Split(aLevel, aSteps - 1, aProperties, aL1 + 1, aL2);
            }
            else
            {
                mChild0.Split(aLevel - 1, 0, aProperties, aL1 + 1, aL2);
            }
            mChild1.Split(aLevel - 1, 0, aProperties, aL1, aL2 + 1);
        }
    }
}
