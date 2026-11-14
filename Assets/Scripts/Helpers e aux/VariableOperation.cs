using System;
using UnityEngine;
/// <summary>
/// Stores the numeric variable type and operation to apply.
/// The upgrade system uses this to select and modify a GlobalStats field.
/// </summary>

[Serializable]
public struct VariableOperation<T> where T : struct, IComparable
{
    public OperationType operationType;
    /// the lower bound of this interval
    [Tooltip("the lower bound of this interval")]
    public T variable;

    public VariableOperation(T variable, OperationType operationType) : this()
    {
        this.variable = variable;
        this.operationType = operationType;

    }

}