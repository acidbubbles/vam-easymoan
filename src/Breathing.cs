using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using System.Threading;
using System.Text.RegularExpressions;

//mod by geesp0t
namespace extraltodeusBreathingPlugin
{
    public class B : MVRScript
    {

        Dictionary<string, UIDynamicToggle> toggles = new Dictionary<string, UIDynamicToggle>();

        protected JSONStorableBool playingBool;
        protected JSONStorableBool speedControlledByArousal;
        protected JSONStorableFloat chestSlider;
        protected JSONStorableFloat bellySlider;
        protected JSONStorableFloat sternumSlider;
        protected JSONStorableFloat ribcageSlider;
        protected JSONStorableFloat intensitySlider;
        protected JSONStorableFloat animLengthSlider;

        private Atom person;
        private FreeControllerV3 personChestControl;
        private Transform chest;

        public float percentToOrgasm = 0;

        float inOut = 1f;
        float f = 0f;
        float varia = 0f;

        protected void Update()
        {
            person = containingAtom;
            personChestControl = (FreeControllerV3)person.GetStorableByID("chestControl");
            chest = personChestControl.transform;
            Vector3 eulerAngles = chest.transform.localEulerAngles;

            JSONStorable geometry = containingAtom.GetStorableByID("geometry");

            JSONStorableFloat morphBreath = geometry.GetFloatJSONParam("Breath1");
            JSONStorableFloat morphRibcage = geometry.GetFloatJSONParam("Ribcage Size");
            JSONStorableFloat morphSternum = geometry.GetFloatJSONParam("Sternum Depth");

            if (toggles["Play"].toggle.isOn)
            {
                if (speedControlledByArousal.val)
                {
                    animLengthSlider.SetVal(Mathf.Lerp(2.0f, 10.0f, percentToOrgasm));
                }

                f += animLengthSlider.val * Time.deltaTime * 60.0f;
                varia = (float)Math.Sin(f / 100) * intensitySlider.val;

                morphBreath.morphValue = varia * bellySlider.val;

                if (ribcageSlider.val != 0)
                    morphRibcage.morphValue += (varia * ribcageSlider.val) / 500;

                //SuperController.LogMessage("Time.deltaTime: " + Time.deltaTime + ", ribcageSlider: " + ribcageSlider.val + " , varia: " + varia + " , morphRibcage.morphValue: " + morphRibcage.morphValue);

                if (sternumSlider.val != 0)
                    morphSternum.morphValue += (varia * sternumSlider.val) / 500;

                if (chestSlider.val > 0)
                {
                    eulerAngles = chest.transform.localEulerAngles;
                    eulerAngles.x -= varia / 15 * animLengthSlider.val * chestSlider.val;
                    chest.transform.localEulerAngles = eulerAngles;
                }

                if (f > (Math.PI * 200))
                    f = 0;
            }
        }

        public override void Init()
        {
            try
            {
                #region Sliders
                JSONStorable geometry = containingAtom.GetStorableByID("geometry");

                intensitySlider = new JSONStorableFloat("Intensity", 0.3f, 0f, 1.0f, false);
                intensitySlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(intensitySlider);
                CreateSlider(intensitySlider, true);

                speedControlledByArousal = new JSONStorableBool("Speed Controlled by Arousal", true);
                speedControlledByArousal.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(speedControlledByArousal);
                CreateToggle(speedControlledByArousal, true);

                animLengthSlider = new JSONStorableFloat("Speed", 5, 0.1f, 15f, false);
                animLengthSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(animLengthSlider);
                CreateSlider(animLengthSlider, true);

                chestSlider = new JSONStorableFloat("Chest rotation intensity", 0.2f, 0f, 2f, true);
                chestSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(chestSlider);
                CreateSlider(chestSlider, true);

                bellySlider = new JSONStorableFloat("Belly size link intensity", 0.8f, -1f, 1f, false);
                bellySlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(bellySlider);
                CreateSlider(bellySlider, true);

                ribcageSlider = new JSONStorableFloat("Ribcage size link intensity", 2.5f, -5f, 5f, false);
                ribcageSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(ribcageSlider);
                CreateSlider(ribcageSlider, true);

                sternumSlider = new JSONStorableFloat("Sternum link intensity", 2.5f, -5f, 5f, false);
                sternumSlider.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(sternumSlider);
                CreateSlider(sternumSlider, true);

                JSONStorableBool playingBool = new JSONStorableBool("Play", true);
                playingBool.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(playingBool);
                toggles["Play"] = CreateToggle((playingBool), true);
                #endregion
            }

            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }
    }
}
