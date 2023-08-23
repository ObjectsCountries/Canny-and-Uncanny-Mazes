# Canny Maze

## Current Plan

* Maze with the cannys 1-7
* Dimensions of the maze range from 5 to 8, randomly generated
* No wraparound, also no penalty for trying to go past the edge
* I need to make it distinct enough so that I can't just replace the cannys with numbers to get the exact same concept

## To-Do

* Implement button on module to view numbers instead of cannys when viewing entire maze (makes communication far easier)
* Ask for help implementing everything about solving the module
* Also ask for help with making mazes for manual
* Add enum of maze types (including ruleseed-exclusive)

```cs
private enum MazeTypes{
  Sum=1,
  Compare=2,
  Movement=3,
  Binary=4,
  Avoid=5,
  Strict=6,
  Walls=7,
  Average=-1,
  Digital=-2,
  Fours=-3//these last three are exclusive to ruleseeds
}
```

## Ruleseed

* Sum Maze: Keep for some ruleseeds, replace with either of the following for others:
  * Average Maze
    : Sum all orthogonally adjacent tiles, take the average, and round to the nearest whole number, then modulo 7 and add 1. Whichever surrounding tile is closest or equal to this number is the tile to navigate to.
  * Digital Maze
    : Sum all orthogonally adjacent tiles and take the digital root, then modulo 7 and add 1. Whichever surrounding tile is closest or equal to this number is the tile to navigate to.
* Compare Maze: Sometimes up/left & down/right, sometimes up/right and down/left, sometimes whichever is smaller
* Movement Maze: Sometimes the number of movements made rather than tiles traversed
* Binary Maze: Keep for some ruleseeds, change to "any two bits" for some, replace with the following for others:
  * Fours Maze
    : Take the base-4 representation of each surrounding tile. Whichever tiles provide the number that is in the majority are safe to navigate to.
* Avoid Maze: Swap around numbers
* Strict Maze: Sometimes prime/composite rather than odd/even (1 is composite to balance things out), sometimes move right to lower or left to higher tiles, sometimes up/down and left/right swap mixed with changed conditions
* Walls Maze: Different wall generation
* Swap around the maze types associated with each image

## Twitch Plays Support

!{0} u/d/l/r to move up/down/left/right respectively; multiple can be strung together (i.e. !{0} rrulludr). !{0} m/maze to toggle view of the whole maze, !{0} n/numbers to toggle view of numbers when showing whole maze.
