using System;
using DitzelGames.FastIK;
using UnityEngine;

public class IKSystem : MonoBehaviour
{
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

    [Header("Debug area")]
    public IKData leftHandData;
    public IKData rightHandData;
    public IKData leftFootData;
    public IKData rightFootData;

    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;

    private void OnEnable()
    {
        Init();
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
    }

    public void Init()
    {
        if (humanoid && humanoid.avatar && humanoid.avatar.isHuman)
        {
            head = humanoid.GetBoneTransform(HumanBodyBones.Head);
            leftHand = humanoid.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = humanoid.GetBoneTransform(HumanBodyBones.RightHand);
            leftFoot = humanoid.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = humanoid.GetBoneTransform(HumanBodyBones.RightFoot);
            float height = (humanoid.GetBoneTransform(HumanBodyBones.Head).position - humanoid.GetBoneTransform(HumanBodyBones.Hips).position).magnitude;
            float scl = height;

            // left hand
            {
                leftHandData.target = new GameObject("LeftHand Target").transform;
                leftHandData.target.SetParent(transform);
                leftHandData.target.position = leftHand.position;
                leftHandData.target.rotation = leftHand.rotation;
                leftHandData.pole = new GameObject("LeftHand Pole").transform;
                leftHandData.pole.SetParent(transform);
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
                rightHandData.pole.SetParent(transform);
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
                leftFootData.pole.SetParent(transform);
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
                rightFootData.pole.SetParent(transform);
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
