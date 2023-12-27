using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nebula.UI
{
    public class InputRebinderGenerator : MonoBehaviour
    {
        public InputActionAsset inputAsset;
        public string inputMap;

        public RectTransform container;
        public InputRebinder inputRebinderPrefab;

        public void Start()
        {
            var map = inputAsset.FindActionMap(inputMap);
            if (map == null)
                return;

            SpawnAndAssign(map);
        }

        private void SpawnAndAssign(InputActionMap map)
        {
            foreach(var inputAction in map)
            {
                if (inputAction.processors.Contains(nameof(InputRebinderIgnore)))
                    continue;

                var rebinderInstance = Instantiate(inputRebinderPrefab, container);
                rebinderInstance.ActionReference = InputActionReference.Create(inputAction);
            }
        }
    }
}
