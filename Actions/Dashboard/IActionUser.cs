using System.Threading.Tasks;

namespace Actions.Dashboard
{
    public interface IActionUser
    {
        string ID { get; }
        string Name { get; }
        string? ProfilePictureURL { get; }
        Task Ban(float? length);
    }
}