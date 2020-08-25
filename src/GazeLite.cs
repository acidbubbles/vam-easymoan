// Original VaM ScriptEngine plugin created by MacGruber 07/09/2018
// Code ported to new style VaM plugin by VeeRifter 25/10/2018
// Lite version by geesp0t for Easy Moan

using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace geesp0t
{
    public class GazeLite
    {
	
        public void Init(Atom containingAtom, FreeControllerV3 theHeadControl) {
            try {

				person = containingAtom;
				personHeadControl = theHeadControl;
				head = personHeadControl.transform;
				SetReference(person, "chestControl");
				SetLookAtPlayer(-0.2f * Vector3.up);		
			
			}
			catch (System.Exception e) {
				SuperController.LogError("Exception caught: " + e);
			}
		}

        public void FixedUpdate(Atom gazeTargetAtom, FreeControllerV3 gazeTargetFreeController)
        {
            if (lookAtTarget == null || head == null || reference == null)
                return;

            if (gazeTargetAtom != null)
            {
                if (gazeTargetFreeController != null)
                {
                    lookAtTarget = gazeTargetFreeController.transform;
                } else
                {
                    lookAtTarget = gazeTargetAtom.transform;
                }
            }

            // compute horizontal and vertical angles
            Vector3 lookAtPosition = lookAtTarget.TransformPoint(lookAtOffset);
            Vector3 actualDir = reference.InverseTransformDirection(head.forward);
            Vector3 targetDir = lookAtPosition - head.position;
            targetDir.Normalize();
            targetDir = reference.InverseTransformDirection(targetDir);
            Vector2 actualDirH = new Vector2(actualDir.x, actualDir.z);
            Vector2 targetDirH = new Vector2(targetDir.x, targetDir.z);
            Vector2 actualDirV = new Vector2(actualDirH.magnitude, actualDir.y);
            Vector2 targetDirV = new Vector2(targetDirH.magnitude, targetDir.y);
            actualDirH.Normalize();
            targetDirH.Normalize();
            actualDirV.Normalize();
            targetDirV.Normalize();
            float actualH = Mathf.Atan2(actualDirH.x, actualDirH.y);
            float targetH = Mathf.Atan2(targetDirH.x, targetDirH.y);
            float actualV = Mathf.Atan2(actualDirV.y, actualDirV.x);
            float targetV = Mathf.Atan2(targetDirV.y, targetDirV.x);

            // apply focus
            focusChangeClock += focusChangeSpeed * Time.fixedDeltaTime;
            if (focusChangeClock >= 1.0f)
            {
                focusChangeSpeed = 1.0f / Random.Range(focusChangeDurationMin, focusChangeDurationMax);
                focusChangeClock = 0.0f;
                focusPrev = focusNext;
                focusNext = Random.insideUnitCircle;
            }
            float t = Mathf.SmoothStep(0.0f, 1.0f, focusChangeClock);
            targetH += Mathf.Lerp(focusPrev.x, focusNext.x, t) * focusAngleH * Mathf.Deg2Rad;
            targetV += Mathf.Lerp(focusPrev.y, focusNext.y, t) * focusAngleV * Mathf.Deg2Rad;

            // adjust angles
            targetH = Mathf.Clamp(targetH, -maxAngleH, maxAngleH);
            targetV = Mathf.Clamp(targetV, -maxAngleV, maxAngleV);

            actualH = Mathf.SmoothDamp(actualH, targetH, ref velocityH, gazeDuration, Mathf.Infinity, Time.fixedDeltaTime);
            actualV = Mathf.SmoothDamp(actualV, targetV, ref velocityV, gazeDuration, Mathf.Infinity, Time.fixedDeltaTime);

            // recombine
            actualDir = RecombineDirection(actualH, actualV);
            targetDir = RecombineDirection(targetH, targetV);
            actualDir = reference.TransformDirection(actualDir);
            head.transform.LookAt(head.transform.position + actualDir);

            // apply roll
            rollChangeClock += rollChangeSpeed * Time.fixedDeltaTime;
            if (rollChangeClock >= 1.0f)
            {
                rollChangeSpeed = 1.0f / Random.Range(rollChangeDurationMin, rollChangeDurationMax);
                rollChangeClock = 0.0f;
                rollPrev = rollNext;
                rollNext = Random.Range(-(rollAngleMax * Mathf.Deg2Rad), rollAngleMax * Mathf.Deg2Rad);
            }
            t = Mathf.SmoothStep(0.0f, 1.0f, rollChangeClock);
            float roll = Mathf.Lerp(rollPrev, rollNext, t);
            Vector3 eulerAngles = head.transform.localEulerAngles;
            eulerAngles.z = roll * Mathf.Rad2Deg;
            head.transform.localEulerAngles = eulerAngles;

            // compute angle
            currentAngle = Vector3.Angle(actualDir, targetDir);
        }

        // Set a reference object to determine where "forward" is.
        public void SetReference(Atom atom, string controlID)
        {
            reference = atom.GetStorableByID(controlID).transform;
        }

        // Set player as target to look at.
        public void SetLookAtPlayer(Vector3 offset)
        {
            lookAtTarget = CameraTarget.centerTarget.transform;
            lookAtOffset = offset;
        }
		
		public void SetLookAtWindowCamera(Vector3 offset)
		{
			Atom atom = SuperController.singleton.GetAtomByUid("WindowCamera");
            lookAtTarget = atom.mainController.transform;
            lookAtOffset = offset;
        }

        private Vector3 RecombineDirection(float angleH, float angleV)
        {
            float cosV = Mathf.Cos(angleV);
            return new Vector3(
                Mathf.Sin(angleH) * cosV,
                Mathf.Sin(angleV),
                Mathf.Cos(angleH) * cosV
            );
        }

        private Atom person;
        private FreeControllerV3 personHeadControl;
        private Transform lookAtTarget;
        private Vector3 lookAtOffset;
        private Transform head;
        private Transform reference;

        // tweak parameters
        protected float gazeDuration = 0.7f; //0,5
		protected float focusChangeDurationMin = 1.0f; //1,10
		protected float focusChangeDurationMax = 4.0f; //1,10
		protected float focusAngleV = 6.0f; //1,10
		protected float focusAngleH = 4.0f; //1,10
		protected float rollChangeDurationMin = 2.0f; //1,10
		protected float rollChangeDurationMax = 6.0f; //1,10
		protected float rollAngleMax = 6.0f; //1,10

        // runtime data
        private float velocityH = 0.0f;
        private float velocityV = 0.0f;
        private float focusChangeClock = 1.0f;
        private float focusChangeSpeed = 1.0f;
        private Vector2 focusNext = Vector2.zero;
        private Vector2 focusPrev = Vector2.zero;
        private float rollNext = 0.0f;
        private float rollPrev = 0.0f;
        private float rollChangeClock = 1.0f;
        private float rollChangeSpeed = 1.0f;
        private float currentAngle = 0.0f;

        private const float maxAngleH = 90.0f * Mathf.Deg2Rad;
        private const float maxAngleV = 45.0f * Mathf.Deg2Rad;
    }
}
