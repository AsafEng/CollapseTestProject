using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Collapse.Blocks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Collapse {
    /**
     * BoardManager keeps track and manages creation deletion and interaction of all game pieces.
     * There should be no reason to modify this file in the context of this test.
     */
    public partial class BoardManager : MonoBehaviour {
        [SerializeField]
        [Description("List of Block Prefabs, code assumes BlockType Enum Ordinal will match the indices in this list.")]
        private List<Block> Prefabs;

        [SerializeField]
        private Vector2Int BoardSize;

        [SerializeField]
        private Vector2 Spacing;

        [SerializeField]
        private float BoardRegenerationDelay;

        // Our grid of blocks
        private Block[,] blocks;

        // Keep count of how many blocks exist in each column
        private int[] colCounters;
        private Coroutine scheduledRegeneration;

        // Singleton
        public static BoardManager Instance { get; private set; } 

        private void Awake() {
            Instance = this;
            // Init arrays
            blocks = new Block[BoardSize.x, BoardSize.y];
            colCounters = new int[BoardSize.x];
        }

        private void Start() {
            // Fill board on start
            ScheduleRegenerateBoard();
            Debug.Log("Remove me for bonus points!");
        }

        /**
         * Clear block from local arrays, remember to destroy the GameObject yourself!
         */
        public void ClearBlockFromGrid(Block block) {
            // Clear grid
            blocks[block.GridPosition.x, block.GridPosition.y] = null;
            colCounters[block.GridPosition.x]--;
        }

        /**
         * Returns random block type
         */
        private static BlockType RandomBlockType() {
            var type = (BlockType) Random.Range(0, Enum.GetValues(typeof(BlockType)).Length);
            return type;
        }

        /**
         * Create a block GameObject of specified type
         */
        private Block CreateBlock(BlockType type, int col, int row) {
            var block = Instantiate(Prefabs[(int) type], transform);
            block.transform.localPosition = GridLocalPosition(col, row);
            block.GridPosition.x = col;
            block.GridPosition.y = row;
            blocks[col, row] = block;
            colCounters[col]++;
            return block;
        }

        /**
         * Returns a Vector3 representing the local position of a grid cell 
         */
        private Vector3 GridLocalPosition(int col, int row) {
            return new Vector3((col - BoardSize.x / 2) * Spacing.x, (row - BoardSize.y / 2) * Spacing.y);
        }

        /**
         * Coroutine that waits a little and then regenerates all missing blocks on the board.
         * This should NOT be called directly but scheduled with ScheduleRegenerateBoard()
         */
        private IEnumerator RegenerateBoard() {
            yield return new WaitForSeconds(BoardRegenerationDelay);
            
            for (var col = 0; col < BoardSize.x; col++) {
                for (var row = 0; row < BoardSize.y; row++) {
                    if (blocks[col, row] == null) {
                        CreateBlock(RandomBlockType(), col, row);
                    }
                }
            }

            scheduledRegeneration = null;
        }

        /**
         * Schedules a board regeneration after a delay, cancels existing pending schedules
         */
        private void ScheduleRegenerateBoard() {
            if (scheduledRegeneration != null) {
                StopCoroutine(scheduledRegeneration);
                scheduledRegeneration = null;
            }
            scheduledRegeneration = StartCoroutine(RegenerateBoard());
        }
    }
}