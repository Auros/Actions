using HMUI;
using Zenject;
using UnityEngine;
using VRUIControls;
using IPA.Utilities;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;

namespace Actions.Components
{
    internal abstract class FloatingViewController<T> : BSMLAutomaticViewController where T : BSMLAutomaticViewController
    {
        private static readonly FieldAccessor<VRGraphicRaycaster, PhysicsRaycasterWithCache>.Accessor PhysicsRaycaster = FieldAccessor<VRGraphicRaycaster, PhysicsRaycasterWithCache>.GetAccessor("_physicsRaycaster");

        protected FloatingScreen? _floatingScreen;
        private CurvedCanvasSettings? _curvedCanvasSettings;

        #region Properties

        private Vector2 _size = new Vector2(100f, 100f);
        internal Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                if (_floatingScreen != null)
                {
                    _floatingScreen.ScreenSize = _size;
                }
            }
        }

        private Vector3 _position = new Vector3(0f, 3.5f, 3f);
        internal Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                if (_floatingScreen != null)
                {
                    _floatingScreen.ScreenPosition = _position;
                }
            }
        }

        private Quaternion _rotation = Quaternion.identity;
        internal Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                if (_floatingScreen != null)
                {
                    _floatingScreen.ScreenRotation = _rotation;
                }
            }
        }

        private float _curveRadius = 0f;
        internal float CurveRadius
        {
            get => _curveRadius;
            set
            {
                _curveRadius = value;
                if (_curvedCanvasSettings != null)
                {
                    _curvedCanvasSettings.SetRadius(_curveRadius);
                }
            }
        }

        public bool Moveable
        {
            get => _floatingScreen?.ShowHandle ?? false;
            set
            {
                if (_floatingScreen != null)
                {
                    _floatingScreen.ShowHandle = value;
                }
            }
        }

        #endregion

        [Inject]
        protected virtual void Construct(PhysicsRaycasterWithCache physicsRaycasterWithCache)
        {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(Size, false, Position, Rotation, CurveRadius);
            _curvedCanvasSettings = GetComponentInChildren<CurvedCanvasSettings>(true);
            var graphicRaycaster = _floatingScreen.GetComponent<VRGraphicRaycaster>();
            PhysicsRaycaster(ref graphicRaycaster) = physicsRaycasterWithCache;
            _floatingScreen.SetRootViewController(this, AnimationType.None);
            _floatingScreen.name = $"{typeof(T).Name}Screen";
            name = $"{typeof(T).Name}View";
        }
    }
}