using Godot;
using System;

public struct Properties
{
    public float mClumpMax;
    public float mClumpMin;
    public float mLengthFalloffFactor;
    public float mLengthFalloffPower;
    public float mBranchFactor;
    public float mRadiusFalloffRate;
    public float mClimbRate;
    public float mTrunkKink;
    public float mMaxRadius;
    public int mTreeSteps;
    public float mTaperRate;
    public float mTwistRate;
    public int mSegments;
    public int mLevels;
    public float mSweepAmount;
    public float mInitialBranchLength;
    public float mTrunkLength;
    public float mDropAmount;
    public float mGrowAmount;
    public float mVMultiplier;
    public float mTwigScale;
    public int mSeed;
    public int mRseed;

    public Properties(
        float aClumpMax,
        float aClumpMin,
        float aLengthFalloffFactor,
        float aLengthFalloffPower,
        float aBranchFactor,
        float aRadiusFalloffRate,
        float aClimbRate,
        float aTrunkKink,
        float aMaxRadius,
        int aTreeSteps,
        float aTaperRate,
        float aTwistRate,
        int aSegments,
        int aLevels,
        float aSweepAmount,
        float aInitialBranchLength,
        float aTrunkLength,
        float aDropAmount,
        float aGrowAmount,
        float aVMultiplier,
        float aTwigScale,
        int aSeed)
    {
        mSeed = aSeed;
        mSegments = aSegments;
        mLevels = aLevels;
        mVMultiplier = aVMultiplier;
        mTwigScale = aTwigScale;
        mInitialBranchLength = aInitialBranchLength;
        mLengthFalloffFactor = aLengthFalloffFactor;
        mLengthFalloffPower = aLengthFalloffPower;
        mClumpMax = aClumpMax;
        mClumpMin = aClumpMin;
        mBranchFactor = aBranchFactor;
        mDropAmount = aDropAmount;
        mGrowAmount = aGrowAmount;
        mSweepAmount = aSweepAmount;
        mMaxRadius = aMaxRadius;
        mClimbRate = aClimbRate;
        mTrunkKink = aTrunkKink;
        mTreeSteps = aTreeSteps;
        mTaperRate = aTaperRate;
        mRadiusFalloffRate = aRadiusFalloffRate;
        mTwistRate = aTwistRate;
        mTrunkLength = aTrunkLength;
    }

    public Properties()
    {
        mSeed = 262;
        mSegments = 6;
        mLevels = 5;
        mVMultiplier = 0.36f;
        mTwigScale = 0.39f;
        mInitialBranchLength = 0.49f;
        mLengthFalloffFactor = 0.85f;
        mLengthFalloffPower = 0.99f;
        mClumpMax = 0.454f;
        mClumpMin = 0.404f;
        mBranchFactor = 2.45f;
        mDropAmount = -0.1f;
        mGrowAmount = 0.235f;
        mSweepAmount = 0.01f;
        mMaxRadius = 0.139f;
        mClimbRate = 0.371f;
        mTrunkKink = 0.093f;
        mTreeSteps = 5;
        mTaperRate = 0.947f;
        mRadiusFalloffRate = 0.73f;
        mTwistRate = 3.02f;
        mTrunkLength = 2.4f;
    }

    public float random(float aFixed)
    {
        if (aFixed == 0)
        {
            aFixed = mRseed++;
        }
        return (float)Math.Abs(Math.Cos(aFixed + aFixed * aFixed));
    }
}
