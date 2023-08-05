using Wawa.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class _cannyScript:ModdedModule{

    public KMSelectable thing;

    //place module stuff like buttons here

    internal bool moduleSolved;

	protected override void Awake(){

    }

    void Start(){

	}

#pragma warning disable 414
    private readonly string TwitchHelpMessage=@"Use !{0} x to interact with the module.";
    private readonly string TwitchManualCode="https://ktane.timwi.de/HTML/Template.html";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command){
        yield return null;//placeholder
    }

    IEnumerator TwitchHandleForcedSolve(){
        yield return null;//placeholder
    }
}