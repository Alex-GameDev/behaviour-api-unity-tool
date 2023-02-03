using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissingNode : Node
{
    public override Type ChildType => typeof(MissingNode); //Unvalid type

    public override int MaxInputConnections => -1;

    public override int MaxOutputConnections => -1;
}
