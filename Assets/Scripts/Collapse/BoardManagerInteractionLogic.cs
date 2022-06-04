using System;
using System.Collections;
using System.Collections.Generic;
using Collapse.Blocks;
using UnityEngine;

namespace Collapse
{
    /**
     * Partial class for separating the main functions that are needed to be modified in the context of this test
     */
    public partial class BoardManager
    {
        // Store the current tested block
        private Block testBlock = null;

        // Flag for bomb sequence
        public bool ActiveCombo = false;

        /**
         * Trigger a bomb
         */
        public void TriggerBomb(Bomb bomb)
        {
            // Don't start a bomb sequence if already in one
            if (ActiveCombo) return;

            // Activate combo sequence
            ActiveCombo = true;

            // Initialize lists and variables
            var tilesToDestroy = new List<Block>();
            var bombs = new List<Block>();
            var tested = new List<(int row, int col)>();
            var bombsDelay = 0f;
            var tilesDelay = 0.3f;
            var endDelay = 0.5f;

            // Find all related bombs "chain"
            FindBombs(bomb.GridPosition.x, bomb.GridPosition.y, tested, bombs);

            // Add the origin bomb
            bombs.Insert(0, bomb);

            // Loop through all founded bombs
            for (var i = 0; i < bombs.Count; i++)
            {
                // Trigger a founded bomb, in a delay
                bombs[i].Trigger(bombsDelay);

                // Find all surrounding tiles to be destroyed by this one bomb
                FindTiles(bombs[i].GridPosition.x, bombs[i].GridPosition.y, tilesToDestroy);

                // Destroy the founded tiles
                foreach (Block tile in tilesToDestroy)
                {
                    tile.Trigger(bombsDelay + tilesDelay);
                }

                // Add a delay between each bomb, in order to get a chain effect
                bombsDelay += 0.6f - EasingCubic(0.2f, 0.5f, (float)i / (float)bombs.Count);
            }

            // Regenerate the board after all the chain of bombs is finished
            StartCoroutine(RegenerateBoardBombDelay(bombsDelay + endDelay));
        }

        /**
         * Trigger a match
         */
        public void TriggerMatch(Block block)
        {
            // Find all blocks in this match
            var results = new List<Block>();
            var tested = new List<(int row, int col)>();
            FindChainRecursive(block.Type, block.GridPosition.x, block.GridPosition.y, tested, results);

            // Trigger blocks
            for (var i = 0; i < results.Count; i++)
            {
                results[i].Trigger(0);
            }

            // Regenerate
            ScheduleRegenerateBoard();
        }

        /**
        * Enumerator for delay regenerating the board
        */
        private IEnumerator RegenerateBoardBombDelay(float delay)
        {
            // Wait a chosen delay
            yield return new WaitForSeconds(delay);

            // Deactivate combo sequence
            ActiveCombo = false;

            // Regenerate
            ScheduleRegenerateBoard();
        }

        /**
         * Recursively collect all neighbors of same type to build a full list of blocks in this "chain" in the results list
         */
        private void FindChainRecursive(BlockType type, int col, int row, List<(int row, int col)> testedPositions, List<Block> results)
            {
            // Recursive stop Condition
            if (testedPositions.Contains((row, col)))
            {
                return;
            }

            // List of conditions to match
            List<Action> list = new List<Action>();
            list.Add(() => CheckUp(col, row));
            list.Add(() => CheckRight(col, row));
            list.Add(() => CheckDown(col, row));
            list.Add(() => CheckLeft(col, row));

            // Run the list of conditions
            foreach (Action action in list)
            {
                action.Invoke();
                // If a block exist in the check
                if (testBlock)
                {
                    // Now check if it's the same type as the current block
                    if (testBlock.Type == type)
                    {
                        // Add self to the array so we know to not check it again
                        testedPositions.Add((row, col));

                        // Call the recursive call on the other block, to check deeper
                        FindChainRecursive(type, testBlock.GridPosition.x, testBlock.GridPosition.y, testedPositions, results);
                    }
                }
            }

            /**
            * Add to the results array if the tested positions size is larger than 2
            */
            if (testedPositions.Count > 2)
            {
                // Add each block that was found in that group
                foreach ((int col, int row) pos in testedPositions)
                {
                    results.Add(blocks[pos.row, pos.col]);
                }

                // Add self
                results.Add(blocks[col, row]);
            }
        }

        /**
         * Collect bombs in all 8 directions
         */
        private void FindBombs(int col, int row, List<(int row, int col)> testedPositions, List<Block> bombs)
        {
            // Recursive stop condition
            if (testedPositions.Contains((row, col)))
            {
                return;
            }

            // List of conditions to match
            List<Action> list = new List<Action>();
            list.Add(() => CheckUp(col, row));
            list.Add(() => CheckUpRight(col, row));
            list.Add(() => CheckRight(col, row));
            list.Add(() => CheckDownRight(col, row));
            list.Add(() => CheckDown(col, row));
            list.Add(() => CheckDownLeft(col, row));
            list.Add(() => CheckLeft(col, row));
            list.Add(() => CheckUpLeft(col, row));

            // Loop through the list of actions
            foreach (Action action in list)
            {
                // Run an action
                action.Invoke();

                // Check if a block exist in this action
                if (testBlock)
                {
                    //Recursive call, if it's a bomb
                    if (testBlock.Type == BlockType.Bomb && !testedPositions.Contains((testBlock.GridPosition.y, testBlock.GridPosition.x)))
                    {
                        // Add the founded bomb to the bombs array
                        bombs.Add(testBlock);

                        // If it doesn't exist in the tested array, add it
                        if (!testedPositions.Contains((row, col)))
                        testedPositions.Add((row, col));

                        // Recursive call of the founded bomb
                        FindBombs(testBlock.GridPosition.x, testBlock.GridPosition.y, testedPositions, bombs);
                    }
                }
            }
        }

         /**
         * Collect tiles in all 8 directons
         */
        private void FindTiles(int col, int row, List<Block> results)
        {
            // List of conditions to match
            List<Action> list = new List<Action>();
            list.Add(() => CheckUp(col, row));
            list.Add(() => CheckUpRight(col, row));
            list.Add(() => CheckRight(col, row));
            list.Add(() => CheckDownRight(col, row));
            list.Add(() => CheckDown(col, row));
            list.Add(() => CheckDownLeft(col, row));
            list.Add(() => CheckLeft(col, row));
            list.Add(() => CheckUpLeft(col, row));

            // Loop through the list of actions
            foreach (Action action in list)
            {
                // Run an action
                action.Invoke();

                // If a block exist in this action, add it to the results
                if (testBlock && testBlock.Type != BlockType.Bomb)
                {
                    results.Add(testBlock);
                }
            }
        }

        // Straight directions checks
        private void CheckUp(int col, int row)
        {
            testBlock = (row < 10 && blocks[col, row + 1] != null) ? blocks[col, row + 1] : null;
        }

        private void CheckRight(int col, int row)
        {
            testBlock = (col < 10 && blocks[col + 1, row] != null) ? blocks[col + 1, row] : null;
        }

        private void CheckDown(int col, int row)
        {
            testBlock = (row > 0 && blocks[col, row - 1] != null) ? blocks[col, row - 1] : null;
        }

        private void CheckLeft(int col, int row)
        {
            testBlock = (col > 0 && blocks[col - 1, row] != null) ? blocks[col - 1, row] : null;
        }

        // Diagonal directions checks
        private void CheckUpRight(int col, int row)
        {
            testBlock = (col < 10 && row < 10 && blocks[col + 1, row + 1] != null) ? blocks[col + 1, row + 1] : null;
        }

        private void CheckUpLeft(int col, int row)
        {
            testBlock = (col > 0 && row < 10 && blocks[col - 1, row + 1] != null) ? blocks[col - 1, row + 1] : null;
        }

        private void CheckDownRight(int col, int row)
        {
            testBlock = (col < 10 && row > 0 && blocks[col + 1, row - 1] != null) ? blocks[col + 1, row - 1] : null;
        }

        private void CheckDownLeft(int col, int row)
        {
            testBlock = (col > 0 && row > 0 && blocks[col - 1, row - 1] != null) ? blocks[col - 1, row - 1] : null;
        }

        // Easing math formula
        private float EasingCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }
    }
}