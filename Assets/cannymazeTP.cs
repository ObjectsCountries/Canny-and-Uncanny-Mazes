using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wawa.Modules;
using Wawa.Extensions;
using Wawa.TwitchPlays;
using Wawa.TwitchPlays.Domains;

public sealed class cannymazeTP:Twitch<_cannymaze> {
    void Start(){
        Module.GetComponent<KMBombModule>().Add(onActivate:()=>{
            if(IsTP)
                Module.animSpeed=2;
        },
        onPass:()=>{
            if(!IsTP&&Module.music)
                Module.Play(new Sound("song"+Module.currentTile));
        });
    }
    [Command("")]
	IEnumerable<Instruction> Press(string movement){
        List<KMSelectable> buttons=new List<KMSelectable>();
        if(!Module.mazeGenerated)
            yield break;
		movement=movement.ToLowerInvariant();
		switch(movement){
            case "m":
            case "maze":
                yield return new KMSelectable[]{Module.maze};
				yield break;
            case "n":
            case "numbers":
                if(!Module.viewingWholeMaze)
                    buttons.Add(Module.maze);
                buttons.Add(Module.numbersButton);
                yield return buttons.ToArray();
				yield break;
            case "reset":
                if(Module.viewingWholeMaze)
                    buttons.Add(Module.maze);
                buttons.Add(Module.resetButton);
                yield return buttons.ToArray();
				yield break;
            default:
                break;
		}
        buttons.Clear();
		if(Module.viewingWholeMaze)
            buttons.Add(Module.maze);
        foreach(char c in movement){
            switch(c){
                case 'u':
                    buttons.Add(Module.arrowup);
                    break;
                case 'd':
                    buttons.Add(Module.arrowdown);
                    break;
                case 'l':
                    buttons.Add(Module.arrowleft);
                    break;
                case 'r':
                    buttons.Add(Module.arrowright);
                    break;
                case ' ':
                    break;
                default:
                    yield return buttons.ToArray();
                    yield break;
            }
            yield return buttons.ToArray();
        }
	}

	public override IEnumerator TwitchHandleForcedSolve(){
        if(!Module.mazeGenerated)
            yield break;
        if(Module.tookTooLong){
            Module.numbersButton.OnInteract();
            yield break;
        }
        if(Module.viewingWholeMaze)
			Module.maze.OnInteract();
        Module.resetButton.OnInteract();
        foreach(string dir in Module.correctPath){
            switch(dir){
                case "up":
                    Module.arrowup.OnInteract();
                    break;
                case "down":
                    Module.arrowdown.OnInteract();
                    break;
                case "left":
                    Module.arrowleft.OnInteract();
                    break;
                case "right":
                    Module.arrowright.OnInteract();
                    break;
                default:
                    yield break;
            }
        }
	}

    public override IEnumerable<Instruction> ForceSolve(){
        TwitchHandleForcedSolve();
        yield return null;
    }
}