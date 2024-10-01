namespace InfinityNumerology.DataSource.Model
{
    public class SuperUser
    {
        public long user_id { get; set; }
        public string firstname { get; set; }
        public string username { get; set; }
        public string bio { get; set; }
        public DateTime user_date { get; set; }
        public int balance_access {  get; set; }
        public DateTime last_request {  get; set; }
        public int count { get; set; }
        public string command_name { get; set; }
    }
}
