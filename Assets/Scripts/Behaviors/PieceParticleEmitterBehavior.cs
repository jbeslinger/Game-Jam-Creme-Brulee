using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(ParticleSystemRenderer))]
public class PieceParticleEmitterBehavior : MonoBehaviour
{
    #region Fields
    public PieceBehavior parentPiece;

    public Material softSprite, hardSprite;
    #endregion

    #region Members
    ParticleSystem _particleSystem;
    ParticleSystemRenderer _particleSystemRenderer;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        if (parentPiece == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(parentPiece)));
        }
        else if (softSprite == null || hardSprite == null)
        {
            throw new UnassignedReferenceException(string.Format("Please be sure to assign both {0} and {1} fields in the inspector.", nameof(softSprite), nameof(hardSprite)));
        }

#pragma warning disable CS0618 // Type or member is obsolete
        _particleSystem.startColor = PieceBehavior.colorDict[parentPiece.Type];
#pragma warning restore CS0618 // Type or member is obsolete

        parentPiece.OnBreak +=() =>
        {
            if (parentPiece.Hardened)
            {
                _particleSystemRenderer.material = hardSprite;
            }
            else
            {
                _particleSystemRenderer.material = softSprite;
            }
            _particleSystem.Emit(10);
        };
    }
    #endregion
}
