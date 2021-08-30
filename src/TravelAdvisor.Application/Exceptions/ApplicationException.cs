using System;

namespace TravelAdvisor.Application.Exceptions
{
    public abstract class ApplicationException : Exception
    {
        public ApplicationException(string message ) : base(message)
        {
            
        }
    }

    public class ApplicationNotFoundException : ApplicationException
    {
        public ApplicationNotFoundException(string message) : base(message)
        {
            
        }
    }
    
    public class ApplicationValidationException : ApplicationException
    {
        public ApplicationValidationException(string message) : base(message)
        {
            
        }
    }
    
    

}