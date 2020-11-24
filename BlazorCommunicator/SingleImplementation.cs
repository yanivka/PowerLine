using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorCommunicator
{
    internal class SingleImplementation<ApiInterface, ApiImplementation> : SingleImplementation where ApiImplementation: ApiInterface
    {

        public readonly string AssmblyName;
        public readonly ApiImplementation Implementation;
    }
    internal class SingleImplementation{}
}
