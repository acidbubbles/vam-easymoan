using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
namespace geesp0t
{
    public class EasyMoanCycleForce : MVRScript
    {
        protected bool logMessages = false;
        public bool touchingVag = false;
        public bool orgasming = false;
        public float percentToOrgasm = 0;

        //control a cycleforce

        protected JSONStorableString explanationString;
        public JSONStorableBool cycleForceCreatesVagTouch;
        protected JSONStorableBool cycleForceRequiresVagTouch;
        protected JSONStorableBool cycleForceFactor;
        protected JSONStorableFloat cycleForceFactorMin;
        protected JSONStorableFloat cycleForceFactorMax;
        protected JSONStorableBool cycleForceQuickness;
        protected JSONStorableFloat cycleForceQuicknessMin;
        protected JSONStorableFloat cycleForceQuicknessMax;
        protected JSONStorableBool cycleForcePeriod;
        protected JSONStorableFloat cycleForcePeriodMin;
        protected JSONStorableFloat cycleForcePeriodMax;
        protected JSONStorableBool cycleForceRatio;
        protected JSONStorableFloat cycleForceRatioMin;
        protected JSONStorableFloat cycleForceRatioMax;
        private JSONStorableStringChooser _targetCycleForceAtomChooser;
        private UIDynamicPopup _chooseCycleForceAtomPopup;
        private FreeControllerV3 _cycleForceAtomController;
        private CycleForceProducerV2 _targetCycleForce;
        protected Atom createdCycleForceAtom = null;
        protected String cycleForceNamePrefix = "CycleForce_EM_";
        protected String createCycleForceNamed = "";
        protected String optimalCycleForce = "";

        public bool createdCycleForce = false;

        public override void Init()
        {
            try
            {
                createdCycleForce = false;
                createCycleForceNamed = cycleForceNamePrefix + containingAtom.name;
                CreateButton("Create / Refresh EM Cycle Force").button.onClick.AddListener(
                    () => CreateCycleForceIfNeeded());

                _targetCycleForceAtomChooser = new JSONStorableStringChooser("cycSourceTargetCycleForceAtom",
                    GetTargetCycleForceAtomChoices(), "", "Target Force Producer",
                    (name) =>
                    {
                        _cycleForceAtomController = null;
                        _targetCycleForce = null;

                        if (string.IsNullOrEmpty(name))
                        {
                            return;
                        }

                        var atom = SuperController.singleton.GetAtomByUid(name);
                        if (atom && atom.forceProducers.Length > 0)
                        {
                            if (atom.freeControllers.Length > 0) _cycleForceAtomController = atom.freeControllers[0];
                            _targetCycleForce = atom.GetComponentInChildren<CycleForceProducerV2>();
                        }
                    });
                RegisterStringChooser(_targetCycleForceAtomChooser);
                if (string.IsNullOrEmpty(_targetCycleForceAtomChooser.val))
                {
                    _targetCycleForceAtomChooser.SetVal(_targetCycleForceAtomChooser.choices[0]);
                }
                _chooseCycleForceAtomPopup = CreateScrollablePopup(_targetCycleForceAtomChooser);
                _chooseCycleForceAtomPopup.popup.onOpenPopupHandlers += () =>
                {
                    _targetCycleForceAtomChooser.choices = GetTargetCycleForceAtomChoices();
                };
                cycleForceCreatesVagTouch = new JSONStorableBool("Always Moan if Cycle Force On", true);
                CreateToggle(cycleForceCreatesVagTouch);
                RegisterBool(cycleForceCreatesVagTouch);
                cycleForceCreatesVagTouch.storeType = JSONStorableParam.StoreType.Full;

                cycleForceRequiresVagTouch = new JSONStorableBool("Cycle Force Requires Vag Touch", false);
                CreateToggle(cycleForceRequiresVagTouch);
                RegisterBool(cycleForceRequiresVagTouch);
                cycleForceRequiresVagTouch.storeType = JSONStorableParam.StoreType.Full;

                explanationString = new JSONStorableString("Arousal: 0%", "");
                UIDynamicTextField dtext = CreateTextField(explanationString);

                cycleForceQuickness = new JSONStorableBool("Force Quickness Linked to Arousal", true);
                CreateToggle(cycleForceQuickness);
                RegisterBool(cycleForceQuickness);
                cycleForceQuickness.storeType = JSONStorableParam.StoreType.Full;

                float forceQuicknessDefault = 0.35f;
                if (IS_AUTOMATE_VERSION.IS_AUTOMATE) forceQuicknessDefault = 1.5f;
                cycleForceQuicknessMin = new JSONStorableFloat("Force Quickness Min", forceQuicknessDefault, 0f, 10f, false);
                RegisterFloat(cycleForceQuicknessMin);
                cycleForceQuicknessMin.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForceQuicknessMin);

                cycleForceQuicknessMax = new JSONStorableFloat("Force Quickness Max", 5f, 0f, 10f, false);
                RegisterFloat(cycleForceQuicknessMax);
                cycleForceQuicknessMax.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForceQuicknessMax);

                cycleForcePeriod = new JSONStorableBool("Force Period Linked to Arousal", true);
                CreateToggle(cycleForcePeriod, true);
                RegisterBool(cycleForcePeriod);
                cycleForcePeriod.storeType = JSONStorableParam.StoreType.Full;

                float cycleForceDefault = 6.0f;
                if (IS_AUTOMATE_VERSION.IS_AUTOMATE) cycleForceDefault = 1.5f;
                cycleForcePeriodMin = new JSONStorableFloat("Force Period Min", cycleForceDefault, 0.1f, 10f, false);
                RegisterFloat(cycleForcePeriodMin);
                cycleForcePeriodMin.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForcePeriodMin, true);

                cycleForcePeriodMax = new JSONStorableFloat("Force Period Max", 0.5f, 0.1f, 10f, false);
                RegisterFloat(cycleForcePeriodMax);
                cycleForcePeriodMax.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForcePeriodMax, true);

                cycleForceFactor = new JSONStorableBool("Force Factor Linked to Arousal", true);
                CreateToggle(cycleForceFactor, true);
                RegisterBool(cycleForceFactor);
                cycleForceFactor.storeType = JSONStorableParam.StoreType.Full;

                float forceFactorDefault = 30.0f;
                if (IS_AUTOMATE_VERSION.IS_AUTOMATE) forceFactorDefault = 200.0f;
                cycleForceFactorMin = new JSONStorableFloat("Force Factor Min", forceFactorDefault, 0f, 500f, false);
                RegisterFloat(cycleForceFactorMin);
                cycleForceFactorMin.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForceFactorMin, true);

                cycleForceFactorMax = new JSONStorableFloat("Force Factor Max", 400f, 0f, 500f, false);
                RegisterFloat(cycleForceFactorMax);
                cycleForceFactorMax.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForceFactorMax, true);

                cycleForceRatio = new JSONStorableBool("Force Ratio Linked to Arousal", true);
                CreateToggle(cycleForceRatio, true);
                RegisterBool(cycleForceRatio);
                cycleForceRatio.storeType = JSONStorableParam.StoreType.Full;

                cycleForceRatioMin = new JSONStorableFloat("Force Ratio Min", 0.5f, 0f, 1.0f, false);
                RegisterFloat(cycleForceRatioMin);
                cycleForceRatioMin.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForceRatioMin, true);

                cycleForceRatioMax = new JSONStorableFloat("Force Ratio Max", 0.35f, 0f, 1.0f, false);
                RegisterFloat(cycleForceRatioMax);
                cycleForceRatioMax.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(cycleForceRatioMax, true);
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public bool HasCycleForce()
        {
            createdCycleForceAtom = SuperController.singleton.GetAtomByUid(createCycleForceNamed);
            if (createdCycleForceAtom != null)
            {
                _targetCycleForce = createdCycleForceAtom.GetComponentInChildren<CycleForceProducerV2>();
                if (_targetCycleForce != null)
                {
                    if (_targetCycleForce.on)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void CreateCycleForceIfNeeded()
        {
            createdCycleForceAtom = SuperController.singleton.GetAtomByUid(createCycleForceNamed);
            if (createdCycleForceAtom == null) { 
                StartCoroutine(CreateCycleForce());
            } else
            {
                optimalCycleForce = createCycleForceNamed;
                RefreshForceProducers();

                _targetCycleForce.on = true;

                _targetCycleForce.periodRatio = 0.25f;
                RotateCreatedCycleForceToHip();
            }
        }

        private IEnumerator CreateCycleForce()
        {
            yield return SuperController.singleton.AddAtomByType("CycleForce", createCycleForceNamed);
            if (logMessages) SuperController.LogMessage("created " + createCycleForceNamed);

            createdCycleForceAtom = SuperController.singleton.GetAtomByUid(createCycleForceNamed);


            //now select the cycle force
            optimalCycleForce = createCycleForceNamed;
            RefreshForceProducers();

            _targetCycleForce.periodRatio = 0.25f;

            RotateCreatedCycleForceToHip();
        }

        public void RemoveCycleForce() { 
            if (_targetCycleForce != null)
            {
                _targetCycleForce.on = false;
            }
        }

        private void RotateCreatedCycleForceToHip()
        {
            createdCycleForce = true;

            //set rotation to equal hip rotation, then rotate z an extra 90 degrees
            Rigidbody hip = containingAtom.rigidbodies.First(rb => rb.name == "hip");
            if (logMessages) SuperController.LogMessage(containingAtom.name + " hip rotation: " + hip.rotation.eulerAngles.ToString());

            createdCycleForceAtom.transform.position = hip.transform.position;

            createdCycleForceAtom.transform.rotation = hip.rotation;
            if (logMessages) SuperController.LogMessage(containingAtom.name + " cycle force rotation after match to hip: " + createdCycleForceAtom.transform.eulerAngles.ToString());

            createdCycleForceAtom.transform.Rotate(0, 0, 90.0f);
            if (logMessages) SuperController.LogMessage(containingAtom.name + " cycle force rotation after 90 degree z: " + createdCycleForceAtom.transform.eulerAngles.ToString());

            //link it to the abdomen
            Rigidbody abdomenRB = containingAtom.rigidbodies.First(rb => rb.name == "abdomenControl");
            if (abdomenRB != null)
            {
                createdCycleForceAtom.freeControllers[0].SelectLinkToRigidbody(abdomenRB);
            }

            foreach (ForceReceiver fr in containingAtom.forceReceivers)
            {
                if (fr.name == "hip") _targetCycleForce.receiver = fr;
            }
        }

        public void RefreshForceProducers()
        {
            _targetCycleForceAtomChooser.choices = GetTargetCycleForceAtomChoices();

            if (optimalCycleForce == "")
            {
                _targetCycleForceAtomChooser.SetVal(_targetCycleForceAtomChooser.choices[0]);
            } else
            {
                _targetCycleForceAtomChooser.SetVal(optimalCycleForce);
            }
        }

        private List<string> GetTargetCycleForceAtomChoices()
        {

            List<string> result = new List<string>();
            foreach (var uid in SuperController.singleton.GetAtomUIDs())
            {
                var atom = SuperController.singleton.GetAtomByUid(uid);
                if (atom.forceProducers.Length > 0)
                {
                    CycleForceProducerV2 cycleForceProducerV2 = atom.GetComponentInChildren<CycleForceProducerV2>();
                    if (cycleForceProducerV2 != null)
                    {
                        result.Add(uid);
                        if (cycleForceProducerV2.receiver != null) { 
                            if (optimalCycleForce == "" && (cycleForceProducerV2.receiver.name == "hip" || cycleForceProducerV2.receiver.name == "pelvis"))
                            {
                                optimalCycleForce = uid;
                            }
                        }
                    }
                }
            }
            result.Add("None");

            return result;
        }

        public void Start()
        {
            try
            {

            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

       

        public void Update()
        {
            try
            {
                explanationString.val = string.Format("Arousal: {0:P}", percentToOrgasm);

                if (_targetCycleForce != null)
                {
                    if (cycleForceQuickness.val) _targetCycleForce.forceQuickness = Mathf.Lerp(cycleForceQuicknessMin.val, cycleForceQuicknessMax.val, percentToOrgasm);
                    if (cycleForcePeriod.val) _targetCycleForce.period = Mathf.Lerp(cycleForcePeriodMin.val, cycleForcePeriodMax.val, Mathf.Sqrt(percentToOrgasm));
                    if (cycleForceFactor.val) _targetCycleForce.forceFactor = Mathf.Lerp(cycleForceFactorMin.val, cycleForceFactorMax.val, percentToOrgasm * percentToOrgasm * percentToOrgasm);
                    if (cycleForceRatio.val) _targetCycleForce.periodRatio = Mathf.Lerp(cycleForceRatioMin.val, cycleForceRatioMax.val, percentToOrgasm);
                    
                    if (cycleForceRequiresVagTouch.val)
                    {
                        if (!_targetCycleForce.enabled)
                        {
                            if (touchingVag || orgasming)
                            {
                                _targetCycleForce.enabled = true;
                            }
                        }
                        else
                        {
                            if (!touchingVag && !orgasming)
                            {
                                _targetCycleForce.enabled = false;
                            }
                        }
                    } else if (!_targetCycleForce.enabled)
                    {
                        _targetCycleForce.enabled = true;
                    }
                }
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }


        void OnDestroy()
        {
            try
            {
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }

    }
}