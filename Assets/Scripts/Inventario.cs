
using UnityEngine;

public class Inventario : MonoBehaviour
{
    public static Inventario instancia;

    //Inspector
    [SerializeField] int patatasCocinadas;

    public int PatatasCocinadas => patatasCocinadas;

    void Awake()
    {
        instancia = this;
    }

    public void AnadirPatataCocinada()
    {
        patatasCocinadas++;
        Debug.Log("Inventario: patatas cocinadas = " + patatasCocinadas);
    }
}
