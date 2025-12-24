using System;
using UnityEngine;

[Serializable]
public struct RuntimeLevelParameters
{
    public int width;
    public int height;
    public int curvePercent;
    public int minStarDistance;
}
public static class RuntimeLevelProgression
{
    public static RuntimeLevelParameters GetParametersForLevel(int levelIndex)
    {
        RuntimeLevelParameters p = new RuntimeLevelParameters();

        // -------------------------
        // INITIAL VALUES (STEP 1)
        // -------------------------
        int width = 3;
        int height = 10;
        int curve = 30;
        int starDistance = 2;

        // -------------------------
        // PHASE CONTROL
        // -------------------------
        int phase = 0;

        /*
            Phase 0: Width 3 → 10 (Height 10)
            Phase 1: Curve 30 → 0
            Phase 2: Width+Height += 2, Curve reset to 30
            Phase 3: Curve 30 → 0
            Phase 4+: repeat 2 & 3 until width/height reach 20
            Star distance increases after max size reached
        */

        for (int i = 0; i < levelIndex; i++)
        {
            switch (phase)
            {
                // STEP 2 - Width increase
                case 0:
                    width++;
                    starDistance = 2;
                    if (width >= 10)
                        phase = 1;
                    break;

                // STEP 3 - Curve decrease
                case 1:
                    starDistance = 4;
                    curve--;
                    if (curve <= 0)
                        phase = 2;
                    break;

                // STEP 4  - Curve reset, Width/Height increase (+ 2)
                case 2:
                    curve = 30;
                    width += 2;
                    height += 2;

                    if (width < 20 || height < 20)
                        phase = 3;
                    else
                        phase = 4;
                    break;

                // STEP 5 - (curve decrease again)
                case 3:
                    curve--;
                    if (curve <= 0)
                        phase = 2;
                    break;

                // STEP 6 - Star distance increase
                case 4:
                    if (starDistance < 10)
                        starDistance++;
                    else phase = 0; // Phase reset
                        break;
            }
        }

        p.width = width;
        p.height = height;
        p.curvePercent = Mathf.Clamp(curve, 0, 100);
        p.minStarDistance = Mathf.Clamp(starDistance, 1, 10);

        return p;
    }
}
