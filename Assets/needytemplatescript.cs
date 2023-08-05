using Wawa.Modules;
using Wawa.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class needytemplatescript:ModdedModule{
    
    public KMSelectable thing;

    protected override void Awake(){
        Get<KMNeedyModule>().Set(
            onNeedyActivation:OnNeedyActivation,
            onNeedyDectivation:OnNeedyDeactivation,
            onTimerExpired:OnTimerExpired);
    }

    protected bool Solve(){
        GetComponent<KMNeedyModule>().OnPass();
        return false;
    }

    protected void OnNeedyActivation(){

    }

    protected void OnNeedyDeactivation(){

    }

    protected void OnTimerExpired(){
        Strike("Time Up!");
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