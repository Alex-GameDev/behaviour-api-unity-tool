using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class BehaviourGraphVisualRunner : MonoBehaviour
    {
        [SerializeField] BehaviourGraphAsset _graphAsset;

        private void Awake()
        {
            // Build the BehaviourGraphAsset into the _graph contained
            _graphAsset.Build();
        }


        // Start is called before the first frame update
        void Start()
        {
            _graphAsset.Graph.Start();
        }

        // Update is called once per frame
        void Update()
        {
            _graphAsset.Graph.Update();
        }
    }
}
