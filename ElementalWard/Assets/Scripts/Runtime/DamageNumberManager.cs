using Nebula;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.LowLevel;

namespace ElementalWard
{
    public class DamageNumberManager : MonoBehaviour
    {
        private static GameObjectPool pool;
        private static Transform lookAtTransform;

        public TextMeshPro textMesh;
        public float lifeTime;
        public AnimationCurve speedCurve;
        public AnimationCurve sizeCurve;

        private float stopwatch;
        public static GameObject Spawn(float dmgAmount, Vector3 pos, Color32 color, bool isHealing)
        {
            if (pool == null)
                pool = new GameObjectPool(Addressables.LoadAssetAsync<GameObject>("ElementalWard/Base/DamageNumberManager.prefab").WaitForCompletion());

            var instance = pool.Request();
            SetupInstance(instance, dmgAmount, pos, color, isHealing);
            instance.SetActive(true);
            return instance;
        }

        private static void SetupInstance(GameObject instance, float amount, Vector3 pos, Color32 color, bool isHealing)
        {
            if (!lookAtTransform)
                lookAtTransform = UnityUtil.MainCamera.transform;
            instance.transform.position = pos;
            instance.transform.localScale = Vector3.one;
            DamageNumberManager componentInstance = instance.GetComponent<DamageNumberManager>();
            componentInstance.textMesh.color = color;

            char magnitude = char.MinValue;
            float displayedValue = 0;
            if (amount > 1_000_000)
            {
                displayedValue /= 1_000_000;
                magnitude = 'M';
            }
            else if (amount > 1_000)
            {
                displayedValue /= 1_000;
                magnitude = 'K';
            }
            else
            {
                displayedValue = amount;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(isHealing ? '+' : '-');
            stringBuilder.Append(displayedValue.ToString("#.#"));
            if(magnitude != char.MinValue)
            {
                stringBuilder.Append(magnitude);
            }
            componentInstance.textMesh.text = stringBuilder.ToString();
            componentInstance.stopwatch = 0;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            stopwatch += deltaTime;
            var num = stopwatch / lifeTime;
            var translation = Vector3.up * speedCurve.Evaluate(num);
            var newsScale = Vector3.one * sizeCurve.Evaluate(num);
            transform.Translate(translation * deltaTime);
            transform.LookAt(lookAtTransform.position);
            transform.localScale = newsScale;
            if(stopwatch > lifeTime)
            {
                pool.Return(gameObject);
            }
        }
    }
}
