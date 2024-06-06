using System;
using DitzelGames.FastIK;
using UnityEngine;

public class IKSystem : MonoBehaviour
{
    public enum IKLoopState
    {
        Apex,
        Buildup,
        Contact,
        FollowThru,
        ThruApex,
    }

    [Serializable]
    public struct IKData
    {
        public FastIKFabric ik;
        public Transform target;
        public Transform pole;
    }

    [Header("Settings")]
    public Animator humanoid;
    [Range(0f, 1f)]
    public float SnapBackStrength = 0.5f;
    public bool handIk = true;
    public bool footIk = true;
    public bool moveFeet = true;

    public float minStepHeight = 0.1f;
    public float maxStepHeight = 0.2f;

    public float minStepLength = -0.5f;
    public float maxStepLength = 0.5f;

    public float loopSize = 1f;

    [Min(0f)]
    public float stepDistance = 0.2f;
    [Min(0f)]
    public float reachDistance = 0.2f;
    [Min(0.01f)]
    public float footMoveSpeed = 1f;
    public AnimationCurve velCurve;

    [Header("Debug area")]
    public IKData leftHandData;
    public IKData rightHandData;
    public IKData leftFootData;
    public IKData rightFootData;

    public Transform head;
    public Transform headTarget;
    public Vector3 direction;
    public Transform hips;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;

    public float footDistance;

    public Vector3 leftFootPos;
    public Vector3 rightFootPos;
    public bool flipFlop = false;
    public float flipDist => flipFlop ? 1f : 1f;

    public Vector3 hipsPos;
    public float timer;
    public float velTimer;
    public float leftTimer;
    public float rightTimer;

    public IKLoopState leftLoopState;
    public IKLoopState rightLoopState;

    private void OnEnable()
    {
        Init();
    }

    private static float MapValue(float value, float min1, float max1, float min2, float max2)
    {
        return (value - min1) / (max1 - min1) * (max2 - min2) + min2;
    }

    private void LateUpdate()
    {
        if (leftHandData.ik)
            leftHandData.ik.enabled = handIk;
        if (rightHandData.ik)
            rightHandData.ik.enabled = handIk;
        if (leftFootData.ik)
            leftFootData.ik.enabled = footIk;
        if (rightFootData.ik)
            rightFootData.ik.enabled = footIk;
        if (hips && head && headTarget)
        {
            hips.localPosition = hips.parent.InverseTransformPoint(headTarget.position) - hips.parent.InverseTransformVector(direction);
            head.position = headTarget.position;
            head.rotation = headTarget.rotation;
        }

        Vector3 vel = hips.position - hipsPos;
#if false
        bool found = false;
        if (leftFootData.target && leftFootData.ik)
        {
            var scl = humanoid.transform.right + humanoid.transform.forward;
            var footPos = Vector3.Scale(leftFootData.target.position, scl);
            var otherFootPos = Vector3.Scale(rightFootData.target.position, scl);
            var hipsPos = Vector3.Scale(hips.position, scl);
            var dir = hipsPos - footPos;
            var otherDir = hipsPos - otherFootPos;
            float amount = 1f;
            // if (Vector3.Dot(dir, humanoid.transform.right) < 0f)
            //     amount = 2f;
            if (leftFootData.ik.ReachAmountSqr >= Mathf.Pow(reachDistance * leftFootData.ik.CompleteLength, 2f) /*&& timer <= 0f*/ && flipFlop)
            // if (leftFootData.ik.IsOutOfReach && /*timer <= 0f &&*/ flipFlop)
            {
                found = true;
                leftTimer = 0.1f;
                timer = 0.1f;
                leftFootPos = leftFootData.target.position + dir.normalized * (stepDistance * amount * leftFootData.ik.CompleteLength * velCurve.Evaluate(velTimer));
                flipFlop = !flipFlop;
            }
            leftFootData.target.position = Vector3.Lerp(leftFootData.target.position, leftFootPos, Time.deltaTime * footMoveSpeed);
            if ((leftFootData.target.position - leftFootPos).sqrMagnitude >= 0.1f * 0.1f)
            {
                leftTimer = 0.1f;
                timer = 0.1f;
            }
        }
        if (rightFootData.target && rightFootData.ik)
        {
            var scl = humanoid.transform.right + humanoid.transform.forward;
            var footPos = Vector3.Scale(rightFootData.target.position, scl);
            var otherFootPos = Vector3.Scale(leftFootData.target.position, scl);
            var hipsPos = Vector3.Scale(hips.position, scl);
            var dir = hipsPos - footPos;
            float amount = 1f;
            // if (Vector3.Dot(dir, humanoid.transform.right) > 0f)
            //     amount = 2f;
            if (rightFootData.ik.ReachAmountSqr >= Mathf.Pow(reachDistance * rightFootData.ik.CompleteLength, 2f) /*&& timer <= 0f*/ && !flipFlop && !found)
            // if (rightFootData.ik.IsOutOfReach && /*timer <= 0f &&*/ !flipFlop && !found)
            {
                rightTimer = 0.1f;
                timer = 0.1f;
                rightFootPos = rightFootData.target.position + dir.normalized * (stepDistance * amount * rightFootData.ik.CompleteLength * velCurve.Evaluate(velTimer));
                flipFlop = !flipFlop;
            }
            rightFootData.target.position = Vector3.Lerp(rightFootData.target.position, rightFootPos, Time.deltaTime * footMoveSpeed);
            if ((rightFootData.target.position - rightFootPos).sqrMagnitude >= 0.1f * 0.1f)
            {
                rightTimer = 0.1f;
                timer = 0.1f;
            }
        }
        if (timer > 0f)
            timer -= Time.deltaTime;
        if (leftTimer > 0f)
            leftTimer -= Time.deltaTime;
        if (rightTimer > 0f)
            rightTimer -= Time.deltaTime;
        // if (velTimer > 0f)
            velTimer += Time.deltaTime;
        if (vel.magnitude > Time.deltaTime * 0.1f)
            velTimer = 0f;
#endif

#if false
        if (moveFeet)
        {
            timer += vel.magnitude;
            float speed = vel.magnitude / Time.deltaTime;

            // left foot
            {
                float y = MapValue(Mathf.Sin(timer + Mathf.Deg2Rad * 180f), -1f, 1f, minStepHeight * speed, maxStepHeight * speed);
                float z = MapValue(-Mathf.Cos(timer + Mathf.Deg2Rad * 180f), -1f, 1f, minStepLength * speed, maxStepLength * speed);

                leftFootData.target.localPosition = new Vector3(footDistance * -0.5f, y, z);
            }
            // right foot
            {
                float y = MapValue(Mathf.Sin(timer), -1f, 1f, minStepHeight * speed, maxStepHeight * speed);
                float z = MapValue(-Mathf.Cos(timer), -1f, 1f, minStepLength * speed, maxStepLength * speed);

                rightFootData.target.localPosition = new Vector3(footDistance * 0.5f, y, z);
            }
        }
#endif

        if (moveFeet)
        {
            float speed = vel.magnitude;
            // speed = 1f;
            {
                Vector2 pos = EvaluateLoop(ref leftLoopState, ref leftTimer, loopSize, speed, 0f);
                float y = MapValue(pos.y, 0f, 1f, minStepHeight, maxStepHeight);
                float z = MapValue(pos.x, -1f, 1f, minStepLength, maxStepLength);

                Vector3 end = Vector3.zero;
                if (vel.magnitude > 0f)
                    end = Quaternion.LookRotation(humanoid.transform.InverseTransformDirection(vel.normalized)) * new Vector3(0f, y, z);
                end.x += -footDistance * 0.5f;
                leftFootData.target.localPosition = end;
            }
            {
                Vector2 pos = EvaluateLoop(ref rightLoopState, ref rightTimer, loopSize, speed, 0.5f);
                float y = MapValue(pos.y, 0f, 1f, minStepHeight, maxStepHeight);
                float z = MapValue(pos.x, -1f, 1f, minStepLength, maxStepLength);

                Vector3 end = Vector3.zero;
                if (vel.magnitude > 0f)
                    end = Quaternion.LookRotation(humanoid.transform.InverseTransformDirection(vel.normalized)) * new Vector3(0f, y, z);
                end.x += footDistance * 0.5f;
                rightFootData.target.localPosition = end;
            }
        }

        if (hips)
            hipsPos = hips.position;
    }

    public void Init()
    {
        leftLoopState = IKLoopState.Apex;
        rightLoopState = IKLoopState.Contact;
        leftTimer = 0f;
        rightTimer = 0f;
        OnDisable();
        if (humanoid && humanoid.avatar && humanoid.avatar.isHuman)
        {
            head = humanoid.GetBoneTransform(HumanBodyBones.Head);
            hips = humanoid.GetBoneTransform(HumanBodyBones.Hips);
            leftHand = humanoid.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = humanoid.GetBoneTransform(HumanBodyBones.RightHand);
            leftFoot = humanoid.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = humanoid.GetBoneTransform(HumanBodyBones.RightFoot);
            direction = humanoid.GetBoneTransform(HumanBodyBones.Head).position - humanoid.GetBoneTransform(HumanBodyBones.Hips).position;
            float scl = direction.magnitude;
            footDistance = Vector3.Distance(leftFoot.position, rightFoot.position);

            // hips and head
            {
                headTarget = new GameObject("Head Target").transform;
                headTarget.SetParent(transform);
                headTarget.position = head.position;
                headTarget.rotation = head.rotation;
                hipsPos = hips.position;
            }

            // left hand
            {
                leftHandData.target = new GameObject("LeftHand Target").transform;
                leftHandData.target.SetParent(transform);
                leftHandData.target.position = leftHand.position;
                leftHandData.target.rotation = leftHand.rotation;
                leftHandData.pole = new GameObject("LeftHand Pole").transform;
                leftHandData.pole.SetParent(head);
                leftHandData.pole.position = head.position - transform.right * scl - transform.forward * scl + transform.up * 0.5f * scl;

                FastIKFabric ik = leftHand.gameObject.AddComponent<FastIKFabric>();
                ik.Target = leftHandData.target;
                ik.Pole = leftHandData.pole;
                if (humanoid.GetBoneTransform(HumanBodyBones.LeftShoulder))
                    ik.ChainLength = 3;
                // else
                    ik.ChainLength = 2;
                ik.SnapBackStrength = SnapBackStrength;
                ik.Init();
                leftHandData.ik = ik;
            }
            // right hand
            {
                rightHandData.target = new GameObject("RightHand Target").transform;
                rightHandData.target.SetParent(transform);
                rightHandData.target.position = rightHand.position;
                rightHandData.target.rotation = rightHand.rotation;
                rightHandData.pole = new GameObject("RightHand Pole").transform;
                rightHandData.pole.SetParent(head);
                rightHandData.pole.position = head.position + transform.right * scl - transform.forward * scl + transform.up * 0.5f * scl;

                FastIKFabric ik = rightHand.gameObject.AddComponent<FastIKFabric>();
                ik.Target = rightHandData.target;
                ik.Pole = rightHandData.pole;
                if (humanoid.GetBoneTransform(HumanBodyBones.RightShoulder))
                    ik.ChainLength = 3;
                // else
                    ik.ChainLength = 2;
                ik.SnapBackStrength = SnapBackStrength;
                ik.Init();
                rightHandData.ik = ik;
            }

            // left foot
            {
                leftFootData.target = new GameObject("LeftFoot Target").transform;
                leftFootData.target.SetParent(transform);
                leftFootData.target.position = leftFoot.position;
                leftFootData.target.rotation = leftFoot.rotation;
                leftFootData.pole = new GameObject("LeftFoot Pole").transform;
                leftFootData.pole.SetParent(hips);
                leftFootData.pole.position = humanoid.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position - transform.right * 0.25f * scl + transform.forward * 1.5f * scl + transform.up * 0.9f * scl;

                FastIKFabric ik = leftFoot.gameObject.AddComponent<FastIKFabric>();
                ik.Target = leftFootData.target;
                ik.Pole = leftFootData.pole;
                ik.ChainLength = 2;
                ik.SnapBackStrength = SnapBackStrength;
                ik.Init();
                leftFootData.ik = ik;
                leftFootPos = leftFoot.position;
            }
            // right foot
            {
                rightFootData.target = new GameObject("RightFoot Target").transform;
                rightFootData.target.SetParent(transform);
                rightFootData.target.position = rightFoot.position;
                rightFootData.target.rotation = rightFoot.rotation;
                rightFootData.pole = new GameObject("RightFoot Pole").transform;
                rightFootData.pole.SetParent(hips);
                rightFootData.pole.position = humanoid.GetBoneTransform(HumanBodyBones.RightUpperLeg).position + transform.right * 0.25f * scl + transform.forward * 1.5f * scl + transform.up * 0.9f * scl;

                FastIKFabric ik = rightFoot.gameObject.AddComponent<FastIKFabric>();
                ik.Target = rightFootData.target;
                ik.Pole = rightFootData.pole;
                ik.ChainLength = 2;
                ik.SnapBackStrength = SnapBackStrength;
                ik.Init();
                rightFootData.ik = ik;
                rightFootPos = rightFoot.position;
            }
        }
    }

    // https://github.com/MMMaellon/renik/blob/master/RenIK%20Foot%20Placement.png
    private Vector2 EvaluateLoop(ref IKLoopState state, ref float offset, float speed, float dt, float phase)
    {
        float realOffset = offset + phase;
        Vector2 apexPt = new Vector2(speed, speed * 0.7f);
        Vector2 buildupPt = new Vector2(speed * 0.8f, speed * 0.5f);
        Vector2 contactPt = new Vector2(speed * 0.3f, 0f);
        Vector2 followPt = new Vector2(-speed * 0.1f, 0f);
        Vector2 thruPt = new Vector2(-speed, speed * 0.4f);
        Vector2 point = Vector2.zero;
        switch (state)
        {
            case IKLoopState.Apex:
                point = Vector2.Lerp(apexPt, buildupPt, realOffset);
                offset += dt * 1.5f;
                break;
            case IKLoopState.Buildup:
                point = Vector2.Lerp(buildupPt, contactPt, realOffset);
                offset += dt * 2f;
                break;
            case IKLoopState.Contact:
                point = Vector2.Lerp(contactPt, thruPt, realOffset);
                // point = Vector2.Lerp(contactPt, followPt, realOffset);
                offset += dt * 2f;
                break;
            case IKLoopState.FollowThru:
                point = Vector2.Lerp(followPt, thruPt, realOffset);
                offset += dt * 0.5f;
                break;
            case IKLoopState.ThruApex:
                point = Vector2.Lerp(thruPt, apexPt, realOffset);
                offset += dt;
                break;
        }
        if (offset + phase >= 1f)
        {
            offset = -phase;
            switch (state)
            {
                case IKLoopState.Apex:
                    state = IKLoopState.Buildup;
                    break;
                case IKLoopState.Buildup:
                    state = IKLoopState.Contact;
                    break;
                case IKLoopState.Contact:
                    state = IKLoopState.ThruApex;
                    break;
                case IKLoopState.FollowThru:
                    state = IKLoopState.ThruApex;
                    break;
                case IKLoopState.ThruApex:
                    state = IKLoopState.Apex;
                    break;
            }
        }
        return point;
    }

    private void OnDisable()
    {
        void DestroyIKData(IKData data)
        {
            if (data.target)
                DestroyImmediate(data.target.gameObject);
            if (data.pole)
                DestroyImmediate(data.pole.gameObject);
            if (data.ik)
                DestroyImmediate(data.ik);
        }
        DestroyIKData(leftHandData);
        DestroyIKData(rightHandData);
        DestroyIKData(leftFootData);
        DestroyIKData(rightFootData);
    }

    private void OnDrawGizmosSelected()
    {
        void DrawIKData(IKData data)
        {
            Gizmos.color = Color.red;
            if (data.target)
                Gizmos.DrawSphere(data.target.position, 0.1f);
            Gizmos.color = Color.blue;
            if (data.pole)
                Gizmos.DrawSphere(data.pole.position, 0.1f);
        }
        DrawIKData(leftHandData);
        DrawIKData(rightHandData);
        DrawIKData(leftFootData);
        DrawIKData(rightFootData);
    }
}
