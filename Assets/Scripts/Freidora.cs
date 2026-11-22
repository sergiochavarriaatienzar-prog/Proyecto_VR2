
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;               
using UnityEngine.XR.Interaction.Toolkit.Interactables; 
using UnityEngine.XR.Interaction.Toolkit.Interactors;   

[RequireComponent(typeof(XRSimpleInteractable))]
public class Freidora : MonoBehaviour
{
    enum Estado { Vacia, Cocinando, Lista }

    [Header("Cocinado")]
    [SerializeField] float tiempoCocinado = 5f;     
    [SerializeField] float distanciaMaxima = 2f;    

    [Header("Barra de progreso")]
    [SerializeField] Slider barra;                  

    [Header("Avisos")]
    public UnityEvent alTerminar;                   //Evento al terminar

    [Header("Debug (solo lectura)")]
    [SerializeField] Estado estado = Estado.Vacia;
    [SerializeField] float progreso;                

    XRSimpleInteractable interactable;
    Collider col;
    XRBaseInputInteractor mano;     
    bool avisoVisible;              
    float tiempo;                   

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        col = GetComponentInChildren<Collider>();
        if (barra == null)
        {
            Debug.Log("falta el slider", this);
        }
        else
        {
            barra.minValue = 0f;
            barra.maxValue = 1f;
            barra.gameObject.SetActive(false);   
        }
    }

    void OnEnable()
    {
        interactable.hoverEntered.AddListener(OnHoverEntered);
        interactable.hoverExited.AddListener(OnHoverExited);
    }

    void OnDisable()
    {
        interactable.hoverEntered.RemoveListener(OnHoverEntered);
        interactable.hoverExited.RemoveListener(OnHoverExited);
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        mano = args.interactorObject as XRBaseInputInteractor;
        Debug.Log("Freidora: te apunta " + args.interactorObject.transform.name + " (mano valida: " + (mano != null) + ")", this);
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        mano = null;
        OcultarAviso();
    }

    void Update()
    {
        //Cocinando
        if (estado == Estado.Cocinando)
        {
            tiempo += Time.deltaTime;
            progreso = Mathf.Clamp01(tiempo / tiempoCocinado);
            if (barra != null) barra.value = progreso;

            if (progreso >= 1f) Terminar();
        }

        //Cartel aparece
        bool puedoInteractuar = mano != null && EstaCerca() && estado != Estado.Cocinando;
        if (puedoInteractuar && !avisoVisible) MostrarAviso();
        else if (!puedoInteractuar && avisoVisible) OcultarAviso();

        //Gatillo
        if (mano != null && mano.activateInput.ReadWasPerformedThisFrame())
        {
            Debug.Log("Freidora: gatillo pulsado. Cerca: " + EstaCerca() + ", estado: " + estado, this);
            if (puedoInteractuar)
            {
                if (estado == Estado.Vacia) Empezar();
                else if (estado == Estado.Lista) Recoger();
            }
        }
    }

    void Empezar()
    {
        estado = Estado.Cocinando;
        tiempo = 0f;
        progreso = 0f;
        if (barra != null)
        {
            barra.value = 0f;
            barra.gameObject.SetActive(true);
        }
        Debug.Log("Freidora: cocinando...");
    }

    void Terminar()
    {
        estado = Estado.Lista;
        if (barra != null) barra.gameObject.SetActive(false);
        Debug.Log("Freidora: ¡patata lista! Acercate a recogerla.");
        alTerminar.Invoke();
    }

    void Recoger()
    {
        Inventario.instancia.AnadirPatataCocinada();
        estado = Estado.Vacia;
        progreso = 0f;
    }

    bool EstaCerca()
    {
        return Vector3.Distance(Camera.main.transform.position, transform.position) <= distanciaMaxima;
    }

    void MostrarAviso()
    {
        Vector3 arriba = col.bounds.center + Vector3.up * col.bounds.extents.y;
        AvisoInteractuar.instancia.Mostrar(arriba);
        avisoVisible = true;
    }

    void OcultarAviso()
    {
        if (!avisoVisible) return;
        AvisoInteractuar.instancia.Ocultar();
        avisoVisible = false;
    }
}