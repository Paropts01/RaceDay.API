namespace RaceDay.API
{
    public static class SessionExtensions
    {
        public static void SetUser(this ISession session, int userId, string role)
        {
            session.SetInt32("UserId", userId);
            session.SetString("Role", role);
        }

        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32("UserId");
        }

        public static string? GetRole(this ISession session)
        {
            return session.GetString("Role");
        }
    }
}
