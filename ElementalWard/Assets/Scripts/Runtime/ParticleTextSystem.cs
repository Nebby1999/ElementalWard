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
    //https://www.gamedeveloper.com/programming/how-to-display-in-game-messages-with-unity-particle-system#close-modal
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleTextSystem : MonoBehaviour
    {
        [Serializable]
        public struct SymbolsTextureData
        {
            public Texture texture;
            public char[] chars;

            private Dictionary<char, Vector2> charDictionary;

            public void Initialize()
            {
                charDictionary = new Dictionary<char, Vector2>();
                for(int i = 0; i < chars.Length; i++)
                {
                    var c = char.ToLowerInvariant(chars[i]);
                    if (charDictionary.ContainsKey(c))
                        continue;

                    var x = i % 4;
                    var y = 3 - i / 4;
                    var uv = new Vector2(x,y);
                    charDictionary.Add(c, uv);
                }
            }

            public Vector2 GetTextureCoordinates(char c)
            {
                c = char.ToLowerInvariant(c);
                if (charDictionary == null)
                    Initialize();

                if(charDictionary.TryGetValue(c, out Vector2 texCoord))
                {
                    return texCoord;
                }
                return Vector2.zero;
            }
        }
        public const string DEFAULT_PREFAB_ADDRESS = "ElementalWard/Base/ParticleTextSystem.prefab";

        [SerializeField] private SymbolsTextureData symbols;
        private ParticleSystemRenderer _particleSystemRenderer;
        private ParticleSystem _particleSystem;

        private static ParticleTextSystem defaultInstance;
        private static string[] magnitudeChars = new string[]
        {
            "",
            "K",
            "M"
        };

        [ContextMenu("TestText")]
        private void TestText()
        {
            var text = FormatDamage(10234, false);
            Debug.Log($"Testing: \"{text}\"");
            this.SpawnParticle(transform.position, text, Color.white);
        }
        public static string FormatDamage(float damage, bool isHealing)
        {
            int index = (int)Mathf.Clamp(0f, Mathf.Floor(Mathf.Log10(damage) / 3), (float)(magnitudeChars.Length - 1));
            string magnitude = magnitudeChars[index];
            float displayedValue = damage / Mathf.Pow(10f, index * 3);
            return $"{(isHealing ? "+" : "-")}{displayedValue}{magnitude}";
        }
        public static void SpawnParticle(Vector3 position, string message, Color color, ParticleTextSystem instance = null)
        {
            if (!instance)
            {
                if (!defaultInstance)
                {
                    var prefab = Addressables.LoadAssetAsync<GameObject>(DEFAULT_PREFAB_ADDRESS).WaitForCompletion();
                    defaultInstance = Instantiate(prefab).GetComponent<ParticleTextSystem>();
                }
                instance = defaultInstance;
            }
            instance.SpawnParticle(position, message, color);
        }
        //Supplementing the body of the particle spawn method
        public void SpawnParticle(Vector3 position, string message, Color color)
        {
            var texCords = new Vector2[24];
            //an array of 24 elements - 23 symbols + the length of the mesage
            var messageLenght = Mathf.Min(23, message.Length);
            texCords[texCords.Length - 1] = new Vector2(0, messageLenght);
            for (int i = 0; i < texCords.Length; i++)
            {
                if (i >= messageLenght) break;
                //Calling the method GetTextureCoordinates() from SymbolsTextureData to obtain the symbol's position
                texCords[i] = symbols.GetTextureCoordinates(message[i]);
            }

            var custom1Data = CreateCustomData(texCords);
            var custom2Data = CreateCustomData(texCords, 12);

            var emitParams = new ParticleSystem.EmitParams
            {
                startColor = color,
                position = position,
                applyShapeToPosition = true,
                startSize3D = new Vector3(messageLenght, 1, 1)
            };

            _particleSystem.Emit(emitParams, 1);

            var customData = new List<Vector4>();
            _particleSystem.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
            customData[customData.Count - 1] = custom1Data;
            _particleSystem.SetCustomParticleData(customData, ParticleSystemCustomData.Custom1);


            _particleSystem.GetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
            customData[customData.Count - 1] = custom2Data;
            _particleSystem.SetCustomParticleData(customData, ParticleSystemCustomData.Custom2);
        }
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        }
        //Vector2 array packing function with symbols' coordinates in "float" 
        private float PackFloat(Vector2[] vecs)
        {
            if (vecs == null || vecs.Length == 0) return 0;
            //Bitwise adding the coordinates of the vectors in float
            var result = vecs[0].y * 10000 + vecs[0].x * 100000;
            if (vecs.Length > 1) result += vecs[1].y * 100 + vecs[1].x * 1000;
            if (vecs.Length > 2) result += vecs[2].y + vecs[2].x * 10;
            return result;
        }

        //Create Vector4 function for the stream with CustomData
        private Vector4 CreateCustomData(Vector2[] texCoords, int offset = 0)
        {
            var data = Vector4.zero;
            for (int i = 0; i < 4; i++)
            {
                var vecs = new Vector2[3];
                for (int j = 0; j < 3; j++)
                {
                    var ind = i * 3 + j + offset;
                    if (texCoords.Length > ind)
                    {
                        vecs[j] = texCoords[ind];
                    }
                    else
                    {
                        data[i] = PackFloat(vecs) + 0.1f;
                        i = 5;
                        break;
                    }
                }
                if (i < 4) data[i] = PackFloat(vecs) + 0.1f;
            }
            return data;
        }
    }
}
