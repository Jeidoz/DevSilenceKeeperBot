using System;

namespace DevSilenceKeeperBot.Exceptions
{
    public sealed class AddingDublicateRecord : ArgumentException
    {
        public AddingDublicateRecord() 
            : base() 
        {

        }

        public AddingDublicateRecord(string message)
            : base(message) 
        {
        
        }

        public AddingDublicateRecord(string message, Exception innerException) 
            : base(message, innerException) 
        { 
        
        }

        public AddingDublicateRecord(string message, string paramName)
            : base(message, paramName)
        {

        }

        public AddingDublicateRecord(string message, string paramName, Exception innerException)
            : base(message, paramName, innerException)
        {

        }
    }
}
