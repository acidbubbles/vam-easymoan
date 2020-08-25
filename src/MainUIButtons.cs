using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VR;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace geesp0t
{
    //Adapted from VAMDeluxe Dollmaster UI
    public class MainUIButtons
    {

        MVRScript plugin;

        private Camera _mainCamera;
        public static Canvas canvas = null;
        private float UIScale = 1.0f;

        UIDynamicButton hipCycleForceButton = null;
        UIDynamicButton penisButton = null;
        UIDynamicButton rotatePenisButton = null;

        private bool isDesktopMode = false;

        public bool wantToCreateHipCycleForce = false;
        public bool wantToCreatePenis = false;
        
        public bool wantToAnimatePenis = false;

        public bool wantToRemoveHipCycleForce = false;
        public bool wantToRemovePenis = false;
        public bool wantToRotatePenis = false;

        public bool hasCycleForce = false;
        public bool hasPenis = false;
        public bool animatingPenis = false;

        public void Init(MVRScript _plugin)
        {
            plugin = _plugin;
            _mainCamera = CameraTarget.centerTarget?.targetCamera;
            isDesktopMode = !(SuperController.singleton.isOVR || SuperController.singleton.isOpenVR);
        }

        public void Start()
        {
            Cleanup();
            float worldScale = SuperController.singleton.worldScale;
            SuperController.singleton.worldScale = 1.0f;
            CreateButtons();
            SuperController.singleton.worldScale = worldScale;
        }

        public void Cleanup()
        {
            if (canvas != null)
            {
                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                canvas.transform.SetParent(null, false);

                if (canvas.gameObject != null)
                {
                    GameObject.Destroy(canvas.gameObject);
                }
            }
        }

        public void CreateButtons()
        {
            float scale = 0.001f;

            Cleanup();

            GameObject canvasObject = new GameObject();
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.pixelPerfect = false;
            SuperController.singleton.AddCanvas(canvas);

            canvas.transform.SetParent(SuperController.singleton.mainHUD, false);

            CanvasScaler cs = canvasObject.AddComponent<CanvasScaler>();
            cs.scaleFactor = 80.0f;
            cs.dynamicPixelsPerUnit = 1f;

            GraphicRaycaster gr = canvasObject.AddComponent<GraphicRaycaster>();

            canvas.transform.localScale = new Vector3(scale, scale, scale);
            //canvas.transform.localPosition = new Vector3(-0.7f, 0, 0);

            canvas.transform.localPosition = new Vector3(-.45f, -0.72f, 0.35f);

            LookAtCamera();

            int startingColumn = 0;
            if (IS_AUTOMATE_VERSION.IS_AUTOMATE) startingColumn = 2;

            hipCycleForceButton = AddButton("Thrust When Aroused", () =>
            {
                if (!hasCycleForce) wantToCreateHipCycleForce = true;
                else wantToRemoveHipCycleForce = true;
            }, startingColumn, 1);

            penisButton = AddButton("Create Penis", () =>
            {
                if (hasPenis) {
                    if (animatingPenis) {
                        wantToRemovePenis = true;
                        animatingPenis = false;
                    } else {
                        wantToAnimatePenis = true;
                    }
                } else {
                    wantToCreatePenis = true;
                }
            }, startingColumn, 0);


            rotatePenisButton = AddButton("Rotate Penis", () =>
            {
                wantToRotatePenis = true;
            }, startingColumn+1, 0);
            rotatePenisButton.gameObject.SetActive(false);

            CheckButtonNames();

            canvas.transform.Translate(0, 0.2f, 0);
        }

        public void CheckButtonNames()
        {
            if (penisButton == null || hipCycleForceButton == null) return;

            if (hasPenis)
            {
                rotatePenisButton.gameObject.SetActive(true);
                if (animatingPenis) {
                    penisButton.buttonText.text = "Remove Penis >";
                } else {
                    penisButton.buttonText.text = "Animate Penis >";
                }
            } else
            {
                rotatePenisButton.gameObject.SetActive(false);
                penisButton.buttonText.text = "Create Penis >";
            }

            if (hasCycleForce)
            {
                hipCycleForceButton.buttonText.text = "Remove Thrust";
            } else
            {
                hipCycleForceButton.buttonText.text = "Thrust";
            }
        }

        public UIDynamicButton AddButton(string name, UnityAction callback, int column, int row)
        {
            Color accessButtonColor = new Color(0.8392f, 0.8392f, 0.8392f);
            Color accessTextColor = new Color(0, 0, 0);
            float xSpacing = 0.22f;
            float ySpacing = 0.05f;

            UIDynamicButton button = CreateButton(name, 100, 40);
            button.button.onClick.AddListener(callback);
            button.transform.Translate(column * xSpacing, 0.45f - row * ySpacing, 0, Space.Self);
            ColorButton(button, accessTextColor, accessButtonColor);

            return button;
        }

        public UIDynamicButton CreateButton(string name, float width = 100, float height = 80)
        {
            Transform button = GameObject.Instantiate<Transform>(plugin.manager.configurableButtonPrefab);
            ConfigureTransform(button, width, height);
            ParentToCanvas(button);

            UIDynamicButton uiButton = button.GetComponent<UIDynamicButton>();
            uiButton.label = name;
            uiButton.buttonText.fontSize = 18;
            return uiButton;
        }

        public static void ColorButton(UIDynamicButton button, Color textColor, Color buttonColor)
        {
            button.textColor = textColor;
            button.buttonColor = buttonColor;
        }

        private void ConfigureTransform(Transform t, float width, float height)
        {
            t.transform.position = Vector3.zero;
            RectTransform rt = t.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(width / 2, height / 2);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void ParentToCanvas(Transform t)
        {
            t.SetParent(canvas.transform, false);
        }

        public void LookAtCamera()
        {
            if (isDesktopMode)
            {
                canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
            }
            else
            {
                if (XRSettings.enabled == false)
                {
                    Transform cameraT = SuperController.singleton.lookCamera.transform;
                    Vector3 endPos = cameraT.position + cameraT.forward * 10000000.0f;
                    canvas.transform.LookAt(endPos, cameraT.up);
                }
                else
                {
                    canvas.transform.localEulerAngles = new Vector3(28, 180, 0);
                }
            }
        }

        public void OnDestroy()
        {
            try
            {
                if (SuperController.singleton != null)
                {
                    SuperController.singleton.RemoveCanvas(canvas);
                }

                if (canvas != null)
                {
                    canvas.transform.SetParent(null, false);

                    if (canvas.gameObject != null)
                    {
                        GameObject.Destroy(canvas.gameObject);
                    }
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }

        }
    }
}
