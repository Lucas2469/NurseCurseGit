namespace NurseCourse.DTO
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public int QuestionType { get; set; }
        public string CorrectAnswer { get; set; }
        public List<OptionDTO> Options { get; set; }
    }
}
