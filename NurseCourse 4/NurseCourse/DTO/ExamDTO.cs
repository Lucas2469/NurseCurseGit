namespace NurseCourse.DTO
{
    public class ExamDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}
