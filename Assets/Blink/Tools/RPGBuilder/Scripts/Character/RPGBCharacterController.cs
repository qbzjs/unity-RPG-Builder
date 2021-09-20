using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Utility;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace BLINK.RPGBuilder.Character
{
    public class RPGBCharacterController : MonoBehaviour
    {

        public RPGBCharacterControllerEssentials ControllerEssentials;
        
        #region General config

        public enum ControllerType
        {
            TopDown,
            ClickMove,
            FirstPerson,
            ThirdPerson
        }

        private Vector3 lastPos;
        public ControllerType currentController;
        [SerializeField]
        public ControllerType CurrentController
        {
            get => currentController;
            set
            {
                currentController = value;
                HandleControllerChange();
            }
        }

        public Animator anim;
        [SerializeField] protected float speed;
        [SerializeField] protected float gravityMod = 1;
        private CharacterController characterController;
        public Transform tpRef, topDownRef, fpRef;
        [SerializeField] private Camera mainCam;
        private float startSpeed;
        private Rigidbody rbd;
        private Vector3 cameraOffset;
        private float desiredSpeed, tpDistance;
        [SerializeField] public LayerMask GroundLayers;
        private bool isGrounded;
        private CombatNode thisCombatNode;

        private Vector2 lastCursorPos;
        private Vector3 mousePosition = Vector3.zero;
        private bool cursorNeedReset;
        private bool leftButtonDown = false;
        private bool rightButtonDown = false;
        private bool mouseLook;

        #endregion

        #region ThirdPerson

        private Transform noRotcamera;
        private bool _clickToRotate;
        public bool ClickToRotate
        {
            get => _clickToRotate;

            set
            {
                _clickToRotate = value;
                ChangeRotationMethod();
            }
        }
        public LayerMask camCollisionMask;
        public List<string> camCollisonTags;
        //TODO Change this for your player layer
        public LayerMask playerLayer;
        public float mouseSensitivity = 1, mouseZoomSpeed, jumpHeight;
        public int invertMouse = 1;
        public float minVertAngle, maxVertAngle, minZoom = -0.7f, maxZoom = -6, cameraReturnSpeed = 2;
        private float minZoomCache, maxZoomCache;
        private Quaternion intendeRot, startRot;
        private Vector3 playerVelocity;
        [SerializeField] private Vector3 headOffset;
        private Vector3 headOffsetCache;
        #endregion

        #region ClickSpecific

        public NavMeshAgent navAgent;
        #endregion

        #region TopDown Specific

        private Vector2 screenMiddle
        {
            get
            {
                var returno = new Vector2();
                returno.x = Screen.width / 2;
                returno.y = Screen.height / 2;
                return returno;
            }
            set { }
        }
        #endregion

        private void Start()
        {
            if (CombatManager.playerCombatNode != null) InitCharacter();
        }

        private void InitCharacter()
        {
            if (ControllerEssentials == null)
            {
                ControllerEssentials = GetComponent<RPGBCharacterControllerEssentials>();
            }
            startRot = transform.rotation;
            if (mainCam == null)
                mainCam = Camera.main;

            headOffset += tpRef.parent.localPosition;
            headOffsetCache = headOffset;
            minZoomCache = minZoom;
            maxZoomCache = maxZoom;
            tpDistance = Vector3.Distance(tpRef.position, transform.position);
            anim = GetComponentInChildren<Animator>();
            startSpeed = speed;
            lastPos = transform.position;
            navAgent = GetComponent<NavMeshAgent>();
            characterController = GetComponent<CharacterController>();
            rbd = GetComponent<Rigidbody>();
            noRotcamera = topDownRef.GetChild(0);
            noRotcamera.localEulerAngles = Vector3.zero;
            noRotcamera.localEulerAngles = new Vector3(-topDownRef.localEulerAngles.x, noRotcamera.localEulerAngles.y, noRotcamera.localEulerAngles.z);
            thisCombatNode = GetComponent<CombatNode>();
            CurrentController = ControllerType.ThirdPerson;
            ClickToRotate = true;
        }

        public UnityEvent OnMove = new UnityEvent();

        private void AdjustCamera()
        {
            mainCam.transform.position = cameraOffset + transform.position;
            if (CurrentController != ControllerType.ThirdPerson) return;
            CheckCamCollision();
            tpRef.parent.localPosition = headOffset;
        }

        private float GetAxisByName(string name)
        {
            if (DevUIManager.Instance.thisCG.alpha == 1 && DevUIManager.Instance.IsTypingInField()) return 0;
            switch (name)
            {
                case "Horizontal":
                {
                    var axis = 0;
                    if (Input.GetKey(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveLeft")))
                        axis -= 1;
                    if (Input.GetKey(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveRight")))
                        axis += 1;
                    return axis;
                }
                case "Vertical":
                {
                    var axis = 0;
                    if (Input.GetKey(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveForward")))
                        axis -= 1;
                    if (Input.GetKey(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveBackward")))
                        axis += 1;
                    return axis;
                }
                default:
                    return 0;
            }
        }

        public float GetDesiredSpeed()
        {
            return desiredSpeed;
        }

        public void SetDesiredSpeed(float newSpeed)
        {
            desiredSpeed = newSpeed;
        }

        private void FixedUpdate()
        {
            if (CombatManager.playerCombatNode == null) return;
            if (CombatManager.playerCombatNode.dead) return;
            RaycastHit hit;
            if (Physics.Raycast(transform.TransformPoint(characterController.center), Vector3.down, out hit, 1000f))
            {
                if (hit.distance <= 1.1f && GroundLayers == (GroundLayers | (1 << hit.collider.gameObject.layer)))
                    isGrounded = true;
                else
                    isGrounded = false;
                anim.SetFloat("GroundDistance", hit.distance);
            }
            else
            {
                anim.SetFloat("GroundDistance", 1000);
            }

            if (ControllerEssentials.isTeleporting) return;

            rbd.velocity = Vector3.zero;
            if (!ControllerEssentials.HasMovementRestrictions())
            {
                switch (currentController)
                {
                    case ControllerType.TopDown:
                        TopWASDMovement();
                        break;
                    case ControllerType.ClickMove:
                        ClickController();
                        break;
                    case ControllerType.ThirdPerson:
                        ThirdPersonMovement();
                        break;
                }
            }
            else
            {
                if(currentController == ControllerType.ClickMove) ApplyRotation();
            }

            anim.SetBool("isGrounded", isGrounded);

        }

        private void UpdateCursorVisibility(bool mouseButtonDown)
        {
            if (RPGBuilderUtilities.IsPointerOverUIObject())
            {
                if (!mouseButtonDown) Cursor.lockState = CursorLockMode.None;
            }
            else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                mousePosition = Input.mousePosition;
                mouseLook = true;
            }

            if (mouseButtonDown) return;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            mouseLook = false;

            if (!cursorNeedReset) return;
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Win32.SetCursorPos((int)lastCursorPos.x, (int)lastCursorPos.y);
            cursorNeedReset = false;
            #endif
        }


        private void LateUpdate()
        {
            if (CombatManager.playerCombatNode == null) return;
            if (CombatManager.playerCombatNode.dead) return;
            
            AdjustCamera();

            #region ThirdPerson
            if (CurrentController == ControllerType.ThirdPerson)
            {
                if (Input.GetKeyDown(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("Jump")) && isGrounded)
                {
                    playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * (Physics.gravity.y * gravityMod));
                    anim.SetTrigger("Jump");
                }

                playerVelocity.y += Physics.gravity.y * Time.deltaTime;
                if (isGrounded && playerVelocity.y < 0 && anim.GetFloat("GroundDistance") < 1.1f) playerVelocity.y = 0f;
                characterController.Move(playerVelocity * Time.deltaTime);
            }

            if (CurrentController != ControllerType.ThirdPerson) return;
            mainCam.transform.LookAt(tpRef.parent);
            if (ClickToRotate)
            {
                if (mouseLook)
                {
                    if (Input.GetKey(KeyCode.Mouse0))
                    {
                        tpRef.parent.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity, Space.Self);
                        tpRef.parent.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * mouseSensitivity * invertMouse, Space.Self);
                        intendeRot = ClampRotationAroundXAxis(tpRef.parent.localRotation);
                        tpRef.parent.localRotation = intendeRot;
                    }
                    else if (Input.GetKey(KeyCode.Mouse1))
                    {
                        if (tpRef.parent.localEulerAngles.y != 0)
                        {
                            transform.eulerAngles = new Vector3(transform.eulerAngles.x, tpRef.parent.eulerAngles.y, transform.eulerAngles.z);
                            tpRef.parent.localEulerAngles = new Vector3(tpRef.parent.localEulerAngles.x, 0, tpRef.parent.localEulerAngles.z);
                        }
                        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);

                        tpRef.parent.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * mouseSensitivity * invertMouse, Space.Self);
                        intendeRot = ClampRotationAroundXAxis(tpRef.parent.localRotation);
                        tpRef.parent.localRotation = intendeRot;
                    }
                }
            }
            else
            {
                transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);

                tpRef.parent.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * mouseSensitivity * invertMouse, Space.Self);
                intendeRot = ClampRotationAroundXAxis(tpRef.parent.localRotation);
                tpRef.parent.localRotation = intendeRot;
            }

            if (Input.GetAxis("Mouse ScrollWheel") == 0) return;
            if (RPGBuilderUtilities.IsPointerOverUIObject()) return;
            tpRef.Translate(Vector3.forward * mouseZoomSpeed * Input.GetAxis("Mouse ScrollWheel"), Space.Self);
            tpRef.localPosition = new Vector3(tpRef.localPosition.x, tpRef.localPosition.y, Mathf.Clamp(tpRef.localPosition.z, minZoom, maxZoom));
            tpDistance = Vector3.Distance(tpRef.position, transform.position);
            #endregion
        }

        [ExecuteInEditMode]
        private void HandleControllerChange()
        {
            ClickToRotate = true;
            navAgent.enabled = false;
            characterController.enabled = true;
            transform.rotation = startRot;
            rbd.isKinematic = false;
            anim.transform.localRotation = Quaternion.identity;
            anim.SetFloat("MoveSpeed", 0);
            anim.SetFloat("direction", 0);
            anim.SetFloat("strafeDir", 0);

            switch (currentController)
            {
                case ControllerType.TopDown:
                    rbd.isKinematic = true;
                    cameraOffset = topDownRef.position - transform.position;
                    mainCam.transform.rotation = topDownRef.rotation;
                    mainCam.nearClipPlane = 2f;
                    break;
                case ControllerType.ClickMove:
                    transform.rotation = startRot;
                    navAgent.enabled = true;
                    cameraOffset = topDownRef.position - transform.position;
                    mainCam.transform.rotation = topDownRef.rotation;
                    mainCam.nearClipPlane = 2f;
                    break;

                case ControllerType.ThirdPerson:
                    rbd.isKinematic = true;
                    cameraOffset = tpRef.position - transform.position;
                    mainCam.transform.rotation = tpRef.rotation;
                    mainCam.nearClipPlane = 0.3f;
                    break;

                case ControllerType.FirstPerson:
                    cameraOffset = fpRef.position - transform.position;
                    mainCam.transform.rotation = fpRef.rotation;
                    break;
            }
        }

        #region TopDown WASD Controllers

        private void TopWASDMovement()
        {
            if (GetAxisByName("Horizontal") != 0 || GetAxisByName("Vertical") != 0)
            {
                var noRotForward = new Vector3(mainCam.transform.forward.x, 0, mainCam.transform.forward.z);
                var noRotRight = new Vector3(mainCam.transform.right.x, 0, mainCam.transform.right.z);
                var moveVec = (noRotForward * GetAxisByName("Vertical") + noRotRight * GetAxisByName("Horizontal")).normalized * speed;
                var animVec = new Vector3(GetAxisByName("Horizontal"), 0, GetAxisByName("Vertical"));
                characterController.Move(moveVec * Time.deltaTime);
                desiredSpeed = Mathf.InverseLerp(0, moveVec.magnitude, (lastPos - transform.position).magnitude / Time.deltaTime);

                RaycastHit hit;
                if (Physics.Raycast(transform.TransformPoint(characterController.center), Vector3.down * 10, out hit, 1000f))
                {
                    if (currentController == ControllerType.TopDown)
                        transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                }
                
                var mouseInput = ((Vector2)Input.mousePosition - screenMiddle).normalized;
                float direct;
                float strafeDir = 0;

                if (mouseInput.y < -.6f || mouseInput.y > .6f)
                {
                    direct = Math.Sign(animVec.z) == Math.Sign(mouseInput.y) ? 0 : 1;

                    if (animVec.x != 0)
                    {
                        strafeDir = Math.Sign(animVec.x) == Math.Sign(mouseInput.y) ? 0 : 1;
                        if (animVec.z != 0)
                            direct = direct == 0 ? .25f : .85f;
                        else
                            direct = .5f;
                    }
                }
                else
                {
                    direct = Math.Sign(animVec.x) == Math.Sign(mouseInput.x) ? 0 : 1;

                    if (animVec.z != 0)
                    {
                        strafeDir = Math.Sign(animVec.z) == Math.Sign(mouseInput.x) ? 1 : 0;
                        if (animVec.x != 0)
                            direct = direct == 0 ? .25f : .85f;
                        else
                            direct = .5f;
                    }
                }

                var currentDirect = anim.GetFloat("direction");
                currentDirect = Mathf.Lerp(currentDirect, direct, 5 * Time.deltaTime);
                anim.SetFloat("direction", currentDirect);

                var currentStrafe = anim.GetFloat("strafeDir");
                currentStrafe = Mathf.Lerp(currentStrafe, strafeDir, 5 * Time.deltaTime);
                anim.SetFloat("strafeDir", currentStrafe);


            }
            else
            {
                desiredSpeed = 0;
            }


            var currentSpeed = anim.GetFloat("MoveSpeed");
            currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, 10 * Time.deltaTime);
            anim.SetFloat("MoveSpeed", currentSpeed);
            ApplyRotation();
            if (lastPos != transform.position)
                OnMove?.Invoke();

            lastPos = transform.position;
        }

        public void PlayerLookAtCursor()
        {
            var playerPlane = new Plane(Vector3.up, transform.position);
            var ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (!playerPlane.Raycast(ray, out var hitDist)) return;
            var targetPoint = ray.GetPoint(hitDist);
            var targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = targetRotation;
        }

        private void ApplyRotation()
        {
            var playerPlane = new Plane(Vector3.up, transform.position);
            var ray = mainCam.ScreenPointToRay(Input.mousePosition);
            var hitDist = 0f;
            if (!playerPlane.Raycast(ray, out hitDist)) return;
            var targetPoint = ray.GetPoint(hitDist);
            var targetRotation = Quaternion.LookRotation(targetPoint - transform.position);
            targetRotation.x = 0;
            targetRotation.z = 0;
            transform.rotation = targetRotation;
        }
        #endregion

        #region Click to move controllers

        private bool cachedGroundCasting;

        public IEnumerator UpdateCachedGroundCasting(bool isGroundCasting)
        {
            yield return new WaitForSeconds(0.1f);
            cachedGroundCasting = isGroundCasting;
        }
        private void ClickController()
        {
            if (!navAgent.enabled) return;
            if (navAgent.hasPath)
            {
                if (navAgent.path.corners.Length >= 1)
                {
                    var intendedRot = Quaternion.LookRotation(navAgent.path.corners[1] - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, intendedRot, 5 * Time.deltaTime);
                }
                else
                {
                    transform.LookAt(navAgent.destination);
                }
            }
            desiredSpeed = Mathf.InverseLerp(0, 1, (lastPos - transform.position).magnitude / Time.deltaTime);
            var currentSpeed = anim.GetFloat("MoveSpeed");
            currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, 10 * Time.deltaTime);
            anim.SetFloat("MoveSpeed", currentSpeed);

            if (lastPos != transform.position)
                OnMove?.Invoke();

            lastPos = transform.position;

            if (!Input.GetMouseButton(0) || RPGBuilderUtilities.IsPointerOverUIObject() || cachedGroundCasting) return;
            RaycastHit hit;
            var r = mainCam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(r.origin, r.direction, Color.yellow);
            
            if (Physics.Raycast(r, out hit, Mathf.Infinity, GroundLayers)) 
            {
                navAgent.destination = hit.point;
                navAgent.velocity = (navAgent.destination - transform.position).normalized * navAgent.speed;
            }
        }
        #endregion

        #region ThirdPerson

        private void ThirdPersonMovement()
        {
            if (GetAxisByName("Horizontal") != 0 || GetAxisByName("Vertical") != 0)
            {
                if (!Input.GetKey(KeyCode.Mouse0))
                    if (tpRef.parent.localEulerAngles.y != 0) tpRef.parent.localRotation = Quaternion.Slerp(tpRef.parent.localRotation, Quaternion.Euler(tpRef.parent.localEulerAngles.x, 0, tpRef.parent.localEulerAngles.z), cameraReturnSpeed * Time.deltaTime);

                var moveVec = (transform.right * GetAxisByName("Horizontal") + transform.forward * GetAxisByName("Vertical")).normalized * speed;
                var animVec = new Vector3(GetAxisByName("Horizontal"), 0, GetAxisByName("Vertical"));

                characterController.Move(moveVec * Time.deltaTime);
                desiredSpeed = Mathf.InverseLerp(0, moveVec.magnitude, (lastPos - transform.position).magnitude / Time.deltaTime);

                var direct = Mathf.Clamp(animVec.z, 1, 0);
                float strafeDir = 0;

                if (animVec.x != 0)
                {
                    strafeDir = Mathf.Clamp(animVec.x, 1, 0);
                    strafeDir = direct < 0 ? .25f : strafeDir;
                    if (animVec.z != 0)
                        direct = direct == 0 ? .25f : .85f;
                    else
                        direct = .5f;
                }

                var currentDirect = anim.GetFloat("direction");
                currentDirect = Mathf.Lerp(currentDirect, direct, 5 * Time.deltaTime);
                anim.SetFloat("direction", currentDirect);

                var currentStrafe = anim.GetFloat("strafeDir");
                currentStrafe = Mathf.Lerp(currentStrafe, strafeDir, 5 * Time.deltaTime);
                anim.SetFloat("strafeDir", currentStrafe);
            }
            else
            {
                desiredSpeed = 0;
            }


            var currentSpeed = anim.GetFloat("MoveSpeed");
            currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, 10 * Time.deltaTime);
            anim.SetFloat("MoveSpeed", currentSpeed);

            if (lastPos != transform.position)
                OnMove?.Invoke();

            lastPos = transform.position;
        
        }

        private void CheckCamCollision()
        {
            RaycastHit hit;
            if (Physics.Raycast(tpRef.parent.position, tpRef.position - tpRef.parent.position, out hit, tpDistance, camCollisionMask))
            {
                if ((camCollisionMask & (1 << hit.collider.gameObject.layer)) != 1 << hit.collider.gameObject.layer)
                    return;
                if (camCollisonTags.Count > 0)
                    if (camCollisonTags.IndexOf(hit.collider.gameObject.tag) < 0)
                        return;

                tpRef.position = hit.point + -(tpRef.position - tpRef.parent.position).normalized;
                tpRef.localPosition = new Vector3(tpRef.localPosition.x, tpRef.localPosition.y,
                    Mathf.Clamp(tpRef.localPosition.z, tpRef.localPosition.z, -0.5f));
                cameraOffset = tpRef.position - transform.position;
            }
            else
            {
                cameraOffset = tpRef.position - transform.position;
            }
        }

        private void ChangeRotationMethod()
        {
            if (ClickToRotate)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                headOffset.x = 0;
                headOffset.z = 0;
                minZoom = minZoomCache;
                maxZoom = maxZoomCache;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                headOffset.x = headOffsetCache.x;
                headOffset.z = headOffsetCache.z;
                minZoom = -2.5f;
                maxZoom = -2.5f;
                tpRef.localPosition = new Vector3(tpRef.localPosition.x, tpRef.localPosition.y, Mathf.Clamp(tpRef.localPosition.z, minZoom, maxZoom));
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minVertAngle, maxVertAngle);
            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
        #endregion


        private void Update()
        {
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.speed = speed;
                navAgent.acceleration = speed;
            }

            if (Input.GetMouseButtonDown(0) && !RPGBuilderUtilities.IsPointerOverUIObject())
                leftButtonDown = true;
            if (Input.GetMouseButtonUp(0))
                leftButtonDown = false;
            if (Input.GetMouseButtonDown(1) && !RPGBuilderUtilities.IsPointerOverUIObject())
                rightButtonDown = true;
            if (Input.GetMouseButtonUp(1))
                rightButtonDown = false;

            if (ClickToRotate)
                UpdateCursorVisibility(leftButtonDown || rightButtonDown);

            if (RPGBuilderUtilities.IsPointerOverUIObject() || !mouseLook ||
                CurrentController != ControllerType.ThirdPerson || !ClickToRotate) return;
            if (Cursor.lockState == CursorLockMode.Locked) return;
            
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            var pt = new Win32.POINT();
            Win32.GetCursorPos(out pt);
            lastCursorPos.x = pt.X;
            lastCursorPos.y = pt.Y;
            #endif

            if (!(Vector3.Distance(Input.mousePosition, mousePosition) > 5)) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorNeedReset = true;
        }
        
        
        public bool GetIsGrounded()
        {
            return isGrounded;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public void SetPlayerVelocity(Vector3 newVelocity)
        {
            playerVelocity = newVelocity;
        }
        
    }
}
