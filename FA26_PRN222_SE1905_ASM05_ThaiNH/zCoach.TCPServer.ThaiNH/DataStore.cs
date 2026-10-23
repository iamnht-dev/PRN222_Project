using zCoach.TCPServer.ThaiNH.Models;

namespace zCoach.TCPServer.ThaiNH;

public static class DataStore
{
    // Sample Coach records; this exercise uses memory, not a database connection.
    public static List<CoachThaiNh> Coaches { get; } = new()
    {
        new CoachThaiNh
        {
            CoachThaiNhid = 1, FullName = "Nguyen Van Thai", Email = "thai@example.com",
            Phone = "0901111111", HireDate = DateTime.Today, Salary = 10000000m, Status = 1, IsActive = true
        },
        new CoachThaiNh
        {
            CoachThaiNhid = 2, FullName = "Tran Minh An", Email = "an@example.com",
            Phone = "0902222222", HireDate = DateTime.Today, Salary = 12000000m, Status = 1, IsActive = true
        },
        new CoachThaiNh
        {
            CoachThaiNhid = 3, FullName = "Le Thanh Binh", Email = "binh@example.com",
            Phone = "0903333333", HireDate = DateTime.Today, Salary = 15000000m, Status = 1, IsActive = true
        }
    };
}
