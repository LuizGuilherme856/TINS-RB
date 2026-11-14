using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIComponentsForSettings
{
    public class SelectionUGUI : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("SelectionEventListneners")]
        public SelectionEventListener[] SelectionEventListeners;

        [Tooltip("Will be active while it is selected.")]
        public GameObject Selected;

        [Tooltip("Enable if you do not want the selected highlight to show up if coming from the mouse.")]
        public bool IgnoreSelectionsFromMouse = true;

        protected float m_lastMouseUseTime;

        public void Start()
        {
            SelectionStateRefresh_TechBooh();
        }

        public void OnEnable()
        {
            SelectionStateRefresh_TechBooh();
        }

        public void OnDisable()
        {
            DisconnectFromListeners();

            if (Selected != null)
            {
                Selected.SetActive(false);
            }
        }

        protected SelectionEventListener FindFirstActiveListener()
        {
            foreach (var listener in SelectionEventListeners)
            {
                if (listener != null && listener.enabled && listener.gameObject.activeInHierarchy)
                    return listener;
            }

            return null;
        }

        public void ConnectToListeners()
        {
            foreach (var listener in SelectionEventListeners)
            {
                if (listener == null)
                    continue;

                listener.OnSelectionChanged += onSelectionChanged;
            }
        }

        public void DisconnectFromListeners()
        {
            foreach (var listener in SelectionEventListeners)
            {
                if (listener == null)
                    continue;

                listener.OnSelectionChanged -= onSelectionChanged;
            }
        }

        // TechBooh modification:
        // The original asset could keep the previous selection highlight visible when a menu
        // was disabled without a matching deselect event. We reconnect and resync the state
        // every time the object becomes active, and we always hide the highlight on disable.
        protected void SelectionStateRefresh_TechBooh()
        {
            DisconnectFromListeners();
            ConnectToListeners();

            if (Selected == null)
            {
                return;
            }

            var listener = FindFirstActiveListener();
            Selected.SetActive(listener != null && listener.IsSelected);
        }

        protected void onSelectionChanged(bool isSelected)
        {
            updateLastMouseUseTime();

            if (Selected != null)
            {
                if (isSelected && mouseUsed(IgnoreSelectionsFromMouse))
                    return;

                Selected.SetActive(isSelected);
            }
        }

        protected bool mouseUsed(bool ignore)
        {
            if (!ignore)
                return false;

            return mouseWasRecentlyUsed(0.3f);
        }

        protected void updateLastMouseUseTime()
        {
            bool primaryMouseUsed = InputUtils.LeftMouse();
            if (primaryMouseUsed)
            {
                m_lastMouseUseTime = Time.realtimeSinceStartup;
            }
        }

        protected bool mouseWasRecentlyUsed(float maxDelay = 0.3f)
        {
            if (InputUtils.LeftMouse())
                return true;

            return Time.realtimeSinceStartup - m_lastMouseUseTime < maxDelay;
        }

#if UNITY_EDITOR
        public void Reset()
        {
            var listener = GetComponent<SelectionEventListener>();
            if (transform.parent != null && listener == null)
            {
                listener = transform.parent.GetComponent<SelectionEventListener>();
            }
            if (listener != null)
            {
                SelectionEventListeners = new SelectionEventListener[] { listener };
            }
        }
#endif
    }
}
