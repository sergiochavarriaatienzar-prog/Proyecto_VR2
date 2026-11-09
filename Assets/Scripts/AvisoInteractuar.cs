// AvisoInteractuar.cs
// El cartel "Pulsa el gatillo para interactuar". Solo hay uno en la escena
// y lo usan todos los Interactuable: se pone encima del objeto al que apuntas
// y se esconde al dejar de apuntar. Va en el Canvas (World Space) del cartel.
using UnityEngine;

public class AvisoInteractuar : MonoBehaviour
{
    // static: es del script, no de un objeto, así cualquier Interactuable
    // lo encuentra sin tener que arrastrar nada en el Inspector.
    public static AvisoInteractuar instancia;

    [SerializeField] float altura = 0.15f;   // metros por encima del objeto

    // El Canvas tiene que estar ACTIVO al empezar: aquí se apunta y se esconde.
    void Awake()
    {
        instancia = this;
        gameObject.SetActive(false);
    }

    public void Mostrar(Vector3 arribaDelObjeto)
    {
        transform.position = arribaDelObjeto + Vector3.up * altura;
        gameObject.SetActive(true);
    }

    public void Ocultar()
    {
        gameObject.SetActive(false);
    }

    // Mientras se ve, gira para mirar siempre a la cabeza del jugador
    void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
