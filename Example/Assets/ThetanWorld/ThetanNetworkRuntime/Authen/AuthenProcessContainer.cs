using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.RestAPI.ThetanAuth
{
    public class AuthenProcessContainer : MonoBehaviour
    {
        [SerializeField] private WolffunIdAuthenProcess _wfidAuthenProcess;
        [SerializeField] private ThetanAppAuthenProcess _thetanAppAuthenProcess;

        public WolffunIdAuthenProcess WFIDAuthenProcess => _wfidAuthenProcess;

        public ThetanAppAuthenProcess ThetanAppAuthenProcess => _thetanAppAuthenProcess;
    }
}

