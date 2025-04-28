namespace Domain
{
    public class TestModel
    {
        public int Id { get; set; } // این مهمه چون keyName گذاشتی Id
        public string Name { get; set; }
        public StatusType Status { get; set; }
        public AnckerType Ancker { get; set; }
        public bool IsActive { get; set; }
    }
}
