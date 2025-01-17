using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using YARG.Core.Input;
using YARG.Helpers;
using YARG.Menu.Navigation;
using YARG.Settings;

namespace YARG.Menu.Settings
{
    public class SettingsButton : NavigatableBehaviour
    {
        [SerializeField]
        private GameObject _buttonTemplate;

        [Space]
        [SerializeField]
        private NavigationGroup _navGroup;
        [SerializeField]
        private Transform _container;

        private bool _focused;

        public void SetInfo(string tab, IEnumerable<string> buttons)
        {
            // Spawn button(s)
            foreach (var buttonName in buttons)
            {
                var button = Instantiate(_buttonTemplate, _container);

                // Set button text
                button.GetComponentInChildren<LocalizeStringEvent>().StringReference =
                    LocaleHelper.StringReference("Settings", $"Button.{tab}.{buttonName}");

                // Set button action
                var capture = buttonName;
                button.GetComponentInChildren<Button>()
                    .onClick.AddListener(() => SettingsManager.InvokeButton(capture));

                // Add to nav group
                _navGroup.AddNavigatable(button.GetComponentInChildren<NavigatableUnityButton>());
            }

            // Remove the template
            Destroy(_buttonTemplate);
        }

        public void SetCustomCallback(Action action, string localizationKey)
        {
            _buttonTemplate.GetComponentInChildren<LocalizeStringEvent>().StringReference =
                LocaleHelper.StringReference("Settings", localizationKey);

            _buttonTemplate.GetComponentInChildren<Button>().onClick.AddListener(() => action?.Invoke());
        }

        public override void Confirm()
        {
            var scheme = new NavigationScheme(new()
            {
                NavigationScheme.Entry.NavigateSelect,
                new NavigationScheme.Entry(MenuAction.Red, "Back", () =>
                {
                    Navigator.Instance.PopScheme();
                }),
                NavigationScheme.Entry.NavigateUp,
                NavigationScheme.Entry.NavigateDown
            }, true);

            scheme.PopCallback = () =>
            {
                _focused = false;
                _navGroup.SelectLastNavGroup();
            };

            Navigator.Instance.PushScheme(scheme);

            _focused = true;
            _navGroup.PushNavGroupToStack();
            _navGroup.SelectFirst();
        }

        protected override void OnSelectionChanged(bool selected)
        {
            base.OnSelectionChanged(selected);
            OnDisable();
        }

        private void OnDisable()
        {
            // If the visual's nav scheme is still in the stack, make sure to pop it.
            if (_focused)
            {
                Navigator.Instance.PopScheme();
            }
        }
    }
}