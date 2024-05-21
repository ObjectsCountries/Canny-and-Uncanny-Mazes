using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wawa.TwitchPlays;
using Wawa.TwitchPlays.Domains;

public class UncannyMazeTP : Twitch<UncannyMaze>
{
    [Command("")]
    public IEnumerable<Instruction> input()
    {
        yield return null;
    }

    public override IEnumerable<Instruction> ForceSolve()
    {
        yield return Instruction.Pause;
    }
}
