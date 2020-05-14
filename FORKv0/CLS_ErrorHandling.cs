using System;
static class CustomExceptions
{
    public class unknownMappingException : Exception
    {

        public unknownMappingException()
        {
        }

    }

    public class dateRangeException : Exception
    {

        public dateRangeException()
        {
        }

        public dateRangeException(string message) : base(message)
        {
        }

        public dateRangeException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class dataConnectionException : Exception
    {

        public dataConnectionException()
        {
        }

        public dataConnectionException(string message) : base(message)
        {
        }

        public dataConnectionException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class genericMessageException : Exception
    {

        public genericMessageException()
        {
        }

        public genericMessageException(string message) : base(message)
        {
        }

        public genericMessageException(string message, Exception inner) : base(message, inner)
        {
        }
    }


    public class columnStubbingException : Exception
    {

        public columnStubbingException()
        {
            //   Settings.Default.AdvancedSettings_isAvailabilityMode = True
        }

        public columnStubbingException(string message) : base(message)
        {
        }

        public columnStubbingException(string message, Exception inner) : base(message, inner)
        {
        }
    }

}
