# Canny Maze

## Ruleseed Plans

* Sum Maze: Keep for some ruleseeds, replace with either of the following for others: (**IMPLEMENTED**)
  * Average Maze: Sum all orthogonally adjacent tiles, take the average, and round to the nearest whole number, then modulo 7 and add 1. Whichever surrounding tile is closest or equal to this number is the tile to navigate to.
  * Digital Maze: Sum all orthogonally adjacent tiles and take the digital root, then modulo 7 and add 1. Whichever surrounding tile is closest or equal to this number is the tile to navigate to.
* Compare Maze: Sometimes up/left & down/right, sometimes up/right and down/left, sometimes whichever is smaller
* Movement Maze: Sometimes the number of movements made rather than tiles traversed (**IMPLEMENTED**)
* Binary Maze: Keep for some ruleseeds, change to "any two bits" for some, replace with the following for others: (**IMPLEMENTED**)
  * Fours Maze: Take the base-4 representation of each surrounding tile. Whichever tiles provide the number that is in the majority are safe to navigate to.
* Avoid Maze: Swap around numbers
* Strict Maze: Sometimes prime/composite rather than odd/even (1 is composite to balance things out), sometimes move right to lower or left to higher tiles, sometimes up/down and left/right swap mixed with changed conditions
* Walls Maze: Different wall generation
* Swap around the maze types associated with each image
