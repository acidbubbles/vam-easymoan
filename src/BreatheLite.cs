using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
namespace geesp0t
{
    //from MacGruber Breathe, which was from VAMDeluxe Breathe
    public class BreatheLite
    {
        
        private float breatheCycle = 0.0f;
        private float breatheDuration = 1.0f;
        private float breathePower = 1.0f;
        private int breatheIndex = 0;
        private BreathEntry breatheEntry;
        private bool breatheNeedInit = true;
        private DAZMorph stomachMorph;
        private FreeControllerV3 chestController;
        
        private struct BreathEntry
        {
            public float breatheOut;
            public float holdOut;
            public float breatheIn;
            public float holdInReference;
            public bool noseIn;

            public BreathEntry(float breatheOut, float holdOut, float breatheIn, float holdIn, bool noseIn)
            {
                this.breatheOut = breatheOut;
                this.holdOut = holdOut;
                this.breatheIn = breatheIn;
                this.holdInReference = holdIn - breatheIn;
                this.noseIn = noseIn;
            }
        }

        private BreathEntry[] breathEntries = new BreathEntry[] {
            new BreathEntry(0.372f, 0.509f, 0.850f, 1.227f, false),
            new BreathEntry(0.391f, 0.524f, 0.800f, 0.977f, false),
            new BreathEntry(0.624f, 1.199f, 2.183f, 2.350f, true ),
            new BreathEntry(0.266f, 0.400f, 0.650f, 0.878f, false),
            new BreathEntry(0.449f, 0.604f, 0.850f, 1.159f, false),
            new BreathEntry(0.435f, 0.597f, 1.100f, 1.389f, false),
            new BreathEntry(0.343f, 0.697f, 1.106f, 1.358f, false),
            new BreathEntry(0.497f, 0.905f, 1.202f, 1.439f, false),
            new BreathEntry(0.325f, 0.545f, 0.833f, 0.979f, false),
            new BreathEntry(0.304f, 0.516f, 0.848f, 1.152f, false),
            new BreathEntry(0.307f, 0.447f, 0.756f, 0.900f, true ),
            new BreathEntry(0.612f, 0.835f, 1.122f, 1.250f, false),
            new BreathEntry(0.800f, 1.070f, 1.539f, 1.793f, false),
            new BreathEntry(0.614f, 0.812f, 1.191f, 1.558f, false),
            new BreathEntry(0.355f, 0.455f, 0.909f, 1.348f, false),
            new BreathEntry(0.421f, 0.557f, 0.817f, 1.045f, false),
            new BreathEntry(0.391f, 0.435f, 0.668f, 1.387f, false),
            new BreathEntry(0.371f, 0.443f, 0.809f, 1.561f, false),
            new BreathEntry(0.379f, 0.472f, 0.780f, 1.012f, false),
            new BreathEntry(0.386f, 0.505f, 0.811f, 1.065f, false),
            new BreathEntry(0.454f, 0.590f, 0.817f, 1.244f, true ),
            new BreathEntry(0.331f, 0.447f, 0.744f, 1.003f, true ),
            new BreathEntry(0.457f, 0.596f, 0.927f, 1.161f, true ),
            new BreathEntry(0.294f, 0.454f, 0.773f, 0.927f, false),
            new BreathEntry(0.600f, 0.679f, 1.160f, 1.640f, true ),
            new BreathEntry(0.444f, 0.515f, 0.816f, 1.039f, false),
            new BreathEntry(0.427f, 0.478f, 0.689f, 0.987f, false),
            new BreathEntry(0.299f, 0.325f, 0.635f, 0.877f, false),
            new BreathEntry(0.266f, 0.334f, 0.685f, 1.142f, false),
            new BreathEntry(0.414f, 0.487f, 0.799f, 1.040f, false),
            new BreathEntry(0.303f, 0.340f, 0.495f, 0.892f, false),
            new BreathEntry(0.652f, 0.738f, 0.985f, 1.462f, false),
            new BreathEntry(0.436f, 0.495f, 0.747f, 1.208f, false),
            new BreathEntry(0.384f, 0.525f, 0.990f, 1.538f, false),
            new BreathEntry(0.482f, 0.546f, 0.927f, 1.100f, true ),
            new BreathEntry(0.381f, 0.432f, 0.909f, 0.991f, true ),
            new BreathEntry(0.441f, 0.467f, 0.743f, 0.852f, false),
            new BreathEntry(0.320f, 0.376f, 0.712f, 0.900f, true ),
            new BreathEntry(0.429f, 0.558f, 0.907f, 1.250f, false),
            new BreathEntry(0.326f, 0.371f, 0.595f, 0.827f, false)
        };

        public void UpdateBreathing(float speed)
        {
            breatheCycle += Time.deltaTime * speed;
            float breatheIntensity = 0; //0, 1
            float stomachPower = 0.3f; //0, 1
            float chestDriveMin = -3.0f; //-20, 20
            float chestDriveMax = 20.0f; //-20, 20
            float chestSpring = 90.0f; //0, 250

            if (breatheCycle >= breatheDuration)
            {
                breathePower = UnityEngine.Random.Range(0.8f, 1.2f);
                float v = 0.2f * (breathEntries.Length - 1);
                float min = Mathf.Max(0 - v - 1.0f, -0.5f);
                float max = Mathf.Min(v + 1.0f, breathEntries.Length - 0.5f);
                if (min < breatheIndex && max > breatheIndex)
                    max -= 1.0f;
                int index = Mathf.RoundToInt(UnityEngine.Random.Range(min, max));
                index = Mathf.Clamp(index, 0, breathEntries.Length - 2);
                breatheIndex = index < breatheIndex ? index : index + 1;
                breatheEntry = breathEntries[breatheIndex];
                breatheDuration = breatheEntry.breatheIn + UnityEngine.Random.Range(breatheEntry.holdInReference * 0.9f, breatheEntry.holdInReference * 1.1f);
                breatheCycle = 0.0f;
                breatheNeedInit = false;
            }

            if (stomachMorph != null)
            {
                float power = breatheIntensity * 0.7f + 0.3f;
                power *= breathePower;
                float t = 1.0f - BlendOutIn(0.0f, 0.0f, 0.0f, 0.0f);
                float max = stomachPower * power;
                stomachMorph.morphValue = Mathf.SmoothStep(0.3f, -max, t);
            }

            if (chestController != null)
            {
                float power = breatheIntensity * 0.5f + 0.5f;
                power *= breathePower;
                float t = 1.0f - BlendOutIn(0.0f, 0.0f, 0.0f, 0.0f);
                float max = chestDriveMin + power * (chestDriveMax - chestDriveMin);
                float target = Mathf.SmoothStep(chestDriveMin, max, t);
                chestController.jointRotationDriveXTarget = target;
                chestController.jointRotationDriveSpring = chestSpring;
            }
        }

        public void InitBreathing(Atom containingAtom, DAZMorph theStomachMorph)
        {
            try
            {
                stomachMorph = theStomachMorph;

                chestController = containingAtom.GetStorableByID("chestControl") as FreeControllerV3;

                if (chestController != null)
                {
                    chestController.jointRotationDriveSpring = 90.0f;
                    chestController.jointRotationDriveDamper = 1.0f;
                }


                breatheNeedInit = true;

                float v = 0.2f * (breathEntries.Length - 1);
                float min = Mathf.Max(0 - v - 1.0f, -0.5f);
                float max = Mathf.Min(v + 1.0f, breathEntries.Length - 0.5f);
                int index = Mathf.RoundToInt(UnityEngine.Random.Range(min, max));
                breatheIndex = Mathf.Clamp(index, 0, breathEntries.Length - 1);

                breatheCycle = 0.0f;
                breathePower = UnityEngine.Random.Range(0.8f, 1.2f);
                breatheDuration = UnityEngine.Random.Range(0.0f, 1.5f);
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e + " (May be related to: Easy Sounds must be loaded on a female Person atom."); }
        }

        private float BlendOutIn(float st, float bo, float ho, float bi)
        {
            if (breatheNeedInit)
                return 0.0f;

            bo += breatheEntry.breatheOut;
            ho += breatheEntry.holdOut;
            bi += breatheEntry.breatheIn;

            float e = 0.001f;
            bo = Mathf.Clamp(bo, e, breatheDuration - e);
            ho = Mathf.Clamp(ho, e, breatheDuration - e);
            bi = Mathf.Clamp(bi, e, breatheDuration - e);
            if (bo + 3 * e > bi)
            {
                bo = (bo + bi) * 0.5f;
                bi = bo + 3 * e;
            }
            st = Mathf.Clamp(st, 0.0f, bo - e);
            ho = Mathf.Clamp(ho, bo + e, bi - e);

            return BlendInternal(st, bo, ho, bi);
        }

        private float BlendOut(float st, float bo, float duration)
        {
            if (breatheNeedInit)
                return 0.0f;

            float e = 0.001f;
            bo += breatheEntry.breatheOut;
            st = Mathf.Clamp(st, 0.0f, bo - e);

            float a, b, c, d;

            float hd = duration * 0.5f;
            a = Mathf.Clamp(st - hd, 0.0f, breatheDuration - e);
            d = Mathf.Clamp(bo + hd, a + 3 * e, breatheDuration - e);
            if (d - a < duration + 2 * e)
            {
                b = (a + d) * 0.5f - e;
                c = (a + d) * 0.5f + e;
            }
            else
            {
                b = Mathf.Clamp(st + hd, a + e, d - 2 * e);
                c = Mathf.Clamp(bo - hd, b + e, d - e);
            }
            return BlendInternal(a, b, c, d);
        }

        private float BlendIn(float ho, float bi, float duration)
        {
            if (breatheNeedInit)
                return 0.0f;

            ho += breatheEntry.holdOut;
            bi += breatheEntry.breatheIn;

            float a, b, c, d;
            float e = 0.001f;
            float hd = duration * 0.5f;
            a = Mathf.Clamp(ho - hd, 0.0f, breatheDuration - e);
            d = Mathf.Clamp(bi + hd, a + 3 * e, breatheDuration - e);
            if (d - a < duration + 2 * e)
            {
                b = (a + d) * 0.5f - e;
                c = (a + d) * 0.5f + e;
            }
            else
            {
                b = Mathf.Clamp(ho + hd, a + e, d - 2 * e);
                c = Mathf.Clamp(bi - hd, b + e, d - e);
            }
            return BlendInternal(a, b, c, d);
        }

        private float BlendInternal(float a, float b, float c, float d)
        {
            if (breatheCycle < a)
                return 0.0f;
            else if (breatheCycle < b)
                return Mathf.Clamp01((breatheCycle - a) / (b - a));
            else if (breatheCycle < c)
                return 1.0f;
            else if (breatheCycle < d)
                return 1.0f - Mathf.Clamp01((breatheCycle - c) / (d - c));
            else
                return 0.0f;
        }

    }
}