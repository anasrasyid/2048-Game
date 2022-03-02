using UnityEngine;

public class NodeBoard : MonoBehaviour
{
    public Vector3 Pos
    {
        get => transform.localPosition; 
        set => transform.localPosition = value; 
    }

    public BlockBoard occupiedBlock = null;
}
