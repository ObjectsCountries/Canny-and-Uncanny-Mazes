using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wawa.Modules;
using Wawa.Extensions;
using Wawa.IO;

public class UncannyMaze : ModdedModule
{
    public KMSelectable arrowleft, arrowright, arrowup, arrowdown, maze, numbersButton, resetButton, appendButton;
    internal bool viewingWholeMaze = false;
    private Vector2 currentPosition, startingPosition;
    private string currentCoords, leftTile, rightTile, aboveTile, belowTile, goalCoords, startingCoords;
    private int xcoords, ycoords, xGoal, yGoal;
    public TextureGeneratorUncanny t;

    void Start()
    {

    }
}
