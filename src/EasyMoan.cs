using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
namespace geesp0t
{
    public class EasyMoan : MVRScript
    {
        protected JSONStorableBool soundOn;
        protected JSONStorableBool facialExpressionsOn;
        protected JSONStorableBool breathingOn;
        protected JSONStorableBool gazeOn;

        protected JSONStorableFloat morphPowerPercent;
        protected JSONStorableFloat timeBetweenMoans;
        protected JSONStorableFloat randomExtraTimePercent;

        protected JSONStorableFloat endBlowJobSpeed;
        protected JSONStorableFloat stimulationToOrgasm;

        protected JSONStorableFloat headMinDistance;

        protected JSONStorableString explanationString;

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

        protected bool logMessages = false;

        protected NamedAudioClip audioClip;
        protected BreatheLite breatheLite;
        protected GazeLite gazeLite;
        
        //plays morphs better when PlaySound is called from update, otherwise it breaks for some reason
        protected NamedAudioClip soundToPlay = null;

        public class SoundWithMorph
        {
            public NamedAudioClip sound;
            public DAZMorph morph;

            public SoundWithMorph(NamedAudioClip theSound, DAZMorph theMorph)
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
            public DAZMorph morph = null;

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
        protected List<NamedAudioClip> preOrgasmAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> orgasm2AudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> extendedOrgasmAudioClips = new List<NamedAudioClip>();
        protected List<NamedAudioClip> coolDownAudioClips = new List<NamedAudioClip>();

        protected DAZMorph dazMorph;

        protected List<string> orgasmMorphs = new List<string>();

        protected DAZMorph currentMorph;
        protected DAZMorph desiredMorph;
        protected bool desiredMorphReady = false;

        protected float morphActivateTime = 0.05f;
        protected float morphHoldTime = 0; //set dynamically
        protected float morphDeactivateTime = 0.3f;
        protected float morphTime = 0;
        protected float morphHoldOscVal = 0;
        protected bool morphChanging = false;

        protected bool orgasming = false;
        protected int orgasmStep = 0;
        protected float orgasmStartTime = 0;

        protected int soundTesterIndex = 0;
        protected int soundTesterCategory = 0;

        protected bool wasVAMAutoBlink = false;

        public override void Init()
        {
            try
            {
                soundOn = new JSONStorableBool("Sound On", true);
                CreateToggle(soundOn);
                RegisterBool(soundOn);
                soundOn.storeType = JSONStorableParam.StoreType.Full;

                facialExpressionsOn = new JSONStorableBool("Facial Expressions On", true);
                CreateToggle(facialExpressionsOn);
                RegisterBool(facialExpressionsOn);
                facialExpressionsOn.storeType = JSONStorableParam.StoreType.Full;

                breathingOn = new JSONStorableBool("Breathing On", true);
                CreateToggle(breathingOn);
                RegisterBool(breathingOn);
                breathingOn.storeType = JSONStorableParam.StoreType.Full;

                gazeOn = new JSONStorableBool("Gaze On", false);
                CreateToggle(gazeOn);
                RegisterBool(gazeOn);
                gazeOn.storeType = JSONStorableParam.StoreType.Full;

                headMinDistance = new JSONStorableFloat("Head Audio Distance (volume)", 0.8f, 0.3f, 10.0f, false);
                RegisterFloat(headMinDistance);
                headMinDistance.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(headMinDistance);

                morphPowerPercent = new JSONStorableFloat("Expression Max Power", 1.0f, 0.0f, 1.0f, false);
                RegisterFloat(morphPowerPercent);
                morphPowerPercent.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(morphPowerPercent, true);

                timeBetweenMoans = new JSONStorableFloat("Min Time Between Moans", 1.5f, 0.0f, 20.0f, false);
                RegisterFloat(timeBetweenMoans);
                timeBetweenMoans.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(timeBetweenMoans);

                randomExtraTimePercent = new JSONStorableFloat("Random Extra Percent Between Moans", 0.3f, 0.0f, 20.0f, false);
                RegisterFloat(randomExtraTimePercent);
                randomExtraTimePercent.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(randomExtraTimePercent);

                endBlowJobSpeed = new JSONStorableFloat("Finished Blowjob Delay", 0.65f, 0f, 2f, false);
                RegisterFloat(endBlowJobSpeed);
                endBlowJobSpeed.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(endBlowJobSpeed);

                explanationString = new JSONStorableString("Orgasm Percent: 0%", "");
                UIDynamicTextField dtext = CreateTextField(explanationString, false);

                stimulationToOrgasm = new JSONStorableFloat("Touch Time Till Orgasm (shortest possible time)", 45.0f, 15.0f, 240.0f, false);
                RegisterFloat(stimulationToOrgasm);
                stimulationToOrgasm.storeType = JSONStorableParam.StoreType.Full;
                CreateSlider(stimulationToOrgasm);

                CreateButton("Reset Easy Moan Facial Expressions", true).button.onClick.AddListener(() =>
                {
                    ResetFacialExpressions();
                });
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void ResetFacialExpressions()
        {
            JSONStorable js = containingAtom.GetStorableByID("geometry");
            DAZCharacterSelector dcs = js as DAZCharacterSelector;
            GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;
            foreach (SoundWithMorphTableEntry tableEntry in soundWithMorphTable)
            {
                tableEntry.morph = morphUI.GetMorphByDisplayName(tableEntry.morphName);
                if (tableEntry.morph != null)
                {
                    tableEntry.morph.SetValue(0);
                }
            }
        }

        public void Start()
        {
            try
            {
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
                orgasmMorphs.Clear();
                orgasmMorphs.Add("AAsex_sqntwrry2OH9");
                orgasmMorphs.Add("AAsex_brraiseOH8");
                orgasmMorphs.Add("AAsex_closetightAA6");
                orgasmMorphs.Add("AAsex_closetightsm1e");
                orgasmMorphs.Add("AAsex_squintbrOH8");
                orgasmMorphs.Add("AAsex_wideOH7");
                for (int j = 0; j < orgasmMorphs.Count(); j++)
                {
                    soundWithMorphTable.Add(new SoundWithMorphTableEntry("Orgasm" + j, orgasmMorphs[j]));
                }

                //PRE ORGASM
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathSteady", "AAsex_sqntwrry2AA3"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemGasps2", "AAsex_sqntwrry2AA6"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemGasps3", "AAsex_closedAA1"));

                //POST ORGASM
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathCoolDown1", "AAsex_wideOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathCoolDown2", "AAsex_closedOH7"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathCoolDown3", "AAsex_sqntwrry2SH2"));
                soundWithMorphTable.Add(new SoundWithMorphTableEntry("FemBreathCoolDown4", "AAsex_wideAA3"));
                
                //find morphs
                JSONStorable js = containingAtom.GetStorableByID("geometry");
                DAZCharacterSelector dcs = js as DAZCharacterSelector;
                GenerateDAZMorphsControlUI morphUI = dcs.morphsControlUI;

                DAZMorph stomachMorph = morphUI.GetMorphByDisplayName("Breath1");
                if (stomachMorph == null) SuperController.LogError("Missing stomach morph");

                foreach (SoundWithMorphTableEntry tableEntry in soundWithMorphTable)
                {
                    tableEntry.morph = morphUI.GetMorphByDisplayName(tableEntry.morphName);
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
                preOrgasmAudioClips.Clear();
                orgasm2AudioClips.Clear();
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
                        else if (namedAudioClip.displayName.StartsWith("FemMoanExaggerated"))
                        {
                            orgasm2AudioClips.Add(namedAudioClip);
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
                        if (namedAudioClip.displayName.StartsWith("FemBreathCoolDown"))
                        {
                            coolDownAudioClips.Add(namedAudioClip);
                        }
                        else if (namedAudioClip.displayName.StartsWith("FemGasps"))
                        {
                            preOrgasmAudioClips.Add(namedAudioClip);
                        }
                        else if (namedAudioClip.displayName.EndsWith("FemBreathSteady"))
                        {
                            preOrgasmAudioClips.Add(namedAudioClip);
                        }
                    }
                    //SuperController.LogMessage(namedAudioClip.category + " " + namedAudioClip.displayName);
                }

                //if we want to have a clip play on start
                /*audioClip = helloAudioClips[UnityEngine.Random.Range(0, helloAudioClips.Count())];
                if (soundOn.val) headAudio.CallAction("PlayNowClearQueue", audioClip);*/

                breatheLite = new BreatheLite();
                breatheLite.InitBreathing(containingAtom, stomachMorph);

                gazeLite = new GazeLite();
                gazeLite.Init(containingAtom, (FreeControllerV3)containingAtom.GetStorableByID("headControl"));

            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void FixedUpdate()
        {
            if (gazeOn.val) gazeLite.FixedUpdate();
        }

        public void PlaySound(NamedAudioClip audioClip, bool duringOrgasm = false, bool useMorph = true, bool overrideIsOrgasmSound = false)
        {
            try
            {
                if (orgasming && !duringOrgasm) return;

                if (soundOn.val)
                {
                    headAudio.CallAction("PlayNow", audioClip);
                }
                if (facialExpressionsOn.val && useMorph)
                {

                    desiredMorphReady = false;
                    foreach (SoundWithMorphTableEntry tableEntry in soundWithMorphTable)
                    {
                        if (tableEntry.soundName == audioClip.displayName)
                        {
                            morphHoldTime = audioClip.clipToPlay.length - (morphActivateTime + morphDeactivateTime + 0.25f);
                            if (logMessages) SuperController.LogMessage("Found sound " + audioClip.displayName + " " + " " + audioClip.category + " " + audioClip.clipToPlay.length);

                            //special for orgasm, choose a random expression
                            if (tableEntry.isOrgasmSound || overrideIsOrgasmSound)
                            {
                                int orgasmIndex = UnityEngine.Random.Range(0, orgasmMorphs.Count());
                                string orgasmString = "Orgasm" + orgasmIndex;
                                foreach (SoundWithMorphTableEntry subTableEntry in soundWithMorphTable)
                                {
                                    if (subTableEntry.soundName == orgasmString)
                                    {
                                        desiredMorph = subTableEntry.morph;
                                        if (logMessages) SuperController.LogMessage("Desired Orgasm Morph " + desiredMorph.morphName);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                desiredMorph = tableEntry.morph;
                                if (logMessages) SuperController.LogMessage("Desired Morph " + desiredMorph.morphName);
                            }

                            desiredMorphReady = true;
                            break;
                        }
                    }

                    if (logMessages && !desiredMorphReady) SuperController.LogMessage("Missing morph for sound " + audioClip.displayName + " " + audioClip.category);
                }

               if (logMessages) SuperController.LogMessage("Playing sound " + audioClip.displayName + " morph " + desiredMorph.displayName);
            }
            catch (Exception e) { SuperController.LogError("Exception caught: " + e); }
        }

        public void SetLastMoanTime()
        {
            lastMoanTime = Time.timeSinceLevelLoad;
            extraRandomTimeBetweenMoans = UnityEngine.Random.value * timeBetweenMoans.val * randomExtraTimePercent.val;
        }

        public void SetVAMBlinkEnabled(bool enabled)
        {
            JSONStorable personEyelids = containingAtom.GetStorableByID("EyelidControl");
            if (personEyelids != null)
            {
                personEyelids.SetBoolParamValue("blinkEnabled", enabled);
            }
        }

        public void Update()
        {
            try
            {
                TestAllSounds();
                //to test sounds starting in a location, also put this in start() soundTesterCategory = 5; soundTesterIndex = 0;
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

                //apply morph
                if (facialExpressionsOn.val && desiredMorphReady)
                {
                    desiredMorphReady = false;

                    if (currentMorph != null) currentMorph.SetValue(0);
                    currentMorph = desiredMorph;

                    morphChanging = true;
                    morphTime = 0;
                    currentMorph.SetValue(0);
                    if (logMessages) SuperController.LogMessage("Desired Morph: " + desiredMorph.displayName);

                    SetVAMBlinkEnabled(false);
                }

                if (facialExpressionsOn.val && morphChanging && currentMorph != null)
                {
                    morphTime += Time.deltaTime;
                    if (morphTime < morphActivateTime)
                    {
                        currentMorph.SetValue((morphTime / morphActivateTime) * morphPowerPercent.val);
                        morphHoldOscVal = 0;
                    }
                    else if (morphTime < morphActivateTime + morphHoldTime)
                    {
                        //oscillate a little during hold
                        morphHoldOscVal += Time.deltaTime;
                        currentMorph.SetValue((1.0f - Mathf.Sin(morphHoldOscVal * 7.0f) * 0.07f) * morphPowerPercent.val);
                    }
                    else if (morphTime >= morphActivateTime + morphHoldTime + morphDeactivateTime)
                    {
                        currentMorph.SetValue(0);
                        morphChanging = false;
                        SetVAMBlinkEnabled(wasVAMAutoBlink);
                    }
                    else
                    {
                        currentMorph.SetValue((1.0f - (morphTime - (morphActivateTime + morphHoldTime)) / morphDeactivateTime) * morphPowerPercent.val);
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
                if (vagTouchLastTime - Time.timeSinceLevelLoad < 1.0f && (labiaTouching || vagTouching || deepVagTouching))
                {
                    vagTouchTime += Time.deltaTime;
                }
                else if (foreplayTouchLastTime - Time.timeSinceLevelLoad < 1.0f && (lBreastTouching || rBreastTouching || lipTouching))
                {
                    //foreplay goes half way
                    if (vagTouchTime < stimulationToOrgasm.val / 2.0f) vagTouchTime += Time.deltaTime;
                }
                else if (vagTouchTime > 0)
                {
                    vagTouchTime -= Time.deltaTime / 5.0f;
                }

                if (vagTouchTime >= stimulationToOrgasm.val)
                {
                    //ORGASM
                    vagTouchTime = -15.0f;
                    orgasming = true;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                    orgasmStep = 0;

                    if (logMessages) SuperController.LogMessage("Start orgasm sequence");
                }

                if (orgasming)
                {
                    HandleOrgasm();
                }

                float percentToOrgasm = vagTouchTime / stimulationToOrgasm.val;
                explanationString.val = string.Format("Orgasm Percent: {0:P}", percentToOrgasm);

                if (breathingOn.val) breatheLite.UpdateBreathing(Mathf.Lerp(0.5f, 1.5f, percentToOrgasm));
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

        void TestAllSounds()
        {
            if (!headAudioSource.isPlaying && Input.GetKey(KeyCode.Space))
            {
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
                        TestSoundCategory(preOrgasmAudioClips); break;
                    case 6:
                        TestSoundCategory(coolDownAudioClips); break;

                }
            }
        }

        void HandleOrgasm()
        {
            //give it a little time to finish each step before moving on
            float timeBetweenSteps = 0.5f;
            bool progressToNextStep = false;
            NamedAudioClip clipToPlay = null;
            if (currentMorph == null || currentMorph.morphValue < 0.1f)
            {
                if (Time.timeSinceLevelLoad - orgasmStartTime > timeBetweenSteps)
                {
                    progressToNextStep = true;
                }
            }
            if (orgasmStep == 0 && progressToNextStep)
            {
                headAudio.CallAction("Stop");
                clipToPlay = lipTouchAudioClips.Find(item => item.displayName == "FemPixieW1012");
                if (clipToPlay == null) clipToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                PlaySound(clipToPlay, true);
                orgasmStep++;
                orgasmStartTime = Time.timeSinceLevelLoad;
            }
            else if (orgasmStep == 1 && progressToNextStep)
            {
                if (!headAudioSource.isPlaying)
                {
                    clipToPlay = lipTouchAudioClips.Find(item => item.displayName == "FemPixieW1011");
                    if (clipToPlay == null) clipToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    PlaySound(clipToPlay, true);

                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 2 && progressToNextStep)
            {
                if (!headAudioSource.isPlaying)
                {
                    clipToPlay = lipTouchAudioClips.Find(item => item.displayName == "FemPixieW1010");
                    if (clipToPlay == null) clipToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    PlaySound(clipToPlay, true);

                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 3 && progressToNextStep)
            {
                if (!headAudioSource.isPlaying)
                {
                    clipToPlay = lipTouchAudioClips.Find(item => item.displayName == "FemPixieW1009");
                    if (clipToPlay == null) clipToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    PlaySound(clipToPlay, true);

                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 4 && progressToNextStep)
            {
                if (!headAudioSource.isPlaying)
                {
                    PlaySound(preOrgasmAudioClips[UnityEngine.Random.Range(0, preOrgasmAudioClips.Count())], true);
                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 5 && progressToNextStep)
            {
                if (!headAudioSource.isPlaying)
                {
                    clipToPlay = deepVagTouchAudioClips.Find(item => item.displayName == "FemPixieW1097");
                    if (clipToPlay == null) clipToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                    PlaySound(clipToPlay, true, true, true);
                    orgasmStep += UnityEngine.Random.Range(1, 3);
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 6 && progressToNextStep)
            {
                //may be skipped
                if (!headAudioSource.isPlaying)
                {
                    clipToPlay = lipTouchAudioClips.Find(item => item.displayName == "FemPixieW1009");
                    if (clipToPlay == null) clipToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    PlaySound(clipToPlay, true);

                    orgasmStep += UnityEngine.Random.Range(1, 3);
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 7 && progressToNextStep)
            {
                //may be skipped
                if (!headAudioSource.isPlaying)
                {
                    PlaySound(coolDownAudioClips[UnityEngine.Random.Range(0, coolDownAudioClips.Count())], true);
                    orgasmStep += UnityEngine.Random.Range(1, 3);
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 8 && progressToNextStep)
            {
                //may be skipped if other one is not
                if (!headAudioSource.isPlaying)
                {
                    PlaySound(coolDownAudioClips[UnityEngine.Random.Range(0, coolDownAudioClips.Count())], true);
                    orgasmStep++;
                    orgasmStartTime = Time.timeSinceLevelLoad;
                }
            }
            else if (orgasmStep == 9)
            {
                if (!headAudioSource.isPlaying)
                {
                    orgasming = false;
                    orgasmStep = 0;
                }
            }
        }

        void ObserveLipTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered")
            {
                if (!lipTouching && Time.timeSinceLevelLoad - lastDesiredThroatSound > 1.0f)
                {
                    foreplayTouchLastTime = Time.timeSinceLevelLoad;
                    lipTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = lipTouchAudioClips[UnityEngine.Random.Range(0, lipTouchAudioClips.Count())];
                    }

                    /*if (logMessages) SuperController.LogMessage("Play Lip Sound" + " sender: " + sender.ToString());
                    if (logMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);*/
                }
            }
            else
            {
                lipTouching = false;
            }
        }

        void ObserveMouthTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered")
            {
                lastDesiredThroatSound = Time.timeSinceLevelLoad;
                if (!mouthTouching)
                {
                    mouthTouching = true;
                    wasThroat = true;

                    if (!headAudioSource.isPlaying)
                    {
                        SetLastMoanTime();
                        soundToPlay = mouthTouchAudioClips[UnityEngine.Random.Range(0, mouthTouchAudioClips.Count())];
                    }

                    //if (logMessages) SuperController.LogMessage("Play Mouth Sound 1");
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
            if (e.evtType == "Entered")
            {
                lastDesiredThroatSound = Time.timeSinceLevelLoad;
                if (!throatTouching)
                {
                    throatTouching = true;
                    wasThroat = true;

                    if (!headAudioSource.isPlaying)
                    {
                        soundToPlay = throatTouchAudioClips[UnityEngine.Random.Range(0, throatTouchAudioClips.Count())];
                    }

                    //if (logMessages) SuperController.LogMessage("Play Throat Sound 1");
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
            if (e.evtType == "Entered")
            {
                if (!lBreastTouching && !throatTouching && !mouthTouching)
                {
                    foreplayTouchLastTime = Time.timeSinceLevelLoad;
                    lBreastTouching = true;


                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = breastTouchAudioClips[UnityEngine.Random.Range(0, breastTouchAudioClips.Count())];
                    }

                    //if (logMessages) SuperController.LogMessage("Play Breast Sound");
                    //if (logMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                lBreastTouching = false;
            }
        }

        void ObserverBreastTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered")
            {
                if (!rBreastTouching && !throatTouching && !mouthTouching)
                {
                    foreplayTouchLastTime = Time.timeSinceLevelLoad;
                    rBreastTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = breastTouchAudioClips[UnityEngine.Random.Range(0, breastTouchAudioClips.Count())];
                    }

                    //if (logMessages) SuperController.LogMessage("Play Breast Sound");
                    //if (logMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                rBreastTouching = false;
            }
        }

        void ObserveLabiaTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered")
            {
                if (!labiaTouching)
                {
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                    labiaTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = labiaTouchAudioClips[UnityEngine.Random.Range(0, labiaTouchAudioClips.Count())];
                    }

                    if (logMessages) SuperController.LogMessage("Play Labia Sound");
                    if (logMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                labiaTouching = false;
            }
        }

        void ObserveVagTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered")
            {
                if (!vagTouching)
                {
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                    vagTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = vagTouchAudioClips[UnityEngine.Random.Range(0, vagTouchAudioClips.Count())];
                    }

                    //if (logMessages) SuperController.LogMessage("Play Vag Sound");
                    //if (logMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                vagTouching = false;
            }
        }

        void ObserveDeepVagTrigger(object sender, TriggerEventArgs e)
        {
            if (e.evtType == "Entered")
            {
                if (!deepVagTouching)
                {
                    vagTouchLastTime = Time.timeSinceLevelLoad;
                    deepVagTouching = true;

                    if (!headAudioSource.isPlaying && Time.timeSinceLevelLoad - lastMoanTime > timeBetweenMoans.val + extraRandomTimeBetweenMoans)
                    {
                        SetLastMoanTime();
                        soundToPlay = deepVagTouchAudioClips[UnityEngine.Random.Range(0, deepVagTouchAudioClips.Count())];
                    }

                    //if (logMessages) SuperController.LogMessage("Play Deep Vag Sound");
                    //if (logMessages) SuperController.LogMessage("time: " + vagTouchTime + ", touching: " + labiaTouching + " " + vagTouching + " " + deepVagTouching);
                }
            }
            else
            {
                deepVagTouching = false;
            }
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
            //SuperController.LogMessage("Trigger Enter coll: " + other.tag + " " + other.attachedRigidbody + " " + other.ToString());
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