using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PieceParticleEmitterBehavior : MonoBehaviour
{
    #region Fields
    public PieceBehavior parentPiece;
    #endregion

    #region Members
    ParticleSystem _particleSystem;
    #endregion

    #region Unity Methods
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (parentPiece == null)
        {
            throw new UnassignedReferenceException(string.Format("Please assign the {0} field in the inspector.", nameof(parentPiece)));
        }

#pragma warning disable CS0618 // Type or member is obsolete
        _particleSystem.startColor = parentPiece.colorDict[parentPiece.Type];
#pragma warning restore CS0618 // Type or member is obsolete

        parentPiece.OnBreak += () => { _particleSystem.Emit(10); };
    }
    #endregion
}
