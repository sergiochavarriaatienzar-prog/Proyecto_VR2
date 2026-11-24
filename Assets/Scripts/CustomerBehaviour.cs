// CustomerBehaviour.cs (versión de tu amigo, solo con los cambios para que encaje con NPCSistema)
// CAMBIOS:
//  1. Fuera toda la parte de interacción (hover, mano, cartel, gatillo, distanciaMaxima
//     y el RequireComponent de XRSimpleInteractable): eso ahora lo hace NPCSistema.
//  2. Al llegar al mostrador enciende NPCSistema (antes no se puede hablar con él).
//  3. MarcarComoAtendido se llama desde el evento alTerminar de NPCSistema (en el prefab).
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Estado y Eventos")]
    [SerializeField] bool atendido = false;
    public UnityEvent alSerAtendido;

    [Header("Debug (solo lectura)")]
    [SerializeField] Estado estado = Estado.CaminandoAlMostrador;

    Vector3 posicionInicial;
    Vector3 posicionMostrador;
    Vector3 direccionAvance;
    bool esInstanciaSpawneada;

    public bool Atendido => atendido;

    void Awake()
    {
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

    void Update()
    {
        switch (estado)
        {
            case Estado.CaminandoAlMostrador:
                MoverHacia(posicionMostrador, () =>
                {
                    estado = Estado.Esperando;
                    Debug.Log("Cliente: ha llegado y se queda esperando.", this);

                    // CAMBIO: ya ha llegado, ahora sí se puede hablar con él
                    NPCSistema conversacion = GetComponent<NPCSistema>();
                    if (conversacion != null) conversacion.enabled = true;
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

    // CAMBIO: lo llama el evento alTerminar de NPCSistema (después del "¡Gracias!")
    public void MarcarComoAtendido()
    {
        if (atendido) return;

        atendido = true;
        Debug.Log("Cliente: marcado como atendido.", this);
        alSerAtendido.Invoke();
        IniciarSalida();
    }

    void IniciarSalida()
    {
        if (estado == Estado.Yendose) return;
        estado = Estado.Yendose;
    }
}
