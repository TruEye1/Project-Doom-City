using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemigoIA enemigo;

    private void Awake()
    {
        enemigo = GetComponentInParent<EnemigoIA>();
    }

    public void ActivarHitbox()
    {
        if (enemigo != null)
        {
            enemigo.ActivarHitbox();
        }
    }

    public void DesactivarHitbox()
    {
        if (enemigo != null)
        {
            enemigo.DesactivarHitbox();
        }
    }

    public void FinAtaque()
    {
        if (enemigo != null)
        {
            enemigo.FinAtaque();
        }
    }

    public void FinGolpe()
    {
        if (enemigo != null)
        {
            enemigo.FinGolpe();
        }
    }

    public void FinKnockdown()
    {
        if (enemigo != null)
        {
            enemigo.FinKnockdown();
        }
    }

    public void Levantarse()
    {
        if (enemigo != null)
        {
            enemigo.Levantarse();
        }
    }

    public void FinalizarMuerte()
    {
        if (enemigo != null)
        {
            enemigo.FinalizarMuerte();
        }
    }

    public void FinInvulnerabilidadKnockdown()
    {
        if (enemigo != null)
        {
            enemigo.FinInvulnerabilidadKnockdown();
        }
    }

    public void FinKnockback()
    {
        if (enemigo != null)
        {
            enemigo.FinKnockback();
        }
    }
}
