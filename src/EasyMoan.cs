using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace geesp0t
{
    public class EasyMoan : MVRScript
    {
        protected bool logMessages = false;
        protected bool logTriggerMessages = false;

        protected JSONStorableBool soundOn;
        protected JSONStorableBool facialExpressionsOn;
        protected JSONStorableBool breathingOn;
        protected JSONStorableBool gazeOn;
        protected JSONStorableBool buttonsOn;
        protected JSONStorableBool genitalMorphsOn;

        protected JSONStorableFloat penisAtomRotation;
        protected JSONStorableFloat penisAtomInsertionAmount;
        protected JSONStorableFloat penisAtomThrustSpeed;
        protected JSONStorableFloat penisAtomThrustAmount;

        protected JSONStorableBool usingDildo;

        protected JSONStorableFloat morphPowerPercent;
        protected JSONStorableFloat morphPowerDuringOrgasm;
        protected JSONStorableFloat timeBetweenMoans;
        protected JSONStorableFloat randomExtraTimePercent;
        protected JSONStorableFloat randomExtraTimePercentNearOrgasm;
        protected JSONStorableFloat percentToOrgasmFloat;

        protected JSONStorableFloat endBlowJobSpeed;
        protected JSONStorableFloat stimulationToOrgasm;

        protected JSONStorableFloat headMinDistance;
        protected JSONStorableFloat pitchShift;

        protected JSONStorableString explanationString;

        protected JSONStorableBool resetExpressionsOnLoad;
        protected UIDynamicToggle resetExpressionsToggle;

        private JSONStorableStringChooser targetAtomChooser;
        private UIDynamicPopup targetAtomPopup;
        private JSONStorableStringChooser targetAtomControllerChooser;
        private UIDynamicPopup targetAtomControllerPopup;

        private UIDynamicButton createCycleForceButton;

        protected float lastHeadMinDistance = 0;

        protected bool _loaded = false;
        protected JSONStorable headAudio;
        protected AudioSource headAudioSource;
        protected FreeControllerV3 _thisAtomFC;

        protected Rigidbody lipTrigger;
        protected bool lipTouching = false;

        protected Rigidbody lBreastTrigger;
        protected bool lBreastTouching = false;

        protected Rigidbody rBreastTrigger;
        protected bool rBreastTouching = false;

        protected Rigidbody labiaTrigger;
        protected bool labiaTouching = false;

        protected Rigidbody vagTrigger;
        protected bool vagTouching = false;

        protected Rigidbody deepVagTrigger;
        protected bool deepVagTouching = false;

        protected Rigidbody mouthTrigger;
        protected bool mouthTouching = false;

        protected Rigidbody throatTrigger;
        protected bool throatTouching = false;

        protected float vagTouchTime = 0;
        protected float vagTouchLastTime = 0;
        protected float foreplayTouchLastTime = 0;

        protected bool wasThroat = false;
        protected float lastDesiredThroatSound = 0;

        protected float lastMoanTime = 0;
        protected float extraRandomTimeBetweenMoans = 0;

        protected NamedAudioClip audioClip;

        protected GazeLite gazeLite;
        private Atom gazeTargetAtom = null;
        private FreeControllerV3 gazeTargetFreeController = null;

        //plays morphs better when PlaySound is called from update, otherwise it breaks for some reason
        protected NamedAudioClip soundToPlay = null;

        Rigidbody hipRB = null;
        Rigidbody deeperVagTrigger = null;

        //VAM LAUNCH FOR DILDO MOVEMENT
        private string _scriptConnectionAtomName = "VAMLaunchScriptConnection";
        private Atom _scriptConnectionAtom = null; //connect to VAMLaunch
        private Transform _scriptConnectionTransform = null;
        private float vamLaunchDirection = 0;

        bool wasLoading = true;

        public class SoundWithMorph
        {
            public NamedAudioClip sound;
            public JSONStorableFloat morph;

            public SoundWithMorph(NamedAudioClip theSound, JSONStorableFloat theMorph)
            {
                sound = theSound;
                morph = theMorph;
            }
        }

        public class SoundWithMorphTableEntry
        {
            public string soundName;
            public string morphName;
            public bool isOrgasmSound = false;
            public JSONStorableFloat morph = null;

            public SoundWithMorphTableEntry(string theSoundName, string theMorphName, bool theIsOrgasmSound = false)
            {
                soundName = theSoundName;
                morphName = theMorphName;
                isOrgasmSound = theIsOrgasmSound;
            }
        }

        protected List<SoundWithMorphTableEntry> soundWithMorphTable = new List<SoundWithMorphTableEntry>();

        protected List<NamedAudioClip> breastTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> lipTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> labiaTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> vagTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> deepVagTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> mouthTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> throatTouchAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> orgasmAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> longOrgasmAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> extendedOrgasmAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> coolDownAudioClips = new List<NamedAudioClip>();

        protected JSONStorableFloat JSONStorableFloat;

        protected JSONStorableFloat currentMorph;
        protected JSONStorableFloat desiredMorph;
        protected bool desiredMorphReady = false;

        protected float morphActivateTime = 0.05f;
        protected float morphHoldTime = 0; //set dynamically
        protected float morphDeactivateTime = 3.0f;
        protected float morphTime = 0;
        protected float morphHoldOscVal = 0;
        protected bool morphChanging = false;

        protected bool orgasming = false;
        protected int orgasmStep = 0;
        protected float orgasmStartTime = 0;
        protected int lastOrgasmStep = 2;
        protected bool orgasmAgain = false;
        protected float percentToOrgasm = 0;

        protected int soundTesterIndex = 0;
        protected int soundTesterCategory = 0;

        protected bool wasVAMAutoBlink = false;

        //DILDO
        protected Atom createdDildoAtom = null;
        protected String dildoNamePrefix = "Dildo_EM_";
        protected String createDildoNamed = "";
        FreeControllerV3 dildoController = null;
        protected float dildoCycle = 0;
        protected float insertDildo = 0;
        protected float removeDildo = 0;
        protected bool setDildoPositions = false;
        protected Vector3 dildoStartPosition;
        protected Vector3 dildoTargetPosition;
        protected float lastDildoCyclePercent = 0;

        static float reinsertTime = 1.0f;
        protected float reinsertTimer = reinsertTime;
        int currentTargetRBIndex = 0;
        Rigidbody currentTargetRB = null;
        float lastThrustAmount;

        float extraInsertAmount = 0;
        float insertVertOffset = 0;

        bool resetVaginaMorphs = false;
        protected JSONStorableFloat labiaMinoraLowL;
        protected JSONStorableFloat labiaMinoraLowR;
        protected JSONStorableFloat labiaMajoraLowL;
        protected JSONStorableFloat labiaMajoraLowR;
        protected JSONStorableFloat anusOpenOut;
        protected JSONStorableFloat anusPushPull;
        protected JSONStorableFloat vaginaExpansion;

        float labiaMinoraLowL_start = 0;
        float labiaMinoraLowR_start = 0;
        float labiaMajoraLowL_start = 0;
        float labiaMajoraLowR_start = 0;
        float anusOpenOut_start = 0;
        float anusPushPull_start = 0;
        float vaginaExpansion_start = 0;

        float labiaMinoraLowL_max = 0;
        float labiaMinoraLowR_max = 0;
        float labiaMajoraLowL_max = 0;
        float labiaMajoraLowR_max = 0;
        float anusOpenOut_max = 0;
        float anusPushPull_max = 0;
        float vaginaExpansion_max = 0;

        private float lastThrustSpeed = 0.5f;

        //PLUGINS
        protected EasyMoanCycleForce easyMoanCycleForce = null;

        //BREATHING PLUGIN, OR BREATHELITE
        //protected extraltodeusBreathingPlugin.B breathing = null;
        protected BreatheLite breatheLite;

        //MENU UI
        protected MainUIButtons mainUIButtons = null;

        bool useDeepVagTarget = false;

        public static T FindInPlugin<T>(MVRScript self) where T : MVRScript // thanks mcgruber
        {
            int i = self.name.IndexOf('_');
            if (i < 0)
                return null;
            string prefix = self.name.Substring(0, i + 1);
            string scriptName = prefix + typeof(T).FullName;
            return self.containingAtom.GetStorableByID(scriptName) as T;
        }


        public override void Init()
        {
            try
            {
                createCycleForceButton = CreateButton("Create / Refresh Cycle Force Thrust");

                createDildoNamed = dildoNamePrefix + containingAtom.name;
                CreateButton("Create / Align Penis Atom", true).button.onClick.AddListener(
                    () => ButtonCreateDildoIfNeeded());


                penisAtomRotation = new JSONStorableFloat("Penis Atom Rotation", 0.0f, 0.0f, 360.0f, true);
                penisAtomRotation.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(penisAtomRotation);
                CreateSlider(penisAtomRotation, true);


                penisAtomInsertionAmount = new JSONStorableFloat("Penis Atom Insertion Amount", 1.4f, 0.0f, 2.0f, false);
                penisAtomInsertionAmount.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(penisAtomInsertionAmount);
                CreateSlider(penisAtomInsertionAmount, true);

                penisAtomThrustSpeed = new JSONStorableFloat("Penis Atom Thrust Speed (0 for manual)", 0.5f, 0.0f, 1.5f, false);
                penisAtomThrustSpeed.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(penisAtomThrustSpeed);
                CreateSlider(penisAtomThrustSpeed, true);

                penisAtomThrustAmount = new JSONStorableFloat("Penis Atom Thrust Amount", 0.12f, 0.0f, 0.2f, false);
                penisAtomThrustAmount.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(penisAtomThrustAmount);
                CreateSlider(penisAtomThrustAmount, true);

                usingDildo = new JSONStorableBool("Using Dildo", false);
                usingDildo.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(usingDildo);

                //default value is random
                float defaultPitchShift = 1.0f + (UnityEngine.Random.value * 0.2f - 0.1f);
                pitchShift = new JSONStorableFloat("Voice Pitch", defaultPitchShift, 0.7f, 1.3f, true);
                pitchShift.storeType = JSONStorableParam.StoreType.Full;
                RegisterFloat(pitchShift);
                CreateSlider(pitchShift);

                soundOn = new JSONStorableBool("Sound On", true);
                CreateToggle(soundOn);
                RegisterBool(soundOn);
                soundOn.storeType = JSONStorableParam.StoreType.Full;

                facialExpressionsOn = new JSONStorableBool("Facial Expressions On", true);
                CreateToggle(facialExpressionsOn);
                RegisterBool(facialExpressionsOn);
                facialExpressionsOn.storeType = JSONStorableParam.StoreType.Full;

                //if using breatheLight
                breathingOn = new JSONStorableBool("Breathing On", !IS_AUTOMATE_VERSION.IS_AUTOMATE);
                CreateToggle(breathingOn);
                RegisterBool(breathingOn);
                breathingOn.storeType = JSONStorableParam.StoreType.Full;

                gazeOn = new JSONStorableBool("Gaze On", false);
                CreateToggle(gazeOn);
                RegisterBool(gazeOn);
                gazeOn.storeType = JSONStorableParam.StoreType.Full;

                buttonsOn = new JSONStorableBool("HUD Buttons On", false);
                CreateToggle(buttonsOn);
                RegisterBool(buttonsOn);
                buttonsOn.storeType = JSONStorableParam.StoreType.Full;
                buttonsOn.setCallbackFunction = (bool val) => {
                    if(val)
                    {
                        mainUIButtons.Start();
                        if (easyMoanCycleForce != null)
                        {
                            if (easyMoanCycleForce.HasCycleForce())
                            {
                                mainUIButtons.hasCycleForce = true;
                                mainUIButtons.CheckButtonNames();
                            }
                        }
                    }
                    else
                    {
                        mainUIButtons.Cleanup();
                    }
                };

                genitalMorphsOn = new JSONStorableBool("Genital Morphs On", false);
                CreateToggle(genitalMorphsOn);
                RegisterBool(genitalMorphsOn);
                genitalMorphsOn.storeType = JSONStorableParam.StoreType.Full;
                genitalMorphsOn.setCallbackFunction = (bool val) =>
                {
                    if (!val) RestoreGenitalMorphs();
                };

                bool defaultResetExpressionsOnLoad = false;
                if (IS_AUTOMATE_VERSION.IS_AUTOMATE) defaultResetExpressionsOnLoad = true;
                resetExpressionsOnLoad = new JSONStorableBool("Reset Expressions on Load", defaultResetExpressionsOnLoad);
                resetExpressionsToggle = CreateToggle(resetExpressionsOnLoad, true);
                RegisterBool(resetExpressionsOnLoad);
                resetExpressionsOnLoad.storeType = JSONStorableParam.StoreType.Full;

                targetAtomChooser = new JSONStorableStringChooser("targetAtom",
                     SuperController.singleton.GetAtomUIDs(), "", "Gaze Target Atom", (name) =>
                     {
                         gazeTargetAtom = SuperController.singleton.GetAtomByUid(name);
                     });
                RegisterStringChooser(targetAtomChooser);
                if (string.IsNullOrEmpty(targetAtomChooser.val))
                {
                    //is there a person that isn't me?
                    if (targetAtomChooser.choices.Count > 0) {
                        string selectedItem = targetAtomChooser.choices[0];
                        foreach (Atom atom in SuperController.singleton.GetAtoms())
                        {
                            if (atom.type == "Person" && atom.name != containingAtom.name)
                            {
                                selectedItem = atom.name;
                                break;
                            }
                        }
                        targetAtomChooser.SetVal(selectedItem);
                    }
                }
                targetAtomPopup = CreateScrollablePopup(targetAtomChooser);
                targetAtomPopup.popup.onOpenPopupHandlers += () =>
                {
                    targetAtomChooser.choices = SuperController.singleton.GetAtomUIDs();
                };

                 targetAtomControllerChooser = new JSONStorableStringChooser("targetAtomController",
                   GetFreeControllersInGazeTarget(), "", "Gaze Target Atom Controller", (name) =>
                   {
                       if (gazeTargetAtom != null && gazeTargetAtom.freeControllers.Length > 0) {
                           foreach (FreeControllerV3 freeController in gazeTargetAtom.freeControllers)
                           {
                               if (freeController.name == name)
                               {
                                   gazeTargetFreeController = freeController;
                               }
                           }
                       }
                   });
                RegisterStringChooser(targetAtomControllerChooser);
                  if (string.IsNullOrEmpty(targetAtomControllerChooser.val))
                  {
                     //is there a person that isn't me?
                     if (targetAtomChooser.choices.Count > 0)
                    {
                         string selectedItem = targetAtomControllerChooser.choices[0];
                        if (gazeTargetAtom != null && gazeTargetAtom.freeControllers.Length > 0)
                        {
                            foreach (FreeControllerV3 freeController in gazeTargetAtom.freeControllers)
                            {
                                if (freeController.name == "headControl" || freeController.name == "head")
                                {
                                    selectedItem = freeController.name;
                                    break;
                                }
                            }
                        }
                        targetAtomControllerChooser.SetVal(selectedItem);
                    }
                }
                targetAtomControllerPopup = CreateScrollablePopup(targetAtomControllerChooser);
                targetAtomControllerPopup.popup.onOpenPopupHandlers += () =>
                {
                    targetAtomControllerChooser.choices = GetFreeControllersInGazeTarget();
                };

                headMinDistance = new JSONStorableFloat("Head Audio Distance (volume)", 0.8f, 0.3f, 10.0f, false);
                RegisterFloat(headMinDistance);
                headMinDistance.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(headMinDistance);

                morphPowerPercent = new JSONStorableFloat("Expression Max Power", 0.85f, 0.0f, 1.0f, false);
                RegisterFloat(morphPowerPercent);
                morphPowerPercent.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(morphPowerPercent, true);

                morphPowerDuringOrgasm = new JSONStorableFloat("Expression Max During Orgasm", 1.0f, 0.0f, 1.0f, false);
                RegisterFloat(morphPowerDuringOrgasm);
                morphPowerDuringOrgasm.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(morphPowerDuringOrgasm, true);

                timeBetweenMoans = new JSONStorableFloat("Min Time Between Moans", 1.0f, 0.0f, 20.0f, false);
                RegisterFloat(timeBetweenMoans);
                timeBetweenMoans.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(timeBetweenMoans);

                randomExtraTimePercent = new JSONStorableFloat("Random Extra Percent Between Moans", 10.0f, 0.0f, 20.0f, false);
                RegisterFloat(randomExtraTimePercent);
                randomExtraTimePercent.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(randomExtraTimePercent);

                randomExtraTimePercentNearOrgasm = new JSONStorableFloat("Percent Between Moans Near Orgasm", 1.0f, 0.0f, 20.0f, false);
                RegisterFloat(randomExtraTimePercentNearOrgasm);
                randomExtraTimePercentNearOrgasm.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(randomExtraTimePercentNearOrgasm);

                endBlowJobSpeed = new JSONStorableFloat("Finished Blowjob Delay", 0.65f, 0f, 2f, false);
                RegisterFloat(endBlowJobSpeed);
                endBlowJobSpeed.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(endBlowJobSpeed);

                explanationString = new JSONStorableString("Orgasm Percent: 0%", "");
                UIDynamicTextField dtext = CreateTextField(explanationString, true);

                stimulationToOrgasm = new JSONStorableFloat("Touch Time Till Orgasm (shortest possible time)", 120.0f, 10.0f, 240.0f, false);
                RegisterFloat(stimulationToOrgasm);
                stimulationToOrgasm.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(stimulationToOrgasm, true);

                percentToOrgasmFloat = new JSONStorableFloat("Percent to Orgasm Float Value", 0.0f, 0.0f, 1.0f, false);
                RegisterFloat(percentToOrgasmFloat);
                percentToOrgasmFloat.storeType = JSONStorableParam.StoreType.Full;

                mainUIButtons = new MainUIButtons();
                mainUIButtons.Init(this);
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void ButtonCreateDildoIfNeeded()
        {
            extraInsertAmount = 0;
            CreateDildoIfNeeded();
        }

        public void CreateDildoIfNeeded()
        {
            bool doDestroy = true;

            createdDildoAtom = SuperController.singleton.GetAtomByUid(createDildoNamed);

            if (doDestroy)
            {
                if (createdDildoAtom != null)
                {
                    createdDildoAtom.Remove();
                }

                StartCoroutine(CreateDildo());
            } else
            {
                if (createdDildoAtom == null)
                {
                    StartCoroutine(CreateDildo());
                } else
                {
                   RotateCreatedDildoToHip();
                }
            }
        }

        private IEnumerator CreateDildo()
        {
            yield return SuperController.singleton.AddAtomByType("Dildo", createDildoNamed);
            if (logMessages) SuperController.LogMessage("created " + createDildoNamed);

            createdDildoAtom = SuperController.singleton.GetAtomByUid(createDildoNamed);

            RotateCreatedDildoToHip();
        }

        public void RemoveDildo()
        {
            if (createdDildoAtom != null) {
                SuperController.singleton.RemoveAtom(createdDildoAtom);
                createdDildoAtom = null;
                dildoController = null;
            }
        }

        private void RotateCreatedDildoToHip()
        {
            if (mainUIButtons != null)
            {
                mainUIButtons.hasPenis = true;
                mainUIButtons.CheckButtonNames();
            }

            // Acidbubbles: This is not working :|
            // createdDildoAtom.LoadPreset("SELF:/Custom/Scripts/geesp0t/EasyMoan/presets/EasyMoanPenis.json");

            //set rotation to equal hip rotation, then rotate z an extra 90 degrees
            Rigidbody targetRB = containingAtom.rigidbodies.First(rb => rb.name == "VaginaTrigger");

            dildoController = createdDildoAtom.freeControllers[0];

            dildoController.transform.rotation = targetRB.transform.rotation;

            dildoController.transform.Rotate(-90.0f, -90.0f, 90.0f, Space.Self);

            Vector3 eulerRot = dildoController.transform.localRotation.eulerAngles;
            eulerRot.z = 0;
            dildoController.transform.localRotation = Quaternion.Euler(eulerRot);

            createdDildoAtom.transform.position = targetRB.transform.position;
            dildoController.transform.position = targetRB.transform.position;

            dildoTargetPosition = dildoController.transform.position;

            dildoController.transform.Translate(new Vector3(0, -0.05f, -0.35f), Space.Self);
            dildoStartPosition = dildoController.transform.position;

            insertDildo = penisAtomInsertionAmount.val + extraInsertAmount;
            removeDildo = 0;
            dildoCycle = 0;
            setDildoPositions = false;
            lastThrustSpeed = penisAtomThrustSpeed.val;
            penisAtomThrustSpeed.SetVal(0);
        }

        protected List<string> GetFreeControllersInGazeTarget()
        {
            List<string> controllers;
            if (gazeTargetAtom != null)
            {
                controllers = SuperController.singleton.GetFreeControllerNamesInAtom(gazeTargetAtom.uid);
                if (controllers.Count == 0) controllers.Add("None");
            }
            else
            {
                controllers = new List<string>();
                controllers.Add("None");
            }
            return controllers;
        }

        public void UpdateResetExpressionsOnLoad()
        {
            if (resetExpressionsOnLoad.val)
            {
                ResetFacialExpressions();
            }
        }

        public void ResetFacialExpressions()
        {
            JSONStorable js = containingAtom.GetStorableByID("geometry");
            foreach (SoundWithMorphTableEntry tableEntry in soundWithMorphTable)
            {
                tableEntry.morph = js.GetFloatJSONParam(tableEntry.morphName);
                if (tableEntry.morph != null)
                {
                    tableEntry.morph.val = 0;
                }
            }

            headAudio.CallAction("Stop");
        }

        public void Start()
        {
            try
            {

                //in case we created it then saved the scene
                createdDildoAtom = SuperController.singleton.GetAtomByUid(createDildoNamed);
                if (createdDildoAtom != null)
                {
                    RotateCreatedDildoToHip();
                } else if (usingDildo.val)
                {
                    //should be using one, but it's missing, add it
                    ButtonCreateDildoIfNeeded();
                }

                if (easyMoanCycleForce == null)
                {
                    easyMoanCycleForce = FindInPlugin<EasyMoanCycleForce>(this);
                    createCycleForceButton.button.onClick.AddListener(
                        () => easyMoanCycleForce.CreateCycleForceIfNeeded());
                }

                JSONStorable personEyelids = containingAtom.GetStorableByID("EyelidControl");
                if (personEyelids != null)
                {
                    wasVAMAutoBlink = personEyelids.GetBoolParamValue("blinkEnabled");
                }

                soundWithMorphTable.Clear();
                //LIP
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1009", "AAsex_closedAA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1010", "AAsex_wideAA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1011", "AAsex_sqntwrry2AA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1012", "AAsex_sqntwrry2AA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1013", "AAsex_sqntwrry1FF2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1026", "AAsex_sqntwrry1FF2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1027", "AAsex_closedFF2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1028", "AAsex_sqntwrry2M2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1029", "AAsex_sqntwrry1FF2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("kiss", "Mouth Resting"));

                //MOUTH
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("suckinglicking", "Enjoying It"));

                //BREAST
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1030", "AAsex_browhiFF2"));//mmm start
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1031", "AAsex_sqntwrry2sm1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1032", "AAsex_sqntwrry1FF2"));//mmm end
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1033", "AAsex_closetightFF2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1034", "AAsex_closedAA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1035", "AAsex_sqntwrry2AA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1036", "AAsex_wideAA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1037", "AAsex_wideAA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1038", "AAsex_wideAA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1039", "AAsex_sqntwrry2AA1"));

                //LABIA
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1050", "AAsex_sqntwrry2AA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1051", "AAsex_wideAA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1052", "AAsex_sqntwrry1AA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1053", "AAsex_sqntwrry1AA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1054", "AAsex_sqntwrry1AA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1055", "AAsex_sqntwrry2ER2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1056", "AAsex_sqntwrry2AA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1057", "AAsex_brraiseAA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1058", "AAsex_sqntwrry1AA1"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1059", "AAsex_sqntwrry1AA3"));

                //VAG
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1070", "Enjoying It"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1071", "Taking It"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1072", "AAsex_browhiAA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1073", "AAsex_browhiAA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1074", "AAsex_browhiOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1075", "AAsex_brraiseER2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1076", "AAsex_closedOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1077", "AAsex_sqntwrry2AA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1078", "AAsex_sqntwrry2AA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1079", "AAsex_sqntwrry2ER4"));

                //DEEP VAG
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1090", "AAsex_sqntwrry1OH4"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1091", "AAsex_squintbrER5"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1092", "AAsex_squintbrOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1093", "AAsex_wideER5"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1094", "AAsex_closetightAA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1095", "AAsex_wideOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1096", "AAsex_closedAA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1097", "AAsex_sqntwrry1ER5"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1098", "AAsex_sqntwrry1OH4"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemPixieW1099", "AAsex_squintbrER5"));

                //ORGASM (NOW PREORGASM IS USED AS THE ORGASM SOUNDS)
                //soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemMoanExaggerated", "AAsex_sqntwrry2OH9", true));

                //other options for orgasm
                //orgasmMorphs.Add("AAsex_wideOH7"); //good eyes open pleasure
                //orgasmMorphs.Add("AAsex_squintbrOH4"); //ohhh with eyes squiting
                //orgasmMorphs.Add("AAsex_closetightsm1e"); //huge pleasure smile
                //AAsex_squintbrER5  //great for something, a bit of an ahhhh
                //AAsex_closetightAA6
                //AAsex_brraiseOH8

                //ORGASM
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("Late-20s-Woman-Exaggerated", "AAsex_closetightAA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("woman-orgasm-1", "AAsex_sqntwrry1AA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemGasps2", "AAsex_sqntwrry2AA6"));

                //LONG ORGASM, REPLACES COMPLEX ORGASM STEPS WITH A SINGLE AUDIO CLIP
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemMoanSex", "AAsex_closetightAA6"));

                //POST ORGASM
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathShiver", "AAsex_closetightAA6"));
                //soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathCoolDown1", "AAsex_wideOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathCoolDown4", "AAsex_closetightAA6"));

                //find morphs
                JSONStorable js = containingAtom.GetStorableByID("geometry");
                JSONStorableFloat stomachMorph = js.GetFloatJSONParam("Breath1");
                //if using Breathing plugin
                /*if (js.GetFloatJSONParam("Breath1") == null) SuperController.LogError("Missing stomach morph for breathing");
                if (js.GetFloatJSONParam("Ribcage Size") == null) SuperController.LogError("Missing Ribcage Size morph for breathing");
                if (js.GetFloatJSONParam("Sternum Depth") == null) SuperController.LogError("Missing Sternum Depth morph for breathing");*/

                //if using BreatheLight
                if (stomachMorph == null) SuperController.LogError("Missing stomach morph");

                labiaMinoraLowL = js.GetFloatJSONParam("Labia minora-spread-LLow"); //0.3 during arousal
                labiaMinoraLowR = js.GetFloatJSONParam("Labia minora-spread-RLow");
                labiaMajoraLowL = js.GetFloatJSONParam("Labia majora-spread-LLow"); //0.3 during arousal
                labiaMajoraLowR = js.GetFloatJSONParam("Labia majora-spread-RLow");
                // From Genitalia Reloaded, which is not a free addon.
                anusOpenOut = js.GetFloatJSONParam("Anus-Open-Out"); //0 0.28 cycle during orgasm
                anusPushPull = js.GetFloatJSONParam("Anus-Push.Pull"); //-.15 .15
                vaginaExpansion = js.GetFloatJSONParam("Vagina-expansion"); //0 0.2 during arousal, up to 0.5  during orgasm

                labiaMinoraLowL_start = labiaMinoraLowL?.val ?? 0;
                labiaMinoraLowR_start = labiaMinoraLowR?.val ?? 0;
                labiaMajoraLowL_start = labiaMajoraLowL?.val ?? 0;
                labiaMajoraLowR_start = labiaMajoraLowR?.val ?? 0;
                anusOpenOut_start = anusOpenOut?.val ?? 0;
                anusPushPull_start = (anusPushPull?.val - 0.15f) ?? 0;
                vaginaExpansion_start = vaginaExpansion?.val ?? 0;

                labiaMinoraLowL_max = labiaMinoraLowL_start + 0.3f;
                labiaMinoraLowR_max = labiaMinoraLowR_start + 0.3f;
                labiaMajoraLowL_max = labiaMajoraLowL_start + 0.3f;
                labiaMajoraLowR_max = labiaMajoraLowR_start + 0.3f;
                anusOpenOut_max = anusOpenOut_start + 0.25f;
                anusPushPull_max = (anusPushPull?.val + 0.15f) ?? 0f;
                vaginaExpansion_max = vaginaExpansion_start + 0.2f;


                foreach (SoundWithMorphTableEntry tableEntry in soundWithMorphTable)
                {
                    tableEntry.morph = js.GetFloatJSONParam(tableEntry.morphName);
                    if (tableEntry.morph == null)
                    {
                        SuperController.LogError("Missing morph: " + tableEntry.morphName + ". Please add all morphs from the AshAuryn Sexpressions pack TenStrip_Expressions pack to VAM.");
                    }
                    else
                    {
                        //we may want to use one of these as a default value so don't disable them automatically
                        //tableEntry.morph.SetValue(0);
                    }
                }

                if (containingAtom.category == "People")
                {
                    _loaded = true;
                    _thisAtomFC = containingAtom.freeControllers.First(freec => freec.name == "control");

                    TriggerCollide tempCollider;

                    lipTrigger = containingAtom.rigidbodies.First(rb => rb.name == "LipTrigger");
                    tempCollider = lipTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = lipTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserveLipTrigger;

                    mouthTrigger = containingAtom.rigidbodies.First(rb => rb.name == "MouthTrigger");
                    tempCollider = mouthTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = mouthTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserveMouthTrigger;

                    throatTrigger = containingAtom.rigidbodies.First(rb => rb.name == "ThroatTrigger");
                    tempCollider = throatTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = throatTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserveThroatTrigger;

                    lBreastTrigger = containingAtom.rigidbodies.First(rb => rb.name == "lNippleTrigger");
                    tempCollider = lBreastTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = lBreastTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObservelBreastTrigger;

                    rBreastTrigger = containingAtom.rigidbodies.First(rb => rb.name == "rNippleTrigger");
                    tempCollider = rBreastTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = rBreastTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserverBreastTrigger;

                    labiaTrigger = containingAtom.rigidbodies.First(rb => rb.name == "LabiaTrigger");
                    tempCollider = labiaTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = labiaTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserveLabiaTrigger;

                    vagTrigger = containingAtom.rigidbodies.First(rb => rb.name == "VaginaTrigger");
                    tempCollider = vagTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = vagTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserveVagTrigger;

                    deepVagTrigger = containingAtom.rigidbodies.First(rb => rb.name == "DeepVaginaTrigger");
                    tempCollider = deepVagTrigger.gameObject.GetComponentInChildren<TriggerCollide>();
                    if (tempCollider == null) tempCollider = deepVagTrigger.gameObject.AddComponent<TriggerCollide>();
                    tempCollider.OnCollide += ObserveDeepVagTrigger;

                    deeperVagTrigger = containingAtom.rigidbodies.First(rb => rb.name == "DeeperVaginaTrigger");
                }
                else
                {
                    _loaded = false; SuperController.LogError("Easy Sounds must be loaded on a female Person atom.");
                }

                breastTouchAudioClips.Clear();
                lipTouchAudioClips.Clear();
                labiaTouchAudioClips.Clear();
                vagTouchAudioClips.Clear();
                deepVagTouchAudioClips.Clear();
                mouthTouchAudioClips.Clear();
                throatTouchAudioClips.Clear();
                longOrgasmAudioClips.Clear();
                orgasmAudioClips.Clear();
                extendedOrgasmAudioClips.Clear();
                coolDownAudioClips.Clear();

                headAudio = containingAtom.GetStorableByID("HeadAudioSource");
                if (soundOn.val) headAudio.SetFloatParamValue("volume", 1.0f);
                lastHeadMinDistance = headMinDistance.val;

                headAudioSource = headAudio.gameObject.GetComponentInChildren<AudioSource>();

                int i = 0;
                foreach (NamedAudioClip namedAudioClip in EmbeddedAudioClipManager.singleton.embeddedClips)
                {
                    if (namedAudioClip.category == "FemaleMoan")
                    {
                        if (namedAudioClip.displayName.StartsWith("FemPixie"))
                        {
                            i++;
                            if (i >= 9 && i < 14)
                            {
                                lipTouchAudioClips.Add(namedAudioClip);
                            }
                            else if (i >= 26 && i < 30)
                            {
                                lipTouchAudioClips.Add(namedAudioClip);
                            }
                            else if (i >= 30 && i < 40)
                            {
                                breastTouchAudioClips.Add(namedAudioClip);
                            }
                            else if (i >= 50 && i < 60)
                            {
                                labiaTouchAudioClips.Add(namedAudioClip);
                            }
                            else if (i >= 70 && i < 80)
                            {
                                vagTouchAudioClips.Add(namedAudioClip);
                            }
                            else if (i >= 90 && i < 100)
                            {
                                deepVagTouchAudioClips.Add(namedAudioClip);
                            }
                        }
                        else if (namedAudioClip.displayName.StartsWith("FemSexySigh"))
                        {
                            extendedOrgasmAudioClips.Add(namedAudioClip);
                        }
                    }
                    else if (namedAudioClip.category == "PersonOral")
                    {
                        if (namedAudioClip.displayName.StartsWith("sucking")) // || namedAudioClip.displayName.StartsWith("licking"))  LICKING SOUND HAS TOO LONG A DELAY BEFORE STARTING :(
                        {
                            mouthTouchAudioClips.Add(namedAudioClip);
                        }
                        if (namedAudioClip.displayName.StartsWith("sucking"))
                        {
                            throatTouchAudioClips.Add(namedAudioClip);
                        }
                        if (namedAudioClip.displayName.EndsWith("kiss"))
                        {
                            lipTouchAudioClips.Add(namedAudioClip);
                        }
                    }
                    else if (namedAudioClip.category == "FemaleBreath")
                    {
                        if (namedAudioClip.displayName.StartsWith("FemBreathCoolDown4"))
                        {
                            coolDownAudioClips.Add(namedAudioClip);
                        }
                        else if (namedAudioClip.displayName.StartsWith("FemBreathShiver"))
                        {
                            coolDownAudioClips.Add(namedAudioClip);
                        }
                        else if (namedAudioClip.displayName.StartsWith("FemGasps2"))
                        {
                           orgasmAudioClips.Add(namedAudioClip);
                        }
                    }
                    else if (namedAudioClip.category == "FemaleOrgasm")
                    {
                        if (namedAudioClip.displayName.StartsWith("Late-20s-Woman"))
                        {
                            orgasmAudioClips.Add(namedAudioClip);
                        }
                        else if (namedAudioClip.displayName.StartsWith("woman-orgasm-1"))
                        {
                            orgasmAudioClips.Add(namedAudioClip);
                        }
                    }
                    else if (namedAudioClip.category == "FemaleSexLong")
                    {
                        if (namedAudioClip.displayName.StartsWith("FemMoanSex"))
                        {
                            longOrgasmAudioClips.Add(namedAudioClip);
                        }
                    }
                    //if (logMessages) SuperController.LogMessage(namedAudioClip.category + " " + namedAudioClip.displayName);
                }

                resetExpressionsToggle.toggle.onValueChanged.AddListener(delegate { UpdateResetExpressionsOnLoad(); });
                if (resetExpressionsOnLoad.val) ResetFacialExpressions();

                //breathing plugin
                //if (breathing == null) breathing = FindInPlugin<extraltodeusBreathingPlugin.B>(this);

                //if using breatheLight
                breatheLite = new BreatheLite();
                breatheLite.InitBreathing(containingAtom, stomachMorph);

                gazeLite = new GazeLite();
                gazeLite.Init(containingAtom, (FreeControllerV3)containingAtom.GetStorableByID("headControl"));

                if (mainUIButtons != null && buttonsOn.val)
                {
                    mainUIButtons.Start();
                    if (easyMoanCycleForce != null)
                    {
                        if (easyMoanCycleForce.HasCycleForce())
                        {
                            mainUIButtons.hasCycleForce = true;
                            mainUIButtons.CheckButtonNames();
                        }
                    }
                }



                ResetTouching();

            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void FixedUpdate()
        {
            if (gazeOn.val)
            {
                gazeLite.FixedUpdate(gazeTargetAtom, gazeTargetFreeController);
            }
        }

        public void PlaySound(NamedAudioClip audioClip, bool duringOrgasm = false, bool useMorph = true)
        {
            try
            {
                if (orgasming && !duringOrgasm) return;

                if (soundOn.val)
                {
                    headAudioSource.pitch = pitchShift.val;
                    headAudio.CallAction("PlayNow", audioClip);
                }
                if (facialExpressionsOn.val && useMorph)
                {

                    desiredMorphReady = false;
                    foreach (SoundWithMorphTableEntry tableEntry in soundWithMorphTable)
                    {
                        if (tableEntry.soundName == audioClip.displayName)
                        {
                            morphHoldTime = audioClip.clipToPlay.length - (morphActivateTime + 0.25f);
                            if (logMessages) SuperController.LogMessage("Found sound " + audioClip.displayName + " " + " " + audioClip.category + " " + audioClip.clipToPlay.length);

                            desiredMorph = tableEntry.morph;
                            if (logMessages) SuperController.LogMessage("Desired Morph " + desiredMorph?.altName);

                            if(desiredMorph != null) desiredMorphReady = true;

                            break;
                        }
                    }

                    if (logMessages && !desiredMorphReady) SuperController.LogMessage("Missing morph for sound " + audioClip.displayName + " " + audioClip.category);
                }

               if (logMessages) SuperController.LogMessage("Playing sound " + audioClip.displayName + " morph " + desiredMorph?.altName);
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void SetLastMoanTime()
        {
            lastMoanTime = Time.timeSinceLevelLoad;
            float adjustedExtraTimePercent = Mathf.Lerp(randomExtraTimePercent.val, randomExtraTimePercentNearOrgasm.val, percentToOrgasm);
            extraRandomTimeBetweenMoans = UnityEngine.Random.value * timeBetweenMoans.val * adjustedExtraTimePercent;
        }

        public void SetVAMBlinkEnabled(bool enabled)
        {
            JSONStorable personEyelids = containingAtom.GetStorableByID("EyelidControl");
            if (personEyelids != null)
            {
                personEyelids.SetBoolParamValue("blinkEnabled", enabled);
            }
        }

        public void ResetTouching()
        {
            lipTouching = false;
            lBreastTouching = false;
            rBreastTouching = false;
            labiaTouching = false;
            vagTouching = false;
            deepVagTouching = false;
            mouthTouching = false;
            throatTouching = false;
        }

        public void CheckScriptConnection()
        {
            if (_scriptConnectionAtom == null)
            {
                _scriptConnectionAtom = SuperController.singleton.GetAtomByUid(_scriptConnectionAtomName);
                if (_scriptConnectionAtom != null)
                {
                    _scriptConnectionTransform = _scriptConnectionAtom.transform;
                }
            }
        }

        public void Update()
        {
            try
            {
                //DISABLE WHEN NOT TESTING
                //TestAllSounds(); soundTesterCategory = 6;
                //to test sounds starting in a location, also put this in start() soundTesterCategory = 5; soundTesterIndex = 0;

                if (SuperController.singleton.isLoading && !wasLoading)
                {
                    wasLoading = true;
                    if (logMessages) SuperController.LogMessage("START LOADING time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                } else if (!SuperController.singleton.isLoading && wasLoading)
                {
                    wasLoading = false;

                    CheckScriptConnection();

                    if (logMessages) SuperController.LogMessage("STOP LOADING time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                    ResetTouching();
                }

                //if we set her to cycle, and the cycle force
                if (easyMoanCycleForce != null && easyMoanCycleForce.HasCycleForce() && easyMoanCycleForce.cycleForceCreatesVagTouch.val)
                    SimVagTrigger();

                if (dildoController != null)
                {
                    //won't be stimulated if a static penis is there and she is not thrusting
                    if (removeDildo <= 0 && insertDildo <= 0 && !orgasming && !vagTouching && Time.timeSinceLevelLoad - vagTouchLastTime > 7.0f)
                    {
                        if (penisAtomThrustSpeed.val <= 0 && (easyMoanCycleForce != null && !easyMoanCycleForce.HasCycleForce()))
                        {
                            //won't be stimulated if a static penis is there and she is not thrusting
                            //so don't try to reinsert
                        }
                        else {
                            //want it, try to rotate towards a likely target

                            reinsertTimer -= Time.deltaTime;
                            if (reinsertTimer < 0)
                            {
                                reinsertTimer = reinsertTime;

                                removeDildo = penisAtomInsertionAmount.val;

                                useDeepVagTarget = !useDeepVagTarget;
                            }
                        }
                    }

                    if (removeDildo > 0)
                    {
                        removeDildo -= Time.deltaTime;
                        dildoController.transform.Translate(0.0f, 0.0f, Time.deltaTime * -0.10f, Space.Self);
                        if (removeDildo <= 0)
                        {
                            if (extraInsertAmount < 0.6f)
                            {
                                extraInsertAmount += 0.1f;
                            }
                            insertDildo = penisAtomInsertionAmount.val + extraInsertAmount;
                        }
                    } else if (insertDildo > 0)
                    {
                        if (useDeepVagTarget)
                        {
                            dildoController.transform.LookAt(deeperVagTrigger.transform);
                        } else
                        {
                            dildoController.transform.LookAt(vagTrigger.transform);
                        }
                        Vector3 eulerRot = dildoController.transform.rotation.eulerAngles;
                        eulerRot.z = penisAtomRotation.val;
                        dildoController.transform.rotation = Quaternion.Euler(eulerRot);

                        insertDildo -= Time.deltaTime;
                        dildoController.transform.Translate(0.0f, 0.0f, Time.deltaTime * 0.10f, Space.Self);
                        dildoCycle = 0;

                        if (insertDildo <= 0)
                        {
                            dildoStartPosition = dildoController.transform.position;
                            dildoController.transform.Translate(0.0f, 0.0f, penisAtomThrustAmount.val, Space.Self);
                            dildoTargetPosition = dildoController.transform.position;
                            dildoController.transform.Translate(0.0f, 0.0f, -penisAtomThrustAmount.val, Space.Self);
                            lastThrustAmount = penisAtomThrustAmount.val;
                            if (logMessages) SuperController.LogMessage("insertDildo sets start position: " + dildoStartPosition);
                            if (logMessages) SuperController.LogMessage("insertDildo sets target position: " + dildoTargetPosition);
                            setDildoPositions = true;
                        }
                    } else
                    {
                        dildoCycle += Time.deltaTime * penisAtomThrustSpeed.val * 20.0f;
                        float dildoCyclePercent = Mathf.Sin(dildoCycle) / 2.0f + 0.5f;
                        if (lastDildoCyclePercent > dildoCyclePercent)
                        {
                            vamLaunchDirection = -1.0f;

                        }
                        else if (lastDildoCyclePercent < dildoCyclePercent)
                        {
                            vamLaunchDirection = 1.0f;
                        }
                        else
                        {
                            vamLaunchDirection = 0f;
                        }
                        //depending on distance? we shouldn't look at something too close to us
                        float sinCycle = Mathf.Sin(dildoCycle);
                        if (dildoCyclePercent < 0.1f && dildoCyclePercent > lastDildoCyclePercent)
                        {
                            if (useDeepVagTarget)
                            {
                                float distanceToDeeperVag = Vector3.Distance(dildoController.transform.position, deeperVagTrigger.transform.position);
                                if (distanceToDeeperVag > 0.1f)
                                {
                                    dildoController.transform.LookAt(deeperVagTrigger.transform.position);
                                }
                            } else
                            {
                                float distanceToVag = Vector3.Distance(dildoController.transform.position, vagTrigger.transform.position);
                                if (distanceToVag > 0.1f)
                                {
                                    dildoController.transform.LookAt(vagTrigger.transform.position);
                                }
                            }
                        }
                        lastDildoCyclePercent = dildoCyclePercent;

                        Vector3 eulerRot = dildoController.transform.rotation.eulerAngles;
                        eulerRot.z = penisAtomRotation.val;
                        dildoController.transform.rotation = Quaternion.Euler(eulerRot);

                        if (penisAtomThrustSpeed.val > 0 && penisAtomThrustAmount.val > 0 && !SuperController.singleton.freezeAnimation)
                        {
                            if (setDildoPositions)
                            {
                                if (penisAtomThrustAmount.val != lastThrustAmount)
                                {
                                    dildoController.transform.position = dildoStartPosition;
                                    dildoController.transform.Translate(0.0f, 0.0f, penisAtomThrustAmount.val, Space.Self);
                                    dildoTargetPosition = dildoController.transform.position;
                                    dildoController.transform.Translate(0.0f, 0.0f, -penisAtomThrustAmount.val, Space.Self);
                                    lastThrustAmount = penisAtomThrustAmount.val;
                                    if (logMessages) SuperController.LogMessage("change of thrust amount sets target position: " + dildoTargetPosition);
                                }
                                dildoController.transform.position = Vector3.Lerp(dildoStartPosition, dildoTargetPosition, dildoCyclePercent);

                                if (_scriptConnectionTransform != null)
                                {
                                    _scriptConnectionTransform.position = new Vector3(vamLaunchDirection, penisAtomThrustSpeed.val, _scriptConnectionTransform.position.z);
                                }
                            }
                        }
                    }
                }

                if (soundOn.val)
                {
                    if (lastHeadMinDistance != headMinDistance.val)
                    {
                        lastHeadMinDistance = headMinDistance.val;
                        headAudio.SetFloatParamValue("minDistance", headMinDistance.val);
                        headAudio.SetFloatParamValue("maxDistance", 500.0f);
                    }
                }

                if (soundToPlay != null)
                {
                    if (logMessages) SuperController.LogMessage("playing sound on update: " + soundToPlay.displayName);
                    PlaySound(soundToPlay);
                    soundToPlay = null;
                }

                float morphPower = morphPowerPercent.val;
                if (orgasming) morphPower = morphPowerDuringOrgasm.val;

                //apply morph
                if (facialExpressionsOn.val && desiredMorphReady)
                {
                    desiredMorphReady = false;

                    if (currentMorph != null) currentMorph.val = 0;
                    currentMorph = desiredMorph;

                    if (currentMorph != null)
                    {
                        morphChanging = true;
                        morphTime = 0;
                        currentMorph.val = 0;
                    }
                    if (logMessages) SuperController.LogMessage("Desired Morph: " + desiredMorph?.altName);

                    SetVAMBlinkEnabled(false);
                }

                if (facialExpressionsOn.val && morphChanging && currentMorph != null)
                {
                    morphTime += Time.deltaTime;
                    if (morphTime < morphActivateTime)
                    {
                        currentMorph.val = Mathf.Min((morphTime / morphActivateTime) * morphPower, currentMorph.max);
                        morphHoldOscVal = 0;
                    }
                    else if (morphTime < morphActivateTime + morphHoldTime)
                    {
                        //oscillate a little during hold
                        morphHoldOscVal += Time.deltaTime;
                        currentMorph.val = Mathf.Min((1.0f - Mathf.Sin(morphHoldOscVal * 3.5f) * 0.07f) * morphPower, currentMorph.max);
                    }
                    else if (morphTime >= morphActivateTime + morphHoldTime + morphDeactivateTime)
                    {
                        currentMorph.val = 0;
                        morphChanging = false;
                        SetVAMBlinkEnabled(wasVAMAutoBlink);
                    }
                    else
                    {
                        float morphDeactivateVal = (1.0f - (morphTime - (morphActivateTime + morphHoldTime)) / morphDeactivateTime);
                        morphDeactivateVal = Mathf.Pow(morphDeactivateVal, 1.5f);
                        currentMorph.val = Mathf.Min(morphDeactivateVal * morphPower, currentMorph.max);
                    }
                }

                if (wasThroat && !throatTouching && !mouthTouching && Time.timeSinceLevelLoad - lastDesiredThroatSound > endBlowJobSpeed.val && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                {
                    wasThroat = false;
                    // a little moan on exit
                    audioClip = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    PlaySound(audioClip, false);
                    SetLastMoanTime();
                    if (logMessages) SuperController.LogMessage("STOP Throat Sound with Lip Sound");
                }

                //we entered recently, plus are touching
                if (Time.timeSinceLevelLoad - vagTouchLastTime < 1.0f && (labiaTouching || vagTouching || deepVagTouching))
                {
                    //if (logMessages) SuperController.LogMessage("Time.timeSinceLevelLoad: " + Time.timeSinceLevelLoad + ", vagTouchLastTime: " + vagTouchLastTime + ", touching labia: " + labiaTouching + ", vag: " + vagTouching + ", deep vag;" + deepVagTouching);
                    vagTouchTime += Time.deltaTime;
                    if (easyMoanCycleForce != null) easyMoanCycleForce.touchingVag = true;
                }
                else if (Time.timeSinceLevelLoad - foreplayTouchLastTime < 1.0f && (lBreastTouching || rBreastTouching || lipTouching))
                {
                    //foreplay goes half way
                    if (vagTouchTime < stimulationToOrgasm.val / 2.0f) vagTouchTime += Time.deltaTime;
                }
                else if (vagTouchTime > 0)
                {
                    vagTouchTime -= Time.deltaTime / 5.0f;
                } else
                {
                    if (easyMoanCycleForce != null) easyMoanCycleForce.touchingVag = false;
                }

                if (vagTouchTime >= stimulationToOrgasm.val)
                {
                    //ORGASM
                    if (orgasming)
                    {
                        //as soon as we finish, start again
                        orgasmAgain = true;
                    } else {
                        StartOrgasm();
                    }
                }

                if (orgasmAgain && !orgasming)
                {
                    orgasmAgain = false;
                    StartOrgasm();
                }

                if (orgasming)
                {
                    HandleOrgasm();
                }

                percentToOrgasm = vagTouchTime / stimulationToOrgasm.val;
                if (percentToOrgasm < 0) percentToOrgasm = 0;
                if (orgasming) percentToOrgasm = 1.0f;

                explanationString.val = string.Format("Orgasm Percent: {0:P}", percentToOrgasm);

                percentToOrgasmFloat.SetVal(percentToOrgasm);

                //handle vagina morphs based on arousal and orgasm
                if (genitalMorphsOn.val)
                {
                    if (orgasming)
                    {
                        float contractionPercent = Mathf.PingPong(Time.time * 2.8f, 1.0f);
                        if (anusOpenOut != null)
                            anusOpenOut.val = Mathf.Min(Mathf.Lerp(anusOpenOut_start, anusOpenOut_max, contractionPercent), anusOpenOut.max);
                        if (anusPushPull != null)
                            anusPushPull.val = Mathf.Min(Mathf.Lerp(anusPushPull_start, anusPushPull_max, contractionPercent), anusPushPull.max);
                        if (vaginaExpansion != null)
                            vaginaExpansion.val = Mathf.Min(Mathf.Lerp(vaginaExpansion_max, vaginaExpansion_max + 0.5f, contractionPercent), vaginaExpansion.max);
                        if (labiaMinoraLowL != null)
                            labiaMinoraLowL.val = Mathf.Min(Mathf.Lerp(labiaMinoraLowL_max - 0.1f, labiaMinoraLowL_max + 0.1f, contractionPercent), labiaMinoraLowL.max);
                        if (labiaMinoraLowR != null)
                            labiaMinoraLowR.val = Mathf.Min(Mathf.Lerp(labiaMinoraLowR_max - 0.1f, labiaMinoraLowL_max + 0.1f, contractionPercent), labiaMinoraLowR.max);
                        if (labiaMajoraLowL != null)
                            labiaMajoraLowL.val = Mathf.Min(Mathf.Lerp(labiaMajoraLowL_max - 0.2f, labiaMinoraLowL_max + 0.2f, contractionPercent), labiaMajoraLowL.max);
                        if (labiaMajoraLowR != null)
                            labiaMajoraLowR.val = Mathf.Min(Mathf.Lerp(labiaMajoraLowR_max - 0.2f, labiaMinoraLowL_max + 0.2f, contractionPercent), labiaMajoraLowR.max);
                    }
                    else
                    {
                        if (anusOpenOut != null)
                            anusOpenOut.val = anusOpenOut_start;
                        if (anusPushPull != null)
                            anusPushPull.val = anusPushPull_start;
                        if (labiaMinoraLowL != null)
                            labiaMinoraLowL.val = Mathf.Min(Mathf.Lerp(labiaMinoraLowL_start, labiaMinoraLowL_max, percentToOrgasm), labiaMinoraLowL.max);
                        if (labiaMinoraLowR != null)
                            labiaMinoraLowR.val = Mathf.Min(Mathf.Lerp(labiaMinoraLowR_start, labiaMinoraLowR_max, percentToOrgasm), labiaMinoraLowR.max);
                        if (labiaMajoraLowL != null)
                            labiaMajoraLowL.val = Mathf.Min(Mathf.Lerp(labiaMajoraLowL_start, labiaMajoraLowL_max, percentToOrgasm), labiaMajoraLowL.max);
                        if (labiaMajoraLowR != null)
                            labiaMajoraLowR.val = Mathf.Min(Mathf.Lerp(labiaMajoraLowR_start, labiaMajoraLowR_max, percentToOrgasm), labiaMajoraLowR.max);
                        if (vaginaExpansion != null)
                            vaginaExpansion.val = Mathf.Min(Mathf.Lerp(vaginaExpansion_start, vaginaExpansion_max, percentToOrgasm), vaginaExpansion.max);
                    }
                }

                if (easyMoanCycleForce != null) easyMoanCycleForce.percentToOrgasm = percentToOrgasm;

                //if using Breathing Plugin
                //if (breathing != null) breathing.percentToOrgasm = percentToOrgasm;

                //if using BreatheLight
                if (breathingOn.val) breatheLite.UpdateBreathing(Mathf.Lerp(0.25f, 1.5f, percentToOrgasm));

                if (mainUIButtons != null)
                {
                    if (easyMoanCycleForce.createdCycleForce)
                    {
                        easyMoanCycleForce.createdCycleForce = false;
                        mainUIButtons.hasCycleForce = true;
                        mainUIButtons.CheckButtonNames();
                    }

                    if (mainUIButtons.wantToCreateHipCycleForce)
                    {
                        mainUIButtons.wantToCreateHipCycleForce = false;
                        if (easyMoanCycleForce != null) easyMoanCycleForce.CreateCycleForceIfNeeded();
                        mainUIButtons.hasCycleForce = true;
                        //switch vam launch to use hip thrust
                        CheckScriptConnection();
                        if (_scriptConnectionTransform != null)
                        {
                            _scriptConnectionTransform.position = new Vector3(_scriptConnectionTransform.position.x, _scriptConnectionTransform.position.y, 0.1f); //0.1 means switch to easy moan cycle force
                        }
                        mainUIButtons.CheckButtonNames();
                    }

                    if (mainUIButtons.wantToCreatePenis)
                    {
                        mainUIButtons.wantToCreatePenis = false;
                        ButtonCreateDildoIfNeeded();
                        mainUIButtons.hasPenis = true;
                        mainUIButtons.CheckButtonNames();
                    }

                    if (mainUIButtons.wantToRotatePenis)
                    {
                        mainUIButtons.wantToRotatePenis = false;
                        penisAtomRotation.SetVal(penisAtomRotation.val + 45.0f);
                        if (penisAtomRotation.val >= 359.0f) penisAtomRotation.SetVal(0);
                    }

                    if (mainUIButtons.wantToAnimatePenis)
                    {
                        mainUIButtons.wantToAnimatePenis = false;
                        mainUIButtons.animatingPenis = true;
                        penisAtomThrustSpeed.SetVal(lastThrustSpeed);
                        //switch vam launch to use penis if we don't have hip thrust
                        if (easyMoanCycleForce == null || !easyMoanCycleForce.HasCycleForce())
                        {
                            CheckScriptConnection();
                            if (_scriptConnectionTransform != null)
                            {
                                _scriptConnectionTransform.position = new Vector3(_scriptConnectionTransform.position.x, _scriptConnectionTransform.position.y, 0.3f); //0.3 means switch to script connection
                            }
                        }
                        mainUIButtons.CheckButtonNames();
                    }

                    if (mainUIButtons.wantToRemoveHipCycleForce)
                    {
                        mainUIButtons.wantToRemoveHipCycleForce = false;
                        if (easyMoanCycleForce != null) easyMoanCycleForce.RemoveCycleForce();
                        mainUIButtons.hasCycleForce = false;
                        if (mainUIButtons.animatingPenis)
                        {
                            CheckScriptConnection();
                            if (_scriptConnectionTransform != null)
                            {
                                _scriptConnectionTransform.position = new Vector3(_scriptConnectionTransform.position.x, _scriptConnectionTransform.position.y, 0.3f); //0.3 means switch to script connection
                            }
                        }
                        mainUIButtons.CheckButtonNames();
                    }

                    if (mainUIButtons.wantToRemovePenis)
                    {
                        mainUIButtons.wantToRemovePenis = false;
                        RemoveDildo();
                        mainUIButtons.hasPenis = false;
                        mainUIButtons.animatingPenis = false;
                        mainUIButtons.CheckButtonNames();
                        useDeepVagTarget = !useDeepVagTarget;
                    }
                }
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        void TestSoundCategory(List<NamedAudioClip> soundCategory)
        {
            PlaySound(soundCategory[soundTesterIndex], true);
            soundTesterIndex++;
            if (soundTesterIndex >= soundCategory.Count())
            {
                soundTesterIndex = 0;
                soundTesterCategory++;
            }
        }

        void StartOrgasm()
        {
            vagTouchTime = -stimulationToOrgasm.val / 3.0f;
            orgasming = true;
            orgasmStartTime = Time.timeSinceLevelLoad;
            orgasmStep = 0;

            if (logMessages) SuperController.LogMessage("Start orgasm sequence");
        }

        void TestAllSounds()
        {
            if (!headAudioSource.isPlaying && Input.GetKey(KeyCode.Space))
            {
                //StartOrgasm(); return;
                switch (soundTesterCategory)
                {
                    case 0:
                        TestSoundCategory(lipTouchAudioClips); break;
                    case 1:
                        TestSoundCategory(breastTouchAudioClips); break;
                    case 2:
                        TestSoundCategory(labiaTouchAudioClips); break;
                    case 3:
                        TestSoundCategory(vagTouchAudioClips); break;
                    case 4:
                        TestSoundCategory(deepVagTouchAudioClips); break;
                    case 5:
                        TestSoundCategory(orgasmAudioClips); break;
                    case 6:
                        TestSoundCategory(coolDownAudioClips); break;
                    case 7:
                        TestSoundCategory(longOrgasmAudioClips); break;

                }
            }
        }

        void HandleOrgasm()
        {
            //give it a little time to finish each step before moving on
            float timeBetweenSteps = 0.5f;
            bool progressToNextStep = false;
            if (currentMorph == null || currentMorph.val < 0.1f)
            {
                if (Time.timeSinceLevelLoad - orgasmStartTime > timeBetweenSteps)
                {
                    progressToNextStep = true;
                }
            }

            //don't blink during orgasm
            SetVAMBlinkEnabled(false);
            if (orgasmStep == 0 && progressToNextStep)
            {
                headAudio.CallAction("Stop");
                //if we choose a long orgasm clip, then just play it, and don't play the rest, 25% chance
                if (UnityEngine.Random.value < 0.25f)
                {
                    PlaySound(longOrgasmAudioClips[UnityEngine.Random.Range(0, longOrgasmAudioClips.Count())], true);

                    orgasmStep = lastOrgasmStep;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
                else
                {
                    PlaySound(orgasmAudioClips[UnityEngine.Random.Range(0, orgasmAudioClips.Count())], true);

                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 1 && progressToNextStep)
            {
                if (!headAudioSource.isPlaying)
                {
                    PlaySound(coolDownAudioClips[UnityEngine.Random.Range(0, coolDownAudioClips.Count())], true);

                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == lastOrgasmStep && progressToNextStep) //MAKE SURE THIS IS ONE AFTER THE PRIOR STEP
            {
                if (!headAudioSource.isPlaying)
                {
                    orgasming = false;
                    orgasmStep = 0;
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                }
            }
        }

        void ObserveLipTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                if (!lipTouching && Time.timeSinceLevelLoad - lastDesiredThroatSound > 1.0f && !orgasming)
                {
                    foreplayTouchLastTime = Time.timeSinceLevelLoad;
                    lipTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    }

                    /*if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Lip Sound" + " sender: " + sender.ToString());
                    if (logMessages && logTriggerMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);*/
                }
            }
            else
            {
                lipTouching = false;
            }
        }

        void ObserveMouthTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                lastDesiredThroatSound = Time.timeSinceLevelLoad;
                if (!mouthTouching && !orgasming)
                {
                    mouthTouching = true;
                    wasThroat = true;

                    if (!headAudioSource.isPlaying)
                    {
                        SetLastMoanTime();
                        soundToPlay = mouthTouchAudioClips[UnityEngine.Random.Range(0, mouthTouchAudioClips.Count())];
                    }

                    //if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Mouth Sound 1");
                }
            }
            else
            {
                if (mouthTouching)
                {
                    mouthTouching = false;
                }
            }
        }

        void ObserveThroatTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                lastDesiredThroatSound = Time.timeSinceLevelLoad;
                if (!throatTouching && !orgasming)
                {
                    throatTouching = true;
                    wasThroat = true;

                    if (!headAudioSource.isPlaying)
                    {
                        soundToPlay = throatTouchAudioClips[UnityEngine.Random.Range(0, throatTouchAudioClips.Count())];
                    }


                }
            }
            else
            {
                if (throatTouching)
                {
                    throatTouching = false;
                }
            }
        }

        void ObservelBreastTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                if (!lBreastTouching && !throatTouching && !mouthTouching && !orgasming)
                {
                    foreplayTouchLastTime = Time.timeSinceLevelLoad;
                    lBreastTouching = true;


                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = breastTouchAudioClips[UnityEngine.Random.Range(0, breastTouchAudioClips.Count())];
                    }

                    // if (logMessages && logTriggerMessages) SuperController.LogMessage("l breast touch   time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                lBreastTouching = false;
                //if (logMessages && logTriggerMessages) SuperController.LogMessage("stop touching l breast");
            }
        }

        void ObserverBreastTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                if (!rBreastTouching && !throatTouching && !mouthTouching && !orgasming)
                {
                    foreplayTouchLastTime = Time.timeSinceLevelLoad;
                    rBreastTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = breastTouchAudioClips[UnityEngine.Random.Range(0, breastTouchAudioClips.Count())];
                    }

                    //  if (logMessages && logTriggerMessages) SuperController.LogMessage("r breast touch   time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                rBreastTouching = false;
                //if (logMessages && logTriggerMessages) SuperController.LogMessage("stop touching r breast");
            }
        }

        void ObserveLabiaTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                if (!labiaTouching && !orgasming)
                {
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                    labiaTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        if (percentToOrgasm > 0.9)
                        {
                            soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                            if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Deep Vag sound when touching labia due to orgasm percent: " + percentToOrgasm);
                        }
                        else if (percentToOrgasm > 0.75 && UnityEngine.Random.value > 0.5f)
                        {
                            //when close to orgasm include also vag touch clips
                            soundToPlay = vagTouchAudioClips[UnityEngine.Random.Range(0, vagTouchAudioClips.Count())];
                            if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Vag sound when touching labia due to chance and orgasm percent: " + percentToOrgasm);
                        }
                        else
                        {
                            soundToPlay = labiaTouchAudioClips[UnityEngine.Random.Range(0, labiaTouchAudioClips.Count())];
                            if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Labia Sound");
                        }
                    }

                    // if (logMessages && logTriggerMessages) SuperController.LogMessage("labia touch   time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                labiaTouching = false;
                // if (logMessages && logTriggerMessages) SuperController.LogMessage("stop labia touch");
            }
        }

        void SimVagTrigger()
        {
            if (!orgasming) {
                vagTouchLastTime = Time.timeSinceLevelLoad;
                vagTouching = true;

                if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                {
                    SetLastMoanTime();
                    if (percentToOrgasm > 0.9)
                    {
                        soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                        if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Deep Vag sound when touching vag due to orgasm percent: " + percentToOrgasm);
                    }
                    else if (percentToOrgasm > 0.75 && UnityEngine.Random.value > 0.5f)
                    {
                        //when close to orgasm include also deep vag touch clips
                        soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                        if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Deep Vag sound when touching vag due to chance and orgasm percent: " + percentToOrgasm);
                    }
                    else
                    {
                        soundToPlay = vagTouchAudioClips[UnityEngine.Random.Range(0, vagTouchAudioClips.Count())];
                        if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Vag Sound");
                    }
                }
            }
        }

        void ObserveVagTrigger(object sender, TriggerEventArgs e)
        {
            bool isEntered = e.evtType == "Entered";
            bool isAlwaysEntered = false;
            if (easyMoanCycleForce != null && easyMoanCycleForce.HasCycleForce())
            {
                //she is cycling, so even if it stays in the whole time, it's moving, and should be considered stimulating
                isEntered = true;
                isAlwaysEntered = true;
            }
            if (isEntered && !SuperController.singleton.isLoading)
            {
                //either we are entering after having not been inside
                //or she is thrusting, then we don't need to remove and retouch
                if ((!vagTouching || isAlwaysEntered) && !orgasming)
                {
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                    vagTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        if (percentToOrgasm > 0.9)
                        {
                            soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                            if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Deep Vag sound when touching vag due to orgasm percent: " + percentToOrgasm);
                        }
                        else if (percentToOrgasm > 0.75 && UnityEngine.Random.value > 0.5f)
                        {
                            //when close to orgasm include also deep vag touch clips
                            soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                            if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Deep Vag sound when touching vag due to chance and orgasm percent: " + percentToOrgasm);
                        } else {
                            soundToPlay = vagTouchAudioClips[UnityEngine.Random.Range(0, vagTouchAudioClips.Count())];
                            if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Vag Sound");
                        }
                    }

                    //if (logMessages && logTriggerMessages) SuperController.LogMessage("vag touch   time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                vagTouching = false;
                //if (logMessages && logTriggerMessages) SuperController.LogMessage("stop vag touch");

            }
        }

        void ObserveDeepVagTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered" && !SuperController.singleton.isLoading)
            {
                if (!deepVagTouching && !orgasming)
                {
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                    deepVagTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                        if (logMessages && logTriggerMessages) SuperController.LogMessage("Play Deep Vag Sound");
                    }

                    //if (logMessages && logTriggerMessages) SuperController.LogMessage("deepvag touch   time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                deepVagTouching = false;
                //if (logMessages && logTriggerMessages) SuperController.LogMessage("stop deep vag touch");
            }
        }

        void OnDestroy()
        {
            try
            {
                if (genitalMorphsOn.val)
                {
                    RestoreGenitalMorphs();
                }

                if (mainUIButtons != null) mainUIButtons.OnDestroy();
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }

        private void RestoreGenitalMorphs()
        {
            if (anusOpenOut != null) anusOpenOut.val = anusOpenOut_start;
            if (anusPushPull != null) anusPushPull.val = anusPushPull_start;
            if (labiaMinoraLowL != null) labiaMinoraLowL.val = labiaMinoraLowL_start;
            if (labiaMinoraLowR != null) labiaMinoraLowR.val = labiaMinoraLowR_start;
            if (labiaMajoraLowL != null) labiaMajoraLowL.val = labiaMajoraLowL_start;
            if (labiaMajoraLowR != null) labiaMajoraLowR.val = labiaMajoraLowR_start;
            if (vaginaExpansion != null) vaginaExpansion.val = vaginaExpansion_start;
        }
    }

    //VRAdultFun trigger helper
    public class TriggerEventArgs : EventArgs
    {
        public Collider collider { get; set; }
        public string evtType { get; set; }
    }

    public class TriggerCollide : MonoBehaviour
    {
        TriggerEventArgs lastEvent;

        public event EventHandler<TriggerEventArgs> OnCollide;

        void Awake()
        {
            lastEvent = new TriggerEventArgs
            {
                evtType = "none",
                collider = null
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            //if (logMessages) SuperController.LogMessage("Trigger Enter coll: " + other.tag + " " + other.attachedRigidbody + " " + other.ToString());
            //Don't collide with abdomen, or abdomen triggers vag collider when it shouldn't
            if (other.attachedRigidbody.name.StartsWith("AutoColliderFemaleAutoCollidersabdomen"))
            {
                return;
            }
            DoCollideEvent("Entered", other);
        }

        private void OnTriggerExit(Collider other)
        {
            DoCollideEvent("Exited", other);
        }

        /*
        private void OnTriggerStay(Collider other)
        {
            DoCollideEvent("Stay", other);
        }
        */

        private void DoCollideEvent(string evtType, Collider col)
        {
            if (string.Equals(evtType, lastEvent.evtType) && col.gameObject == lastEvent.collider.gameObject)
            {
                return;
            }
            else
            {
                TriggerEventArgs tempEvent = new TriggerEventArgs
                {
                    collider = col,
                    evtType = evtType
                };
                OnCollideEvent(tempEvent);
                lastEvent = tempEvent;
            }
        }

        protected virtual void OnCollideEvent(TriggerEventArgs e)
        {
            EventHandler<TriggerEventArgs> handler = OnCollide;
            handler?.Invoke(this, e);
        }

    }
}
