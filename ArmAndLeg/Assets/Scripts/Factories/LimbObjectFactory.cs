﻿using System;
using System.Linq;

using Factories;

using Items;

using UnityEngine;

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class LimbObjectFactory
{
    private static LimbObjectFactorySettings s_Settings;

    private static LimbObjectFactorySettings settings
    {
        get
        {
            if (s_Settings)
                return s_Settings;

            Random.InitState(DateTime.Now.Millisecond);
            s_Settings = Resources.LoadAll<LimbObjectFactorySettings>("Settings").First();

            return s_Settings;
        }
    }

    [Flags]
    public enum PhysicsType
    {
        None = 0,

        Translate = 1 << 0,
        Rotate = 1 << 1,
    }

    public static GameObject CreateObject<T>(
        T limb,
        Vector3 position,
        PhysicsType physicsType = PhysicsType.Translate | PhysicsType.Rotate) where T : Limb
    {
        var newLimbObject =
            Object.Instantiate(settings.limbPrefab, position, Quaternion.identity) as GameObject;

        if (!newLimbObject)
            return null;

        EnemyLimbBehaviour matchingEnemyLimb = null;
        foreach (var enemyLimb in Object.FindObjectsOfType<EnemyLimbBehaviour>())
            if (enemyLimb.limb == limb)
                matchingEnemyLimb = enemyLimb;

        if (matchingEnemyLimb)
        {
            newLimbObject.transform.position = matchingEnemyLimb.transform.position;
            newLimbObject.transform.rotation = matchingEnemyLimb.transform.rotation;
            newLimbObject.transform.localScale = matchingEnemyLimb.transform.localScale;
        }

        var rigidbody = newLimbObject.GetComponent<Rigidbody2D>();
        if (rigidbody)
        {
            if ((physicsType & PhysicsType.Translate) != 0)
                rigidbody.AddForce(
                    new Vector3(
                        GetTranslationValue(),
                        GetTranslationValue(),
                        0f));

            if ((physicsType & PhysicsType.Rotate) != 0)
                rigidbody.AddTorque(GetRotationValue());
        }

        var spriteRenderer = newLimbObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            if (matchingEnemyLimb)
            {
                var matchingSpriteRenderer = matchingEnemyLimb.GetComponent<SpriteRenderer>();
                if (matchingSpriteRenderer)
                {
                    spriteRenderer.sprite = matchingSpriteRenderer.sprite;

                    spriteRenderer.flipX = matchingSpriteRenderer.flipX;
                    spriteRenderer.flipY = matchingSpriteRenderer.flipY;
                }
            }
            else
            {
                if (limb is Arm)
                    spriteRenderer.sprite = settings.defaultArmSprite;
                else if (limb is Leg)
                    spriteRenderer.sprite = settings.defaultLegSprite;
            }
        }

        var collider2D = newLimbObject.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        var newLimbBehaviour = newLimbObject.AddComponent<LimbBehaviour>();
        newLimbBehaviour.Init(limb, settings.aliveTime);

        return newLimbObject;
    }

    private static float GetTranslationValue()
    {
        var isNegative = (int)Mathf.Round(Random.value) == 1;

        var negativeCoefficient = Random.Range(-1, 2);

        return
             Random.Range(settings.translationRandomMin, settings.translationRandomMax)
            * negativeCoefficient;
    }

    private static float GetRotationValue()
    {
        var isNegative = (int)Mathf.Round(Random.value) == 1;

        var negativeCoefficient = isNegative ? -1 : 1;

        return
            Random.Range(settings.rotationRandomMin, settings.rotationRandomMax)
            * negativeCoefficient;
    }

    public static GameObject CreateWeapon<T>(
        GameObject owner,
        T limb) where T : Limb
    {
        var newLimbWeapon =
            Object.Instantiate(
                settings.limbWeaponPrefab, owner.transform.position, Quaternion.identity) as GameObject;

        var spriteRenderer = newLimbWeapon.GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            if (limb is Arm)
                spriteRenderer.sprite = settings.deafultArmWeaponSprite;
            if (limb is Leg)
                spriteRenderer.sprite = settings.deafultLegWeaponSprite;
        }

        var collider2D = newLimbWeapon.AddComponent<BoxCollider2D>();
        collider2D.isTrigger = true;

        var weaponBehaviour = newLimbWeapon.GetComponent<LimbWeaponBehaviour>();
        if (!weaponBehaviour)
            return null;

        weaponBehaviour.Init(owner, limb);

        return newLimbWeapon;
    }
}
