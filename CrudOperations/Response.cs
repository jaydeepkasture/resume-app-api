using System;
using System.Collections.Generic;

namespace CrudOperations
{
    public class ResponseRunTimeError
    {
        public bool Status { get; set; }
        public string UserMessage { get; set; }
        public string DeveloperMessage { get; set; }
        public string Data { get; set; }
    }

    public class Response
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
    }

    public class Response<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ResponseList<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
        public int RecordsFiltered { get; set; }
    }
    public class ResponseList<T1, T2>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<T1> Data { get; set; }
        public T2 Data2 { get; set; }
        public int TotalRecords { get; set; }
        public int RecordsFiltered { get; set; }
    }
    public class ClsResponseObj
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
    public class ClsObjectResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public long? TotalRecords { get; set; } = 0;
    }
    public class ClsMultipleTabResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public IEnumerable<T> Data2 { get; set; }
        public IEnumerable<T> Data3 { get; set; }
        public IEnumerable<T> Data4 { get; set; }
        public IEnumerable<T> Data5 { get; set; }
        public IEnumerable<T> Data6 { get; set; }
        public IEnumerable<T> Data7 { get; set; }
        // public int TotalRecords { get; set; }
        //public int RecordsFiltered { get; set; }
    }

    public class ApiResponse<T> : Response<T>
    {
        public byte[] FileByte { get; set; }
    }


    public class ResponseListWithIds<T> : ResponseList<T>
    {
        public List<Guid> ClientIds { get; set; } = new List<Guid>();
        public List<Guid> PayerIds { get; set; } = new List<Guid>();
    }

}