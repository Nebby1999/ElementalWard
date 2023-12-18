using System.Collections.ObjectModel;
using System.Text;
using TMPro;
using UnityEngine;
using Nebula.Console;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using System.ComponentModel;
using UnityEngine.AddressableAssets;

namespace Nebula.UI
{
    public class ConsoleWindow : MonoBehaviour
    {
        public TextMeshProUGUI outputView;
        public TMP_InputField inputField;

        public GameObject consoleObject;
        private StringBuilder stringBuilder = new StringBuilder();
        private NebulaConsoleCommand keyCombo;
        private void Awake()
        {
            ConsoleSystem.OnLogChanged += OutputLog;
            keyCombo = new NebulaConsoleCommand();
            keyCombo.Enable();
            keyCombo.Nebula.OpenConsole.started += (arg) =>
            {
                consoleObject.SetActive(!consoleObject.activeSelf);
            };
            inputField.onEndEdit.AddListener(TryToExecute);
        }

        private void TryToExecute(string text)
        {
            string[] args = text.Split(' ');
            if (args.Length == 0)
            {
                inputField.SetTextWithoutNotify(string.Empty);
                return;
            }

            var commandName = args[0];
            ArrayUtils.RemoveAtAndResize(ref args, 0, 1);

            ConsoleSystem.ExecuteCommand(commandName, args.ToArray());
            inputField.SetTextWithoutNotify(string.Empty);
        }

        private void OutputLog(ReadOnlyCollection<ConsoleSystem.Log> collection)
        {
            if (!outputView)
                return;

            stringBuilder.Clear();
            foreach(var log in collection)
            {
                stringBuilder.AppendLine(log.ToString());
            }

            outputView.SetText(stringBuilder);
        }

        public void OnDestroy()
        {
            ConsoleSystem.OnLogChanged -= OutputLog;
            keyCombo.Disable();
            keyCombo.Dispose();
        }
    }
}