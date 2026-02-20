namespace DataTransferObject.DTO.Response
{
    public class DTOGenericResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        // Constructor for easy initialization
        public DTOGenericResponse(int code, string message, T data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}