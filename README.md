# Plans For Uncanny Maze

* Cruel-red background
* Backtracking **not** automatically allowed, still no wraparound, still no penalty for trying to go past walls
* 4×4, 5×5 or 6×6
* Because images are disturbing, add option to blur them in settings (`blurImageThreshold`: enter a number to blur images of that number and greater, and have number written on blurred textures; enter anything less than 1 or greater than 10 to not blur any images)

## Navigation

* Images labeled 0-9
* Maze type changes every tile
* Additional button labeled “Append”
* Note: Focus more on the entirety of the maze rather than surrounding tiles

## Maze Types

* Goal Maze
  * Whichever surrounding tile(s) are closest to the goal number are okay to navigate to
* Total Maze
  * Sum up entirety of the maze, modulo 10 and add 1
  * Whichever surrounding tile(s) are closest to this number are okay to navigate to
* Cross Maze
  * Sum current row and current column, multiply them, modulo 10 and add 1
  * Whichever surrounding tile(s) are closest to this number are okay to navigate to
* Border Maze
  * Individually sum up the leftmost column, rightmost column, highest row and lowest row (do not modulo 10 or add 1)
  * Whichever direction(s) are associated with the column(s)/row(s) of the highest sum number are okay to navigate towards
* Corners Maze
  * Take numbers modulo dimensions of corner tiles, make every possible combination of \[x, y\] coordinates (includes swapping numbers)
  * Whichever surrounding tile(s) are in any of these coordinates are okay to navigate to

## Maze Sizes

### 4×4

* For every quadrant of the module, concatenate each digit to get a 4-digit number
  * Note: for some ruleseeds, use rows or columns
* Convert each number to its 14-bit binary equivalent
* Prepend 1 if the goal is in the quadrant, 0 if not
* Same with starting position (starting position bit goes to the left of goal bit)
* Concatenate each string of bits to get a 64-bit string
* Convert 64-bit string to unsigned long
* For each digit in unsigned long:
  * Find tile with that digit closest in Manhattan distance to last appended tile (if no tiles have been appended, use starting tile)
  * Press “Append” while on that tile to append it
  * Press “Append” on the goal to submit sequence
* Notes:
  * According to calculations, resulting unsigned long could range from 49,152 (5 digits) to 16,649,569,293,445,310,223 (20 digits)

### 5×5

* Create playfair cipher table:
  * First 0 in reading order is replaced with A, second 0 replaced with B, etc.
  * After all 0s have been replaced, continue with 1s
  * Forgo Z
  * Repeat until every tile is replaced with a letter
* The table used for submission is different; alphabetical order in the arrangement determined by the goal number (starting with 1 for the purposes of this draft)
  * LRUD means left to right, from the uppermost row to the lowermost row
  * DULR means down to up, from the leftmost column to the rightmost column
* Word to be encrypted is also determined by goal number
* Click “Append” on the tile associated with each letter from the encrypted word
* Click “Append” on the goal to submit the sequence

1. SU PR GM NG (RLUD)
2. UN CA NX NY (LRUD)
3. TR AC TO RX (LRDU)
4. IN CR ED BL (RLDU)
5. ON LY OH IO (UDLR)
6. SK IB DI TL (UDRL)
7. JM BO JO SH (DULR)
8. WH IS TL NG (DURL)
9. FO RG OR XD (LRUD, then RLUD every other row)
10. TR OL FA CE (RLUD, then LRUD every other row)

### 6×6

* Assign letters to tiles similarly to 5×5, but this time the first of each tile is given its number (0, A, B, C, 1, D, E, etc.)
* Convert special number into base-36 based on sum of all tiles
* Append every tile of the converted number and submit on goal tile

