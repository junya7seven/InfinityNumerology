namespace InfinityNumerology.DataSource.Model
{
    public class RequestCount
    {
        public int request_count_id {  get; set; }
        public long user_id { get; set; }
        public int count { get; set; }
        public string command_name { get; set; }
        public DateOnly last_request {  get; set; }
    }
}
