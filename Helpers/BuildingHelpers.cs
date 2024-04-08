using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildingHelpers {

    public static NPos RotateNode(this NPos n, bool quad){
        if (quad == true) { // Used only for rotating direct neighbours
            n = n switch {
                NPos.Up => NPos.Right,
                NPos.Right => NPos.Down,
                NPos.Down => NPos.Left,
                NPos.Left => NPos.Up,
                _ => NPos.Up,
            };
        } else {
            n = n switch { // Rotates all the neighbours
                NPos.Up => NPos.Right,
                NPos.Right => NPos.Down,
                NPos.Down => NPos.Left,
                NPos.Left => NPos.Up,
                NPos.LeftUp => NPos.RightUp,
                NPos.LeftDown => NPos.LeftUp,
                NPos.RightUp => NPos.RightDown,
                NPos.RightDown => NPos.LeftDown,
                _ => NPos.Up,
            };
        }
        return n;
    }
}
