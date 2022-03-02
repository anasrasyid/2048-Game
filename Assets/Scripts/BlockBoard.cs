using TMPro;
using UnityEngine;

public class BlockBoard : MonoBehaviour
{
    public BlockType Type { get; private set; }
    public NodeBoard Node { get; private set; }

    private bool isMerging;
    public BlockBoard MergingBlock { get; private set; }
    public bool CanMerging(int value) => value == Type.value && !isMerging && MergingBlock == null;

    [SerializeField] private TextMeshPro displayText;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Init(BlockType type)
    {
        Type = type;
        
        displayText.text = Type.value.ToString();
        spriteRenderer.color = Type.color;
    }

    public void SetNode(NodeBoard node)
    {
        if (Node != null) Node.occupiedBlock = null;

        Node = node;
        Node.occupiedBlock = this;
    }

    public void MergeBlock(BlockBoard blockToMerge)
    {
        MergingBlock = blockToMerge;
        MergingBlock.Node.occupiedBlock = null;
        MergingBlock.isMerging = true;
    }
}