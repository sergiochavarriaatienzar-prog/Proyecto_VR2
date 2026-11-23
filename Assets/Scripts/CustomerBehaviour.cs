using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRSimpleInteractable))]
public class CustomerBehaviour : MonoBehaviour
{
    enum Estado { CaminandoAlMostrador, Esperando, Yendose }

    [Header("Spawn y Movimiento")]
    [SerializeField] GameObject customerPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float velocidadMovimiento = 2f;
    [SerializeField] float distanciaRecorrido = 3f;
    [SerializeField] float distanciaSalida = 5f;
    [SerializeField] bool spawnearAlIniciar = true;

    [Header("Interaccion VR")]
    [SerializeField] float distanciaMaxima = 2.5f;

    [Header("Estado y Eventos")]
    [SerializeField] bool atendido = false;
    public UnityEvent alSerAtendido;

    [Header("Debug (solo lectura)")]
    [SerializeField] Estado estado = Estado.CaminandoAlMostrador;

    XRSimpleInteractable interactable;
    Collider col;
    XRBaseInputInteractor mano;

    Vector3 posicionInicial;
    Vector3 posicionMostrador;
    Vector3 direccionAvance;
    bool avisoVisible;
    bool esInstanciaSpawneada;

    public bool Atendido => atendido;

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        col = GetComponentInChildren<Collider>();
        atendido = false;
    }

    void Start()
    {
        if (spawnearAlIniciar && customerPrefab != null && !esInstanciaSpawneada)
        {
            SpawnearCliente();
        }
        else
        {
            ConfigurarRecorridoDesdePosicionActual();
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
        OcultarAviso();
    }

    public void SpawnearCliente()
    {
        Transform origen = spawnPoint != null ? spawnPoint : transform;
        GameObject instancia = Instantiate(customerPrefab, origen.position, origen.rotation);

        CustomerBehaviour comportamiento = instancia.GetComponent<CustomerBehaviour>();
        if (comportamiento == null)
        {
            comportamiento = instancia.AddComponent<CustomerBehaviour>();
        }

        comportamiento.esInstanciaSpawneada = true;
        comportamiento.velocidadMovimiento = velocidadMovimiento;
        comportamiento.distanciaRecorrido = distanciaRecorrido;
        comportamiento.distanciaSalida = distanciaSalida;
        comportamiento.distanciaMaxima = distanciaMaxima;
        comportamiento.spawnearAlIniciar = false;
        comportamiento.ConfigurarRecorridoDesdePosicionActual();

        // Si este objeto solo actuaba como punto de spawn vacío, se desactiva tras instanciar
        if (gameObject != instancia && GetComponent<MeshRenderer>() == null)
        {
            gameObject.SetActive(false);
        }
    }

    void ConfigurarRecorridoDesdePosicionActual()
    {
        atendido = false;
        posicionInicial = transform.position;
        direccionAvance = transform.forward.normalized;
        posicionMostrador = posicionInicial + direccionAvance * distanciaRecorrido;
        estado = Estado.CaminandoAlMostrador;
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        mano = args.interactorObject as XRBaseInputInteractor;
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        mano = null;
        OcultarAviso();
    }

    void Update()
    {
        switch (estado)
        {
            case Estado.CaminandoAlMostrador:
                MoverHacia(posicionMostrador, () =>
                {
                    estado = Estado.Esperando;
                    Debug.Log("Cliente: ha llegado y se queda esperando.", this);
                });
                break;

            case Estado.Esperando:
                if (atendido)
                {
                    IniciarSalida();
                }
                break;

            case Estado.Yendose:
                transform.position += -direccionAvance * velocidadMovimiento * Time.deltaTime;
                if (Vector3.Distance(posicionMostrador, transform.position) >= distanciaSalida)
                {
                    Destroy(gameObject);
                }
                break;
        }

        // Mostrar aviso solo cuando está esperando, cerca y apuntado
        bool puedoInteractuar = mano != null && EstaCerca() && estado == Estado.Esperando && !atendido;
        if (puedoInteractuar && !avisoVisible) MostrarAviso();
        else if (!puedoInteractuar && avisoVisible) OcultarAviso();

        // Interacción con el gatillo (igual que en Freidora)
        if (mano != null && mano.activateInput.ReadWasPerformedThisFrame())
        {
            if (puedoInteractuar)
            {
                MarcarComoAtendido();
            }
        }
    }

    void MoverHacia(Vector3 destino, System.Action alLlegar)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidadMovimiento * Time.deltaTime
        );

        if (Vector3.SqrMagnitude(transform.position - destino) <= 0.0001f)
        {
            transform.position = destino;
            alLlegar?.Invoke();
        }
    }

    public void MarcarComoAtendido()
    {
        if (atendido) return;

        atendido = true;
        OcultarAviso();
        Debug.Log("Cliente: marcado como atendido.", this);
        alSerAtendido.Invoke();
        IniciarSalida();
    }

    void IniciarSalida()
    {
        if (estado == Estado.Yendose) return;
        estado = Estado.Yendose;
    }

    bool EstaCerca()
    {
        if (Camera.main == null) return false;
        return Vector3.Distance(Camera.main.transform.position, transform.position) <= distanciaMaxima;
    }

    void MostrarAviso()
    {
        if (col == null || AvisoInteractuar.instancia == null) return;
        Vector3 arriba = col.bounds.center + Vector3.up * col.bounds.extents.y;
        AvisoInteractuar.instancia.Mostrar(arriba);
        avisoVisible = true;
    }

    void OcultarAviso()
    {
        if (!avisoVisible) return;
        if (AvisoInteractuar.instancia != null)
        {
            AvisoInteractuar.instancia.Ocultar();
        }
        avisoVisible = false;
    }
}