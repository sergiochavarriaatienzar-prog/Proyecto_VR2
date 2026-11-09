// Interactuable.cs
// Al apuntar al objeto sale el cartel "Pulsa el gatillo para interactuar";
// al apretar el gatillo escribe en la consola. Va en el objeto (el que tiene
// el Collider), junto a su XR Simple Interactable.
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;               // HoverEnterEventArgs, HoverExitEventArgs
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRSimpleInteractable
using UnityEngine.XR.Interaction.Toolkit.Interactors;   // XRBaseInputInteractor

[RequireComponent(typeof(XRSimpleInteractable))]
public class Interactuable : MonoBehaviour
{
    XRSimpleInteractable interactable;
    Collider col;                 // para saber dónde está la parte de arriba del objeto
    XRBaseInputInteractor mano;   // la mano que me está apuntando (null si ninguna)

    void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        col = GetComponentInChildren<Collider>();
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

    // Empiezo a apuntarle: me guardo la mano y saco el cartel encima del objeto
    void OnHoverEntered(HoverEnterEventArgs args)
    {
        mano = args.interactorObject as XRBaseInputInteractor;
        Vector3 arriba = col.bounds.center + Vector3.up * col.bounds.extents.y;
        AvisoInteractuar.instancia.Mostrar(arriba);
    }

    // Dejo de apuntarle: olvido la mano y escondo el cartel
    void OnHoverExited(HoverExitEventArgs args)
    {
        mano = null;
        AvisoInteractuar.instancia.Ocultar();
    }

    void Update()
    {
        // Si le estoy apuntando y aprieto el gatillo
        if (mano != null && mano.activateInput.ReadWasPerformedThisFrame())
        {
            Debug.Log("Has interactuado con " + name);
        }
    }
}