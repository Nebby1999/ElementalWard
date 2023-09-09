using Nebula;
using UnityEditor;
using UnityEngine;

namespace ElementalWard
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private bool isOpen;
        [Header("Wall Settings")]
        [ForcePrefab, SerializeField] private GameObject wallPrefab;
        [SerializeField] private Vector3 wallPositionOffset;
        [SerializeField] private Quaternion wallRotationOffset;

        [SerializeField, HideInInspector] private GameObject wallInstance;

        private void OnValidate()
        {
            if(!wallInstance && wallPrefab)
            {
                wallInstance = Instantiate(wallPrefab);
                wallInstance.transform.localPosition += wallPositionOffset;
                wallInstance.transform.localRotation = wallRotationOffset;
            }

            if (!wallInstance)
                return;

            wallInstance.SetActive(!isOpen);
        }

#if UNITY_EDITOR
        private void DestroyWall()
        {
            EditorApplication.update -= DestroyWall;
            DestroyImmediate(wallInstance);
        }
#endif
    }
}