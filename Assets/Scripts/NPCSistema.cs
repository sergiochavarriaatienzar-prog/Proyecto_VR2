// NPCSistema.cs
// Sistema de preguntas muy básico con un cliente (NPC).
// Al apuntarle sale el cartel; con el gatillo se habla con él, una frase cada vez:
//   1er gatillo: Tú "¿Qué desea?"  -> (espera) ->  Cliente "Unas patatas, por favor."
//   (aquí iría el minijuego de las patatas)
//   2º gatillo:  Tú "Toma."        -> (espera) ->  Cliente "¡Gracias!"
//   Al rato avisa con el evento alTerminar y el spawner se encarga de que se vaya.
// Va en el cliente (el que tiene el Collider), junto a su XR Simple Interactable.
using System.Collections;                               // IEnumerator (corrutinas)
using TMPro;                                            // TMP_Text
using UnityEngine;
using UnityEngine.Events;                               // UnityEvent
using UnityEngine.XR.Interaction.Toolkit;               // HoverEnterEventArgs, HoverExitEventArgs
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRSimpleInteractable
using UnityEngine.XR.Interaction.Toolkit.Interactors;   // XRBaseInputInteractor

[RequireComponent(typeof(XRSimpleInteractable))]
public class NPCSistema : MonoBehaviour
{
    [Header("Bocadillo encima de la cabeza")]
    [SerializeField] GameObject bocadillo;   // el Canvas del cliente (se enciende y se apaga)
    [SerializeField] TMP_Text texto;         // el Text (TMP) que hay dentro

    [Header("Conversación")]
    [SerializeField] string pregunta = "¿Qué desea?";
    [SerializeField] string pedido = "Unas patatas, por favor.";
    [SerializeField] string entrega = "Toma.";
    [SerializeField] string gracias = "¡Gracias!";
    [SerializeField] float esperaEntreFrases = 1.5f; // segundos entre tu frase y la del cliente
    [SerializeField] float esperaAlFinal = 2f;       // segundos con el "¡Gracias!" antes de avisar

    // Se lanza al acabar la conversación. En el prefab se le arrastra
    // el método del cliente/spawner que hace que se vaya.
    public UnityEvent alTerminar;

    // En qué punto de la conversación estamos
    enum Paso { Preguntar, Entregar, Terminado }
    Paso paso = Paso.Preguntar;
    bool hablando;   // mientras salen las frases no se puede volver a pulsar

    XRSimpleInteractable interactable;
    Collider col;                 // para saber dónde está la parte de arriba del cliente
    XRBaseInputInteractor mano;   // la mano que me está apuntando (null si ninguna)

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        col = GetComponentInChildren<Collider>();
        bocadillo.SetActive(false);   // al principio no dice nada
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

    // Empiezo a apuntarle: me guardo la mano y saco el cartel
    void OnHoverEntered(HoverEnterEventArgs args)
    {
        mano = args.interactorObject as XRBaseInputInteractor;
        MostrarCartel();
    }

    // Dejo de apuntarle: olvido la mano y escondo el cartel
    void OnHoverExited(HoverExitEventArgs args)
    {
        mano = null;
        AvisoInteractuar.instancia.Ocultar();
    }

    void Update()
    {
        // Si le estoy apuntando, aprieto el gatillo y no está hablando ya
        if (mano != null && !hablando && mano.activateInput.ReadWasPerformedThisFrame())
        {
            if (paso == Paso.Preguntar)
            {
                paso = Paso.Entregar;
                StartCoroutine(Hablar(pregunta, pedido));
            }
            else if (paso == Paso.Entregar)
            {
                paso = Paso.Terminado;
                StartCoroutine(Hablar(entrega, gracias));
            }
        }
    }

    // Corrutina: saca tu frase, espera unos segundos y saca la del cliente.
    // (yield return new WaitForSeconds = "espera aquí y sigue luego")
    IEnumerator Hablar(string fraseTuya, string fraseCliente)
    {
        hablando = true;
        AvisoInteractuar.instancia.Ocultar();   // mientras habla no sale el cartel

        bocadillo.SetActive(true);
        texto.text = "Tú: " + fraseTuya;
        yield return new WaitForSeconds(esperaEntreFrases);
        texto.text = "Cliente: " + fraseCliente;

        if (paso == Paso.Terminado)
        {
            yield return new WaitForSeconds(esperaAlFinal);
            bocadillo.SetActive(false);
            alTerminar.Invoke();   // el spawner hace que se vaya
        }

        hablando = false;
        if (mano != null) MostrarCartel();   // si le sigues apuntando, vuelve el cartel
    }

    // El cartel cambia según lo que toca hacer (y no sale si ya ha terminado)
    void MostrarCartel()
    {
        if (hablando || paso == Paso.Terminado) return;

        string mensaje = paso == Paso.Preguntar
            ? "Pulsa el gatillo para preguntar"
            : "Pulsa el gatillo para entregar";
        Vector3 arriba = col.bounds.center + Vector3.up * col.bounds.extents.y;
        AvisoInteractuar.instancia.Mostrar(arriba, mensaje);
    }

    // Mientras se ve, el bocadillo mira siempre al jugador (igual que el cartel)
    void LateUpdate()
    {
        if (bocadillo.activeSelf)
        {
            Vector3 direccion = bocadillo.transform.position - Camera.main.transform.position;
            direccion.y = 0;
            bocadillo.transform.rotation = Quaternion.LookRotation(direccion);
        }
    }
}
