namespace ResumeInOneMinute.Domain.Model;

public class Response
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }

    public class Response<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public T Data { get; set; } = default!;
    }

    public class ResponseList<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int RecordsFiltered { get; set; }
    }