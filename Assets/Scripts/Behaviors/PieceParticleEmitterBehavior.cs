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

        _particleSystem.startColor = parentPiece.colorDict[parentPiece.Type];

        parentPiece.OnBreak += () => { _particleSystem.Emit(10); };
    }
    #endregion
}
