using System;
using DitzelGames.FastIK;
using UnityEngine;

public class IKSystem2 : MonoBehaviour
{
    public enum FootIkState : int
    {
        Idle,
        Moving,
        Timeout,
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
    public bool hipIk = true;
    public bool footIk = true;
    public bool moveFeet = true;

    public float minStepHeight = 0.1f;

    [Min(0f)]
    public float minStepDistance = 0.1f;
    [Min(0f)]
    public float maxStepDistance = 0.25f;
    [Min(0.01f)]
    public float footMoveSpeed = 1f;
    public AnimationCurve footAnimCurve;
    public float timeoutTime = 0.25f;

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
    public float leftFootRest;
    public float rightFootRest;

    public Vector3 hipsPos;

    public Vector3 leftFootCurrentPos;
    public Vector3 leftFootSourcePos;
    public Vector3 leftFootTargetPos;
    public FootIkState leftState = FootIkState.Idle;
    public float leftTimer;
    public Vector3 rightFootCurrentPos;
    public Vector3 rightFootSourcePos;
    public Vector3 rightFootTargetPos;
    public FootIkState rightState = FootIkState.Idle;
    public float rightTimer;

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
        if (hips && head && headTarget && hipIk)
        {
            hips.localPosition = hips.parent.InverseTransformPoint(headTarget.position) - hips.parent.InverseTransformVector(direction);
            head.position = headTarget.position;
            head.rotation = headTarget.rotation;
        }

        Vector3 vel = hips.position - hipsPos;

        if (moveFeet)
        {
            // left
            switch (leftState)
            {
                default:
                case FootIkState.Idle:
                    if (leftFootData.ik.IsOutOfReach || IsFootMoveTime(PlaceLeftFoot(Vector3.zero), leftFootCurrentPos, maxStepDistance))
                    {
                        leftFootSourcePos = leftFootCurrentPos;
                        leftFootTargetPos = PlaceLeftFoot(vel);
                        leftState = FootIkState.Moving;
                        leftTimer = 0f;
                    }
                    break;
                case FootIkState.Moving:
                    if (LerpFoot(ref leftFootCurrentPos, leftFootSourcePos, leftFootTargetPos, ref leftTimer))
                    {
                        leftState = FootIkState.Timeout;
                        leftTimer = 0f;
                    }
                    break;
                case FootIkState.Timeout:
                    leftTimer += Time.deltaTime;
                    if (leftTimer >= timeoutTime)
                    {
                        leftState = FootIkState.Idle;
                        leftTimer = 0f;
                    }
                    break;
            }
            leftFootData.target.position = leftFootCurrentPos;

            // right
            switch (rightState)
            {
                default:
                case FootIkState.Idle:
                    if (rightFootData.ik.IsOutOfReach || IsFootMoveTime(PlaceRightFoot(Vector3.zero), rightFootCurrentPos, maxStepDistance))
                    {
                        rightFootSourcePos = rightFootCurrentPos;
                        rightFootTargetPos = PlaceRightFoot(vel);
                        rightState = FootIkState.Moving;
                        rightTimer = 0f;
                    }
                    break;
                case FootIkState.Moving:
                    if (leftState != FootIkState.Moving && LerpFoot(ref rightFootCurrentPos, rightFootSourcePos, rightFootTargetPos, ref rightTimer))
                    {
                        rightState = FootIkState.Timeout;
                        rightTimer = 0f;
                    }
                    break;
                case FootIkState.Timeout:
                    rightTimer += Time.deltaTime;
                    if (rightTimer >= timeoutTime)
                    {
                        rightState = FootIkState.Idle;
                        rightTimer = 0f;
                    }
                    break;
            }
            rightFootData.target.position = rightFootCurrentPos;
        }

        if (hips)
            hipsPos = hips.position;
    }

    public Vector3 PlaceLeftFoot(Vector3 vel)
    {
        return BaseToWorld(SnapBy2(WorldToBase(humanoid.transform.position), minStepDistance)) - humanoid.transform.right * footDistance * 0.5f + vel;
    }

    public Vector3 PlaceRightFoot(Vector3 vel)
    {
        return BaseToWorld(SnapBy2(WorldToBase(humanoid.transform.position), minStepDistance)) + humanoid.transform.right * footDistance * 0.5f + vel;
    }

    public static Vector2 SnapBy2(Vector2 input, float interval)
    {
        float x = Mathf.Round(input.x / interval) * interval;
        float y = Mathf.Round(input.y / interval) * interval;
        return new Vector2(x, y);
    }

    public static Vector3 SnapBy3(Vector3 input, float interval)
    {
        float x = Mathf.Round(input.x / interval) * interval;
        float y = Mathf.Round(input.y / interval) * interval;
        float z = Mathf.Round(input.z / interval) * interval;
        return new Vector3(x, y, z);
    }

    public Vector2 WorldToBase(Vector3 input)
    {
        return new Vector2(input.x, input.z);
    }

    public Vector3 BaseToWorld(Vector2 input)
    {
        return new Vector3(input.x, humanoid.transform.position.y, input.y);
    }

    public bool CheckFootDot()
    {
        return Mathf.Abs(Vector3.Dot(leftFootCurrentPos - humanoid.transform.position, rightFootCurrentPos - humanoid.transform.position)) > 0.25;
    }

    public bool IsFootMoveTime(Vector3 target, Vector3 current, float maxDist = 0.25f)
    {
        return (WorldToBase(target) - WorldToBase(current)).sqrMagnitude > maxDist * maxDist;
        // return (new Vector2(foot.x, foot.z) - new Vector2(hips.x, hips.z)).sqrMagnitude < maxDist * maxDist;
    }

    public bool LerpFoot(ref Vector3 current, Vector3 source, Vector3 target, ref float t)
    {
        t += footMoveSpeed * Time.deltaTime;
        Vector3 output = BaseToWorld(Vector2.Lerp(WorldToBase(source), WorldToBase(target), t));
        output.y += footAnimCurve.Evaluate(t) * minStepHeight;
        current = output;
        return t >= 1f;
    }

    public bool LerpFootOld(ref Vector3 current, Vector3 target, float delta)
    {
        current = Vector3.MoveTowards(current, target, delta);
        return (current - target).sqrMagnitude < 0.01f * 0.01f;
    }

    public void Init()
    {
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
            leftFootRest = Vector3.Distance(hips.position, leftFoot.position);
            rightFootRest = Vector3.Distance(hips.position, rightFoot.position);

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
            }
        }
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
