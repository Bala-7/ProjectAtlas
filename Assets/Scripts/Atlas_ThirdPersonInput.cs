using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.Characters.ThirdPerson;

public class Atlas_ThirdPersonInput : ThirdPersonUserControl
{

    public enum CAMERA_MODE { BASE, HIDE };
    public enum PLAYER_STATE { NORMAL, HIDING };

    #region Atributos
    // Públicos
    public Transform chest;     // El pecho del personaje. Se usa para rotar el personaje hacia la dirección en la que queremos apuntar.
    public HideFX _hideFXPrefab;
    public float maxHideTime;
    
    // Privados
    private MyTPCharacter tpc;          // Script asociado al modelo del robot. Uso esto solo para poder animarlo
    private CinemachineFreeLook cfl;    // Este es el objeto que se crea en la escena al crear una cámara Cinemachine. Lo usamos para controlar la posición y movimiento de la cámara

    #region PostProcesado
    private PostProcessVolume ppv;
    private ColorGrading pp_cg;
    private LensDistortion pp_ld;
    private HideFX _hideFX;
    #endregion
    private PLAYER_STATE _state;
    private float currentHideTime;

    private List<Light> lights;
    private Canvas _canvas;
    private Image _stamina;
    #endregion


    // La función Awake se llama antes que Start, y es donde se inicializan todos los parámetros de la clase
    private void Awake()
    {
        cfl = FindObjectOfType<CinemachineFreeLook>();
        ppv = FindObjectOfType<PostProcessVolume>();
        ppv.profile.TryGetSettings<ColorGrading>(out pp_cg);
        pp_cg.enabled.value = true;
        ppv.profile.TryGetSettings<LensDistortion>(out pp_ld);
        pp_ld.enabled.value = true;

        tpc = FindObjectOfType<MyTPCharacter>();
        SetCameraMode(CAMERA_MODE.BASE);
        
        GameObject l = GameObject.FindGameObjectWithTag("Lights");
        lights = new List<Light>(l.GetComponentsInChildren<Light>());

        _hideFX = Instantiate(_hideFXPrefab, transform);

        _canvas = FindObjectOfType<Canvas>();
        _stamina = _canvas.transform.GetChild(0).GetComponent<Image>();
        _stamina.gameObject.SetActive(false);

        _state = PLAYER_STATE.NORMAL;

    }

    protected void Update()
    {
        bool hidePressed = Input.GetKeyDown(KeyCode.Q);      // true en el frame en que se pulsa el botón de apuntar
        bool hideReleased = Input.GetKeyUp(KeyCode.Q);
        
        bool hiding = (_state == PLAYER_STATE.HIDING);

        bool canHide = CanHide();


        // Actualizamos el medidor de stamina
        if (_state == PLAYER_STATE.HIDING)
        {
            currentHideTime += Time.deltaTime;
            float timePercentage = currentHideTime / maxHideTime;
            _stamina.fillAmount = 1 - timePercentage;
            canHide = canHide && (timePercentage < 1);

            if (timePercentage > 0.8f) {
                _stamina.color = Color.red;
            }
        }

        // Actualizamos los efectos dependiendo del estado del jugador
        if (hidePressed && canHide && _state == PLAYER_STATE.NORMAL)
        {
            _hideFX.transform.position = transform.position;

            _hideFX.Play(); // Activamos los efectos de humo
            tpc.Hide(); // Escondemos el personaje
            
            _stamina.gameObject.SetActive(true);    // Activamos la barra de stamina
            _stamina.color = new Color(0, 255, 224);    // La dibujamos de color azul claro

            _state = PLAYER_STATE.HIDING;   // Cambiamos el estado del personaje
            currentHideTime = 0;    // Reiniciamos el contador de tiempo que lleva escondido el jugador

            SetCameraMode(CAMERA_MODE.HIDE);    // Cambiamos la camara
        }
        else if (hideReleased || (hiding && !canHide))
        {
            if(_state == PLAYER_STATE.HIDING) tpc.Show();
            _stamina.gameObject.SetActive(false);

            _state = PLAYER_STATE.NORMAL;
            _hideFX.Stop();

            // Reconfiguramos la camara al modo normal
            SetCameraMode(CAMERA_MODE.BASE);
        }


        
    }

    // La función FixedUpdate se llama varias veces cada frame.
    protected new void FixedUpdate()
    {
        base.FixedUpdate();

        bool walking = (m_Move.x != 0 || m_Move.z != 0);    // true si el personaje está moviéndose
        bool hidePressed = Input.GetKeyDown(KeyCode.Q);      // true en el frame en que se pulsa el botón de apuntar
        bool hideReleased = Input.GetKeyUp(KeyCode.Q);
        bool hiding = _state == PLAYER_STATE.HIDING;              // true mientras el botón de apuntar esté pulsado
        
        
        tpc.GetAnimator().SetBool("IsWalking", walking);    // Se le dice al Animator que el personaje está andando    

        // Se coloca el 'aimTarget' en el centro de la cámara. Nuestro personaje mirará hacia él para apuntar
        //aimTarget.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, aimDistance));


        hiding = hiding && CanHide();



        // Llamamos al Move del padre para que mueva el GameObject
        m_Character.Move(m_Move, false, m_Jump);
        tpc.GetAnimator().SetBool("IsHiding", hiding);
        tpc.SetGlow(CanHide());
        //aimCursor.gameObject.SetActive(aiming);
        m_Jump = false;
    }


    /**
     *  Esta función cambia la configuración de la cámara dependiendo de si estamos en el modo de apuntado o en el modo normal
     */
    private void SetCameraMode(CAMERA_MODE mode) {

        switch (mode) {
            case CAMERA_MODE.BASE:
                { // Esta será la configuración de la cámara cuando el personaje está andando
                  // Top Orbit
                    cfl.m_Orbits[0].m_Height = 4.5f;
                    cfl.m_Orbits[0].m_Radius = 1.75f;

                    // Middle Orbit
                    cfl.m_Orbits[1].m_Radius = 4f;

                    // Bottom Orbit
                    cfl.m_Orbits[2].m_Height = 1.0f;
                    cfl.m_Orbits[2].m_Radius = 4.0f;

                    // Rigs: 0-Top, 1-Middle, 2-Botton
                    cfl.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;
                    cfl.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;
                    cfl.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;

                    pp_cg.enabled.value = true;
                    pp_cg.saturation.value = -55;
                    pp_ld.intensity.value = -55f;

                    InvokeRepeating("ShowPostProcessFX", 0, 0.03f);
                    break;
                }
            case CAMERA_MODE.HIDE:   // Esta será la configuración de la cámara en el modo de apuntado
                // Top Ring
                cfl.m_Orbits[0].m_Height = 4.5f;
                cfl.m_Orbits[0].m_Radius = 3.0f;

                // Middle Ring
                cfl.m_Orbits[1].m_Radius = 6f;

                // Bottom Ring
                cfl.m_Orbits[2].m_Height = 1.0f;
                cfl.m_Orbits[2].m_Radius = 6.0f;

                cfl.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;
                cfl.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;
                cfl.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.5f;


                pp_cg.enabled.value = true;
                pp_cg.saturation.value = 10;
                pp_ld.intensity.value = 0.0f;
                InvokeRepeating("HidePostProcessFX", 0, 0.03f);
                break;
            default: break;
        }
    
    }


    private bool CanHide() {

        bool canHide = true;

        foreach(Light l in lights)
        {
            if ((l.type == LightType.Point)) { 
                float distance = Vector3.Distance(transform.position, l.transform.position);
                Vector3 lDirection = l.transform.position - transform.position;
                RaycastHit hitInfo;
                bool raycastHit = Physics.Raycast(transform.position, lDirection, out hitInfo, distance);
                Color rayColor = Color.green;

                bool far = distance >= l.range;


                if ((!far && !raycastHit)) {
                    canHide = false;
                    rayColor = Color.red;
                    Debug.DrawRay(transform.position, lDirection, rayColor);
                    break;
                }
                Debug.DrawRay(transform.position, lDirection, rayColor);

            }
        }

        return canHide;
    }

    private void HidePostProcessFX() {
        pp_cg.saturation.value -= 5;
        pp_ld.intensity.value -= 5;
        if (pp_cg.saturation.value <= -55) CancelInvoke();
    }

    private void ShowPostProcessFX()
    {
        pp_cg.saturation.value += 5;
        if (pp_ld.intensity < 0) pp_ld.intensity.value += 5;
        if (pp_cg.saturation.value >= 10) CancelInvoke();
    }
}



