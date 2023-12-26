using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Nebula.UI
{
    public class InputRebinder : MonoBehaviour
    {

        public InputActionReference ActionReference
        {
            get => _actionReference;
            set
            {
                _actionReference = value;
                if(value.action != null)
                {
                    BindingID = value.action.bindings[0].id.ToString();
                }
                UpdateBindingText();
                UpdateActionLabel();
            }
        }
        [SerializeField, ReadOnly]
        private InputActionReference _actionReference;

        public string BindingID
        {
            get => _bindingID;
            set
            {
                _bindingID = value;
                UpdateBindingText();
            }
        }
        [SerializeField, ReadOnly]
        private string _bindingID;

        public InputBinding.DisplayStringOptions DisplayStringOptions
        {
            get => _displayStringOptions;
            set
            {
                _displayStringOptions = value;
                UpdateBindingText();
            }
        }
        [SerializeField] private InputBinding.DisplayStringOptions _displayStringOptions;

        public TextMeshProUGUI ActionLabel
        {
            get => _actionLabel;
            set
            {
                _actionLabel = value;
                UpdateActionLabel();
            }
        }
        public TextMeshProUGUI _actionLabel;

        public TextMeshProUGUI BindingLabel
        {
            get => _bindingText;
            set
            {
                _bindingText = value;
                UpdateBindingText();
            }
        }
        public TextMeshProUGUI _bindingText;

        [SerializeField] GameObject _rebindOverlay;
        [SerializeField] TextMeshProUGUI _rebindText;
        
        [SerializeField] private UpdateBindingUIEvent _updateBindingUIEvent;
        [SerializeField] private InteractiveRebindEvent _startRebindEvent;
        [SerializeField] private InteractiveRebindEvent _stopRebindEvent;

        public InputActionRebindingExtensions.RebindingOperation OngoingRebind => _rebindOperation;
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private void UpdateActionLabel()
        {
            if (!_actionLabel)
                return;

            var action = ActionReference?.action;
            _actionLabel.text = action != null ? action.name : string.Empty;
        }

        private void UpdateBindingText()
        {
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            var action = _actionReference?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == _bindingID);
                if (bindingIndex != -1)
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, _displayStringOptions);
            }

            if (_bindingText != null)
                _bindingText.text = displayString;

            _updateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        protected void OnEnable()
        {
            InstanceTracker.Add(this);
            if(InstanceTracker.GetInstances<InputRebinder>().Count == 1)
            {
                InputSystem.onActionChange += OnActionChange;
            }
        }

        protected void OnDisable()
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;

            InstanceTracker.Remove(this);
            if (InstanceTracker.GetInstances<InputRebinder>().Count == 0)
            {
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = _actionReference?.action;
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(_bindingID))
                return false;

            // Look up binding index.
            var bindingId = new Guid(_bindingID);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        public void ResetToDefault()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }
            UpdateBindingText();

            var map = action.actionMap;
            if(map != null)
            {
                string json = map.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString("PlayerInputOverrides", json);
            }
        }

        public void StartInteractiveRebind()
        {
            if (!ResolveActionAndBinding(out var action, out var bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            _rebindOperation?.Cancel();

            void CleanUp()
            {
                _rebindOperation?.Dispose();
                _rebindOperation = null;
            }

            action.Disable();

            _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithExpectedControlType(action.expectedControlType)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(
                    operation =>
                    {
                        action.Enable();
                        _stopRebindEvent?.Invoke(this, operation);
                        _rebindOverlay.AsValidOrNull()?.SetActive(false);
                        UpdateBindingText();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        action.Enable();
                        _rebindOverlay.AsValidOrNull()?.SetActive(false);


                        if(CheckDuplicateBindings(action, bindingIndex, allCompositeParts))
                        {
                            action.RemoveBindingOverride(bindingIndex);
                            CleanUp();
                            PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                            return;
                        }

                        UpdateBindingText();
                        CleanUp();

                        if (allCompositeParts)
                        {
                            var nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }

                        _stopRebindEvent?.Invoke(this, operation);
                    });

            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            _rebindOverlay.AsValidOrNull()?.SetActive(true);
            if (_rebindText != null)
            {
                var text = !string.IsNullOrEmpty(_rebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {_rebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                _rebindText.text = text;
            }

            if (_rebindOverlay == null && _rebindText == null && _startRebindEvent == null && _bindingText != null)
                _bindingText.text = "<Waiting...>";

            _startRebindEvent?.Invoke(this, _rebindOperation);

            _rebindOperation.Start();
        }

        private bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts)
        {
            InputBinding newBinding = action.bindings[bindingIndex];

            foreach (InputBinding binding in action.actionMap.bindings)
            {
                if (binding.action == newBinding.action)
                    continue;

                if(binding.effectivePath == newBinding.effectivePath)
                {
                    return true;
                }
            }

            //Composite Check
            if(allCompositeParts)
            {
                for(int i = 1; i < bindingIndex; i++)
                {
                    if (action.bindings[i].effectivePath == newBinding.effectivePath)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            var instances = InstanceTracker.GetInstances<InputRebinder>();
            for (var i = 0; i < instances.Count; ++i)
            {
                var component = instances[i];
                var referencedAction = component.ActionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingText();
            }
        }

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<InputRebinder, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<InputRebinder, InputActionRebindingExtensions.RebindingOperation>
        {
        }
    }
}
   