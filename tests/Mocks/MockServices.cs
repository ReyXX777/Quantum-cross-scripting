public class MockAuthService : AuthService
{
    public override bool Authenticate(string username, string password)
    {
        var users = MockData.GetMockUsers();
        return users.Any(u => u.Username == username && u.Password == password);
    }
}

public class MockDetectionService : DetectionService
{
    public override bool Detect(string input)
    {
        // Simple mock logic: consider any input containing "<script>" as malicious
        return input.Contains("<script>");
    }
}

public class MockLogService : LogService
{
    public override void AddLog(string message)
    {
        var logs = MockData.GetMockLogs();
        logs.Add(new Log { Id = logs.Count + 1, Message = message, Timestamp = DateTime.Now });
    }

    public override List<Log> GetLogs()
    {
        return MockData.GetMockLogs();
    }
}
