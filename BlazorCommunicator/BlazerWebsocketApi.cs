using System;
using System.Collections.Generic;

namespace BlazorCommunicator
{
    public class BlazerWebsocketApi
    {

        private List<SingleImplementation> implementations;
        public BlazerWebsocketApi()
        {

        }

        public void Implement<ApiInterface, ApiImplementation>(ApiImplementation implementation) where ApiImplementation : ApiInterface
        {
            
        }
    }
}
