// GENERATED AUTOMATICALLY FROM 'Assets/NWH/Dynamic Water Physics 2/Scripts/ShipController/Input/InputProviders/InputSystemProvider/ShipInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace NWH.DWP2.ShipController
{
    public class @ShipInputActions : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @ShipInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""ShipInputActions"",
    ""maps"": [
        {
            ""name"": ""ShipControls"",
            ""id"": ""200a0048-834b-4c46-8e58-cb0180a3f09b"",
            ""actions"": [
                {
                    ""name"": ""Steering"",
                    ""type"": ""Value"",
                    ""id"": ""4c14d84a-48f6-429e-9111-d009cff86527"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Throttle"",
                    ""type"": ""Value"",
                    ""id"": ""067e3728-8c0e-4c68-8b07-765ef5a0b2ff"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Throttle2"",
                    ""type"": ""Value"",
                    ""id"": ""ec48226f-41c5-4f41-addf-d89dc4fe9489"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Throttle3"",
                    ""type"": ""Value"",
                    ""id"": ""adb1ee87-c871-44cb-9536-be9a29269bd3"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Throttle4"",
                    ""type"": ""Value"",
                    ""id"": ""09912318-ce4d-4f0b-8336-e581f237a903"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""BowThruster"",
                    ""type"": ""Value"",
                    ""id"": ""a09cdc8f-5c97-4068-8e2c-f72377d022f2"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SternThruster"",
                    ""type"": ""Value"",
                    ""id"": ""61f34607-04fc-4263-b801-6bb95dda9649"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SubmarineDepth"",
                    ""type"": ""Value"",
                    ""id"": ""c2a3958c-d006-4c67-8425-3635c0303de7"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""EngineStartStop"",
                    ""type"": ""Button"",
                    ""id"": ""83764afd-1cc4-4537-9658-82bb7f635f32"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Anchor"",
                    ""type"": ""Button"",
                    ""id"": ""0050fc1b-a233-4284-b206-2e5254c3fc79"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""1bff6693-e128-4d74-ad2f-ad4e229608a8"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""6a4beedd-9d33-48f8-ab0b-2fa4835b56d5"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""3ce20f4e-6e04-4810-b8c6-829ced054390"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""acf664f4-dde0-4367-8e3a-7fbdf293bc0c"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": ""AxisDeadzone"",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""0332ccc4-b818-41e6-8455-330ec56c13de"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""89725913-86df-4c0b-893e-7dab71609463"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steering"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""4b4d086e-45c1-4efd-8515-ae86f80a90b1"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""6f9e3d4b-0428-4529-8135-15a721c1c69c"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""6da65c4c-15f8-4e4d-9560-05883d18ce16"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""405ba6f9-74dd-4b74-8687-7dda1bc905d3"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""7d163169-2e04-49bb-8dad-3fa803f4201e"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""64d90bc6-be37-4aab-ae3e-418884101999"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Throttle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""72d84049-f629-4a9e-bcfc-28b9c25ab184"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SternThruster"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""518e635c-d6f9-4b2c-927c-ef61e6ab41d2"",
                    ""path"": ""<Keyboard>/z"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SternThruster"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""0f31564a-814a-4fb0-b549-e2c7e3e93935"",
                    ""path"": ""<Keyboard>/c"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SternThruster"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""e04c0968-0501-4c7c-8326-dc8e845631aa"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BowThruster"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""1d34dc63-cb94-4628-8a5d-f1b5b5c553de"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BowThruster"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""7d4e3e8a-9ffe-40e0-b96e-b7b1f94a90cb"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""BowThruster"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""d62bb2c8-9196-4156-8dcb-6880975b400b"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""EngineStartStop"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""41b26daf-b33a-44aa-914f-71aa6d3e1e60"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Anchor"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Keyboard"",
                    ""id"": ""b228703a-0571-41de-9716-c9193a6cb635"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SubmarineDepth"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Negative"",
                    ""id"": ""92ec8857-35e5-49fd-9008-80f3f4a5704b"",
                    ""path"": ""<Keyboard>/k"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SubmarineDepth"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Positive"",
                    ""id"": ""511c38f0-ff55-41b6-b29c-968ae5d0bdca"",
                    ""path"": ""<Keyboard>/i"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SubmarineDepth"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
            // ShipControls
            m_ShipControls = asset.FindActionMap("ShipControls", throwIfNotFound: true);
            m_ShipControls_Steering = m_ShipControls.FindAction("Steering", throwIfNotFound: true);
            m_ShipControls_Throttle = m_ShipControls.FindAction("Throttle", throwIfNotFound: true);
            m_ShipControls_Throttle2 = m_ShipControls.FindAction("Throttle2", throwIfNotFound: true);
            m_ShipControls_Throttle3 = m_ShipControls.FindAction("Throttle3", throwIfNotFound: true);
            m_ShipControls_Throttle4 = m_ShipControls.FindAction("Throttle4", throwIfNotFound: true);
            m_ShipControls_BowThruster = m_ShipControls.FindAction("BowThruster", throwIfNotFound: true);
            m_ShipControls_SternThruster = m_ShipControls.FindAction("SternThruster", throwIfNotFound: true);
            m_ShipControls_SubmarineDepth = m_ShipControls.FindAction("SubmarineDepth", throwIfNotFound: true);
            m_ShipControls_EngineStartStop = m_ShipControls.FindAction("EngineStartStop", throwIfNotFound: true);
            m_ShipControls_Anchor = m_ShipControls.FindAction("Anchor", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // ShipControls
        private readonly InputActionMap m_ShipControls;
        private IShipControlsActions m_ShipControlsActionsCallbackInterface;
        private readonly InputAction m_ShipControls_Steering;
        private readonly InputAction m_ShipControls_Throttle;
        private readonly InputAction m_ShipControls_Throttle2;
        private readonly InputAction m_ShipControls_Throttle3;
        private readonly InputAction m_ShipControls_Throttle4;
        private readonly InputAction m_ShipControls_BowThruster;
        private readonly InputAction m_ShipControls_SternThruster;
        private readonly InputAction m_ShipControls_SubmarineDepth;
        private readonly InputAction m_ShipControls_EngineStartStop;
        private readonly InputAction m_ShipControls_Anchor;
        public struct ShipControlsActions
        {
            private @ShipInputActions m_Wrapper;
            public ShipControlsActions(@ShipInputActions wrapper) { m_Wrapper = wrapper; }
            public InputAction @Steering => m_Wrapper.m_ShipControls_Steering;
            public InputAction @Throttle => m_Wrapper.m_ShipControls_Throttle;
            public InputAction @Throttle2 => m_Wrapper.m_ShipControls_Throttle2;
            public InputAction @Throttle3 => m_Wrapper.m_ShipControls_Throttle3;
            public InputAction @Throttle4 => m_Wrapper.m_ShipControls_Throttle4;
            public InputAction @BowThruster => m_Wrapper.m_ShipControls_BowThruster;
            public InputAction @SternThruster => m_Wrapper.m_ShipControls_SternThruster;
            public InputAction @SubmarineDepth => m_Wrapper.m_ShipControls_SubmarineDepth;
            public InputAction @EngineStartStop => m_Wrapper.m_ShipControls_EngineStartStop;
            public InputAction @Anchor => m_Wrapper.m_ShipControls_Anchor;
            public InputActionMap Get() { return m_Wrapper.m_ShipControls; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(ShipControlsActions set) { return set.Get(); }
            public void SetCallbacks(IShipControlsActions instance)
            {
                if (m_Wrapper.m_ShipControlsActionsCallbackInterface != null)
                {
                    @Steering.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSteering;
                    @Steering.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSteering;
                    @Steering.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSteering;
                    @Throttle.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle;
                    @Throttle.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle;
                    @Throttle.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle;
                    @Throttle2.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle2;
                    @Throttle2.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle2;
                    @Throttle2.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle2;
                    @Throttle3.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle3;
                    @Throttle3.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle3;
                    @Throttle3.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle3;
                    @Throttle4.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle4;
                    @Throttle4.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle4;
                    @Throttle4.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnThrottle4;
                    @BowThruster.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnBowThruster;
                    @BowThruster.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnBowThruster;
                    @BowThruster.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnBowThruster;
                    @SternThruster.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSternThruster;
                    @SternThruster.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSternThruster;
                    @SternThruster.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSternThruster;
                    @SubmarineDepth.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSubmarineDepth;
                    @SubmarineDepth.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSubmarineDepth;
                    @SubmarineDepth.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnSubmarineDepth;
                    @EngineStartStop.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnEngineStartStop;
                    @EngineStartStop.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnEngineStartStop;
                    @EngineStartStop.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnEngineStartStop;
                    @Anchor.started -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnAnchor;
                    @Anchor.performed -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnAnchor;
                    @Anchor.canceled -= m_Wrapper.m_ShipControlsActionsCallbackInterface.OnAnchor;
                }
                m_Wrapper.m_ShipControlsActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Steering.started += instance.OnSteering;
                    @Steering.performed += instance.OnSteering;
                    @Steering.canceled += instance.OnSteering;
                    @Throttle.started += instance.OnThrottle;
                    @Throttle.performed += instance.OnThrottle;
                    @Throttle.canceled += instance.OnThrottle;
                    @Throttle2.started += instance.OnThrottle2;
                    @Throttle2.performed += instance.OnThrottle2;
                    @Throttle2.canceled += instance.OnThrottle2;
                    @Throttle3.started += instance.OnThrottle3;
                    @Throttle3.performed += instance.OnThrottle3;
                    @Throttle3.canceled += instance.OnThrottle3;
                    @Throttle4.started += instance.OnThrottle4;
                    @Throttle4.performed += instance.OnThrottle4;
                    @Throttle4.canceled += instance.OnThrottle4;
                    @BowThruster.started += instance.OnBowThruster;
                    @BowThruster.performed += instance.OnBowThruster;
                    @BowThruster.canceled += instance.OnBowThruster;
                    @SternThruster.started += instance.OnSternThruster;
                    @SternThruster.performed += instance.OnSternThruster;
                    @SternThruster.canceled += instance.OnSternThruster;
                    @SubmarineDepth.started += instance.OnSubmarineDepth;
                    @SubmarineDepth.performed += instance.OnSubmarineDepth;
                    @SubmarineDepth.canceled += instance.OnSubmarineDepth;
                    @EngineStartStop.started += instance.OnEngineStartStop;
                    @EngineStartStop.performed += instance.OnEngineStartStop;
                    @EngineStartStop.canceled += instance.OnEngineStartStop;
                    @Anchor.started += instance.OnAnchor;
                    @Anchor.performed += instance.OnAnchor;
                    @Anchor.canceled += instance.OnAnchor;
                }
            }
        }
        public ShipControlsActions @ShipControls => new ShipControlsActions(this);
        public interface IShipControlsActions
        {
            void OnSteering(InputAction.CallbackContext context);
            void OnThrottle(InputAction.CallbackContext context);
            void OnThrottle2(InputAction.CallbackContext context);
            void OnThrottle3(InputAction.CallbackContext context);
            void OnThrottle4(InputAction.CallbackContext context);
            void OnBowThruster(InputAction.CallbackContext context);
            void OnSternThruster(InputAction.CallbackContext context);
            void OnSubmarineDepth(InputAction.CallbackContext context);
            void OnEngineStartStop(InputAction.CallbackContext context);
            void OnAnchor(InputAction.CallbackContext context);
        }
    }
}
