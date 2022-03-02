using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class GameBoard : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float width;
    [SerializeField] private float height;

    [SerializeField] private float spacing;
    [SerializeField] private Vector2 padding;
    [SerializeField] private float movingSpeed = 0.2f;

    [SerializeField] private List<BlockType> blockTypes;
    public BlockType GetBlockType(int value) => blockTypes.FirstOrDefault(b => b.value == value);

    private List<NodeBoard> nodes;
    private NodeBoard GetNode(Vector3 pos) => nodes.FirstOrDefault(n => n.Pos == pos);

    private List<BlockBoard> blocks;

    public enum State
    {
        GeneratingLevel,
        SpawningBlock,
        WaitingInput,
        Moving
    }
    private State currentState;

    [Header("Reference")]
    [SerializeField] private SpriteRenderer boardRenderer;
    [SerializeField] private NodeBoard nodePrefab;
    [SerializeField] private BlockBoard blockPrefab;

    [SerializeField] private Transform nodesParent;
    [SerializeField] private Transform blocksParent;

    private void Awake()
    {
        GenerateNodes();
    }

    private void Update()
    {
        if (currentState != State.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) ShiftBlock(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ShiftBlock(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) ShiftBlock(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) ShiftBlock(Vector2.down);
    }

    private void GenerateNodes()
    {
        currentState = State.GeneratingLevel;
        nodes = new List<NodeBoard>();
        blocks = new List<BlockBoard>();
        var position = new Vector3(-width + 1, -height + 1);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var node = Instantiate(nodePrefab, nodesParent);
                node.Pos = position + new Vector3(i, j) * spacing;
                nodes.Add(node);
            }
        }

        boardRenderer.size = new Vector2(width, height) + padding;
        SpawnBlocks(2);
    }

    private void SpawnBlocks(int amount = 1)
    {
        currentState = State.SpawningBlock;
        var freeNodes = nodes.Where(n => n.occupiedBlock == null).OrderBy(n => Random.value).ToList();
        foreach (var node in freeNodes.Take(amount))
        {
            SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
        }

        if(freeNodes.Count == 0)
        {
            Debug.Log("Game Over");
            return;
        }
        currentState = State.WaitingInput;
    }

    private void SpawnBlock(NodeBoard node, int value)
    {
        var block = Instantiate(blockPrefab, blocksParent);
        block.transform.localPosition = node.Pos;

        block.Init(GetBlockType(value));
        block.SetNode(node);
        blocks.Add(block);
    }

    private void ShiftBlock(Vector2 dir)
    {
        currentState = State.Moving;
        var orderBlocks = blocks.OrderBy(b => b.Node.Pos.x).ThenBy(b => b.Node.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderBlocks.Reverse();

        foreach (var block in orderBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetNode(next);
                var possibleNode = GetNode(next.Pos + (Vector3)dir * spacing);
                if(possibleNode != null)
                {
                    var isOccupied = possibleNode.occupiedBlock != null;

                    if (isOccupied && possibleNode.occupiedBlock.CanMerging(block.Type.value)) possibleNode.occupiedBlock.MergeBlock(block);
                    else if (!isOccupied) next = possibleNode;
                }
            } while (next != block.Node);
        }

        Sequence sequences = DOTween.Sequence();
        foreach (var block in orderBlocks)
        {
            sequences.Insert(0, block.transform.DOLocalMove(block.Node.Pos, movingSpeed).SetEase(Ease.InQuad));
        }
        sequences.OnComplete(() =>
        {
            var mergingBlocks = orderBlocks.Where(b => b.MergingBlock != null).ToList();
            foreach (var block in mergingBlocks)
            {
                MergingBlock(block, block.MergingBlock);
            }
            SpawnBlocks();
        });
    }

    private void MergingBlock(BlockBoard baseBlock, BlockBoard mergeBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Type.value * 2);
        RemoveBlock(baseBlock);
        RemoveBlock(mergeBlock);
    }

    private void RemoveBlock(BlockBoard block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
}
